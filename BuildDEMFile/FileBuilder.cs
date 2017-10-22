using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace BuildDEMFile {

   class FileBuilder {

      const int STDTILESIZE = 64;


      class Data4Zoomlevel {
         /// <summary>
         /// Datendatei
         /// </summary>
         public string Datafile;
         /// <summary>
         /// Pfad zu den HGT-Dateien
         /// </summary>
         public string HgtPath;
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
         /// Ausgabedatei für HGT-Daten
         /// </summary>
         public string HgtOutput;

         /// <summary>
         /// 
         /// </summary>
         /// <param name="Datafile">Datendatei</param>
         /// <param name="HgtPath">Pfad zu den HGT-Dateien</param>
         /// <param name="Left">westliche DEM-Grenze</param>
         /// <param name="Top">nördliche DEM-Grenze</param>
         /// <param name="Width">Breite DEM-Grenze</param>
         /// <param name="Height">Höhe DEM-Grenze</param>
         /// <param name="Latdist">Höhe des Punktabstandes</param>
         /// <param name="Londist">Breite des Punktabstandes</param>
         /// <param name="HgtOutput">Ausgabedatei für HGT-Daten</param>
         public Data4Zoomlevel(string Datafile, string HgtPath, double Left, double Top, double Width, double Height, double Latdist, double Londist, string HgtOutput) {
            this.Datafile = string.IsNullOrEmpty(Datafile) ? null : Datafile.Trim();
            this.HgtPath = string.IsNullOrEmpty(HgtPath) ? null : HgtPath.Trim();
            this.Left = Left;
            this.Top = Top;
            this.Latdist = Latdist;
            this.Londist = Londist;
            this.Width = Width;
            this.Height = Height;
            this.HgtOutput = string.IsNullOrEmpty(HgtOutput) ? null : HgtOutput.Trim(); ;
         }

      }

      List<Data4Zoomlevel> data4Zoomlevel;


      /// <summary>
      /// 
      /// </summary>
      /// <param name="datafile">Textdatei mit Höhendaten</param>
      /// <param name="hgtpath">Pfad zu den HGT-Dateien</param>
      /// <param name="hgtoutput">Datei für die Ausgabe der Textdaten</param>
      /// <param name="left">Ecke links-oben</param>
      /// <param name="top">Ecke links-oben</param>
      /// <param name="width">Breite des DEM-Bereiches</param>
      /// <param name="height">Höhe des DEM-Bereiches</param>
      /// <param name="londist">Breite des Punktabstandes</param>
      /// <param name="latdist">Höhe des Punktabstandes</param>
      public FileBuilder(List<string> datafile,
                         List<string> hgtpath,
                         List<string> hgtoutput,
                         List<double> left, List<double> top,
                         List<double> width, List<double> height,
                         List<double> londist, List<double> latdist) {

         data4Zoomlevel = new List<FileBuilder.Data4Zoomlevel>();

         int count = Math.Min(datafile.Count, hgtpath.Count);
         count = Math.Min(count, hgtoutput.Count);
         count = Math.Min(count, left.Count);
         count = Math.Min(count, top.Count);
         count = Math.Min(count, londist.Count);
         count = Math.Min(count, latdist.Count);
         count = Math.Min(count, width.Count);
         count = Math.Min(count, height.Count);

         for (int i = 0; i < count; i++) {
            data4Zoomlevel.Add(new Data4Zoomlevel(datafile[i], hgtpath[i], left[i], top[i], width[i], height[i], latdist[i], londist[i], hgtoutput[i]));

            if (data4Zoomlevel[i].Datafile != null) {
               data4Zoomlevel[i].HgtPath = null;
               data4Zoomlevel[i].HgtOutput = null;
            }
            if (data4Zoomlevel[i].HgtPath != null)
               data4Zoomlevel[i].Datafile = null;

            if (data4Zoomlevel[i].HgtPath == null &&
                data4Zoomlevel[i].Datafile == null)
               throw new Exception("Entweder eine Datendatei oder ein HGT-Pfad muss angegeben sein.");

            if (double.IsNaN(data4Zoomlevel[i].Left) ||
                double.IsNaN(data4Zoomlevel[i].Top))
               throw new Exception("Die Position des DEM-Bereiches muss angegeben sein.");

            if (data4Zoomlevel[i].HgtPath != null) {
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
              new List<string>() { hgtpath },
              new List<string>() { hgtoutput },
              new List<double>() { left },
              new List<double>() { top },
              new List<double>() { width },
              new List<double>() { height },
              new List<double>() { ptdist },
              new List<double>() { ptdist }) { }

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
              new List<string>() { null },
              new List<string>() { null },
              new List<double>() { left },
              new List<double>() { top },
              new List<double>() { width },
              new List<double>() { height },
              new List<double>() { double.NaN },
              new List<double>() { double.NaN }) { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="datafile">Textdatei mit Höhendaten</param>
      /// <param name="left">Ecke links-oben</param>
      /// <param name="top">Ecke links-oben</param>
      /// <param name="ptdist">Punktabstand</param>
      public FileBuilder(string datafile, double left, double top, double ptdist) :
         this(new List<string>() { datafile },
              new List<string>() { null },
              new List<string>() { null },
              new List<double>() { left },
              new List<double>() { top },
              new List<double>() { double.NaN },
              new List<double>() { double.NaN },
              new List<double>() { ptdist },
              new List<double>() { ptdist }) { }


      /// <summary>
      /// Erzeugt die DEM-Datei
      /// </summary>
      /// <param name="demfile">Name der DEM-Datei</param>
      /// <param name="footflag">Angaben in Fuß (oder Meter)</param>
      /// <param name="lastcolstd">letzte Spalte mit Standardbreite</param>
      /// <param name="overwrite"></param>
      /// <param name="maxthreads"></param>
      /// <returns></returns>
      public bool Create(string demfile, bool footflag, bool lastcolstd, bool overwrite, int maxthreads = 0) {

         if (!overwrite &&
             File.Exists(demfile)) {
            Console.Error.WriteLine("Die Datei '" + demfile + "' existiert schon.");
            return false;
         }

         List<Subtile[,]> tiles4zoomlevel = new List<Subtile[,]>(); // für jeden Zoomlevel ein Tile-Array

         Console.WriteLine(data4Zoomlevel.Count.ToString() + " Zoomlevel");
         // Daten einlesen
         for (int z = 0; z < data4Zoomlevel.Count; z++) {
            Data2Dim rawdata;
            int minheight, maxheight;
            if (!string.IsNullOrEmpty(data4Zoomlevel[z].HgtPath)) {
               /* ACHTUNG
                * Wenn die Breite des Gebiets 1 Punktabstand ist, werden 2 Punkte ermittelt, d.h.
                * wenn die Breite des Gebiets n Punktabstände ist, werden n+1 Punkte ermittelt.
                * Für ein Vielfaches von 64 Punkten wird also eine Breite x*64 - 1 benötigt.
               */
               if (lastcolstd) { // notfalls auf 64er-Breite bringen
                  double t = data4Zoomlevel[z].Width / (STDTILESIZE * data4Zoomlevel[z].Londist);
                  if ((int)t < t) {
                     data4Zoomlevel[z].Width = (1 + (int)t) * STDTILESIZE * data4Zoomlevel[z].Londist;
                     data4Zoomlevel[z].Width -= 1.1 * data4Zoomlevel[z].Londist;
                  }
               }
               rawdata = ReadData(data4Zoomlevel[z].HgtPath,
                                  data4Zoomlevel[z].HgtOutput,
                                  data4Zoomlevel[z].Left,
                                  data4Zoomlevel[z].Top,
                                  data4Zoomlevel[z].Width,
                                  data4Zoomlevel[z].Height,
                                  data4Zoomlevel[z].Londist,
                                  data4Zoomlevel[z].Latdist,
                                  footflag,
                                  out minheight,
                                  out maxheight);
            } else {
               rawdata = ReadData(data4Zoomlevel[z].Datafile, out minheight, out maxheight);

               if (Double.IsNaN(data4Zoomlevel[z].Width)) {
                  data4Zoomlevel[z].Width = rawdata.Width * data4Zoomlevel[z].Latdist;
               } else {
                  if (Double.IsNaN(data4Zoomlevel[z].Latdist)) {
                     data4Zoomlevel[z].Latdist = data4Zoomlevel[z].Width / rawdata.Width;
                  }
               }

               if (Double.IsNaN(data4Zoomlevel[z].Height)) {
                  data4Zoomlevel[z].Height = rawdata.Height * data4Zoomlevel[z].Latdist;
               } else {
                  if (Double.IsNaN(data4Zoomlevel[z].Londist)) {
                     data4Zoomlevel[z].Londist = data4Zoomlevel[z].Height / rawdata.Height;
                  }
               }

            }
            Console.WriteLine("Daten " + rawdata.ToString() + ", Min. " + minheight.ToString() + ", Max. " + maxheight.ToString() + " für Zoomlevel " + z.ToString());

            if (rawdata.Height > 0 && rawdata.Width > 0) {
               Subtile[,] tiles = BuildSubtileArrays(rawdata, lastcolstd, STDTILESIZE);
               Console.WriteLine("Kachelanzahl " + tiles.GetLength(0) + " x " + tiles.GetLength(1) +
                                                " (rechte Spalte " + tiles[tiles.GetLength(0) - 1, 0].Width.ToString() + " breit, untere Zeile " + tiles[0, tiles.GetLength(1) - 1].Height.ToString() + " hoch)");
               tiles4zoomlevel.Add(tiles);
            }

            Console.WriteLine(string.Format("Rand links {0}°, oben {1}°", data4Zoomlevel[z].Left, data4Zoomlevel[z].Top));
            Console.WriteLine(string.Format("Pixelgröße {0}° x {1}°", data4Zoomlevel[z].Londist, data4Zoomlevel[z].Latdist));
         }

         if (tiles4zoomlevel.Count > 0) {

            if (maxthreads <= 0)
               maxthreads = Environment.ProcessorCount;
            Console.Error.WriteLine(maxthreads == 1 ? "{0} Thread" : "{0} Threads", maxthreads);

            MyThreadgroup tg = new MyThreadgroup(maxthreads);
            tg.InfoEvent += new MyThreadgroup.Info(tg_InfoEvent);

            Console.WriteLine("encodiere Kacheln ...");
            // alle Daten encodieren
            int count = 0;
            for (int z = 0; z < tiles4zoomlevel.Count; z++)
               for (int y = 0; y < tiles4zoomlevel[z].GetLength(1); y++)
                  for (int x = 0; x < tiles4zoomlevel[z].GetLength(0); x++) {

                     tg.Start(new object[] { tiles4zoomlevel[z][x, y] });

                     //tiles4zoomlevel[z][x, y].Encoding();
                     //Console.Write(".");

                     count++;

                     if (count % 50 == 0)
                        Console.Write(".");
                  }

            tg.NothingToDo.WaitOne();

            Console.WriteLine();
            Console.WriteLine(count.ToString() + " Kacheln encodiert");

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

         } else {
            Console.Error.WriteLine("zu wenig Daten");
            return false;
         }
         return true;
      }

      /// <summary>
      /// liest die gesamten Höhendaten aus der Textdatei ein
      /// </summary>
      /// <param name="txtfile"></param>
      /// <param name="minheight"></param>
      /// <param name="maxheight"></param>
      /// <returns></returns>
      Data2Dim ReadData(string txtfile, out int minheight, out int maxheight) {
         List<int> heights = new List<int>();
         int width = -1;
         int height = 0;
         maxheight = int.MinValue;
         minheight = int.MaxValue;

         try {
            String txt = ReadTxt(txtfile);
            string[] lines = txt.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines) {
               string[] sData = line.Split(new char[] { ' ', ',', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);

               if (width < 0) // 1. Datenzeile gibt die Breite vor
                  width = sData.Length;
               else {
                  if (width != sData.Length)
                     throw new Exception("Die Datenanzahl (" + sData.Length.ToString() + ") in Datenzeile " + (height + 1).ToString() + " ist nicht identisch zur 1. Zeile (" + width.ToString() + ").");
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
            Console.WriteLine("Die Datei '" + txtfile + "' konnte nicht gelesen werden:");
            Console.WriteLine(e.Message);
         }

         return new Data2Dim(heights, width);
      }

      /// <summary>
      /// liest die gesamten Höhendaten aus den HGT-Daten ein
      /// </summary>
      /// <param name="hgtpath"></param>
      /// <param name="hgtoutput"></param>
      /// <param name="left"></param>
      /// <param name="top"></param>
      /// <param name="width"></param>
      /// <param name="height"></param>
      /// <param name="stepwidth"></param>
      /// <param name="stepheight"></param>
      /// <param name="foot"></param>
      /// <returns></returns>
      Data2Dim ReadData(string hgtpath, string hgtoutput,
                        double left, double top, double width, double height,
                        double stepwidth, double stepheight,
                        bool foot,
                        out int minheight, out int maxheight) {
         maxheight = int.MinValue;
         minheight = int.MaxValue;
         // ---- Daten einlesen und konvertieren ----
         DataConverter conv = new DataConverter(left, top, width, height);
         conv.ReadData(hgtpath);
         int[,] heights = conv.BuildHeightArray(stepwidth, stepheight, foot);

         for (int i = 0; i < heights.GetLength(0); i++)
            for (int j = 0; j < heights.GetLength(1); j++) {
               minheight = Math.Min(minheight, heights[i, j]);
               if (heights[i, j] < Subtile.UNDEF)
                  maxheight = Math.Max(maxheight, heights[i, j]);
            }

         if (!string.IsNullOrEmpty(hgtoutput)) {
            if (Path.GetExtension(hgtoutput).ToUpper() == ".ZIP") {
               Console.WriteLine("schreibe extrahierte HGT-Daten in die Datei '" + hgtoutput + "' ...");

               using (FileStream zipstream = new FileStream(hgtoutput, FileMode.Create)) {
                  using (ZipArchive zip = new ZipArchive(zipstream, ZipArchiveMode.Create)) {
                     ZipArchiveEntry entry = zip.CreateEntry(Path.GetFileNameWithoutExtension(hgtoutput), CompressionLevel.Optimal);
                     using (StreamWriter w = new StreamWriter(entry.Open())) {
                        for (int j = 0; j < heights.GetLength(1); j++) {
                           for (int i = 0; i < heights.GetLength(0); i++) {
                              if (i > 0)
                                 w.Write(" ");
                              w.Write(heights[i, j]);
                           }
                           w.WriteLine();
                        }
                     }
                  }
               }

            } else {

               using (StreamWriter w = new StreamWriter(hgtoutput)) {
                  for (int j = 0; j < heights.GetLength(1); j++) {
                     for (int i = 0; i < heights.GetLength(0); i++) {
                        if (i > 0)
                           w.Write(" ");
                        w.Write(heights[i, j]);
                     }
                     w.WriteLine();
                  }
               }

            }
         }

         return new Data2Dim(heights);
      }

      /// <summary>
      /// liest den gesamten Text einer Textdatei oder der 1. Datei in einer ZIP-Datei
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      string ReadTxt(string file) {
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



      const int MINWIDTH = 63;

      /// <summary>
      /// bildet ein <see cref="Subtile"/>-Array in dem jedes <see cref="Subtile"/> die entsprechenden int-Daten enthält
      /// </summary>
      /// <param name="srcdata"></param>
      /// <param name="lastcolstd">letzte Spalte mit Standardbreite</param>
      /// <param name="tilesize"></param>
      /// <returns></returns>
      Subtile[,] BuildSubtileArrays(Data2Dim srcdata, bool lastcolstd, int tilesize) {
         if (lastcolstd) {
            int increase = tilesize - srcdata.Width % tilesize;
            if (increase != tilesize)
               //srcdata.IncreaseWidth(increase, Subtile.UNDEF);
               srcdata.IncreaseWidth(increase);
         }

         Data2Dim[,] tmpdataarray = new Data2Dim[srcdata.Width / tilesize + 1, srcdata.Height / tilesize + 1];

         for (int y = 0; y < tmpdataarray.GetLength(1); y++) {
            int startidxx = 0;
            int startidxy = y * tilesize;
            for (int x = 0; x < tmpdataarray.GetLength(0); x++) {
               int dx = tilesize, dy = tilesize;

               if (startidxx + dx + MINWIDTH > srcdata.Width) // Rest: Wenn der Rest > tilesize + MINWIDTH, wird ein Standard-Tile gebildet. -> MINWIDTH <= dx <= tilesize + MINWIDTH + 1
                  dx = srcdata.Width - startidxx;

               if (startidxy + dy > srcdata.Height) // Rest
                  dy = srcdata.Height - startidxy;

               if (startidxx + dx <= srcdata.Width &&
                   startidxy + dy <= srcdata.Height &&
                   dx > 0 &&
                   dy > 0) {
                  tmpdataarray[x, y] = new Data2Dim(srcdata.GetRange(startidxx, startidxy, dx, dy), dx);
                  startidxx += dx;
               } else
                  tmpdataarray[x, y] = null;
            }
         }

         int dw = 0, dh = 0;
         for (int i = 0; i < tmpdataarray.GetLength(0); i++)
            if (tmpdataarray[i, 0] == null)
               break;
            else
               dw++;
         for (int i = 0; i < tmpdataarray.GetLength(1); i++)
            if (tmpdataarray[0, i] == null)
               break;
            else
               dh++;

         Subtile[,] tilearray = new Subtile[dw, dh];
         for (int i = 0; i < dw; i++)
            for (int j = 0; j < dh; j++)
               tilearray[i, j] = new Subtile(tmpdataarray[i, j]);

         return tilearray;
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

         if (maxoffset < 255)
            zoomleveldata.Tableitem.Structure_OffsetSize = 1;
         else if (maxoffset < 65536)
            zoomleveldata.Tableitem.Structure_OffsetSize = 2;
         else
            zoomleveldata.Tableitem.Structure_OffsetSize = 3;

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
            for (int y = 0; y < tiles[z].GetLength(1); y++)
               for (int x = 0; x < tiles[z].GetLength(0); x++)
                  zl[z].Subtiles.Add(tiles[z][x, y]);

            zl[z].Tableitem.No = (ushort)z;
            zl[z].Tableitem.PointsHoriz = STDTILESIZE;
            zl[z].Tableitem.PointsVert = STDTILESIZE;

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
            zl[z].Tableitem.PtrHeightdata = (uint)(zl[z].Tableitem.PtrSubtileTable + zl[z].GetTableAreaSize());
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
               w.Write(Zoomlevel[z].Subtiles[i].CodedData);
         }

         // Zoomlevel-Tabelle schreiben
         for (int z = 0; z < Zoomlevel.Count; z++)
            Zoomlevel[z].Tableitem.Write(w);
      }


      #region Threadgroup

      abstract class Threadgroup {

         /// <summary>
         /// max. Anzahl von erlaubten Threads
         /// </summary>
         int max;
         /// <summary>
         /// Anzahl der akt. Threads
         /// </summary>
         int threadcount;
         /// <summary>
         /// Lock-Objekt für die interne Nutzung
         /// </summary>
         object locker;
         /// <summary>
         /// gesetzt, wenn kein Thread mehr läuft
         /// </summary>
         public ManualResetEvent NothingToDo { get; private set; }

         ManualResetEvent[] EndEvent;
         bool[] EndEventIsInUse;
         private int i;

         public Threadgroup(int max) {
            this.max = max;
            threadcount = 0;
            NothingToDo = new ManualResetEvent(false);
            EndEvent = new ManualResetEvent[max];
            EndEventIsInUse = new bool[max];
            for (int i = 0; i < EndEvent.Length; i++) {
               EndEvent[i] = new ManualResetEvent(false);
               EndEventIsInUse[i] = false;
            }
            locker = new object();
         }

         /// <summary>
         /// startet einen Thread (sofort, oder wenn wieder einer "frei wird")
         /// </summary>
         /// <param name="para"></param>
         public void Start(object para) {

            Monitor.Enter(locker);
            int actualthreadcount = threadcount;
            NothingToDo.Reset(); // auf jeden Fall
            Monitor.Exit(locker);

            // Index eines freien Threadplatzes ermitteln (ev. warten bis ein Threadplatz frei wird)
            int idx;
            if (actualthreadcount >= max) {                 // z.Z. kein freier Thread
               idx = WaitHandle.WaitAny(EndEvent);
               EndEvent[idx].Reset();
            } else {
               Monitor.Enter(locker);
               for (idx = 0; idx < EndEventIsInUse.Length; idx++)
                  if (!EndEventIsInUse[idx])
                     break;
            }

            if (!Monitor.IsEntered(locker))
               Monitor.Enter(locker);

            threadcount++;
            EndEventIsInUse[idx] = true;
            Thread t = new Thread(DoWorkFrame);       // Thread erzeugen ...
            t.Start(new object[] { idx, para });      // ... und starten

            Monitor.Exit(locker);
         }

         protected void DoWorkFrame(object para) {
            object[] data = para as object[];
            int freeidx = (int)data[0];
            DoWork(data[1]);

            Monitor.Enter(locker);

            threadcount--;
            EndEventIsInUse[freeidx] = false;
            EndEvent[freeidx].Set();
            if (threadcount == 0)
               NothingToDo.Set(); // das war der letzte Thread

            Monitor.Exit(locker);
         }

         protected virtual void DoWork(object para) { }

      }

      /// <summary>
      /// Threadgruppe für die Verkettung
      /// </summary>
      class MyThreadgroup : Threadgroup {

         public delegate void Info(string txt);
         public event Info InfoEvent;

         public MyThreadgroup(int max)
            : base(max) { }

         protected override void DoWork(object para) {
            if (para is object[]) {
               object[] args = para as object[];
               if (args.Length == 1) {

                  if (args[0] is Subtile) {
                     (args[0] as Subtile).Encoding();
                     //InfoEvent(".");
                  }

               }
            }
         }
      }

      /// <summary>
      /// Lock-Objekt für das Schreiben auf die Konsole
      /// </summary>
      object consolewritelocker = new object();

      /// <summary>
      /// Zwischenmeldung damit der Anwender nicht nervös wird
      /// </summary>
      void tg_InfoEvent(string txt) {
         lock (consolewritelocker) {
            Console.Write(txt);
         }
      }

      #endregion

   }

}
