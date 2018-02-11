using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BuildDEMFile {

   /// <summary>
   /// zum Einlesen der HGT-Daten und liefern von interpolierten Höhenwerten
   /// </summary>
   class DEMDataConverter : IDisposable {

      /// <summary>
      /// linker Rand des gesamten Gebietes
      /// </summary>
      public double Left { get; private set; }
      /// <summary>
      /// oberer Rand des gesamten Gebietes
      /// </summary>
      public double Top { get; private set; }
      /// <summary>
      /// rechter Rand des gesamten Gebietes
      /// </summary>
      public double Right { get; private set; }
      /// <summary>
      /// unterer Rand des gesamten Gebietes
      /// </summary>
      public double Bottom { get; private set; }
      /// <summary>
      /// Breite des gesamten Gebietes
      /// </summary>
      public double Width { get { return Right - Left; } }
      /// <summary>
      /// Höhe des gesamten Gebietes
      /// </summary>
      public double Height { get { return Top - Bottom; } }

      /// <summary>
      /// Array der DEM's
      /// </summary>
      DEM1x1[,] dat;

      /// <summary>
      /// zur sicheren/einfachen Datenübergabe an den Thread
      /// </summary>
      class ThreadInputData {
         public int lon, lat;
         public int x, y;
         public List<string> hgtpath;
         public bool dummydataonerror;
         public DEM1x1[,] dat;

         public ThreadInputData(int lon, int lat, int x, int y, List<string> hgtpath, bool dummydataonerror, DEM1x1[,] dat) {
            this.lon = lon;
            this.lat = lat;
            this.x = x;
            this.y = y;
            this.hgtpath = hgtpath;
            this.dummydataonerror = dummydataonerror;
            this.dat = dat;
         }
      }


      public DEMDataConverter() {
         Left = 0;
         Top = 0;
         Right = 0;
         Bottom = 0;
      }

      /// <summary>
      /// liest alle nötigen HGT's ein
      /// </summary>
      /// <param name="hgtpath"></param>
      /// <param name="left"></param>
      /// <param name="top"></param>
      /// <param name="right"></param>
      /// <param name="bottom"></param>
      /// <param name="dummydataonerror"></param>
      /// <returns></returns>
      public bool ReadData(List<string> hgtpath, double left, double top, double right, double bottom, bool dummydataonerror) {
         DateTime starttime = DateTime.Now;

         bool ret = true;
         // Voraussetzung: Die Datendateien liegen im 1-Grad-Raster vor. In den Dateinamen ist die linke untere Ecke enthalten.

         Left = Math.Floor(left);     // "größte ganze Zahl die kleiner ist als ..." fkt. auch für negative Zahlen
         Right = Math.Ceiling(right);
         Top = Math.Ceiling(top);
         Bottom = Math.Floor(bottom);

         int iLeft = (int)Left;
         int iBottom = (int)Bottom;
         int iRight = (int)Right - 1;
         int iTop = (int)Top - 1;

         dat = new DEM1x1[iRight - iLeft + 1, iTop - iBottom + 1];

         ReaderThreadPoolExt readerpool = new ReaderThreadPoolExt(ReaderMessage);

         Console.WriteLine("read DEM's ...");
         for (int lon = iLeft; lon <= iRight; lon++)
            for (int lat = iBottom; lat <= iTop; lat++)
               readerpool.Start(new ThreadInputData(lon, lat, lon - iLeft, lat - iBottom, hgtpath, dummydataonerror, dat));
         readerpool.Wait4NotWorking();
         ret = readerpool.ExceptionCount == 0;

         Console.WriteLine("time for read {0:N1}s", (DateTime.Now - starttime).TotalSeconds);

         return ret;
      }

      static void ReaderMessage(object para) {
         if (para != null)
            if (para is string) {
               Console.WriteLine(para as string);
            }
      }

      class ReaderThreadPoolExt : ThreadPoolExt {

         public ReaderThreadPoolExt(WaitCallback msgfunc) : base(msgfunc, null) { }

         protected override void DoWork(object para) {
            if (ExceptionCount > 0)
               return;

            try {
               if (para != null && para is ThreadInputData) {
                  ThreadInputData tid = para as ThreadInputData;

                  DEM1x1 dem1x1 = null;
                  string basefilename = DEM1x1.GetStandardBasefilename(tid.lon, tid.lat);
                  for (int i = 0; i < tid.hgtpath.Count; i++) { // für jeden Pfad ...
                     string demfile = "";
                     demfile = Path.Combine(tid.hgtpath[i], basefilename + ".hgt");
                     if (!File.Exists(demfile)) {
                        demfile = Path.Combine(tid.hgtpath[i], basefilename + ".hgt.zip");
                        if (!File.Exists(demfile)) {
                           demfile = Path.Combine(tid.hgtpath[i], basefilename + ".tif");
                           if (!File.Exists(demfile)) {
                              demfile = Path.Combine(tid.hgtpath[i], basefilename + ".tiff");
                              if (!File.Exists(demfile)) {
                                 if (!tid.dummydataonerror)
                                    throw new Exception(string.Format("data for longitude={0}° and latitude={1}° don't exist", tid.lon, tid.lat));
                                 else
                                    dem1x1 = new DEMNoValues(tid.lon, tid.lat);
                              } else {
                                 dem1x1 = new DEMTiffReader(tid.lon, tid.lat, demfile);
                                 break;
                              }
                           } else {
                              dem1x1 = new DEMTiffReader(tid.lon, tid.lat, demfile);
                              break;
                           }
                        } else {
                           dem1x1 = new DEMHGTReader(tid.lon, tid.lat, tid.hgtpath[i]);
                           break;
                        }
                     } else {
                        dem1x1 = new DEMHGTReader(tid.lon, tid.lat, tid.hgtpath[i]);
                        break;
                     }
                  }

                  if (dem1x1 != null) {
                     dem1x1.SetDataArray();

                     if (msgfunc != null) {
                        string msg = string.Format("altitudes for lon {0}° .. {1}° / lat {2}° .. {3}° read in, {4}x{5}, {6}",
                                                   dem1x1.Left,
                                                   dem1x1.Left + 1,
                                                   dem1x1.Bottom,
                                                   dem1x1.Bottom + 1,
                                                   dem1x1.Columns,
                                                   dem1x1.Rows,
                                                   dem1x1.Minimum == dem1x1.Maximum ?
                                                      " (only dummyvalues)" :
                                                      string.Format("values {0} .. {1}", dem1x1.Minimum, dem1x1.Maximum));
                        lock (msglocker) {
                           msgfunc(msg);
                        }
                     }

                     //if (tid.changehgtsize > 0) {
                     //   int oldtablesize = dem1x1.Columns;
                     //   if (dem1x1.ResizeDatatable(tid.changehgtsize, tid.changehgtsize))
                     //      if (msgfunc != null) {
                     //         string msg = string.Format("tablesize for lon {0}° .. {1}° / lat {2}° .. {3}° changed from {4} to {5}",
                     //                                    dem1x1.Left,
                     //                                    dem1x1.Left + 1,
                     //                                    dem1x1.Bottom,
                     //                                    dem1x1.Bottom + 1,
                     //                                    oldtablesize,
                     //                                    dem1x1.Columns);
                     //         lock (msglocker) {
                     //            msgfunc(msg);
                     //         }
                     //      }
                     //}
                  }

                  tid.dat[tid.x, tid.y] = dem1x1;
               }
            } catch (Exception ex) {
               IncrementExceptionCount();
               if (msgfunc != null)
                  lock (msglocker) {
                     msgfunc("FEHLER: " + ex.Message);
                  }
            }
         }

      }

      /// <summary>
      /// liefert die (interpolierte) Höhe
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="intpol"></param>
      /// <returns></returns>
      double GetHeight(double lon, double lat, DEM1x1.InterpolationType intpol) {
         // Quell-Gebiet bestimmen
         int x = (int)(lon - Left);
         int y = (int)(lat - Bottom);
         if (0 <= x && x < dat.GetLength(0) &&
             0 <= y && y < dat.GetLength(1)) {
            return dat[x, y].InterpolatedHeight(lon, lat, intpol);
         }
         return DEM1x1.NOVALUED;
      }

      /// <summary>
      /// berechnet das Array der interpolierten Höhen (Koordinatenursprung links oben)
      /// </summary>
      /// <param name="left">linken Rand</param>
      /// <param name="top">oberer Rand</param>
      /// <param name="stepwidth">horizontale Schrittweite</param>
      /// <param name="stepheight">vertikale Schrittweite</param>
      /// <param name="loncount">Datenwerte waagerecht</param>
      /// <param name="latcount">Datenwerte senkrecht</param>
      /// <param name="shrink">ungerader Verkleinerungsfaktor</param>
      /// <param name="foot">Daten in Fuß</param>
      /// <param name="intpol"></param>
      /// <returns></returns>
      public int[,] BuildHeightArray(double left, double top, double stepwidth, double stepheight, int loncount, int latcount, int shrink, bool foot, DEM1x1.InterpolationType intpol) {
         double bottom = top - (latcount - 1) * stepheight;
         if (left < Left || Right < left + (loncount - 1) * stepwidth ||
             bottom < Bottom || Top < top)
            throw new Exception(string.Format("The area {0}° .. {1}° / {2}° .. {3}° is not inside the HGT-Area {4}° .. {5}° / {6}° .. {7}°.",
                                              left,
                                              left + (loncount - 1) * stepwidth,
                                              bottom,
                                              top,
                                              Left,
                                              Right,
                                              Bottom,
                                              Top));

         int[,] heights = new int[loncount, latcount];    // Array darf nicht größer als 2GB-x werden -> 532000000 Elemente fkt. (z.B. 23065 x 23065)
                                                          // Int16-Array benötigt leider genausoviel Speicher wie int-Array!!!
         for (int y = 0; y < latcount; y++) {
            double lat = top - y * stepheight;

            for (int x = 0; x < loncount; x++) {
               double lon = left + x * stepwidth;

               double h = GetHeight(lon, lat, intpol);  // interpolierte Höhe
               if (h != DEM1x1.NOVALUED) {
                  if (foot)
                     h /= DEM1x1.FOOT;
                  if (shrink == 1)
                     heights[x, y] = (int)Math.Round(h, 0);
                  else
                     heights[x, y] = (int)Math.Round(h / shrink, 0);
               } else
                  heights[x, y] = Subtile.UNDEF4ENCODER;
            }
         }

         return heights;
      }

      /// <summary>
      /// berechnet das Array der interpolierten Höhen (Koordinatenursprung links oben)
      /// </summary>
      /// <param name="left">linken Rand</param>
      /// <param name="top">oberer Rand</param>
      /// <param name="width">Breite</param>
      /// <param name="height">Höhe</param>
      /// <param name="stepwidth">horizontale Schrittweite</param>
      /// <param name="stepheight">vertikale Schrittweite</param>
      /// <param name="shrink">ungerader Verkleinerungsfaktor</param>
      /// <param name="foot">Daten in Fuß</param>
      /// <param name="intpol"></param>
      /// <returns></returns>
      public int[,] BuildHeightArray(double left, double top, double width, double height, double stepwidth, double stepheight, int shrink, bool foot, DEM1x1.InterpolationType intpol) {
         if (left < Left ||
             top > Top ||
             left + width > Right ||
             top - height < Bottom)
            throw new Exception(string.Format("The area {0}° .. {1}° / {2}° .. {3}° is not inside the HGT-Area {4}° .. {5}° / {6}° .. {7}°.",
                                              left,
                                              left + width,
                                              top - height,
                                              top,
                                              Left,
                                              Right,
                                              Bottom,
                                              Top));

         int iCountLon = (int)(width / stepwidth);
         if (iCountLon * stepwidth < width)
            iCountLon++;
         iCountLon++;

         int iCountLat = (int)(height / stepheight);
         if (iCountLat * stepheight < height)
            iCountLat++;
         iCountLat++;

         return BuildHeightArray(left,
                                 top,
                                 stepwidth,
                                 stepheight,
                                 iCountLon,
                                 iCountLat,
                                 shrink,
                                 foot,
                                 intpol);
      }

      public override string ToString() {
         return string.Format("Lon {0} .. {1}, Lat {2} .. {3}, HGT-Parts {4}",
                              Left, Right,
                              Bottom, Top,
                              dat != null ? dat.GetLength(0) * dat.GetLength(1) : 0);
      }

      #region Implementierung der IDisposable-Schnittstelle

      ~DEMDataConverter() {
         Dispose(false);
      }

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// überschreibt die Standard-Methode
      /// <para></para>
      /// </summary>
      /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
      protected virtual void Dispose(bool notfromfinalizer) {
         if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben

               for (int i = 0; i < dat.GetLength(0); i++)
                  for (int j = 0; j < dat.GetLength(1); j++)
                     dat[i, j].Dispose();

            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }
}
