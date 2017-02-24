using System;
using System.Collections.Generic;
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
               Idx = Idx + 1;
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
      /// spezielle <see cref="HeightUnit"/>, wenn ddiff=0
      /// </summary>
      class HeightUnit0 : HeightUnit {

         int maxheigthdiff = 0;

         /// <summary>
         /// akt. für diese Summe der Absolutwerte
         /// </summary>
         public int AbsSum0 { get; private set; }
         /// <summary>
         /// akt. für diese Zeilennummer
         /// </summary>
         public int Line { get; private set; }

         public HeightUnit0(int maxheigthdiff)
            : base(maxheigthdiff) {
            this.maxheigthdiff = maxheigthdiff;
            AbsSum0 = 0;
            Line = -1;
         }

         public HeightUnit0(HeightUnit0 hu)
            : base(hu) {
            maxheigthdiff = hu.maxheigthdiff;
            AbsSum0 = hu.AbsSum0;
            Line = hu.Line;
         }

         /// <summary>
         /// der Datenwert der 1. Spalte der nächsten Zeile wird ergänzt
         /// </summary>
         /// <param name="col0data"></param>
         public void AddCol0Value(int col0data) {
            AbsSum0 += Math.Abs(col0data);
            Line++;
            SetHunit4HuvAndTabCol(AbsSum0 - 1, Line + 1, maxheigthdiff);
         }

         public override string ToString() {
            return base.ToString() + string.Format(", AbsSum0={0}, Line={1}", AbsSum0, Line);
         }
      }

      /// <summary>
      /// spezielle <see cref="HeightUnit"/> für die 1. Spalte
      /// </summary>
      class HeightUnit4Column0 : HeightUnit {

         int maxheigthdiff = 0;

         /// <summary>
         /// akt. für diese Zeilennummer
         /// </summary>
         public int Line {
            get {
               return data0.Count - 1;
            }
         }

         List<int> data0;
         List<int> height0;


         public HeightUnit4Column0(int maxheigthdiff)
            : base(maxheigthdiff, true) {                // Init. für d(0,0)
            this.maxheigthdiff = maxheigthdiff;
            data0 = new List<int>();
            height0 = new List<int>();
         }

         public HeightUnit4Column0(HeightUnit4Column0 hu)
            : base(hu) {
            maxheigthdiff = hu.maxheigthdiff;
            data0 = new List<int>(hu.data0);
            height0 = new List<int>(hu.height0);
         }

         /// <summary>
         /// der Datenwert der 1. Spalte der nächsten Zeile wird ergänzt
         /// </summary>
         /// <param name="col0data"></param>
         /// <param name="col0height"></param>
         public void AddCol0Value(int col0data, int col0height) {
            data0.Add(col0data);
            height0.Add(col0height);

            int huv = 0;
            int hunittabcol = 0;

            switch (data0.Count) {
               case 1:
                  huv = data0[0];
                  break;

               case 2:
                  huv = data0[0] + Math.Abs(data0[1]);
                  if (height0[1] > height0[0])
                     huv--;
                  break;

               case 3:
                  huv = data0[0] + Math.Abs(data0[1]) + Math.Abs(data0[2]);
                  if (height0[2] > height0[1] &&
                      height0[1] > height0[0])
                     huv--;
                  else if (height0[2] < height0[1] &&
                           height0[1] < height0[0])
                     huv++;
                  break;




               default:
                  throw new Exception(string.Format("hunit-Bildung für H(0,{0}) noch unbekannt", data0.Count - 1));
            }
            hunittabcol = data0.Count;

            SetHunit4HuvAndTabCol(huv, hunittabcol, maxheigthdiff);
         }

         public override string ToString() {
            return base.ToString() + string.Format(", Line={0}", Line);
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
            : base(maxheigthdiff) {                   // Init für d(1,0) analog wie d(0,0)
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
         /// Tabellenpos. für das 1. 1-Bit bei einem Sprung
         /// </summary>
         public int PlateauTabColPos { get; private set; }
         /// <summary>
         /// Zeilenlänge (nur bei Sprung nötig)
         /// </summary>
         int linelength;

         public HeightElement(Typ typ, int data = int.MinValue,
                              EncodeMode encoding = EncodeMode.Special,
                              int line = int.MinValue,
                              int column = int.MinValue,
                              int hunit = int.MinValue,
                              int tabcolpos = int.MinValue,
                              int linelength = int.MinValue) {
            Bits = new List<byte>();

            Column = column;
            Line = line;
            ElementTyp = typ;
            Data = data;
            Encoding = encoding;
            HUnit = hunit;
            PlateauTabColPos = 1;

            switch (typ) {
               case Typ.Value:
                  switch (Encoding) {
                     case EncodeMode.Hybrid:
                        EncodeHybrid(data, hunit);
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

         static public HeightElement CreateHeightElement_ValueH(int data, int hunit, int column, int line) {
            return new HeightElement(HeightElement.Typ.Value, data, HeightElement.EncodeMode.Hybrid, line, column, hunit);
         }

         static public HeightElement CreateHeightElement_ValueL(int data, bool l0, int column, int line) {
            return new HeightElement(HeightElement.Typ.Value, data, l0 ? HeightElement.EncodeMode.Length0 : HeightElement.EncodeMode.Length1, line, column);
         }

         static public HeightElement CreateHeightElement_Plateau(int length, int column, int line, int linelength, int tabcolpos) {
            return new HeightElement(HeightElement.Typ.Plateau, length, HeightElement.EncodeMode.Special, line, column, int.MinValue, tabcolpos, linelength);
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
         /// <param name="hunit"></param>
         void EncodeHybrid(int data, int hunit) {
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

            EncodeLength(m);                       // längencodierten Teil speichern
            EncodeBinary(bin, hunitexp);           // binär codierten Teil speichern
            Bits.Add((byte)(data > 0 ? 1 : 0));    // Vorzeichen speichern
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
         /// Hilfsfunktion für die Längencodierung
         /// </summary>
         /// <param name="count0bits"></param>
         void EncodeLength(int count0bits) {
            if (count0bits > 22)
               throw new Exception("Mehr als 22 0-Bits können für die Längencodierung nicht verwendet werden.");

            for (int i = 0; i < count0bits; i++)
               Bits.Add(0);
            Bits.Add(1);
         }

         /// <summary>
         /// erzeugt ein Plateau der vorgegebenen Länge
         /// </summary>
         /// <param name="length"></param>
         /// <param name="startcolumn"></param>
         /// <param name="startline"></param>
         /// <param name="linelength"></param>
         void EncodePlateau(int length, int startcolumn, int startline, int linelength) {
            if ((length + startcolumn) < linelength)  // bleibt innerhalb einer Zeile
               length = EncodePlateauBase(length);
            else { // mit Übergang auf nächste Zeile/n
               do {
                  EncodePlateauBase(linelength - startcolumn);
                  Bits.Add(1);
                  PlateauTabColPos++;
                  length -= linelength - startcolumn;
                  startcolumn = 0;
               } while (length >= linelength);

               while (length >= linelength) {
                  EncodePlateauBase(linelength);
                  Bits.Add(1);
                  PlateauTabColPos++;
                  length -= linelength;
               }
               length = EncodePlateauBase(length);
            }

            // Basis abschließen
            Bits.Add(0);

            // Rest binär codieren
            int binbits = PlateauTabColPos / 4;
            if (binbits > 0)
               EncodeBinary(length, binbits);
         }

         /// <summary>
         /// schreibt nur die nötigen 1-Bits
         /// </summary>
         /// <param name="length"></param>
         /// <returns>Restlänge</returns>
         int EncodePlateauBase(int length) {
            int unit = 0;
            do {
               unit = 1 << ((PlateauTabColPos - 1) / 4);
               if (length >= unit) {
                  Bits.Add(1);
                  PlateauTabColPos++;
                  length -= unit;
               }
            } while (length >= unit);
            return length;
         }

         public override string ToString() {
            StringBuilder sb = new StringBuilder(ElementTyp.ToString());
            if (Column >= 0)
               sb.Append(", Column=" + Column.ToString());
            if (Line >= 0)
               sb.Append(", Line=" + Line.ToString());
            sb.Append(", Data=" + Data.ToString());
            if (this.ElementTyp == Typ.Plateau)
               sb.Append(", PlateauTabColPos=" + PlateauTabColPos.ToString());
            sb.Append(", Encoding=" + Encoding.ToString());
            if (Encoding == EncodeMode.Hybrid)
               sb.Append(" (" + HUnit.ToString() + ")");
            sb.Append(", Bits=");
            for (int i = 0; i < Bits.Count; i++)
               sb.Append(Bits[i] > 0 ? "1" : ".");
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
         /// berechnet aus D1..Dn die min. Höhe für C1
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
         /// Verschiebung der Erhöhungsfolge (für C1)
         /// </summary>
         /// <param name="shift1">Verschiebung für Zeile 1 (0 .. n-2)</param>
         /// <param name="dn_1">Wert des vorletzten Deltas (entspricht Zeilennummer; &gt;= 1)</param>
         /// <param name="n">Anzahl der Deltas - 1 (D0..Dn)</param>
         /// <returns></returns>
         static int Shift4RaisingN(int shift1, int dn_1, int n) {
            if (n > 1 &&
                1 <= dn_1 && dn_1 <= shift1 + n)         // Zeilenbereich für Verschiebungen
               return (shift1 + 2 * n + 1 - dn_1) % n;
            return 0;
         }

         /// <summary>
         /// (zusätzliche) Erhöhung für das Delta Dn (für C1)
         /// </summary>
         /// <param name="n">Anzahl der Deltas - 1 (D0..Dn)</param>
         /// <param name="shift">Verschiebung der Erhöhungsfolge für D(n-1)</param>
         /// <param name="d1">Delta D1 (wegen Sonderfälle bei n == 3 oder bei n == 2)</param>
         /// <param name="d2">Delta D2 (wegen Sonderfall bei n == 3)</param>
         /// <param name="dn">Delta Dn</param>
         /// <returns></returns>
         static int RaisingN(int n, int shift, int d1, int d2, int dn) {
            if (n <= 0) {
               return 0;         // (sinnlos)
            } else if (n == 1) {
               // -9 .. 9: ...,2,2,2,2,2,2,2,2,0,-,2,6,6,8,10,12,14,16,18,...
               return 2 * (dn < -1 ? 1 :
                           dn == -1 ? 0 :
                           dn == 2 ? 3 :
                                      dn);
            } else {
               int raising = 0;
               if (dn >= 0) {         // pos. Dn

                  if (dn <= shift + 1) {
                     raising = 0;
                  } else if (dn <= shift + n + 2) {
                     if (n == 2 &&
                         d1 == 1 &&
                         dn == 2) {
                        raising = 1;
                     } else {
                        raising = 2 * (n + 1);
                     }
                  } else {
                     raising = 2 * (dn - shift - 1);
                  }

               } else {                // neg. Dn

                  if (n == 1) {
                     if (dn >= -1) {
                        raising = 0;
                     } else {
                        raising = 2;
                     }
                  } else {
                     if (dn < shift - 2 * n) {
                        raising = 2 * (n - 1 - shift);
                     } else {
                        raising = 2 * ((1 - dn + shift) % (n + 1) - shift - 1);
                     }
                  }

               }

               if (n == 3 &&
                   d1 == 1 &&
                   d2 == 2 &&
                   dn != 1) {         // Sonderfall 1,2,dn
                  raising += 5;
               } else if (n == 3 &&
                        d1 == 1 &&
                        d2 == 1 &&
                        dn != 1) {    // Sonderfall -1,1,dn
                  raising -= 1;
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


      HeightUnit0 hu0;
      HeightUnit4Column0 hu4col0, next_hu4col0;
      HeightUnitStd hustd;

      public List<HeightElement> Elements { get; private set; }

      int plateau_TabColPos;






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
      /// hunit für Spalte 0 der akt. Zeile
      /// </summary>
      public int FirstColumnHeigthUnit {
         get {
            return hu4col0 != null ? hu4col0.Value : -1;
         }
      }

      /// <summary>
      /// Spezial-hunit für akt. Zeile (für Plateau)
      /// </summary>
      public int Column0HeigthUnit {
         get {
            return hu0 != null ? hu0.Value : -1;
         }
      }

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

         hu0 = new HeightUnit0(MaxHeigth);
         hu4col0 = new HeightUnit4Column0(MaxHeigth);
         next_hu4col0 = new HeightUnit4Column0(MaxHeigth);
         hustd = new HeightUnitStd(MaxHeigth);

         Elements = new List<HeightElement>();

         plateau_TabColPos = 1;

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
         int height = Height(pos);
         HeigthUnit = 0;
         if (height >= 0) { // nur nichtnegative Höhen

            if (pos.X == 0) { // 1. Spalte
               if (StdHeigth == height) {

                  int length = PlateauLength(pos, height);
                  Elements.Add(HeightElement.CreateHeightElement_Plateau(length, pos.X, pos.Y, TileSize, plateau_TabColPos));
                  plateau_TabColPos = Elements[Elements.Count - 1].PlateauTabColPos;
                  pos.Next(length);

               } else {
                  // height != height_old, da sonst StdHeigth == height (s.o.)
                  int data = HeigthVDiff(pos);
                  if (data < 0)
                     data++;

                  StdHeigth = height;

                  hu4col0 = next_hu4col0;

                  HeigthUnit = hu4col0.Value;
                  Elements.Add(HeightElement.CreateHeightElement_Plateau(0, pos.X, pos.Y, TileSize, plateau_TabColPos));
                  Elements.Add(HeightElement.CreateHeightElement_ValueH(data, HeigthUnit, pos.X, pos.Y));
                  pos.Next();

                  next_hu4col0 = new HeightUnit4Column0(hu4col0);
                  next_hu4col0.AddCol0Value(data, height);
                  hu0.AddCol0Value(data);
               }

            } else // weitere Spalten

               if (pos.Y == 0) { // 1. Zeile

                  int data = HeigthHDiff(pos);

                  HeigthUnit = pos.X > 1 ?
                                    hustd.Value :
                                    hu4col0.Value;
                  if (HeigthUnit >= 1)
                     Elements.Add(HeightElement.CreateHeightElement_ValueH(data, HeigthUnit, pos.X, pos.Y));
                  else
                     Elements.Add(new HeightElement(HeightElement.Typ.Value, data, LengthCodingType.GetLengthCoding(pos, Elements), pos.X, pos.Y));
                  pos.Next();

                  hustd.AddValue(data);

               } else { // nächste Zeilen

                  // Vermutung für Normalfall: d(i,n) = -sgn(ddiff(i, n)) * (hdiff(i, n) – hdiff(i, n-1)))

                  int ddiff = HeigthDDiff(pos);
                  int hdiff = HeigthHDiff(pos);
                  int hdiff_old = HeigthHDiff(pos.X, pos.Y - 1);

                  if (ddiff != 0) {    // "Normalfall"
                     //       B
                     //    A  x

                     int data = -Math.Sign(ddiff) * (hdiff - hdiff_old);

                     HeigthUnit = hustd.Value;
                     if (HeigthUnit >= 1)
                        Elements.Add(HeightElement.CreateHeightElement_ValueH(data, HeigthUnit, pos.X, pos.Y));
                     else
                        Elements.Add(new HeightElement(HeightElement.Typ.Value, data, LengthCodingType.GetLengthCoding(pos, Elements), pos.X, pos.Y));
                     pos.Next();

                     hustd.AddValue(data);

                  } else {       // Die Diagonale hat konstante Höhe.

                     if (hdiff != 0) {
                        //       A
                        //    A  x

                        int data = hdiff;
                        if (data < 0)
                           data++;

                        HeigthUnit = hu0.Value;
                        Elements.Add(HeightElement.CreateHeightElement_Plateau(0, pos.X, pos.Y, TileSize, plateau_TabColPos));
                        Elements.Add(HeightElement.CreateHeightElement_ValueH(data, HeigthUnit, pos.X, pos.Y));
                        pos.Next();

                     } else {    // Spezialfall "Plateau"
                        //       A
                        //    A  A

                        // Die Daten für mehrere Höhenpos. (min. 2) und dem Nachfolgewert werden gemeinsam erzeugt!

                        int length = PlateauLength(pos, Height(pos));
                        Elements.Add(HeightElement.CreateHeightElement_Plateau(length, pos.X, pos.Y, TileSize, plateau_TabColPos));
                        plateau_TabColPos = Elements[Elements.Count - 1].PlateauTabColPos;
                        pos.Next(length);

                        int follower = -1;
                        int follower_ddiff = 0;
                        int follower_vdiff = 0;
                        if (pos.X != 0) {
                           follower = Height(pos);
                           follower_ddiff = HeigthDDiff(pos);
                           follower_vdiff = HeigthVDiff(pos);
                           pos.Next();
                        }

                        if (follower >= 0) {
                           if (follower_ddiff != 0) {
                              if (follower_ddiff > 0)
                                 follower_vdiff *= -1;
                              HeigthUnit = 1;
                              Elements.Add(HeightElement.CreateHeightElement_ValueH(follower_vdiff, HeigthUnit, pos.X, pos.Y));
                           } else {
                              HeigthUnit = hu0.Value;
                              if (follower_ddiff > 0)
                                 Elements.Add(HeightElement.CreateHeightElement_ValueH(follower_vdiff, HeigthUnit, pos.X, pos.Y));
                              else
                                 Elements.Add(HeightElement.CreateHeightElement_ValueH(follower_vdiff + 1, HeigthUnit, pos.X, pos.Y));
                           }
                        }
                     }
                  }
               }

         }
      }

      /// <summary>
      /// ermittel die Länge eines Plateaus ab der Position mit der vorgegebenen Höhe
      /// </summary>
      /// <param name="pos">Startpos.</param>
      /// <param name="value">Höhe</param>
      /// <returns></returns>
      int PlateauLength(Position pos, int value) {
         Position tst = new Position(pos);
         int length = 0;

         while (tst.Idx < HeightValues.Count) {
            if (HeightValues[tst.Idx] != value)
               break;
            length++;
            tst.Next();
         }

         return length;
      }


      #region spezielle Höhendifferenzen

      /// <summary>
      /// liefert die horizontale Höhendifferenz (zur Vorgängerhöhe)
      /// </summary>
      /// <param name="pos"></param>
      /// <returns></returns>
      int HeigthHDiff(Position pos) {
         return HeigthHDiff(pos.X, pos.Y);
      }
      /// <summary>
      /// liefert die horizontale Höhendifferenz (zur Vorgängerhöhe)
      /// </summary>
      /// <param name="col"></param>
      /// <param name="line"></param>
      /// <returns></returns>
      int HeigthHDiff(int col, int line) {
         return ValidHeight(col, line) - ValidHeight(col - 1, line);
      }
      /// <summary>
      /// liefert die vertikale Höhendifferenz (zur darüber liegenden Höhe)
      /// </summary>
      /// <param name="pos"></param>
      /// <returns></returns>
      int HeigthVDiff(Position pos) {
         return HeigthVDiff(pos.X, pos.Y);
      }
      /// <summary>
      /// liefert die vertikale Höhendifferenz (zur darüber liegenden Höhe)
      /// </summary>
      /// <param name="col"></param>
      /// <param name="line"></param>
      /// <returns></returns>
      int HeigthVDiff(int col, int line) {
         return ValidHeight(col, line) - ValidHeight(col, line - 1);
      }
      /// <summary>
      /// liefert die diagonale Höhendifferenz (der darüber liegenden Höhe zur Vorgängerhöhe)
      /// </summary>
      /// <param name="pos"></param>
      /// <returns></returns>
      int HeigthDDiff(Position pos) {
         return HeigthDDiff(pos.X, pos.Y);
      }
      /// <summary>
      /// liefert die diagonale Höhendifferenz (der darüber liegenden Höhe zur Vorgängerhöhe)
      /// </summary>
      /// <param name="col"></param>
      /// <param name="line"></param>
      /// <returns></returns>
      int HeigthDDiff(int col, int line) {
         return ValidHeight(col, line - 1) - ValidHeight(col - 1, line);
      }

      #endregion

      #region Zugriff auf Höhenwerte

      /// <summary>
      /// liefert immer eine verarbeitbare Höhe, d.h. außerhalb der <see cref="TileSize"/> immer 0
      /// </summary>
      /// <param name="column"></param>
      /// <param name="line"></param>
      /// <returns></returns>
      public int ValidHeight(int column, int line) {
         if (0 <= column && column < TileSize &&
             0 <= line && line < TileSize) {
            int h = Height(column, line);
            if (h >= 0)
               return h;
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


      static public List<byte> LengthCoding0(int data) {
         return new List<byte>(new HeightElement(HeightElement.Typ.Value, data, HeightElement.EncodeMode.Length0).Bits);
      }

      static public List<byte> LengthCoding1(int data) {
         return new List<byte>(new HeightElement(HeightElement.Typ.Value, data, HeightElement.EncodeMode.Length1).Bits);
      }

      static public List<byte> HybridCoding(int data, int hunit) {
         return new List<byte>(new HeightElement(HeightElement.Typ.Value, data, HeightElement.EncodeMode.Hybrid, 0, 0, hunit).Bits);
      }

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
