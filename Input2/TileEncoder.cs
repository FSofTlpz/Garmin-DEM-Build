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
            if ((object)pos == null) // NICHT "p == null" usw. --> führt zur Endlosschleife
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
            /// normaler Höhenwert
            /// </summary>
            Value,
            /// <summary>
            /// ein Plateau
            /// </summary>
            Plateau,
            /// <summary>
            /// Wert hinter einem Plateau
            /// </summary>
            PlateauFollower,
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
            ///// Berechnungstyp
            ///// </summary>
            public CalculationType Caltype { get; private set; }
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
            /// Plateaulänge
            /// </summary>
            public int LineLength { get; private set; }
            /// <summary>
            /// vorhergehendes <see cref="HeightElement"/> (nur bei Plateau)
            /// </summary>
            public HeightElement LastHeightElement { get; private set; }


            protected HeightElementInfo(Typ typ, int column, int line, int data, CalculationType caltype, EncodeMode encmode, bool wrapped, bool topaligned) {
               Typ = typ;
               Data = data;
               Column = column;
               Line = line;
               Caltype = caltype;
               Wrapped = wrapped;
               EncMode = encmode;
               Hunit = int.MinValue;
               Ddiff = int.MinValue;
               LineLength = int.MinValue;
               TopAligned = topaligned;
               LastHeightElement = null;
            }


            public HeightElementInfo(Typ typ, int data, CalculationType caltype, EncodeMode encmode, bool wrapped, bool topaligned, int column, int line) :
               this(typ, column, line, data, caltype, encmode, wrapped, topaligned) {
            }

            public HeightElementInfo(Typ typ, int data, CalculationType caltype, EncodeMode encmode, bool wrapped, int hunit, bool topaligned, int column, int line) :
               this(typ, column, line, data, caltype, encmode, wrapped, topaligned) {
               Hunit = hunit;
            }

            public HeightElementInfo(Typ typ, int data, int ddiff, CalculationType caltype, EncodeMode encmode, bool wrapped, int hunit, bool topaligned, int column, int line) :
               this(typ, column, line, data, caltype, encmode, wrapped, topaligned) {
               Ddiff = ddiff;
               Hunit = hunit;
            }

            public HeightElementInfo(Typ typ, int data, int ddiff, CalculationType caltype, EncodeMode encmode, bool wrapped, bool topaligned, int column, int line) :
               this(typ, column, line, data, caltype, encmode, wrapped, topaligned) {
               Ddiff = ddiff;
            }

            public HeightElementInfo(Typ typ, int length, int linelength, IList<HeightElement> oldheightelements, bool topaligned, int column, int line) :
               this(typ, column, line, length, CalculationType.nothing, EncodeMode.Plateau, false, topaligned) {
               LineLength = linelength;
               for (int i = oldheightelements.Count - 1; i >= 0; i--) // letztes Plateau-Element suchen
                  if (oldheightelements[i].Info.Typ == HeightElement.Typ.Plateau) {
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
         /// 
         /// </summary>
         public int PlateauFollowerDdiff { get; private set; }
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

            PlateauFollowerDdiff = Info.Ddiff;
            if (Info.LastHeightElement != null)
               PlateauTable4Tile = Info.LastHeightElement.PlateauTable4Tile;
            else
               PlateauTable4Tile = new PlateauTable();

            switch (hi.Typ) {
               case Typ.Value:
               case Typ.PlateauFollower:
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

               case Typ.Plateau:
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
            if (Info.Typ == Typ.Plateau) {
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

            if (Info.Typ == Typ.Plateau)
               sb.Append(" " + GetPlateauUnitsText());
            else if (Info.Typ == Typ.PlateauFollower)
               sb.Append(" ddiff=" + PlateauFollowerDdiff);

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
         /// <param name="data">Wert</param>
         /// <param name="wrapped">gesetzt, wenn ein gewrapter Wert geliefert wird</param>
         /// <param name="em">Codierart des Wertes</param>
         /// <returns>zu verwendender Wert</returns>
         public int Wrap(int data, out bool wrapped, EncodeMode em) {
            wrapped = false;
            int datawrapped = int.MinValue;

            switch (em) {
               case EncodeMode.Hybrid:
                  if (data > H_wrapdown)
                     datawrapped = WrapDown(data);
                  else if (data < H_wrapup)
                     datawrapped = WrapUp(data);
                  break;

               case EncodeMode.Length0:
                  if (data > L0_wrapdown)
                     datawrapped = WrapDown(data);
                  else if (data < L0_wrapup)
                     datawrapped = WrapUp(data);
                  break;

               case EncodeMode.Length1:
                  if (data > L1_wrapdown)
                     datawrapped = WrapDown(data);
                  else if (data < L1_wrapup)
                     datawrapped = WrapUp(data);
                  break;

               case EncodeMode.Length2:
                  if (data > L2_wrapdown)
                     datawrapped = WrapDown(data);
                  else if (data < L2_wrapup)
                     datawrapped = WrapUp(data);
                  break;

               case EncodeMode.BigBinary:
               case EncodeMode.BigBinaryL2: {
                     HeightElement.GetValueRangeBigBin(out int minval, out int maxval);
                     if (data < minval || maxval < data) {
                        if (data > maxval)
                           datawrapped = WrapDown(data);
                        else if (data < minval)
                           datawrapped = WrapUp(data);
                     }
                  }
                  break;

               case EncodeMode.BigBinaryL1: {
                     HeightElement.GetValueRangeBigBinL1(out int minval, out int maxval);
                     if (data < minval || maxval < data) {
                        if (data > maxval)
                           datawrapped = WrapDown(data);
                        else if (data < minval)
                           datawrapped = WrapUp(data);
                     }
                  }
                  break;
            }

            wrapped = datawrapped != int.MinValue;

            return wrapped ? datawrapped : data;
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
         public EncodeMode FitEncodeMode(int data, EncodeMode em, int hunit, int iPlateauLengthBinBits = -1) {
            int minval, maxval;
            switch (em) {
               case EncodeMode.Hybrid:
                  HeightElement.GetValueRangeHybrid(hunit, out minval, out maxval, iPlateauLengthBinBits);
                  return data < minval || maxval < data ?
                                 EncodeMode.BigBinary :
                                 em;

               case EncodeMode.Length0:
                  HeightElement.GetValueRangeLength0(out minval, out maxval, iPlateauLengthBinBits);
                  return data < minval || maxval < data ?
                                 EncodeMode.BigBinary :
                                 em;

               case EncodeMode.Length1:
                  HeightElement.GetValueRangeLength1(out minval, out maxval, iPlateauLengthBinBits);
                  return data < minval || maxval < data ?
                                 EncodeMode.BigBinaryL1 :
                                 em;

               case EncodeMode.Length2:
                  HeightElement.GetValueRangeLength2(out minval, out maxval, iPlateauLengthBinBits);
                  return data < minval || maxval < data ?
                                 EncodeMode.BigBinaryL2 :
                                 em;

               default:
                  return em;
            }
         }


         //public int WrapAlt(int data, out bool wrapped, ref EncodeMode em, int hunit, int iPlateauLengthBinBits) {
         //   int minval, maxval;
         //   wrapped = false;
         //   int datawrapped = int.MinValue;

         //   wrapped = false;
         //   if (data > 0) {
         //      if (2 * data > max + 1) {
         //         data = WrapDown(data);
         //         wrapped = true;
         //      }
         //   } else if (data < 0) {
         //      if (2 * data < -(max + 1)) {
         //         data = WrapUp(data);
         //         wrapped = true;
         //      }
         //   }


         //   switch (em) {
         //      case EncodeMode.Length0:
         //         if (data > L0_wrapdown)
         //            datawrapped = WrapDown(data);
         //         else if (data < L0_wrapup)
         //            datawrapped = WrapUp(data);

         //         HeightElement.GetValueRangeLength0(max, out minval, out maxval, iPlateauLengthBinBits);
         //         if (datawrapped != int.MinValue) {
         //            if (datawrapped < minval || maxval < datawrapped) {
         //               datawrapped = int.MinValue; // wird doch nicht benötigt
         //               em = EncodeMode.BigBinary;
         //            }
         //         } else if (data < minval || maxval < data)
         //            em = EncodeMode.BigBinary;
         //         break;

         //      case EncodeMode.Length1:
         //         if (data > L1_wrapdown)
         //            datawrapped = WrapDown(data);
         //         else if (data < L1_wrapup)
         //            datawrapped = WrapUp(data);

         //         HeightElement.GetValueRangeLength1(max, out minval, out maxval, iPlateauLengthBinBits);
         //         if (datawrapped != int.MinValue) {
         //            if (datawrapped < minval || maxval < datawrapped) {
         //               datawrapped = int.MinValue; // wird doch nicht benötigt
         //               em = EncodeMode.BigBinaryL1;
         //            }
         //         } else if (data < minval || maxval < data)
         //            em = EncodeMode.BigBinaryL1;
         //         break;

         //      case EncodeMode.Length2:
         //         if (data > L2_wrapdown)
         //            datawrapped = WrapDown(data);
         //         else if (data < L2_wrapup)
         //            datawrapped = WrapUp(data);

         //         HeightElement.GetValueRangeLength2(max, out minval, out maxval, iPlateauLengthBinBits);
         //         if (datawrapped != int.MinValue) {
         //            if (datawrapped < minval || maxval < datawrapped) {
         //               datawrapped = int.MinValue; // wird doch nicht benötigt
         //               em = EncodeMode.BigBinaryL2;
         //            }
         //         } else if (data < minval || maxval < data)
         //            em = EncodeMode.BigBinaryL2;
         //         break;

         //      case EncodeMode.Hybrid:
         //         if (data > H_wrapdown)
         //            datawrapped = WrapDown(data);
         //         else if (data < H_wrapup)
         //            datawrapped = WrapUp(data);

         //         HeightElement.GetValueRangeHybrid(hunit, max, out minval, out maxval, iPlateauLengthBinBits);
         //         if (datawrapped != int.MinValue) {
         //            if (datawrapped < minval || maxval < datawrapped) {
         //               datawrapped = int.MinValue; // wird doch nicht benötigt
         //               em = EncodeMode.BigBinary;
         //            }
         //         } else if (data < minval || maxval < data)
         //            em = EncodeMode.BigBinary;
         //         break;
         //   }

         //   // schon gewrapte Werte sind auf keinen Fall BigBin
         //   if (datawrapped == int.MinValue) {
         //      switch (em) {
         //         case EncodeMode.BigBinary:
         //            // minval und maxval beziehen sich hier auf den theoretischen (!) Wertebereich auf Grund der Bitanzahl
         //            //HeightElement.GetValueRangeBigBin(max, out minval, out maxval);
         //            maxval = max / 2;
         //            minval = -maxval;
         //            if (data > maxval)
         //               datawrapped = WrapDown(data);
         //            else if (data < minval)
         //               datawrapped = WrapUp(data);
         //            break;

         //         case EncodeMode.BigBinaryL1:
         //            HeightElement.GetValueRangeBigBinL1(max, out minval, out maxval);
         //            if (data > maxval)
         //               datawrapped = WrapDown(data);
         //            else if (data < minval)
         //               datawrapped = WrapUp(data);
         //            break;
         //      }
         //   }

         //   wrapped = datawrapped != int.MinValue;

         //   return wrapped ? datawrapped : data;
         //}

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

         int WrapDown(int data) {
            return data -= max + 1;
         }

         int WrapUp(int data) {
            return data += max + 1;
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

      abstract class CodingType {

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

         Length2,

         /// <summary>
         /// Spezialcodierung für Plateaulänge
         /// </summary>
         Plateau,

         /// <summary>
         /// Codierung im "festen" Binärformat (für große Zahlen)
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
      }

      /// <summary>
      /// Art der Berechnung
      /// </summary>
      public enum CalculationType {
         /// <summary>
         /// nur für Init.
         /// </summary>
         nothing,

         /// <summary>
         /// Standardberechnung für 0 kleiner Hook kleiner Max
         /// </summary>
         Std,
         /// <summary>
         /// Standardberechnung für Max kleiner/gleich Hook
         /// </summary>
         StdHookOverMax,
         /// <summary>
         /// Standardberechnung für Hook kleiner/gleich 0
         /// </summary>
         StdHookNotPos,

         /// <summary>
         /// Nachfolger nach Plateau (ddiff=0)
         /// </summary>
         PlateauFollower0,
         /// <summary>
         /// Nachfolger nach Plateau (ddiff<>0)
         /// </summary>
         PlateauFollower1,
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
      CodingTypeStd ct_std;

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

      CodingTypeStd _initialHeightUnit;

      /// <summary>
      /// hunit bei der Init. des Encoders (abh. von der Maximalhöhe; konstant; max. 256)
      /// </summary>
      public int InitialHeightUnit {
         get {
            return _initialHeightUnit.HunitValue;
         }
         private set {
            _initialHeightUnit = new CodingTypeStd(value, shrink.ShrinkValue > 0);
         }
      }

      /// <summary>
      /// akt. hunit
      /// </summary>
      public int HeightUnit { get; private set; }

      /// <summary>
      /// liefert die akt. Höhe
      /// </summary>
      public int ActualHeight {
         get {
            return nextPosition.Idx > 0 ? HeightValues[nextPosition.Idx - 1] : 0;
         }
      }

      /// <summary>
      /// Standardhöhe der akt. Zeile
      /// </summary>
      public int StdHeight { get; protected set; }

      /// <summary>
      /// Zeile der nächsten Höhe
      /// </summary>
      public int NextHeightLine {
         get {
            return nextPosition.Y;
         }
      }

      /// <summary>
      /// Spalte der nächsten Höhe
      /// </summary>
      public int NextHeightColumn {
         get {
            return nextPosition.X;
         }
      }

#endif

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
         /// reale Höhendaten müssen noch verkleinert werden
         /// </summary>
         public bool ShrinkHeightsNecessary { get; private set; }

         /// <summary>
         /// Differenz zwischen theoretischen Maximalwert (<see cref="MaxEncoderheight"/> * <see cref="ShrinkValue"/>) und <see cref="MaxRealheight"/>
         /// </summary>
         readonly int delta;

         class TopAlignedArray {

            SetType[] dat;

            public int Width, Height;

            enum SetType : byte {
               Unknown = 0,
               No,
               Yes,
            }

            public TopAlignedArray(ushort width, ushort height) {
               Width = width;
               Height = height;
               dat = new SetType[Width * Height];
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
                  dat[y * Width + x] = top ? SetType.Yes : SetType.No;
            }

            public bool Get(int x, int y) {
               if (x < 0 || y < 0)
                  return false;
               int idx = y * Width + x;
               return dat[idx] == SetType.Yes;
            }

            public string ToString(int y) {
               StringBuilder sb = new StringBuilder();
               for (int i = y * Width; i < dat.Length; i++) {
                  if (i == (y + 1) * Width)
                     break;
                  switch (dat[i]) {
                     case SetType.No: sb.Append("N"); break;
                     case SetType.Yes: sb.Append("Y"); break;
                     default: sb.Append("?"); break;
                  }
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

         TopAlignedArray TopAlignedData;


         /// <summary>
         /// 
         /// </summary>
         /// <param name="width">Spaltenanzahl des Datenbereiches</param>
         /// <param name="height">Zeilenanzahl des Datenbereiches</param>
         /// <param name="shrink">Verkleinerungsfaktor (1, 3, 5, ...)</param>
         /// <param name="maxrealheight">max. reale Höhe</param>
         /// <param name="maxencoderheight">max. Höhe mit der der Encoder rechnet</param>
         /// <param name="shrinkheightsnecessary">true, wenn reale (noch nicht verkleinerte) Höhen geliefert werden</param>
         public Shrink(ushort width, ushort height, int shrink, int maxrealheight, int maxencoderheight = 0, bool shrinkheightsnecessary = false) {
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
         public int GetEncoderHeight4RealHeight(int x, int y, int realheight) {
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
         /// setzt die Ausrichtung für diesen Wert (und liefert sie)
         /// </summary>
         /// <param name="x">Spalte</param>
         /// <param name="y">Zeile</param>
         /// <param name="heigth">Höhe</param>
         /// <param name="hook">Höhe davor + Höhe darüber - Höhe links darüber</param>
         /// <returns></returns>
         public bool SetTopAligned4Normal(int x, int y, int heigth, int hook, AlignType3 aligntype) {
            if (TopAlignIsPossible) {
               bool topaligned = false;
               if (heigth == 0) {
                  topaligned = false;
               } else if (heigth == MaxEncoderheight) {
                  topaligned = true;
               } else {
                  switch (aligntype) {
                     case AlignType3.TA111:
                     case AlignType3.TA100:
                     case AlignType3.TA010:
                     case AlignType3.TA001:
                        if (0 < hook)
                           topaligned = true;
                        break;

                     case AlignType3.TA110:
                     case AlignType3.TA000:
                     case AlignType3.TA101:
                     case AlignType3.TA011:
                        if (MaxEncoderheight <= hook)
                           topaligned = true;
                        break;
                  }
               }
               TopAlignedData.Set(x, y, topaligned);
               return topaligned;
            }
            return false;
         }

         public bool SetTopAligned4Plateau(int x, int y, int length) {
            if (TopAlignIsPossible) {
               bool topaligned = TopAlignedData.Get(x - 1, y);
               for (int i = 0; i < length; i++)
                  TopAlignedData.Set(x + i, y, topaligned);
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
         /// <param name="ddiffTis0">"Diagonaldifferenz" unter Berücksichtigung der Ausrichtung (<see cref="Ddiff"/>())</param>
         /// <returns></returns>
         public bool SetTopAligned4Plateaufollower(int x, int y, int heigth, bool ddiffTis0) {
            if (TopAlignIsPossible) {
               bool topaligned = false;
               if (heigth == 0) {
                  topaligned = false;
               } else if (heigth == MaxEncoderheight) {
                  topaligned = true;
               } else {
                  if (ddiffTis0)
                     topaligned = TopAlignedData.Get(x - 1, y);
                  else
                     topaligned = TopAlignedData.Get(x, y - 1);
               }
               TopAlignedData.Set(x, y, topaligned);
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
            return x >= 0 && y >= 0 ?
                        TopAlignedData.Get((ushort)x, (ushort)y) :
                        (x < 0 && y > 0) ?
                              TopAlignedData.Get(0, (ushort)(y - 1)) :
                              false;
         }

         /// <summary>
         /// Beginnt an dieser Position ein Plateau?
         /// </summary>
         /// <param name="x"></param>
         /// <param name="y"></param>
         /// <param name="hl">Höhe links</param>
         /// <param name="hu">Höhe darüber</param>
         /// <param name="hlu">Höhe links darüber</param>
         /// <returns></returns>
         public bool IsPlateauStart(int x, int y, int hl, int hu, int hll, int hlu, int hlll, int hllu, int hlllu) {
            if (TopAlignIsPossible && // sonst gelten die Standardregeln
                x > 0) { // bei x==0 immer Plateau

               AlignType3 tmp;
               switch (GetAlignType3(x, y)) {
                  case AlignType3.TA000:
                  case AlignType3.TA001:
                  case AlignType3.TA110:
                  case AlignType3.TA111:
                     return hl == hu;

                  case AlignType3.TA101:
                     return hl == hu + 1;

                  case AlignType3.TA010:
                     return hl + 1 == hu;

                  case AlignType3.TA100:
                     if (GetAlignType3(x - 1, y) == AlignType3.TA001 && Hook(x, y, hll, hlu, hllu, out tmp) >= 1)
                        return hl == hu;
                     else
                        return hl == hu + 1;

                  case AlignType3.TA011:
                     //if (GetAlignType3(x - 1, y) == AlignType3.TA110 && Hook(x, y, hl, hu, hlu, out tmp) >= 1)
                     //   return hl == hu;
                     //else
                     //   return hl + 1 == hu;

                     tmp = GetAlignType3(x - 1, y);
                     if (hl + 1 == hu) {
                        if (tmp == AlignType3.TA110) {
                           return Hook(x - 1, y, hll, hlu, hllu, out tmp) <= 1 &&
                                  Hook(x - 2, y, hlll, hllu, hlllu, out tmp) <= 1;
                        } else
                           return true;      // Standard
                     } else {
                        if (hl == hu)     // Spezialfall für zusätzliche Plateaus
                           if (tmp == AlignType3.TA110)
                              return Hook(x - 1, y, hll, hlu, hllu, out tmp) > 1;
                     }
                     return false;
               }

            }
            return hl == hu;
         }

         /// <summary>
         /// berechnet die "Diagonaldifferenz" unter Berücksichtigung von <see cref="TopAlignedData"/>
         /// </summary>
         /// <param name="x"></param>
         /// <param name="y"></param>
         /// <param name="hl"></param>
         /// <param name="hu"></param>
         /// <param name="usetopalign">TopAlign berücksichtigen</param>
         /// <returns></returns>
         public int Ddiff(int x, int y, int hl, int hu, bool usetopalign = true) {
            int diff = hu - hl;
            if (usetopalign && TopAlignIsPossible) {              // sonst gelten die Standardregeln
               switch (GetAlignType2(x, y)) {
                  case AlignType2.TA00:
                  case AlignType2.TA11:
                     break;

                  case AlignType2.TA10:
                     diff++;
                     break;

                  case AlignType2.TA01:
                     diff--;
                     break;
               }
            }
            return diff;
         }

         /// <summary>
         /// liefert den Hook und setzt den Align-Type für die akt. Pos.
         /// </summary>
         /// <param name="x"></param>
         /// <param name="y"></param>
         /// <param name="hl"></param>
         /// <param name="hu"></param>
         /// <param name="hlu"></param>
         /// <param name="aligntype3"></param>
         /// <returns></returns>
         public int Hook(int x, int y, int hl, int hu, int hlu, out AlignType3 aligntype3) {
            int hook = hl + hu - hlu;
            if (TopAlignIsPossible) {
               aligntype3 = GetAlignType3(x, y);
               switch (aligntype3) {
                  case AlignType3.TA001:
                     hook++;
                     break;
                  case AlignType3.TA110:
                     hook--;
                     break;
               }
            } else
               aligntype3 = AlignType3.unknown;
            return hook;
         }

         /// <summary>
         /// 0- oder Top-Align für die 3 Nachbarn (hl, hu, hlu bzw. x,y,z)
         /// </summary>
         public enum AlignType3 {
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
         /// 0- oder Top-Align für die 2 direkten Nachbarn (hl, hu)
         /// </summary>
         public enum AlignType2 {
            TA00 = 0,
            TA10 = 10,
            TA01 = 1,
            TA11 = 11,
         }

         /// <summary>
         /// liefert den Align-Typ für die Position basierend auf den 3 Nachbarwerten
         /// </summary>
         /// <param name="x"></param>
         /// <param name="y"></param>
         /// <returns></returns>
         public AlignType3 GetAlignType3(int x, int y) {
            int ht = 0;
            if (TopAlignedData.Get((ushort)x - 1, (ushort)y))
               ht += 100;
            if (TopAlignedData.Get((ushort)x, (ushort)y - 1))
               ht += 10;
            if (TopAlignedData.Get((ushort)x - 1, (ushort)y - 1))
               ht += 1;
            return (AlignType3)ht;
         }

         /// <summary>
         /// liefert den Align-Typ für die Position basierend auf den 2 direkten Nachbarwerten
         /// </summary>
         /// <param name="x"></param>
         /// <param name="y"></param>
         /// <returns></returns>
         public AlignType2 GetAlignType2(int x, int y) {
            int ht = 0;
            if (TopAlignedData.Get((ushort)x - 1, (ushort)y))
               ht += 10;
            if (TopAlignedData.Get((ushort)x, (ushort)y - 1))
               ht += 1;
            return (AlignType2)ht;
         }

      }

      public Shrink shrink;

      /// <summary>
      /// alle Höhenwerte
      /// </summary>
      public List<int> HeightValues { get; protected set; }


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
         this.shrink = new Shrink((ushort)tilesizehorz, (ushort)tilesizevert, shrink, maxrealheight, maxheightencoder, shrinkheights);

         MaxRealHeight = this.shrink.MaxRealheight;
         MaxHeight = this.shrink.MaxEncoderheight;

         TileSizeHorz = tilesizehorz;
         TileSizeVert = tilesizevert;

         HeightElement.Shrink = this.shrink.ShrinkValue;
         HeightElement.MaxEncoderheight = MaxHeight;
         HeightElement.MaxRealheight = MaxRealHeight;

#if EXPLORERFUNCTION

         Codingtype = codingtyp;
         StdHeight = 0;
         InitialHeightUnit = MaxHeight; // die korrekte hunit wird intern bestimmt

#endif

         HeightValues = new List<int>(height);
         nextPosition = new Position(tilesizehorz, tilesizevert);

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
         if (nextPosition.Idx < HeightValues.Count)
            bTileIsFull = CalculateData(nextPosition);      // die akt. Höhe oder zusätzlich mehrere folgende Höhen codieren

         if (nextPosition.Idx > HeightValues.Count)   // sicherheitshalber eingrenzen
            nextPosition.Idx = HeightValues.Count;

         return Elements.Count - elements; ;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="pos"></param>
      /// <returns>true, wenn die Endpos. rechts-unten erreicht wurde</returns>
      bool CalculateData(Position pos) {
         bool bEnd = false;
         int h = ValidHeight(pos);

         if (h >= 0) { // nur nichtnegative Höhen

            try {

               //if (ValidHeightDDiff(pos) == 0) { // die Diagonale hat konstante Höhe (gilt auch für die 1. Spalte) -> immer Plateau (ev. auch mit Länge 0)
               if (shrink.IsPlateauStart(pos.X, pos.Y,
                                         ValidHeight(pos.X - 1, pos.Y), ValidHeight(pos.X, pos.Y - 1),
                                         ValidHeight(pos.X - 2, pos.Y), ValidHeight(pos.X - 1, pos.Y - 1),
                                         ValidHeight(pos.X - 3, pos.Y), ValidHeight(pos.X - 2, pos.Y - 1),
                                         ValidHeight(pos.X - 3, pos.Y - 1)))
                  bEnd = WritePlateau(pos, ct_ddiff4plateaufollower_zero, ct_ddiff4plateaufollower_notzero);
               else
                  bEnd = WriteStandardValue(pos, h, ct_std);

            } catch (Exception ex) {
               throw new Exception(string.Format("interner Fehler bei Position {0}, Höhe {1}: {2}", pos, ValidHeight(pos), ex.Message));
            }
         } else
            throw new Exception(string.Format("negative Daten (Pos {0}) können nicht verarbeitet werden.", pos));
         return bEnd;
      }

      /// <summary>
      /// erzeugt ein <see cref="HeightElement"/> für den Standardwert
      /// </summary>
      /// <param name="pos">akt. Position</param>
      /// <param name="h">akt. Höhe</param>
      /// <param name="ct"></param>
      /// <returns></returns>
      bool WriteStandardValue(Position pos, int h, CodingTypeStd ct) {
         int hl = ValidHeight(pos.X - 1, pos.Y);
         int hu = ValidHeight(pos.X, pos.Y - 1);
         int hlu = ValidHeight(pos.X - 1, pos.Y - 1);

         Shrink.AlignType3 aligntype;
         int hook = shrink.Hook(pos.X, pos.Y, hl, hu, hlu, out aligntype);
         int ddiff = shrink.Ddiff(pos.X, pos.Y, hl, hu);
         if (ddiff == 0)
            ddiff = shrink.Ddiff(pos.X, pos.Y, hl, hu, false);
         int sgnddiff = ddiff > 0 ? 1 : ddiff < 0 ? -1 : 0;

         if (sgnddiff == 0)
            throw new Exception(string.Format("sgnddiff == 0 bei Pos {0}", pos));

         CalculationType caltype = CalculationType.nothing;
         int data = 0;
         if (shrink.MaxEncoderheight <= hook) {

            data = -sgnddiff * (h + 1);
            caltype = CalculationType.StdHookOverMax;

         } else if (hook <= 0) {

            data = -sgnddiff * h;
            caltype = CalculationType.StdHookNotPos;

         } else {

            data = -sgnddiff * (h - hook);
            caltype = CalculationType.Std;

         }

         bool wrapped = false;
         data = ValueWrap.Wrap(data, out wrapped, ct.EncodeMode);

         if (shrink.TopAlignIsPossible) {
            // Sonderfälle bei denen 0 als MaxEncoderheight bzw. MaxEncoderheight als 0 dargestellt wird vermeiden
            bool wrapspec = false;
            if (hook <= 0) {
               if (data == -sgnddiff * shrink.MaxEncoderheight)
                  wrapspec = true;
            } else if (0 < hook && hook < shrink.MaxEncoderheight) {
               switch (aligntype) {
                  case Shrink.AlignType3.TA101:
                  case Shrink.AlignType3.TA100:
                  case Shrink.AlignType3.TA010:
                  case Shrink.AlignType3.TA111:
                     if (data == sgnddiff * hook)
                        wrapspec = true;
                     break;

                  case Shrink.AlignType3.TA000:
                  case Shrink.AlignType3.TA011:
                     if (data == sgnddiff * (hook - shrink.MaxEncoderheight))
                        wrapspec = true;
                     break;
               }
            } else {
               if (data == sgnddiff * shrink.MaxEncoderheight)
                  wrapspec = true;
            }

            if (wrapspec) {
               if (data < 0)
                  data += shrink.MaxEncoderheight + 1;
               else if (data > 0)
                  data -= shrink.MaxEncoderheight + 1;
            }

            shrink.SetTopAligned4Normal(pos.X, pos.Y, h, hook, aligntype);
         }

         EncodeMode em = ValueWrap.FitEncodeMode(data, ct.EncodeMode, ct.HunitValue);

         AddHeightValue(data, caltype, pos, ct, em, int.MinValue, wrapped);

         return !pos.MoveForward();
      }

      /// <summary>
      /// erzeugt ein <see cref="HeightElement"/> für die Plateaulänge und eins für den Plateaunachfolger
      /// </summary>
      /// <param name="pos">akt. Position</param>
      /// <param name="ct_followerddiffzero">hunit-Berechnung wenn die ddiff für den Nachfolger 0 ist</param>
      /// <param name="ct_followerddiffnotzero">hunit-Berechnung wenn die ddiff für den Nachfolger ungleich 0 ist</param>
      /// <returns>true, wenn die Endpos. rechts-unten erreicht wurde</returns>
      bool WritePlateau(Position pos, CodingTypePlateauFollowerZero ct_followerddiffzero, CodingTypePlateauFollowerNotZero ct_followerddiffnotzero) {
         bool bEnd = GetPlateauLength(pos, out int length);

         shrink.SetTopAligned4Plateau(pos.X, pos.Y, length);

         // pos steht am Anfang des Plateaus bzw., bei Länge 0, schon auf dem Follower
         // also zeigt pos.X+length auf die Pos. des Followers

         // Plateaulänge codieren
         HeightElement he = new HeightElement(new HeightElement.HeightElementInfo(HeightElement.Typ.Plateau, length, TileSizeHorz, Elements, shrink.IsTopAligned(pos.X, pos.Y), pos.X, pos.Y));
         Elements.Add(he);
         bool bLineFilled = pos.X + length >= TileSizeHorz;
         bEnd = !pos.MoveForward(length);

         if (!bLineFilled) { // Nachfolgewert bestimmen
            if (!bEnd) {
               int h = ValidHeight(pos);
               int hu = ValidHeight(pos.X, pos.Y - 1);
               int hl = ValidHeight(pos.X - 1, pos.Y);

               int hlu_start = ValidHeight(pos.X - length - 1, pos.Y - 1);


               int ddiff = pos.X == 0 ?
                                 0 : // wegen virt. Spalte
                                 shrink.Ddiff(pos.X, pos.Y, hl, hu);

               CodingType ct;
               if (ddiff != 0)
                  ct = ct_followerddiffnotzero;
               else
                  ct = ct_followerddiffzero;

               // Nachfolger codieren
               int data = h - hu;  // vdiff

               EncodeMode em = ct.EncodeMode;
               bool wrapped = false;
               // "einfaches" Wrapping
               //if (data > shrink.MaxEncoderheight / 2)
               //   data -= shrink.MaxEncoderheight + 1;
               //else if (data < -shrink.MaxEncoderheight / 2)
               //   data += shrink.MaxEncoderheight + 1;
               CalculationType caltyp2 = CalculationType.nothing;
               data = ValueWrap.Wrap(data, out wrapped, em);

               if (ddiff != 0) {

                  caltyp2 = CalculationType.PlateauFollower1;
                  if (ddiff > 0)
                     data = -data;

                  if (shrink.TopAlignIsPossible) {
                     // Sonderfälle vermeiden
                     bool wrapspec = false;
                     switch (shrink.GetAlignType2(pos.X, pos.Y)) {
                        case Shrink.AlignType2.TA11:
                        case Shrink.AlignType2.TA01:
                           if (h == 0)
                              if (ddiff > 0) {
                                 if (data == hu)
                                    wrapspec = true;
                              } else {
                                 if (data == -hu)
                                    wrapspec = true;
                              }
                           break;

                        case Shrink.AlignType2.TA10:
                        case Shrink.AlignType2.TA00:
                           // Sonderfälle für MaxEncoderheight
                           if (h == shrink.MaxEncoderheight) {  // statt MaxEncoderheight wird 0 angezeigt
                              if (ddiff > 0) {
                                 if (data == -(shrink.MaxEncoderheight - hu))  // data <= 0
                                    wrapspec = true;
                              } else {
                                 if (data == shrink.MaxEncoderheight - hu)  // data >= 0
                                    wrapspec = true;
                              }
                           }
                           break;
                     }

                     if (wrapspec) {
                        if (data < 0)
                           data += shrink.MaxEncoderheight + 1;
                        else if (data > 0)
                           data -= shrink.MaxEncoderheight + 1;
                     }

                     shrink.SetTopAligned4Plateaufollower(pos.X, pos.Y, h, false);
                  }

               } else {

                  caltyp2 = CalculationType.PlateauFollower0;

                  if (shrink.TopAlignIsPossible) {

                     // spez. Berechnung (normalerweise:  if (data < 0) data++)
                     switch (shrink.GetAlignType2(pos.X, pos.Y)) {
                        case Shrink.AlignType2.TA11:
                        case Shrink.AlignType2.TA00:
                           // hl=hu; data=h-hu -> data=h-hl -> wegen h <> hl folgt data <> 0, d.h. data<0 kann um 1 vergrößert werden
                           if (data < 0)
                              data++;
                           break;

                        case Shrink.AlignType2.TA10:
                           // hl=hu+1; data=h-hu -> data=h-(hl-1)=h-hl+1 -> wegen h <> hl folgt data <> 1, d.h. data>1 kann um 1 verringert werden
                           if (data > 0)
                              data--;
                           break;

                        case Shrink.AlignType2.TA01:
                           // hl+1=hu; data=h-hu -> data=h-(hl+1)=h-hl-1 -> wegen h <> hl folgt data <> -1, d.h. 
                           //    data > -1 kann um 1 vergrößert werden
                           //    data < -1 kann sogar um 2 vergrößert werden
                           if (data >= 0)
                              data++;
                           else
                              data += 2;
                           break;
                     }

                     // Sonderfälle vermeiden
                     bool wrapspec = false;
                     switch (shrink.GetAlignType2(pos.X, pos.Y)) {
                        case Shrink.AlignType2.TA11:
                           if (h == 0 && data <= 0)
                              wrapspec = true;
                           break;

                        case Shrink.AlignType2.TA10:
                           if (h == 0 && data <= 0)
                              wrapspec = true;
                           break;

                        case Shrink.AlignType2.TA01:
                        case Shrink.AlignType2.TA00:
                           if (h == shrink.MaxEncoderheight && data > 0)
                              wrapspec = true;
                           break;
                     }

                     if (wrapspec) {
                        if (data < 0)
                           data += shrink.MaxEncoderheight + 1;
                        else if (data > 0)
                           data -= shrink.MaxEncoderheight + 1;
                     }

                     shrink.SetTopAligned4Plateaufollower(pos.X, pos.Y, h, true);

                  } else {

                     if (data < 0)
                        data++;

                  }
               }

               em = ValueWrap.FitEncodeMode(data, em, ct.HunitValue, he.PlateauBinBits);
               AddHeightValue(data, caltyp2, pos, ct, em, ddiff, wrapped);

               bEnd = !pos.MoveForward();
            }
         }

         return bEnd;
      }

      /// <summary>
      /// ermittelt die Länge eines Plateaus ab der Startposition (ev. auch 0)
      /// </summary>
      /// <param name="pos">Startpos.</param>
      /// <param name="length">Länge</param>
      /// <returns>true, wenn die Endpos. rechts-unten erreicht wurde</returns>
      bool GetPlateauLength(Position startpos, out int length) {
         Position tst = new Position(startpos);
         length = 0;
         int value = -1;
         bool bEnd = false;

         while (tst.Idx < HeightValues.Count) {
            if (value < 0)
               value = ValidHeight(tst.X - 1, tst.Y);

            if (HeightValues[tst.Idx] != value)
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
      /// fügt eine neues <see cref="HeightElement"/> an die Liste <see cref="Elements"/> an und registriert den Datenwert im <see cref="CodingType"/>
      /// </summary>
      /// <param name="data">Datenwert</param>
      /// <param name="caltype">Berechnungstyp (Plateau-Nachfolger oder andere)</param>
      /// <param name="pos">Pos. in der Kachel</param>
      /// <param name="ct">Codierart</param>
      /// <param name="em_explicit">ausdrücklicher Codiermodus bei einem Plateau-Nachfolger</param>
      /// <param name="extdata">ddiff für den Plateau-Nachfolger</param>
      /// <param name="wrapped_info">true, falls der Datenwert gewrapt ist</param>
      void AddHeightValue(int data,
                          CalculationType caltype,
                          Position pos,
                          CodingType ct,
                          EncodeMode em_explicit = EncodeMode.notdefined,
                          int extdata = int.MinValue,
                          bool wrapped_info = false) {
         HeightElement elem = null;
         bool topaligned = shrink.IsTopAligned(pos.X, pos.Y);

         switch (caltype) {
            case CalculationType.PlateauFollower0:
            case CalculationType.PlateauFollower1:
               switch (em_explicit) {
                  case EncodeMode.Hybrid:
                     elem = new HeightElement(new HeightElement.HeightElementInfo(HeightElement.Typ.PlateauFollower, data, extdata, caltype, EncodeMode.Hybrid, wrapped_info, ct.HunitValue, topaligned, pos.X, pos.Y));
                     break;

                  case EncodeMode.Length0:
                  case EncodeMode.Length1:
                  case EncodeMode.Length2:
                     elem = new HeightElement(new HeightElement.HeightElementInfo(HeightElement.Typ.PlateauFollower, data, extdata, caltype, em_explicit, wrapped_info, topaligned, pos.X, pos.Y));
                     break;

                  case EncodeMode.BigBinary:
                     elem = new HeightElement(new HeightElement.HeightElementInfo(HeightElement.Typ.PlateauFollower, data, caltype, EncodeMode.BigBinary, wrapped_info, extdata, topaligned, pos.X, pos.Y));
                     break;

                  case EncodeMode.BigBinaryL1:
                     elem = new HeightElement(new HeightElement.HeightElementInfo(HeightElement.Typ.PlateauFollower, data, caltype, EncodeMode.BigBinaryL1, wrapped_info, extdata, topaligned, pos.X, pos.Y));
                     break;

                  case EncodeMode.BigBinaryL2:
                     elem = new HeightElement(new HeightElement.HeightElementInfo(HeightElement.Typ.PlateauFollower, data, caltype, EncodeMode.BigBinaryL2, wrapped_info, extdata, topaligned, pos.X, pos.Y));
                     break;
               }
               break;

            default: // Standardwert
               switch (em_explicit) {
                  case EncodeMode.Hybrid:
                     elem = new HeightElement(new HeightElement.HeightElementInfo(HeightElement.Typ.Value, data, caltype, EncodeMode.Hybrid, wrapped_info, ct.HunitValue, topaligned, pos.X, pos.Y));
                     break;

                  case EncodeMode.Length0:
                  case EncodeMode.Length1:
                     elem = new HeightElement(new HeightElement.HeightElementInfo(HeightElement.Typ.Value, data, caltype, em_explicit, wrapped_info, topaligned, pos.X, pos.Y));
                     break;

                  case EncodeMode.BigBinary:
                     elem = new HeightElement(new HeightElement.HeightElementInfo(HeightElement.Typ.Value, data, caltype, EncodeMode.BigBinary, wrapped_info, topaligned, pos.X, pos.Y));
                     break;

                  case EncodeMode.BigBinaryL1:
                     elem = new HeightElement(new HeightElement.HeightElementInfo(HeightElement.Typ.Value, data, caltype, EncodeMode.BigBinaryL1, wrapped_info, topaligned, pos.X, pos.Y));
                     break;
               }
               break;
         }

         if (elem != null) {
            Elements.Add(elem);

            if (ct != null) {

               ct.AddValue(data);

#if EXPLORERFUNCTION

               List<string> infolst = null;

               if (ct is CodingTypeStd) {
                  infolst = CodingTypeStd_Info;
               } else if (ct is CodingTypePlateauFollowerNotZero) {
                  infolst = CodingTypePlateauFollowerNotZero_Info;
               } else if (ct is CodingTypePlateauFollowerZero) {
                  infolst = CodingTypePlateauFollowerZero_Info;
               }

               if (infolst.Count == 0)
                  infolst.Add("Bonus " + ct.Bonus.ToString());

               string info = string.Format("Position=[{0},{1}], ElemCount={2}, data={3}, eval={4}, HunitValue={5}, SumH={6}, SumL={7}",
                                            pos.X, pos.Y, ct.ElemCount, data, ct.Eval, ct.HunitValue, ct.SumH, ct.SumL);
               if (ct.ExtInfo4LastAdd.Length > 0)
                  info += "; [" + ct.ExtInfo4LastAdd + "]";

               infolst.Add(info);

#endif
            }
         }
      }

      #region spezielle Höhendifferenzen

      /// <summary>
      /// liefert die horizontale Höhendifferenz (zur Vorgängerhöhe)
      /// </summary>
      /// <param name="pos"></param>
      /// <returns></returns>
      int ValidHeightHDiff(Position pos) {
         return ValidHeightHDiff(pos.X, pos.Y);
      }

      /// <summary>
      /// liefert die horizontale Höhendifferenz (zur Vorgängerhöhe)
      /// </summary>
      /// <param name="col"></param>
      /// <param name="line"></param>
      /// <returns></returns>
      int ValidHeightHDiff(int col, int line) {
         return ValidHeight(col, line) - ValidHeight(col - 1, line);
      }

      /// <summary>
      /// liefert die vertikale Höhendifferenz (zur darüber liegenden Höhe)
      /// </summary>
      /// <param name="pos"></param>
      /// <returns></returns>
      int ValidHeightVDiff(Position pos) {
         return ValidHeightVDiff(pos.X, pos.Y);
      }

      /// <summary>
      /// liefert die vertikale Höhendifferenz (zur darüber liegenden Höhe)
      /// </summary>
      /// <param name="col"></param>
      /// <param name="line"></param>
      /// <returns></returns>
      int ValidHeightVDiff(int col, int line) {
         return ValidHeight(col, line) - ValidHeight(col, line - 1);
      }

      /// <summary>
      /// liefert die diagonale Höhendifferenz (der darüber liegenden Höhe zur Vorgängerhöhe)
      /// </summary>
      /// <param name="pos"></param>
      /// <returns></returns>
      int ValidHeightDDiff(Position pos) {
         return ValidHeightDDiff(pos.X, pos.Y);
      }

      /// <summary>
      /// liefert die diagonale Höhendifferenz (der darüber liegenden Höhe zur Vorgängerhöhe)
      /// </summary>
      /// <param name="col"></param>
      /// <param name="line"></param>
      /// <returns></returns>
      int ValidHeightDDiff(int col, int line) {
         return ValidHeight(col, line - 1) - ValidHeight(col - 1, line);
      }

      #endregion

      #region Zugriff auf Höhenwerte

      /// <summary>
      /// liefert die Höhe aus Spalte und Zeile (Spalte und Zeile müssen gültige Werte sein, da intern der Index verwendet wird)
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns>negativ, wenn ungültig</returns>
      public int Height(int x, int y) {
         int idx = TileSizeHorz * y + x;
         int h = 0 <= idx && idx < HeightValues.Count ?
                        HeightValues[idx] :
                        -1;
         return shrink.GetEncoderHeight4RealHeight(x, y, h);
      }

      /// <summary>
      /// liefert die Höhe bezüglich der Position (mit dem Delta muss sich eine gültige Position ergeben, da intern der Index verwendet wird)
      /// </summary>
      /// <param name="pos"></param>
      /// <param name="deltax">Spalten-Delta zur Position</param>
      /// <param name="deltay">Zeilen-Delta zur Position</param>
      /// <returns>negativ, wenn ungültig</returns>
      int ValidHeight(Position pos) {
         if (pos.X < 0 || pos.X >= pos.Width ||
             pos.Y < 0 || pos.Y >= pos.Height)
            return -1;
         return Height(pos.X, pos.Y);
      }

      /// <summary>
      /// liefert auch für ungültige Spalten und Zeilen eine verarbeitbare Höhe, d.h. außerhalb von <see cref="TileSizeHorz"/> bzw. <see cref="TileSizeVert"/> immer 0 
      /// bzw. bei Spalte -1 die virtuelle Spalte (Spalte 0 der Vorgängerzeile)
      /// </summary>
      /// <param name="column"></param>
      /// <param name="line"></param>
      /// <returns></returns>
      public int ValidHeight(int column, int line) {
         if (0 <= column && column < TileSizeHorz &&
             0 <= line && line < TileSizeVert) { // innerhalb des Standardbereiches
            int h = Height(column, line);
            if (h >= 0) {
               return h;
            }
         }
         if (column == -1 &&
             0 <= line && line < TileSizeVert) { // virtuelle Spalte
            int h = Height(0, line - 1);
            return h >= 0 ? h : 0;
         }
         return 0;
      }

      #endregion

#if EXPLORERFUNCTION

      #region zum Ermitteln der Bitfolgen

      static public List<byte> LengthCoding0(int data, int shrink = 1, int maxrealheight = 0) {
         HeightElement.Shrink = shrink;
         HeightElement.MaxRealheight = maxrealheight;
         return new List<byte>(new HeightElement(new HeightElement.HeightElementInfo(HeightElement.Typ.Value, data, CalculationType.nothing, EncodeMode.Length0, false, false, int.MinValue, int.MinValue)).Bits);
      }

      static public List<byte> LengthCoding1(int data, int shrink = 1, int maxrealheight = 0) {
         HeightElement.Shrink = shrink;
         HeightElement.MaxRealheight = maxrealheight;
         return new List<byte>(new HeightElement(new HeightElement.HeightElementInfo(HeightElement.Typ.Value, data, CalculationType.nothing, EncodeMode.Length1, false, false, int.MinValue, int.MinValue)).Bits);
      }

      static public List<byte> LengthCoding2(int data, int shrink = 1, int maxrealheight = 0) {
         HeightElement.Shrink = shrink;
         HeightElement.MaxRealheight = maxrealheight;
         return new List<byte>(new HeightElement(new HeightElement.HeightElementInfo(HeightElement.Typ.Value, data, CalculationType.nothing, EncodeMode.Length2, false, false, int.MinValue, int.MinValue)).Bits);
      }

      static public List<byte> HybridCoding(int data, int maxHeight, int hunit, int shrink = 1, int maxrealheight = 0) {
         HeightElement.MaxEncoderheight = maxHeight;
         HeightElement.Shrink = shrink;
         HeightElement.MaxRealheight = maxrealheight;
         return new List<byte>(new HeightElement(new HeightElement.HeightElementInfo(HeightElement.Typ.Value, data, CalculationType.nothing, EncodeMode.Hybrid, false, hunit, false, int.MinValue, int.MinValue)).Bits);
      }

      static public List<byte> BigValueCodingHybrid(int data, int maxHeight, int shrink = 1, int maxrealheight = 0) {
         HeightElement.MaxEncoderheight = maxHeight;
         HeightElement.Shrink = shrink;
         HeightElement.MaxRealheight = maxrealheight;
         return new List<byte>(new HeightElement(new HeightElement.HeightElementInfo(HeightElement.Typ.Value, data, CalculationType.nothing, EncodeMode.BigBinary, false, false, int.MinValue, int.MinValue)).Bits);
      }

      static public List<byte> BigValueCodingLength0(int data, int maxHeight, int shrink = 1, int maxrealheight = 0) {
         HeightElement.MaxEncoderheight = maxHeight;
         HeightElement.Shrink = shrink;
         HeightElement.MaxRealheight = maxrealheight;
         return new List<byte>(new HeightElement(new HeightElement.HeightElementInfo(HeightElement.Typ.Value, data, CalculationType.nothing, EncodeMode.BigBinary, false, false, int.MinValue, int.MinValue)).Bits);
      }

      static public List<byte> BigValueCodingLength1(int data, int maxHeight, int shrink = 1, int maxrealheight = 0) {
         HeightElement.MaxEncoderheight = maxHeight;
         HeightElement.Shrink = shrink;
         HeightElement.MaxRealheight = maxrealheight;
         return new List<byte>(new HeightElement(new HeightElement.HeightElementInfo(HeightElement.Typ.Value, data, CalculationType.nothing, EncodeMode.BigBinaryL1, false, false, int.MinValue, int.MinValue)).Bits);
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
                              InitialHeightUnit,
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
