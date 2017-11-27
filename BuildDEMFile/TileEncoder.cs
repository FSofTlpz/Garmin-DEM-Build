// höchstwahrscheinlich unnötige Teile
//#define INCLUDENOTNEEDED

// Funktionen nur zur "Erforschung"
//#define EXPLORERFUNCTION

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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

         /// <summary>
         /// Typ des Elements
         /// </summary>
         public Typ ElementTyp { get; private set; }
         /// <summary>
         /// Datenwert
         /// </summary>
         public int Data { get; private set; }
         /// <summary>
         /// Codierart
         /// </summary>
         public EncodeMode Encoding { get; private set; }
         /// <summary>
         /// HUnit, gültig nur bei Hybridcodierung
         /// </summary>
         public int HUnit { get; private set; }
         /// <summary>
         /// Bit-Liste der Codierung
         /// </summary>
         public List<byte> Bits { get; private set; }
         /// <summary>
         /// Element bezieht sich auf diese Zeile
         /// </summary>
         public int Line { get; private set; }
         /// <summary>
         /// Element bezieht sich auf diese Spalte
         /// </summary>
         public int Column { get; private set; }
         /// <summary>
         /// Wurde der Datenwert gewrapped?
         /// </summary>
         public bool WrappedValue { get; private set; }
         /// <summary>
         /// Berechnungstyp
         /// </summary>
         public CalculationType CalculationType { get; private set; }

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


         /// <summary>
         /// Dieser Konstruktor kann nur klassenintern aufgerufen werden. Das erfolgt über die statischen CreateHeightElement-Funktionen.
         /// </summary>
         /// <param name="typ"></param>
         /// <param name="data"></param>
         /// <param name="caltype"></param>
         /// <param name="wrapped"></param>
         /// <param name="encoding"></param>
         /// <param name="column"></param>
         /// <param name="line"></param>
         /// <param name="hunit"></param>
         /// <param name="maxheight"></param>
         /// <param name="linelength"></param>
         /// <param name="lastplateauelem"></param>
         /// <param name="plateaufollowerddiff"></param>
         protected HeightElement(Typ typ,
                                 int data,
                                 CalculationType caltype,
                                 bool wrapped,
                                 EncodeMode encoding,
                                 int column,
                                 int line,
                                 int hunit,
                                 int maxheight,
                                 int linelength,
                                 HeightElement lastplateauelem,
                                 int plateaufollowerddiff) {
            Bits = new List<byte>();

            Column = column;
            Line = line;
            ElementTyp = typ;
            Data = data;
            WrappedValue = wrapped;
            Encoding = encoding;
            HUnit = hunit;

            PlateauFollowerDdiff = plateaufollowerddiff;
            if (lastplateauelem != null)
               PlateauTable4Tile = lastplateauelem.PlateauTable4Tile;
            else
               PlateauTable4Tile = new PlateauTable();

            CalculationType = caltype;

            switch (typ) {
               case Typ.Value:
               case Typ.PlateauFollower:
                  switch (Encoding) {
                     case EncodeMode.Hybrid:
                        EncodeHybrid(data, maxheight, hunit);
                        break;

                     case EncodeMode.Length0:
                        EncodeLength0(data);
                        break;

                     case EncodeMode.Length1:
                        EncodeLength1(data);
                        break;

                     case EncodeMode.Length2:
                        EncodeLength0(-data);
                        break;

                     case EncodeMode.BigBinary:
                     case EncodeMode.BigBinaryL1:
                        EncodeBigBin(data, maxheight, typ == Typ.PlateauFollower);
                        break;

                     case EncodeMode.BigBinaryL2:
                        EncodeBigBin(-data, maxheight, typ == Typ.PlateauFollower);
                        break;

                     default:
                        throw new Exception(string.Format("Falscher Codiertyp: {0}.", Encoding));
                  }
                  break;

               case Typ.Plateau:
                  EncodePlateau(data, column, line, linelength);
                  break;

            }
         }

         #region CreateHeightElement-Funktionen zum Erzeugen eines Objektes

         /// <summary>
         /// erzeugt ein <see cref="HeightElement"/> für einen "normalen" Wert mit Hybridcodierung
         /// </summary>
         /// <param name="data">Datenwert</param>
         /// <param name="wrapped">Wert wurde gewrapped</param>
         /// <param name="maxheight">max. Höhe der Kachel (laut Def.)</param>
         /// <param name="hunit">Heightunit für den Wert</param>
         /// <param name="column">akt. Spalte</param>
         /// <param name="line">akt. Zeile</param>
         /// <returns></returns>
         static public HeightElement CreateHeightElement_ValueH(int data, CalculationType caltype, bool wrapped, int maxheight, int hunit, int column, int line) {
            return new HeightElement(HeightElement.Typ.Value, data, caltype, wrapped, EncodeMode.Hybrid, column, line, hunit, maxheight, int.MinValue, null, int.MinValue);
         }

         /// <summary>
         /// erzeugt ein <see cref="HeightElement"/> für den Plateaunachfolger (analog einem ganz normalen Wert)
         /// </summary>
         /// <param name="data">Datenwert für den Plateaunachfolger</param>
         /// <param name="wrapped">Wert wurde gewrapped</param>
         /// <param name="maxheight">max. Höhe der Kachel (laut Def.)</param>
         /// <param name="hunit">Heightunit für den Plateaunachfolger</param>
         /// <param name="ddiff">Diagonaldiff. für den Plateaunachfolger</param>
         /// <param name="column">akt. Spalte</param>
         /// <param name="line">akt. Zeile</param>
         /// <returns></returns>
         static public HeightElement CreateHeightElement_PlateauFollowerH(int data, CalculationType caltype, bool wrapped, int maxheight, int hunit, int ddiff, int column, int line) {
            return new HeightElement(HeightElement.Typ.PlateauFollower, data, caltype, wrapped, EncodeMode.Hybrid, column, line, hunit, maxheight, int.MinValue, null, ddiff);
         }

         /// <summary>
         /// erzeugt ein <see cref="HeightElement"/> für einen "normalen" Wert mit Längencodierung
         /// </summary>
         /// <param name="data">Datenwert</param>
         /// <param name="wrapped">Wert wurde gewrapped</param>
         /// <param name="em">Längencodierung Variante 0 oder 1</param>
         /// <param name="column">akt. Spalte</param>
         /// <param name="line">akt. Zeile</param>
         /// <returns></returns>
         static public HeightElement CreateHeightElement_ValueL(int data, CalculationType caltype, bool wrapped, EncodeMode em, int column, int line) {
            return new HeightElement(HeightElement.Typ.Value, data, caltype, wrapped, em, column, line, int.MinValue, int.MinValue, int.MinValue, null, int.MinValue);
         }

         /// <summary>
         /// erzeugt ein <see cref="HeightElement"/> für den Plateaunachfolger in Längencodierung
         /// </summary>
         /// <param name="data">Datenwert</param>
         /// <param name="wrapped">Wert wurde gewrapped</param>
         /// <param name="em">Längencodierung</param>
         /// <param name="ddiff">Diagonaldiff. für den Plateaunachfolger</param>
         /// <param name="column">akt. Spalte</param>
         /// <param name="line">akt. Zeile</param>
         /// <returns></returns>
         static public HeightElement CreateHeightElement_PlateauFollowerL(int data, CalculationType caltype, bool wrapped, EncodeMode em, int ddiff, int column, int line) {
            return new HeightElement(HeightElement.Typ.PlateauFollower, data, caltype, wrapped, em, column, line, int.MinValue, int.MinValue, int.MinValue, null, ddiff);
         }

         /// <summary>
         /// erzeugt ein <see cref="HeightElement"/> für einen "normalen", aber großen Wert mit Hybridcodierung
         /// </summary>
         /// <param name="data">Datenwert</param>
         /// <param name="wrapped">Wert wurde gewrapped</param>
         /// <param name="maxheight">max. Höhe der Kachel (laut Def.)</param>
         /// <param name="column">akt. Spalte</param>
         /// <param name="line">akt. Zeile</param>
         /// <returns></returns>
         static public HeightElement CreateHeightElement_BigValue(int data, CalculationType caltype, bool wrapped, int maxheight, int column, int line) {
            return new HeightElement(HeightElement.Typ.Value, data, caltype, wrapped, EncodeMode.BigBinary, column, line, int.MinValue, maxheight, int.MinValue, null, int.MinValue);
         }

         /// <summary>
         /// erzeugt ein <see cref="HeightElement"/> für einen "normalen", aber großen Wert mit Hybridcodierung (an Stelle von <see cref="EncodeMode.Length1"/>)
         /// </summary>
         /// <param name="data">Datenwert</param>
         /// <param name="wrapped">Wert wurde gewrapped</param>
         /// <param name="maxheight">max. Höhe der Kachel (laut Def.)</param>
         /// <param name="column">akt. Spalte</param>
         /// <param name="line">akt. Zeile</param>
         /// <returns></returns>
         static public HeightElement CreateHeightElement_BigValueL1(int data, CalculationType caltype, bool wrapped, int maxheight, int column, int line) {
            return new HeightElement(HeightElement.Typ.Value, data, caltype, wrapped, EncodeMode.BigBinaryL1, column, line, int.MinValue, maxheight, int.MinValue, null, int.MinValue);
         }

         /// <summary>
         /// erzeugt ein <see cref="HeightElement"/> für einen "normalen", aber großen Wert mit Hybridcodierung
         /// </summary>
         /// <param name="data">Datenwert</param>
         /// <param name="wrapped">Wert wurde gewrapped</param>
         /// <param name="maxheight">max. Höhe der Kachel (laut Def.)</param>
         /// <param name="ddiff">Diagonaldiff. für den Plateaunachfolger</param>
         /// <param name="column">akt. Spalte</param>
         /// <param name="line">akt. Zeile</param>
         /// <returns></returns>
         static public HeightElement CreateHeightElement_PlateauFollowerBigValue(int data, CalculationType caltype, bool wrapped, int maxheight, int ddiff, int column, int line) {
            return new HeightElement(HeightElement.Typ.PlateauFollower, data, caltype, wrapped, EncodeMode.BigBinary, column, line, int.MinValue, maxheight, int.MinValue, null, ddiff);
         }

         /// <summary>
         /// erzeugt ein <see cref="HeightElement"/> für einen "normalen", aber großen Wert mit Hybridcodierung (an Stelle von <see cref="EncodeMode.Length1"/>)
         /// </summary>
         /// <param name="data">Datenwert</param>
         /// <param name="wrapped">Wert wurde gewrapped</param>
         /// <param name="maxheight">max. Höhe der Kachel (laut Def.)</param>
         /// <param name="ddiff">Diagonaldiff. für den Plateaunachfolger</param>
         /// <param name="column">akt. Spalte</param>
         /// <param name="line">akt. Zeile</param>
         /// <returns></returns>
         static public HeightElement CreateHeightElement_PlateauFollowerBigValueL1(int data, CalculationType caltype, bool wrapped, int maxheight, int ddiff, int column, int line) {
            return new HeightElement(HeightElement.Typ.PlateauFollower, data, caltype, wrapped, EncodeMode.BigBinaryL1, column, line, int.MinValue, maxheight, int.MinValue, null, ddiff);
         }

         /// <summary>
         /// erzeugt ein <see cref="HeightElement"/> für einen "normalen", aber großen Wert mit Hybridcodierung (an Stelle von <see cref="EncodeMode.Length2"/>)
         /// </summary>
         /// <param name="data">Datenwert</param>
         /// <param name="wrapped">Wert wurde gewrapped</param>
         /// <param name="maxheight">max. Höhe der Kachel (laut Def.)</param>
         /// <param name="ddiff">Diagonaldiff. für den Plateaunachfolger</param>
         /// <param name="column">akt. Spalte</param>
         /// <param name="line">akt. Zeile</param>
         /// <returns></returns>
         static public HeightElement CreateHeightElement_PlateauFollowerBigValueL2(int data, CalculationType caltype, bool wrapped, int maxheight, int ddiff, int column, int line) {
            return new HeightElement(HeightElement.Typ.PlateauFollower, data, caltype, wrapped, EncodeMode.BigBinaryL2, column, line, int.MinValue, maxheight, int.MinValue, null, ddiff);
         }

         /// <summary>
         /// erzeugt ein <see cref="HeightElement"/> für die Plateaulänge
         /// </summary>
         /// <param name="length">Plateaulänge</param>
         /// <param name="column">akt. Spalte</param>
         /// <param name="line">akt. Zeile</param>
         /// <param name="linelength">Zeilenlänge, d.h. Kachelbreite</param>
         /// <param name="oldheightelements">Liste aller bisher ermittelten <see cref="HeightElement"/>'s</param>
         /// <returns></returns>
         static public HeightElement CreateHeightElement_Plateau(int length, int column, int line, int linelength, IList<HeightElement> oldheightelements) {
            HeightElement last = null;
            for (int i = oldheightelements.Count - 1; i >= 0; i--) // letztes Plateau-Element suchen
               if (oldheightelements[i].ElementTyp == HeightElement.Typ.Plateau) {
                  last = oldheightelements[i];
                  break;
               }
            return new HeightElement(HeightElement.Typ.Plateau, length, CalculationType.nothing, false, EncodeMode.Plateau, column, line, int.MinValue, int.MinValue, linelength, last, int.MinValue);
         }

         #endregion

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
         /// <param name="maxheight"></param>
         /// <param name="hunit"></param>
         void EncodeHybrid(int data, int maxheight, int hunit) {
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

            int maxm = GetMaxLengthZeroBits(maxheight);
            if (m <= maxm) {
               EncodeLength(m);                       // längencodierten Teil speichern
               EncodeBinary((uint)bin, hunitexp);           // binär codierten Teil speichern
               Bits.Add((byte)(data > 0 ? 1 : 0));    // Vorzeichen speichern
            } else
               throw new Exception(string.Format("Der Betrag des Wertes {0} ist für die Codierung {1} bei der Maximalhöhe {2} und mit Heightunit {3} zu groß.",
                                                   data,
                                                   EncodeMode.Hybrid,
                                                   maxheight,
                                                   hunit));
         }

         /// <summary>
         /// codiert eine "große" Zahl binär mit führender 0-Bitfolge
         /// </summary>
         /// <param name="data"></param>
         /// <param name="maxheight"></param>
         /// <param name="follower">für Plateaunachfolger</param>
         void EncodeBigBin(int data, int maxheight, bool plateaufollower) {
            if (data == 0)
               throw new Exception(string.Format("Der Wert 0 kann nicht in der Codierung {0} erzeugt werden.", EncodeMode.BigBinary));

            int length0 = GetMaxLengthZeroBits(maxheight) + 1; // 1 Bit mehr als Max. als BigBin-Kennung
            if (plateaufollower)
               length0--;
            EncodeLength(length0); // 0-Bits und 1-Bit

            int min, max;
            if (Encoding == EncodeMode.BigBinaryL1) {
               data = 1 - data; // Umwandlung, um die gleiche Codierfunktion verwendet zu können
               GetValueRangeBigBin(maxheight, out min, out max);
            } else {
               GetValueRangeBigBin(maxheight, out min, out max);
            }
            if (data < min || max < data) {
               if (data > 0)
                  data -= maxheight + 1;
               else if (data < 0)
                  data += maxheight + 1;
            }

            int bitcount = GetBigBinBits(maxheight);
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

         /// <summary>
         /// liefert die max. mögliche Anzahl 0-Bits für die Hybrid- oder Längencodierung
         /// </summary>
         /// <param name="maxheight"></param>
         /// <returns></returns>
         static int GetMaxLengthZeroBits(int maxheight) {
            if (maxheight < 2)
               return 15;
            if (maxheight < 4)
               return 16;
            if (maxheight < 8)
               return 17;
            if (maxheight < 16)
               return 18;
            if (maxheight < 32)
               return 19;
            if (maxheight < 64)
               return 20;
            if (maxheight < 128)
               return 21;
            if (maxheight < 256)
               return 22;
            if (maxheight < 512)
               return 25;
            if (maxheight < 1024)
               return 28;
            if (maxheight < 2048)
               return 31;
            if (maxheight < 4096)
               return 34;
            if (maxheight < 8192)
               return 37;
            if (maxheight < 16384)
               return 40;
            return 43;
         }

         /// <summary>
         /// liefert die Anzahl der binären Bits (einschließlich Vorzeichenbit) für BigBin-Zahlen (1 + int(ld(max)))
         /// </summary>
         /// <param name="maxheight"></param>
         /// <returns></returns>
         static int GetBigBinBits(int maxheight) {
            if (maxheight < 2)
               return 1;
            else if (maxheight < 4)
               return 2;
            else if (maxheight < 8)
               return 3;
            else if (maxheight < 16)
               return 4;
            else if (maxheight < 32)
               return 5;
            else if (maxheight < 64)
               return 6;
            else if (maxheight < 128)
               return 7;
            else if (maxheight < 256)
               return 8;
            else if (maxheight < 512)
               return 9;
            else if (maxheight < 1024)
               return 10;
            else if (maxheight < 2048)
               return 11;
            else if (maxheight < 4096)
               return 12;
            else if (maxheight < 8192)
               return 13;
            else if (maxheight < 16384)
               return 14;
            else
               return 15;
         }

         /// <summary>
         /// liefert den kleinsten und den größten verwendbaren Wert bei Hybridcodierung
         /// </summary>
         /// <param name="hunit"></param>
         /// <param name="maxheight">max. Kachelhöhe</param>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="max">größter verwendbarer Wert</param>
         /// <param name="iPlateauLengthBinBits">Anzahl der Binbits für die ev. vorausgehende Plateaulänge</param>
         public static void GetValueRangeHybrid(int hunit, int maxheight, out int min, out int max, int iPlateauLengthBinBits) {
            int lbits = GetMaxLengthZeroBits(maxheight);
            if (iPlateauLengthBinBits >= 0) // bei Plateaufollower weniger Bit erlaubt
               lbits -= iPlateauLengthBinBits + 1;
            max = (lbits + 1) * hunit;
            min = -max + 1;
         }
         /// <summary>
         /// liefert den kleinsten und den größten verwendbaren Wert bei Länge-Codierung 0
         /// </summary>
         /// <param name="maxheight">max. Kachelhöhe</param>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="max">größter verwendbarer Wert</param>
         /// <param name="iPlateauLengthBinBits">Anzahl der Binbits für die ev. vorausgehende Plateaulänge</param>
         public static void GetValueRangeLength0(int maxheight, out int min, out int max, int iPlateauLengthBinBits) {
            int lbits = GetMaxLengthZeroBits(maxheight);
            if (iPlateauLengthBinBits >= 0) // bei Plateaufollower weniger Bit erlaubt
               lbits -= iPlateauLengthBinBits + 1;
            min = -(lbits / 2);
            max = min + lbits;
         }
         /// <summary>
         /// liefert den kleinsten und den größten verwendbaren Wert bei Länge-Codierung 1
         /// </summary>
         /// <param name="maxheight">max. Kachelhöhe</param>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="max">größter verwendbarer Wert</param>
         /// <param name="iPlateauLengthBinBits">Anzahl der Binbits für die ev. vorausgehende Plateaulänge</param>
         public static void GetValueRangeLength1(int maxheight, out int min, out int max, int iPlateauLengthBinBits) {
            int lbits = GetMaxLengthZeroBits(maxheight);
            if (iPlateauLengthBinBits >= 0) // bei Plateaufollower weniger Bit erlaubt
               lbits -= iPlateauLengthBinBits + 1;
            max = lbits / 2 + 1;
            min = max - lbits;
         }
         /// <summary>
         /// liefert den kleinsten und den größten verwendbaren Wert bei Länge-Codierung 2
         /// </summary>
         /// <param name="maxheight">max. Kachelhöhe</param>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="max">größter verwendbarer Wert</param>
         /// <param name="iPlateauLengthBinBits">Anzahl der Binbits für die ev. vorausgehende Plateaulänge</param>
         public static void GetValueRangeLength2(int maxheight, out int min, out int max, int iPlateauLengthBinBits) {
            int lbits = GetMaxLengthZeroBits(maxheight);
            if (iPlateauLengthBinBits >= 0) // bei Plateaufollower weniger Bit erlaubt
               lbits -= iPlateauLengthBinBits + 1;
            max = lbits / 2;
            min = max - lbits;
         }
         /// <summary>
         /// liefert den kleinsten und den größten verwendbaren Wert bei BigBin-Codierung (an Stelle von Hybridcodierung bzw. Länge-Codierung 0)
         /// </summary>
         /// <param name="maxheight">max. Kachelhöhe</param>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="plateaufollower">wenn true, dann für Plateau-Nachfolger</param>
         public static void GetValueRangeBigBin(int maxheight, out int min, out int max) {
            int bbits = GetBigBinBits(maxheight) - 1;
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
         /// <param name="maxheight">max. Kachelhöhe</param>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="max">größter verwendbarer Wert</param>
         public static void GetValueRangeBigBinL1(int maxheight, out int min, out int max) {
            GetValueRangeBigBin(maxheight, out min, out max);
            max++;
            min++;
         }

         public override string ToString() {
            StringBuilder sb = new StringBuilder(ElementTyp.ToString());
            if (Column >= 0)
               sb.Append(", Column=" + Column.ToString());
            if (Line >= 0)
               sb.Append(", Line=" + Line.ToString());
            sb.Append(", Data=" + Data.ToString());
            if (WrappedValue)
               sb.Append("  (Wrap)");
            if (this.ElementTyp == Typ.Plateau) {
               sb.Append(", PlateauTableIdx=" + PlateauTableIdx.ToString());
               sb.Append(", PlateauBinBits=" + PlateauBinBits.ToString());
            }
            sb.Append(", Encoding=" + Encoding.ToString());
            if (Encoding == EncodeMode.Hybrid)
               sb.Append(" (" + HUnit.ToString() + ")");
            sb.Append(", Bits=");
            for (int i = 0; i < Bits.Count; i++)
               sb.Append(Bits[i] > 0 ? "1" : ".");

#if EXPLORERFUNCTION

            if (this.ElementTyp == Typ.Plateau)
               sb.Append(" " + GetPlateauUnitsText());
            else if (this.ElementTyp == Typ.PlateauFollower)
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
         int L0_wrapdown;
         /// <summary>
         /// unterer Grenzwert für <see cref="HeightElement.EncodeMode.Length0"/> der für das Wrapping unterschritten werden muss
         /// </summary>
         int L0_wrapup;

         /// <summary>
         /// oberer Grenzwert für <see cref="HeightElement.EncodeMode.Length1"/> der für das Wrapping überschritten werden muss
         /// </summary>
         int L1_wrapdown;
         /// <summary>
         /// unterer Grenzwert für <see cref="HeightElement.EncodeMode.Length1"/> der für das Wrapping unterschritten werden muss
         /// </summary>
         int L1_wrapup;

         /// <summary>
         /// oberer Grenzwert für <see cref="HeightElement.EncodeMode.Length2"/> der für das Wrapping überschritten werden muss
         /// </summary>
         int L2_wrapdown;
         /// <summary>
         /// unterer Grenzwert für <see cref="HeightElement.EncodeMode.Length2"/> der für das Wrapping unterschritten werden muss
         /// </summary>
         int L2_wrapup;

         /// <summary>
         /// oberer Grenzwert für <see cref="HeightElement.EncodeMode.Hybrid"/> der für ein eventuelles Wrapping mindestens überschritten werden muss
         /// </summary>
         int H_wrapdown;
         /// <summary>
         /// unterer Grenzwert für <see cref="HeightElement.EncodeMode.Hybrid"/> der für ein eventuelles Wrapping mindestens unterschritten werden muss
         /// </summary>
         int H_wrapup;

#if INCLUDENOTNEEDED

         /// <summary>
         /// oberer Grenzwerte für <see cref="HeightElement.EncodeMode.Hybrid"/> je Hunit, die für ein sicheres Wrapping überschritten werden müssen
         /// </summary>
         SortedDictionary<int, int> H_wrapdown_safely = new SortedDictionary<int, int>();
         /// <summary>
         /// unterer Grenzwerte für <see cref="HeightElement.EncodeMode.Hybrid"/> je Hunit, die für ein sicheres Wrapping unterschritten werden müssen
         /// </summary>
         SortedDictionary<int, int> H_wrapup_safely = new SortedDictionary<int, int>();

#endif

         /// <summary>
         /// Maximalhöhe der Kachel
         /// </summary>
         int max;


         public Wraparound(int maxheight) {
            max = maxheight;

            // L0: z.B.  9 -> -4 .. +5 (max. 9 Bit)
            //          10 -> -5 .. +5 (max. 10 Bit)
            if (maxheight % 2 == 0) {
               L0_wrapdown = maxheight / 2;
               L0_wrapup = -maxheight / 2;
            } else {
               L0_wrapdown = (maxheight + 1) / 2;
               L0_wrapup = -(maxheight - 1) / 2;
            }

            // L1: z.B.  9 -> -4 .. +5 (max. 9 Bit)
            //          10 -> -5 .. +6 (max. 10 Bit)
            if (maxheight % 2 == 0) {
               L1_wrapdown = (maxheight + 2) / 2;
               L1_wrapup = -maxheight / 2;
            } else {
               L1_wrapdown = (maxheight + 1) / 2;
               L1_wrapup = -(maxheight - 1) / 2;
            }

            // L2: z.B.  9 -> -5 .. +4 (max. 9 Bit)
            //          10 -> -5 .. +5 (max. 10 Bit)
            if (maxheight % 2 == 0) {
               L2_wrapdown = maxheight / 2;
               L2_wrapup = -maxheight / 2;
            } else {
               L2_wrapdown = (maxheight - 1) / 2;
               L2_wrapup = -(maxheight + 1) / 2;
            }
            //L2_wrapdown /= 4;
            //L2_wrapup /= 4;

            H_wrapdown = (maxheight + 1) / 2;
            H_wrapup = -(maxheight - 1) / 2;

#if INCLUDENOTNEEDED

            H_wrapdown_safely = new SortedDictionary<int, int>();
            H_wrapdown_safely.Add(1, (maxheight + 2) / 2); // für hunit=1
            H_wrapdown_safely.Add(2, maxheight / 2 + 2);
            H_wrapdown_safely.Add(4, maxheight / 2 + 4);
            H_wrapdown_safely.Add(8, maxheight / 2 + 8);
            H_wrapdown_safely.Add(16, maxheight / 2 + 16);
            H_wrapdown_safely.Add(32, maxheight / 2 + 32);
            H_wrapdown_safely.Add(64, maxheight / 2 + 64);
            H_wrapdown_safely.Add(128, maxheight / 2 + 128);
            H_wrapdown_safely.Add(256, maxheight / 2 + 256);

            H_wrapup_safely = new SortedDictionary<int, int>();
            H_wrapup_safely.Add(1, -maxheight / 2); // für hunit=1
            H_wrapup_safely.Add(2, -maxheight / 2 - 2);
            H_wrapup_safely.Add(4, -maxheight / 2 - 4);
            H_wrapup_safely.Add(8, -maxheight / 2 - 8);
            H_wrapup_safely.Add(16, -maxheight / 2 - 16);
            H_wrapup_safely.Add(32, -maxheight / 2 - 32);
            H_wrapup_safely.Add(64, -maxheight / 2 - 64);
            H_wrapup_safely.Add(128, -maxheight / 2 - 128);
            H_wrapup_safely.Add(256, -maxheight / 2 - 256);

#endif
         }

         /// <summary>
         /// ein Wert wird bei Bedarf gewrapt, notfalls auch die Codierart auf BiBin gesetzt
         /// </summary>
         /// <param name="data">Wert</param>
         /// <param name="wrapped">gesetzt, wenn ein gewrapter Wert geliefert wird</param>
         /// <param name="em">Codierart des Wertes; danach ev. auf BigBin gesetzt</param>
         /// <param name="hunit">nur für die Codierart <see cref="HeightElement.EncodeMode.Hybrid"/> nötig</param>
         /// <param name="iPlateauLengthBinBits">Anzahl der Binbits für die ev. vorausgehende Plateaulänge</param>
         /// <returns>zu verwendender Wert</returns>
         public int Wrap(int data, out bool wrapped, ref EncodeMode em, int hunit, int iPlateauLengthBinBits) {
            int minval, maxval;
            wrapped = false;
            int datawrapped = int.MinValue;

            switch (em) {
               case EncodeMode.Length0:
                  if (data > L0_wrapdown)
                     datawrapped = WrapDown(data);
                  else if (data < L0_wrapup)
                     datawrapped = WrapUp(data);

                  HeightElement.GetValueRangeLength0(max, out minval, out maxval, iPlateauLengthBinBits);
                  if (datawrapped != int.MinValue) {
                     if (datawrapped < minval || maxval < datawrapped) {
                        datawrapped = int.MinValue; // wird doch nicht benötigt
                        em = EncodeMode.BigBinary;
                     }
                  } else if (data < minval || maxval < data)
                     em = EncodeMode.BigBinary;
                  break;

               case EncodeMode.Length1:
                  if (data > L1_wrapdown)
                     datawrapped = WrapDown(data);
                  else if (data < L1_wrapup)
                     datawrapped = WrapUp(data);

                  HeightElement.GetValueRangeLength1(max, out minval, out maxval, iPlateauLengthBinBits);
                  if (datawrapped != int.MinValue) {
                     if (datawrapped < minval || maxval < datawrapped) {
                        datawrapped = int.MinValue; // wird doch nicht benötigt
                        em = EncodeMode.BigBinaryL1;
                     }
                  } else if (data < minval || maxval < data)
                     em = EncodeMode.BigBinaryL1;
                  break;

               case EncodeMode.Length2:
                  if (data > L2_wrapdown)
                     datawrapped = WrapDown(data);
                  else if (data < L2_wrapup)
                     datawrapped = WrapUp(data);

                  HeightElement.GetValueRangeLength2(max, out minval, out maxval, iPlateauLengthBinBits);
                  if (datawrapped != int.MinValue) {
                     if (datawrapped < minval || maxval < datawrapped) {
                        datawrapped = int.MinValue; // wird doch nicht benötigt
                        em = EncodeMode.BigBinaryL2;
                     }
                  } else if (data < minval || maxval < data)
                     em = EncodeMode.BigBinaryL2;
                  break;

               case EncodeMode.Hybrid:
                  if (data > H_wrapdown)
                     datawrapped = WrapDown(data);
                  else if (data < H_wrapup)
                     datawrapped = WrapUp(data);

                  HeightElement.GetValueRangeHybrid(hunit, max, out minval, out maxval, iPlateauLengthBinBits);
                  if (datawrapped != int.MinValue) {
                     if (datawrapped < minval || maxval < datawrapped) {
                        datawrapped = int.MinValue; // wird doch nicht benötigt
                        em = EncodeMode.BigBinary;
                     }
                  } else if (data < minval || maxval < data)
                     em = EncodeMode.BigBinary;
                  break;
            }

            // schon gewrapte Werte sind auf keinen Fall BigBin
            if (datawrapped == int.MinValue) {
               switch (em) {
                  case EncodeMode.BigBinary:
                     // minval und maxval beziehen sich hier auf den theoretischen (!) Wertebereich auf Grund der Bitanzahl
                     //HeightElement.GetValueRangeBigBin(max, out minval, out maxval);
                     maxval = max / 2;
                     minval = -maxval;
                     if (data > maxval)
                        datawrapped = WrapDown(data);
                     else if (data < minval)
                        datawrapped = WrapUp(data);
                     break;

                  case EncodeMode.BigBinaryL1:
                     HeightElement.GetValueRangeBigBinL1(max, out minval, out maxval);
                     if (data > maxval)
                        datawrapped = WrapDown(data);
                     else if (data < minval)
                        datawrapped = WrapUp(data);
                     break;
               }
            }

            wrapped = datawrapped != int.MinValue;

            return wrapped ? datawrapped : data;
         }

         /// <summary>
         /// ein Wrap erfolgt, wenn der Betrag des neuen Wertes kleiner als der Originalwert ist
         /// </summary>
         /// <param name="data"></param>
         /// <param name="wrapped">gesetzt, wenn ein gewrapter Wert geliefert wird</param>
         /// <returns></returns>
         public int SimpleWrap(int data, out bool wrapped) {
            wrapped = false;
            if (data > 0) {
               if (2 * data > max + 1) {
                  data = WrapDown(data);
                  wrapped = true;
               }
            } else if (data < 0) {
               if (2 * data < -(max + 1)) {
                  data = WrapUp(data);
                  wrapped = true;
               }
            }
            return data;
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
         /// <param name="spec64">spez. für 64. Wert</param>
         /// <returns></returns>
         protected int EvaluateData(int oldsum, int elemcount, int newdata, bool spec64) {
            /*
               D < -2 – (ls + 3*k)/2   -1 – ls – k	
               D < 0 – (ls + k)/2      2*(d + k) + 3	
               D < 2 – (ls – k)/2      2*d – 1	
               D < 4 – (ls – 3*k)/2    2*(d – k) - 5	
                                       1 – ls + k	
             */

            int v = 0;

            // Spezialfall
            if (elemcount == 63 && oldsum == -63 && newdata == -1)
               return -3;

            if (newdata < -2 - ((oldsum + 3 * elemcount) >> 1)) {
               v = -1 - oldsum - elemcount;
            } else if (newdata < -((oldsum + elemcount) >> 1)) {
               v = 2 * (newdata + elemcount) + 3;
            } else if (newdata < 2 - ((oldsum - elemcount) >> 1)) {
               v = 2 * newdata - 1;
            } else if (newdata < 4 - ((oldsum - 3 * elemcount) >> 1)) {
               v = 2 * (newdata - elemcount) - 5;
            } else {
               v = 1 - oldsum + elemcount;
            }

            return v;
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

         /// <summary>
         /// bildet <see cref="CodingType"/> für die Standard-Codierung
         /// </summary>
         /// <param name="maxheightdiff">max. Höhendiff.</param>
         public CodingTypeStd(int maxheightdiff) : base(maxheightdiff) { }

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

            if (ElemCount == 63) {  // besondere Bewertung bei "Halbierung"
               if (SumL > 0) { // pos. SumL

                  if ((SumL + 1) % 4 == 0) {
                     if (data % 2 != 0)
                        data--;
                  } else {
                     if (data % 2 == 0)
                        data--;
                  }

               } else { // neg. SumL

                  if ((SumL - 1) % 4 == 0) {
                     if (data % 2 != 0)
                        data++;
                  } else {
                     if (data % 2 == 0)
                        data++;
                  }

               }
            }

            Eval = EvaluateData(SumL, ElemCount, data, true);

            SumH += dh;
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
            SetHunit4SumAndElemcount(SumH, ElemCount, true);

            // ---- EncodeMode neu setzen ----
            if (HunitValue > 0)
               EncodeMode = EncodeMode.Hybrid;
            else
               EncodeMode = SumL > 0 ? EncodeMode.Length1 : EncodeMode.Length0;

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

      public enum CalculationType {
         /// <summary>
         /// nur für Init.
         /// </summary>
         nothing,

         /// <summary>
         /// Standardberechnung
         /// </summary>
         Standard,

         /// <summary>
         /// Standardberechnung bei großer pos. horizontale Diff.
         /// </summary>
         StandardHdiffHigh,
         /// <summary>
         /// Standardberechnung bei kleiner neg. horizontale Diff.
         /// </summary>
         StandardHdiffLow,

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
      CodingTypePlateauFollowerZero ct_ddiff4plateaufollower_zero;

      /// <summary>
      /// zur Bestimmung der Heightunit für die Gruppe der Plateau-Nachfolger mit ddiff!=0
      /// </summary>
      CodingTypePlateauFollowerNotZero ct_ddiff4plateaufollower_notzero;

      Wraparound ValueWrap;


      /// <summary>
      /// Liste der registrierten Höhenelemente
      /// </summary>
      public List<HeightElement> Elements { get; private set; }

#if EXPLORERFUNCTION

      public List<string> CodingTypeStd_Info { get; private set; }

      public List<string> CodingTypePlateauFollowerNotZero_Info { get; private set; }

      public List<string> CodingTypePlateauFollowerZero_Info { get; private set; }

#endif

#if EXPLORERFUNCTION

      /// <summary>
      /// akt. Codierungs-Art (Codierung, die vom letzten <see cref="HeightElement"/> geliefert wird)
      /// </summary>
      public EncodeMode ActualMode {
         get {
            return Elements.Count > 0 ?
                        Elements[Elements.Count - 1].Encoding :
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
            _initialHeightUnit = new CodingTypeStd(value);
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
      /// <param name="maxheight">max. Höhe</param>
      /// <param name="codingtyp">Codiertyp (z.Z. nicht verwendet)</param>
      /// <param name="tilesizehorz">Breite der Kachel</param>
      /// <param name="tilesizevert">Höhe der Kachel</param>
      /// <param name="height">Liste der Höhendaten (Anzahl normalerweise <see cref="tilesize"/> * <see cref="tilesize"/>)</param>
      public TileEncoder(int maxheight, byte codingtyp, int tilesizehorz, int tilesizevert, IList<int> height) {
         MaxHeight = maxheight;
         TileSizeHorz = tilesizehorz;
         TileSizeVert = tilesizevert;

#if EXPLORERFUNCTION

         Codingtype = codingtyp;
         StdHeight = 0;
         InitialHeightUnit = MaxHeight; // die korrekte hunit wird intern bestimmt

#endif

         HeightValues = new List<int>(height);
         nextPosition = new Position(tilesizehorz, tilesizevert);

         ct_std = new CodingTypeStd(MaxHeight);
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

         if (ValidHeight(pos) >= 0) { // nur nichtnegative Höhen

            try {

               if (ValidHeightDDiff(pos) == 0) { // die Diagonale hat konstante Höhe (gilt auch für die 1. Spalte) -> immer Plateau (ev. auch mit Länge 0)

                  bEnd = WritePlateau(pos, ct_ddiff4plateaufollower_zero, ct_ddiff4plateaufollower_notzero);

               } else {

                  int data = 0;
                  CalculationType caltype = CalculationType.nothing;

                  int hdiff_up = ValidHeightHDiff(pos.X, pos.Y - 1);       // horiz. Diff. der Höhe in der Zeile "über" der akt. Pos.
                  int hv = ValidHeight(pos, -1, 0);
                  int sgnddiff = Math.Sign(ValidHeightDDiff(pos));

                  if (hdiff_up >= MaxHeight - hv) {

                     data = -sgnddiff * (ValidHeight(pos) + 1);
                     caltype = CalculationType.StandardHdiffHigh;

                  } else if (hdiff_up <= -hv) {

                     data = -sgnddiff * ValidHeight(pos);
                     caltype = CalculationType.StandardHdiffLow;

                  } else {

                     data = -sgnddiff * (ValidHeightHDiff(pos) - hdiff_up);
                     caltype = CalculationType.Standard;

                  }

                  AddHeightValue(data, caltype, pos, ct_std);

                  bEnd = !pos.MoveForward();
                  //}
               }

            } catch (Exception ex) {
               throw new Exception(string.Format("interner Fehler bei Position {0}, Höhe {1}: {2}", pos, ValidHeight(pos), ex.Message));
            }
         } else
            throw new Exception(string.Format("negative Daten (Pos {0}) können nicht verarbeitet werden.", pos));
         return bEnd;
      }

      /// <summary>
      /// erzeugt ein <see cref="HeightElement"/> für die Plateaulänge und eins für den Plateaunachfolger
      /// </summary>
      /// <param name="pos">akt. Position</param>
      /// <param name="ct_followerddiffzero">hunit-Berechnung wenn die ddiff für den Nachfolger 0 ist</param>
      /// <param name="ct_followerddiffnotzero">hunit-Berechnung wenn die ddiff für den Nachfolger ungleich 0 ist</param>
      /// <returns>true, wenn die Endpos. rechts-unten erreicht wurde</returns>
      bool WritePlateau(Position pos, CodingTypePlateauFollowerZero ct_followerddiffzero, CodingTypePlateauFollowerNotZero ct_followerddiffnotzero) {
         int length;
         bool bEnd = GetPlateauLength(pos, out length);

         // pos steht am Anfang des Plateaus bzw., bei Länge 0, schon auf dem Follower
         // also zeigt pos.X+length auf die Pos. des Followers

         // Plateaulänge codieren
         HeightElement he = HeightElement.CreateHeightElement_Plateau(length, pos.X, pos.Y, TileSizeHorz, Elements);
         Elements.Add(he);
         bool bLineFilled = pos.X + length >= TileSizeHorz;
         bEnd = !pos.MoveForward(length);

         if (!bLineFilled) { // Nachfolgewert bestimmen
            if (!bEnd) {
               int follower = ValidHeight(pos);
               int follower_ddiff = pos.X == 0 ?
                                          0 : // wegen virt. Spalte
                                          ValidHeightDDiff(pos);
               int follower_vdiff = ValidHeightVDiff(pos);

               int data = follower_vdiff;

               CodingType ct;
               if (follower_ddiff != 0)
                  ct = ct_followerddiffnotzero;
               else
                  ct = ct_followerddiffzero;

               EncodeMode em = ct.EncodeMode;
               bool wrapped;
               // Nachfolger codieren
               CalculationType caltyp2 = CalculationType.nothing;
               if (follower_ddiff != 0) {

                  caltyp2 = CalculationType.PlateauFollower1;
                  if (follower_ddiff > 0)
                     data = -data;

                  data = ValueWrap.SimpleWrap(data, out wrapped);
                  data = ValueWrap.Wrap(data, out wrapped, ref em, ct.HunitValue, he.PlateauBinBits);

               } else {

                  caltyp2 = CalculationType.PlateauFollower0;

                  data = ValueWrap.SimpleWrap(data, out wrapped);
                  data = ValueWrap.Wrap(data, out wrapped, ref em, ct.HunitValue, he.PlateauBinBits);
                  if (data < 0)
                     data++;

               }

               AddHeightValue(data, caltyp2, pos, ct, em, follower_ddiff, wrapped);

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
      /// fügt eine neues <see cref="HeightElement"/> an die Liste <see cref="Elements"/> an
      /// </summary>
      /// <param name="data">Datenwert</param>
      /// <param name="caltype">Berechnungstyp</param>
      /// <param name="pos">Pos. in der Kachel</param>
      /// <param name="ct">Codierart</param>
      /// <param name="emfollower">ausdrücklicher Codiermodus bei einem Plateau-Nachfolger</param>
      /// <param name="extdata">ddiff für den Plateau-Nachfolger oder Länge bei Moved Range</param>
      /// <param name="wrapped">true, falls der Plateau-Nachfolger gewrapt ist</param>
      void AddHeightValue(int data, CalculationType caltype, Position pos, CodingType ct, EncodeMode emfollower = EncodeMode.notdefined, int extdata = int.MinValue, bool wrapped = false) {
         HeightElement elem = null;

         switch (caltype) {
            case CalculationType.PlateauFollower0:
            case CalculationType.PlateauFollower1:
               if (emfollower != EncodeMode.notdefined) {
                  switch (emfollower) {
                     case EncodeMode.Hybrid:
                        elem = HeightElement.CreateHeightElement_PlateauFollowerH(data, caltype, wrapped, MaxHeight, ct.HunitValue, extdata, pos.X, pos.Y);
                        break;

                     case EncodeMode.Length0:
                     case EncodeMode.Length1:
                     case EncodeMode.Length2:
                        elem = HeightElement.CreateHeightElement_PlateauFollowerL(data, caltype, wrapped, emfollower, extdata, pos.X, pos.Y);
                        break;

                     case EncodeMode.BigBinary:
                        elem = HeightElement.CreateHeightElement_PlateauFollowerBigValue(data, caltype, wrapped, MaxHeight, extdata, pos.X, pos.Y);
                        break;

                     case EncodeMode.BigBinaryL1:
                        elem = HeightElement.CreateHeightElement_PlateauFollowerBigValueL1(data, caltype, wrapped, MaxHeight, extdata, pos.X, pos.Y);
                        break;

                     case EncodeMode.BigBinaryL2:
                        elem = HeightElement.CreateHeightElement_PlateauFollowerBigValueL2(data, caltype, wrapped, MaxHeight, extdata, pos.X, pos.Y);
                        break;
                  }
               } else
                  throw new Exception("Bei einem Plateau-Nachfolger muss explizit der Codiermodus angegeben sein.");
               break;

            default: {
                  EncodeMode em = ct.EncodeMode; // wird ev. verändert auf BigBin
                  data = ValueWrap.SimpleWrap(data, out wrapped);
                  data = ValueWrap.Wrap(data, out wrapped, ref em, ct.HunitValue, -1);

                  //Debug.Write(string.Format("[{0},{1}], ", pos.X, pos.Y));

                  switch (em) {
                     case EncodeMode.Hybrid:
                        elem = HeightElement.CreateHeightElement_ValueH(data, caltype, wrapped, MaxHeight, ct.HunitValue, pos.X, pos.Y);
                        break;

                     case EncodeMode.Length0:
                     case EncodeMode.Length1:
                        elem = HeightElement.CreateHeightElement_ValueL(data, caltype, wrapped, em, pos.X, pos.Y);
                        break;

                     case EncodeMode.BigBinary:
                        elem = HeightElement.CreateHeightElement_BigValue(data, caltype, wrapped, MaxHeight, pos.X, pos.Y);
                        break;

                     case EncodeMode.BigBinaryL1:
                        elem = HeightElement.CreateHeightElement_BigValueL1(data, caltype, wrapped, MaxHeight, pos.X, pos.Y);
                        break;
                  }
                  break;
               }
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
      /// liefert die Höhe zum Index (alle Höhen sind zeilenweise hintereinander angeordnet)
      /// </summary>
      /// <param name="idx"></param>
      /// <returns>negativ, wenn ungültig</returns>
      int Height(int idx) {
         return 0 <= idx && idx < HeightValues.Count ?
                        HeightValues[idx] :
                        -1;
      }

      /// <summary>
      /// liefert die Höhe aus Spalte und Zeile (Spalte und Zeile müssen gültige Werte sein, da intern der Index verwendet wird)
      /// </summary>
      /// <param name="column"></param>
      /// <param name="line"></param>
      /// <returns>negativ, wenn ungültig</returns>
      public int Height(int column, int line) {
         return Height(TileSizeHorz * line + column);
      }

      /// <summary>
      /// liefert die Höhe bezüglich der Position (mit dem Delta muss sich eine gültige Position ergeben, da intern der Index verwendet wird)
      /// </summary>
      /// <param name="pos"></param>
      /// <param name="deltax">Spalten-Delta zur Position</param>
      /// <param name="deltay">Zeilen-Delta zur Position</param>
      /// <returns>negativ, wenn ungültig</returns>
      int ValidHeight(Position pos, int deltax = 0, int deltay = 0) {
         if (pos.X + deltax < 0 || pos.X + deltax >= pos.Width ||
             pos.Y + deltay < 0 || pos.Y + deltay >= pos.Height)
            return -1;
         return Height(new Position(pos, deltax, deltay).Idx);
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
            if (h >= 0)
               return h;
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

      static public List<byte> LengthCoding0(int data) {
         return new List<byte>(HeightElement.CreateHeightElement_ValueL(data, CalculationType.nothing, false, EncodeMode.Length0, int.MinValue, int.MinValue).Bits);
      }

      static public List<byte> LengthCoding1(int data) {
         return new List<byte>(HeightElement.CreateHeightElement_ValueL(data, CalculationType.nothing, false, EncodeMode.Length1, int.MinValue, int.MinValue).Bits);
      }

      static public List<byte> LengthCoding2(int data) {
         return new List<byte>(HeightElement.CreateHeightElement_ValueL(data, CalculationType.nothing, false, EncodeMode.Length2, int.MinValue, int.MinValue).Bits);
      }

      static public List<byte> HybridCoding(int data, int maxHeight, int hunit) {
         return new List<byte>(HeightElement.CreateHeightElement_ValueH(data, CalculationType.nothing, false, maxHeight, hunit, int.MinValue, int.MinValue).Bits);
      }

      static public List<byte> BigValueCodingHybrid(int data, int maxHeight) {
         return new List<byte>(HeightElement.CreateHeightElement_BigValue(data, CalculationType.nothing, false, maxHeight, int.MinValue, int.MinValue).Bits);
      }

      static public List<byte> BigValueCodingLength0(int data, int maxHeight) {
         return new List<byte>(HeightElement.CreateHeightElement_BigValue(data, CalculationType.nothing, false, maxHeight, int.MinValue, int.MinValue).Bits);
      }

      static public List<byte> BigValueCodingLength1(int data, int maxHeight) {
         return new List<byte>(HeightElement.CreateHeightElement_BigValueL1(data, CalculationType.nothing, false, maxHeight, int.MinValue, int.MinValue).Bits);
      }

      #endregion

#endif

#if EXPLORERFUNCTION

      public override string ToString() {
         return string.Format("MaxHeight={0}, Codingtype={1}, TileSize={2}x{3}, BaseHeightUnit={4}, HeightUnit={5}, ActualMode={6}, ActualHeight={7}",
                              MaxHeight,
                              Codingtype,
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
