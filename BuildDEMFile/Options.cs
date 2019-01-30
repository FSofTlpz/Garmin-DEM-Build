using System;
using System.Collections.Generic;

namespace BuildDEMFile {

   /// <summary>
   /// Optionen und Argumente werden zweckmäßigerweise in eine (programmabhängige) Klasse gekapselt.
   /// Erzeugen des Objektes und Evaluate() sollten in einem try-catch-Block erfolgen.
   /// </summary>
   public class Options {

      // alle Optionen sind i.A. 'read-only'

      /// <summary>
      /// Name der zu erzeugenden Datei
      /// </summary>
      public string DEMFilename { get; private set; }

      /// <summary>
      /// Pfade zu den DEM-Dateien
      /// </summary>
      public List<string> DemPath { get; private set; }

      /// <summary>
      /// verwendet Dummydaten wenn keine HGT-Daten ex.
      /// </summary>
      public bool UseDummyData { get; private set; }

      /// <summary>
      /// Pfad zu einer GMPA-Karte
      /// </summary>
      public string GmapPath { get; private set; }

      /// <summary>
      /// falls DEM-Daten gelesen werden, können sie in eine Textdatei ausgegeben werden
      /// </summary>
      public bool DemDataOutput { get; private set; }

      /// <summary>
      /// Name der Textdatei mit den Daten
      /// </summary>
      public List<string> DataFilename { get; private set; }

      /// <summary>
      /// Datenangaben in Fuß oder Meter
      /// </summary>
      public bool DataInFoot { get; private set; }

      /// <summary>
      /// linker Rand der TRE-Datei
      /// </summary>
      public double TRELeft { get; set; }

      /// <summary>
      /// rechter Rand der TRE-Datei
      /// </summary>
      public double TRETop { get; set; }

      /// <summary>
      /// Name der TRE-Datei (nur zum ermitteln des Randes links und oben)
      /// </summary>
      public string TREFilename { get; private set; }

      /// <summary>
      /// Breite eines DEM-Pixels
      /// </summary>
      public List<double> PixelWidth { get; private set; }

      /// <summary>
      /// Höhe eines DEM-Pixels
      /// </summary>
      public List<double> PixelHeight { get; private set; }

      /// <summary>
      /// Pixelabstand
      /// </summary>
      public List<int> PixelDistance { get; private set; }

      /// <summary>
      /// Breite eines DEM-Pixels für die Overview-Karte
      /// </summary>
      public List<double> OverviewPixelWidth { get; private set; }

      /// <summary>
      /// Höhe eines DEM-Pixels für die Overview-Karte
      /// </summary>
      public List<double> OverviewPixelHeight { get; private set; }

      /// <summary>
      /// Pixelabstand für die Overview-Karte
      /// </summary>
      public List<int> OverviewPixelDistance { get; private set; }

      /// <summary>
      /// ungerade Verkleinerungsfaktoren
      /// </summary>
      public List<int> OverviewShrink { get; private set; }

      /// <summary>
      /// Breite des DEM-Bereiches
      /// </summary>
      public double DEMWidth { get; set; }

      /// <summary>
      /// Höhe des DEM-Bereiches
      /// </summary>
      public double DEMHeight { get; set; }

      /// <summary>
      /// letzte Kachelspalte auf jeden Fall Std.breite
      /// </summary>
      public bool LastColStd { get; private set; }

      /// <summary>
      /// Ausgabeziel ev. überschreiben
      /// </summary>
      public bool OutputOverwrite { get; private set; }

      /// <summary>
      /// mit oder ohne "Einrasten" der DEM-Ecke links-oben
      /// </summary>
      public bool NoSnapLeftTop { get; private set; }

      /// <summary>
      /// Standard oder Bikubisch
      /// </summary>
      public bool StdInterpolation { get; private set; }

      /// <summary>
      /// den ursprünglichen Testencoder verwenden (langsam!)
      /// </summary>
      public bool UseTestEncoder { get; private set; }

      /// <summary>
      /// Berechnung multithreaded
      /// </summary>
      public int Multithread { get; private set; }


      FSoftUtils.CmdlineOptions cmd;

      enum MyOptions {
         DEMFilename,
         DemPath,
         GmapPath,
         TREFilename,
         PixelDistance,
         PixelWidth,
         PixelHeight,
         OverviewPixelDistance,
         OverviewPixelWidth,
         OverviewPixelHeight,
         OverviewShrink,
         UseDummyData,
         StdInterpolation,
         OutputOverwrite,
         DataInFoot,

         DemDataOutput,
         DataFilename,
         TRELeft,
         TRETop,
         DEMWidth,
         DEMHeight,
         LastColStd,
         NoSnapLeftTop,
         UseTestEncoder,

         Multithread,

         Help,
      }

      public Options() {
         Init();
         cmd = new FSoftUtils.CmdlineOptions();
         // Definition der Optionen
         cmd.DefineOption((int)MyOptions.DEMFilename, "dem", "d", "name of the new DEM-file", FSoftUtils.CmdlineOptions.OptionArgumentType.String);
         cmd.DefineOption((int)MyOptions.DemPath, "hgtpath", "", "path of the HGT/TIF-files (multiple usage)", FSoftUtils.CmdlineOptions.OptionArgumentType.String, int.MaxValue);
         cmd.DefineOption((int)MyOptions.GmapPath, "gmappath", "", "path of the gmap-map", FSoftUtils.CmdlineOptions.OptionArgumentType.String);
         cmd.DefineOption((int)MyOptions.TREFilename, "tre", "", "name of TRE-file (with the bounding area)", FSoftUtils.CmdlineOptions.OptionArgumentType.String);
         cmd.DefineOption((int)MyOptions.PixelDistance, "dist", "", "distance between DEM-points (multiple usage for different zoomlevel; priority over dlon/dlat)", FSoftUtils.CmdlineOptions.OptionArgumentType.PositivInteger, int.MaxValue);
         cmd.DefineOption((int)MyOptions.PixelWidth, "dlon", "o", "horizontal distance between DEM-points (multiple usage for different zoomlevel)", FSoftUtils.CmdlineOptions.OptionArgumentType.Double, int.MaxValue);
         cmd.DefineOption((int)MyOptions.PixelHeight, "dlat", "a", "vertical distance between DEM-points (multiple usage for different zoomlevel;" + Environment.NewLine +
                                                                   "default the same as dlon and then not necessary)", FSoftUtils.CmdlineOptions.OptionArgumentType.Double, int.MaxValue);
         cmd.DefineOption((int)MyOptions.OverviewPixelDistance, "ovdist", "", "distance between DEM-points for verviewmap (multiple usage for different zoomlevel;" + Environment.NewLine +
                                                                              "priority over ovdlon/ovdlat)", FSoftUtils.CmdlineOptions.OptionArgumentType.PositivInteger, int.MaxValue);
         cmd.DefineOption((int)MyOptions.OverviewPixelWidth, "ovdlon", "", "horizontal distance between DEM-points for verviewmap (multiple usage for different zoomlevel)", FSoftUtils.CmdlineOptions.OptionArgumentType.Double, int.MaxValue);
         cmd.DefineOption((int)MyOptions.OverviewPixelHeight, "ovdlat", "", "vertical distance between DEM-points for verviewmap (multiple usage for different zoomlevel)", FSoftUtils.CmdlineOptions.OptionArgumentType.Double, int.MaxValue);
         cmd.DefineOption((int)MyOptions.OverviewShrink, "ovshrink", "", "uneven shrinkvalue (multiple usage for different zoomlevel, default 1)", FSoftUtils.CmdlineOptions.OptionArgumentType.PositivInteger, int.MaxValue);
         cmd.DefineOption((int)MyOptions.UseDummyData, "usedummydata", "", "use NODATA-values (" + short.MinValue.ToString() + ") for absent HGT's (without arg 'true', default 'false')", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNot);
         cmd.DefineOption((int)MyOptions.OutputOverwrite, "overwrite", "O", "overwrites the  DEM file if exist (without arg 'true', default 'false')", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNot);
         cmd.DefineOption((int)MyOptions.StdInterpolation, "extintpol", "", "standard or extended (bicubic) interpolation (without arg 'true', default 'false')", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNot);
         cmd.DefineOption((int)MyOptions.DataInFoot, "foot", "f", "values in DEM in foot or else in meter (without arg 'true', default 'false')", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNot);

         cmd.DefineOption((int)MyOptions.DemDataOutput, "hgtoutput", "", "write interpolated data in text files (default 'false'; for test)", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNot);
         cmd.DefineOption((int)MyOptions.DataFilename, "data", "i", "read height data from text files (multiple usage; for test)", FSoftUtils.CmdlineOptions.OptionArgumentType.String, int.MaxValue);
         cmd.DefineOption((int)MyOptions.TRELeft, "left", "l", "westerly border of the area (alternatively for --tre; for test)", FSoftUtils.CmdlineOptions.OptionArgumentType.Double);
         cmd.DefineOption((int)MyOptions.TRETop, "top", "t", "northerly border of the area (alternatively for --tre; for test)", FSoftUtils.CmdlineOptions.OptionArgumentType.Double);
         cmd.DefineOption((int)MyOptions.DEMWidth, "width", "w", "width of the area (alternatively for --tre; for test)", FSoftUtils.CmdlineOptions.OptionArgumentType.Double);
         cmd.DefineOption((int)MyOptions.DEMHeight, "height", "h", "height of the area (alternatively for --tre; for test)", FSoftUtils.CmdlineOptions.OptionArgumentType.Double);
         cmd.DefineOption((int)MyOptions.LastColStd, "lastcolstd", "", "last subtile column have default width (64 points) (without arg 'true', default 'false'; for test)", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNot);
         cmd.DefineOption((int)MyOptions.NoSnapLeftTop, "lefttopnosnap", "", "without snapping for left-top dem-corner (without arg 'false', default 'true'; for test)", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNot);
         cmd.DefineOption((int)MyOptions.UseTestEncoder, "testencoder", "", "use testencoder (slow!) (without arg 'true', default 'false')", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNot);

         cmd.DefineOption((int)MyOptions.Multithread, "mt", "", "Berechnung multithreaded; Threadanzahl (ohne Argument '0', Standard '0')", FSoftUtils.CmdlineOptions.OptionArgumentType.IntegerOrNot);

         cmd.DefineOption((int)MyOptions.Help, "help", "?", "this text", FSoftUtils.CmdlineOptions.OptionArgumentType.Nothing);
      }

      /// <summary>
      /// Standardwerte setzen
      /// </summary>
      void Init() {
         DEMFilename = "";
         DataFilename = new List<string>();
         DemPath = new List<string>();
         DemDataOutput = false;
         GmapPath = "";
         UseDummyData = false;
         DataInFoot = false;
         TRELeft =
         TRETop =
         DEMWidth =
         DEMHeight = double.NaN;
         PixelDistance = new List<int>();
         PixelWidth = new List<double>();
         PixelHeight = new List<double>();
         OverviewPixelDistance = new List<int>();
         OverviewPixelWidth = new List<double>();
         OverviewPixelHeight = new List<double>();
         OverviewShrink = new List<int>();
         TREFilename = "";
         StdInterpolation = true;
         LastColStd = false;
         OutputOverwrite = false;
         NoSnapLeftTop = false;
         UseTestEncoder = false;
         Multithread = 0;
      }

      /// <summary>
      /// Auswertung der Optionen
      /// </summary>
      /// <param name="args"></param>
      public void Evaluate(string[] args) {
         if (args == null) return;
         List<string> InputArray_Tmp = new List<string>();

         try {
            cmd.Parse(args);

            foreach (MyOptions opt in Enum.GetValues(typeof(MyOptions))) {    // jede denkbare Option testen
               int optcount = cmd.OptionAssignment((int)opt);                 // Wie oft wurde diese Option verwendet?
               if (optcount > 0)
                  switch (opt) {
                     case MyOptions.DataFilename:
                        for (int i = 0; i < optcount; i++) {
                           string tmp = cmd.StringValue((int)opt, i).Trim();
                           if (tmp.Length > 0)
                              DataFilename.Add(tmp);
                        }
                        break;

                     case MyOptions.DEMFilename:
                        DEMFilename = cmd.StringValue((int)opt).Trim();
                        break;

                     case MyOptions.DemPath:
                        for (int i = 0; i < optcount; i++) {
                           string tmp = cmd.StringValue((int)opt, i).Trim();
                           if (tmp.Length > 0)
                              DemPath.Add(tmp);
                        }
                        break;

                     case MyOptions.GmapPath:
                        GmapPath = cmd.StringValue((int)opt).Trim();
                        break;

                     case MyOptions.DemDataOutput:
                        if (cmd.ArgIsUsed((int)opt))
                           DemDataOutput = cmd.BooleanValue((int)opt);
                        else
                           DemDataOutput = true;
                        break;

                     case MyOptions.UseDummyData:
                        if (cmd.ArgIsUsed((int)opt))
                           UseDummyData = cmd.BooleanValue((int)opt);
                        else
                           UseDummyData = true;
                        break;

                     case MyOptions.TREFilename:
                        TREFilename = cmd.StringValue((int)opt).Trim();
                        break;

                     case MyOptions.OutputOverwrite:
                        if (cmd.ArgIsUsed((int)opt))
                           OutputOverwrite = cmd.BooleanValue((int)opt);
                        else
                           OutputOverwrite = true;
                        break;

                     case MyOptions.LastColStd:
                        if (cmd.ArgIsUsed((int)opt))
                           LastColStd = cmd.BooleanValue((int)opt);
                        else
                           LastColStd = true;
                        break;

                     case MyOptions.DataInFoot:
                        if (cmd.ArgIsUsed((int)opt))
                           DataInFoot = cmd.BooleanValue((int)opt);
                        else
                           DataInFoot = true;
                        break;

                     case MyOptions.TRELeft:
                        TRELeft = cmd.DoubleValue((int)opt);
                        break;

                     case MyOptions.TRETop:
                        TRETop = cmd.DoubleValue((int)opt);
                        break;

                     case MyOptions.PixelWidth:
                        for (int i = 0; i < optcount; i++)
                           PixelWidth.Add(cmd.DoubleValue((int)opt, i));
                        break;

                     case MyOptions.PixelHeight:
                        for (int i = 0; i < optcount; i++)
                           PixelHeight.Add(cmd.DoubleValue((int)opt, i));
                        break;

                     case MyOptions.PixelDistance:
                        for (int i = 0; i < optcount; i++)
                           PixelDistance.Add((int)cmd.PositivIntegerValue((int)opt, i));
                        break;

                     case MyOptions.OverviewPixelWidth:
                        for (int i = 0; i < optcount; i++)
                           OverviewPixelWidth.Add(cmd.DoubleValue((int)opt, i));
                        break;

                     case MyOptions.OverviewPixelHeight:
                        for (int i = 0; i < optcount; i++)
                           OverviewPixelHeight.Add(cmd.DoubleValue((int)opt, i));
                        break;

                     case MyOptions.OverviewPixelDistance:
                        for (int i = 0; i < optcount; i++)
                           OverviewPixelDistance.Add((int)cmd.PositivIntegerValue((int)opt, i));
                        break;

                     case MyOptions.OverviewShrink:
                        for (int i = 0; i < optcount; i++) {
                           uint v = cmd.PositivIntegerValue((int)opt, i);
                           if (v % 2 != 1)
                              throw new Exception("only uneven values for shrink permitted");
                           OverviewShrink.Add((int)v);
                        }
                        break;

                     case MyOptions.DEMWidth:
                        DEMWidth = cmd.DoubleValue((int)opt);
                        break;

                     case MyOptions.DEMHeight:
                        DEMHeight = cmd.DoubleValue((int)opt);
                        break;

                     case MyOptions.NoSnapLeftTop:
                        if (cmd.ArgIsUsed((int)opt))
                           NoSnapLeftTop = cmd.BooleanValue((int)opt);
                        else
                           NoSnapLeftTop = true;
                        break;

                     case MyOptions.StdInterpolation:
                        if (cmd.ArgIsUsed((int)opt))
                           StdInterpolation = cmd.BooleanValue((int)opt);
                        else
                           StdInterpolation = false;
                        break;

                     case MyOptions.UseTestEncoder:
                        if (cmd.ArgIsUsed((int)opt))
                           UseTestEncoder = cmd.BooleanValue((int)opt);
                        else
                           UseTestEncoder = true;
                        break;


                     case MyOptions.Multithread:
                        if (cmd.ArgIsUsed((int)opt))
                           Multithread = cmd.IntegerValue((int)opt);
                        else
                           Multithread = 0;
                        break;

                     case MyOptions.Help:
                        ShowHelp();
                        break;

                  }
            }

            //TestParameter = new string[cmd.Parameters.Count];
            //cmd.Parameters.CopyTo(TestParameter);

            // für gleiche Anzahl Höhen und Breiten sorgen
            if (PixelDistance.Count > 0) {
               PixelHeight.Clear();
               PixelWidth.Clear();
            } else {
               while (PixelHeight.Count < PixelWidth.Count) {
                  PixelHeight.Add(PixelWidth[PixelHeight.Count]);
               }
               while (PixelWidth.Count < PixelHeight.Count) {
                  PixelWidth.Add(PixelHeight[PixelWidth.Count]);
               }
            }

            if (OverviewPixelDistance.Count > 0) {
               OverviewPixelHeight.Clear();
               OverviewPixelWidth.Clear();
            } else {
               while (OverviewPixelHeight.Count < OverviewPixelWidth.Count) {
                  OverviewPixelHeight.Add(OverviewPixelWidth[OverviewPixelHeight.Count]);
               }
               while (OverviewPixelWidth.Count < OverviewPixelHeight.Count) {
                  OverviewPixelWidth.Add(OverviewPixelHeight[OverviewPixelWidth.Count]);
               }
            }
            while (OverviewShrink.Count < Math.Max(OverviewPixelDistance.Count, OverviewPixelHeight.Count)) {
               OverviewShrink.Add(1);
            }

            if (Multithread == 0)
               Multithread = FSoftUtils.TaskQueue.LogicalProcessorCores();

            if (cmd.Parameters.Count > 0)
               throw new Exception(string.Format("args not permitted ({0})", cmd.Parameters[0]));

         } catch (Exception ex) {
            Console.Error.WriteLine(ex.Message);
            ShowHelp();
            throw new Exception("Error on prog-options.");
         }
      }

      /// <summary>
      /// Hilfetext für Optionen ausgeben
      /// </summary>
      /// <param name="cmd"></param>
      public void ShowHelp() {
         List<string> help = cmd.GetHelpText();
         for (int i = 0; i < help.Count; i++) Console.Error.WriteLine(help[i]);
         Console.Error.WriteLine();


         Console.Error.WriteLine("You can substitut '--' with '/' and '=' with ':' or space.");
         Console.Error.WriteLine("Args with ';' are split to single args. If you need a ';' in arg use \"arg\".");

         // ...

      }


   }
}
