using System;
using System.Threading;

namespace BuildDEMFile {

   /// <summary>
   /// zum Einlesen der HGT-Daten und liefern von interpolierten Höhenwerten
   /// </summary>
   class HgtDataConverter : IDisposable {

      /// <summary>
      /// intern: Wert für "Höhe ist undefiniert"
      /// </summary>
      const double UNDEFD = double.NaN;

      /// <summary>
      /// Wert für "Höhe ist undefiniert" in den Ergebnisdaten (beachte <see cref="Subtile.UNDEF"/>)
      /// </summary>
      public const int UNDEF = short.MaxValue;

      /// <summary>
      /// Fuß in Meter
      /// </summary>
      const double FOOT = 0.3048;

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
      /// Array der HGT's
      /// </summary>
      HGTReader[,] dat;


      public HgtDataConverter() {
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
      public bool ReadData(string hgtpath, double left, double top, double right, double bottom, bool dummydataonerror) {
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

         dat = new HGTReader[iRight - iLeft + 1, iTop - iBottom + 1];

         ReaderThreadPoolExt readerpool = new ReaderThreadPoolExt(ReaderMessage);

         Console.WriteLine("HGT-Daten lesen ...");
         for (int lon = iLeft; lon <= iRight; lon++)
            for (int lat = iBottom; lat <= iTop; lat++)
               readerpool.Start(new object[] { lon, lat, lon - iLeft, lat - iBottom, hgtpath, dummydataonerror, dat });
         readerpool.Wait4NotWorking();
         ret = readerpool.ExceptionCount == 0;

         Console.WriteLine("Einlesezeit {0:N1}s", (DateTime.Now - starttime).TotalSeconds);

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
               if (para != null && para is object[]) {
                  object[] args = para as object[];
                  if (args.Length == 7) {
                     int lon = (int)args[0];
                     int lat = (int)args[1];
                     int x = (int)args[2];
                     int y = (int)args[3];
                     string hgtpath = args[4] as string;
                     bool dummydataonerror = (bool)args[5];
                     HGTReader[,] dat = args[6] as HGTReader[,];

                     dat[x, y] = new HGTReader(lon, lat, hgtpath, dummydataonerror);

                     if (msgfunc != null) {
                        string msg = string.Format("Höhen für {0}° .. {1}° / {2}° .. {3}° eingelesen, {4}",
                                                   dat[x, y].Left, dat[x, y].Left + 1,
                                                   dat[x, y].Bottom, dat[x, y].Bottom + 1,
                                                   dat[x, y].Minimum == dat[x, y].Maximum ? " (nur Dummywerte)" : string.Format("Werte {0} .. {1}", dat[x, y].Minimum, dat[x, y].Maximum));
                        lock (msglocker) {
                           msgfunc(msg);
                        }
                     }
                  }
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

      //public bool ReadData(string hgtpath, double left, double top, double right, double bottom, bool dummydataonerror) {
      //   DateTime starttime = DateTime.Now;

      //   bool ret = true;
      //   // Voraussetzung: Die Datendateien liegen im 1-Grad-Raster vor. In den Dateinamen ist die linke untere Ecke enthalten.

      //   int iLeft = (int)left;
      //   int iRight = (int)right;
      //   int iTop = (int)top;
      //   int iBottom = (int)bottom;

      //   Left = iLeft;
      //   Bottom = iBottom;
      //   Right = iRight + 1;
      //   Top = iTop + 1;

      //   dat = new HGTReader[iRight - iLeft + 1, iTop - iBottom + 1];

      //   for (int lon = iLeft; lon <= iRight; lon++)
      //      for (int lat = iBottom; lat <= iTop; lat++) {
      //         try {
      //            HGTReader hgt = new HGTReader(lon, lat, hgtpath, dummydataonerror);
      //            dat[lon - iLeft, lat - iBottom] = hgt;
      //            Console.WriteLine(string.Format("Höhen für {0}° .. {1}° / {2}° .. {3}° eingelesen, {4}",
      //                                            lon, lon + 1,
      //                                            lat, lat + 1,
      //                                            hgt.Minimum == hgt.Maximum ? " (nur Dummywerte)" : string.Format("Werte {0} .. {1}", hgt.Minimum, hgt.Maximum)));
      //         } catch (Exception ex) {
      //            Console.Error.WriteLine(ex.Message);
      //            ret = false;
      //         }
      //      }

      //   Console.WriteLine("Einlesezeit {0:N1}s", (DateTime.Now - starttime).TotalSeconds);

      //   return ret;
      //}

      /// <summary>
      /// liefert die (interpolierte) Höhe
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <returns></returns>
      double GetHeight(double lon, double lat) {
         // Quell-Gebiet bestimmen
         int x = (int)(lon - Left);
         int y = (int)(lat - Bottom);
         if (0 <= x && x < dat.GetLength(0) &&
             0 <= y && y < dat.GetLength(1)) {
            double h = dat[x, y].InterpolatedHeight(lon, lat);
            return h != HGTReader.NOVALUE ? h : UNDEFD;
         }
         return UNDEFD;
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
      /// <param name="foot">Daten in Fuß</param>
      /// <returns></returns>
      public int[,] BuildHeightArray(double left, double top, double stepwidth, double stepheight, int loncount, int latcount, bool foot) {
         double bottom = top - (latcount - 1) * stepheight;
         if (left < Left || Right < left + (loncount - 1) * stepwidth ||
             bottom < Bottom || Top < top)
            throw new Exception(string.Format("Der gewünschte Bereich {0}° .. {1}° / {2}° .. {3}° überschreitet den Bereich der eingelesenen HGT-Werte {4}° .. {5}° / {6}° .. {7}°.",
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

               int iHeight = UNDEF;
               double h = GetHeight(lon, lat);  // interpolierte Höhe
               if (!double.IsNaN(h)) {
                  if (foot)
                     h /= FOOT;
                  iHeight = (int)Math.Round(h, 0);
               }
               heights[x, y] = iHeight;
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
      /// <param name="foot">Daten in Fuß</param>
      /// <returns></returns>
      public int[,] BuildHeightArray(double left, double top, double width, double height, double stepwidth, double stepheight, bool foot) {
         if (left < Left ||
             top > Top ||
             left + width > Right ||
             top - height < Bottom)
            throw new Exception(string.Format("Der gewünschte Bereich {0}° .. {1}° / {2}° .. {3}° überschreitet den Bereich der eingelesenen HGT-Werte {4}° .. {5}° / {6}° .. {7}°.",
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
                                 foot);
      }


      public override string ToString() {
         return string.Format("Lon {0} .. {1}, Lat {2} .. {3}, HGT-Parts {4}",
                              Left, Right,
                              Bottom, Top,
                              dat != null ? dat.GetLength(0) * dat.GetLength(1) : 0);
      }

      #region Implementierung der IDisposable-Schnittstelle

      ~HgtDataConverter() {
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
