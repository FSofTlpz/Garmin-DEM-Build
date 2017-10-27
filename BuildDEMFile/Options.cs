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
      /// Pfad zu den HGT-Dateien
      /// </summary>
      public string HGTPath { get; private set; }

      /// <summary>
      /// verwendet Dummydaten wenn keine HGT-Daten ex.
      /// </summary>
      public bool UseDummyData { get; private set; }

      /// <summary>
      /// falls HGT-Daten gelesen werden, können sie in diese Textdatei ausgegeben werden
      /// </summary>
      public string HGTDataOutput { get; private set; }

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


      FSoftUtils.CmdlineOptions cmd;

      enum MyOptions {
         DEMFilename,
         DataFilename,
         HGTPath,
         HGTDataOutput,
         UseDummyData,
         DataInFoot,
         TRELeft,
         TRETop,
         TREFilename,
         PixelWidth,
         PixelHeight,
         DEMWidth,
         DEMHeight,
         LastColStd,
         OutputOverwrite,

         Help,
      }

      public Options() {
         Init();
         cmd = new FSoftUtils.CmdlineOptions();
         // Definition der Optionen
         cmd.DefineOption((int)MyOptions.DEMFilename, "dem", "d", "Name der zu erzeugenden DEM-Datei", FSoftUtils.CmdlineOptions.OptionArgumentType.String);
         cmd.DefineOption((int)MyOptions.DataFilename, "data", "i", "Name der Textdatei mit den Daten", FSoftUtils.CmdlineOptions.OptionArgumentType.String, int.MaxValue);
         cmd.DefineOption((int)MyOptions.HGTPath, "hgtpath", "", "Pfad zum HGT-Verzeichnis", FSoftUtils.CmdlineOptions.OptionArgumentType.String);
         cmd.DefineOption((int)MyOptions.HGTDataOutput, "hgtoutput", "", "Ausgabe der verwendeten Daten in eine Textdatei", FSoftUtils.CmdlineOptions.OptionArgumentType.String);
         cmd.DefineOption((int)MyOptions.UseDummyData, "usedummydata", "", "verwendet Dummy-Daten, wenn keine HGT-Daten vorhanden sind (ohne Argument 'true', Standard 'false')", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNot);
         cmd.DefineOption((int)MyOptions.DataInFoot, "foot", "f", "Daten in Fuß, sonst Meter (ohne Argument 'true', Standard 'false')", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNot);
         cmd.DefineOption((int)MyOptions.TRELeft, "left", "l", "linker Rand der TRE-Datei", FSoftUtils.CmdlineOptions.OptionArgumentType.Double);
         cmd.DefineOption((int)MyOptions.TRETop, "top", "t", "oberer Rand der TRE-Datei", FSoftUtils.CmdlineOptions.OptionArgumentType.Double);
         cmd.DefineOption((int)MyOptions.PixelWidth, "dlon", "o", "Breite eines DEM-Pixels (mehrfach verwendbar für versch. Level)", FSoftUtils.CmdlineOptions.OptionArgumentType.Double, int.MaxValue);
         cmd.DefineOption((int)MyOptions.PixelHeight, "dlat", "a", "Höhe eines DEM-Pixels (mehrfach verwendbar für versch. Level)", FSoftUtils.CmdlineOptions.OptionArgumentType.Double, int.MaxValue);
         cmd.DefineOption((int)MyOptions.DEMWidth, "width", "w", "Breite des DEM-Bereiches", FSoftUtils.CmdlineOptions.OptionArgumentType.Double);
         cmd.DefineOption((int)MyOptions.DEMHeight, "height", "h", "Höhe des DEM-Bereiches", FSoftUtils.CmdlineOptions.OptionArgumentType.Double);
         cmd.DefineOption((int)MyOptions.TREFilename, "tre", "", "Name der TRE-Datei (zur Bestimmung der Ränder und der Pixelbreite)", FSoftUtils.CmdlineOptions.OptionArgumentType.String);
         cmd.DefineOption((int)MyOptions.LastColStd, "lastcolstd", "", "letzte Kachelspalte hat Standardbreite (ohne Argument 'true', Standard 'false')", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNot);
         cmd.DefineOption((int)MyOptions.OutputOverwrite, "overwrite", "O", "Ausgabeziel bei Bedarf überschreiben (ohne Argument 'true', Standard 'false')", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNot);

         cmd.DefineOption((int)MyOptions.Help, "help", "?", "diese Hilfe", FSoftUtils.CmdlineOptions.OptionArgumentType.Nothing);
      }

      /// <summary>
      /// Standardwerte setzen
      /// </summary>
      void Init() {
         DEMFilename = "";
         DataFilename = new List<string>();
         HGTPath = "";
         UseDummyData = false;
         DataInFoot = false;
         TRELeft =
         TRETop =
         DEMWidth =
         DEMHeight = double.NaN;
         PixelWidth = new List<double>();
         PixelHeight = new List<double>();
         TREFilename = "";
         LastColStd = false;
         OutputOverwrite = false;
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

                     case MyOptions.HGTPath:
                        HGTPath = cmd.StringValue((int)opt).Trim();
                        break;

                     case MyOptions.HGTDataOutput:
                        HGTDataOutput = cmd.StringValue((int)opt).Trim();
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

                     case MyOptions.DEMWidth:
                        DEMWidth = cmd.DoubleValue((int)opt);
                        break;

                     case MyOptions.DEMHeight:
                        DEMHeight = cmd.DoubleValue((int)opt);
                        break;

                     case MyOptions.Help:
                        ShowHelp();
                        break;

                  }
            }

            //TestParameter = new string[cmd.Parameters.Count];
            //cmd.Parameters.CopyTo(TestParameter);

            // für gleiche Anzahl Höhen und Breiten sorgen
            while (PixelHeight.Count < PixelWidth.Count) {
               PixelHeight.Add(PixelWidth[PixelHeight.Count]);
            }
            while (PixelWidth.Count < PixelHeight.Count) {
               PixelWidth.Add(PixelHeight[PixelWidth.Count]);
            }

            if (cmd.Parameters.Count > 0)
               throw new Exception("Es sind keine Argumente sondern nur Optionen erlaubt.");

         } catch (Exception ex) {
            Console.Error.WriteLine(ex.Message);
            ShowHelp();
            throw new Exception("Fehler beim Ermitteln oder Anwenden der Programmoptionen.");
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
         Console.Error.WriteLine("Zusatzinfos:");


         Console.Error.WriteLine("Für '--' darf auch '/' stehen und für '=' auch ':' oder Leerzeichen.");
         Console.Error.WriteLine("Argumente mit ';' werden an diesen Stellen in Einzelargumente aufgetrennt.");

         // ...

      }


   }
}
