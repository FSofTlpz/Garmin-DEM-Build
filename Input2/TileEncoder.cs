// höchstwahrscheinlich unnötige Teile
//#define INCLUDENOTNEEDED

// Funktionen nur zur "Erforschung"
//#define EXPLORERFUNCTION

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

#pragma warning disable IDE1006 // Benennungsstile

/*
CS0659: "Klasse" überschreibt Object.Equals(object o), aber nicht Object.GetHashCode() ('class' overrides Object.Equals(object o) but does not override Object.GetHashCode())
CS0660: "Klasse" definiert den Operator == oder !=, aber überschreibt Object.Equals(object o) nicht. ('class' defines operator == or operator != but does not override Object.Equals(object o))
CS0661: "Klasse" definiert den Operator == oder !=, aber überschreibt Object.GetHashCode() nicht. ('class' defines operator == or operator != but does not override Object.GetHashCode())
#pragma warning disable 659, 661
*/

namespace Encoder {

   /// <summary>
   /// Encoder für eine einzelne DEM-Kachel
   /// </summary>
   public class TileEncoder {

      #region Hilfsklassen

      /// <summary>
      /// zum einfacheren Umgang mit Positionen
      /// </summary>
      class Position {
         /// <summary>
         /// Matrix-Breite
         /// </summary>
         public int Width { get; private set; }
         /// <summary>
         /// Matrix-Höhe
         /// </summary>
         public int Height { get; private set; }
         /// <summary>
         /// Position waagerecht
         /// </summary>
         public int X { get; private set; }
         /// <summary>
         /// Position senkrecht
         /// </summary>
         public int Y { get; private set; }
         /// <summary>
         /// liefert oder setzt den fortlaufenden Index der Position (0 .. (<see cref="Width"/> * <see cref="Height"/> - 1))
         /// </summary>
         public int Idx {
            get {
               return Y * Width + X;
            }
            set {
               if (value < Width * Height) {
                  Set(value % Width, value / Width);
                  PositionError = false;
               } else {
                  Set(Width - 1, Height - 1);
                  PositionError = true;
               }
            }
         }
         /// <summary>
         /// liefert true, wenn die letzte Positionierung eine Bereichsüberschreitung ergab
         /// </summary>
         public bool PositionError { get; private set; }


         /// <summary>
         /// erzeugt eine Position
         /// </summary>
         /// <param name="width">Breite</param>
         /// <param name="height">Höhe</param>
         /// <param name="x">Position waagerecht</param>
         /// <param name="y">Position senkrecht</param>
         public Position(int width, int height, int x = 0, int y = 0) {
            Width = width;
            Height = height;
            Set(x, y);
         }

         /// <summary>
         /// erzeugt eine Position als Differenz zu einer existierenden Position
         /// </summary>
         /// <param name="pos">existierende Position</param>
         /// <param name="deltax">waagerechte Differenz</param>
         /// <param name="deltay">senkrechte Differenz</param>
         public Position(Position pos, int deltax = 0, int deltay = 0) {
            X = pos.X + deltax;
            Y = pos.Y + deltay;
            Width = pos.Width;
            Height = pos.Height;
         }

         public bool Equals(Position pos) {
            if (pos == null) // NICHT "p == null" usw. --> führt zur Endlosschleife
               return false;
            return (X == pos.X) && (Y == pos.Y);
         }

         public override bool Equals(object obj) {
            if (obj == null)
               return false;

            // If parameter cannot be cast to Point return false.
            Position p = obj as Position;
            if (p == null)
               return false;

            return (X == p.X) && (Y == p.Y);
         }

         public override int GetHashCode() {
            return base.GetHashCode();
         }

         public static bool operator ==(Position a, Position b) {
            return a.Equals(b);
         }

         public static bool operator !=(Position a, Position b) {
            return !a.Equals(b);
         }


         /// <summary>
         /// setzt die Position (automatisch begrenzt durch den "Rand")
         /// </summary>
         /// <param name="x"></param>
         /// <param name="y"></param>
         /// <returns>false, falls der Bereich überschritten würde</returns>
         public bool Set(int x, int y) {
            PositionError = false;
            if (x < 0 || x >= Width) {
               PositionError = true;
               if (x < 0)
                  x = 0;
               else
                  x = Width - 1;
            }
            if (y < 0 || y >= Height) {
               PositionError = true;
               if (y < 0)
                  y = 0;
               else
                  y = Height - 1;
            }
            X = x;
            Y = y;
            return !PositionError;
         }

         /// <summary>
         /// rückt die Position um die entsprechende Anzahl Schritte, ev. auch mit Zeilenwechsel, weiter
         /// </summary>
         /// <param name="count">Anzahl der Schritte</param>
         /// <returns>false, falls der Bereich überschritten würde</returns>
         public bool MoveForward(int count = 1) {
            PositionError = false;
            switch (count) {
               case 0:
                  break;

               case 1:
                  if (++X >= Width)
                     if (Y < Height - 1) {
                        Y++;
                        X = 0;
                     } else {
                        X = Width - 1;
                        PositionError = true;
                     }
                  break;

               default:
                  Idx = Idx + count;
                  if (Idx >= Height * Width) {
                     Y = Height - 1;
                     X = Width - 1;
                     PositionError = true;
                  }
                  break;
            }
            return !PositionError;
         }

#if INCLUDENOTNEEDED

         public static bool operator <=(Position a, Position b) {
            return a.Idx <= b.Idx;
         }

         public static bool operator >=(Position a, Position b) {
            return a.Idx >= b.Idx;
         }

         public static bool operator <(Position a, Position b) {
            return a.Idx < b.Idx;
         }

         public static bool operator >(Position a, Position b) {
            return a.Idx > b.Idx;
         }

         /// <summary>
         /// Änderung der Position (automatisch begrenzt durch den "Rand")
         /// </summary>
         /// <param name="deltax"></param>
         /// <param name="deltay"></param>
         /// <returns>false, falls der Bereich überschritten würde</returns>
         public bool Move(int deltax, int deltay) {
            PositionError = false;
            X += deltax;
            if (X < 0) {
               X = 0;
               PositionError = true;
            } else if (X >= Width) {
               X = Width - 1;
               PositionError = true;
            }
            if (Y < 0) {
               Y = 0;
               PositionError = true;
            } else if (Y >= Height) {
               Y = Height - 1;
               PositionError = true;
            }
            return !PositionError;
         }

         /// <summary>
         /// rückt die Position 1 Schritt, ev. auch mit Zeilenwechsel, zurück
         /// </summary>
         /// <returns>false, falls der Bereich überschritten würde</returns>
         public bool MoveBackward() {
            PositionError = false;
            if (--X < 0)
               if (Y > 0) {
                  Y--;
                  X = Width - 1;
               } else {
                  X = 0;
                  PositionError = true;
               }
            return !PositionError;
         }

         /// <summary>
         /// ändert die vertikale Position um die entsprechende Anzahl Schritte (automatisch begrenzt durch den "Rand")
         /// </summary>
         /// <param name="count">Anzahl der Schritte</param>
         /// <returns>false, falls der Bereich überschritten würde</returns>
         public bool MoveVertical(int count) {
            PositionError = false;
            if (count != 0)
               Y += count;
            if (Y < 0) {
               Y = 0;
               PositionError = true;
            } else if (Y >= Height) {
               Y = Height - 1;
               PositionError = true;
            }
            return !PositionError;
         }

#endif

         public override string ToString() {
            return string.Format("X={0}, Y={1}, Width={2}, Height={3}, PositionError={4}", X, Y, Width, Height, PositionError);
         }

      }

      /// <summary>
      /// Ein Objekt dieser Klasse kann nur mit einer der statischen Funktionen erzeugt werden!
      /// <para>Ein Höhenelement beschreibt im einfachsten Fall die Höhe an einer einzelnen Position. Es kann damit aber auch ein Plateau beschrieben werden.
      /// Für die Nachfolgehöhe eines Plateaus gelten auch spezielle Regeln.</para>
      /// </summary>
      public class HeightElement {

         /// <summary>
         /// Typ des Höhen-Elements
         /// </summary>
         public enum Typ {
            /// <summary>
            /// normaler Höhenwert mit Hook &gt;= Max
            /// </summary>
            ValueHookHigh,

            /// <summary>
            /// normaler Höhenwert mit 0 &lt; Hook &lt; Max
            /// </summary>
            ValueHookMiddle,

            /// <summary>
            /// normaler Höhenwert mit Hook &lt;= 0
            /// </summary>
            ValueHookLow,

            /// <summary>
            /// ein Plateau
            /// </summary>
            PlateauLength,

            /// <summary>
            /// Wert hinter einem Plateau
            /// </summary>
            PlateauFollower,

            /// <summary>
            /// Wert hinter einem Plateau
            /// </summary>
            PlateauFollower0,
         }

         public class HeightElementInfo {

            /// <summary>
            /// Typ des <see cref="HeightElement"/>
            /// </summary>
            public Typ Typ { get; private set; }
            ///// <summary>
            ///// Datenwert
            ///// </summary>
            public int Data { get; private set; }
            ///// <summary>
            ///// Element bezieht sich auf diese Spalte
            ///// </summary>
            public int Column { get; private set; }
            ///// <summary>
            ///// Element bezieht sich auf diese Zeile
            ///// </summary>
            public int Line { get; private set; }
            ///// <summary>
            ///// Wurde der Datenwert gewrapped?
            ///// </summary>
            public bool Wrapped { get; private set; }
            ///// <summary>
            ///// Codierart
            ///// </summary>
            public EncodeMode EncMode { get; set; }
            ///// <summary>
            ///// HUnit, gültig nur bei Hybridcodierung
            ///// </summary>
            public int Hunit { get; private set; }
            /// <summary>
            /// Diagonaldiff. für den Plateaunachfolger
            /// </summary>
            public int Ddiff { get; private set; }
            /// <summary>
            /// Wert ist am Maximum (nur bei shrink möglich) oder am Minimum ausgerichtet
            /// </summary>
            public bool TopAligned { get; private set; }
            /// <summary>
            /// Ausrichtung der umgebenden Daten
            /// </summary>
            public Shrink.Align3Type Alignment { get; private set; }
            /// <summary>
            /// Zeilenlänge
            /// </summary>
            public int LineLength { get; private set; }
            /// <summary>
            /// Hook
            /// </summary>
            public int Hook { get; private set; }
            /// <summary>
            /// Höhe
            /// </summary>
            public int Height { get; private set; }
            /// <summary>
            /// vorhergehendes <see cref="HeightElement"/> (nur bei Plateau)
            /// </summary>
            public HeightElement LastHeightElement { get; private set; }


            public HeightElementInfo(ValueData valdata) {
               Typ = valdata.HeightElementTyp;
               Data = valdata.Data;
               Column = valdata.X;
               Line = valdata.Y;
               Wrapped = valdata.IsWrapped;
               TopAligned = valdata.IsTopAligned;
               EncMode = valdata.Encodemode;
               Hunit = valdata.Hunit;
               Ddiff = valdata.DiagDiff;
               LineLength = int.MinValue;
               Hook = valdata.Hook;
               Height = valdata.Height;
               Alignment = valdata.Alignment;
               LastHeightElement = null;
            }

            public HeightElementInfo(ValueData valdata, int linelength, IList<HeightElement> oldheightelements) :
               this(valdata) {
               LineLength = linelength;
               TopAligned = valdata.IsTopAligned;
               for (int i = oldheightelements.Count - 1; i >= 0; i--) // letztes Plateau-Element suchen
                  if (oldheightelements[i].Info.Typ == Typ.PlateauLength) {
                     LastHeightElement = oldheightelements[i];
                     break;
                  }
            }

         }


         public HeightElementInfo Info;

         /// <summary>
         /// Bit-Liste der Codierung
         /// </summary>
         public List<byte> Bits { get; private set; }

         /// <summary>
         /// Tabelle für die Codierung der Plateaulängen
         /// </summary>
         public class PlateauTable {

            class PlateauTableItem {
               /// <summary>
               /// Wert für ein 1-Bit
               /// </summary>
               public int Unit { get; private set; }
               /// <summary>
               /// Anzahl der Binärbits
               /// </summary>
               public int Bits { get; private set; }
               /// <summary>
               /// Symbol zur Darstellung
               /// </summary>
               public char Symbol { get; private set; }

               public PlateauTableItem(int unit, int bits, char symbol) {
                  Unit = unit;
                  Bits = bits;
                  Symbol = symbol;
               }

               public override string ToString() {
                  return string.Format("Unit={0}, Bits={1}, Symbol={2}", Unit, Bits, Symbol);
               }

            }

            PlateauTableItem[] table;

            /// <summary>
            /// aktueller Tabellenindex
            /// </summary>
            public int Idx { get; private set; }

            /// <summary>
            /// Hat der Index den größten Wert erreicht?
            /// </summary>
            public bool IdxOnTop {
               get {
                  return Idx == table.Length - 1;
               }
            }

            public bool IncrementFailed { get; private set; }

            public bool DecrementFailed { get; private set; }


            public PlateauTable() {
               Idx = 0;
               table = new PlateauTableItem[] {
                        new PlateauTableItem(1,0, '1'),
                        new PlateauTableItem(1,0, '1'),
                        new PlateauTableItem(1,0, '1'),
                        new PlateauTableItem(1,1, 'a'),
                        new PlateauTableItem(2,1, '2'),
                        new PlateauTableItem(2,1, '2'),
                        new PlateauTableItem(2,1, '2'),
                        new PlateauTableItem(2,2, 'b'),
                        new PlateauTableItem(4,2, '4'),
                        new PlateauTableItem(4,2, '4'),
                        new PlateauTableItem(4,2, '4'),
                        new PlateauTableItem(4,3, 'c'),
                        new PlateauTableItem(8,3, '8'),
                        new PlateauTableItem(8,3, '8'),
                        new PlateauTableItem(8,3, '8'),
                        new PlateauTableItem(8,4, 'd'),
                        new PlateauTableItem(16,4, '6'),
                        new PlateauTableItem(16,5, 'e'),
                        new PlateauTableItem(32,5, '3'),
                        new PlateauTableItem(32,6, 'f'),
                        new PlateauTableItem(64,6, 'G'),
                        new PlateauTableItem(64,7, 'g'),
                        new PlateauTableItem(128,8, 'H'),
                     };
            }

            /// <summary>
            /// der aktuelle Tabellenindex wird (wenn möglich) vergrößert
            /// </summary>
            public bool IncrementIdx() {
               DecrementFailed = false;
               if (Idx < table.Length - 1) {
                  Idx++;
                  IncrementFailed = false;
               } else
                  IncrementFailed = true;
               return !IncrementFailed;
            }

            /// <summary>
            /// der aktuelle Tabellenindex wird (wenn möglich) verkleinert
            /// </summary>
            public bool DecrementIdx() {
               IncrementFailed = false;
               if (Idx > 0) {
                  Idx--;
                  DecrementFailed = false;
               } else
                  DecrementFailed = true;
               return !DecrementFailed;
            }

            /// <summary>
            /// Wert des aktuellen 1-Bits
            /// </summary>
            /// <param name="delta">Delta zur aktuellen Position</param>
            /// <returns></returns>
            public int Unit(int delta = 0) {
               return table[Math.Max(0, Math.Min(Idx + delta, table.Length - 1))].Unit;
            }

            /// <summary>
            /// aktuelle Binärbitanzahl
            /// </summary>
            /// <param name="delta">Delta zur aktuellen Position</param>
            /// <returns></returns>
            public int Bits(int delta = 0) {
               return table[Math.Max(0, Math.Min(Idx + delta, table.Length - 1))].Bits;
            }

            /// <summary>
            /// aktuelles Symbol für den Wert des 1-Bits
            /// </summary>
            /// <param name="delta">Delta zur aktuellen Position</param>
            /// <returns></returns>
            public char Symbol(int delta = 0) {
               return table[Math.Max(0, Math.Min(Idx + delta, table.Length - 1))].Symbol;
            }

            public void ClearFailedFlags() {
               IncrementFailed = DecrementFailed = false;
            }

            public override string ToString() {
               return string.Format("Idx={0}, Item={1}", Idx, table[Math.Max(0, Math.Min(table.Length - 1, Idx))]);
            }

         }

         /// <summary>
         /// Tabellenpos. für die letzte Unit des akt. Plateaus
         /// </summary>
         public int PlateauTableIdx { get; private set; }
         /// <summary>
         /// Anzahl der Binärbits für akt. Plateau
         /// </summary>
         public int PlateauBinBits { get; private set; }
         /// <summary>
         /// Plateau-Längenbitliste (nach dem Encodieren)
         /// </summary>
         List<char> PlateauUnits;
         /// <summary>
         /// die für alle <see cref="HeightElement"/> der Kachel gültige Tabelle
         /// </summary>
         public PlateauTable PlateauTable4Tile;

         static int _shrink = 1;
         /// <summary>
         /// Verkleienrungsfaktor 1, 3, 5, ...
         /// </summary>
         public static int Shrink {
            get {
               return _shrink;
            }
            set {
               if (value % 2 == 1) {
                  _shrink = value;
                  InitMax0Bits();
               } else
                  throw new Exception("Nur 1, 3, 5, ... als Shrink erlaubt.");
            }
         }

         static int _maxRealheight = 0;
         /// <summary>
         /// nur bei <see cref="Shrink"/> größer als 1 berücksichtigt
         /// </summary>
         public static int MaxRealheight {
            get {
               return _maxRealheight;
            }
            set {
               if (value > 0) {
                  _maxRealheight = value;
                  InitMax0Bits();
               }
            }
         }

         static int _maxEncoderheight;
         /// <summary>
         /// Die Werte 0 ..  zu diesem Wert können encodiert werden.
         /// </summary>
         public static int MaxEncoderheight {
            get {
               return _maxEncoderheight;
            }
            set {
               if (value > 0) {
                  _maxEncoderheight = value;
                  InitMax0Bits();
               }
            }
         }

         /// <summary>
         ///  max. mögliche Anzahl 0-Bits für die Hybrid- oder Längencodierung
         /// </summary>
         static int Max0Bits;


         public HeightElement(HeightElementInfo hi) {
            Bits = new List<byte>();

            Info = hi;

            if (Info.LastHeightElement != null)
               PlateauTable4Tile = Info.LastHeightElement.PlateauTable4Tile;
            else
               PlateauTable4Tile = new PlateauTable();

            switch (hi.Typ) {
               case Typ.ValueHookHigh:
               case Typ.ValueHookMiddle:
               case Typ.ValueHookLow:
               case Typ.PlateauFollower:
               case Typ.PlateauFollower0:
                  switch (Info.EncMode) {
                     case EncodeMode.Hybrid:
                        EncodeHybrid(Info.Data, Info.Hunit);
                        break;

                     case EncodeMode.Length0:
                        EncodeLength0(Info.Data);
                        break;

                     case EncodeMode.Length1:
                        EncodeLength1(Info.Data);
                        break;

                     case EncodeMode.Length2:
                        EncodeLength0(-Info.Data);
                        break;

                     case EncodeMode.BigBinary:
                     case EncodeMode.BigBinaryL1:
                        EncodeBigBin(Info.Data, hi.Typ == Typ.PlateauFollower);
                        break;

                     case EncodeMode.BigBinaryL2:
                        EncodeBigBin(-Info.Data, hi.Typ == Typ.PlateauFollower);
                        break;

                     default:
                        throw new Exception(string.Format("Falscher Codiertyp: {0}.", Info.EncMode));
                  }
                  break;

               case Typ.PlateauLength:
                  EncodePlateau(Info.Data, Info.Column, Info.Line, Info.LineLength);
                  break;

            }
         }


#if EXPLORERFUNCTION

         /// <summary>
         /// liefert die Bits als Text
         /// </summary>
         /// <returns></returns>
         public string GetBinText() {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Bits.Count; i++)
               sb.Append(Bits[i] != 0 ? "1" : "0");
            sb.Replace('0', '.');
            return sb.ToString();
         }

         /// <summary>
         /// liefert die Bits als Text
         /// </summary>
         /// <returns></returns>
         public string GetPlateauUnitsText() {
            return new string(PlateauUnits.ToArray());
         }

#endif

         /// <summary>
         /// codiert rein binär ohne Vorzeichen (MSB zuerst)
         /// </summary>
         /// <param name="data"></param>
         /// <param name="bitcount">Bitanzahl</param>
         void EncodeBinary(uint data, int bitcount) {
            if (bitcount == 0 && data == 0)
               return;
            int t = 1 << (bitcount - 1);
            if (data >= t << 1)
               throw new Exception(string.Format("Die Zahl {0} ist zu große für die binäre Codierung mit {1} Bits.", data, bitcount));
            while (t > 0) {
               Bits.Add((byte)((data & t) != 0 ? 1 : 0));
               t >>= 1;
            }
         }

         /// <summary>
         /// codiert hybrid
         /// </summary>
         /// <param name="data"></param>
         /// <param name="hunit"></param>
         void EncodeHybrid(int data, int hunit) {
            int hunitexp = HunitExponent(hunit);
            if (hunitexp < 0)
               throw new Exception(string.Format("Die Heightunit {0} für die Codierung {1} ist kein 2er-Potenz.", hunit, EncodeMode.Hybrid));

            // v = [0,1]
            // Delta = (2*v-1) * (m * hunit + Summe[(i=0..(n-1)]{(a(i) * 2^i} + v)
            int m = 0;
            int bin = 0;

            if (data > 0) {
               // Delta = m * hunit + 1 + Summe[(i=0..(n-1)]{(a(i) * 2^i}
               bin = (data - 1) % hunit;
               m = (data - 1 - bin) / hunit;
            } else {
               // -Delta = m * hunit + Summe[(i=0..(n-1)]{(a(i) * 2^i} 
               bin = -data % hunit;
               m = (-data - bin) / hunit;
            }

            int maxm = Max0Bits;
            if (m <= maxm) {
               EncodeLength(m);                       // längencodierten Teil speichern
               EncodeBinary((uint)bin, hunitexp);           // binär codierten Teil speichern
               Bits.Add((byte)(data > 0 ? 1 : 0));    // Vorzeichen speichern
            } else
               throw new Exception(string.Format("Der Betrag des Wertes {0} ist für die Codierung {1} bei der Maximalhöhe {2} und mit Heightunit {3} zu groß.",
                                                   data,
                                                   EncodeMode.Hybrid,
                                                   MaxEncoderheight,
                                                   hunit));
         }

         /// <summary>
         /// codiert eine "große" Zahl binär mit führender 0-Bitfolge
         /// </summary>
         /// <param name="data"></param>
         /// <param name="plateaufollower">für Plateaunachfolger</param>
         void EncodeBigBin(int data, bool plateaufollower) {
            if (data == 0)
               throw new Exception(string.Format("Der Wert 0 kann nicht in der Codierung {0} erzeugt werden.", EncodeMode.BigBinary));

            int length0 = Max0Bits + 1; // 1 Bit mehr als Max. als BigBin-Kennung
            if (plateaufollower)
               length0--;
            EncodeLength(length0); // 0-Bits und 1-Bit

            int min, max;
            if (Info.EncMode == EncodeMode.BigBinaryL1) {
               data = 1 - data; // Umwandlung, um die gleiche Codierfunktion verwendet zu können
               GetValueRangeBigBin(out min, out max);
            } else {
               GetValueRangeBigBin(out min, out max);
            }
            if (data < min || max < data) {
               if (data > 0)
                  data -= MaxEncoderheight + 1;
               else if (data < 0)
                  data += MaxEncoderheight + 1;
            }

            int bitcount = GetBigBinBits();
            byte sign = (byte)(data > 0 ? 0 : 1); // Vorzeichen (1 für <0)
            data = Math.Abs(data) - 1;
            EncodeBinary((uint)data, bitcount - 1); // pos. Wert codieren
            Bits.Add(sign);
         }

         /// <summary>
         /// Hilfsfunktion für die Längencodierung
         /// </summary>
         /// <param name="count0bits"></param>
         void EncodeLength(int count0bits) {
            for (int i = 0; i < count0bits; i++)
               Bits.Add(0);
            Bits.Add(1);
         }

         /// <summary>
         /// speichert den Wert in C0-Codierung
         /// </summary>
         /// <param name="data"></param>
         void EncodeLength0(int data) {
            EncodeLength(2 * Math.Abs(data) - (Math.Sign(data) + 1) / 2);
         }

         /// <summary>
         /// speichert den Wert in C1-Codierung
         /// </summary>
         /// <param name="data"></param>
         void EncodeLength1(int data) {
            EncodeLength(2 * Math.Abs(data - 1) + (Math.Sign(data - 1) - 1) / 2);
         }

         /// <summary>
         /// erzeugt ein Plateau der vorgegebenen Länge, aber nicht über mehrere Zeilen
         /// </summary>
         /// <param name="length">Länge des Plateaus</param>
         /// <param name="startcolumn">Spalte des Startpunktes</param>
         /// <param name="startline">Zeile des Startpunktes</param>
         /// <param name="linelength">Kachelbreite</param>
         void EncodePlateau(int length, int startcolumn, int startline, int linelength) {
            PlateauUnits = new List<char>();

            /* Wird mit den Längenbits (!) das Zeilenende erreicht oder überschritten, 
             * entfallen das nachfolgende 0-Bit und die eventuell nötigen Binärbits. 
             * Wenn mit den Längenbits das Zeilenende genau erreicht wird, wird die Startposition nicht dekrementiert! 
             * 
             * Das heißt hier:
             * Wird mit length die Zeile exakt gefüllt oder "überfüllt", werden NUR Längenbits verwendet. 0-Bit und Binärbits entfallen.
             * Wird mit length die Zeile exakt gefüllt wird außerdem die Startposition nicht dekrementiert.
             */

            int unit;

            if (startcolumn + length >= linelength) { // akt. Zeile "exakt füllen" oder "überfüllen" nur mit Längenbits

               while (startcolumn < linelength) {
                  unit = PlateauTable4Tile.Unit(); // identisch zur letzte Unit vom Vorgänger-Plateau
                  length -= unit;
                  startcolumn += unit;
                  Bits.Add(1);
                  PlateauUnits.Add(PlateauTable4Tile.Symbol());
                  PlateauTable4Tile.IncrementIdx();
               }
               if (startcolumn != linelength) {        // nicht exakt mit 1-Bits gefüllt -> Plateau ohne Trennbit und Binärbits beendet
                  if (!PlateauTable4Tile.IncrementFailed)
                     PlateauTable4Tile.DecrementIdx(); // Plateaulänge ist abgeschlossen
                  PlateauTable4Tile.ClearFailedFlags();
               } else {                                 // Plateaulänge läuft einfach weiter, Rest gilt aber für die nächste Zeile.

               }
               PlateauTableIdx = PlateauTable4Tile.Idx;

               return;

            }

            // Standard, d.h. es folgt auf der gleichen Zeile ein Plateaufollower (es kann sich auch um die Fortsetzung der Plateaulänge von der vorhergehenden Zeile handeln)
            while ((unit = PlateauTable4Tile.Unit()) <= length) {   // Die Unit ist kleiner als die restliche Länge.
               length -= unit;
               startcolumn += unit;
               Bits.Add(1);
               PlateauUnits.Add(PlateauTable4Tile.Symbol());
               PlateauTable4Tile.IncrementIdx();  // Der Index für das nächste 1-Bit wird eingestellt.
            }
            if (!PlateauTable4Tile.IncrementFailed)
               PlateauTable4Tile.DecrementIdx(); // Der zuletzt verwendete Index wird um 1 verringert (jetzt identisch mit letztem 1-Bit vom akt. Plateau).
            PlateauTable4Tile.ClearFailedFlags();

            // Basis abschließen (Trennbit)
            Bits.Add(0);

            PlateauBinBits = PlateauTable4Tile.Bits();
            if (PlateauBinBits > 0)    // Rest binär codieren
               EncodeBinary((uint)length, PlateauBinBits);

            PlateauTableIdx = PlateauTable4Tile.Idx;
         }

         /// <summary>
         /// liefert den Exponent n von 2^n oder -1, wenn hunit nicht 2^n ist
         /// </summary>
         /// <param name="hunit"></param>
         /// <returns></returns>
         static int HunitExponent(int hunit) {
            for (int i = 0; i < 32; i++) {
               int t = 1 << i;
               if ((hunit & t) != 0) {
                  if ((hunit & ~t) != 0)
                     return -1;
                  return i;
               }
            }
            return -1;
         }

         static void InitMax0Bits() {
            if (Shrink == 1) {

               if (MaxEncoderheight < 2) Max0Bits = 15;
               else if (MaxEncoderheight < 4) Max0Bits = 16;
               else if (MaxEncoderheight < 8) Max0Bits = 17;
               else if (MaxEncoderheight < 16) Max0Bits = 18;
               else if (MaxEncoderheight < 32) Max0Bits = 19;
               else if (MaxEncoderheight < 64) Max0Bits = 20;
               else if (MaxEncoderheight < 128) Max0Bits = 21;
               else if (MaxEncoderheight < 256) Max0Bits = 22;
               else if (MaxEncoderheight < 512) Max0Bits = 25;
               else if (MaxEncoderheight < 1024) Max0Bits = 28;
               else if (MaxEncoderheight < 2048) Max0Bits = 31;
               else if (MaxEncoderheight < 4096) Max0Bits = 34;
               else if (MaxEncoderheight < 8192) Max0Bits = 37;
               else if (MaxEncoderheight < 16384) Max0Bits = 40;
               else Max0Bits = 43;

            } else {
               if (MaxRealheight > 0) {
                  SortedDictionary<int, int> tmp = new SortedDictionary<int, int>();

                  int s = int_ld((Shrink - 1) / 2);
                  for (int i = 0; i < 16; i++) {
                     int v = 3 * (i + 1) + s;
                     int k = (int)Math.Pow(2, i);
                     tmp.Add(k, v);
                  }

                  for (int i = 1; i < 16; i++) {
                     int k = Shrink * ((int)Math.Pow(2, i) - 1) + 1;
                     if (k >= 65536)
                        break;
                     if (tmp.ContainsKey(k)) {
                        //int v = tmp[k];
                        //tmp.Remove(k);
                        //tmp.Add(k + 1, v);
                        tmp[k]--;
                     } else
                        tmp.Add(k, -1);
                  }

                  int[] keys = new int[tmp.Count];
                  tmp.Keys.CopyTo(keys, 0);
                  for (int i = keys.Length - 1; i >= 0; i--) {
                     if (MaxRealheight >= keys[i]) {
                        if (tmp[keys[i]] > 0)
                           Max0Bits = tmp[keys[i]];
                        else {
                           // 1 kleiner als der Vorgänger
                           Max0Bits = tmp[keys[i - 1]] - 1;
                        }
                        break;
                     }
                  }
               }
            }
         }

         static int int_ld(int v) {
            return (int)Math.Floor(Math.Log(v) / Math.Log(2));
         }

         /// <summary>
         /// liefert die Anzahl der binären Bits (einschließlich Vorzeichenbit) für BigBin-Zahlen (1 + int(ld(max)))
         /// </summary>
         /// <returns></returns>
         static int GetBigBinBits() {
            if (MaxEncoderheight < 2) return 1;
            if (MaxEncoderheight < 4) return 2;
            if (MaxEncoderheight < 8) return 3;
            if (MaxEncoderheight < 16) return 4;
            if (MaxEncoderheight < 32) return 5;
            if (MaxEncoderheight < 64) return 6;
            if (MaxEncoderheight < 128) return 7;
            if (MaxEncoderheight < 256) return 8;
            if (MaxEncoderheight < 512) return 9;
            if (MaxEncoderheight < 1024) return 10;
            if (MaxEncoderheight < 2048) return 11;
            if (MaxEncoderheight < 4096) return 12;
            if (MaxEncoderheight < 8192) return 13;
            if (MaxEncoderheight < 16384) return 14;
            return 15;
         }

         /// <summary>
         /// liefert den kleinsten und den größten verwendbaren Wert bei Hybridcodierung
         /// </summary>
         /// <param name="hunit"></param>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="max">größter verwendbarer Wert</param>
         /// <param name="iPlateauLengthBinBits">Anzahl der Binbits für die ev. vorausgehende Plateaulänge</param>
         public static void GetValueRangeHybrid(int hunit, out int min, out int max, int iPlateauLengthBinBits) {
            int lbits = Max0Bits;
            if (iPlateauLengthBinBits >= 0) // bei Plateaufollower weniger Bit erlaubt
               lbits -= iPlateauLengthBinBits + 1;
            max = (lbits + 1) * hunit;
            min = -max + 1;
         }
         /// <summary>
         /// liefert den kleinsten und den größten verwendbaren Wert bei Länge-Codierung 0
         /// </summary>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="max">größter verwendbarer Wert</param>
         /// <param name="iPlateauLengthBinBits">Anzahl der Binbits für die ev. vorausgehende Plateaulänge</param>
         public static void GetValueRangeLength0(out int min, out int max, int iPlateauLengthBinBits) {
            int lbits = Max0Bits;
            if (iPlateauLengthBinBits >= 0) // bei Plateaufollower weniger Bit erlaubt
               lbits -= iPlateauLengthBinBits + 1;
            min = -(lbits / 2);
            max = min + lbits;
         }
         /// <summary>
         /// liefert den kleinsten und den größten verwendbaren Wert bei Länge-Codierung 1
         /// </summary>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="max">größter verwendbarer Wert</param>
         /// <param name="iPlateauLengthBinBits">Anzahl der Binbits für die ev. vorausgehende Plateaulänge</param>
         public static void GetValueRangeLength1(out int min, out int max, int iPlateauLengthBinBits) {
            int lbits = Max0Bits;
            if (iPlateauLengthBinBits >= 0) // bei Plateaufollower weniger Bit erlaubt
               lbits -= iPlateauLengthBinBits + 1;
            max = lbits / 2 + 1;
            min = max - lbits;
         }
         /// <summary>
         /// liefert den kleinsten und den größten verwendbaren Wert bei Länge-Codierung 2
         /// </summary>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="max">größter verwendbarer Wert</param>
         /// <param name="iPlateauLengthBinBits">Anzahl der Binbits für die ev. vorausgehende Plateaulänge</param>
         public static void GetValueRangeLength2(out int min, out int max, int iPlateauLengthBinBits) {
            int lbits = Max0Bits;
            if (iPlateauLengthBinBits >= 0) // bei Plateaufollower weniger Bit erlaubt
               lbits -= iPlateauLengthBinBits + 1;
            max = lbits / 2;
            min = max - lbits;
         }
         /// <summary>
         /// liefert den kleinsten und den größten verwendbaren Wert bei BigBin-Codierung (an Stelle von Hybridcodierung bzw. Länge-Codierung 0)
         /// </summary>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="plateaufollower">wenn true, dann für Plateau-Nachfolger</param>
         public static void GetValueRangeBigBin(out int min, out int max) {
            int bbits = GetBigBinBits() - 1;
            max = 0;
            switch (bbits) {
               case 1: max = 2; break;
               case 2: max = 4; break;
               case 3: max = 8; break;
               case 4: max = 16; break;
               case 5: max = 32; break;
               case 6: max = 64; break;
               case 7: max = 128; break;
               case 8: max = 256; break;
               case 9: max = 512; break;
               case 10: max = 1024; break;
               case 11: max = 2048; break;
               case 12: max = 4096; break;
               case 13: max = 8192; break;
               case 14: max = 16384; break;
            }
            min = -max;
         }
         /// <summary>
         /// liefert den kleinsten und den größten verwendbaren Wert bei BigBin-Codierung (an Stelle von Länge-Codierung 1)
         /// </summary>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="max">größter verwendbarer Wert</param>
         public static void GetValueRangeBigBinL1(out int min, out int max) {
            GetValueRangeBigBin(out min, out max);
            max++;
            min++;
         }

         public override string ToString() {
            StringBuilder sb = new StringBuilder(Info.Typ.ToString());
            if (Info.Column >= 0)
               sb.Append(", Column=" + Info.Column.ToString());
            if (Info.Line >= 0)
               sb.Append(", Line=" + Info.Line.ToString());
            sb.Append(", Data=" + Info.Data.ToString());
            if (Info.Wrapped)
               sb.Append(" (Wrap)");
            if (Info.TopAligned)
               sb.Append(" (TopAligned)");
            if (Info.Typ == Typ.PlateauLength) {
               sb.Append(", PlateauTableIdx=" + PlateauTableIdx.ToString());
               sb.Append(", PlateauBinBits=" + PlateauBinBits.ToString());
            }
            sb.Append(", Encoding=" + Info.EncMode.ToString());
            if (Info.EncMode == EncodeMode.Hybrid)
               sb.Append(" (" + Info.Hunit.ToString() + ")");
            sb.Append(", Bits=");
            for (int i = 0; i < Bits.Count; i++)
               sb.Append(Bits[i] > 0 ? "1" : ".");

#if EXPLORERFUNCTION

            if (Info.Typ == Typ.PlateauLength)
               sb.Append(" " + GetPlateauUnitsText());
            else if (Info.Typ == Typ.PlateauFollower)
               sb.Append(" ddiff=" + Info.Ddiff.ToString());

#endif

            return sb.ToString();
         }

      }

      /// <summary>
      /// managed das Wrapping von Werten
      /// </summary>
      class Wraparound {

         /// <summary>
         /// oberer Grenzwert für <see cref="HeightElement.EncodeMode.Length0"/> der für das Wrapping überschritten werden muss
         /// </summary>
         readonly int L0_wrapdown;

         /// <summary>
         /// unterer Grenzwert für <see cref="HeightElement.EncodeMode.Length0"/> der für das Wrapping unterschritten werden muss
         /// </summary>
         readonly int L0_wrapup;

         /// <summary>
         /// oberer Grenzwert für <see cref="HeightElement.EncodeMode.Length1"/> der für das Wrapping überschritten werden muss
         /// </summary>
         readonly int L1_wrapdown;

         /// <summary>
         /// unterer Grenzwert für <see cref="HeightElement.EncodeMode.Length1"/> der für das Wrapping unterschritten werden muss
         /// </summary>
         readonly int L1_wrapup;

         /// <summary>
         /// oberer Grenzwert für <see cref="HeightElement.EncodeMode.Length2"/> der für das Wrapping überschritten werden muss
         /// </summary>
         readonly int L2_wrapdown;

         /// <summary>
         /// unterer Grenzwert für <see cref="HeightElement.EncodeMode.Length2"/> der für das Wrapping unterschritten werden muss
         /// </summary>
         readonly int L2_wrapup;

         /// <summary>
         /// oberer Grenzwert für <see cref="HeightElement.EncodeMode.Hybrid"/> der für ein eventuelles Wrapping mindestens überschritten werden muss
         /// </summary>
         readonly int H_wrapdown;

         /// <summary>
         /// unterer Grenzwert für <see cref="HeightElement.EncodeMode.Hybrid"/> der für ein eventuelles Wrapping mindestens unterschritten werden muss
         /// </summary>
         readonly int H_wrapup;

#if INCLUDENOTNEEDED

         /// <summary>
         /// oberer Grenzwerte für <see cref="HeightElement.EncodeMode.Hybrid"/> je Hunit, die für ein sicheres Wrapping überschritten werden müssen
         /// </summary>
         readonly SortedDictionary<int, int> H_wrapdown_safely = new SortedDictionary<int, int>();

         /// <summary>
         /// unterer Grenzwerte für <see cref="HeightElement.EncodeMode.Hybrid"/> je Hunit, die für ein sicheres Wrapping unterschritten werden müssen
         /// </summary>
         readonly SortedDictionary<int, int> H_wrapup_safely = new SortedDictionary<int, int>();

#endif

         /// <summary>
         /// Maximalhöhe der Kachel
         /// </summary>
         readonly int max;

         public Wraparound(int maxheight) {
            max = maxheight;

            // Wrapping lohnt sich NICHT, wenn: L0   -(2 * maxheight + 1) / 4 <= v <= (2 * maxheight + 3) / 4
            L0_wrapup = -((2 * maxheight + 1) / 4);
            L0_wrapdown = (2 * maxheight + 3) / 4;

            // Wrapping lohnt sich NICHT, wenn: L1   -(2 * maxheight - 1) / 4 <= v <= (2 * maxheight + 5) / 4
            L1_wrapup = -((2 * maxheight - 1) / 4);
            L1_wrapdown = (2 * maxheight + 5) / 4;

            // Wrapping lohnt sich NICHT, wenn: L2   -(2 * maxheight + 3) / 4 <= v <= (2 * maxheight + 1) / 4
            L2_wrapup = -((2 * maxheight + 3) / 4);
            L2_wrapdown = (2 * maxheight + 1) / 4;

            H_wrapdown = (maxheight + 1) / 2;
            H_wrapup = -((maxheight - 1) / 2);


            //// L0: z.B.  9 -> -4 .. +5 (max. 9 Bit)
            ////          10 -> -5 .. +5 (max. 10 Bit)
            //if (maxheight % 2 == 0) {
            //   L0_wrapdown = maxheight / 2;
            //   L0_wrapup = -maxheight / 2;
            //} else {
            //   L0_wrapdown = (maxheight + 1) / 2;
            //   L0_wrapup = -(maxheight - 1) / 2;
            //}

            //// L1: z.B.  9 -> -4 .. +5 (max. 9 Bit)
            ////          10 -> -5 .. +6 (max. 10 Bit)
            //if (maxheight % 2 == 0) {
            //   L1_wrapdown = (maxheight + 2) / 2;
            //   L1_wrapup = -maxheight / 2;
            //} else {
            //   L1_wrapdown = (maxheight + 1) / 2;
            //   L1_wrapup = -(maxheight - 1) / 2;
            //}

            //// L2: z.B.  9 -> -5 .. +4 (max. 9 Bit)
            ////          10 -> -5 .. +5 (max. 10 Bit)
            //if (maxheight % 2 == 0) {
            //   L2_wrapdown = maxheight / 2;
            //   L2_wrapup = -maxheight / 2;
            //} else {
            //   L2_wrapdown = (maxheight - 1) / 2;
            //   L2_wrapup = -(maxheight + 1) / 2;
            //}
            ////L2_wrapdown /= 4;
            ////L2_wrapup /= 4;

            //H_wrapdown = (maxheight + 1) / 2;
            //H_wrapup = -(maxheight - 1) / 2;

#if INCLUDENOTNEEDED

            H_wrapdown_safely = new SortedDictionary<int, int> {
               { 1, (maxheight + 2) / 2 }, // für hunit=1
               { 2, maxheight / 2 + 2 },
               { 4, maxheight / 2 + 4 },
               { 8, maxheight / 2 + 8 },
               { 16, maxheight / 2 + 16 },
               { 32, maxheight / 2 + 32 },
               { 64, maxheight / 2 + 64 },
               { 128, maxheight / 2 + 128 },
               { 256, maxheight / 2 + 256 }
            };

            H_wrapup_safely = new SortedDictionary<int, int> {
               { 1, -maxheight / 2 }, // für hunit=1
               { 2, -maxheight / 2 - 2 },
               { 4, -maxheight / 2 - 4 },
               { 8, -maxheight / 2 - 8 },
               { 16, -maxheight / 2 - 16 },
               { 32, -maxheight / 2 - 32 },
               { 64, -maxheight / 2 - 64 },
               { 128, -maxheight / 2 - 128 },
               { 256, -maxheight / 2 - 256 }
            };

#endif
         }

         /// <summary>
         /// ein Wert wird bei Bedarf gewrapt
         /// </summary>
         /// <param name="valdata">Wert</param>
         /// <param name="normalwrap">wenn falls, wird ein um 1 kleinerer Wrap-Wert verwendet</param>
         /// <returns>zu verwendender Wert</returns>
         public int Wrap(ValueData valdata, bool normalwrap) {
            valdata.IsWrapped = false;
            int datawrapped = int.MinValue;

            switch (valdata.Encodemode) {
               case EncodeMode.Hybrid:
                  if (valdata.Data > H_wrapdown)
                     datawrapped = WrapDown(valdata.Data, normalwrap);
                  else if (valdata.Data < H_wrapup)
                     datawrapped = WrapUp(valdata.Data, normalwrap);
                  break;

               case EncodeMode.Length0:
                  if (valdata.Data > L0_wrapdown)
                     datawrapped = WrapDown(valdata.Data, normalwrap);
                  else if (valdata.Data < L0_wrapup)
                     datawrapped = WrapUp(valdata.Data, normalwrap);
                  break;

               case EncodeMode.Length1:
                  if (valdata.Data > L1_wrapdown)
                     datawrapped = WrapDown(valdata.Data, normalwrap);
                  else if (valdata.Data < L1_wrapup)
                     datawrapped = WrapUp(valdata.Data, normalwrap);
                  break;

               case EncodeMode.Length2:
                  if (valdata.Data > L2_wrapdown)
                     datawrapped = WrapDown(valdata.Data, normalwrap);
                  else if (valdata.Data < L2_wrapup)
                     datawrapped = WrapUp(valdata.Data, normalwrap);
                  break;

               case EncodeMode.BigBinary:
               case EncodeMode.BigBinaryL2: {
                     HeightElement.GetValueRangeBigBin(out int minval, out int maxval);
                     if (valdata.Data < minval || maxval < valdata.Data) {
                        if (valdata.Data > maxval)
                           datawrapped = WrapDown(valdata.Data, normalwrap);
                        else if (valdata.Data < minval)
                           datawrapped = WrapUp(valdata.Data, normalwrap);
                     }
                  }
                  break;

               case EncodeMode.BigBinaryL1: {
                     HeightElement.GetValueRangeBigBinL1(out int minval, out int maxval);
                     if (valdata.Data < minval || maxval < valdata.Data) {
                        if (valdata.Data > maxval)
                           datawrapped = WrapDown(valdata.Data, normalwrap);
                        else if (valdata.Data < minval)
                           datawrapped = WrapUp(valdata.Data, normalwrap);
                     }
                  }
                  break;
            }

            valdata.IsWrapped = datawrapped != int.MinValue;

            return valdata.IsWrapped ? datawrapped : valdata.Data;
         }

         /// <summary>
         /// der <see cref="EncodeMode"/> wird bei Bedarf geändert
         /// <para>Die Bereichsgrenze für die BigBin-Bereiche wird NICHT überprüft.</para>
         /// </summary>
         /// <param name="data">Wert</param>
         /// <param name="em">Codierart des Wertes; danach ev. auf BigBin gesetzt</param>
         /// <param name="hunit">nur für die Codierart <see cref="HeightElement.EncodeMode.Hybrid"/> nötig</param>
         /// <param name="iPlateauLengthBinBits">Anzahl der Binbits für die ev. vorausgehende Plateaulänge</param>
         /// <returns>zu verwendender Wert</returns>
         public EncodeMode FitEncodeMode(ValueData valdata, int iHunitValue, int iPlateauLengthBinBits = -1) {
            int minval, maxval;
            switch (valdata.Encodemode) {
               case EncodeMode.Hybrid:
                  HeightElement.GetValueRangeHybrid(iHunitValue, out minval, out maxval, iPlateauLengthBinBits);
                  return valdata.Data < minval || maxval < valdata.Data ?
                                 EncodeMode.BigBinary :
                                 valdata.Encodemode;

               case EncodeMode.Length0:
                  HeightElement.GetValueRangeLength0(out minval, out maxval, iPlateauLengthBinBits);
                  return valdata.Data < minval || maxval < valdata.Data ?
                                 EncodeMode.BigBinary :
                                 valdata.Encodemode;

               case EncodeMode.Length1:
                  HeightElement.GetValueRangeLength1(out minval, out maxval, iPlateauLengthBinBits);
                  return valdata.Data < minval || maxval < valdata.Data ?
                                 EncodeMode.BigBinaryL1 :
                                 valdata.Encodemode;

               case EncodeMode.Length2:
                  HeightElement.GetValueRangeLength2(out minval, out maxval, iPlateauLengthBinBits);
                  return valdata.Data < minval || maxval < valdata.Data ?
                                 EncodeMode.BigBinaryL2 :
                                 valdata.Encodemode;

               default:
                  return valdata.Encodemode;
            }
         }

#if INCLUDENOTNEEDED

         /// <summary>
         /// liefert die Anzahl der nötigen 0-Bits
         /// </summary>
         /// <param name="data"></param>
         /// <param name="hunit"></param>
         /// <returns></returns>
         int GetHunit0Bits(int data, int hunit) {
            if (data > 0)
               return (data - 1) / hunit;
            else if (data < 0)
               return -data / hunit;
            return 0;
         }

#endif

         int WrapDown(int data, bool normalwrap) {
            return data -= max + (normalwrap ? 1 : 0);
         }

         int WrapUp(int data, bool normalwrap) {
            return data += max + (normalwrap ? 1 : 0);
         }

         public override string ToString() {
            return string.Format("Max. {0}, ohne Wrap: L0 {1}..{2}, L1 {3}..{4}, L2 {5}..{6}, H {7}..{8}",
                                 max,
                                 L0_wrapup, L0_wrapdown,
                                 L1_wrapup, L1_wrapdown,
                                 L2_wrapup, L2_wrapdown,
                                 H_wrapup, H_wrapdown);
         }

      }

      #endregion

      #region CodingType-Klassen

      public abstract class CodingType {

         /// <summary>
         /// akt. Codierungsart
         /// </summary>
         public EncodeMode EncodeMode { get; protected set; }

         /// <summary>
         /// liefert den Wert (immer einer 2er-Potenz)
         /// </summary>
         public int HunitValue { get; protected set; }

         /// <summary>
         /// liefert den Exponent der 2er-Potenz zu <see cref="HunitValue"/>
         /// </summary>
         public int HunitExponent { get; protected set; }

         /// <summary>
         /// akt. Summe für die Hybridcodierung
         /// </summary>
         public int SumH { get; protected set; }

         /// <summary>
         /// akt. Summe der Bewertungen für die Art der Längencodierung
         /// </summary>
         public int SumL { get; protected set; }

         /// <summary>
         /// akt. Anzahl Elemente
         /// </summary>
         public int ElemCount { get; protected set; }

#if EXPLORERFUNCTION

         /// <summary>
         /// Bonus für <see cref="SumL"/> zur Entscheidung, welche Codierung verwendet wird (i.A. 0)
         /// </summary>
         public int Bonus { get; protected set; }

#endif

         /// <summary>
         /// "bewerteter" Wert aus dem Datenwert für die Längencodierung
         /// </summary>
         public int Eval { get; protected set; }

         protected int maxheightdiff;

         protected int unitdelta;


         public CodingType() {
            EncodeMode = EncodeMode.Hybrid;
            SumH = SumL = 0;
            ElemCount = 0;
            HunitValue = 0;
            HunitExponent = 0;

#if EXPLORERFUNCTION

            ExtInfo4LastAdd = "";

#endif
         }

         /// <summary>
         /// bildet <see cref="CodingType"/> für die Hybridcodierung
         /// </summary>
         /// <param name="maxheightdiff">max. Höhendiff.</param>
         public CodingType(int maxheightdiff) : this() {
            if (maxheightdiff < 0)
               throw new Exception("Der Wert von maxHeightdiff kann nicht kleiner 0 sein.");

            this.maxheightdiff = maxheightdiff;
            HunitValue = GetHeightUnit4MaxHeight(maxheightdiff);
            unitdelta = GetHeightUnitDelta(maxheightdiff);
            ElemCount = 0;
            SumH = 0;
            SumL = 0;
         }

         /// <summary>
         /// erzeugt eine Kopie
         /// </summary>
         /// <param name="ct"></param>
         public CodingType(CodingType ct) {
            maxheightdiff = ct.maxheightdiff;
            HunitValue = ct.HunitValue;
            HunitExponent = ct.HunitExponent;
            SumH = ct.SumH;
            SumL = ct.SumL;
            ElemCount = ct.ElemCount;
            EncodeMode = ct.EncodeMode;
         }

         abstract public void AddValue(int data);

#if EXPLORERFUNCTION

         /// <summary>
         /// zusätzliche Info für das letzte <see cref="AddValue(int)"/>
         /// </summary>
         public string ExtInfo4LastAdd { get; protected set; }

#endif

         /// <summary>
         /// setzt die <see cref="HeightUnit"/> neu
         /// </summary>
         /// <param name="abssum">bisherige Summe</param>
         /// <param name="elemcount">Anzahl der Elemente</param>
         /// <param name="bStd">Standardverfahren (sonst für Plateau)</param>
         protected void SetHunit4SumAndElemcount(int abssum, int elemcount, bool bStd) {
            int counter = abssum + unitdelta;
            int denominator = elemcount;

            if (bStd) {

               // hu = (sum + 1) / (vg + 1)
               counter++;
               denominator++;

            } else {

               // hu = (sum + 1 - int(vg/2) / (vg + 1)
               counter++;
               counter -= elemcount / 2;
               denominator++;

            }

            SetHunitValue(counter >= denominator ? counter / denominator : 0);
         }

         /// <summary>
         /// liefert den <see cref="HeightUnit"/>-Wert für die max. Höhendiff.
         /// </summary>
         /// <param name="maxheight"></param>
         /// <returns></returns>
         static public int GetHeightUnit4MaxHeight(int maxheight) {
            if (maxheight < 0x9f)
               return 1;
            else if (maxheight < 0x11f)
               return 2;
            else if (maxheight < 0x21f)
               return 4;
            else if (maxheight < 0x41f)
               return 8;
            else if (maxheight < 0x81f)
               return 16;
            else if (maxheight < 0x101f)
               return 32;
            else if (maxheight < 0x201f)
               return 64;
            else if (maxheight < 0x401f)
               return 128;
            return 256;
         }

         /// <summary>
         /// liefert den "Deltawert" für die Abssum-Tabelle für die max. Höhendiff. (0, ...)
         /// </summary>
         /// <param name="maxheight">max. Höhendiff.</param>
         /// <returns></returns>
         static public int GetHeightUnitDelta(int maxheight) {
            return Math.Max(0, maxheight - 0x5f) / 0x40;
         }

         /// <summary>
         /// Bewertung des neuen Wertes für Längencodierung
         /// </summary>
         /// <param name="oldsum">bisherige Summe</param>
         /// <param name="elemcount">bisher registrierte Elementanzahl</param>
         /// <param name="newdata">neuer Wert</param>
         /// <param name="region">Bereich in dem newdata liegt (0 ist der niedrigste)</param>
         /// <returns></returns>
         protected int EvaluateData(int oldsum, int elemcount, int newdata, ref int region) {
            /*
               D < -2 – (ls + 3*k)/2   -1 – ls – k	
               D < 0 – (ls + k)/2      2*(d + k) + 3	
               D < 2 – (ls – k)/2      2*d – 1	
               D < 4 – (ls – 3*k)/2    2*(d – k) - 5	
                                       1 – ls + k	
             */

            if (region < 0)
               region = GetEvaluateDataRegion(oldsum, elemcount, newdata);

            switch (region) {
               case 0:
                  return -1 - oldsum - elemcount;

               case 1:
                  return 2 * (newdata + elemcount) + 3;

               case 2:
                  return 2 * newdata - 1;

               case 3:
                  return 2 * (newdata - elemcount) - 5;

               default:
                  return 1 - oldsum + elemcount;
            }
         }

         protected int GetEvaluateDataRegion(int oldsum, int elemcount, int newdata) {
            /*
               D < -2 – (ls + 3*k)/2   -1 – ls – k	
               D < 0 – (ls + k)/2      2*(d + k) + 3	
               D < 2 – (ls – k)/2      2*d – 1	
               D < 4 – (ls – 3*k)/2    2*(d – k) - 5	
                                       1 – ls + k	
             */

            if (elemcount < 63) {

               if (newdata < -2 - ((oldsum + 3 * elemcount) >> 1)) {
                  return 0;
               } else if (newdata < -((oldsum + elemcount) >> 1)) {
                  return 1;
               } else if (newdata < 2 - ((oldsum - elemcount) >> 1)) {
                  return 2;
               } else if (newdata < 4 - ((oldsum - 3 * elemcount) >> 1)) {
                  return 3;
               } else {
                  return 4;
               }

            } else {

               if (newdata < -2 - ((oldsum + 3 * elemcount) >> 1)) {
                  return 0;
               } else if (newdata < -((oldsum + elemcount) >> 1) - 1) {    // <-- Sonderfall bei "Halbierung"
                  return 1;
               } else if (newdata < 2 - ((oldsum - elemcount) >> 1)) {
                  return 2;
               } else if (newdata < 4 - ((oldsum - 3 * elemcount) >> 1)) {
                  return 3;
               } else {
                  return 4;
               }

            }
         }

         /// <summary>
         /// setzt <see cref="CodingType"/> für die Hybridcodierung auf die größte 2er-Potenz, die nicht größer als <see cref="value"/> ist
         /// </summary>
         /// <param name="hunitvalue"></param>
         protected void SetHunitValue(int hunitvalue) {
            if (hunitvalue < 1) {
               HunitValue = 0;
               HunitExponent = 0;
               //throw new Exception("SetHunitValue() ist für 0 nicht möglich.");
            } else {
               if (hunitvalue < 2)
                  HunitExponent = 0;
               else if (hunitvalue < 4)
                  HunitExponent = 1;
               else if (hunitvalue < 8)
                  HunitExponent = 2;
               else if (hunitvalue < 16)
                  HunitExponent = 3;
               else if (hunitvalue < 32)
                  HunitExponent = 4;
               else if (hunitvalue < 64)
                  HunitExponent = 5;
               else if (hunitvalue < 128)
                  HunitExponent = 6;
               else if (hunitvalue < 256)
                  HunitExponent = 7;
               else if (hunitvalue < 512)
                  HunitExponent = 8;
               else if (hunitvalue < 1024)
                  HunitExponent = 9;
               else if (hunitvalue < 2048)
                  HunitExponent = 10;
               else if (hunitvalue < 4096)
                  HunitExponent = 11;
               else if (hunitvalue < 8192)
                  HunitExponent = 12;
               else if (hunitvalue < 16384)
                  HunitExponent = 13;
               else if (hunitvalue < 32768)
                  HunitExponent = 14;
               else if (hunitvalue < 65536)
                  HunitExponent = 15;
               else
                  HunitExponent = 16;

               HunitValue = 1 << HunitExponent;
            }
         }

         public override string ToString() {
            StringBuilder sb = new StringBuilder(EncodeMode.ToString());
            sb.Append(": ElemCount=");
            sb.Append(ElemCount.ToString());
            if (EncodeMode == EncodeMode.Hybrid) {
               sb.Append(", HunitValue=");
               sb.Append(HunitValue.ToString());
               sb.Append(", HunitExponent=");
               sb.Append(HunitExponent.ToString());
               sb.Append(", SumH=");
               sb.Append(SumH.ToString());
            } else {
               sb.Append(", SumL=");
               sb.Append(SumL.ToString());
            }
            return sb.ToString();
         }

      }

      class CodingTypeStd : CodingType {

         readonly bool whithoutL1;

         /// <summary>
         /// bildet <see cref="CodingType"/> für die Standard-Codierung
         /// </summary>
         /// <param name="maxheightdiff">max. Höhendiff.</param>
         /// <param name="whithoutL1"></param>
         public CodingTypeStd(int maxheightdiff, bool whithoutL1) : base(maxheightdiff) {
            this.whithoutL1 = whithoutL1;
         }

#if INCLUDENOTNEEDED

         /// <summary>
         /// erzeugt eine Kopie
         /// </summary>
         /// <param name="ct"></param>
         public CodingTypeStd(CodingTypeStd ct) : base(ct) { }

#endif

         override public void AddValue(int data) {
            addValue(data);
         }

         void addValue(int data) {

#if EXPLORERFUNCTION
            ExtInfo4LastAdd = "";
#endif

            int dh = data > 0 ? data :
                                -data;

            int evalregion = -1;
            if (ElemCount == 63) {     // besondere Bewertung bei "Halbierung" durch Manipulation von data; SumL bei ElemCount == 63 ist immer ungerade
               evalregion = GetEvaluateDataRegion(SumL, ElemCount, data);

               bool datagerade = data % 2 == 0;
               bool SumL1 = (SumL - 1) % 4 == 0;

               switch (evalregion) {
                  case 0:
                  case 2:
                  case 4:
                     if ((SumL1 && !datagerade) ||
                         (!SumL1 && datagerade)) {
                        data++;
#if EXPLORERFUNCTION
                        ExtInfo4LastAdd += ";data++";
#endif
                     }
                     break;
                  case 1:
                     data++;
#if EXPLORERFUNCTION
                     ExtInfo4LastAdd += ";data++";
#endif
                     if ((SumL1 && !datagerade) ||
                         (!SumL1 && datagerade)) {
                        data++;
#if EXPLORERFUNCTION
                        ExtInfo4LastAdd += ";data++";
#endif
                     }
                     break;
                  case 3:
                     if ((SumL1 && datagerade) ||
                         (!SumL1 && !datagerade)) {
                        data--;
#if EXPLORERFUNCTION
                        ExtInfo4LastAdd += ";data--";
#endif
                     }
                     break;
               }


            }
            Eval = EvaluateData(SumL, ElemCount, data, ref evalregion);

            SumH += dh;
            if (SumH + unitdelta + 1 >= 0xFFFF)
               SumH -= 0x10000;

            SumL += Eval;

            // ---- ElemCount aktualisieren ----
            ElemCount++;

            // ---- Korrektur der Werte bei großem ElemCount ----
            if (ElemCount == 64) {
               ElemCount = 32;

               SumH = ((SumH - unitdelta) >> 1) - 1;

               SumL /= 2;
            }

            // ---- Hunit ermitteln ----
            SetHunit4SumAndElemcount(SumH, ElemCount, true);

            // ---- EncodeMode neu setzen ----
            if (HunitValue > 0)
               EncodeMode = EncodeMode.Hybrid;
            else
               EncodeMode = SumL > 0 && !whithoutL1 ? EncodeMode.Length1 : EncodeMode.Length0;

#if EXPLORERFUNCTION

            if (ExtInfo4LastAdd.Length > 0)
               ExtInfo4LastAdd = ExtInfo4LastAdd.Substring(1);

#endif
         }

      }

      class CodingTypePlateauFollowerNotZero : CodingType {

         /// <summary>
         /// bildet <see cref="CodingType"/> für die Codierung der Plateaufollower mit ddiff ungleich 0
         /// </summary>
         /// <param name="maxheightdiff">max. Höhendiff.</param>
         public CodingTypePlateauFollowerNotZero(int maxheightdiff) : base(maxheightdiff) { }

#if INCLUDENOTNEEDED

         /// <summary>
         /// erzeugt eine Kopie
         /// </summary>
         /// <param name="ct"></param>
         public CodingTypePlateauFollowerNotZero(CodingTypePlateauFollowerNotZero ct) : base(ct) { }

#endif

         override public void AddValue(int data) {

#if EXPLORERFUNCTION

            ExtInfo4LastAdd = "";

#endif

            // ---- SumH aktualisieren ----
            SumH += Math.Abs(data);
            if (SumH + unitdelta + 1 >= 0xFFFF)
               SumH -= 0x10000;

            // ---- ElemCount aktualisieren ----
            ElemCount++;

            // ---- SumL aktualisieren ----
            Eval = data > 0 ? 1 : -1;
            SumL += Eval;

            // ---- Korrektur der Werte bei großem ElemCount ----
            if (ElemCount == 64) {
               ElemCount = 32;

               SumH = ((SumH - unitdelta) >> 1) - 1;

               SumL /= 2;
               if (SumL % 2 != 0) {
                  SumL--;

#if EXPLORERFUNCTION

                  ExtInfo4LastAdd = ";SumL--";

#endif
               }
            }

            // ---- Hunit ermitteln ----
            SetHunit4SumAndElemcount(SumH, ElemCount, true);

            // ---- EncodeMode neu setzen ----
            if (HunitValue > 0)
               EncodeMode = EncodeMode.Hybrid;
            else
               EncodeMode = SumL > 0 ? EncodeMode.Length0 : EncodeMode.Length2;

#if EXPLORERFUNCTION

            if (ExtInfo4LastAdd.Length > 0)
               ExtInfo4LastAdd = ExtInfo4LastAdd.Substring(1);

#endif

         }

      }

      class CodingTypePlateauFollowerZero : CodingType {

         /// <summary>
         /// bildet <see cref="CodingType"/> für die Codierung der Plateaufollower mit ddiff=0
         /// </summary>
         /// <param name="maxheighdiff">max. Höhendiff.</param>
         public CodingTypePlateauFollowerZero(int maxheightdiff) : base(maxheightdiff) { }

#if INCLUDENOTNEEDED

         /// <summary>
         /// erzeugt eine Kopie
         /// </summary>
         /// <param name="ct"></param>
         public CodingTypePlateauFollowerZero(CodingTypePlateauFollowerZero ct) : base(ct) { }

#endif

         override public void AddValue(int data) {

#if EXPLORERFUNCTION

            ExtInfo4LastAdd = "";

#endif

            // ---- SumH aktualisieren ----
            if (data > 0)
               SumH += data;
            else
               SumH += 1 - data;
            if (SumH + unitdelta + 1 >= 0xFFFF)
               SumH -= 0x10000;

            Eval = data <= 0 ? -1 : 1;
            SumL += Eval;

            // ---- ElemCount aktualisieren ----
            ElemCount++;

            // ---- Korrektur der Werte bei großem ElemCount ----
            if (ElemCount == 64) {
               ElemCount = 32;

               SumH = ((SumH - unitdelta) >> 1) - 1;

               SumL /= 2;
               if (SumL % 2 != 0) {
                  SumL++;

#if EXPLORERFUNCTION

                  ExtInfo4LastAdd += ";SumL++";

#endif
               }
            }

            // ---- Hunit ermitteln ----
            SetHunit4SumAndElemcount(SumH, ElemCount, false);

            //Debug.WriteLine(string.Format(", ElemCount={0}", ElemCount));

            // ---- EncodeMode neu setzen ----
            if (HunitValue > 0)
               EncodeMode = EncodeMode.Hybrid;
            else
               EncodeMode = SumL < 0 ? EncodeMode.Length0 : EncodeMode.Length1;

#if EXPLORERFUNCTION

            if (ExtInfo4LastAdd.Length > 0)
               ExtInfo4LastAdd = ExtInfo4LastAdd.Substring(1);

#endif
         }

      }

      #endregion

      /// <summary>
      /// Art der Codierung
      /// </summary>
      public enum EncodeMode {
         /// <summary>
         /// noch nicht definiert
         /// </summary>
         notdefined,
         /// <summary>
         /// hybride Codierung
         /// </summary>
         Hybrid,
         /// <summary>
         /// Längencodierung Variante 0
         /// </summary>
         Length0,
         /// <summary>
         /// Längencodierung Variante 1
         /// </summary>
         Length1,
         /// <summary>
         /// Längencodierung Variante 2 (Negation von L0)
         /// </summary>
         Length2,
         /// <summary>
         /// Codierung im "festen" Binärformat (für große Zahlen an Stelle der <see cref="Hybrid"/>- und <see cref="Length0"/>-Codierung)
         /// </summary>
         BigBinary,
         /// <summary>
         /// Codierung im "festen" Binärformat (für große Zahlen an Stelle der <see cref="Length1"/>-Codierung)
         /// </summary>
         BigBinaryL1,
         /// <summary>
         /// Codierung im "festen" Binärformat (für große Zahlen an Stelle der <see cref="Length2"/>-Codierung)
         /// </summary>
         BigBinaryL2,

         /// <summary>
         /// Spezialcodierung für Plateaulänge
         /// </summary>
         PlateauLength,
      }


      /// <summary>
      /// nächste Position für Höhen und Daten
      /// </summary>
      Position nextPosition;

      /// <summary>
      /// max. zulässige Höhe der Kachel unge"shrinkt"
      /// </summary>
      public int MaxRealHeight { get; protected set; }

      /// <summary>
      /// max. zulässige Höhe der Kachel
      /// </summary>
      public int MaxHeight { get; protected set; }

      /// <summary>
      /// Kachelbreite
      /// </summary>
      public int TileSizeHorz { get; protected set; }

      /// <summary>
      /// Kachelhöhe
      /// </summary>
      public int TileSizeVert { get; protected set; }

      /// <summary>
      /// zur Bestimmung der Heightunit für die Gruppe der Standardwerte
      /// </summary>
      readonly CodingTypeStd ct_std;

      /// <summary>
      /// zur Bestimmung der Heightunit für die Gruppe der Plateau-Nachfolger mit ddiff=0
      /// </summary>
      readonly CodingTypePlateauFollowerZero ct_ddiff4plateaufollower_zero;

      /// <summary>
      /// zur Bestimmung der Heightunit für die Gruppe der Plateau-Nachfolger mit ddiff!=0
      /// </summary>
      readonly CodingTypePlateauFollowerNotZero ct_ddiff4plateaufollower_notzero;

      Wraparound ValueWrap;


      /// <summary>
      /// Liste der registrierten Höhenelemente
      /// </summary>
      public List<HeightElement> Elements { get; private set; }

#if EXPLORERFUNCTION

      public List<string> CodingTypeStd_Info { get; private set; }

      public List<string> CodingTypePlateauFollowerNotZero_Info { get; private set; }

      public List<string> CodingTypePlateauFollowerZero_Info { get; private set; }


      /// <summary>
      /// akt. Codierungs-Art (Codierung, die vom letzten <see cref="HeightElement"/> geliefert wird)
      /// </summary>
      public EncodeMode ActualMode {
         get {
            return Elements.Count > 0 ?
                        Elements[Elements.Count - 1].Info.EncMode :
                        EncodeMode.notdefined;
         }
      }

      /// <summary>
      /// Codiertyp (z.Z. nicht verwendet)
      /// </summary>
      public byte Codingtype { get; protected set; }

      /// <summary>
      /// hunit bei der Init. des Encoders (abh. von der Maximalhöhe; konstant; max. 256)
      /// </summary>
      CodingTypeStd initialHeightUnit;

      /// <summary>
      /// akt. hunit
      /// </summary>
      public int HeightUnit { get; private set; }

      /// <summary>
      /// liefert die akt. Höhe
      /// </summary>
      public int ActualHeight {
         get {
            return nextPosition.Idx > 0 ? RealHeightValues.Get(nextPosition.Idx - 1) : 0;
         }
      }

#endif

      /// <summary>
      /// zum einfacheren Testen des Verhaltens beim Shrink
      /// <para>Alle Funktionen können auch für "ungeshrinkte" Werte verwendet werden.</para>
      /// </summary>
      public class Shrink {

         /// <summary>
         /// Shrink-Faktor (ungerade)
         /// </summary>
         public int ShrinkValue { get; private set; }

         /// <summary>
         /// max. reale Höhe
         /// </summary>
         public int MaxRealheight { get; private set; }

         /// <summary>
         /// max. Höhe für den Encoder
         /// </summary>
         public int MaxEncoderheight { get; private set; }

         /// <summary>
         /// true wenn die top-Ausrichtung möglich ist, d.h. überhaupt ein shrink erfolgt, bei dem nicht maxenc direkt auf maxdiff trifft
         /// </summary>
         public bool TopAlignIsPossible { get; private set; }

         /// <summary>
         /// reale Höhendaten müssen noch verkleinert werden (sonst werden die Originaldaten z.B. für Tests verwendet)
         /// </summary>
         public bool ShrinkHeightsNecessary { get; private set; }

         /// <summary>
         /// Differenz zwischen theoretischen Maximalwert (<see cref="MaxEncoderheight"/> * <see cref="ShrinkValue"/>) und <see cref="MaxRealheight"/>
         /// </summary>
         readonly int delta;

         #region interne Hilfsklassen

         class TopAlignedArray {

            readonly SetType[,] dat;

            public int Width, Height;

            enum SetType : byte {
               Unknown = 0,
               No,
               Yes,
            }

            public TopAlignedArray(ushort width, ushort height) {
               Width = width;
               Height = height;
               dat = new SetType[Width, Height];
            }

            /// <summary>
            /// setzt die einzelne Position
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="top"></param>
            public void Set(int x, int y, bool top) {
               if (0 <= x && x < Width &&
                   0 <= y && y < Height)
                  dat[x, y] = top ? SetType.Yes : SetType.No;
            }

            public bool Get(int x, int y) {
               if (x < -1 || y < -1 || Width <= x || Height <= y)
                  return false;

               if (x >= 0 && y >= 0)                  // Standard
                  return dat[x, y] == SetType.Yes;
               if (x < 0 && y > 0)                    // virt. Spalte (Übernahme von "rechts oben")
                  return dat[0, y - 1] == SetType.Yes;
               else                                   // virt. Zeile (immer No)
                  return false;
            }

            public string ToString(int y) {
               StringBuilder sb = new StringBuilder();
               for (int x = 0; x < Width; x++)
                  switch (dat[x, y]) {
                     case SetType.No: sb.Append("N"); break;
                     case SetType.Yes: sb.Append("Y"); break;
                     default: sb.Append("?"); break;
                  }
               return sb.ToString();
            }

            public override string ToString() {
               StringBuilder sb = new StringBuilder();
               for (int y = 0; y < Height; y++)
                  sb.AppendLine(ToString(y));
               return sb.ToString();
            }
         }

         class SurelyAlignedArray {

            enum SurelyAligned {
               /// <summary>
               /// Position ohne Wert
               /// </summary>
               Nothing,
               /// <summary>
               /// sicher topaligned (Wert ist <see cref="MaxEncoderheight"/> oder Hook ist größer oder gleich <see cref="MaxEncoderheight"/>)
               /// </summary>
               Top,
               /// <summary>
               /// (Wert und Hook sind zwischen 0 und <see cref="MaxEncoderheight"/>)
               /// </summary>
               Between,
               /// <summary>
               /// sicher NICHT topaligned (Wert ist 0 oder Hook ist kleiner oder gleich 0)
               /// </summary>
               Bottom,
            }

            /// <summary>
            /// Topaligned-Werte
            /// </summary>
            readonly SurelyAligned[,] data;

            public readonly int Width;
            public readonly int Height;

            readonly int max;


            public SurelyAlignedArray(ushort width, ushort height, int maxvalue) {
               max = maxvalue;
               Width = width;
               Height = height;
               data = new SurelyAligned[width, height];
               for (int y = 0; y < height; y++)
                  for (int x = 0; x < width; x++)
                     data[x, y] = SurelyAligned.Nothing;
            }

            public void AddValue(ValueData vd) {
               data[vd.X, vd.Y] = GetStatus(vd);
            }

            SurelyAligned GetStatus(ValueData vd) {
               if (vd.Height == max)
                  return SurelyAligned.Top;
               if (vd.Height == 0)
                  return SurelyAligned.Bottom;
               if (vd.Hook >= max)
                  return SurelyAligned.Top;
               if (vd.Hook <= 0)
                  return SurelyAligned.Bottom;
               return SurelyAligned.Between;
            }

            /// <summary>
            /// Ist der Wert mit Sicherheit topaligned (abgeleitet aus dem Hook und dem Wert)?
            /// </summary>
            /// <param name="vd"></param>
            /// <returns></returns>
            public bool IsSurelyTopAligned(ValueData vd) {
               return GetStatus(vd) == SurelyAligned.Top;
            }

            /// <summary>
            /// Ist der Wert mit Sicherheit downaligned (abgeleitet aus dem Hook und dem Wert)?
            /// </summary>
            /// <param name="vd"></param>
            /// <returns></returns>
            public bool IsSurelyBottomAlignedNo(ValueData vd) {
               return GetStatus(vd) == SurelyAligned.Bottom;
            }

            /// <summary>
            /// liefert nur dann true, wenn in der Zeile ein <see cref="SurelyAligned.Bottom"/> (vor einem ev. vorhandenen <see cref="SurelyAligned.Top"/>) gefunden wird
            /// <para>Die Suche erfolgt nach links.</para>
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public bool HasOnLine_SurelyBottomAligned(int x, int y) {
               for (int linepos = x - 1; linepos >= 0; linepos--) {
                  switch (data[linepos, y]) {
                     case SurelyAligned.Top:
                        return false;
                     case SurelyAligned.Bottom:
                        return true;
                  }
               }
               return false;
            }

            /// <summary>
            /// liefert nur dann true, wenn in der Zeile ein <see cref="SurelyAligned.Top"/> (vor einem ev. vorhandenen <see cref="SurelyAligned.Bottom"/>) gefunden wird
            /// <para>Die Suche erfolgt nach links.</para>
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public bool HasOnLine_SurelyTopAligned(int x, int y) {
               for (int linepos = x - 1; linepos >= 0; linepos--) {
                  switch (data[linepos, y]) {
                     case SurelyAligned.Bottom:
                        return false;
                     case SurelyAligned.Top:
                        return true;
                  }
               }
               return false;
            }

            /// <summary>
            /// liefert nur dann true, wenn in der Spalte ein <see cref="SurelyAligned.Bottom"/> (vor einem ev. vorhandenen <see cref="SurelyAligned.Top"/>) gefunden wird
            /// <para>Die Suche erfolgt nach oben.</para>
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public bool HasOnColumn_SurelyBottomAligned(int x, int y) {
               for (int colpos = y - 1; colpos >= 0; colpos--) {
                  switch (data[x, colpos]) {
                     case SurelyAligned.Top:
                        return false;
                     case SurelyAligned.Bottom:
                        return true;
                  }
               }
               return false;
            }

            /// <summary>
            /// liefert nur dann true, wenn in der Spalte ein <see cref="SurelyAligned.Top"/> (vor einem ev. vorhandenen <see cref="SurelyAligned.Bottom"/>) gefunden wird
            /// <para>Die Suche erfolgt nach oben.</para>
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public bool HasOnColumn_SurelyTopAligned(int x, int y) {
               for (int colpos = y - 1; colpos >= 0; colpos--) {
                  switch (data[x, colpos]) {
                     case SurelyAligned.Bottom:
                        return false;
                     case SurelyAligned.Top:
                        return true;
                  }
               }
               return false;
            }

         }

         #endregion

         SurelyAlignedArray SurelyAlignedData;

         TopAlignedArray TopAlignedData;

         /// <summary>
         /// reale Höhendaten
         /// </summary>
         DataArray realdata;


         /// <summary>
         /// 
         /// </summary>
         /// <param name="enc">zugehöriger Encoder</param>
         /// <param name="width">Spaltenanzahl des Datenbereiches</param>
         /// <param name="height">Zeilenanzahl des Datenbereiches</param>
         /// <param name="shrink">Verkleinerungsfaktor (1, 3, 5, ...)</param>
         /// <param name="maxrealheight">max. reale Höhe</param>
         /// <param name="maxencoderheight">max. Höhe mit der der Encoder rechnet</param>
         /// <param name="shrinkheightsnecessary">true, wenn reale (noch nicht verkleinerte) Höhen geliefert werden</param>
         public Shrink(DataArray data, ushort width, ushort height, int shrink, int maxrealheight, int maxencoderheight = 0, bool shrinkheightsnecessary = false) {
            this.realdata = data;
            ShrinkValue = shrink;
            MaxRealheight = maxrealheight;

            if (ShrinkValue > 1) {
               if (maxencoderheight > 0)
                  MaxEncoderheight = maxencoderheight;
               else
                  MaxEncoderheight = 1 + (maxrealheight - 1) / ShrinkValue;
            } else
               MaxEncoderheight = MaxRealheight;

            delta = MaxEncoderheight * ShrinkValue - MaxRealheight;

            TopAlignIsPossible = ShrinkValue > 1 && delta > ShrinkValue / 2;  // nur unter diesen Bedingungen kann es eine top-Ausrichtung geben

            if (ShrinkValue > 1)
               ShrinkHeightsNecessary = shrinkheightsnecessary;
            else
               ShrinkHeightsNecessary = false;

            TopAlignedData = new TopAlignedArray(width, height);
            SurelyAlignedData = new SurelyAlignedArray(width, height, MaxEncoderheight);
         }

         /// <summary>
         /// berechnet für den realen Höhenwert den Wert für den Encoder
         /// <para>Es wird nur dann der korrekte Wert geliefert, wenn <see cref="TopAlignedData"/> bis direkt vor diese Position schon korrekt ermittelt wurde. Das ist erst bei der 
         /// Encodierung der Fall.</para>
         /// </summary>
         /// <param name="x"></param>
         /// <param name="y"></param>
         /// <param name="realheight"></param>
         /// <returns></returns>
         protected int GetEncoderHeight4RealHeight(int x, int y, int realheight) {
            if (ShrinkHeightsNecessary) {
               if (!IsTopAligned(x, y)) {

                  if (realheight < MaxRealheight - (ShrinkValue - delta) / 2)
                     realheight = (realheight + ShrinkValue / 2) / ShrinkValue;
                  else
                     realheight = MaxEncoderheight;

               } else {

                  if (realheight <= (ShrinkValue - delta) / 2)
                     realheight = 0;
                  else
                     realheight = (realheight + delta) / ShrinkValue;

               }
            }
            return realheight;
         }

         /// <summary>
         /// liefert den Höhenwert für den Encoder
         /// <para>Es wird nur dann der korrekte Wert geliefert, wenn <see cref="TopAlignedData"/> bis direkt vor diese Position schon korrekt ermittelt wurde. Das ist erst bei der 
         /// Encodierung der Fall!</para>
         /// </summary>
         /// <param name="x"></param>
         /// <param name="y"></param>
         /// <returns></returns>
         public int GetEncoderHeightValue(int x, int y) {
            return GetEncoderHeight4RealHeight(x, y, realdata.Get(x, y));
         }

         /// <summary>
         /// setzt die Ausrichtung für diesen Wert (und liefert sie)
         /// </summary>
         /// <param name="valenv"></param>
         /// <returns></returns>
         public bool SetTopAligned4Normal(ValueData valenv) {
            if (TopAlignIsPossible) {
               bool topaligned = false;
               if (valenv.Height == 0) {
                  topaligned = false;
               } else if (valenv.Height == MaxEncoderheight) {
                  topaligned = true;
               } else {
                  switch (valenv.Alignment) {
                     case Align3Type.TA111:
                     case Align3Type.TA100:
                     case Align3Type.TA010:
                     case Align3Type.TA001:
                        if (0 < valenv.Hook)
                           topaligned = true;
                        break;

                     case Align3Type.TA110:
                     case Align3Type.TA000:
                     case Align3Type.TA101:
                     case Align3Type.TA011:
                        if (MaxEncoderheight <= valenv.Hook)
                           topaligned = true;
                        break;
                  }
               }
               TopAlignedData.Set(valenv.X, valenv.Y, topaligned);
               SurelyAlignedData.AddValue(valenv);
               return topaligned;
            }
            return false;
         }

         /// <summary>
         /// setzt die Ausrichtung für das Plateau (und liefert sie)
         /// </summary>
         /// <param name="valenv"></param>
         /// <param name="length">Plateaulänge</param>
         /// <returns></returns>
         public bool SetTopAligned4Plateau(ValueData valenv, int length) {
            if (TopAlignIsPossible) {
               bool topaligned = TopAlignedData.Get(valenv.X - 1, valenv.Y);
               for (int i = 0; i < length; i++)
                  TopAlignedData.Set(valenv.X + i, valenv.Y, topaligned);
               SurelyAlignedData.AddValue(valenv);
               return topaligned;
            }
            return false;
         }

         /// <summary>
         /// setzt die Ausrichtung für diesen Wert (und liefert sie)
         /// </summary>
         /// <param name="x">Spalte</param>
         /// <param name="y">Zeile</param>
         /// <param name="heigth">Höhe</param>
         /// <returns></returns>
         public bool SetTopAligned4Plateaufollower(ValueData valenv) {
            if (TopAlignIsPossible) {
               bool topaligned = false;
               if (valenv.Height == 0) {
                  topaligned = false;
               } else if (valenv.Height == MaxEncoderheight) {
                  topaligned = true;
               } else {
                  switch (valenv.Alignment) {
                     case Align3Type.TA000:
                     case Align3Type.TA001:
                        topaligned = false;
                        break;
                     case Align3Type.TA110:
                     case Align3Type.TA111:
                        topaligned = true;
                        break;
                     case Align3Type.TA100:
                     case Align3Type.TA101:
                        topaligned = valenv.HeightUpper + 1 == valenv.HeightLeft;
                        break;
                     case Align3Type.TA011:
                        topaligned = valenv.HeightUpper - 1 != valenv.HeightLeft;
                        break;
                     case Align3Type.TA010:
                        topaligned = valenv.HeightUpper != valenv.HeightLeft;
                        break;
                  }
               }
               TopAlignedData.Set(valenv.X, valenv.Y, topaligned);
               SurelyAlignedData.AddValue(valenv);
               return topaligned;
            }
            return false;
         }

         /// <summary>
         /// Gilt an dieser Position der TopAligned-Modus?
         /// </summary>
         /// <param name="x"></param>
         /// <param name="y"></param>
         /// <returns></returns>
         public bool IsTopAligned(int x, int y) {
            if (TopAlignIsPossible)
               return TopAlignedData.Get((ushort)x, (ushort)y);
            return false;
         }

         /// <summary>
         /// Beginnt an dieser Position ein Plateau?
         /// </summary>
         /// <param name="valdata"></param>
         /// <returns></returns>
         public bool IsPlateauStart(ValueData valdata) {
            if (TopAlignIsPossible &&     // sonst gilt die einfache Standardregeln
                valdata.X > 0) {          // bei x==0 immer Plateau

               switch (valdata.Alignment) {
                  case Align3Type.TA000:
                  case Align3Type.TA001:
                  case Align3Type.TA110:
                  case Align3Type.TA111:
                     return valdata.HeightUpper == valdata.HeightLeft;

                  // Bei TA01* und TA10* kann ein Sonderfall auftreten.
                  // Bei TA*xy (x != y) muss die Spalte m, bei TA*xx die Zeile n für Position (m,n) getestet werden.

                  case Align3Type.TA010:
                  case Align3Type.TA011:
                     if (valdata.HeightUpper == valdata.HeightLeft + 1 ||    // normalerweise Plateau bei TA01
                         valdata.HeightUpper == valdata.HeightLeft) {        // normalerweise KEIN Plateau bei TA01
                        return !IsDiagonalSpecialcase(valdata) ?
                                    valdata.HeightUpper == valdata.HeightLeft + 1 :
                                    valdata.HeightUpper != valdata.HeightLeft + 1; // also valdata.HeightUpper == valdata.HeightLeft
                     }
                     return false;

                  case Align3Type.TA100:
                  case Align3Type.TA101:
                     if (valdata.HeightUpper == valdata.HeightLeft - 1 ||    // normalerweise Plateau bei TA10
                         valdata.HeightUpper == valdata.HeightLeft) {        // normalerweise KEIN Plateau bei TA10
                        return !IsDiagonalSpecialcase(valdata) ?
                                    valdata.HeightUpper == valdata.HeightLeft - 1 :
                                    valdata.HeightUpper != valdata.HeightLeft - 1; // also valdata.HeightUpper == valdata.HeightLeft
                     }
                     return false;

               }

            }
            return valdata.HeightUpper == valdata.HeightLeft;
         }

         /// <summary>
         /// Sonderfall für Plateaustart und Plateaufollower bei TA01 und TA10 bei ddiff=-1, 0 oder 1
         /// </summary>
         /// <param name="valdata"></param>
         /// <returns></returns>
         public bool IsDiagonalSpecialcase(ValueData valdata) {
            bool specialcase = true;
            switch (valdata.Alignment) {
               case Align3Type.TA011:
                  // Vorgänger in Zeile valdata.Y untersuchen:
                  //    Ein Vorgänger mit MaxEncoderheight oder hook >= MaxEncoderheight führt zu einem positiven Testabbruch.
                  //    Ein Vorgänger mit 0 oder hook <= 0 führt zu einem negativen Testabbruch.
                  //    Ohne Abbruch pos.
                  specialcase = !SurelyAlignedData.HasOnLine_SurelyBottomAligned(valdata.X, valdata.Y);
                  break;

               case Align3Type.TA101:
                  // Vorgänger in Spalte valdata.X untersuchen:
                  //    Ein Vorgänger mit MaxEncoderheight oder hook >= MaxEncoderheight führt zu einem positiven Testabbruch.
                  //    Ein Vorgänger mit 0 oder hook <= 0 führt zu einem negativen Testabbruch.
                  //    Ohne Abbruch pos.
                  specialcase = !SurelyAlignedData.HasOnColumn_SurelyBottomAligned(valdata.X, valdata.Y);
                  break;

               case Align3Type.TA100:
                  // Vorgänger in Zeile valdata.Y untersuchen:
                  //    Ein Vorgänger mit 0 oder hook <= 0 führt zu einem positiven Testabbruch.
                  //    Ein Vorgänger mit MaxEncoderheight oder hook >= MaxEncoderheight führt zu einem negativen Testabbruch.
                  //    Ohne Abbruch pos.
                  specialcase = !SurelyAlignedData.HasOnLine_SurelyTopAligned(valdata.X, valdata.Y);
                  break;

               case Align3Type.TA010:
                  // Vorgänger in Spalte valdata.X untersuchen:
                  //    Ein Vorgänger mit 0 oder hook <= 0 führt zu einem positiven Testabbruch.
                  //    Ein Vorgänger mit MaxEncoderheight oder hook >= MaxEncoderheight führt zu einem negativen Testabbruch.
                  //    Ohne Abbruch pos.
                  specialcase = !SurelyAlignedData.HasOnColumn_SurelyTopAligned(valdata.X, valdata.Y);
                  break;

            }
            return specialcase;
         }

         /// <summary>
         /// ermittelt die Länge eines Plateaus ab der Startposition (ev. auch 0)
         /// </summary>
         /// <param name="valenv">Startpos.</param>
         /// <param name="length">Länge</param>
         /// <returns>true, wenn die Endpos. rechts-unten erreicht wurde</returns>
         public bool GetPlateauLength(ValueData valenv, out int length) {
            Position tst = new Position(realdata.Width, realdata.Height, valenv.X, valenv.Y);
            length = 0;
            bool bEnd = false;
            int value = GetEncoderHeightValue(tst.X - 1, tst.Y);

            while (tst.Idx < realdata.Count) {
               if (GetEncoderHeightValue(tst.X, tst.Y) != value)
                  break;

               length++;

               if (tst.X == tst.Width - 1)   // Zeilenende ereicht
                  break;

               if (!tst.MoveForward()) { // Ende erreicht
                  bEnd = true;
                  break;
               }
            }
            return bEnd;
         }

         /// <summary>
         /// liefert nur dann true, wenn in der Zeile ein <see cref="SurelyTopAligned.No"/> (vor einem ev. vorhandenen <see cref="SurelyTopAligned.Yes"/>) gefunden wird
         /// <para>Die Suche erfolgt nach links.</para>
         /// </summary>
         /// <param name="valenv"></param>
         /// <returns></returns>
         public bool HasOnLine_SurelyTopAlignedNo(ValueData valenv) {
            return SurelyAlignedData.HasOnLine_SurelyBottomAligned(valenv.X, valenv.Y);
         }


         /// <summary>
         /// 0- oder Top-Align für die 3 Nachbarn (hl, hu, hlu bzw. x,y,z)
         /// </summary>
         public enum Align3Type {
            unknown = -1,
            TA000 = 0,
            TA100 = 100,
            TA010 = 10,
            TA110 = 110,
            TA001 = 1,
            TA101 = 101,
            TA011 = 11,
            TA111 = 111,
         }

         /// <summary>
         /// liefert den Align-Typ für die Position basierend auf den 3 Nachbarwerten
         /// </summary>
         /// <param name="x"></param>
         /// <param name="y"></param>
         /// <returns></returns>
         public Align3Type GetAlignType3(int x, int y) {
            if (!TopAlignIsPossible)
               return Align3Type.TA000;
            int ht = 0;
            if (TopAlignedData.Get((ushort)x - 1, (ushort)y))
               ht += 100;
            if (TopAlignedData.Get((ushort)x, (ushort)y - 1))
               ht += 10;
            if (TopAlignedData.Get((ushort)x - 1, (ushort)y - 1))
               ht += 1;
            return (Align3Type)ht;
         }

      }

      /// <summary>
      /// zum Verwalten eines 2-dim-Arrays von ushort-Daten
      /// </summary>
      public class DataArray {

         readonly ushort[,] h = null;

         public int Width { get; }

         public int Height { get; }

         /// <summary>
         /// höchste serielle Position der gesetzten Werte
         /// </summary>
         public int Count { get; private set; }


         public DataArray(int width, int height) {
            if (width <= 0 || height <= 0)
               throw new ArgumentException("width and height in DataArray must be greater then 0");
            Width = width;
            Height = height;
            h = new ushort[width, height];
            Count = 0;
         }

         public DataArray(int width, int height, IList<int> data) : this(width, height) {
            int i = 0;
            for (int y = 0; y < height; y++)
               for (int x = 0; x < width && i < data.Count; x++, i++)
                  h[x, y] = (ushort)data[i];
            Count = i;
         }

         public DataArray(int width, int height, IList<ushort> data) : this(width, height) {
            int i = 0;
            for (int y = 0; y < height; y++)
               for (int x = 0; x < width && i < data.Count; x++, i++)
                  h[x, y] = data[i];
            Count = i;
         }

#if DEBUG
         void coordcheck(int x, int y) {
            if (x < 0 || y < 0 ||
                Width <= x || Height <= y)
               throw new ArgumentException("coords in DataArray not valid");
         }
#endif

         public void Set(int x, int y, int value) {
            Set(x, y, (ushort)value);
         }

         public void Set(int x, int y, ushort value) {
#if DEBUG
            coordcheck(x, y);
#endif
            h[x, y] = value;
            int count = y * Width + x;
            if (Count < count)
               Count = count;
         }

         public ushort Get(int x, int y) {
            if (x < 0 || y < 0) {
               if (x < 0) {
                  x = 0;
                  y = y - 1;  // virtuelle Spalte
               }
               if (y < 0) {
                  return 0;   // virtuelle Zeile
               }
            }
#if DEBUG
            coordcheck(x, y);
#endif
            return h[x, y];
         }

         /// <summary>
         /// Wert der seriellen Position
         /// </summary>
         /// <param name="pos"></param>
         /// <returns></returns>
         public ushort Get(int pos) {
            return Get(pos % Width, pos / Height);
         }

         public override string ToString() {
            return string.Format("Width={0} x ={1}, Count={2}", Width, Height, Count);
         }

      }

      /// <summary>
      /// ein Höhenwert mit seinen "umgebenden" Höhenwerten und weiteren korrespondierenden Daten
      /// </summary>
      public class ValueData {

         /// <summary>
         /// Position des Wertes
         /// </summary>
         public int X, Y;

         /// <summary>
         /// Höhenwert
         /// </summary>
         public int Height;

         /// <summary>
         /// Höhenwert links
         /// </summary>
         public int HeightLeft;

         /// <summary>
         /// Höhenwert oben
         /// </summary>
         public int HeightUpper;

         /// <summary>
         /// Höhenwert links oben
         /// </summary>
         public int HeightLeftUpper;

         /// <summary>
         /// Ausrichtung der Nachbarwerte (links, oben, link oben)
         /// </summary>
         public Shrink.Align3Type Alignment;

         /// <summary>
         /// Hook
         /// </summary>
         public int Hook;

         /// <summary>
         /// Diagonaldiff.
         /// </summary>
         public int DiagDiff;

         /// <summary>
         /// Signum der Diagonaldiff.
         /// </summary>
         public int SgnDiagDiff;

         /// <summary>
         /// zu speichernder Wert
         /// </summary>
         public int Data;

         /// <summary>
         /// Wert wurde gewrapt
         /// </summary>
         public bool IsWrapped;

         /// <summary>
         /// Art der Codierung
         /// </summary>
         public EncodeMode Encodemode;

         /// <summary>
         /// Art des Elementes
         /// </summary>
         public HeightElement.Typ HeightElementTyp;


         /// <summary>
         /// Hunit (nut gültig wenn <see cref="Encodemode"/>==<see cref="EncodeMode.Hybrid"/>)
         /// </summary>
         public int Hunit;

         /// <summary>
         /// Ist der akt. Wert topaligned?
         /// </summary>
         public bool IsTopAligned;

         public static int GetHook(int left, int upper, int leftupper, Shrink.Align3Type alignment) {
            int hook = left + upper - leftupper;
            switch (alignment) {
               case Shrink.Align3Type.TA001:
                  hook++;
                  break;

               case Shrink.Align3Type.TA110:
                  hook--;
                  break;
            }
            return hook;
         }

         public static int GetDiagDiff(int left, int upper, Shrink.Align3Type alignment) {
            int ddiff = upper - left;
            switch (alignment) {
               case Shrink.Align3Type.TA010:
               case Shrink.Align3Type.TA011:
                  ddiff--;
                  break;

               case Shrink.Align3Type.TA100:
               case Shrink.Align3Type.TA101:
                  ddiff++;
                  break;
            }
            return ddiff;
         }

         public static int GetSgn(int v) {
            return v > 0 ? 1 : v < 0 ? -1 : 0; ;
         }


         public ValueData() {
            Alignment = Shrink.Align3Type.unknown;
            X = Y = int.MinValue;
            Hook = DiagDiff = SgnDiagDiff = int.MinValue;
            Height = HeightLeft = HeightUpper = HeightLeftUpper = int.MinValue;
            Data = 0;

            IsWrapped = false;
            IsTopAligned = false;
            Encodemode = EncodeMode.notdefined;
            Hunit = int.MinValue;
         }

         public ValueData(Shrink shrink, int x, int y) : this() {
            X = x;
            Y = y;

            Height = shrink.GetEncoderHeightValue(x, y);
            HeightLeft = shrink.GetEncoderHeightValue(x - 1, y);
            HeightUpper = shrink.GetEncoderHeightValue(x, y - 1);
            HeightLeftUpper = shrink.GetEncoderHeightValue(x - 1, y - 1);

            Alignment = shrink.GetAlignType3(x, y);
            Hook = GetHook(HeightLeft, HeightUpper, HeightLeftUpper, Alignment);
            DiagDiff = HeightUpper - HeightLeft; // GetDiagDiff(HeightLeft, HeightUpper, Alignment);
            SgnDiagDiff = GetSgn(DiagDiff);
         }

         public override string ToString() {
            return string.Format("X={0}, Y={1}, Height={2}, HeightLeft={3}, HeightUpper={4}, HeightLeftUpper={5}, Alignment={6}, Hook={7}, DiagDiff={8}, SgnDiagDiff={9}, Data={10}, IsWrapped={11}, Encodemode={12}",
                                 X, Y,
                                 Height, HeightLeft, HeightUpper, HeightLeftUpper,
                                 Alignment,
                                 Hook,
                                 DiagDiff, SgnDiagDiff,
                                 Data,
                                 IsWrapped,
                                 Encodemode == EncodeMode.Hybrid ? Encodemode.ToString() + Hunit.ToString() : Encodemode.ToString());
         }
      }


      public Shrink shrink;

      /// <summary>
      /// alle (realen, ungeshrinkten) Höhenwerte
      /// </summary>
      public DataArray RealHeightValues { get; protected set; }

      /// <summary>
      /// erzeugt einen Encoder für eine Kachel
      /// <para>Mit dem wiederholten Aufruf von <see cref="ComputeNext"/> werden die gelieferten Daten schrittweise codiert. Wenn diese Funktion 0 liefert, ist
      /// die Codierung abgeschlossen. Die fertige Bytefolge knn mit <see cref="GetCodedBytes"/> abgerufen werden.
      /// </para>
      /// </summary>
      /// <param name="maxrealheight">max. Höhe</param>
      /// <param name="maxheightencoder">expl. gesetzt, wenn größer als 0 und shrink größer als 1</param>
      /// <param name="codingtyp">Codiertyp (z.Z. nicht verwendet)</param>
      /// <param name="shrink">Shrink-Faktor (ungerade)</param>
      /// <param name="tilesizehorz">Breite der Kachel</param>
      /// <param name="tilesizevert">Höhe der Kachel</param>
      /// <param name="height">Liste der Höhendaten (Anzahl normalerweise <see cref="tilesize"/> * <see cref="tilesize"/>)</param>
      /// <param name="shrinkheights">true, wenn reale (noch nicht verkleinerte) Höhen geliefert werden</param>
      public TileEncoder(int maxrealheight, int maxheightencoder, byte codingtyp, int shrink, int tilesizehorz, int tilesizevert, IList<int> height, bool shrinkheights) {
         TileSizeHorz = tilesizehorz;
         TileSizeVert = tilesizevert;

         RealHeightValues = new DataArray(TileSizeHorz, TileSizeVert, height);

         this.shrink = new Shrink(RealHeightValues, (ushort)tilesizehorz, (ushort)tilesizevert, shrink, maxrealheight, maxheightencoder, shrinkheights);

         MaxRealHeight = this.shrink.MaxRealheight;
         MaxHeight = this.shrink.MaxEncoderheight;
         HeightElement.Shrink = this.shrink.ShrinkValue;
         HeightElement.MaxEncoderheight = MaxHeight;
         HeightElement.MaxRealheight = MaxRealHeight;

#if EXPLORERFUNCTION

         Codingtype = codingtyp;
         initialHeightUnit = new CodingTypeStd(MaxHeight, this.shrink.ShrinkValue > 0);

#endif
         nextPosition = new Position(TileSizeHorz, TileSizeVert);

         ct_std = new CodingTypeStd(MaxHeight, shrink > 1);
         ct_ddiff4plateaufollower_zero = new CodingTypePlateauFollowerZero(MaxHeight);
         ct_ddiff4plateaufollower_notzero = new CodingTypePlateauFollowerNotZero(MaxHeight);

         Elements = new List<HeightElement>();

         ValueWrap = new Wraparound(MaxHeight);

#if EXPLORERFUNCTION

         CodingTypeStd_Info = new List<string>();
         CodingTypePlateauFollowerNotZero_Info = new List<string>();
         CodingTypePlateauFollowerZero_Info = new List<string>();

#endif
      }


      /// <summary>
      /// liefert die codierte Bitfolge als Bytes
      /// </summary>
      /// <param name="fillbits">Auffüllbits für das letzte Byte (1 wenn true)</param>
      /// <returns></returns>
      public byte[] GetCodedBytes(bool fillbits = true) {
         List<byte> Bits = new List<byte>();
         foreach (var item in Elements)
            Bits.AddRange(item.Bits);

         byte[] data = new byte[Bits.Count % 8 == 0 ? Bits.Count / 8 : Bits.Count / 8 + 1];

         for (int i = 0; i < Bits.Count; i += 8) {
            int bidx = i / 8;
            for (int j = 0; j < 8 && i + j < Bits.Count; j++) {
               switch (j) {
                  case 0: data[bidx] |= (byte)(Bits[i + j] << 7); break;
                  case 1: data[bidx] |= (byte)(Bits[i + j] << 6); break;
                  case 2: data[bidx] |= (byte)(Bits[i + j] << 5); break;
                  case 3: data[bidx] |= (byte)(Bits[i + j] << 4); break;
                  case 4: data[bidx] |= (byte)(Bits[i + j] << 3); break;
                  case 5: data[bidx] |= (byte)(Bits[i + j] << 2); break;
                  case 6: data[bidx] |= (byte)(Bits[i + j] << 1); break;
                  case 7: data[bidx] |= (byte)(Bits[i + j]); break;
               }
            }
         }
         if (fillbits &&
             data.Length > 0) {
            switch (Bits.Count % 8) {
               case 0: break;
               case 1: data[data.Length - 1] |= 0x7f; break;
               case 2: data[data.Length - 1] |= 0x3f; break;
               case 3: data[data.Length - 1] |= 0x1f; break;
               case 4: data[data.Length - 1] |= 0x0f; break;
               case 5: data[data.Length - 1] |= 0x07; break;
               case 6: data[data.Length - 1] |= 0x03; break;
               case 7: data[data.Length - 1] |= 0x01; break;
            }
         }

         return data;
      }

#if EXPLORERFUNCTION

      /// <summary>
      /// liefert die codierte Bitfolge als Zeichenkette mit '1' und '.'
      /// </summary>
      /// <param name="from"></param>
      /// <param name="length"></param>
      /// <returns></returns>
      public string GetBinText(int from = 0, int length = 0) {
         StringBuilder sb = new StringBuilder();

         List<byte> Bits = new List<byte>();
         foreach (var item in Elements)
            Bits.AddRange(item.Bits);

         if (from < 0)
            from = 0;
         if (length <= 0 || Bits.Count - from < length)
            length = Bits.Count - from;
         for (int i = from; i < from + length; i++)
            sb.Append(Bits[i] != 0 ? "1" : "0");
         sb.Replace('0', '.');
         return sb.ToString();
      }

#endif

      /// <summary>
      /// führt die Codierung der nächsten Höhe/n aus
      /// <para>I.A. wird hierbei der nächste Höhenwert codiert. Es können aber auch mehrere Höhenwerte gemeinsam codiert werden, wenn dass wegen des 
      /// Codierverfahrens nötig ist.</para>
      /// </summary>
      /// <param name="bTileIsFull">true, wenn die Kachel gefüllt ist</param>
      /// <returns>Anzahl der im aktuellen Schritt codierten Elemente (0 bedeutet Ende der Codierung)</returns>
      public int ComputeNext(out bool bTileIsFull) {
         int elements = Elements.Count;
         bTileIsFull = true;
         if (nextPosition.Idx < RealHeightValues.Count)
            bTileIsFull = CalculateData(nextPosition);      // die akt. Höhe oder zusätzlich mehrere folgende Höhen codieren

         if (nextPosition.Idx > RealHeightValues.Count)   // sicherheitshalber eingrenzen
            nextPosition.Idx = RealHeightValues.Count;

         return Elements.Count - elements; ;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="pos"></param>
      /// <returns>true, wenn die Endpos. rechts-unten erreicht wurde</returns>
      bool CalculateData(Position pos) {
         bool bEnd = false;
         ValueData valenv = new ValueData(shrink, pos.X, pos.Y);

         if (valenv.Height >= 0) { // nur nichtnegative Höhen

            try {

               if (shrink.IsPlateauStart(valenv))
                  bEnd = WritePlateau(pos, valenv);
               else
                  bEnd = WriteStandardValue(pos, valenv);

            } catch (Exception ex) {
               throw new Exception(string.Format("interner Fehler bei Position {0}, Höhe {1}: {2}", pos, valenv.Height, ex.Message));
            }
         } else
            throw new Exception(string.Format("negative Daten (Pos {0}) können nicht verarbeitet werden.", pos));
         return bEnd;
      }

      /// <summary>
      /// erzeugt ein <see cref="HeightElement"/> für den Standardwert
      /// </summary>
      /// <param name="pos">akt. Position</param>
      /// <param name="valdata">akt. Höhendaten</param>
      /// <returns></returns>
      bool WriteStandardValue(Position pos, ValueData valdata) {
         if (valdata.DiagDiff == 0)
            switch (valdata.Alignment) {
               case Shrink.Align3Type.TA010:
               case Shrink.Align3Type.TA011:
                  valdata.DiagDiff--;
                  valdata.SgnDiagDiff = ValueData.GetSgn(valdata.DiagDiff);
                  break;

               case Shrink.Align3Type.TA101:
               case Shrink.Align3Type.TA100:
                  valdata.DiagDiff++;
                  valdata.SgnDiagDiff = ValueData.GetSgn(valdata.DiagDiff);
                  break;
            }
         if (valdata.SgnDiagDiff == 0)
            throw new Exception(string.Format("sgnddiff == 0 bei Pos [{0},{1}]", valdata.X, valdata.Y));

         if (shrink.MaxEncoderheight <= valdata.Hook) {

            valdata.Data = -valdata.SgnDiagDiff * (valdata.Height + 1);
            valdata.HeightElementTyp = HeightElement.Typ.ValueHookHigh;

         } else if (valdata.Hook <= 0) {

            valdata.Data = -valdata.SgnDiagDiff * valdata.Height;
            valdata.HeightElementTyp = HeightElement.Typ.ValueHookLow;

         } else {

            valdata.Data = -valdata.SgnDiagDiff * (valdata.Height - valdata.Hook);
            valdata.HeightElementTyp = HeightElement.Typ.ValueHookMiddle;

         }

         valdata.Encodemode = ct_std.EncodeMode;
         valdata.Hunit = ct_std.HunitValue;
         valdata.Data = ValueWrap.Wrap(valdata, true);

         if (shrink.TopAlignIsPossible) {
            AvoidSpecialMinMax4StandardValues(valdata);
            shrink.SetTopAligned4Normal(valdata);
         }

         valdata.Encodemode = ValueWrap.FitEncodeMode(valdata, ct_std.HunitValue);
         AddHeightValue(valdata, ct_std);

         return !pos.MoveForward();
      }

      /// <summary>
      /// erzeugt ein <see cref="HeightElement"/> für die Plateaulänge und eins für den Plateaunachfolger
      /// </summary>
      /// <param name="pos">akt. Position</param>
      /// <param name="valdata">akt. Höhendaten</param>
      /// <returns>true, wenn die Endpos. rechts-unten erreicht wurde</returns>
      bool WritePlateau(Position pos, ValueData valdata) {
         bool bEnd = shrink.GetPlateauLength(valdata, out int length);
         valdata.Data = length;
         shrink.SetTopAligned4Plateau(valdata, length);
         valdata.Encodemode = EncodeMode.PlateauLength;
         valdata.HeightElementTyp = HeightElement.Typ.PlateauLength;
         AddHeightValue(valdata, null);

         // pos steht am Anfang des Plateaus bzw., bei Länge 0, schon auf dem Follower
         // also zeigt pos.X+length auf die Pos. des Followers
         bool bLineFilled = pos.X + length >= TileSizeHorz;
         bEnd = !pos.MoveForward(length);

         CodingType ct = null;
         if (!bLineFilled) { // Nachfolgewert bestimmen
            if (!bEnd) {

               valdata = new ValueData(shrink, pos.X, pos.Y);
               valdata.Data = valdata.Height - valdata.HeightUpper;  // vdiff
               int sgndiagdiff = valdata.SgnDiagDiff;

               // immer PlateauFollower0, wenn bei
               //    TA00, TA11:    DiagDiff = 0
               //    TA01:          DiagDiff = 1   ( -> sgn(ddiff - 1))
               //    TA10:          DiagDiff = -1  ( -> sgn(ddiff + 1))
               bool bCodingType0 = false;
               bool specialcase = false;
               switch (valdata.Alignment) {
                  case Shrink.Align3Type.TA000:
                  case Shrink.Align3Type.TA001:
                  case Shrink.Align3Type.TA110:
                  case Shrink.Align3Type.TA111:
                     switch (valdata.DiagDiff) {
                        case 0:
                           bCodingType0 = true;
                           break;
                     }
                     break;

                  case Shrink.Align3Type.TA010:
                  case Shrink.Align3Type.TA011:
                     valdata.DiagDiff--;
                     switch (valdata.DiagDiff) {
                        case 0:
                           bCodingType0 = true;
                           specialcase = shrink.IsDiagonalSpecialcase(valdata);
                           if (specialcase) {      // dann doch kein PlateauFollower0
                              bCodingType0 = false;
                              //sgndiagdiff = 1;  // stimmt schon
                           }
                           break;
                        case -1:
                           bCodingType0 = false;
                           sgndiagdiff = -1;
                           specialcase = shrink.IsDiagonalSpecialcase(valdata);
                           if (specialcase) {      // dann doch PlateauFollower0
                              bCodingType0 = true;
                           }
                           break;
                     }
                     break;

                  case Shrink.Align3Type.TA100:
                  case Shrink.Align3Type.TA101:
                     valdata.DiagDiff++;
                     switch (valdata.DiagDiff) {
                        case 0:
                           bCodingType0 = true;
                           specialcase = shrink.IsDiagonalSpecialcase(valdata);
                           if (specialcase) {      // dann doch kein PlateauFollower0
                              bCodingType0 = false;
                              //sgndiagdiff = -1; // stimmt schon
                           }
                           break;
                        case 1:
                           bCodingType0 = false;
                           sgndiagdiff = 1;
                           specialcase = shrink.IsDiagonalSpecialcase(valdata);
                           if (specialcase) {      // dann doch PlateauFollower0
                              bCodingType0 = true;
                           }
                           break;
                     }
                     break;
               }

               if (!bCodingType0) {

                  ct = ct_ddiff4plateaufollower_notzero;
                  valdata.Encodemode = ct.EncodeMode;
                  valdata.HeightElementTyp = HeightElement.Typ.PlateauFollower;

                  if (sgndiagdiff > 0)                // data = -(valdata.Height - valdata.HeightUpper)
                     valdata.Data = -valdata.Data;    // data =  (valdata.Height - valdata.HeightUpper)

                  valdata.Data = ValueWrap.Wrap(valdata, true);

                  if (shrink.TopAlignIsPossible) {
                     AvoidSpecialMinMax4PlateauFollower(valdata);
                     shrink.SetTopAligned4Plateaufollower(valdata);
                  }

               } else { // bCodingType0 == true

                  ct = ct_ddiff4plateaufollower_zero;
                  valdata.Encodemode = ct.EncodeMode;
                  valdata.HeightElementTyp = HeightElement.Typ.PlateauFollower0;

                  if (!specialcase) {
                     // bisher gilt nur: data = h - hu
                     //    TA00,TA11  data = (h - hu);     d < 0 -> d++
                     //    TA10       data = (h - hu);     d > 0 -> d--
                     //    TA01       data = (h - hu) + 1; d < 0 -> d++
                     switch (valdata.Alignment) {
                        case Shrink.Align3Type.TA000:
                        case Shrink.Align3Type.TA001:
                        case Shrink.Align3Type.TA110:
                        case Shrink.Align3Type.TA111:
                           if (valdata.Data < 0)
                              valdata.Data++;
                           break;

                        case Shrink.Align3Type.TA100:
                        case Shrink.Align3Type.TA101:
                           if (valdata.Data > 0)
                              valdata.Data--;
                           break;

                        case Shrink.Align3Type.TA011:
                        case Shrink.Align3Type.TA010:
                           valdata.Data++;
                           if (valdata.Data < 0)
                              valdata.Data++;
                           break;
                     }
                  } else {
                     // bisher gilt nur: data = h - hu
                     //    immer     data = (h - hu);     d < 0 -> d++
                     if (valdata.Data < 0)
                        valdata.Data++;
                  }

                  valdata.Data = ValueWrap.Wrap(valdata, false);

                  AvoidSpecialMinMax4PlateauFollower0(valdata);
                  shrink.SetTopAligned4Plateaufollower(valdata);

               }

               valdata.Hunit = ct.HunitValue;
               valdata.Encodemode = ValueWrap.FitEncodeMode(valdata, ct.HunitValue, Elements[Elements.Count - 1].PlateauBinBits);
               AddHeightValue(valdata, ct);

               bEnd = !pos.MoveForward();
            }
         }

         return bEnd;
      }

      /// <summary>
      /// spezielle Datenwerte vermeiden, mit denen 0 bzw. der Maximalwert NICHT erreicht wird
      /// </summary>
      /// <param name="valdata"></param>
      void AvoidSpecialMinMax4StandardValues(ValueData valdata) {
         // Sonderfälle bei denen 0 als MaxEncoderheight bzw. MaxEncoderheight als 0 dargestellt wird vermeiden
         bool wrap4minmax = false;

         if (valdata.Height == 0) {

            switch (valdata.Alignment) {
               case Shrink.Align3Type.TA100:
               case Shrink.Align3Type.TA010:
               case Shrink.Align3Type.TA111:
                  if ((0 < valdata.Hook && valdata.Hook < shrink.MaxEncoderheight && !valdata.IsWrapped) ||
                      (shrink.MaxEncoderheight <= valdata.Hook && valdata.IsWrapped))
                     wrap4minmax = true;
                  break;

               case Shrink.Align3Type.TA001:
                  if ((0 < valdata.Hook && valdata.Hook < shrink.MaxEncoderheight && !valdata.IsWrapped) ||
                      (shrink.MaxEncoderheight <= valdata.Hook && valdata.IsWrapped && shrink.HasOnLine_SurelyTopAlignedNo(valdata)))
                     wrap4minmax = true;
                  break;
            }

         } else if (valdata.Height == shrink.MaxEncoderheight) {

            switch (valdata.Alignment) {
               case Shrink.Align3Type.TA000:
               case Shrink.Align3Type.TA101:
                  if (valdata.Hook < shrink.MaxEncoderheight && !valdata.IsWrapped)
                     wrap4minmax = true;
                  break;

               case Shrink.Align3Type.TA011:
               case Shrink.Align3Type.TA110:
                  if ((valdata.Hook <= 0 && !valdata.IsWrapped) ||
                      (0 < valdata.Hook && valdata.Hook < shrink.MaxEncoderheight && !valdata.IsWrapped && shrink.HasOnLine_SurelyTopAlignedNo(valdata)))
                     wrap4minmax = true;
                  break;
            }

         }

         if (wrap4minmax) {
            if (valdata.Data < 0)
               valdata.Data += shrink.MaxEncoderheight + 1;
            else if (valdata.Data > 0)
               valdata.Data -= shrink.MaxEncoderheight + 1;
            valdata.IsWrapped = true;
         }

      }

      /// <summary>
      /// spezielle Datenwerte vermeiden, mit denen 0 bzw. der Maximalwert NICHT erreicht wird
      /// </summary>
      /// <param name="valdata"></param>
      /// <param name="speccaseTA011"></param>
      /// <param name="speccaseTA10"></param>
      void AvoidSpecialMinMax4PlateauFollower(ValueData valdata) {
         bool wrap4minmax = false;

         if (!valdata.IsWrapped) {
            if (valdata.Height == 0) {
               switch (valdata.Alignment) { // TA*1*
                  case Shrink.Align3Type.TA110:
                  case Shrink.Align3Type.TA111:
                  case Shrink.Align3Type.TA010:
                  case Shrink.Align3Type.TA011:
                     wrap4minmax = true;
                     break;
               }
            } else {
               if (valdata.Height == shrink.MaxEncoderheight) {
                  switch (valdata.Alignment) { // TA*0*
                     case Shrink.Align3Type.TA000:
                     case Shrink.Align3Type.TA001:
                     case Shrink.Align3Type.TA100:
                     case Shrink.Align3Type.TA101:
                        wrap4minmax = true;
                        break;
                  }
               }
            }

            if (wrap4minmax) {
               if (valdata.Data < 0)
                  valdata.Data += shrink.MaxEncoderheight + 1;
               else if (valdata.Data > 0)
                  valdata.Data -= shrink.MaxEncoderheight + 1;
               else // data=0;
                  valdata.Data += shrink.MaxEncoderheight + 1;   // höchtwahrscheinlich egal, ob -= oder +=
               valdata.IsWrapped = true;
            }
         }
      }

      /// <summary>
      /// spezielle Datenwerte vermeiden, mit denen 0 bzw. der Maximalwert NICHT erreicht wird
      /// </summary>
      /// <param name="valdata"></param>
      /// <param name="speccaseTA011"></param>
      /// <param name="speccaseTA10"></param>
      void AvoidSpecialMinMax4PlateauFollower0(ValueData valdata) {
         bool wrap4minmax = false;

         if (!valdata.IsWrapped) {
            if (valdata.Height == 0) {
               switch (valdata.Alignment) { // wenn TA1**
                  case Shrink.Align3Type.TA110:
                  case Shrink.Align3Type.TA111:
                  case Shrink.Align3Type.TA100:
                  case Shrink.Align3Type.TA101:
                     wrap4minmax = true;
                     break;
               }
            } else {
               if (valdata.Height == shrink.MaxEncoderheight) {
                  switch (valdata.Alignment) { // wenn TA0**
                     case Shrink.Align3Type.TA000:
                     case Shrink.Align3Type.TA001:
                     case Shrink.Align3Type.TA010:
                     case Shrink.Align3Type.TA011:
                        if (valdata.Height == shrink.MaxEncoderheight)
                           wrap4minmax = true;
                        break;
                  }
               }
            }

            if (wrap4minmax) { // bei PlateauFollower0 nur shrink.MaxEncoderheight und NICHT (shrink.MaxEncoderheight + 1) !
               if (valdata.Data < 0)
                  valdata.Data += shrink.MaxEncoderheight;
               else if (valdata.Data > 0)
                  valdata.Data -= shrink.MaxEncoderheight;
               else // data=0;
                  valdata.Data += shrink.MaxEncoderheight;   // höchtwahrscheinlich egal, ob -= oder +=
               valdata.IsWrapped = true;
            }

         }
      }

      /// <summary>
      /// fügt eine neues <see cref="HeightElement"/> an die Liste <see cref="Elements"/> an und registriert den Datenwert im <see cref="CodingType"/>
      /// </summary>
      /// <param name="valdata">Datenwert</param>
      /// <param name="em_explicit">ausdrücklicher Codiermodus</param>
      void AddHeightValue(ValueData valdata, CodingType ct) {
         valdata.IsTopAligned = shrink.IsTopAligned(valdata.X, valdata.Y);
         HeightElement.HeightElementInfo hei = null;

         switch (valdata.HeightElementTyp) {
            case HeightElement.Typ.PlateauLength:
               hei = new HeightElement.HeightElementInfo(valdata, TileSizeHorz, Elements);
               break;

            case HeightElement.Typ.PlateauFollower:
            case HeightElement.Typ.PlateauFollower0:
            case HeightElement.Typ.ValueHookHigh:
            case HeightElement.Typ.ValueHookMiddle:
            case HeightElement.Typ.ValueHookLow:
               hei = new HeightElement.HeightElementInfo(valdata);
               break;
         }

         if (hei != null) {
            Elements.Add(new HeightElement(hei));

            if (ct != null) {

               ct.AddValue(valdata.Data);

#if EXPLORERFUNCTION

               List<string> infolst = null;

               switch (valdata.HeightElementTyp) {
                  case HeightElement.Typ.PlateauFollower:
                     infolst = CodingTypePlateauFollowerNotZero_Info;
                     break;

                  case HeightElement.Typ.PlateauFollower0:
                     infolst = CodingTypePlateauFollowerZero_Info;
                     break;

                  case HeightElement.Typ.ValueHookHigh:
                  case HeightElement.Typ.ValueHookMiddle:
                  case HeightElement.Typ.ValueHookLow:
                     infolst = CodingTypeStd_Info;
                     break;
               }

               if (infolst.Count == 0)
                  infolst.Add("Bonus " + ct.Bonus.ToString());

               string info = string.Format("Position=[{0},{1}], ElemCount={2}, data={3}, eval={4}, HunitValue={5}, SumH={6}, SumL={7}",
                                            valdata.X, valdata.Y, ct.ElemCount, valdata.Data, ct.Eval, ct.HunitValue, ct.SumH, ct.SumL);
               if (ct.ExtInfo4LastAdd.Length > 0)
                  info += "; [" + ct.ExtInfo4LastAdd + "]";

               infolst.Add(info);

#endif
            }
         }
      }

#if EXPLORERFUNCTION

      #region zum Ermitteln der Bitfolgen

      static public List<byte> LengthCoding0(int data, int shrink = 1, int maxrealheight = 0) {
         HeightElement.Shrink = shrink;
         HeightElement.MaxRealheight = maxrealheight;
         return new List<byte>(new HeightElement(new HeightElement.HeightElementInfo(new ValueData() { HeightElementTyp = HeightElement.Typ.ValueHookHigh, Data = data, Encodemode = EncodeMode.Length0 })).Bits);
      }

      static public List<byte> LengthCoding1(int data, int shrink = 1, int maxrealheight = 0) {
         HeightElement.Shrink = shrink;
         HeightElement.MaxRealheight = maxrealheight;
         return new List<byte>(new HeightElement(new HeightElement.HeightElementInfo(new ValueData() { HeightElementTyp = HeightElement.Typ.ValueHookHigh, Data = data, Encodemode = EncodeMode.Length1 })).Bits);
      }

      static public List<byte> LengthCoding2(int data, int shrink = 1, int maxrealheight = 0) {
         HeightElement.Shrink = shrink;
         HeightElement.MaxRealheight = maxrealheight;
         return new List<byte>(new HeightElement(new HeightElement.HeightElementInfo(new ValueData() { HeightElementTyp = HeightElement.Typ.ValueHookHigh, Data = data, Encodemode = EncodeMode.Length2 })).Bits);
      }

      static public List<byte> HybridCoding(int data, int maxHeight, int hunit, int shrink = 1, int maxrealheight = 0) {
         HeightElement.MaxEncoderheight = maxHeight;
         HeightElement.Shrink = shrink;
         HeightElement.MaxRealheight = maxrealheight;
         return new List<byte>(new HeightElement(new HeightElement.HeightElementInfo(new ValueData() { HeightElementTyp = HeightElement.Typ.ValueHookHigh, Data = data, Encodemode = EncodeMode.Hybrid, Hunit = hunit })).Bits);
      }

      static public List<byte> BigValueCodingHybrid(int data, int maxHeight, int shrink = 1, int maxrealheight = 0) {
         HeightElement.MaxEncoderheight = maxHeight;
         HeightElement.Shrink = shrink;
         HeightElement.MaxRealheight = maxrealheight;
         return new List<byte>(new HeightElement(new HeightElement.HeightElementInfo(new ValueData() { HeightElementTyp = HeightElement.Typ.ValueHookHigh, Data = data, Encodemode = EncodeMode.BigBinary })).Bits);
      }

      static public List<byte> BigValueCodingLength1(int data, int maxHeight, int shrink = 1, int maxrealheight = 0) {
         HeightElement.MaxEncoderheight = maxHeight;
         HeightElement.Shrink = shrink;
         HeightElement.MaxRealheight = maxrealheight;
         return new List<byte>(new HeightElement(new HeightElement.HeightElementInfo(new ValueData() { HeightElementTyp = HeightElement.Typ.ValueHookHigh, Data = data, Encodemode = EncodeMode.BigBinaryL1 })).Bits);
      }

      static public List<byte> BigValueCodingLength2(int data, int maxHeight, int shrink = 1, int maxrealheight = 0) {
         HeightElement.MaxEncoderheight = maxHeight;
         HeightElement.Shrink = shrink;
         HeightElement.MaxRealheight = maxrealheight;
         return new List<byte>(new HeightElement(new HeightElement.HeightElementInfo(new ValueData() { HeightElementTyp = HeightElement.Typ.ValueHookHigh, Data = data, Encodemode = EncodeMode.BigBinaryL2 })).Bits);
      }

      #endregion

#endif

#if EXPLORERFUNCTION

      public override string ToString() {
         return string.Format("MaxHeight={0}, Codingtype={1}, Shrink={2}, TileSize={3}x{4}, BaseHeightUnit={5}, HeightUnit={6}, ActualMode={7}, ActualHeight={8}",
                              MaxHeight,
                              Codingtype,
                              shrink.ShrinkValue,
                              TileSizeHorz,
                              TileSizeVert,
                              initialHeightUnit.HunitValue,
                              HeightUnit,
                              ActualMode,
                              ActualHeight);
#else

      public override string ToString() {
         return string.Format("MaxHeight={0}, TileSize={1}x{2}",
                              MaxHeight,
                              TileSizeHorz,
                              TileSizeVert);

#endif

      }

   }
}
