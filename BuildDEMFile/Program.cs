using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BuildDEMFile {

   public class Data4DEMJob {

      /// <summary>
      /// Pfade zu den DEM-Dateien
      /// </summary>
      public List<string> DemDataPaths;

      /// <summary>
      /// einzelne DEM-Datendateien (1 je Zoomlevel)
      /// </summary>
      public List<string> DemDataFile;

      /// <summary>
      /// zu erzeugende Garmin-DEM-Datei
      /// </summary>
      public string DemFile;

      /// <summary>
      /// Ausgabe der DEM-Daten in eine Textdatei
      /// </summary>
      public bool DemOutFile;

      /// <summary>
      /// Begrenzung des Gebiets
      /// </summary>
      public double Left, Top, Width, Height;

      /// <summary>
      /// Punktabstände für die einzelnen Zoomlevel
      /// </summary>
      public List<double> LonDistance, LatDistance;


      public Data4DEMJob() {
         DemDataPaths = new List<string>();
         DemDataFile = new List<string>();
         DemOutFile = false;
         DemFile = "";
         Left = Top = Width = Height = 0;
         LonDistance = new List<double>();
         LatDistance = new List<double>();
      }

      public Data4DEMJob(Data4DEMJob dat) {
         DemDataPaths = new List<string>(dat.DemDataPaths);
         DemDataFile = new List<string>(dat.DemDataFile);
         DemOutFile = dat.DemOutFile;
         DemFile = dat.DemFile;
         Left = dat.Left;
         Top = dat.Top;
         Width = dat.Width;
         Height = dat.Height;
         LonDistance = new List<double>(dat.LonDistance);
         LatDistance = new List<double>(dat.LatDistance);
      }

      public bool HasDemDataPaths { get { return DemDataPaths.Count > 0; } }

      public bool HasDemDataFile { get { return !HasDemDataPaths && DemDataFile.Count > 0; } }

      public bool HasDemFile { get { return !string.IsNullOrEmpty(DemFile); } }

      public void EqualizeLatLon() {
         int count = Math.Max(LonDistance.Count, LatDistance.Count);

         for (int i = LatDistance.Count; i < count; i++)
            LatDistance.Add(LonDistance[i]);

         for (int i = LonDistance.Count; i < count; i++)
            LonDistance.Add(LatDistance[i]);

      }

      public void SetExtendFromTRE(string trefile) {
         double west, north, east, south;
         if (!TREFileHelper.ReadEdges(trefile, out west, out north, out east, out south))
            throw new Exception("Couldn't read data from TRE file.");

         Console.WriteLine("data from TRE file '" + trefile + "'");
         Left = west;
         Top = north;
         Width = east - west;
         Height = north - south;
      }

   }


   /// <summary>
   /// erzeugt eine einzelne Garmin-DEM-Datei
   /// </summary>
   class Program {
      static void Main(string[] args) {

         Assembly a = Assembly.GetExecutingAssembly();
         string progname = ((AssemblyProductAttribute)(Attribute.GetCustomAttribute(a, typeof(AssemblyProductAttribute)))).Product + ", Version vom " +
                           ((AssemblyInformationalVersionAttribute)(Attribute.GetCustomAttribute(a, typeof(AssemblyInformationalVersionAttribute)))).InformationalVersion + ", " +
                           ((AssemblyCopyrightAttribute)(Attribute.GetCustomAttribute(a, typeof(AssemblyCopyrightAttribute)))).Copyright;
         Console.Error.WriteLine(progname);

         try {
            Options opt = new Options();
            opt.Evaluate(args);

            List<Data4DEMJob> DEMJobList = new List<Data4DEMJob>();

            /* 
             * PixelWidth, PixelHeight nötig
             * wenn gmap dann
             *    DemPath, OverviewPixelWidth, OverviewPixelHeight nötig
             * sonst 
             *    DEMFilename nötig
             *    wenn TREFilename, dann
             *       DemPath nötig
             *    sonst
             *       TRELeft, TRETop nötig
             *       wenn !DemPath dann
             *          DataFilename nötig
             *       sonst
             *          DEMWidth, DEMHeight nötig
             *    DemDataOutput opt
             */

            Data4DEMJob demjobdat = new Data4DEMJob();
            demjobdat.LatDistance = new List<double>(opt.PixelWidth);
            demjobdat.LonDistance = new List<double>(opt.PixelHeight);
            demjobdat.EqualizeLatLon();

            if (!string.IsNullOrEmpty(opt.GmapPath)) {
               if (opt.DemPath.Count < 0)
                  throw new Exception("Need DEM file with data or path to DEM's.");
               demjobdat.DemDataPaths = new List<string>(opt.DemPath);

               foreach (string trefile in Directory.EnumerateFiles(opt.GmapPath, "*.TRE", SearchOption.AllDirectories)) {
                  Data4DEMJob demjobdat2 = new Data4DEMJob(demjobdat);
                  demjobdat2.SetExtendFromTRE(trefile);
                  DEMJobList.Add(demjobdat2);
               }
               // Data4DEMJob suchen, der alle anderen einschließt -> Overviewmap
               double MinLeft = double.MaxValue, MaxTop = double.MinValue, MaxRight = double.MinValue, MinBottom = double.MaxValue;
               for (int i = 0; i < DEMJobList.Count; i++) {
                  MinLeft = Math.Min(MinLeft, DEMJobList[i].Left);
                  MinBottom = Math.Min(MinBottom, DEMJobList[i].Top - DEMJobList[i].Height);
                  MaxTop = Math.Min(MaxTop, DEMJobList[i].Top);
                  MaxRight = Math.Min(MinLeft, DEMJobList[i].Left + DEMJobList[i].Width);
               }
               int ov = -1;
               for (int i = 0; i < DEMJobList.Count; i++)
                  if (MinLeft == DEMJobList[i].Left &&
                      MinBottom == DEMJobList[i].Top - DEMJobList[i].Height &&
                      MaxTop == DEMJobList[i].Top &&
                      MaxRight == DEMJobList[i].Left + DEMJobList[i].Width) {
                     ov = i;
                     break;
                  }
               if (0 <= ov && ov < DEMJobList.Count) {
                  demjobdat.LatDistance = new List<double>(opt.OverviewPixelWidth);
                  demjobdat.LonDistance = new List<double>(opt.OverviewPixelHeight);
                  demjobdat.EqualizeLatLon();
               }

            } else {

               // einzelne DEM-Datei erzeugen

               if (string.IsNullOrEmpty(opt.DEMFilename))
                  throw new Exception("Need name for DEM file.");
               demjobdat.DemFile = opt.DEMFilename;

               if (File.Exists(opt.DEMFilename) && !opt.OutputOverwrite)
                  throw new Exception("File '" + opt.DEMFilename + " exist.");

               if (!string.IsNullOrEmpty(opt.TREFilename)) {
                  if (opt.DemPath.Count < 0)
                     throw new Exception("Need path to DEM's.");
                  demjobdat.DemDataPaths = new List<string>(opt.DemPath);
                  demjobdat.SetExtendFromTRE(opt.TREFilename);
               } else {
                  if (double.IsNaN(opt.TRELeft))
                     throw new Exception("Need Left for DEM file.");
                  demjobdat.Left = opt.TRELeft;

                  if (double.IsNaN(opt.TRETop))
                     throw new Exception("Need Top for DEM file.");
                  demjobdat.Top = opt.TRETop;

                  if (opt.DemPath.Count > 0) {
                     if (double.IsNaN(opt.DEMWidth))
                        throw new Exception("Need Width for DEM file.");
                     demjobdat.Width = opt.DEMWidth;

                     if (double.IsNaN(opt.DEMHeight))
                        throw new Exception("Need Height for DEM file.");
                     demjobdat.Height = opt.DEMHeight;

                     demjobdat.DemDataPaths = new List<string>(opt.DemPath);
                  } else {    // DEM-daten aus Textdatei/en holen
                     if (opt.DataFilename.Count == 0)
                        throw new Exception("Need text file with DEM data.");
                     demjobdat.DemDataFile = new List<string>(opt.DataFilename);

                     if (!double.IsNaN(opt.DEMWidth))
                        demjobdat.Width = opt.DEMWidth;

                     if (!double.IsNaN(opt.DEMHeight))
                        demjobdat.Height = opt.DEMHeight;

                     if (demjobdat.LonDistance.Count != demjobdat.DemDataFile.Count) // Anzahl der Zoomlevel
                        throw new Exception("To less point distances.");
                  }
               }
               DEMJobList.Add(demjobdat);
            }

            FileBuilder fb = new FileBuilder();
            foreach (Data4DEMJob job in DEMJobList) 
               fb.Create(job.DemDataPaths,
                        job.DemDataFile,
                        job.DemFile,
                        job.Left,
                        job.Top,
                        job.Width,
                        job.Height,
                        job.LonDistance,
                        job.LatDistance,
                        opt.DemDataOutput,
                        opt.DataInFoot,
                        opt.LastColStd,
                        opt.OutputOverwrite,
                        opt.UseDummyData,
                        opt.UseTestEncoder,
                        opt.Multithread);

         } catch (Exception ex) {
            Console.Error.WriteLine(ex.Message);
         }
      }
   }
}
