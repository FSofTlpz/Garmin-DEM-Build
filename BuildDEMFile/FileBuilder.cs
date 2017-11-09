using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace BuildDEMFile {

   class FileBuilder {

      const int STDSUBTILESIZE = 64;

      const int SUBTILEPAKETSIZE = 20;


      class Data4Zoomlevel {
         /// <summary>
         /// Datendatei
         /// </summary>
         public string Datafile;
         /// <summary>
         /// westliche DEM-Grenze
         /// </summary>
         public double Left;
         /// <summary>
         /// nördliche DEM-Grenze
         /// </summary>
         public double Top;
         /// <summary>
         /// Breite des DEM-Bereiches
         /// </summary>
         public double Width;
         /// <summary>
         /// Höhe des DEM-Bereiches
         /// </summary>
         public double Height;
         /// <summary>
         /// Höhe des Punktabstandes
         /// </summary>
         public double Latdist;
         /// <summary>
         /// Breite des Punktabstandes
         /// </summary>
         public double Londist;
         /// <summary>
         /// min. Höhe
         /// </summary>
         public int MinHeight;
         /// <summary>
         /// max. Höhe
         /// </summary>
         public int MaxHeight;

         /// <summary>
         /// 
         /// </summary>
         /// <param name="Datafile">Datendatei</param>
         /// <param name="Left">westliche DEM-Grenze</param>
         /// <param name="Top">nördliche DEM-Grenze</param>
         /// <param name="Width">Breite DEM-Grenze</param>
         /// <param name="Height">Höhe DEM-Grenze</param>
         /// <param name="Latdist">Höhe des Punktabstandes</param>
         /// <param name="Londist">Breite des Punktabstandes</param>
         public Data4Zoomlevel(string Datafile, double Left, double Top, double Width, double Height, double Latdist, double Londist) {
            this.Datafile = string.IsNullOrEmpty(Datafile) ? null : Datafile.Trim();
            this.Left = Left;
            this.Top = Top;
            this.Latdist = Latdist;
            this.Londist = Londist;
            this.Width = Width;
            this.Height = Height;
         }

      }

      /// <summary>
      /// Liste der Zoomleveldaten
      /// </summary>
      List<Data4Zoomlevel> data4Zoomlevel;
      /// <summary>
      /// Pfad zu den HGT-Daten
      /// </summary>
      string HgtPath;
      /// <summary>
      /// Ausgabedatei für HGT-Daten
      /// </summary>
      string HgtOutput;


      /// <summary>
      /// 
      /// </summary>
      /// <param name="datafile">Textdatei mit Höhendaten</param>
      /// <param name="hgtpath">Pfad zu den HGT-Dateien</param>
      /// <param name="left">Ecke links-oben</param>
      /// <param name="top">Ecke links-oben</param>
      /// <param name="width">Breite des DEM-Bereiches</param>
      /// <param name="height">Höhe des DEM-Bereiches</param>
      /// <param name="londist">Breite des Punktabstandes</param>
      /// <param name="latdist">Höhe des Punktabstandes</param>
      /// <param name="hgtoutput">Datei für die Ausgabe der Textdaten</param>
      public FileBuilder(List<string> datafile,
                         string hgtpath,
                         List<double> left, List<double> top,
                         List<double> width, List<double> height,
                         List<double> londist, List<double> latdist,
                         string hgtoutput) {

         data4Zoomlevel = new List<FileBuilder.Data4Zoomlevel>();
         HgtPath = string.IsNullOrEmpty(hgtpath) ? null : hgtpath;
         HgtOutput = string.IsNullOrEmpty(hgtoutput) ? null : hgtoutput;

         int count = Math.Min(datafile.Count, left.Count);
         count = Math.Min(count, top.Count);
         count = Math.Min(count, londist.Count);
         count = Math.Min(count, latdist.Count);
         count = Math.Min(count, width.Count);
         count = Math.Min(count, height.Count);

         for (int i = 0; i < count; i++) {
            data4Zoomlevel.Add(new Data4Zoomlevel(datafile[i], left[i], top[i], width[i], height[i], latdist[i], londist[i]));

            if (HgtPath != null)
               data4Zoomlevel[i].Datafile = null;

            if (HgtPath == null &&
                data4Zoomlevel[i].Datafile == null)
               throw new Exception("Entweder eine Datendatei oder ein HGT-Pfad muss angegeben sein.");

            if (double.IsNaN(data4Zoomlevel[i].Left) ||
                double.IsNaN(data4Zoomlevel[i].Top))
               throw new Exception("Die Position des DEM-Bereiches muss angegeben sein.");

            if (HgtPath != null) {
               if (double.IsNaN(data4Zoomlevel[i].Width) ||
                   double.IsNaN(data4Zoomlevel[i].Height))
                  throw new Exception("Die Ausdehnung des DEM-Bereiches muss angegeben sein.");

               if (double.IsNaN(data4Zoomlevel[i].Latdist) ||
                   double.IsNaN(data4Zoomlevel[i].Londist))
                  throw new Exception("Die Abstände der Punkte müssen angegeben sein.");
            } else {
               if ((double.IsNaN(data4Zoomlevel[i].Width) ||
                    double.IsNaN(data4Zoomlevel[i].Height)) &&
                   (double.IsNaN(data4Zoomlevel[i].Latdist) ||
                    double.IsNaN(data4Zoomlevel[i].Londist)))
                  throw new Exception("Die Ausdehnung des DEM-Bereiches oder die Abstände der Punkte müssen angegeben sein.");
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="hgtpath">Pfad zu den HGT-Dateien</param>
      /// <param name="left">Ecke links-oben</param>
      /// <param name="top">Ecke links-oben</param>
      /// <param name="width">Breite des DEM-Bereiches</param>
      /// <param name="height">Höhe des DEM-Bereiches</param>
      /// <param name="ptdist">Punktabstand</param>
      /// <param name="hgtoutput">Datei für die Ausgabe der Textdaten</param>
      public FileBuilder(string hgtpath, double left, double top, double width, double height, double ptdist, string hgtoutput = null) :
         this(new List<string>() { null },
              hgtpath,
              new List<double>() { left },
              new List<double>() { top },
              new List<double>() { width },
              new List<double>() { height },
              new List<double>() { ptdist },
              new List<double>() { ptdist },
              hgtoutput) { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="datafile">Textdatei mit Höhendaten</param>
      /// <param name="left">Ecke links-oben</param>
      /// <param name="top">Ecke links-oben</param>
      /// <param name="width">Breite des DEM-Bereiches</param>
      /// <param name="height">Höhe des DEM-Bereiches</param>
      public FileBuilder(string datafile, double left, double top, double width, double height) :
         this(new List<string>() { datafile },
              null,
              new List<double>() { left },
              new List<double>() { top },
              new List<double>() { width },
              new List<double>() { height },
              new List<double>() { double.NaN },
              new List<double>() { double.NaN },
              null) { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="datafile">Textdatei mit Höhendaten</param>
      /// <param name="left">Ecke links-oben</param>
      /// <param name="top">Ecke links-oben</param>
      /// <param name="ptdist">Punktabstand</param>
      public FileBuilder(string datafile, double left, double top, double ptdist) :
         this(new List<string>() { datafile },
              null,
              new List<double>() { left },
              new List<double>() { top },
              new List<double>() { double.NaN },
              new List<double>() { double.NaN },
              new List<double>() { ptdist },
              new List<double>() { ptdist },
              null) { }

      /// <summary>
      /// Erzeugt die DEM-Datei
      /// </summary>
      /// <param name="demfile">Name der DEM-Datei</param>
      /// <param name="footflag">Angaben in Fuß (oder Meter)</param>
      /// <param name="lastcolstd">letzte Spalte mit Standardbreite</param>
      /// <param name="overwrite">bestehende Dateien überschreiben</param>
      /// <param name="dummydataonerror">liefert Dummy-Daten, wenn die Datei nicht ex.</param>
      /// <param name="maxthreads">wenn größer 0, dann Berechnung multithread</param>
      /// <returns></returns>
      public bool Create(string demfile, bool footflag, bool lastcolstd, bool overwrite, bool dummydataonerror, int maxthreads = 0) {
         DateTime starttime = DateTime.Now;

         if (!overwrite &&
             File.Exists(demfile)) {
            Console.Error.WriteLine("Die Datei '" + demfile + "' existiert schon.");
            return false;
         }

         List<Subtile[,]> tiles4zoomlevel = new List<Subtile[,]>(); // für jeden Zoomlevel ein Tile-Array
         HgtDataConverter hgtconv = null;

         if (!string.IsNullOrEmpty(HgtPath)) {  // Daten (für alle Zoomlevel) nur 1x aus den HGT-Daten einlesen
            // Begrenzung des Gebiets ermitteln (kann etwas größer sein, als im Zoomlevel angegeben)
            double mostleft = double.MaxValue;
            double mostright = double.MinValue;
            double mostbottom = double.MaxValue;
            double mosttop = double.MinValue;
            for (int z = 0; z < data4Zoomlevel.Count; z++) {
               /* ACHTUNG
                * Wenn die Breite des Gebiets 1 Punktabstand ist, werden 2 Punkte ermittelt, d.h.
                * wenn die Breite des Gebiets n Punktabstände ist, werden n+1 Punkte ermittelt.
                * Für ein Vielfaches von 64 Punkten wird also eine Breite x*64 - 1 benötigt.
               */
               if (lastcolstd) { // notfalls auf 64er-Breite bringen
                  double t = data4Zoomlevel[z].Width / (STDSUBTILESIZE * data4Zoomlevel[z].Londist);
                  if ((int)t < t) {
                     data4Zoomlevel[z].Width = (1 + (int)t) * STDSUBTILESIZE * data4Zoomlevel[z].Londist;
                     data4Zoomlevel[z].Width -= 1.1 * data4Zoomlevel[z].Londist;
                  }
               }
               mostleft = Math.Min(mostleft, data4Zoomlevel[z].Left);
               mosttop = Math.Max(mosttop, data4Zoomlevel[z].Top);
               mostright = Math.Max(mostright, data4Zoomlevel[z].Left + data4Zoomlevel[z].Width);
               mostbottom = Math.Min(mostbottom, data4Zoomlevel[z].Top - data4Zoomlevel[z].Height);
            }

            hgtconv = new HgtDataConverter();
            if (!hgtconv.ReadData(HgtPath, mostleft, mosttop, mostright, mostbottom, dummydataonerror)) {     // HGT-Rohdaten einlesen
               Console.Error.WriteLine("Es konnten nicht alle nötigen Höhen-Daten eingelesen werden.");
               return false;
            }

            if (!string.IsNullOrEmpty(HgtOutput)) {   // HGT-Daten ausgeben
               string ext = Path.GetExtension(HgtOutput).ToLower();
               for (int z = 0; z < data4Zoomlevel.Count; z++)
                  WriteHgtOutput(HgtOutput.Substring(0, HgtOutput.Length - ext.Length) + "_zl" + (z + 1).ToString() + ext,
                                 hgtconv.BuildHeightArray(data4Zoomlevel[z].Left,
                                                          data4Zoomlevel[z].Top,
                                                          data4Zoomlevel[z].Width,
                                                          data4Zoomlevel[z].Height,
                                                          data4Zoomlevel[z].Londist,
                                                          data4Zoomlevel[z].Latdist,
                                                          footflag));
            }
         }

         // Daten für jeden Zoomlevel einlesen
         Console.WriteLine(data4Zoomlevel.Count.ToString() + " Zoomlevel");
         for (int z = 0; z < data4Zoomlevel.Count; z++) {
            Subtile[,] tiles = null;

            if (hgtconv == null) {     // Daten aus Textdatei
               Data2Dim rawdata = ReadTextData(data4Zoomlevel[z].Datafile, out data4Zoomlevel[z].MinHeight, out data4Zoomlevel[z].MaxHeight);

               if (rawdata.Height > 0 && rawdata.Width > 0) {
                  if (double.IsNaN(data4Zoomlevel[z].Width)) {
                     data4Zoomlevel[z].Width = rawdata.Width * data4Zoomlevel[z].Latdist;
                  } else {
                     if (double.IsNaN(data4Zoomlevel[z].Latdist)) {
                        data4Zoomlevel[z].Latdist = data4Zoomlevel[z].Width / rawdata.Width;
                     }
                  }

                  if (double.IsNaN(data4Zoomlevel[z].Height)) {
                     data4Zoomlevel[z].Height = rawdata.Height * data4Zoomlevel[z].Latdist;
                  } else {
                     if (double.IsNaN(data4Zoomlevel[z].Londist)) {
                        data4Zoomlevel[z].Londist = data4Zoomlevel[z].Height / rawdata.Height;
                     }
                  }

                  Console.WriteLine("Textdaten " + rawdata.ToString() + ", Min. " + data4Zoomlevel[z].MinHeight.ToString() + ", Max. " + data4Zoomlevel[z].MaxHeight.ToString() + " für Zoomlevel " + z.ToString());
                  tiles = BuildFilledSubtileArray(rawdata, lastcolstd, STDSUBTILESIZE);

               } else
                  throw new Exception("Keine Textdaten eingelesen.");

               rawdata.Dispose();   // Rohdaten werden nicht mehr benötigt

            } else {                // Date aus HGT-Dateien

               tiles = BuildEmptySubtileArray(data4Zoomlevel[z], lastcolstd, STDSUBTILESIZE);

            }

            // tiles enthält die Subtiles des Zoomlevels mit den Rohdaten, wenn die Daten aus einer Textdatei stammen, sonst nur mit den geplanten Werten
            tiles4zoomlevel.Add(tiles);

            Console.Write("Kachelanzahl " + tiles.GetLength(0) + " x " + tiles.GetLength(1));
            Console.Write(" (rechte Spalte ");
            Console.Write(hgtconv == null ? tiles[tiles.GetLength(0) - 1, 0].Width.ToString() : tiles[tiles.GetLength(0) - 1, 0].PlannedWidth.ToString());
            Console.Write(" breit, untere Zeile ");
            Console.Write(hgtconv == null ? tiles[0, tiles.GetLength(1) - 1].Height.ToString() : tiles[0, tiles.GetLength(1) - 1].PlannedHeight.ToString());
            Console.WriteLine(" hoch)");
            Console.WriteLine(string.Format("Rand links {0}°, oben {1}°, {2}° breit, {3}° hoch", data4Zoomlevel[z].Left, data4Zoomlevel[z].Top, data4Zoomlevel[z].Width, data4Zoomlevel[z].Height));
            Console.WriteLine(string.Format("Pixelgröße {0}° x {1}°", data4Zoomlevel[z].Londist, data4Zoomlevel[z].Latdist));
         }


         // ----- wenn min. 1 Zoomlevel ex. beginnt jetzt die Codierung
         if (tiles4zoomlevel.Count > 0) {
            if (maxthreads > 0)
               Console.WriteLine("multithreaded");

            CalculationThreadPoolExt calctp = maxthreads > 0 ? 
                                                   new CalculationThreadPoolExt(ThreadMsg) : 
                                                   null;

            int count = 0;
            for (int z = 0; z < tiles4zoomlevel.Count; z++)
               count += tiles4zoomlevel[z].GetLength(0) * tiles4zoomlevel[z].GetLength(1);
            Console.Write("encodiere {0} Subtiles ", count);

            count = 0;
            List<Subtile> subtilepacket = new List<Subtile>();
            for (int z = 0; z < tiles4zoomlevel.Count; z++) {
               for (int y = 0; y < tiles4zoomlevel[z].GetLength(1); y++) {
                  for (int x = 0; x < tiles4zoomlevel[z].GetLength(0); x++) {
                     // Jedes Subtile in einem einzelnen Thread zu codieren bringt zuviel "Verwaltungsaufwand" für die Threads.
                     // Deshalb wird immer gleich ein ganzes "Paket" von Subtiles codiert.
                     subtilepacket.Add(tiles4zoomlevel[z][x, y]);
                     if (subtilepacket.Count >= SUBTILEPAKETSIZE) {
                        count += subtilepacket.Count;
                        EncodeSubtilePaket(subtilepacket, maxthreads > 1 ? calctp : null, hgtconv, footflag);
                     }
                  }
               }
            }
            if (subtilepacket.Count > 0) {
               count += subtilepacket.Count;
               EncodeSubtilePaket(subtilepacket, calctp, hgtconv, footflag);
            }

            if (maxthreads > 0) {
               calctp.Wait4NotWorking();
               if (calctp.ExceptionCount > 0)
                  return false;
            }

            Console.WriteLine();
            Console.WriteLine(count.ToString() + " Kacheln encodiert");

            // ----- Codierung beendet


            Head head = new Head();
            head.Footflag = footflag ? 1 : 0;
            head.Unknown1B = 0;
            head.Unknown25 = 1;
            head.Zoomlevel = (ushort)tiles4zoomlevel.Count;
            List<ZoomlevelData> Zoomlevel = CreateZoomlevel(tiles4zoomlevel, head);

            head.PtrZoomlevelTable = head.Length;
            for (int z = 0; z < Zoomlevel.Count; z++)
               head.PtrZoomlevelTable += Zoomlevel[z].GetTableAreaSize() + Zoomlevel[z].GetHeightDataAreaSize();

            // Jetzt sollten alle Daten gesetzt sein und die Datei kann geschrieben werden.
            using (BinaryWriter w = new BinaryWriter(File.Create(demfile)))
               WriteDEM(w, head, Zoomlevel);

            Console.WriteLine("Laufzeit {0:N1}s", (DateTime.Now - starttime).TotalSeconds);

         } else {
            Console.Error.WriteLine("zu wenig Daten");
            return false;
         }
         return true;
      }

      /// <summary>
      /// Encodierung einer Liste von <see cref="Subtile"/>
      /// </summary>
      /// <param name="subtilepacket"></param>
      /// <param name="tg">wenn null, wird ohne Multithreading encodiert</param>
      /// <param name="hgtconv">wenn null, müssen die Höhendaten in den <see cref="Subtile"/> schon vorhanden sein</param>
      /// <param name="footflag">wenn true, dann Höhendaten in Fuß, sonst Meter</param>
      void EncodeSubtilePaket(List<Subtile> subtilepacket, CalculationThreadPoolExt tp, HgtDataConverter hgtconv, bool footflag) {
         if (tp == null) {    // Paket direkt encodieren

            for (int i = 0; i < subtilepacket.Count; i++) {
               if (hgtconv != null) {
                  Data2Dim dat = new Data2Dim(hgtconv.BuildHeightArray(subtilepacket[i].PlannedLeft,
                                                                       subtilepacket[i].PlannedTop,
                                                                       subtilepacket[i].PlannedLonDistance,
                                                                       subtilepacket[i].PlannedLatDistance,
                                                                       subtilepacket[i].Width,
                                                                       subtilepacket[i].Height,
                                                                       footflag));
                  subtilepacket[i].Encoding(dat);  // HGT-Daten müssen noch geliefert werden
                  dat.Dispose();
               } else
                  subtilepacket[i].Encoding();     // Daten aus Textdatei sind schon vorhanden
            }
            Console.Write(".");

         } else {             // Paket im eigenen Thread encodieren

            tp.Start(new object[] { new List<Subtile>(subtilepacket), hgtconv, footflag });  // Kopie der Liste übergeben, weil das Original gleich geleert wird

         }
         subtilepacket.Clear();
      }

      static void ThreadMsg(object para) {
         if (para != null) {
            if (para is string)
               Console.Error.Write(para as string);
         }
      }

      // http://simplygenius.net/Article/FalseSharing


      class CalculationThreadPoolExt : ThreadPoolExt {

         public CalculationThreadPoolExt(WaitCallback msgfunc) : base(msgfunc, null) { }

         protected override void DoWork(object para) {
            if (ExceptionCount > 0)
               return;

            try {
               if (para is object[]) {
                  object[] args = para as object[];
                  if (args.Length == 3) {

                     if (args[0] is List<Subtile>) {
                        List<Subtile> lst = args[0] as List<Subtile>;
                        for (int i = 0; i < lst.Count; i++) {
                           Subtile st = lst[i];
                           if (args[1] == null) // kein HgtDataConverter
                              st.Encoding();          // Daten aus Textdatei sind schon vorhanden
                           else {
                              if (args[1] is HgtDataConverter) {
                                 HgtDataConverter hdc = args[1] as HgtDataConverter;
                                 Data2Dim dat = new Data2Dim(hdc.BuildHeightArray(st.PlannedLeft,
                                                                                  st.PlannedTop,
                                                                                  st.PlannedLonDistance,
                                                                                  st.PlannedLatDistance,
                                                                                  st.PlannedWidth,
                                                                                  st.PlannedHeight,
                                                                                  (bool)args[2]));
                                 st.Encoding(dat);    // HGT-Daten müssen noch geliefert werden
                                 dat.Dispose();
                              }
                           }

                        }
                        msgfunc?.Invoke(".");
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


      /// <summary>
      /// liest die gesamten Höhendaten aus einer Textdatei ein
      /// <para>je Textzeile eine Zeile der Höhendaten</para>
      /// <para>Höhendaten durch Leerzeichen, Komma, Semikolon oder Tabulator getrennt</para>
      /// </summary>
      /// <param name="txtfile"></param>
      /// <param name="minheight"></param>
      /// <param name="maxheight"></param>
      /// <returns></returns>
      Data2Dim ReadTextData(string txtfile, out int minheight, out int maxheight) {
         List<int> heights = new List<int>();
         int width = -1;
         int height = 0;
         maxheight = int.MinValue;
         minheight = int.MaxValue;

         try {
            String txt = ReadSimpleTxtFile(txtfile);
            string[] lines = txt.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines) {
               string[] sData = line.Split(new char[] { ' ', ',', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);

               if (width < 0) // 1. Datenzeile gibt die Breite vor
                  width = sData.Length;
               else {
                  if (width != sData.Length)
                     throw new Exception("Die Anzahl der Werte (" + sData.Length.ToString() + ") in Zeile " + (height + 1).ToString() + " ist nicht identisch zur Anzahl in der 1. Zeile (" + width.ToString() + ").");
               }
               height++;

               for (int i = 0; i < sData.Length; i++) {
                  int val = Convert.ToInt32(sData[i].Trim());
                  minheight = Math.Min(minheight, val);
                  if (val < Subtile.UNDEF)
                     maxheight = Math.Max(maxheight, val);
                  heights.Add(val);
               }
            }
         } catch (Exception e) {
            Console.Error.WriteLine("Die Daten der Datei '" + txtfile + "' konnten nicht gelesen werden:");
            Console.Error.WriteLine(e.Message);
         }

         return new Data2Dim(heights, width);
      }

      /// <summary>
      /// liest den gesamten Text einer Textdatei oder der 1. Datei in einer ZIP-Datei und liefert diesen Text
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      string ReadSimpleTxtFile(string file) {
         String txt = null;

         if (Path.GetExtension(file).ToUpper() == ".ZIP") {

            using (FileStream zipstream = new FileStream(file, FileMode.Open)) {
               using (ZipArchive zip = new ZipArchive(zipstream, ZipArchiveMode.Read)) {
                  if (zip.Entries.Count > 0) {
                     ZipArchiveEntry entry = zip.Entries[0];
                     Stream dat = entry.Open();
                     byte[] buff = new byte[entry.Length];
                     dat.Read(buff, 0, buff.Length);
                     txt = System.Text.Encoding.Default.GetString(buff);
                     dat.Close();
                  }
               }
            }

         } else

            using (StreamReader sr = new StreamReader(file)) {
               txt = sr.ReadToEnd();
            }

         return txt;
      }

      /// <summary>
      /// gibt die Daten des Arrays in eine Text- bzw. ZIP-Datei aus
      /// </summary>
      /// <param name="hgtoutput">Name der Text- oder ZIP-Datei</param>
      /// <param name="heights">Daten-Array</param>
      void WriteHgtOutput(string hgtoutput, int[,] heights) {
         if (!string.IsNullOrEmpty(hgtoutput)) {
            Console.WriteLine("schreibe HGT-Daten in die Datei '" + hgtoutput + "' ...");

            if (Path.GetExtension(hgtoutput).ToUpper() == ".ZIP") {
               using (FileStream zipstream = new FileStream(hgtoutput, FileMode.Create)) {
                  using (ZipArchive zip = new ZipArchive(zipstream, ZipArchiveMode.Create)) {
                     ZipArchiveEntry entry = zip.CreateEntry(Path.GetFileNameWithoutExtension(hgtoutput), CompressionLevel.Optimal);
                     using (StreamWriter w = new StreamWriter(entry.Open())) {
                        WriteHgtOutput2StreamWriter(w, heights);
                     }
                  }
               }
            } else {
               using (StreamWriter w = new StreamWriter(hgtoutput)) {
                  WriteHgtOutput2StreamWriter(w, heights);
               }
            }
         }
      }

      /// <summary>
      /// gibt die Daten des Arrays in den Stream aus
      /// </summary>
      /// <param name="w"></param>
      /// <param name="heights">Daten-Array</param>
      void WriteHgtOutput2StreamWriter(StreamWriter w, int[,] heights) {
         for (int j = 0; j < heights.GetLength(1); j++) {
            for (int i = 0; i < heights.GetLength(0); i++) {
               if (i > 0)
                  w.Write(" ");
               w.Write(heights[i, j]);
            }
            w.WriteLine();
         }
      }

      /// <summary>
      /// liefert ein noch leeres <see cref="Subtile"/>-Array ("Koordinatenursprung" links-oben)
      /// <para>für jedes <see cref="Subtile"/> werden die geplanten Werte gesetzt</para>
      /// </summary>
      /// <param name="zl"></param>
      /// <param name="lastcolstd">wenn true, hat die letzte Spalte der <see cref="Subtile"/> immer die Standardbreite</param>
      /// <param name="subtilesize">Standardbreite und -höhe eines <see cref="Subtile"/></param>
      /// <returns></returns>
      Subtile[,] BuildEmptySubtileArray(Data4Zoomlevel zl, bool lastcolstd, int subtilesize) {
         int ptx = (int)(zl.Width / zl.Londist) + 1;
         int pty = (int)(zl.Height / zl.Latdist) + 1;

         if (lastcolstd) {    // dann ptx auf Vielfache von tilesize vergrößern
            int remainder = ptx % subtilesize;
            if (remainder > 0)
               ptx += subtilesize - remainder;
         }

         int subtilesx = ptx / subtilesize;     // Anzahl der Subtiles nebeneinander
         ptx -= subtilesx * subtilesize;        // restl. Punkte (0..63)
         ptx += subtilesize;                    // 64..127

         //if (ptx > subtilesize / 2)             // waagerechte Punktanzahl für die letzte Spalte 33..63
         //   subtilesx++;
         //else {                              // waagerechte Punktanzahl für die letzte Spalte 0..32
         //   ptx += subtilesize;                 // => 64..96
         //}

         int subtilesy = pty / subtilesize;     // Anzahl der Subtiles untereinander
         pty -= subtilesy * subtilesize;
         if (pty > 0)
            subtilesy++;
         else
            pty = subtilesize;   // Anzahl für das unterste Subtile ist bei Rest 0 trotzdem 64

         Subtile[,] subtiles = new Subtile[subtilesx, subtilesy];

         double subtiledeltalon = zl.Londist * subtilesize;
         double subtiledeltalat = zl.Latdist * subtilesize;
         double left = zl.Left;

         for (int x = 0; x < subtiles.GetLength(0); x++, left += subtiledeltalon) {
            double top = zl.Top;
            for (int y = 0; y < subtiles.GetLength(1); y++, top -= subtiledeltalat) {
               int loncount = x == subtiles.GetLength(0) - 1 ? ptx : subtilesize;
               int latcount = y == subtiles.GetLength(1) - 1 ? pty : subtilesize;
               subtiles[x, y] = new Subtile(left, top, zl.Londist, zl.Latdist, loncount, latcount);
            }
         }

         return subtiles;
      }

      /// <summary>
      /// bildet ein <see cref="Subtile"/>-Array in dem jedes <see cref="Subtile"/> die entsprechenden int-Daten enthält
      /// </summary>
      /// <param name="srcdata">Koordinatenursprung links oben</param>
      /// <param name="lastcolstd">wenn true, hat die letzte Spalte der <see cref="Subtile"/> immer die Standardbreite</param>
      /// <param name="subtilesize">Standardbreite und -höhe eines <see cref="Subtile"/></param>
      /// <returns></returns>
      Subtile[,] BuildFilledSubtileArray(Data2Dim srcdata, bool lastcolstd, int subtilesize) {
         int ptx = srcdata.Width;
         int pty = srcdata.Height;

         if (lastcolstd) {    // dann ptx und pty auf Vielfache von tilesize vergrößern
            int remainder = ptx % subtilesize;
            if (remainder > 0) {
               srcdata.IncreaseWidth(subtilesize - remainder);
               ptx = srcdata.Width;
            }
         }

         int subtilesx = ptx / subtilesize;     // Anzahl der Subtiles nebeneinander
         ptx -= subtilesx * subtilesize;        // restl. Punkte (0..63)
         ptx += subtilesize;                    // 64..127

         //if (ptx > subtilesize / 2)             // waagerechte Punktanzahl für die letzte Spalte 33..63
         //   subtilesx++;
         //else {                              // waagerechte Punktanzahl für die letzte Spalte 0..32
         //   ptx += subtilesize;                 // => 64..96
         //}

         int subtilesy = pty / subtilesize;     // Anzahl der Subtiles untereinander
         pty -= subtilesy * subtilesize;
         if (pty > 0)
            subtilesy++;

         Subtile[,] subtiles = new Subtile[subtilesx, subtilesy];

         for (int x = 0; x < subtiles.GetLength(0); x++) {
            for (int y = 0; y < subtiles.GetLength(1); y++) {
               int left = x * subtilesize;
               int top = y * subtilesize;
               int dx = x == subtiles.GetLength(0) - 1 ? ptx : subtilesize;
               int dy = y == subtiles.GetLength(1) - 1 ? pty : subtilesize;
               subtiles[x, y] = new Subtile(new Data2Dim(srcdata.GetRange(left, top, dx, dy), dx));
            }
         }

         return subtiles;
      }

      /// <summary>
      /// Tabellengröße eines Zommlevels bestimmen
      /// </summary>
      /// <param name="zoomleveldata"></param>
      /// <param name="firstlevel"></param>
      void FitTableSize(ZoomlevelData zoomleveldata, bool firstlevel) {
         int minbaseheight = int.MaxValue;
         int maxbaseheight = int.MinValue;
         int maxheight = int.MinValue;
         uint maxoffset = uint.MinValue;

         uint offset = 0;
         for (int i = 0; i < zoomleveldata.Subtiles.Count; i++) {
            int baseheight = zoomleveldata.Subtiles[i].Tableitem.Baseheight;
            minbaseheight = Math.Min(minbaseheight, baseheight);
            maxbaseheight = Math.Max(maxbaseheight, baseheight);
            maxheight = Math.Max(maxheight, baseheight + zoomleveldata.Subtiles[i].Tableitem.Diff);
            zoomleveldata.Subtiles[i].Tableitem.Offset = zoomleveldata.Subtiles[i].DataLength > 0 ? offset : 0;
            offset += (uint)zoomleveldata.Subtiles[i].DataLength;
            maxoffset = Math.Max(maxoffset, zoomleveldata.Subtiles[i].Tableitem.Offset); // größter zu speichernder Zahlenwert
         }

         zoomleveldata.Tableitem.MinHeight = (short)minbaseheight;
         zoomleveldata.Tableitem.MaxHeight = (ushort)maxheight;

         if (maxoffset < 256)
            zoomleveldata.Tableitem.Structure_OffsetSize = 1;
         else if (maxoffset < 256 * 256)
            zoomleveldata.Tableitem.Structure_OffsetSize = 2;
         else if (maxoffset < 256 * 256 * 256)
            zoomleveldata.Tableitem.Structure_OffsetSize = 3;
         else
            zoomleveldata.Tableitem.Structure_OffsetSize = 4;

         // maxbaseheight kann auch negativ sein!
         if (-128 < minbaseheight && maxbaseheight < 128)
            zoomleveldata.Tableitem.Structure_BaseheightSize = 1;
         else
            zoomleveldata.Tableitem.Structure_BaseheightSize = 2;

         if (maxheight < 255)
            zoomleveldata.Tableitem.Structure_DiffSize = 1;
         else
            zoomleveldata.Tableitem.Structure_DiffSize = 2;

         zoomleveldata.Tableitem.Structure_CodingtypeSize = firstlevel ? 1 : 0;
      }

      /// <summary>
      /// Zoomlevel-Definitionen erzeugen
      /// </summary>
      /// <param name="tiles">Subtiles je Zoomlevel</param>
      /// <param name="head">Header der erzeugt wird</param>
      /// <returns></returns>
      List<ZoomlevelData> CreateZoomlevel(List<Subtile[,]> tiles, Head head) {
         List<ZoomlevelData> zl = new List<ZoomlevelData>();

         for (int z = 0; z < tiles.Count; z++) {
            zl.Add(new ZoomlevelData());

            //// Subtiles in den Zoomlevel übernehmen
            int min = int.MaxValue;
            int max = int.MinValue;
            for (int y = 0; y < tiles[z].GetLength(1); y++) {
               for (int x = 0; x < tiles[z].GetLength(0); x++) {
                  zl[z].Subtiles.Add(tiles[z][x, y]);
                  min = Math.Min(min, tiles[z][x, y].BaseHeight);
                  max = Math.Max(max, tiles[z][x, y].BaseHeight + tiles[z][x, y].MaxDiffHeight);
               }
            }
            zl[z].Tableitem.MinHeight = (short)min;
            zl[z].Tableitem.MaxHeight = (ushort)(max - min);

            zl[z].Tableitem.No = (byte)z;
            zl[z].Tableitem.PointsHoriz = STDSUBTILESIZE;
            zl[z].Tableitem.PointsVert = STDSUBTILESIZE;

            zl[z].Tableitem.PointDistanceHoriz = data4Zoomlevel[z].Londist;
            zl[z].Tableitem.PointDistanceVert = data4Zoomlevel[z].Latdist;

            zl[z].Tableitem.West = data4Zoomlevel[z].Left;
            zl[z].Tableitem.North = data4Zoomlevel[z].Top;

            zl[z].Tableitem.MaxIdxHoriz = tiles[z].GetLength(0) - 1;    // Oder Anzahl der waagerechten 64er Kacheln?
            zl[z].Tableitem.MaxIdxVert = tiles[z].GetLength(1) - 1;
            zl[z].Tableitem.LastColWidth = tiles[z][tiles[z].GetLength(0) - 1, 0].Width - 1;
            zl[z].Tableitem.LastRowHeight = tiles[z][0, tiles[z].GetLength(1) - 1].Height - 1;

            zl[z].Tableitem.Unknown12 = 0;  // <== bei mehr als einem Zoomlevel verschieden, z.B. 0, 1, 2 ,4 !!!!!

         }

         // Tabellengrößen festlegen und Pointer ermitteln
         for (int z = 0; z < tiles.Count; z++) {
            FitTableSize(zl[z], z == 0);

            zl[z].Tableitem.PtrSubtileTable = z == 0 ?
                           head.Length :
                           zl[z - 1].Tableitem.PtrHeightdata + zl[z - 1].GetHeightDataAreaSize();
            zl[z].Tableitem.PtrHeightdata = zl[z].Tableitem.PtrSubtileTable + zl[z].GetTableAreaSize();
         }

         return zl;
      }

      /// <summary>
      /// DEM-Datei schreiben
      /// </summary>
      /// <param name="w"></param>
      /// <param name="head"></param>
      /// <param name="Zoomlevel"></param>
      void WriteDEM(BinaryWriter w, Head head, List<ZoomlevelData> Zoomlevel) {
         head.Write(w);

         for (int z = 0; z < Zoomlevel.Count; z++) {
            // Subtile-Tabelle schreiben
            for (int i = 0; i < Zoomlevel[z].Subtiles.Count; i++)
               Zoomlevel[z].Subtiles[i].Tableitem.Write(w,
                                                        Zoomlevel[z].Tableitem.Structure_OffsetSize,
                                                        Zoomlevel[z].Tableitem.Structure_BaseheightSize,
                                                        Zoomlevel[z].Tableitem.Structure_DiffSize,
                                                        Zoomlevel[z].Tableitem.Structure_CodingtypeSize);

            // Datenschreiben
            for (int i = 0; i < Zoomlevel[z].Subtiles.Count; i++)
               if (Zoomlevel[z].Subtiles[i].CodedData != null) // sollte immer der Fall sein
                  w.Write(Zoomlevel[z].Subtiles[i].CodedData);
         }

         // Zoomlevel-Tabelle schreiben
         for (int z = 0; z < Zoomlevel.Count; z++)
            Zoomlevel[z].Tableitem.Write(w);
      }




   }

}
