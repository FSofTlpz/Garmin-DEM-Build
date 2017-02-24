using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

//CS0659: "Klasse" überschreibt Object.Equals(object o), aber nicht Object.GetHashCode() ('class' overrides Object.Equals(object o) but does not override Object.GetHashCode())
//CS0660: "Klasse" definiert den Operator == oder !=, aber überschreibt Object.Equals(object o) nicht. ('class' defines operator == or operator != but does not override Object.Equals(object o))
//CS0661: "Klasse" definiert den Operator == oder !=, aber überschreibt Object.GetHashCode() nicht. ('class' defines operator == or operator != but does not override Object.GetHashCode())
#pragma warning disable 659, 661


namespace Encoder {

   /// <summary>
   /// Encoder für eine einzelne DEM-Kachel
   /// </summary>
   public class TileEncoder {

      #region Hilfsklassen

      class Position {
         public int Width { get; private set; }
         public int Height { get; private set; }
         public int X { get; private set; }
         public int Y { get; private set; }
         /// <summary>
         /// liefert oder setzt den fortlaufenden Index der Position
         /// </summary>
         public int Idx {
            get {
               return Y * Width + X;
            }
            set {
               Set(value % Width, value / Height);
            }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="width">Breite</param>
         /// <param name="height">Höhe</param>
         /// <param name="x"></param>
         /// <param name="y"></param>
         public Position(int width, int height, int x = 0, int y = 0) {
            Width = width;
            Height = height;
            Set(x, y);
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="width">Breite</param>
         /// <param name="height">Höhe</param>
         /// <param name="idx">fortlaufender Index</param>
         public Position(int width, int height, int idx) :
            this(width, height) {
            Idx = idx;
         }

         public Position(Position pos, int deltax = 0, int deltay = 0) {
            X = pos.X + deltax;
            Y = pos.Y + deltay;
            Width = pos.Width;
            Height = pos.Height;
         }

         public bool Equals(Position pos) {
            if ((object)pos == null)
               return false;
            if (System.Object.ReferenceEquals(this, pos))
               return true;
            return (X == pos.X) && (Y == pos.Y);
         }


         public override bool Equals(object obj) {
            return this.Equals(obj as Position);
         }

         //public bool Equals(TwoDPoint p) {
         //   // If parameter is null, return false.
         //   if (Object.ReferenceEquals(p, null)) {
         //      return false;
         //   }

         //   // Optimization for a common success case.
         //   if (Object.ReferenceEquals(this, p)) {
         //      return true;
         //   }

         //   // If run-time types are not exactly the same, return false.
         //   if (this.GetType() != p.GetType())
         //      return false;

         //   // Return true if the fields match.
         //   // Note that the base class is not invoked because it is
         //   // System.Object, which defines Equals as reference equality.
         //   return (X == p.X) && (Y == p.Y);
         //}

         //public override int GetHashCode() {
         //   return X * 0x00010000 + Y;
         //}


         public static bool operator ==(Position a, Position b) {
            return a.Equals(b);
         }

         public static bool operator !=(Position a, Position b) {
            return !a.Equals(b);
         }

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
         /// setzt die Position (begrenzt durch den "Rand")
         /// </summary>
         /// <param name="x"></param>
         /// <param name="y"></param>
         public void Set(int x, int y) {
            X = Math.Max(0, Math.Min(x, Width - 1));
            Y = Math.Max(0, Math.Min(y, Height - 1));
         }

         /// <summary>
         /// Änderung der Position (begrenzt durch den "Rand")
         /// </summary>
         /// <param name="deltax"></param>
         /// <param name="deltay"></param>
         public void Move(int deltax, int deltay) {
            X += deltax;
            if (X < 0)
               X = 0;
            else if (X >= Width)
               X = Width - 1;
            if (Y < 0)
               Y = 0;
            else if (Y >= Height)
               Y = Height - 1;
         }

         public bool Next(int count = 1) {
            if (count == 1) {
               if (++X >= Width)
                  if (Y < Height - 1) {
                     Y++;
                     X = 0;
                  } else {
                     X = Width - 1;
                     return false;
                  }
            } else {
               Idx = Idx + count;
               if (Y > Height - 1 ||
                   X > Width - 1) {
                  Y = Height - 1;
                  X = Width - 1;
                  return false;
               }
            }
            return true;
         }

         public bool Back() {
            if (--X < 0)
               if (Y > 0) {
                  Y--;
                  X = Width - 1;
               } else {
                  X = 0;
                  return false;
               }
            return true;
         }

         public void Vertical(int count) {
            if (count != 0)
               Y += count;
            if (--Y < 0)
               Y = 0;
            else if (Y >= Height)
               Y = Height - 1;
         }

         public override string ToString() {
            return string.Format("X={0}, Y={1}, Width={2}, Height={3}", X, Y, Width, Height);
         }
      }

      /// <summary>
      /// allgemeine <see cref="HeightUnit"/>
      /// </summary>
      class HeightUnit {

         /// <summary>
         /// liefert den Wert (immer einer 2er-Potenz)
         /// </summary>
         public int Value { get; private set; }
         /// <summary>
         /// liefert den Exponent der 2er-Potenz zu <see cref="Value"/>
         /// </summary>
         public int Exponent { get; private set; }


         public HeightUnit(int hunitvalue, bool formaxheight) {
            if (hunitvalue < 0)
               throw new Exception("Der Wert von HeightUnit kann nicht kleiner 0 sein.");
            if (formaxheight)
               if (hunitvalue < 0x9f)
                  Value = 1;
               else if (hunitvalue < 0x11f)
                  Value = 2;
               else if (hunitvalue < 0x21f)
                  Value = 4;
               else if (hunitvalue < 0x41f)
                  Value = 8;
               else if (hunitvalue < 0x81f)
                  Value = 16;
               else if (hunitvalue < 0x101f)
                  Value = 32;
               else if (hunitvalue < 0x201f)
                  Value = 64;
               else if (hunitvalue < 0x401f)
                  Value = 128;
               else
                  Value = 256;
            else
               SetValue(hunitvalue);
         }

         public HeightUnit(int exponent) {
            if (exponent == 0)
               SetValue(0);
            else {
               if (exponent < 0)
                  throw new Exception("Der Exponent von HeightUnit kann nicht kleiner 0 sein.");
               int tmp = 1;
               while (--exponent > 0)
                  tmp *= 2;
               SetValue(tmp);
            }
         }

         public HeightUnit(HeightUnit hu) {
            Value = hu.Value;
            Exponent = hu.Value;
         }

         /// <summary>
         /// liefert den <see cref="HeightUnit"/>-Wert für die max. Höhendiff.
         /// </summary>
         /// <param name="maxheigth"></param>
         /// <returns></returns>
         static public int GetHeightUnit4MaxHeight(int maxheigth) {
            if (maxheigth < 0x9f)
               return 1;
            else if (maxheigth < 0x11f)
               return 2;
            else if (maxheigth < 0x21f)
               return 4;
            else if (maxheigth < 0x41f)
               return 8;
            else if (maxheigth < 0x81f)
               return 16;
            else if (maxheigth < 0x101f)
               return 32;
            else if (maxheigth < 0x201f)
               return 64;
            else if (maxheigth < 0x401f)
               return 128;
            return 256;
         }

         /// <summary>
         /// liefert die Verschiebung für die Abssum-Tabelle für die max. Höhendiff. (0, ...)
         /// </summary>
         /// <param name="maxheigth"></param>
         /// <returns></returns>
         static public int GetHeightUnitTableDelta4MaxHeight(int maxheigth) {
            return (Math.Max(0x5f, maxheigth) - 0x5f) / 0x40;
         }

         /// <summary>
         /// liefert null, wenn <see cref="HeightUnit"/> kleiner als 1 ist
         /// </summary>
         /// <param name="hunitvalue">bisherige Summe</param>
         /// <param name="hunittabcol">für die Zielposition!</param>
         /// <param name="maxheigthdiff"></param>
         /// <returns></returns>
         static public HeightUnit GetHunit4HuvAndTabCol(int huv, int hunittabcol, int maxheigthdiff) {
            int counter = huv + 1 + GetHeightUnitTableDelta4MaxHeight(maxheigthdiff);
            int denominator = hunittabcol + 1;
            return counter >= denominator ?
                                 new HeightUnit(counter / denominator, false) :
                                 null;
         }

         /// <summary>
         /// setzt die <see cref="HeightUnit"/> neu
         /// </summary>
         /// <param name="hunitvalue">bisherige Summe</param>
         /// <param name="hunittabcol">für die Zielposition!</param>
         /// <param name="maxheigthdiff"></param>
         public void SetHunit4HuvAndTabCol(int huv, int hunittabcol, int maxheigthdiff) {
            int counter = huv + 1 + GetHeightUnitTableDelta4MaxHeight(maxheigthdiff);
            int denominator = hunittabcol + 1;
            if (counter >= denominator)
               SetValue(counter / denominator);
            else
               SetValue(0);
         }

         /// <summary>
         /// setzt <see cref="HeightUnit"/> auf die größte 2er-Potenz, die nicht größer als <see cref="value"/> ist
         /// </summary>
         /// <param name="value"></param>
         protected void SetValue(int value) {
            value &= 0x1FF;
            // höchstes 1-Bit suchen
            Value = 0x100;
            Exponent = 8;
            while (Value > 0 &&
                   (Value & value) == 0) {
               Value >>= 1;
               Exponent--;
            }
         }

         public override string ToString() {
            return string.Format("{0}, Exponent {1}", Value, Exponent);
         }
      }

      /// <summary>
      /// Standard-<see cref="HeightUnit"/> (nicht für die 1. Spalte und Spezialwerte)
      /// </summary>
      class HeightUnitStd : HeightUnit {

         int maxheigthdiff = 0;
         int huv = 0;
         int hunittabcol = 0;


         public HeightUnitStd(int maxheigthdiff)
            : base(maxheigthdiff, true) {
            this.maxheigthdiff = maxheigthdiff;
         }

         public HeightUnitStd(HeightUnitStd hu)
            : base(hu) {
            maxheigthdiff = hu.maxheigthdiff;
            huv = hu.huv;
            hunittabcol = hu.hunittabcol;
         }

         public void AddValue(int data) {
            huv += Math.Abs(data);
            if (++hunittabcol == 64) {
               hunittabcol = 32;
               huv = (huv / 2) - 1;
            }

            SetHunit4HuvAndTabCol(huv, hunittabcol, maxheigthdiff);
         }

         public void SetHunit4HuvAndTabCol(int huv, int hunittabcol) {
            this.huv = huv;
            this.hunittabcol = hunittabcol;
            base.SetHunit4HuvAndTabCol(huv, hunittabcol, maxheigthdiff);
         }

         public override string ToString() {
            return base.ToString() + string.Format(", hunitvalue={0}, hunittabcol={1}", huv, hunittabcol);
         }

      }

      /// <summary>
      /// spezielle <see cref="HeightUnit"/>, z.T. wenn ddiff=0
      /// </summary>
      class HeightUnitPlateau4DdiffEqual : HeightUnit {

         int maxheigthdiff = 0;

         /// <summary>
         /// akt. für diese Summe der Absolutwerte
         /// </summary>
         public int AbsSum0 { get; private set; }
         /// <summary>
         /// akt. für diese Zeilennummer
         /// </summary>
         public int Line { get; private set; }

         public HeightUnitPlateau4DdiffEqual(int maxheigthdiff)
            : base(maxheigthdiff, true) {
            this.maxheigthdiff = maxheigthdiff;
            AbsSum0 = 0;
            Line = 0;
         }

         public HeightUnitPlateau4DdiffEqual(HeightUnitPlateau4DdiffEqual hu)
            : base(hu) {
            maxheigthdiff = hu.maxheigthdiff;
            AbsSum0 = hu.AbsSum0;
            Line = hu.Line;
         }

         /// <summary>
         /// der Datenwert des Nachfolgers wird ergänzt
         /// </summary>
         /// <param name="data"></param>
         public void AddPlateauFollower(int data) {
            AbsSum0 += Math.Abs(data);
            Line++;

            // Standard
            if (Line % 2 == 0)
               AbsSum0--;

            if (data < 0)
               AbsSum0++;

            SetHunit4HuvAndTabCol(AbsSum0, Line, maxheigthdiff);
         }

         public override string ToString() {
            return base.ToString() + string.Format(", AbsSum0={0}, Line={1}", AbsSum0, Line);
         }
      }

      /// <summary>
      /// spezielle <see cref="HeightUnit"/>, z.T. wenn ddiff != 0
      /// </summary>
      class HeightUnitPlateau4DdiffUnequal : HeightUnit {

         int maxheigthdiff = 0;

         /// <summary>
         /// akt. für diese Summe der Absolutwerte
         /// </summary>
         public int AbsSum0 { get; private set; }
         /// <summary>
         /// akt. für diese Zeilennummer
         /// </summary>
         public int Line { get; private set; }

         public HeightUnitPlateau4DdiffUnequal(int maxheigthdiff)
            : base(maxheigthdiff, true) {
            this.maxheigthdiff = maxheigthdiff;
            AbsSum0 = 0;
            Line = 0;
         }

         public HeightUnitPlateau4DdiffUnequal(HeightUnitPlateau4DdiffUnequal hu)
            : base(hu) {
            maxheigthdiff = hu.maxheigthdiff;
            AbsSum0 = hu.AbsSum0;
            Line = hu.Line;
         }

         /// <summary>
         /// der Datenwert des Nachfolgers wird ergänzt
         /// </summary>
         /// <param name="data"></param>
         /// <param name="ext"></param>
         public void AddPlateauFollower(int data) {
            AbsSum0 += Math.Abs(data);
            Line++;
            SetHunit4HuvAndTabCol(AbsSum0, Line, maxheigthdiff);
         }

         public override string ToString() {
            return base.ToString() + string.Format(", AbsSum0={0}, Line={1}", AbsSum0, Line);
         }
      }


      public class HeightElement {

         /// <summary>
         /// Art der Codierung
         /// </summary>
         public enum EncodeMode {
            /// <summary>
            /// hybride Codierung
            /// </summary>
            Hybrid,
            /// <summary>
            /// Längecodierung Variante 0
            /// </summary>
            Length0,
            /// <summary>
            /// Längecodierung Variante 1
            /// </summary>
            Length1,
            /// <summary>
            /// nicht näher erklärt
            /// </summary>
            Special,
         }

         public enum Typ {
            Value,
            Plateau,
            PlateauFollower
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
         /// 
         /// </summary>
         public int PlateauFollowerDdiff { get; private set; }
         /// <summary>
         /// Tabellenpos. für die Unit des 1. 1-Bit des nächsten (!) Plateaus
         /// </summary>
         public int PlateauTabPointer4Unit { get; private set; }
         /// <summary>
         /// Tabellenpos. für die Bitanzahl des 1. 1-Bit des nächsten (!) Plateaus
         /// </summary>
         public int PlateauTabPointer4Bits { get; private set; }

         List<char> BitUnits;

         static List<int> PlateauBitUnitTable;
         static List<int> PlateauBitTable;

         List<int> Lines4Unit8;
         int lastplateauendline;






         static HeightElement() {
            BuildPlateauTables();
         }

         protected HeightElement(Typ typ,
                              int data,
                              EncodeMode encoding,
                              int column,
                              int line,
                              int hunit,
                              int maxheigth,
                              int linelength,
                              HeightElement lastplateau,
                              int plateaufollowerddiff) {
            Bits = new List<byte>();

            Column = column;
            Line = line;
            ElementTyp = typ;
            Data = data;
            Encoding = encoding;
            HUnit = hunit;
            PlateauFollowerDdiff = plateaufollowerddiff;
            if (lastplateau != null) {
               PlateauTabPointer4Unit = lastplateau.PlateauTabPointer4Unit;
               PlateauTabPointer4Bits = lastplateau.PlateauTabPointer4Bits;
               Lines4Unit8 = new List<int>(lastplateau.Lines4Unit8);
               lastplateauendline = lastplateau.lastplateauendline;
            } else {
               PlateauTabPointer4Unit = 0;
               PlateauTabPointer4Bits = 0;
               Lines4Unit8 = new List<int>();
               lastplateauendline = 0;
            }

            switch (typ) {
               case Typ.Value:
               case Typ.PlateauFollower:
                  switch (Encoding) {
                     case EncodeMode.Hybrid:
                        EncodeHybrid(data, maxheigth, hunit);
                        break;

                     case EncodeMode.Length0:
                        EncodeLength0(data);
                        break;

                     case EncodeMode.Length1:
                        EncodeLength1(data);
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

         static public HeightElement CreateHeightElement_ValueH(int data, int maxheigth, int hunit, int column, int line) {
            return new HeightElement(HeightElement.Typ.Value, data, HeightElement.EncodeMode.Hybrid, column, line, hunit, maxheigth, int.MinValue, null, int.MinValue);
         }

         static public HeightElement CreateHeightElement_ValueL(int data, bool l0, int column, int line) {
            return new HeightElement(HeightElement.Typ.Value, data, l0 ? HeightElement.EncodeMode.Length0 : HeightElement.EncodeMode.Length1, column, line, int.MinValue, int.MinValue, int.MinValue, null, int.MinValue);
         }

         static public HeightElement CreateHeightElement_PlateauFollowerH(int data, int hunit, int ddiff, int column, int line) {
            return new HeightElement(HeightElement.Typ.PlateauFollower, data, HeightElement.EncodeMode.Hybrid, column, line, hunit, int.MinValue, int.MinValue, null, ddiff);
         }

         static public HeightElement CreateHeightElement_PlateauFollowerL(int data, bool l0, int ddiff, int column, int line) {
            return new HeightElement(HeightElement.Typ.PlateauFollower, data, l0 ? HeightElement.EncodeMode.Length0 : HeightElement.EncodeMode.Length1, column, line, int.MinValue, int.MinValue, int.MinValue, null, ddiff);
         }

         static public HeightElement CreateHeightElement_Plateau(int length, int column, int line, int linelength, IList<HeightElement> oldheightelements) {
            HeightElement last = null;
            for (int i = oldheightelements.Count - 1; i >= 0; i--)
               if (oldheightelements[i].ElementTyp == HeightElement.Typ.Plateau) {
                  last = oldheightelements[i];
                  break;
               }
            return new HeightElement(HeightElement.Typ.Plateau, length, HeightElement.EncodeMode.Special, column, line, int.MinValue, int.MinValue, linelength, last, int.MinValue);
         }


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
         public string GetBitUnitsText() {
            return new string(BitUnits.ToArray());
         }

         /// <summary>
         /// liefert den Exponent n von 2^n oder -1, wenn hunit nicht 2^n ist
         /// </summary>
         /// <param name="hunit"></param>
         /// <returns></returns>
         int HunitExponent(int hunit) {
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
         /// codiert rein binär ohne Vorzeichen (MSB zuerst)
         /// </summary>
         /// <param name="data"></param>
         /// <param name="bitcount">Bitanzahl</param>
         void EncodeBinary(int data, int bitcount) {
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
         /// coidert hybrid
         /// </summary>
         /// <param name="data"></param>
         /// <param name="maxheigth"></param>
         /// <param name="hunit"></param>
         void EncodeHybrid(int data, int maxheigth, int hunit) {
            int hunitexp = HunitExponent(hunit);
            if (hunitexp < 0)
               throw new Exception(string.Format("HUnit {0} ist kein 2er-Potenz.", hunit));

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

            int maxm = GetMaxLengthBits(maxheigth);
            if (m <= maxm) {
               EncodeLength(m);                       // längencodierten Teil speichern
               EncodeBinary(bin, hunitexp);           // binär codierten Teil speichern
               Bits.Add((byte)(data > 0 ? 1 : 0));    // Vorzeichen speichern
            } else {
               EncodeLength(maxm + 1);
               EncodeBinarySpec(data, maxheigth);
            }
         }

         /// <summary>
         /// spezielle Binärcodierung für Hybridcodierung ohne Längen-0-Bits
         /// </summary>
         /// <param name="data"></param>
         /// <param name="maxheigth"></param>
         void EncodeBinarySpec(int data, int maxheigth) {
            int bitcount = 15;
            if (maxheigth < 2)
               bitcount = 1;
            else if (maxheigth < 4)
               bitcount = 2;
            else if (maxheigth < 8)
               bitcount = 3;
            else if (maxheigth < 16)
               bitcount = 4;
            else if (maxheigth < 32)
               bitcount = 5;
            else if (maxheigth < 64)
               bitcount = 6;
            else if (maxheigth < 128)
               bitcount = 7;
            else if (maxheigth < 256)
               bitcount = 8;
            else if (maxheigth < 512)
               bitcount = 9;
            else if (maxheigth < 1024)
               bitcount = 10;
            else if (maxheigth < 2048)
               bitcount = 11;
            else if (maxheigth < 4096)
               bitcount = 12;
            else if (maxheigth < 8192)
               bitcount = 13;
            else if (maxheigth < 16384)
               bitcount = 14;

            if (data == 0)
               throw new Exception("Der Datenwert 0 kann nicht in spezieller Binärcodierung angegeben werden.");

            bool vz;
            int dat;
            if (data > 0) {
               dat = data - 1;
               vz = false;
            } else {
               dat = -data - 1;
               vz = true;
            }

            int mask = 0x01 << (bitcount - 1);
            if ((dat & mask) != 0)
               throw new Exception(string.Format("Der Datenwert {0} kann nicht in spezieller Binärcodierung mit {1} Bit angegeben werden.", data, bitcount));

            for (int i = 0; i < bitcount - 1; i++) {
               mask >>= 1;
               Bits.Add((byte)((dat & mask) != 0 ? 1 : 0));
            }
            Bits.Add((byte)(vz ? 1 : 0));
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
         /// liefert die max. mögliche Anzahl 0-Bits für die Hybridcodierung
         /// </summary>
         /// <param name="maxheigth"></param>
         /// <returns></returns>
         int GetMaxLengthBits(int maxheigth) {
            if (maxheigth < 2)
               return 15;
            if (maxheigth < 4)
               return 16;
            if (maxheigth < 8)
               return 17;
            if (maxheigth < 16)
               return 18;
            if (maxheigth < 32)
               return 19;
            if (maxheigth < 64)
               return 20;
            if (maxheigth < 128)
               return 21;
            if (maxheigth < 256)
               return 22;
            if (maxheigth < 512)
               return 23;
            if (maxheigth < 1024)
               return 28;
            if (maxheigth < 2048)
               return 31;
            if (maxheigth < 4096)
               return 34;
            if (maxheigth < 8192)
               return 37;
            if (maxheigth < 16384)
               return 40;
            return 43;
         }

         /// <summary>
         /// erzeugt die statischen Plateaudaten
         /// </summary>
         static void BuildPlateauTables() {
            PlateauBitUnitTable = new List<int>();
            PlateauBitTable = new List<int>();

            for (int i = 0; i < 4; i++) PlateauBitUnitTable.Add(1);
            for (int i = 0; i < 4; i++) PlateauBitUnitTable.Add(2);
            for (int i = 0; i < 4; i++) PlateauBitUnitTable.Add(4);
            for (int i = 0; i < 4; i++) PlateauBitUnitTable.Add(8);
            for (int i = 0; i < 3; i++) PlateauBitUnitTable.Add(16);
            for (int i = 0; i < 3; i++) PlateauBitUnitTable.Add(32);
            for (int i = 0; i < 64; i++) PlateauBitUnitTable.Add(64);

            for (int i = 0; i < 3; i++) PlateauBitTable.Add(0);
            for (int i = 0; i < 4; i++) PlateauBitTable.Add(1);
            for (int i = 0; i < 4; i++) PlateauBitTable.Add(2);
            for (int i = 0; i < 4; i++) PlateauBitTable.Add(3);
            for (int i = 0; i < 3; i++) PlateauBitTable.Add(4);
            for (int i = 0; i < 3; i++) PlateauBitTable.Add(5);
            for (int i = 0; i < 65; i++) PlateauBitTable.Add(6);
         }

         /// <summary>
         /// erzeugt ein Plateau der vorgegebenen Länge
         /// </summary>
         /// <param name="length"></param>
         /// <param name="startcolumn"></param>
         /// <param name="startline"></param>
         /// <param name="linelength"></param>
         void EncodePlateau(int length, int startcolumn, int startline, int linelength) {
            BitUnits = new List<char>();

            bool bLineEnd = false;
            int lastunitidx = -1;

            if (lastplateauendline != startline) {
               if (PlateauTabPointer4Unit > 0)
                  PlateauTabPointer4Unit--; // wenn min. 1 Zeilenwechsel ohne Plateau erfolgte
               lastplateauendline = startline;
            }

            int bitunit = 0;
            do {
               bitunit = PlateauBitUnitTable[PlateauTabPointer4Unit];

               if (bitunit == 0x08)
                  Lines4Unit8.Add(startline);

               if (bitunit <= length ||
                   startcolumn + length > linelength) { // mit Zeilenwechsel
                  switch (bitunit) {
                     case 0x1: BitUnits.Add('1'); break;
                     case 0x2: BitUnits.Add('2'); break;
                     case 0x4: BitUnits.Add('4'); break;
                     case 0x8: BitUnits.Add('8'); break;
                     case 0x10: BitUnits.Add('6'); break;
                     case 0x20: BitUnits.Add('3'); break;
                     case 0x40: BitUnits.Add('x'); break;
                  }

                  Bits.Add(1);
                  lastunitidx = PlateauTabPointer4Unit++;
                  PlateauTabPointer4Bits++;

                  length -= bitunit;
                  startcolumn += bitunit;

                  bLineEnd = false;

                  if (startcolumn >= linelength) { // erreicht das Zeilenende oder geht darüber hinaus -> Korrektur
                     bLineEnd = true;
                     startline++;
                     lastplateauendline++;

                     if (startcolumn > linelength &&
                         PlateauTabPointer4Unit > 1) {
                        PlateauTabPointer4Unit--;
                        PlateauTabPointer4Bits--;
                     }

                     length += startcolumn - linelength;
                     startcolumn = 0;
                  }
               } else
                  break;

            } while (true);

            if (Bits.Count > 0) {
               if (PlateauTabPointer4Unit > 0)
                  PlateauTabPointer4Unit--; // Endposition ist neue Startpos.
            }
            if (PlateauTabPointer4Bits > 0)
               PlateauTabPointer4Bits--; // Sollte besser nur so erfolgen, dass hinreichend viele Bits entsprechend PlateauTabPointer4Unit übrig bleiben?!?!!

            int bitsdelta = 0;
            if (bLineEnd) { // wenn am Zeilenende die LETZTE 1er, 2er, 4er oder 8er Unit verwendet wurde, wird akt. 1 Bit weniger im Binärteil verwendet
               if (lastunitidx == 3 ||
                   lastunitidx == 7 ||
                   lastunitidx == 11 ||
                   lastunitidx == 15)
                  bitsdelta = -1;
            }
            if (PlateauTabPointer4Unit >= 16) { // 16er Unit beginnt
               if (Lines4Unit8.Count > 4 || // 8er Unit wurde mehr als 4x ...
                   (Lines4Unit8.Count == 4 && Lines4Unit8[0] != Lines4Unit8[3])) { // ... oder genau 4x aber auf unterschiedlichen Zeilen verwendet
                  PlateauTabPointer4Bits++;
                  Lines4Unit8.Clear();
               }
            }


            // Basis abschließen (Trennbit)
            Bits.Add(0);

            // Rest binär codieren
            if (PlateauBitTable[PlateauTabPointer4Bits] > 0)
               EncodeBinary(length, PlateauBitTable[PlateauTabPointer4Bits + bitsdelta]);
         }

         public override string ToString() {
            StringBuilder sb = new StringBuilder(ElementTyp.ToString());
            if (Column >= 0)
               sb.Append(", Column=" + Column.ToString());
            if (Line >= 0)
               sb.Append(", Line=" + Line.ToString());
            sb.Append(", Data=" + Data.ToString());
            if (this.ElementTyp == Typ.Plateau)
               sb.Append(", PlateauTabColPos=" + PlateauTabPointer4Unit.ToString());
            sb.Append(", Encoding=" + Encoding.ToString());
            if (Encoding == EncodeMode.Hybrid)
               sb.Append(" (" + HUnit.ToString() + ")");
            sb.Append(", Bits=");
            for (int i = 0; i < Bits.Count; i++)
               sb.Append(Bits[i] > 0 ? "1" : ".");
            if (this.ElementTyp == Typ.Plateau)
               sb.Append(" " + GetBitUnitsText());
            else if (this.ElementTyp == Typ.PlateauFollower)
               sb.Append(" ddiff=" + PlateauFollowerDdiff);

            return sb.ToString();
         }

      }

      /// <summary>
      /// Methode zur Bestimmung der C0- oder C1-Codierung
      /// </summary>
      class LengthCodingType {

         /// <summary>
         /// liefert die Art der Längencodierung
         /// </summary>
         /// <param name="column"></param>
         /// <param name="line"></param>
         /// <returns></returns>
         public static HeightElement.EncodeMode GetLengthCoding(Position pos, int tileSize, List<int> Data) {
            List<int> data = new List<int>();
            for (int y = 0; y <= pos.Y; y++) {
               for (int x = 0; x < tileSize; x++) {
                  if (y == pos.Y && x == pos.X)
                     break;
                  if (y == 0 && x == 0)            // oder Spalte 0 nie mitzählen ???
                     continue;

                  data.Add(Data[x + tileSize * y]);
               }
            }
            return GetLengthCoding(data);
         }

         public static HeightElement.EncodeMode GetLengthCoding(Position pos, IList<HeightElement> elements) {
            List<int> data = new List<int>();

            if (elements.Count > 0)
               for (int i = 0; i < elements.Count; i++) {
                  if (elements[i].Line > pos.Y)
                     break;
                  if (elements[i].Line == pos.Y &&
                      elements[i].Column >= pos.X)
                     break;

                  if (elements[i].Line == 0 &&
                      elements[i].Column == 0)            // oder Spalte 0 nie mitzählen ???
                     continue;

                  if (elements[i].Line >= 0 &&
                      elements[i].Column >= 0 &&
                      elements[i].ElementTyp == HeightElement.Typ.Value)
                     data.Add(elements[i].Data);
               }

            return GetLengthCoding(data);
         }

         /// <summary>
         /// liefert die Art der Längencodierung
         /// </summary>
         /// <param name="data"></param>
         /// <returns></returns>
         static HeightElement.EncodeMode GetLengthCoding(List<int> data) {
            int datasum = 0;
            for (int i = 0; i < data.Count; i++)
               datasum += data[i];
            return RaisingSum(data) <= datasum ?
                                    HeightElement.EncodeMode.Length1 :
                                    HeightElement.EncodeMode.Length0;
         }

         /// <summary>
         /// berechnet aus D1..Dn die min. Höhe für L1
         /// </summary>
         /// <returns></returns>
         static int RaisingSum(List<int> data) {
            int shift = 0;
            int raising = 0;
            int d1 = data.Count > 0 ? data[0] : 0;
            int d2 = data.Count > 1 ? data[1] : 0;
            for (int n = 1; n <= data.Count; n++) {
               shift = Shift4RaisingN(shift, n > 1 ? data[n - 2] : 0, n);
               raising += RaisingN(n, shift, d1, d2, data[n - 1]);
            }
            return raising;
         }

         /// <summary>
         /// Verschiebung der Erhöhungsfolge (für L1)
         /// </summary>
         /// <param name="lastshift1">Verschiebung für Zeile 1 (0 .. n-2)</param>
         /// <param name="dn_1">Wert des vorletzten Deltas (entspricht Zeilennummer; &gt;= 1)</param>
         /// <param name="n">Anzahl der Deltas - 1 (D0..Dn)</param>
         /// <returns></returns>
         static int Shift4RaisingN(int lastshift1, int dn_1, int n) {
            if (n > 1) {      // Zeilenbereiche für Verschiebungen testen
               if (dn_1 < lastshift1 - 2 * n + 2)
                  return n - 1;
               if (dn_1 <= lastshift1 + n)
                  return (lastshift1 + 2 * n + 1 - dn_1) % n;
            }
            return 0;
         }

         /// <summary>
         /// (zusätzliche) Erhöhung für das Delta Dn (für C1)
         /// </summary>
         /// <param name="n">Anzahl der Deltas - 1 (D0..Dn)</param>
         /// <param name="shift">Verschiebung der Erhöhungsfolge für D(n-1); 0 .. (n-1)</param>
         /// <param name="d1">Delta D1 (wegen Sonderfälle)</param>
         /// <param name="d2">Delta D2 (wegen Sonderfälle)</param>
         /// <param name="dn">Delta Dn</param>
         /// <returns></returns>
         static int RaisingN(int n, int shift, int d1, int d2, int dn) {
            if (n <= 0) { // (sinnlos)

               return 0;

            } else if (n == 1) { // nur 1 Datenwert

               // -9 .. 9: ...,2,2,2,2,2,2,2,2,0,2,2,6,6,8,10,12,14,16,18,...
               return dn == -1 ? 0 :
                      dn <= 0 ? 2 :
                      dn == 2 ? 6 :
                                 2 * dn;

            } else { // min. 2 Datenwerte

               int raising = 0;
               if (dn >= 0) { // letzter Wert ist nicht negativ

                  // je nach n und shift z.B. für n=2 und shift=1: 0,0,6,6,6, 8,10,12,14,...
                  if (dn <= shift + 1) {
                     raising = 0;
                  } else if (dn <= shift + n + 2) {
                     raising = 2 * (n + 1);
                  } else {
                     raising = 2 * (dn - shift - 1);
                  }

                  // --- Sonderfälle ---
                  if (n == 2) {
                     if (d1 == 0 && dn == 0) {          // Sonderfall "0,0"
                        raising = 1;
                     }
                  } else if (n == 3) {
                     if (d1 == 1 || d1 == -1) {
                        if ((d2 == 0 && dn == 2) ||    // Sonderfall "+-1,0,2" und "+-1,1,1"
                            (d2 == 1 && dn == 1)) {
                           raising = 1;
                        }
                     }
                  }

               } else { // letzter Wert ist negativ

                  // z.B für n=4:
                  //     dn -9 -8 -7 -6 -5 -4 -3 -2 -1
                  //        --------------------------
                  // shif=3: 0, 0, 0, 0, 0,-2,-4,-6,-8
                  // shif=2: 2, 2, 2, 2, 0,-2,-4,-6, 2
                  // shif=1: 4, 4, 4, 2, 0,-2,-4, 4, 2
                  // shif=0: 6, 6, 4, 2, 0,-2, 6, 4, 2
                  if (dn < shift - 2 * n) {
                     raising = 2 * (n - 1 - shift);
                  } else {
                     raising = 2 * ((1 - dn + shift) % (n + 1) - shift - 1);
                  }

                  // --- Sonderfälle ---
                  if (n == 2) {
                     if (d1 == 0 && dn == -1) {           // Sonderfall "0,-1"
                        raising = -3;
                     }
                  } else if (n == 3) {
                     if (d1 == 1 || d1 == -1) {
                        if (d2 == 0 && dn == -2) {        // Sonderfall "+-1,0,-2"
                           raising = -3;
                        } else if (d2 == -1 && dn == -1) {   // Sonderfall "+-1,-1,-1"
                           raising = -5;
                        }
                     }
                  }

               }

               return raising;
            }

         }

      }


      /// <summary>
      /// nächste Position für Höhen und Daten
      /// </summary>
      Position nextPosition;

      /// <summary>
      /// akt. Codierungs-Art
      /// </summary>
      public HeightElement.EncodeMode ActualMode {
         get {
            return Elements.Count > 0 ?
                        Elements[Elements.Count - 1].Encoding :
                        HeightElement.EncodeMode.Special;
         }
      }

      /// <summary>
      /// max. zulässige Höhe
      /// </summary>
      public int MaxHeigth { get; protected set; }

      /// <summary>
      /// Kachelgröße
      /// </summary>
      public int TileSize { get; protected set; }

      #endregion

      HeightUnitPlateau4DdiffEqual hu_plateau_equal;
      HeightUnitPlateau4DdiffUnequal hu_plateau_unequal;
      HeightUnitStd hu_std;

      public List<HeightElement> Elements { get; private set; }



      HeightUnit baseHeigthUnit;

      /// <summary>
      /// hunit bei der Init. des Encoders (abh. von der Maximalhöhe; konstant; max. 256)
      /// </summary>
      public int BaseHeigthUnit {
         get {
            return baseHeigthUnit.Value;
         }
      }

      /// <summary>
      /// akt. hunit
      /// </summary>
      public int HeigthUnit { get; private set; }

      /// <summary>
      /// liefert die akt. Höhe
      /// </summary>
      public int ActualHeigth {
         get {
            Position pos = new Position(nextPosition);
            pos.Back();
            int idx = pos.Idx;
            return idx < HeightValues.Count ? HeightValues[pos.Idx] : 0;
         }
      }

      /// <summary>
      /// Standardhöhe der akt. Zeile
      /// </summary>
      public int StdHeigth { get; protected set; }

      /// <summary>
      /// Zeile der nächsten Höhe
      /// </summary>
      public int NextHeigthLine {
         get {
            return nextPosition.Y;
         }
      }

      /// <summary>
      /// Spalte der nächsten Höhe
      /// </summary>
      public int NextHeigthColumn {
         get {
            return nextPosition.X;
         }
      }

      /// <summary>
      /// alle Höhenwerte
      /// </summary>
      public List<int> HeightValues { get; protected set; }


      /// <summary>
      /// 
      /// </summary>
      /// <param name="maxheigth">max. Höhe</param>
      /// <param name="tilesize">Breite/Höhe der Kachel</param>
      /// <param name="height">Höhendaten (normalerweise <see cref="tilesize"/> * <see cref="tilesize"/>)</param>
      public TileEncoder(int maxheigth, int tilesize, IList<int> height) {
         MaxHeigth = maxheigth;
         TileSize = tilesize;
         StdHeigth = 0;
         HeightValues = new List<int>(height);
         nextPosition = new Position(tilesize, tilesize);

         hu_std = new HeightUnitStd(MaxHeigth);
         hu_plateau_equal = new HeightUnitPlateau4DdiffEqual(MaxHeigth);
         hu_plateau_unequal = new HeightUnitPlateau4DdiffUnequal(MaxHeigth);

         Elements = new List<HeightElement>();

         baseHeigthUnit = new HeightUnit(MaxHeigth, true);

      }

      /// <summary>
      /// liefert die codierten Bytes
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

      /// <summary>
      /// führt die Codierung der nächsten Höhe/n aus
      /// <para>I.A. wird hierbei der nächste Höhenwert codiert. Es können aber auch mehrere Höhenwerte gemeinsam codiert werden, wenn dass wegen des 
      /// Codierverfahrens nötig ist.</para>
      /// </summary>
      /// <returns>Anzahl der im aktuellen Schritt codierten Elemente (0 bedeutet Ende)</returns>
      public int ComputeNext() {
         int elements = Elements.Count;
         if (nextPosition.Idx < HeightValues.Count)
            CalculateData(nextPosition);      // die akt. Höhe oder zusätzlich mehrere folgende Höhen codieren
         return Elements.Count - elements; ;
      }


      /// <summary>
      /// 
      /// </summary>
      /// <param name="pos"></param>
      void CalculateData(Position pos) {
         if (Height(pos) >= 0) { // nur nichtnegative Höhen

            try {

               int ddiff = HeightDDiff(pos);

               if (ddiff == 0) { // die Diagonale hat konstante Höhe (gilt auch für die 1. Spalte) -> immer Plateau (ev. auch mit Länge 0)

                  WritePlateau(GetPlateauLength(pos), pos, hu_plateau_equal, hu_plateau_unequal);

               } else { // "Normalfall"

                  // Vermutung für Normalfall: d(i,n) = -sgn(ddiff(i, n)) * (hdiff(i, n) – hdiff(i, n-1)))
                  int hdiffu = HeightHDiff(pos.X, pos.Y - 1);
                  int data = Height(pos, -1) < -hdiffu ?
                                                   -Math.Sign(ddiff) * Height(pos) :
                                                   -Math.Sign(ddiff) * (HeightHDiff(pos) - hdiffu);


                  // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                  /* vorläufig als Spezialfall: h(col+1,n-1) = h(col+2,n-2)

                   *    * * A
                   *    * A *
                   *    ?
                  */
                  //if (pos.Y > 0 && ValidHeight(pos.X + 1, pos.Y - 1) == ValidHeight(pos.X + 2, pos.Y - 2))
                  //   if (HeigthDDiff(pos) > 0) {
                  //      data = -Height(pos) + 1;
                  //   } else {
                  //      data = Height(pos);
                  //   }
                  // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

                  /*    * A
                   *    A *
                   *    ?
                   */
                  //if (pos.Y == 1) {
                  //   if (ValidHeight(pos.X, pos.Y - 1) == ValidHeight(pos.X + 1, pos.Y - 2)) {
                  //      int testvdiff = HeigthVDiff(pos.X - 1, pos.Y);
                  //      if (testvdiff > 0)
                  //         data = Height(pos) - testvdiff;
                  //      else
                  //         data = HeigthVDiff(pos);
                  //   }
                  //}

                  /*    * 0
                   *    0 *
                   *    ?
                   */
                  if (ValidHeight(pos.X, pos.Y - 1) == 0 &&
                      ValidHeight(pos.X + 1, pos.Y - 2) == 0) {
                     if (HeightVDiff(pos.X - 1, pos.Y) <= 0)
                        data = HeightVDiff(pos);
                  }


                  if (hu_std.Value >= 1)
                     Elements.Add(HeightElement.CreateHeightElement_ValueH(data, MaxHeigth, hu_std.Value, pos.X, pos.Y));
                  else
                     Elements.Add(HeightElement.CreateHeightElement_ValueL(data, LengthCodingType.GetLengthCoding(pos, Elements) == HeightElement.EncodeMode.Length0, pos.X, pos.Y));
                  pos.Next();

                  hu_std.AddValue(data);

               }

            } catch (Exception ex) {
               throw new Exception(string.Format("interner Fehler bei Position {0}, Höhe {1}: {2}", pos, Height(pos), ex.Message));
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="length"></param>
      /// <param name="pos"></param>
      /// <param name="hunit_ddiffequal4follower"></param>
      /// <param name="hunit_ddiffunequal4follower"></param>
      /// <returns>liefert den Datenwert der folgenden Höhe</returns>
      int WritePlateau(int length, Position pos, HeightUnitPlateau4DdiffEqual hunit_ddiffequal4follower, HeightUnitPlateau4DdiffUnequal hunit_ddiffunequal4follower) {
         Elements.Add(HeightElement.CreateHeightElement_Plateau(length, pos.X, pos.Y, TileSize, Elements));

         pos.Next(length);

         int follower = 0;

         if (pos.X != 0 ||       // Das ist die Position für den Follower!
             length == 0) {      // dann muss es immer einen Nachfolger geben
            follower = Height(pos);
            int follower_ddiff = pos.X == 0 ?
                                       0 :                  // in der 1. Spalte ist die Berechnung anders
                                       HeightDDiff(pos);
            int follower_vdiff = HeightVDiff(pos);
            if (follower_ddiff != 0) {
               if (follower_ddiff > 0)
                  follower_vdiff *= -1;
               Elements.Add(HeightElement.CreateHeightElement_PlateauFollowerH(follower_vdiff, hunit_ddiffunequal4follower.Value, follower_ddiff, pos.X, pos.Y));
               hunit_ddiffunequal4follower.AddPlateauFollower(follower_vdiff);
            } else {
               if (follower_vdiff < 0)
                  follower_vdiff++;
               Elements.Add(HeightElement.CreateHeightElement_PlateauFollowerH(follower_vdiff, hunit_ddiffequal4follower.Value, follower_ddiff, pos.X, pos.Y));
               hunit_ddiffequal4follower.AddPlateauFollower(follower_vdiff);
            }
            follower = follower_vdiff;
            pos.Next();
         }

         return follower; // ist jetzt der Datenwert
      }


      /// <summary>
      /// ermittelt die Länge eines Plateaus ab der Startposition (ev. auch 0)
      /// </summary>
      /// <param name="pos">Startpos.</param>
      /// <returns></returns>
      int GetPlateauLength(Position startpos) {
         Position tst = new Position(startpos);
         int length = 0;
         int value = -1;

         while (tst.Idx < HeightValues.Count) {
            if (value < 0)
               value = ValidHeight(tst.X - 1, tst.Y);

            if (HeightValues[tst.Idx] != value)
               break;

            length++;
            tst.Next();

            if (tst.X == 0)
               value = -1;
         }

         return length;
      }


      #region spezielle Höhendifferenzen

      /// <summary>
      /// liefert die horizontale Höhendifferenz (zur Vorgängerhöhe)
      /// </summary>
      /// <param name="pos"></param>
      /// <returns></returns>
      int HeightHDiff(Position pos) {
         return HeightHDiff(pos.X, pos.Y);
      }
      /// <summary>
      /// liefert die horizontale Höhendifferenz (zur Vorgängerhöhe)
      /// </summary>
      /// <param name="col"></param>
      /// <param name="line"></param>
      /// <returns></returns>
      int HeightHDiff(int col, int line) {
         return ValidHeight(col, line) - ValidHeight(col - 1, line);
      }
      /// <summary>
      /// liefert die vertikale Höhendifferenz (zur darüber liegenden Höhe)
      /// </summary>
      /// <param name="pos"></param>
      /// <returns></returns>
      int HeightVDiff(Position pos) {
         return HeightVDiff(pos.X, pos.Y);
      }
      /// <summary>
      /// liefert die vertikale Höhendifferenz (zur darüber liegenden Höhe)
      /// </summary>
      /// <param name="col"></param>
      /// <param name="line"></param>
      /// <returns></returns>
      int HeightVDiff(int col, int line) {
         return ValidHeight(col, line) - ValidHeight(col, line - 1);
      }
      /// <summary>
      /// liefert die diagonale Höhendifferenz (der darüber liegenden Höhe zur Vorgängerhöhe)
      /// </summary>
      /// <param name="pos"></param>
      /// <returns></returns>
      int HeightDDiff(Position pos) {
         return HeightDDiff(pos.X, pos.Y);
      }
      /// <summary>
      /// liefert die diagonale Höhendifferenz (der darüber liegenden Höhe zur Vorgängerhöhe)
      /// </summary>
      /// <param name="col"></param>
      /// <param name="line"></param>
      /// <returns></returns>
      int HeightDDiff(int col, int line) {
         return ValidHeight(col, line - 1) - ValidHeight(col - 1, line);
      }

      #endregion

      #region Zugriff auf Höhenwerte

      /// <summary>
      /// liefert immer eine verarbeitbare Höhe, d.h. außerhalb der <see cref="TileSize"/> immer 0 bzw. die virtuelle Spalte
      /// </summary>
      /// <param name="column"></param>
      /// <param name="line"></param>
      /// <returns></returns>
      public int ValidHeight(int column, int line) {
         if (0 <= column && column < TileSize &&
             0 <= line && line < TileSize) { // innerhalb des Standardbereiches
            int h = Height(column, line);
            if (h >= 0)
               return h;
         }
         if (column == -1 &&
             0 <= line && line < TileSize) { // virtuelle Spalte
            int h = Height(column + 1, line - 1);
            return h >= 0 ? h : 0;
         }
         return 0;
      }

      /// <summary>
      /// liefert die Höhe aus Spalte und Zeile
      /// </summary>
      /// <param name="column"></param>
      /// <param name="line"></param>
      /// <returns>negativ, wenn ungültig</returns>
      public int Height(int column, int line) {
         return Height(TileSize * line + column);
      }
      /// <summary>
      /// liefert die Höhe zum Index
      /// </summary>
      /// <param name="idx"></param>
      /// <returns>negativ, wenn ungültig</returns>
      int Height(int idx) {
         return 0 <= idx && idx < HeightValues.Count ?
                        HeightValues[idx] :
                        -1;
      }
      /// <summary>
      /// liefert die Höhe bezüglich der Position
      /// </summary>
      /// <param name="pos"></param>
      /// <param name="deltax"></param>
      /// <param name="deltay"></param>
      /// <returns>negativ, wenn ungültig</returns>
      int Height(Position pos, int deltax = 0, int deltay = 0) {
         if (pos.X + deltax < 0 || pos.X + deltax >= pos.Width ||
             pos.Y + deltay < 0 || pos.Y + deltay >= pos.Height)
            return -1;
         return Height(new Position(pos, deltax, deltay).Idx);
      }

      #endregion

      #region zum Ermitteln der Bitfolgen

      static public List<byte> LengthCoding0(int data) {
         return new List<byte>(HeightElement.CreateHeightElement_ValueL(data, true, int.MinValue, int.MinValue).Bits);
      }

      static public List<byte> LengthCoding1(int data) {
         return new List<byte>(HeightElement.CreateHeightElement_ValueL(data, false, int.MinValue, int.MinValue).Bits);
      }

      static public List<byte> HybridCoding(int data, int maxheigth, int hunit) {
         return new List<byte>(HeightElement.CreateHeightElement_ValueH(data, maxheigth, hunit, int.MinValue, int.MinValue).Bits);
      }

      #endregion

      public override string ToString() {
         return string.Format("MaxHeigth={0}, TileSize={1}, BaseHeigthUnit={2}, HeigthUnit={3}, ActualMode={4}, ActualHeigth={5}",
                              MaxHeigth,
                              TileSize,
                              BaseHeigthUnit,
                              HeigthUnit,
                              ActualMode,
                              ActualHeigth);
      }

   }
}
