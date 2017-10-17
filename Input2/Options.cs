using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Input2 {

   /// <summary>
   /// Optionen und Argumente werden zweckmäßigerweise in eine (programmabhängige) Klasse gekapselt.
   /// Erzeugen des Objektes und Evaluate() sollten in einem try-catch-Block erfolgen.
   /// </summary>
   public class Options {

      // alle Optionen sind i.A. 'read-only'

      /// <summary>
      /// DEM-Datei
      /// </summary>
      public string PatchFilename { get; private set; }
      /// <summary>
      /// Adresse der Höhendaten in <see cref="PatchFilename"/>
      /// </summary>
      public ulong PatchAdr { get; private set; }
      /// <summary>
      /// Länge der Höhendaten
      /// </summary>
      public uint PatchRange { get; private set; }
      /// <summary>
      /// restlichen Bereich mit 1-Bit füllen (sonst 0)
      /// </summary>
      public bool Fillbit1 { get; private set; }

      /// <summary>
      /// Folge der Binärbist bei der Init.
      /// </summary>
      public string BinInit { get; private set; }

      /// <summary>
      /// Basishöhe (Min.)
      /// </summary>
      public int BaseHeight { get; private set; }
      /// <summary>
      /// Adresse der <see cref="BaseHeight"/>
      /// </summary>
      public ulong BaseHeightAdr { get; private set; }
      /// <summary>
      /// max. Höhendifferenz
      /// </summary>
      public uint MaxHeightDiff { get; private set; }
      /// <summary>
      /// Adresse der <see cref="MaxHeightDiff"/>
      /// </summary>
      public ulong MaxHeightDiffAdr { get; private set; }
      /// <summary>
      /// Codiertyp (nur 1 Byte!)
      /// </summary>
      public int Codingtype { get; private set; }
      /// <summary>
      /// Adresse des <see cref="Codingtype"/>
      /// </summary>
      public ulong CodingtypeAdr { get; private set; }
      /// <summary>
      /// Höhenangaben sind real (sonst auf <see cref="BaseHeight"/> bezogen)
      /// </summary>
      public bool RealHeights { get; private set; }
      /// <summary>
      /// <see cref="BaseHeight"/> und <see cref="MaxHeightDiff"/> automatisch aus den Höhendaten ermitteln
      /// </summary>
      public bool BaseDiffAuto { get; private set; }

      /// <summary>
      /// Kachelbreite
      /// </summary>
      public uint TileWidth { get; private set; }
      /// <summary>
      /// Kachelhöhe
      /// </summary>
      public uint TileHeight { get; private set; }

      /// <summary>
      /// Name der Protokolldatei
      /// </summary>
      public string ProtFilename { get; private set; }
      /// <summary>
      /// externes Kommando
      /// </summary>
      public string ExternCommand { get; private set; }
      /// <summary>
      /// Argumente für <see cref="ExternCommand"/>
      /// </summary>
      public List<string> ExternCommandParams { get { return cmd.Parameters; } }

      /// <summary>
      /// autom. starten und beenden
      /// </summary>
      public bool AutoStartAndEnd { get; private set; }


      FSoftUtils.CmdlineOptions cmd;

      enum MyOptions {
         PatchFilename,
         PatchAdr,
         PatchRange,
         Fillbit1,

         BinInit,

         BaseHeight,
         BaseHeightAdr,
         MaxHeightDiff,
         MaxHeightDiffAdr,
         Codingtype,
         CodingtypeAdr,
         RealHeights,
         BaseDiffAuto,

         TileWidth,
         TileHeight,

         ProtFilename,
         ExternCommand,
         AutoStartAndEnd,


         Help,
      }

      public Options() {
         cmd = new FSoftUtils.CmdlineOptions();
         // Definition der Optionen
         cmd.DefineOption((int)MyOptions.PatchFilename, "dem", "d", "Name der DEM-Datei", FSoftUtils.CmdlineOptions.OptionArgumentType.String);
         cmd.DefineOption((int)MyOptions.PatchAdr, "demadr", "a", "Adresse des Datenbereiches in der DEM-Kachel", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedLong);
         cmd.DefineOption((int)MyOptions.PatchRange, "demlen", "l", "Länge des Datenbereiches in der DEM-Kachel", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedInteger);
         cmd.DefineOption((int)MyOptions.Fillbit1, "fillbit1", "", "restlichen Bereich mit 1-Bit füllen (ohne Argument 'true', Standard 'true')", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNot);
         cmd.DefineOption((int)MyOptions.BinInit, "bits", "", "Init-Bits", FSoftUtils.CmdlineOptions.OptionArgumentType.String);
         cmd.DefineOption((int)MyOptions.BaseHeight, "base", "b", "Basishöhe der DEM-Kachel", FSoftUtils.CmdlineOptions.OptionArgumentType.Integer);
         cmd.DefineOption((int)MyOptions.BaseHeightAdr, "baseadr", "", "Adresse der Basishöhe der DEM-Kachel", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedLong);
         cmd.DefineOption((int)MyOptions.MaxHeightDiff, "maxdiff", "m", "max. Höhendiff. der DEM-Kachel", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedInteger);
         cmd.DefineOption((int)MyOptions.MaxHeightDiffAdr, "maxdiffadr", "", "Adresse der max. Höhendiff. der DEM-Kachel", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedLong);
         cmd.DefineOption((int)MyOptions.Codingtype, "codingtyp", "", "Codiertyp der DEM-Kachel", FSoftUtils.CmdlineOptions.OptionArgumentType.Integer);
         cmd.DefineOption((int)MyOptions.CodingtypeAdr, "codingtypadr", "", "Adresse des Codiertyps der DEM-Kachel", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedLong);
         cmd.DefineOption((int)MyOptions.BaseDiffAuto, "basediffauto", "", "Basishöhe und max. Differenz automatisch aus den Höhenangaben ermitteln (ohne Argument 'true', Standard 'true')", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNot);
         cmd.DefineOption((int)MyOptions.RealHeights, "realheights", "", "reale Höhenangaben oder auf Basishöhe bezogen (ohne Argument 'true', Standard 'false')", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNot);
         cmd.DefineOption((int)MyOptions.TileHeight, "tileheight", "", "Höhe der DEM-Kachel (Standard 64)", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedInteger);
         cmd.DefineOption((int)MyOptions.TileWidth, "tilewidth", "", "Breite der DEM-Kachel (Standard 64)", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedInteger);
         cmd.DefineOption((int)MyOptions.ProtFilename, "prot", "p", "Name der Protokolldatei", FSoftUtils.CmdlineOptions.OptionArgumentType.String);
         cmd.DefineOption((int)MyOptions.ExternCommand, "ext", "e", "Name des externen Programms", FSoftUtils.CmdlineOptions.OptionArgumentType.String);
         cmd.DefineOption((int)MyOptions.AutoStartAndEnd, "autostartend", "", "Programm automatisch beenden (ohne Argument 'true', Standard 'false')", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNot);

         cmd.DefineOption((int)MyOptions.Help, "help", "?", "diese Hilfe", FSoftUtils.CmdlineOptions.OptionArgumentType.Nothing);

         PatchFilename = "";
         PatchAdr = 0;
         PatchRange = 0;
         Fillbit1 = true;

         BinInit = "";
         BaseHeight = 0;
         BaseHeightAdr = 0;
         MaxHeightDiff = 0;
         MaxHeightDiffAdr = 0;
         CodingtypeAdr = 0;
         RealHeights = false;
         BaseDiffAuto = true;

         TileHeight = TileWidth = 64;

         ProtFilename = "";
         ExternCommand = "";
         AutoStartAndEnd = false;
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
                     case MyOptions.PatchFilename:
                        PatchFilename = cmd.StringValue((int)opt).Trim();
                        break;

                     case MyOptions.PatchAdr:
                        PatchAdr = cmd.UnsignedLongValue((int)opt);
                        break;

                     case MyOptions.PatchRange:
                        PatchRange = cmd.UnsignedIntegerValue((int)opt);
                        break;

                     case MyOptions.Fillbit1:
                        if (cmd.ArgIsUsed((int)opt))
                           Fillbit1 = cmd.BooleanValue((int)opt);
                        else
                           Fillbit1 = true;
                        break;

                     case MyOptions.BinInit:
                        BinInit = cmd.StringValue((int)opt).Trim();
                        break;


                     case MyOptions.BaseHeight:
                        BaseHeight = cmd.IntegerValue((int)opt);
                        break;

                     case MyOptions.BaseHeightAdr:
                        BaseHeightAdr = cmd.UnsignedLongValue((int)opt);
                        break;

                     case MyOptions.MaxHeightDiff:
                        MaxHeightDiff = cmd.UnsignedIntegerValue((int)opt);
                        break;

                     case MyOptions.MaxHeightDiffAdr:
                        MaxHeightDiffAdr = cmd.UnsignedLongValue((int)opt);
                        break;

                     case MyOptions.Codingtype:
                        Codingtype = cmd.IntegerValue((int)opt);
                        break;

                     case MyOptions.CodingtypeAdr:
                        CodingtypeAdr = cmd.UnsignedLongValue((int)opt);
                        break;

                     case MyOptions.RealHeights:
                        if (cmd.ArgIsUsed((int)opt))
                           RealHeights = cmd.BooleanValue((int)opt);
                        else
                           RealHeights = true;
                        break;

                     case MyOptions.BaseDiffAuto:
                        if (cmd.ArgIsUsed((int)opt))
                           BaseDiffAuto = cmd.BooleanValue((int)opt);
                        else
                           BaseDiffAuto = true;
                        break;
                       

                     case MyOptions.TileHeight:
                        TileHeight = cmd.UnsignedIntegerValue((int)opt);
                        break;

                     case MyOptions.TileWidth:
                        TileWidth = cmd.UnsignedIntegerValue((int)opt);
                        break;


                     case MyOptions.ProtFilename:
                        ProtFilename = cmd.StringValue((int)opt).Trim();
                        break;

                     case MyOptions.ExternCommand:
                        ExternCommand = cmd.StringValue((int)opt).Trim();
                        break;

                     case MyOptions.AutoStartAndEnd:
                        if (cmd.ArgIsUsed((int)opt))
                           AutoStartAndEnd = cmd.BooleanValue((int)opt);
                        else
                           AutoStartAndEnd = true;
                        break;

                     case MyOptions.Help:
                        ShowHelp();
                        break;

                  }
            }

            //TestParameter = new string[cmd.Parameters.Count];
            //cmd.Parameters.CopyTo(TestParameter);

            //if (cmd.Parameters.Count > 0)
            //   throw new Exception("Es sind keine Argumente sondern nur Optionen erlaubt.");

         } catch (Exception ex) {
            //Console.Error.WriteLine(ex.Message);
            //ShowHelp();
            throw new Exception("Fehler beim Ermitteln oder Anwenden der Programmoptionen: " + System.Environment.NewLine + System.Environment.NewLine + ex.Message);
         }
      }

      /// <summary>
      /// Hilfetext für Optionen ausgeben
      /// </summary>
      /// <param name="cmd"></param>
      public void ShowHelp() {
         List<string> help = cmd.GetHelpText();
         help.Add("Zusatzinfos:");
         help.Add("Für '--' darf auch '/' stehen und für '=' auch ':' oder Leerzeichen.");
         help.Add("Argumente mit ';' werden an diesen Stellen in Einzelargumente aufgetrennt.");
         MessageBox.Show(string.Join(System.Environment.NewLine, help), "Hilfe", MessageBoxButtons.OK, MessageBoxIcon.Information);

         //for (int i = 0; i < help.Count; i++)
         //   Console.Error.WriteLine(help[i]);
         //Console.Error.WriteLine();
         //Console.Error.WriteLine("Zusatzinfos:");


         //Console.Error.WriteLine("Für '--' darf auch '/' stehen und für '=' auch ':' oder Leerzeichen.");
         //Console.Error.WriteLine("Argumente mit ';' werden an diesen Stellen in Einzelargumente aufgetrennt.");

         // ...

      }


   }
}
