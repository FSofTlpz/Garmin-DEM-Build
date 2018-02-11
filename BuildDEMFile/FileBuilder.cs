using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace BuildDEMFile {

   class FileBuilder {

      const int STDSUBTILESIZE = 64;

      const int SUBTILEPAKETSIZE = 200;

      class Data4Zoomlevel {
         /// <summary>
         /// Datendatei
         /// </summary>
         public string Datafile;
         /// <summary>
         /// westliche DEM-Grenze
         /// </summary>
         public int Left;
         /// <summary>
         /// nördliche DEM-Grenze
         /// </summary>
         public int Top;
         /// <summary>
         /// Breite des DEM-Bereiches
         /// </summary>
         public int Width;
         /// <summary>
         /// Höhe des DEM-Bereiches
         /// </summary>
         public int Height;
         /// <summary>
         /// Höhe des Punktabstandes
         /// </summary>
         public int Latdist;
         /// <summary>
         /// Breite des Punktabstandes
         /// </summary>
         public int Londist;
         /// <summary>
         /// Verkleinerungsfaktor
         /// </summary>
         public int Shrink;
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
         public Data4Zoomlevel(string Datafile, int Left, int Top, int Width, int Height, int Latdist, int Londist, int Shrink) {
            this.Datafile = string.IsNullOrEmpty(Datafile) ? null : Datafile.Trim();
            this.Left = Left;
            this.Top = Top;
            this.Latdist = Latdist;
            this.Londist = Londist;
            this.Width = Width;
            this.Height = Height;
            this.Shrink = Shrink;
         }

      }


      public FileBuilder() { }

      /// <summary>
      /// Erzeugt die DEM-Datei
      /// </summary>
      /// <param name="DemDataPaths">Pfade zu den DEM-Dateien</param>
      /// <param name="DemDataFile">einzelne DEM-Datendateien (1 je Zoomlevel)</param>
      /// <param name="DemFile">zu erzeugende Garmin-DEM-Datei</param>
      /// <param name="Left">linker Rand des Gebietes</param>
      /// <param name="Top">oberer Rand des Gebietes</param>
      /// <param name="Width">Breite des Gebietes</param>
      /// <param name="Height">Höhe des Gebietes</param>
      /// <param name="LonDistance">Punktabstabstand waagerecht</param>
      /// <param name="LatDistance">Punktabstabstand senkrecht</param>
      /// <param name="DemOutFile">Input für Encoder als Textdatei ausgeben</param>
      /// <param name="footflag">Daten in Fuß</param>
      /// <param name="lastcolstd">letzte 64x64 Spalte mit Standardbreite</param>
      /// <param name="overwrite">Zieldatei/en ev. überschreiben</param>
      /// <param name="dummydataonerror">Dummywerte falls keine DEM Daten ex.</param>
      /// <param name="usetestencoder">Testencoder verwenden</param>
      /// <param name="maxthreads"></param>
      /// <returns></returns>
      public bool Create(List<string> DemDataPaths,
                          List<string> DemDataFile,
                          string DemFile,
                          List<int> Left,
                          List<int> Top,
                          List<int> Width,
                          List<int> Height,
                          List<int> LonDistance,
                          List<int> LatDistance,
                          List<int> Shrink,
                          bool DemOutFile,
                          bool footflag,
                          bool lastcolstd,
                          bool overwrite,
                          bool dummydataonerror,
                          bool stdintpol,
                          bool usetestencoder,
                          int maxthreads = 0) {
         DateTime starttime = DateTime.Now;

         if (!overwrite &&
             File.Exists(DemFile)) {
            Console.Error.WriteLine("File '" + DemFile + "' exist and must not overwritten.");
            return false;
         }
         Console.WriteLine("create '" + DemFile + "' ...");

         if (LonDistance.Count != LatDistance.Count)
            throw new Exception("number of 'LonDistance' unequal number of 'LatDistance' (is number of Zoomlevels)");
         if (DemDataFile.Count > 0 && DemDataFile.Count != LatDistance.Count)
            throw new Exception("number of 'DemDataFile' unequal number of 'LatDistance' (is number of Zoomlevels)");
         if (DemDataFile.Count == 0 && DemDataPaths.Count == 0)
            throw new Exception("no dem source given");

         List<Data4Zoomlevel> data4Zoomlevel = new List<Data4Zoomlevel>();
         for (int i = 0; i < LonDistance.Count; i++)
            data4Zoomlevel.Add(new Data4Zoomlevel(DemDataFile.Count == LonDistance.Count ? DemDataFile[i] : "",
                                                  Left[i],
                                                  Top[i],
                                                  Width[i],
                                                  Height[i],
                                                  LatDistance[i],
                                                  LonDistance[i],
                                                  Shrink[i]));

         Console.WriteLine(data4Zoomlevel.Count.ToString() + " zoomlevel");

         List<Subtile[,]> tiles4zoomlevel = new List<Subtile[,]>(); // für jeden Zoomlevel ein Tile-Array
         DEMDataConverter demconverter = null;

         if (DemDataPaths.Count > 0) { // ------------------- Daten (für alle Zoomlevel) nur 1x aus den DEM-Daten einlesen
            double mostleft = double.MaxValue;
            double mostright = double.MinValue;
            double mostbottom = double.MaxValue;
            double mosttop = double.MinValue;
            for (int z = 0; z < data4Zoomlevel.Count; z++) {
               // Zoomlevel notfalls auf 64er-Breite bringen
               if (lastcolstd) {
                  /* ACHTUNG
                   * Wenn die Breite des Gebiets 1 Punktabstand ist, werden 2 Punkte ermittelt, d.h.
                   * wenn die Breite des Gebiets n Punktabstände ist, werden n+1 Punkte ermittelt.
                   * Für ein Vielfaches von 64 Punkten wird also eine Breite x*64 - 1 benötigt.
                  */
                  if ((data4Zoomlevel[z].Width - 1) % (STDSUBTILESIZE * data4Zoomlevel[z].Londist) != 0) {
                     int t = (data4Zoomlevel[z].Width - 1) / (STDSUBTILESIZE * data4Zoomlevel[z].Londist) + 1;
                     data4Zoomlevel[z].Width = t * (STDSUBTILESIZE * data4Zoomlevel[z].Londist) + 1;
                  }
               }

               Subtile[,] tiles = BuildEmptySubtileArray(data4Zoomlevel[z], lastcolstd, STDSUBTILESIZE);
               tiles4zoomlevel.Add(tiles);

               foreach (Subtile tile in tiles) {
                  mostleft = Math.Min(mostleft, tile.PlannedLeft);
                  mosttop = Math.Max(mosttop, tile.PlannedTop);
                  mostright = Math.Max(mostright, tile.PlannedLeft + tile.PlannedLonDistance * tile.PlannedWidth);
                  mostbottom = Math.Min(mostbottom, tile.PlannedTop - tile.PlannedLatDistance * tile.PlannedHeight);
                  tile.Shrink = data4Zoomlevel[z].Shrink;
               }

               ShowZoomlevelInfo(tiles, data4Zoomlevel[z], true);
            }

            demconverter = new DEMDataConverter();
            if (!demconverter.ReadData(DemDataPaths, mostleft, mosttop, mostright, mostbottom, dummydataonerror)) {     // DEM-Rohdaten einlesen
               Console.Error.WriteLine("Can not read all necessary DEM's.");
               return false;
            }

         } else { // ------------------- Daten aus Textdatei einlesen

            for (int z = 0; z < data4Zoomlevel.Count; z++) {
               Data2Dim rawdata = ReadTextData(data4Zoomlevel[z].Datafile, out data4Zoomlevel[z].MinHeight, out data4Zoomlevel[z].MaxHeight);

               if (rawdata.Height > 0 && rawdata.Width > 0) {
                  if (data4Zoomlevel[z].Width == int.MinValue) {
                     data4Zoomlevel[z].Width = rawdata.Width * data4Zoomlevel[z].Latdist;
                  } else {
                     if (data4Zoomlevel[z].Latdist == int.MinValue) {
                        data4Zoomlevel[z].Latdist = data4Zoomlevel[z].Width / rawdata.Width;
                     }
                  }

                  if (data4Zoomlevel[z].Height == int.MinValue) {
                     data4Zoomlevel[z].Height = rawdata.Height * data4Zoomlevel[z].Latdist;
                  } else {
                     if (data4Zoomlevel[z].Londist == int.MinValue) {
                        data4Zoomlevel[z].Londist = data4Zoomlevel[z].Height / rawdata.Height;
                     }
                  }

                  Console.WriteLine("Textdaten " + rawdata.ToString() +
                                    ", Min. " + data4Zoomlevel[z].MinHeight.ToString() +
                                    ", Max. " + data4Zoomlevel[z].MaxHeight.ToString() +
                                    " für Zoomlevel " + z.ToString());
                  Subtile[,] tiles = BuildFilledSubtileArray(rawdata, lastcolstd, STDSUBTILESIZE);
                  tiles4zoomlevel.Add(tiles);

                  foreach (Subtile tile in tiles)
                     tile.Shrink = data4Zoomlevel[z].Shrink;

                  ShowZoomlevelInfo(tiles, data4Zoomlevel[z], false);

               } else
                  throw new Exception("No text files read.");

               rawdata.Dispose();   // Rohdaten werden nicht mehr benötigt
            }
         }

         if (DemOutFile) {   // DEM-Daten als Textdatei ausgeben
            string outfile = DemFile.Substring(0, DemFile.Length - Path.GetExtension(DemFile).Length);
            for (int z = 0; z < data4Zoomlevel.Count; z++) {
               outfile += "_zl" + (z + 1).ToString() + ".txt";
               if (!overwrite &&
                   File.Exists(outfile)) {
                  Console.Error.WriteLine("File '" + outfile + "' exist and must not overwritten.");
                  return false;
               }
               WriteHgtOutput(outfile,
                              demconverter.BuildHeightArray(ZoomlevelTableitem.Unit2Degree(data4Zoomlevel[z].Left),
                                                            ZoomlevelTableitem.Unit2Degree(data4Zoomlevel[z].Top),
                                                            ZoomlevelTableitem.Unit2Degree(data4Zoomlevel[z].Width),
                                                            ZoomlevelTableitem.Unit2Degree(data4Zoomlevel[z].Height),
                                                            ZoomlevelTableitem.Unit2Degree(data4Zoomlevel[z].Londist),
                                                            ZoomlevelTableitem.Unit2Degree(data4Zoomlevel[z].Latdist),
                                                            data4Zoomlevel[z].Shrink,
                                                            footflag,
                                                            DEM1x1.InterpolationType.standard));
            }
         }

         if (usetestencoder)
            Console.WriteLine("use testencoder");


         /*    Die interne Optimierung scheint mehr zu bringen als Multithreading!!
          *    32 Bit ist schneller als 64 Bit!
          */
         //maxthreads = 4;

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
            Console.Write("encoding {0} Subtiles ", count);

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
                        EncodeSubtilePaket(subtilepacket, 
                                           maxthreads > 1 ? calctp : null, 
                                           demconverter, 
                                           footflag, 
                                           stdintpol ? DEM1x1.InterpolationType.standard : DEM1x1.InterpolationType.bicubic_catmull_rom, 
                                           usetestencoder);
                     }
                  }
               }
            }
            if (subtilepacket.Count > 0) {
               count += subtilepacket.Count;
               EncodeSubtilePaket(subtilepacket, 
                                  calctp, 
                                  demconverter, 
                                  footflag,
                                  stdintpol ? DEM1x1.InterpolationType.standard : DEM1x1.InterpolationType.bicubic_catmull_rom,
                                  usetestencoder);
            }

            if (maxthreads > 0) {
               calctp.Wait4NotWorking();
               if (calctp.ExceptionCount > 0)
                  return false;
            }

            Console.WriteLine();
            Console.WriteLine(count.ToString() + " Subtiles encoded");

            // ----- Codierung beendet


            Head head = new Head();
            head.Footflag = footflag ? 1 : 0;
            head.Unknown1B = 0;
            head.Unknown25 = 1;
            head.Zoomlevel = (ushort)tiles4zoomlevel.Count;
            List<ZoomlevelData> Zoomlevel = CreateZoomlevel(data4Zoomlevel, tiles4zoomlevel, head);

            head.PtrZoomlevelTable = head.Length;
            for (int z = 0; z < Zoomlevel.Count; z++)
               head.PtrZoomlevelTable += Zoomlevel[z].GetTableAreaSize() + Zoomlevel[z].GetHeightDataAreaSize();

            // Jetzt sollten alle Daten gesetzt sein und die Datei kann geschrieben werden.
            using (BinaryWriter w = new BinaryWriter(File.Create(DemFile))) {
               WriteDEM(w, head, Zoomlevel);
               Console.WriteLine(string.Format(DemFile + ", {0} bytes", w.BaseStream.Length));
            }

            Console.WriteLine("runtime {0:N3}s", (DateTime.Now - starttime).TotalSeconds);

         } else {
            Console.Error.WriteLine("to less data");
            return false;
         }

         return true;
      }

      void ShowZoomlevelInfo(Subtile[,] tiles, Data4Zoomlevel zl, bool plannedvalues) {
         Console.Write("number of subtiles " + tiles.GetLength(0) + " x " + tiles.GetLength(1));
         Console.Write(" (most right column ");
         Console.Write(plannedvalues ? tiles[tiles.GetLength(0) - 1, 0].PlannedWidth.ToString() : tiles[tiles.GetLength(0) - 1, 0].Width.ToString());
         Console.Write(" width, undermost row ");
         Console.Write(plannedvalues ? tiles[0, tiles.GetLength(1) - 1].PlannedHeight.ToString() : tiles[0, tiles.GetLength(1) - 1].Height.ToString());
         Console.WriteLine(" height)");
         Console.WriteLine(string.Format("left {0}°, top {1}°, width {2}°, height {3}°",
                                             ZoomlevelTableitem.Unit2Degree(zl.Left),
                                             ZoomlevelTableitem.Unit2Degree(zl.Top),
                                             ZoomlevelTableitem.Unit2Degree(zl.Width),
                                             ZoomlevelTableitem.Unit2Degree(zl.Height)));
         Console.WriteLine(string.Format("pointsize {0}° x {1}°",
                                             ZoomlevelTableitem.Unit2Degree(zl.Londist),
                                             ZoomlevelTableitem.Unit2Degree(zl.Latdist)));
         Console.WriteLine(string.Format("shrink {0}", zl.Shrink));
      }

      /// <summary>
      /// Encodierung einer Liste von <see cref="Subtile"/>
      /// </summary>
      /// <param name="subtilepacket"></param>
      /// <param name="tp">wenn null, wird ohne Multithreading encodiert</param>
      /// <param name="hgtconv">wenn null, müssen die Höhendaten in den <see cref="Subtile"/> schon vorhanden sein</param>
      /// <param name="footflag">wenn true, dann Höhendaten in Fuß, sonst Meter</param>
      /// <param name="intpol"></param>
      /// <param name="usetestencoder"></param>
      void EncodeSubtilePaket(List<Subtile> subtilepacket, CalculationThreadPoolExt tp, DEMDataConverter hgtconv, bool footflag, DEM1x1.InterpolationType intpol, bool usetestencoder) {
         if (tp == null) {    // Paket direkt encodieren
            for (int i = 0; i < subtilepacket.Count; i++) {
               if (hgtconv != null) {
                  Data2Dim dat = new Data2Dim(hgtconv.BuildHeightArray(subtilepacket[i].PlannedLeft,
                                                                       subtilepacket[i].PlannedTop,
                                                                       subtilepacket[i].PlannedLonDistance,
                                                                       subtilepacket[i].PlannedLatDistance,
                                                                       subtilepacket[i].Width,
                                                                       subtilepacket[i].Height,
                                                                       subtilepacket[i].Shrink,
                                                                       footflag,
                                                                       intpol));
                  subtilepacket[i].Encoding(usetestencoder, dat);  // HGT-Daten müssen noch geliefert werden
                  dat.Dispose();
               } else
                  subtilepacket[i].Encoding(usetestencoder);     // Daten aus Textdatei sind schon vorhanden
            }
            Console.Write(".");
         } else {             // Paket im eigenen Thread encodieren

            tp.Start(new CalculationParam(subtilepacket, hgtconv, footflag, intpol, usetestencoder));

         }
         subtilepacket.Clear();
      }

      static void ThreadMsg(object para) {
         if (para != null) {
            if (para is string)
               Console.Error.Write(para as string);
         }
      }

      // Threading-Problem ev. wegen: http://simplygenius.net/Article/FalseSharing

      class CalculationParam {
         public List<Subtile> subtilelist;
         public DEMDataConverter dataconverter;
         public bool footflag;
         public DEM1x1.InterpolationType intpol;
         public bool usetestencoder;

         public CalculationParam(List<Subtile> subtilelist, DEMDataConverter dataconverter, bool footflag, DEM1x1.InterpolationType intpol, bool usetestencoder) {
            this.subtilelist = new List<Subtile>(subtilelist);    // Kopie der Liste übernehmen, weil das Original gleich geleert wird
            this.dataconverter = dataconverter;
            this.footflag = footflag;
            this.intpol = intpol;
            this.usetestencoder = usetestencoder;
         }
      }

      class CalculationThreadPoolExt : ThreadPoolExt {

         public CalculationThreadPoolExt(WaitCallback msgfunc) : base(msgfunc, null) { }

         protected override void DoWork(object para) {
            if (ExceptionCount > 0)
               return;

            try {
               if (para is CalculationParam) {
                  CalculationParam cp = para as CalculationParam;
                  for (int i = 0; i < cp.subtilelist.Count; i++) {
                     Subtile st = cp.subtilelist[i];
                     if (cp.dataconverter == null)             // kein DEMDataConverter
                        st.Encoding(cp.usetestencoder);        // Daten aus Textdatei sind schon vorhanden
                     else {
                        Data2Dim dat = new Data2Dim(cp.dataconverter.BuildHeightArray(
                                                                         st.PlannedLeft,
                                                                         st.PlannedTop,
                                                                         st.PlannedLonDistance,
                                                                         st.PlannedLatDistance,
                                                                         st.PlannedWidth,
                                                                         st.PlannedHeight,
                                                                         st.Shrink,
                                                                         cp.footflag,
                                                                         cp.intpol));
                        st.Encoding(cp.usetestencoder, dat);    // HGT-Daten müssen noch geliefert werden
                        dat.Dispose();
                     }

                  }
                  msgfunc?.Invoke(".");
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
                     throw new Exception("Number of values (" + sData.Length.ToString() + ") in line " + (height + 1).ToString() + " are not the same with the number in 1. line (" + width.ToString() + ").");
               }
               height++;

               for (int i = 0; i < sData.Length; i++) {
                  int val = Convert.ToInt32(sData[i].Trim());
                  if (val == DEM1x1.NOVALUE ||
                      val > Subtile.UNDEF4ENCODER)    // 2 verschiedene Varianten für "nodata"
                     val = Subtile.UNDEF4ENCODER;

                  minheight = Math.Min(minheight, val);
                  if (val == Subtile.UNDEF4ENCODER)
                     maxheight = Math.Max(maxheight, val);
                  heights.Add(val);
               }
            }
         } catch (Exception e) {
            Console.Error.WriteLine("Data in file '" + txtfile + "' couldn't read:");
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
            Console.WriteLine("write HGT data to '" + hgtoutput + "' ...");

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
         // Anzahl der nötigen Punkte in jede Richtung
         int ptx = zl.Width / zl.Londist + 1;
         int pty = zl.Height / zl.Latdist + 1;

         if (lastcolstd) {    // dann ptx auf Vielfache von tilesize vergrößern
            int remainder = ptx % subtilesize;
            if (remainder > 0)
               ptx += subtilesize - remainder;
         }

         int subtilesx = ptx / subtilesize;     // Anzahl der Subtiles nebeneinander
         if (subtilesx > 0) {
            ptx -= subtilesx * subtilesize;        // restl. Punkte (0..63) für eine weitere Subtile-Spalte
            ptx += subtilesize;                    // 64..127
         } else
            subtilesx = 1; // bringt ev. kein sinnvolles Ergebnis, wenn nur 1 subtile-Spalte ex. die schmaler als 64 ist (?)

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

         int subtiledeltalon = zl.Londist * subtilesize;
         int subtiledeltalat = zl.Latdist * subtilesize;
         for (int x = 0, left = zl.Left; x < subtiles.GetLength(0); x++, left += subtiledeltalon) {
            for (int y = 0, top = zl.Top; y < subtiles.GetLength(1); y++, top -= subtiledeltalat) {
               int loncount = x == subtiles.GetLength(0) - 1 ? ptx : subtilesize;
               int latcount = y == subtiles.GetLength(1) - 1 ? pty : subtilesize;
               subtiles[x, y] = new Subtile(ZoomlevelTableitem.Unit2Degree(left),
                                            ZoomlevelTableitem.Unit2Degree(top),
                                            ZoomlevelTableitem.Unit2Degree(zl.Londist),
                                            ZoomlevelTableitem.Unit2Degree(zl.Latdist),
                                            loncount,
                                            latcount);
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

         int subtilesy = pty / subtilesize;     // Anzahl der vollständigen Subtiles untereinander
         pty -= subtilesy * subtilesize;
         if (pty > 0)
            subtilesy++;
         else
            pty = 64;

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
      void FitTableSize(ZoomlevelData zoomleveldata) {
         int minbaseheight = int.MaxValue;
         int maxbaseheight = int.MinValue;
         int maxheight = int.MinValue;
         int maxdiff = int.MinValue;
         uint maxoffset = uint.MinValue;

         bool bOnylType0 = true; // Werden nur Subtiles mit Typ 0 verwendet?
         uint offset = 0;
         for (int i = 0; i < zoomleveldata.Subtiles.Count; i++) {
            int baseheight = zoomleveldata.Subtiles[i].Tableitem.Baseheight;
            minbaseheight = Math.Min(minbaseheight, baseheight);
            maxbaseheight = Math.Max(maxbaseheight, baseheight);
            maxheight = Math.Max(maxheight, baseheight + zoomleveldata.Subtiles[i].Tableitem.Diff);
            maxdiff = Math.Max(maxdiff, zoomleveldata.Subtiles[i].Tableitem.Diff);
            zoomleveldata.Subtiles[i].Tableitem.Offset = zoomleveldata.Subtiles[i].DataLength > 0 ? offset : 0;
            offset += (uint)zoomleveldata.Subtiles[i].DataLength;
            maxoffset = Math.Max(maxoffset, zoomleveldata.Subtiles[i].Tableitem.Offset); // größter zu speichernder Zahlenwert
            if (zoomleveldata.Subtiles[i].Tableitem.Type != 0)
               bOnylType0 = false;
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

         if (maxdiff < 255)
            zoomleveldata.Tableitem.Structure_DiffSize = 1;
         else
            zoomleveldata.Tableitem.Structure_DiffSize = 2;

         zoomleveldata.Tableitem.Structure_CodingtypeSize = bOnylType0 ? 0 : 1;
      }

      /// <summary>
      /// Zoomlevel-Definitionen erzeugen
      /// </summary>
      /// <param name="tiles">Subtiles je Zoomlevel</param>
      /// <param name="head">Header der erzeugt wird</param>
      /// <returns></returns>
      List<ZoomlevelData> CreateZoomlevel(List<Data4Zoomlevel> data4Zoomlevel, List<Subtile[,]> tiles, Head head) {
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

            zl[z].Tableitem.Maplevel = (byte)z;
            zl[z].Tableitem.PointsHoriz = STDSUBTILESIZE;
            zl[z].Tableitem.PointsVert = STDSUBTILESIZE;

            zl[z].Tableitem.IntPointDistanceHoriz = data4Zoomlevel[z].Londist;
            zl[z].Tableitem.IntPointDistanceVert = data4Zoomlevel[z].Latdist;

            zl[z].Tableitem.IntWest = data4Zoomlevel[z].Left;
            zl[z].Tableitem.IntNorth = data4Zoomlevel[z].Top;

            zl[z].Tableitem.MaxIdxHoriz = tiles[z].GetLength(0) - 1;    // Oder Anzahl der waagerechten 64er Kacheln?
            zl[z].Tableitem.MaxIdxVert = tiles[z].GetLength(1) - 1;
            zl[z].Tableitem.LastColWidth = tiles[z][tiles[z].GetLength(0) - 1, 0].Width - 1;
            zl[z].Tableitem.LastRowHeight = tiles[z][0, tiles[z].GetLength(1) - 1].Height - 1;

            zl[z].Tableitem.Shrink = (short)((tiles[z][0, 0].Shrink - 1) / 2);      // 2*k+1 = shrink

         }

         // Tabellengrößen festlegen und Pointer ermitteln
         for (int z = 0; z < tiles.Count; z++) {
            FitTableSize(zl[z]);

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
