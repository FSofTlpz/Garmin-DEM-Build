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
      public List<int> Left, Top, Width, Height;

      /// <summary>
      /// Punktabstände für die einzelnen Zoomlevel
      /// </summary>
      public List<int> LonDistance, LatDistance;

      /// <summary>
      /// Verkleinerungsfaktoren
      /// </summary>
      public List<int> Shrink;

      /// <summary>
      /// Begrenzung wurde aus der TRE-Datei geholt
      /// </summary>
      public bool ExtentIsFromTRE;


      public Data4DEMJob() {
         DemDataPaths = new List<string>();
         DemDataFile = new List<string>();
         DemOutFile = false;
         DemFile = "";
         Left = new List<int>();
         Top = new List<int>();
         Width = new List<int>();
         Height = new List<int>();
         LonDistance = new List<int>();
         LatDistance = new List<int>();
         Shrink = new List<int>();
         ExtentIsFromTRE = false;
      }

      public Data4DEMJob(Data4DEMJob dat) {
         DemDataPaths = new List<string>(dat.DemDataPaths);
         DemDataFile = new List<string>(dat.DemDataFile);
         DemOutFile = dat.DemOutFile;
         DemFile = dat.DemFile;
         Left = new List<int>(dat.Left);
         Top = new List<int>(dat.Top);
         Width = new List<int>(dat.Width);
         Height = new List<int>(dat.Height);
         LonDistance = new List<int>(dat.LonDistance);
         LatDistance = new List<int>(dat.LatDistance);
         Shrink = new List<int>(dat.Shrink);
      }

      public bool HasDemDataPaths { get { return DemDataPaths.Count > 0; } }

      public bool HasDemDataFile { get { return !HasDemDataPaths && DemDataFile.Count > 0; } }

      public bool HasDemFile { get { return !string.IsNullOrEmpty(DemFile); } }

      public void EqualizeLatLonExtent() {
         int count = Math.Max(LonDistance.Count, LatDistance.Count);

         for (int i = LatDistance.Count; i < count; i++)
            LatDistance.Add(LonDistance[i]);

         for (int i = LonDistance.Count; i < count; i++)
            LonDistance.Add(LatDistance[i]);

         while (Shrink.Count < count)
            Shrink.Add(Shrink.Count > 0 ? Shrink[0] : 1);

         while (Left.Count < count)
            Left.Add(Left.Count > 0 ? Left[0] : int.MinValue);

         while (Top.Count < count)
            Top.Add(Top.Count > 0 ? Top[0] : int.MinValue);

         while (Width.Count < count)
            Width.Add(Width.Count > 0 ? Width[0] : int.MinValue);

         while (Height.Count < count)
            Height.Add(Height.Count > 0 ? Height[0] : int.MinValue);

      }

      public void SetExtentFromTRE(string trefile) {
         double west, north, east, south;
         int iwest, inorth, ieast, isouth;
         if (!TREFileHelper.ReadEdges(trefile,
                                      out west, out north, out east, out south,
                                      out iwest, out inorth, out ieast, out isouth))
            throw new Exception("Couldn't read data from TRE file.");

         Console.WriteLine("data from TRE file '" + trefile + "'");
         Left.Add(iwest << 8);
         Top.Add(inorth << 8);
         Width.Add((ieast - iwest) << 8);
         Height.Add((inorth - isouth) << 8);
         ExtentIsFromTRE = true;
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

            FileBuilder fb = new FileBuilder();
            foreach (Data4DEMJob job in PrepareInput(opt)) {
               fb.Create(job.DemDataPaths,
                         job.DemDataFile,
                         job.DemFile,
                         job.Left,
                         job.Top,
                         job.Width,
                         job.Height,
                         job.LonDistance,
                         job.LatDistance,
                         job.Shrink,
                         opt.DemDataOutput,
                         opt.DataInFoot,
                         opt.LastColStd,
                         opt.OutputOverwrite,
                         opt.UseDummyData,
                         opt.StdInterpolation,
                         opt.UseTestEncoder,
                         opt.Multithread);
            }

         } catch (Exception ex) {
            Console.Error.WriteLine(ex.Message);
         }
      }

      static void FillLatLonList(Options opt, bool ov, List<int> lat, List<int> lon) {
         if (!ov) {
            if (opt.PixelDistance.Count > 0) {        // Vorrang vor PixelHeight und PixelWidth
               foreach (var item in opt.PixelDistance) {
                  lat.Add(item);
                  lon.Add(item);
               }
            } else {
               foreach (var item in opt.PixelHeight)
                  lat.Add(ZoomlevelTableitem.Degree2Unit(item));
               foreach (var item in opt.PixelWidth)
                  lon.Add(ZoomlevelTableitem.Degree2Unit(item));
            }
         } else {
            if (opt.OverviewPixelDistance.Count > 0) {        // Vorrang vor PixelHeight und PixelWidth
               foreach (var item in opt.OverviewPixelDistance) {
                  lat.Add(item);
                  lon.Add(item);
               }
            } else {
               foreach (var item in opt.OverviewPixelHeight)
                  lat.Add(ZoomlevelTableitem.Degree2Unit(item));
               foreach (var item in opt.OverviewPixelWidth)
                  lon.Add(ZoomlevelTableitem.Degree2Unit(item));
            }
         }
      }

      static List<Data4DEMJob> PrepareInput(Options opt) {
         List<Data4DEMJob> lst = new List<Data4DEMJob>();

         /* 
          * a) Karte aus Textdaten (Kennung DataFilename)
          *    DEMFilename immer nötig
          *    DataFilename immer nötig
          *    TRELeft, TRETop, PixelDistance/PixelWidth/PixelHeight
          *       oder
          *    TRELeft, TRETop, DEMWidth, DEMHeight
          *       oder
          *    TREFilename
          * 
          * b) Karte aus DEM-Daten
          *    DEMFilename immer nötig
          *    DemPath immer nötig
          *    PixelDistance/PixelWidth/PixelHeight immer nötig
          *    TRELeft, TRETop, DEMWidth, DEMHeight
          *       oder
          *    TREFilename
          *    optional
          *       OverviewShrink
          *       UseDummyData
          *       
          * c) GmapKarten (Kennung GmapPath)
          *    GmapPath immer nötig
          *    DemPath immer nötig
          *    PixelDistance/PixelWidth/PixelHeight immer nötig
          *    optional
          *       OverviewPixelDistance/OverviewPixelWidth/OverviewPixelHeight
          *       OverviewShrink
          *       UseDummyData
          * 
          * optional
          *    DemDataOutput
          *    DataInFoot
          *    LastColStd
          *    OutputOverwrite
          *    UseTestEncoder
          *    Multithread
          * 
          * 
          */

         Data4DEMJob job = new Data4DEMJob();
         if (opt.DataFilename.Count > 0) {   // einzelne DEM-Datei aus Textdateien erzeugen

            job.DemDataFile = new List<string>(opt.DataFilename);

            if (opt.DEMFilename == "")
               throw new Exception("Need name for DEM file.");
            job.DemFile = opt.DEMFilename;

            if (opt.TREFilename != "")
               job.SetExtentFromTRE(opt.TREFilename);
            else {
               if (double.IsNaN(opt.TRELeft))
                  throw new Exception("Need Left for DEM file.");
               job.Left.Add(ZoomlevelTableitem.Degree2Unit(opt.TRELeft));

               if (double.IsNaN(opt.TRETop))
                  throw new Exception("Need Top for DEM file.");
               job.Top.Add(ZoomlevelTableitem.Degree2Unit(opt.TRETop));

               if (!double.IsNaN(opt.DEMWidth) &&
                   !double.IsNaN(opt.DEMHeight)) {
                  job.Width.Add(ZoomlevelTableitem.Degree2Unit(opt.DEMWidth));
                  job.Height.Add(ZoomlevelTableitem.Degree2Unit(opt.DEMHeight));
                  job.LatDistance.Add(int.MinValue);
                  job.LonDistance.Add(int.MinValue);
               } else {
                  // liest nur die "normalen" oder nur die OV-Werte ein
                  FillLatLonList(opt, false, job.LatDistance, job.LonDistance);
                  if (job.LatDistance.Count == 0)
                     FillLatLonList(opt, true, job.LatDistance, job.LonDistance);
                  if (job.LatDistance.Count == 0)
                     throw new Exception("Need Pixeldistance.");

                  // Shrink gilt beim Test immer
                  foreach (var item in opt.OverviewShrink)
                     job.Shrink.Add(item);
               }
            }

            job.EqualizeLatLonExtent();

            lst.Add(job);

         } else if (opt.GmapPath == "") {    // einzelne DEM-Datei aus DEM-Daten erzeugen

            if (opt.DEMFilename == "")
               throw new Exception("Need name for DEM file.");
            job.DemFile = opt.DEMFilename;

            if (opt.DemPath.Count < 0)
               throw new Exception("Need path to DEM's.");
            job.DemDataPaths = new List<string>(opt.DemPath);

            // liest nur die "normalen" oder nur die OV-Werte ein
            FillLatLonList(opt, false, job.LatDistance, job.LonDistance);
            if (job.LatDistance.Count == 0)
               FillLatLonList(opt, true, job.LatDistance, job.LonDistance);
            if (job.LatDistance.Count == 0)
               throw new Exception("Need Pixeldistance.");

            // Shrink gilt immer
            foreach (var item in opt.OverviewShrink)
               job.Shrink.Add(item);

            if (opt.TREFilename != "")
               job.SetExtentFromTRE(opt.TREFilename);
            else {
               if (double.IsNaN(opt.TRELeft))
                  throw new Exception("Need Left for DEM file.");
               job.Left.Add(ZoomlevelTableitem.Degree2Unit(opt.TRELeft));

               if (double.IsNaN(opt.TRETop))
                  throw new Exception("Need Top for DEM file.");
               job.Top.Add(ZoomlevelTableitem.Degree2Unit(opt.TRETop));

               if (double.IsNaN(opt.DEMWidth))
                  throw new Exception("Need Width for DEM file.");
               job.Width.Add(ZoomlevelTableitem.Degree2Unit(opt.DEMWidth));

               if (double.IsNaN(opt.DEMHeight))
                  throw new Exception("Need Height for DEM file.");
               job.Height.Add(ZoomlevelTableitem.Degree2Unit(opt.DEMHeight));
            }

            for (int i = 0; i < opt.OverviewShrink.Count; i++)
               job.Shrink.Add(opt.OverviewShrink[i]);

            job.EqualizeLatLonExtent();

            lst.Add(job);

         } else {                            // DEM-Dateien für Gmap erzeugen

            if (opt.DemPath.Count < 0)
               throw new Exception("Need path to DEM's.");
            job.DemDataPaths = new List<string>(opt.DemPath);

            FillLatLonList(opt, false, job.LatDistance, job.LonDistance);

            foreach (string trefile in Directory.EnumerateFiles(opt.GmapPath, "*.TRE", SearchOption.AllDirectories)) {
               Data4DEMJob job2 = new Data4DEMJob(job);
               job2.SetExtentFromTRE(trefile);
               job2.DemFile = trefile.Substring(0, trefile.Length - 3) + "DEM";     // Extension in Großbuchstaben nötig!
               job2.EqualizeLatLonExtent();
               lst.Add(job2);
            }

            // Data4DEMJob suchen, der alle anderen einschließt -> Overviewmap
            int MinLeft = int.MaxValue, MaxTop = int.MinValue, MaxRight = int.MinValue, MinBottom = int.MaxValue;
            for (int i = 0; i < lst.Count; i++) {
               for (int j = 0; j < lst[i].Left.Count; j++)
                  MinLeft = Math.Min(MinLeft, lst[i].Left[j]);
               for (int j = 0; j < lst[i].Top.Count; j++)
                  MinBottom = Math.Min(MinBottom, lst[i].Top[j] - lst[i].Height[j]);
               for (int j = 0; j < lst[i].Top.Count; j++)
                  MaxTop = Math.Max(MaxTop, lst[i].Top[0]);
               for (int j = 0; j < lst[i].Left.Count; j++)
                  MaxRight = Math.Max(MinLeft, lst[i].Left[j] + lst[i].Width[j]);
            }
            int ov = -1;
            for (int i = 0; i < lst.Count; i++)
               if (MinLeft == lst[i].Left[0] &&
                   MinBottom == lst[i].Top[0] - lst[i].Height[0] &&
                   MaxTop == lst[i].Top[0] &&
                   MaxRight == lst[i].Left[0] + lst[i].Width[0]) {
                  ov = i;
                  break;
               }
            if (0 <= ov && ov < lst.Count) {
               job = lst[ov];
               job.LatDistance.Clear();
               job.LonDistance.Clear();
               FillLatLonList(opt, true, job.LatDistance, job.LonDistance);

               for (int i = 0; i < opt.OverviewShrink.Count; i++) // nur, wenn OV-Karte
                  job.Shrink.Add(opt.OverviewShrink[i]);

            }

         }


         foreach (Data4DEMJob demjob in lst)
            if (demjob.ExtentIsFromTRE) {
               if (!opt.NoSnapLeftTop)
                  // Korrektur des Bezugspunktes auf Vielfache des Punktabstandes
                  for (int i = 0; i < demjob.LonDistance.Count; i++) {
                     int left = (int)(Math.Floor((double)demjob.Left[i] / demjob.LonDistance[i]) * demjob.LonDistance[i]);
                     int top = (int)(Math.Ceiling((double)demjob.Top[i] / demjob.LatDistance[i]) * demjob.LatDistance[i]);
                     demjob.Width[i] += demjob.Left[i] - left;
                     demjob.Left[i] = left;
                     demjob.Height[i] += top - demjob.Top[i];
                     demjob.Top[i] = top;
                  }
            }

         return lst;
      }

   }
}
