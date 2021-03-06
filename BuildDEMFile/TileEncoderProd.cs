﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
/*
CS0659: "Klasse" überschreibt Object.Equals(object o), aber nicht Object.GetHashCode() ('class' overrides Object.Equals(object o) but does not override Object.GetHashCode())
CS0660: "Klasse" definiert den Operator == oder !=, aber überschreibt Object.Equals(object o) nicht. ('class' defines operator == or operator != but does not override Object.Equals(object o))
CS0661: "Klasse" definiert den Operator == oder !=, aber überschreibt Object.GetHashCode() nicht. ('class' defines operator == or operator != but does not override Object.GetHashCode())
#pragma warning disable 659, 661
*/

namespace Encoder {
   /// <summary>
   /// produktiver TileEncoder
   /// </summary>
   class TileEncoderProd {

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

            public PlateauTableItem(int unit, int bits) {
               Unit = unit;
               Bits = bits;
            }

            public override string ToString() {
               return string.Format("Unit={0}, Bits={1}", Unit, Bits);
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
                        new PlateauTableItem(1, 0),
                        new PlateauTableItem(1, 0),
                        new PlateauTableItem(1, 0),
                        new PlateauTableItem(1, 1),
                        new PlateauTableItem(2, 1),
                        new PlateauTableItem(2, 1),
                        new PlateauTableItem(2, 1),
                        new PlateauTableItem(2, 2),
                        new PlateauTableItem(4, 2),
                        new PlateauTableItem(4, 2),
                        new PlateauTableItem(4, 2),
                        new PlateauTableItem(4, 3),
                        new PlateauTableItem(8, 3),
                        new PlateauTableItem(8, 3),
                        new PlateauTableItem(8, 3),
                        new PlateauTableItem(8, 4),
                        new PlateauTableItem(16, 4),
                        new PlateauTableItem(16, 5),
                        new PlateauTableItem(32, 5),
                        new PlateauTableItem(32, 6),
                        new PlateauTableItem(64, 6),
                        new PlateauTableItem(64, 7),
                        new PlateauTableItem(128, 8),
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

         public void ClearFailedFlags() {
            IncrementFailed = DecrementFailed = false;
         }

         public override string ToString() {
            return string.Format("Idx={0}, Item={1}", Idx, table[Math.Max(0, Math.Min(table.Length - 1, Idx))]);
         }

      }

      public class BitEncoding {
         /// <summary>
         /// max. Höhe
         /// </summary>
         readonly int maxheight;
         /// <summary>
         /// Bitstream
         /// </summary>
         BitStream bits;
         /// <summary>
         /// Grenzwerte
         /// </summary>
         ValueRanges valrange;


         public BitEncoding(int maxheight, BitStream bits, ValueRanges valrange) {
            this.maxheight = maxheight;
            this.bits = bits;
            this.valrange = valrange;
         }

         /// <summary>
         /// speichert den Wert in Hybrid-Codierung
         /// </summary>
         /// <param name="data"></param>
         /// <param name="hunit"></param>
         public void Hybrid(int data, int hunit) {
            int bitcount;
            switch (hunit) {
               case 0x0001: bitcount = 0; break;
               case 0x0002: bitcount = 1; break;
               case 0x0004: bitcount = 2; break;
               case 0x0008: bitcount = 3; break;
               case 0x0010: bitcount = 4; break;
               case 0x0020: bitcount = 5; break;
               case 0x0040: bitcount = 6; break;
               case 0x0080: bitcount = 7; break;
               case 0x0100: bitcount = 8; break;
               case 0x0200: bitcount = 9; break;
               case 0x0400: bitcount = 10; break;
               case 0x0800: bitcount = 11; break;
               case 0x1000: bitcount = 12; break;
               case 0x2000: bitcount = 13; break;
               case 0x4000: bitcount = 14; break;
               case 0x8000: bitcount = 15; break;
               // ...
               default:
                  throw new Exception(string.Format("Die Hunit {0} für die Codierung {1} ist kein 2er-Potenz.", hunit, EncodeMode.Hybrid));
            }

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

            int maxm = valrange.MaxLengthZeroBits;
            if (m <= maxm) {
               Length(m);                       // längencodierten Teil speichern
               Binary((uint)bin, bitcount);     // binär codierten Teil speichern
               bits.Add(data > 0);                    // Vorzeichen speichern
            } else
               throw new Exception(string.Format("Der Betrag des Wertes {0} ist für die Codierung {1} bei der Maximalhöhe {2} und mit Heightunit {3} zu groß.",
                                                   data,
                                                   EncodeMode.Hybrid,
                                                   maxheight,
                                                   hunit));
         }

         /// <summary>
         /// speichert den Wert in L0-Codierung
         /// </summary>
         /// <param name="data"></param>
         public void Length0(int data) {
            Length(2 * Math.Abs(data) - (Math.Sign(data) + 1) / 2);
         }

         /// <summary>
         /// speichert den Wert in L1-Codierung
         /// </summary>
         /// <param name="data"></param>
         public void Length1(int data) {
            Length(2 * Math.Abs(data - 1) + (Math.Sign(data - 1) - 1) / 2);
         }

         /// <summary>
         /// speichert den Wert in L2-Codierung
         /// </summary>
         /// <param name="data"></param>
         public void Length2(int data) {
            Length0(-data);
         }

         /// <summary>
         /// speichert den Wert in BigBin-Codierung an Stelle Hybrid oder L0
         /// </summary>
         /// <param name="data"></param>
         /// <param name="plateaufollower">für Plateaunachfolger</param>
         public void BigBin4HL0(int data, bool plateaufollower = false) {
            if (data == 0)
               throw new Exception(string.Format("Der Wert 0 kann nicht in der Codierung {0} erzeugt werden.", EncodeMode.BigBinary));

            int length0 = valrange.MaxLengthZeroBits + 1; // 1 Bit mehr als Max. als BigBin-Kennung
            if (plateaufollower)    // kann 1 kürzer sein
               length0--;
            Length(length0); // 0-Bits und 1-Bit

            int bitcount = valrange.BigBinBits;
            bool sign = data < 0;                     // Vorzeichen
            if (data > 0)
               Binary((uint)(data - 1), bitcount - 1);      // pos. Wert codieren
            else
               Binary((uint)(-data - 1), bitcount - 1);     // neg. Wert codieren
            bits.Add(sign);
         }

         /// <summary>
         /// speichert den Wert in BigBin-Codierung an Stelle L1
         /// </summary>
         /// <param name="data"></param>
         /// <param name="plateaufollower">für Plateaunachfolger</param>
         public void BigBin4L1(int data, bool plateaufollower = false) {
            BigBin4HL0(1 - data, plateaufollower);
         }

         /// <summary>
         /// speichert den Wert in BigBin-Codierung an Stelle L2 (ex. nur für Plateaufollower)
         /// </summary>
         /// <param name="data"></param>
         public void BigBin4L2(int data) {
            BigBin4HL0(-data, true);
         }

         /// <summary>
         /// erzeugt ein Plateau der vorgegebenen Länge, aber nicht über mehrere Zeilen
         /// </summary>
         /// <param name="length">Länge des Plateaus</param>
         /// <param name="startcolumn">Spalte des Startpunktes</param>
         /// <param name="startline">Zeile des Startpunktes</param>
         /// <param name="linelength">Kachelbreite</param>
         /// <param name="plateauTable4Tile"></param>
         public void Plateau(int length, int startcolumn, int startline, int linelength, PlateauTable plateauTable4Tile) {
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
                  unit = plateauTable4Tile.Unit(); // identisch zur letzte Unit vom Vorgänger-Plateau
                  length -= unit;
                  startcolumn += unit;
                  bits.Add(true);
                  plateauTable4Tile.IncrementIdx();
               }
               if (startcolumn != linelength) {        // nicht exakt mit 1-Bits gefüllt -> Plateau ohne Trennbit und Binärbits beendet
                  if (!plateauTable4Tile.IncrementFailed)
                     plateauTable4Tile.DecrementIdx(); // Plateaulänge ist abgeschlossen
                  plateauTable4Tile.ClearFailedFlags();
               } else {                                 // Plateaulänge läuft einfach weiter, Rest gilt aber für die nächste Zeile.

               }
               return;

            }

            // Standard, d.h. es folgt auf der gleichen Zeile ein Plateaufollower (es kann sich auch um die Fortsetzung der Plateaulänge von der vorhergehenden Zeile handeln)
            while ((unit = plateauTable4Tile.Unit()) <= length) {   // Die Unit ist kleiner als die restliche Länge.
               length -= unit;
               startcolumn += unit;
               bits.Add(true);
               plateauTable4Tile.IncrementIdx();  // Der Index für das nächste 1-Bit wird eingestellt.
            }
            if (!plateauTable4Tile.IncrementFailed)
               plateauTable4Tile.DecrementIdx(); // Der zuletzt verwendete Index wird um 1 verringert (jetzt identisch mit letztem 1-Bit vom akt. Plateau).
            plateauTable4Tile.ClearFailedFlags();

            // Basis abschließen (Trennbit)
            bits.Add(false);

            if (plateauTable4Tile.Bits() > 0)    // Rest binär codieren
               Binary((uint)length, plateauTable4Tile.Bits());

         }


         /// <summary>
         /// Hilfsfunktion für die Längencodierung
         /// </summary>
         /// <param name="count0bits"></param>
         void Length(int count0bits) {
            bits.Add(false, count0bits);
            bits.Add(true);
         }

         /// <summary>
         /// codiert rein binär ohne Vorzeichen (MSB zuerst)
         /// </summary>
         /// <param name="data"></param>
         /// <param name="bitcount">Bitanzahl</param>
         void Binary(uint data, int bitcount) {
            if (bitcount == 0 && data == 0)
               return;
            int t = 1 << (bitcount - 1);
            if (data >= t << 1)
               throw new Exception(string.Format("Die Zahl {0} ist zu große für die binäre Codierung mit {1} Bits.", data, bitcount));
            while (t > 0) {
               bits.Add((data & t) != 0);
               t >>= 1;
            }
         }

         public override string ToString() {
            return string.Format("maxheight={0}, bits={1}, valrange={2}", maxheight, bits.BitCount, valrange);
         }

      }

      /// <summary>
      /// liefert gültige Wertebereiche für verschiedene Codierungen
      /// </summary>
      public class ValueRanges {

         readonly int maxencoderheight;
         readonly int maxrealheight;
         readonly int shrink;

         /// <summary>
         /// liefert die max. mögliche Anzahl 0-Bits für die Hybrid- oder Längencodierung
         /// </summary>
         /// <returns></returns>
         public int MaxLengthZeroBits { get; private set; }

         /// <summary>
         /// liefert die Anzahl der binären Bits (einschließlich Vorzeichenbit) für BigBin-Zahlen (1 + int(ld(max)))
         /// </summary>
         public int BigBinBits { get; private set; }

         /// <summary>
         /// liefert den kleinsten verwendbaren Wert bei BigBin-Codierung (an Stelle von Hybridcodierung bzw. Länge-Codierung 0)
         /// </summary>
         public int MinBigBin { get; private set; }

         /// <summary>
         /// liefert den größten verwendbaren Wert bei BigBin-Codierung (an Stelle von Hybridcodierung bzw. Länge-Codierung 0)
         /// </summary>
         public int MaxBigBin { get; private set; }

         /// <summary>
         /// liefert den kleinsten verwendbaren Wert bei BigBin-Codierung (an Stelle von Länge-Codierung 1)
         /// </summary>
         public int MinBigBinL1 { get; private set; }

         /// <summary>
         /// liefert den größten verwendbaren Wert bei BigBin-Codierung (an Stelle von Länge-Codierung 1)
         /// </summary>
         public int MaxBigBinL1 { get; private set; }

         public int StdMinL0 { get; private set; }

         public int StdMaxL0 { get; private set; }

         public int StdMinL1 { get; private set; }

         public int StdMaxL1 { get; private set; }

         public int StdMinL2 { get; private set; }

         public int StdMaxL2 { get; private set; }

         public int[] StdMinH { get; private set; }

         public int[] StdMaxH { get; private set; }


         public ValueRanges(int maxencoderheight, int maxrealheight, int shrink) {
            this.maxencoderheight = maxencoderheight;
            this.maxrealheight = maxrealheight;
            this.shrink = shrink;

            InitMax0Bits();
            BigBinBits = GetBigBinBits();

            RangeBigBin(out int min, out int max);
            MinBigBin = min;
            MaxBigBin = max;

            RangeBigBinL1(out min, out max);
            MinBigBinL1 = min;
            MaxBigBinL1 = max;

            Length0(out min, out max, -1);
            StdMinL0 = min;
            StdMaxL0 = max;

            Length1(out min, out max, -1);
            StdMinL1 = min;
            StdMaxL1 = max;

            Length2(out min, out max, -1);
            StdMinL2 = min;
            StdMaxL2 = max;

            StdMinH = new int[16];
            StdMaxH = new int[StdMinH.Length];
            for (int i = 1; i <= StdMinH.Length; i++) {
               Hybrid(0x1 << (i - 1), out min, out max, -1);
               StdMinH[i - 1] = min;
               StdMaxH[i - 1] = max;
            }

         }

         /// <summary>
         /// init. die max. mögliche Anzahl 0-Bits für die Hybrid- oder Längencodierung
         /// </summary>
         /// <returns></returns>
         void InitMax0Bits() {
            if (shrink == 1) {

               if (maxencoderheight < 0x0002) MaxLengthZeroBits = 15;
               else if (maxencoderheight < 0x0004) MaxLengthZeroBits = 16;
               else if (maxencoderheight < 0x0008) MaxLengthZeroBits = 17;
               else if (maxencoderheight < 0x0010) MaxLengthZeroBits = 18;
               else if (maxencoderheight < 0x0020) MaxLengthZeroBits = 19;
               else if (maxencoderheight < 0x0040) MaxLengthZeroBits = 20;
               else if (maxencoderheight < 0x0080) MaxLengthZeroBits = 21;
               else if (maxencoderheight < 0x0100) MaxLengthZeroBits = 22;
               else if (maxencoderheight < 0x0200) MaxLengthZeroBits = 25;
               else if (maxencoderheight < 0x0400) MaxLengthZeroBits = 28;
               else if (maxencoderheight < 0x0800) MaxLengthZeroBits = 31;
               else if (maxencoderheight < 0x1000) MaxLengthZeroBits = 34;
               else if (maxencoderheight < 0x2000) MaxLengthZeroBits = 37;
               else if (maxencoderheight < 0x4000) MaxLengthZeroBits = 40;
               else MaxLengthZeroBits = 43;

            } else {
               if (maxrealheight > 0) {
                  SortedDictionary<int, int> tmp = new SortedDictionary<int, int>();

                  int s = int_ld((shrink - 1) / 2);
                  for (int i = 0; i < 16; i++) {
                     int v = 3 * (i + 1) + s;
                     int k = (int)Math.Pow(2, i);
                     tmp.Add(k, v);
                  }

                  for (int i = 1; i < 16; i++) {
                     int k = shrink * ((int)Math.Pow(2, i) - 1) + 1;
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
                     if (maxrealheight >= keys[i]) {
                        if (tmp[keys[i]] > 0)
                           MaxLengthZeroBits = tmp[keys[i]];
                        else {
                           // 1 kleiner als der Vorgänger
                           MaxLengthZeroBits = tmp[keys[i - 1]] - 1;
                        }
                        break;
                     }
                  }
               }
            }
         }

         int int_ld(int v) {
            return (int)Math.Floor(Math.Log(v) / Math.Log(2));
         }

         /// <summary>
         /// liefert die Anzahl der binären Bits (einschließlich Vorzeichenbit) für BigBin-Zahlen (1 + int(ld(max)))
         /// </summary>
         /// <returns></returns>
         int GetBigBinBits() {
            if (maxencoderheight < 0x0002)
               return 1;
            else if (maxencoderheight < 0x0004)
               return 2;
            else if (maxencoderheight < 0x0008)
               return 3;
            else if (maxencoderheight < 0x0010)
               return 4;
            else if (maxencoderheight < 0x0020)
               return 5;
            else if (maxencoderheight < 0x0040)
               return 6;
            else if (maxencoderheight < 0x0080)
               return 7;
            else if (maxencoderheight < 0x0100)
               return 8;
            else if (maxencoderheight < 0x0200)
               return 9;
            else if (maxencoderheight < 0x0400)
               return 10;
            else if (maxencoderheight < 0x0800)
               return 11;
            else if (maxencoderheight < 0x1000)
               return 12;
            else if (maxencoderheight < 0x2000)
               return 13;
            else if (maxencoderheight < 0x4000)
               return 14;
            else
               return 15;
         }

         /// <summary>
         /// liefert den kleinsten und den größten verwendbaren Wert bei BigBin-Codierung (an Stelle von Hybridcodierung bzw. Länge-Codierung 0)
         /// </summary>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="max">größter verwendbarer Wert</param>
         void RangeBigBin(out int min, out int max) {
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
         void RangeBigBinL1(out int min, out int max) {
            RangeBigBin(out min, out max);
            max++;
            min++;
         }

         /// <summary>
         /// Anzahl der möglichen 0-Bits
         /// </summary>
         /// <param name="iPlateauLengthBinBits"></param>
         /// <returns></returns>
         int LBits(int iPlateauLengthBinBits) {
            return iPlateauLengthBinBits >= 0 ?
                     MaxLengthZeroBits - iPlateauLengthBinBits - 1 :    // bei Plateaufollower weniger Bit erlaubt
                     MaxLengthZeroBits;
         }


         /// <summary>
         /// liefert den kleinsten und den größten verwendbaren Wert bei Hybridcodierung
         /// </summary>
         /// <param name="hunit"></param>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="max">größter verwendbarer Wert</param>
         /// <param name="iPlateauLengthBinBits">Anzahl der Binbits für die ev. vorausgehende Plateaulänge</param>
         void Hybrid(int hunit, out int min, out int max, int iPlateauLengthBinBits) {
            max = (LBits(iPlateauLengthBinBits) + 1) * hunit;
            min = -max + 1;
         }

         /// <summary>
         /// liefert den kleinsten und den größten verwendbaren Wert bei Länge-Codierung 0
         /// </summary>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="max">größter verwendbarer Wert</param>
         /// <param name="iPlateauLengthBinBits">Anzahl der Binbits für die ev. vorausgehende Plateaulänge</param>
         void Length0(out int min, out int max, int iPlateauLengthBinBits) {
            int lbits = LBits(iPlateauLengthBinBits);
            min = -(lbits / 2);
            max = min + lbits;
         }

         /// <summary>
         /// liefert den kleinsten und den größten verwendbaren Wert bei Länge-Codierung 1
         /// </summary>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="max">größter verwendbarer Wert</param>
         /// <param name="iPlateauLengthBinBits">Anzahl der Binbits für die ev. vorausgehende Plateaulänge</param>
         void Length1(out int min, out int max, int iPlateauLengthBinBits) {
            int lbits = LBits(iPlateauLengthBinBits);
            max = lbits / 2 + 1;
            min = max - lbits;
         }

         /// <summary>
         /// liefert den kleinsten und den größten verwendbaren Wert bei Länge-Codierung 2
         /// </summary>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="max">größter verwendbarer Wert</param>
         /// <param name="iPlateauLengthBinBits">Anzahl der Binbits für die ev. vorausgehende Plateaulänge</param>
         void Length2(out int min, out int max, int iPlateauLengthBinBits) {
            int lbits = LBits(iPlateauLengthBinBits);
            max = lbits / 2;
            min = max - lbits;
         }


         /// <summary>
         /// testet, ob der Wert im gültigen Zahlenbereich liegt
         /// </summary>
         /// <param name="hunit"></param>
         /// <param name="val"></param>
         /// <param name="iPlateauLengthBinBits"></param>
         /// <returns></returns>
         public bool Hybrid(int hunit, int val, int iPlateauLengthBinBits) {
            if (iPlateauLengthBinBits < 0) {
               int idx = 0;
               switch (hunit) {
                  case 0x0001: idx = 0; break;
                  case 0x0002: idx = 1; break;
                  case 0x0004: idx = 2; break;
                  case 0x0008: idx = 3; break;
                  case 0x0010: idx = 4; break;
                  case 0x0020: idx = 5; break;
                  case 0x0040: idx = 6; break;
                  case 0x0080: idx = 7; break;
                  case 0x0100: idx = 8; break;
                  case 0x0200: idx = 9; break;
                  case 0x0400: idx = 10; break;
                  case 0x0800: idx = 11; break;
                  case 0x1000: idx = 12; break;
                  case 0x2000: idx = 13; break;
                  case 0x4000: idx = 14; break;
                  case 0x8000: idx = 15; break;
               }
               return StdMinH[idx] <= val && val <= StdMaxH[idx];
            } else {
               int max = (LBits(iPlateauLengthBinBits) + 1) * hunit;
               int min = -max + 1;
               return min <= val && val <= max;
            }
         }

         /// <summary>
         /// testet, ob der Wert im gültigen Zahlenbereich liegt
         /// </summary>
         /// <param name="val"></param>
         /// <param name="iPlateauLengthBinBits"></param>
         /// <returns></returns>
         public bool Length0(int val, int iPlateauLengthBinBits) {
            if (iPlateauLengthBinBits < 0)
               return StdMinL0 <= val && val <= StdMaxL0;
            else {
               int lbits = LBits(iPlateauLengthBinBits);
               int min = -(lbits / 2);
               int max = min + lbits;
               return min <= val && val <= max;
            }
         }

         /// <summary>
         /// testet, ob der Wert im gültigen Zahlenbereich liegt
         /// </summary>
         /// <param name="val"></param>
         /// <param name="iPlateauLengthBinBits"></param>
         /// <returns></returns>
         public bool Length1(int val, int iPlateauLengthBinBits) {
            if (iPlateauLengthBinBits < 0)
               return StdMinL1 <= val && val <= StdMaxL1;
            else {
               int lbits = LBits(iPlateauLengthBinBits);
               int max = lbits / 2 + 1;
               int min = max - lbits;
               return min <= val && val <= max;
            }
         }

         /// <summary>
         /// testet, ob der Wert im gültigen Zahlenbereich liegt
         /// </summary>
         /// <param name="val"></param>
         /// <param name="iPlateauLengthBinBits"></param>
         /// <returns></returns>
         public bool Length2(int val, int iPlateauLengthBinBits) {
            if (iPlateauLengthBinBits < 0)
               return StdMinL2 <= val && val <= StdMaxL2;
            else {
               int lbits = LBits(iPlateauLengthBinBits);
               int max = lbits / 2;
               int min = max - lbits;
               return min <= val && val <= max;
            }
         }

         /// <summary>
         /// testet, ob der Wert im gültigen Zahlenbereich liegt
         /// </summary>
         /// <param name="val"></param>
         /// <returns></returns>
         public bool BigBin(int val) {
            return MinBigBin <= val && val <= MaxBigBin;
         }

         /// <summary>
         /// testet, ob der Wert im gültigen Zahlenbereich liegt
         /// </summary>
         /// <param name="val"></param>
         /// <returns></returns>
         public bool BigBinL1(int val) {
            return MinBigBinL1 <= val && val <= MaxBigBinL1;
         }


         public override string ToString() {
            return string.Format("maxheight={0}, MinBigBin={1}, MaxBigBin={2}", maxencoderheight, MinBigBin, MaxBigBin);
         }

      }

      /// <summary>
      ///Wrapping von Werten und das Anpassen des Encodiermodus
      /// </summary>
      class ValueFitting {

         /// <summary>
         /// oberer Grenzwert für <see cref="BitEncoding.EncodeMode.Length0"/> der für das Wrapping überschritten werden muss
         /// </summary>
         readonly int L0_wrapdown;
         /// <summary>
         /// unterer Grenzwert für <see cref="BitEncoding.EncodeMode.Length0"/> der für das Wrapping unterschritten werden muss
         /// </summary>
         readonly int L0_wrapup;

         /// <summary>
         /// oberer Grenzwert für <see cref="BitEncoding.EncodeMode.Length1"/> der für das Wrapping überschritten werden muss
         /// </summary>
         readonly int L1_wrapdown;
         /// <summary>
         /// unterer Grenzwert für <see cref="BitEncoding.EncodeMode.Length1"/> der für das Wrapping unterschritten werden muss
         /// </summary>
         readonly int L1_wrapup;

         /// <summary>
         /// oberer Grenzwert für <see cref="BitEncoding.EncodeMode.Length2"/> der für das Wrapping überschritten werden muss
         /// </summary>
         readonly int L2_wrapdown;
         /// <summary>
         /// unterer Grenzwert für <see cref="BitEncoding.EncodeMode.Length2"/> der für das Wrapping unterschritten werden muss
         /// </summary>
         readonly int L2_wrapup;

         /// <summary>
         /// oberer Grenzwert für <see cref="BitEncoding.EncodeMode.Hybrid"/> der für ein eventuelles Wrapping mindestens überschritten werden muss
         /// </summary>
         readonly int H_wrapdown;
         /// <summary>
         /// unterer Grenzwert für <see cref="BitEncoding.EncodeMode.Hybrid"/> der für ein eventuelles Wrapping mindestens unterschritten werden muss
         /// </summary>
         readonly int H_wrapup;

         /// <summary>
         /// Maximalhöhe der Kachel
         /// </summary>
         int max;

         /// <summary>
         /// Grenzwerte
         /// </summary>
         ValueRanges vr;


         public ValueFitting(int maxheight, ValueRanges valuerange) {
            vr = valuerange;

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
         }

         public int Fit(int data, ref EncodeMode em, int hunit, int iPlateauLengthBinBits) {
            int datawrapped = int.MinValue; // Kennung für "nicht gewrapt"
            bool changed2BigBin = false;

            switch (em) {
               case EncodeMode.Hybrid:
                  // ev. wrap nötig
                  if (data > H_wrapdown)
                     datawrapped = WrapDown(data);
                  else if (data < H_wrapup)
                     datawrapped = WrapUp(data);

                  // ev. Zahlenbereichsüberschreitung -> BigBin
                  if (!vr.Hybrid(hunit, datawrapped != int.MinValue ? datawrapped : data, iPlateauLengthBinBits)) {
                     em = EncodeMode.BigBinary;
                     changed2BigBin = true;
                  }
                  break;

               case EncodeMode.Length0:
                  if (data > L0_wrapdown)
                     datawrapped = WrapDown(data);
                  else if (data < L0_wrapup)
                     datawrapped = WrapUp(data);

                  if (!vr.Length0(datawrapped != int.MinValue ? datawrapped : data, iPlateauLengthBinBits)) {
                     em = EncodeMode.BigBinary;
                     changed2BigBin = true;
                  }
                  break;

               case EncodeMode.Length1:
                  if (data > L1_wrapdown)
                     datawrapped = WrapDown(data);
                  else if (data < L1_wrapup)
                     datawrapped = WrapUp(data);

                  if (!vr.Length1(datawrapped != int.MinValue ? datawrapped : data, iPlateauLengthBinBits)) {
                     em = EncodeMode.BigBinaryL1;
                     changed2BigBin = true;
                  }
                  break;

               case EncodeMode.Length2:
                  if (data > L2_wrapdown)
                     datawrapped = WrapDown(data);
                  else if (data < L2_wrapup)
                     datawrapped = WrapUp(data);

                  if (!vr.Length2(datawrapped != int.MinValue ? datawrapped : data, iPlateauLengthBinBits)) {
                     em = EncodeMode.BigBinaryL2;
                     changed2BigBin = true;
                  }
                  break;
            }

            if (!changed2BigBin) // sonst sollte BigBin immer i.O. sein
               switch (em) {
                  case EncodeMode.BigBinary:
                  case EncodeMode.BigBinaryL2:
                     if (datawrapped != int.MinValue) {
                        if (!vr.BigBin(datawrapped)) {
                           throw new Exception("Der Wert " + datawrapped.ToString() + " kann bei Max " + max.ToString() + " nicht als BigBinary codiert werden."); // sollte nicht vorkommen
                        }
                     } else {
                        if (!vr.BigBin(data)) {
                           if (data > vr.MaxBigBin)
                              datawrapped = WrapDown(data);
                           else if (data < vr.MinBigBin)
                              datawrapped = WrapUp(data);
                        }
                     }
                     break;

                  case EncodeMode.BigBinaryL1:
                     if (datawrapped != int.MinValue) {
                        if (!vr.BigBinL1(datawrapped)) {
                           throw new Exception("Der Wert " + datawrapped.ToString() + " kann bei Max " + max.ToString() + " nicht als BigBinaryL1 codiert werden."); // sollte nicht vorkommen
                        }
                     } else {
                        if (!vr.BigBinL1(data)) {
                           if (data > vr.MaxBigBinL1)
                              datawrapped = WrapDown(data);
                           else if (data < vr.MinBigBinL1)
                              datawrapped = WrapUp(data);
                        }
                     }
                     break;

               }

            return datawrapped != int.MinValue ? datawrapped : data;
         }

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

      class CodingTypeStd {

         /// <summary>
         /// akt. Codierungsart
         /// </summary>
         public EncodeMode EncodeMode { get; protected set; }

         /// <summary>
         /// liefert den Wert (immer einer 2er-Potenz)
         /// </summary>
         public int HunitValue { get; protected set; }

         /// <summary>
         /// akt. Summe für die Hybridcodierung
         /// </summary>
         protected int SumH;

         /// <summary>
         /// akt. Summe der Bewertungen für die Art der Längencodierung
         /// </summary>
         protected int SumL;

         /// <summary>
         /// akt. Anzahl Elemente
         /// </summary>
         protected int ElemCount;

         /// <summary>
         /// "bewerteter" Wert aus dem Datenwert für die Längencodierung
         /// </summary>
         protected int Eval;

         /// <summary>
         /// max. Höhendifferenz
         /// </summary>
         protected int MaxheightDiff;

         /// <summary>
         /// aus maxheightdiff resultierendes Delta
         /// </summary>
         protected int UnitDelta;

         protected bool WhithoutL1;


         /// <summary>
         /// bildet <see cref="CodingTypeStd"/> für die Hybridcodierung
         /// </summary>
         /// <param name="maxheightdiff">max. Höhendiff.</param>
         /// <param name="whithoutL1">wenn true, dann ohne L1-Codierung</param>
         public CodingTypeStd(int maxheightdiff, bool whithoutL1) {
            if (maxheightdiff < 0)
               throw new Exception("Der Wert von maxHeightdiff kann nicht kleiner 0 sein.");

            EncodeMode = EncodeMode.Hybrid;
            MaxheightDiff = maxheightdiff;
            WhithoutL1 = whithoutL1;
            HunitValue = GetHunit4MaxHeight(maxheightdiff);
            UnitDelta = GetHunitDelta(maxheightdiff);
            ElemCount = 0;
            SumH = 0;
            SumL = 0;
         }

         public virtual void AddValue(int data) {

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
                     }
                     break;
                  case 1:
                     data++;
                     if ((SumL1 && !datagerade) ||
                         (!SumL1 && datagerade)) {
                        data++;
                     }
                     break;
                  case 3:
                     if ((SumL1 && datagerade) ||
                         (!SumL1 && !datagerade)) {
                        data--;
                     }
                     break;
               }
            }
            Eval = EvaluateData(SumL, ElemCount, data, ref evalregion);

            SumH += dh;
            if (SumH + UnitDelta + 1 >= 0xFFFF)
               SumH -= 0x10000;

            SumL += Eval;

            // ---- ElemCount aktualisieren ----
            ElemCount++;

            // ---- Korrektur der Werte bei großem ElemCount ----
            if (ElemCount == 64) {
               ElemCount = 32;

               SumH = ((SumH - UnitDelta) >> 1) - 1;

               SumL /= 2;
            }

            // ---- Hunit ermitteln ----
            SetHunit4SumAndElemcount(SumH, ElemCount, true);

            // ---- EncodeMode neu setzen ----
            if (HunitValue > 0)
               EncodeMode = EncodeMode.Hybrid;
            else
               EncodeMode = SumL > 0 && !WhithoutL1 ? EncodeMode.Length1 : EncodeMode.Length0;

         }


         /// <summary>
         /// setzt die <see cref="HeightUnit"/> neu
         /// </summary>
         /// <param name="abssum">bisherige Summe</param>
         /// <param name="elemcount">Anzahl der Elemente</param>
         /// <param name="bStd">Standardverfahren (sonst für Plateau)</param>
         protected void SetHunit4SumAndElemcount(int abssum, int elemcount, bool bStd) {
            int counter = abssum + UnitDelta;
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
         protected int GetHunit4MaxHeight(int maxheight) {
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
         protected int GetHunitDelta(int maxheight) {
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
         /// setzt <see cref="CodingTypeStd"/> für die Hybridcodierung auf die größte 2er-Potenz, die nicht größer als <see cref="value"/> ist
         /// </summary>
         /// <param name="hunitvalue"></param>
         protected void SetHunitValue(int hunitvalue) {
            if (hunitvalue < 1)
               HunitValue = 0;
            else if (hunitvalue < 2)
               HunitValue = 1;
            else if (hunitvalue < 4)
               HunitValue = 1 << 1;
            else if (hunitvalue < 8)
               HunitValue = 1 << 2;
            else if (hunitvalue < 16)
               HunitValue = 1 << 3;
            else if (hunitvalue < 32)
               HunitValue = 1 << 4;
            else if (hunitvalue < 64)
               HunitValue = 1 << 5;
            else if (hunitvalue < 128)
               HunitValue = 1 << 6;
            else if (hunitvalue < 256)
               HunitValue = 1 << 7;
            else if (hunitvalue < 512)
               HunitValue = 1 << 8;
            else if (hunitvalue < 1024)
               HunitValue = 1 << 9;
            else if (hunitvalue < 2048)
               HunitValue = 1 << 10;
            else if (hunitvalue < 4096)
               HunitValue = 1 << 11;
            else if (hunitvalue < 8192)
               HunitValue = 1 << 12;
            else if (hunitvalue < 16384)
               HunitValue = 1 << 13;
            else if (hunitvalue < 32768)
               HunitValue = 1 << 14;
            else if (hunitvalue < 65536)
               HunitValue = 1 << 15;
            else
               HunitValue = 1 << 16;
         }

         public override string ToString() {
            StringBuilder sb = new StringBuilder(EncodeMode.ToString());
            sb.Append(": ElemCount=");
            sb.Append(ElemCount.ToString());
            if (EncodeMode == EncodeMode.Hybrid) {
               sb.Append(", HunitValue=");
               sb.Append(HunitValue.ToString());
               sb.Append(", SumH=");
               sb.Append(SumH.ToString());
            } else {
               sb.Append(", SumL=");
               sb.Append(SumL.ToString());
            }
            return sb.ToString();
         }

      }

      class CodingTypePlateauFollowerNotZero : CodingTypeStd {

         /// <summary>
         /// bildet <see cref="CodingTypeStd"/> für die Codierung der Plateaufollower mit ddiff ungleich 0
         /// </summary>
         /// <param name="maxheightdiff">max. Höhendiff.</param>
         public CodingTypePlateauFollowerNotZero(int maxheightdiff) : base(maxheightdiff, false) { }

         override public void AddValue(int data) {

            // ---- SumH aktualisieren ----
            SumH += Math.Abs(data);
            if (SumH + UnitDelta + 1 >= 0xFFFF)
               SumH -= 0x10000;

            // ---- ElemCount aktualisieren ----
            ElemCount++;

            // ---- SumL aktualisieren ----
            Eval = data > 0 ? 1 : -1;
            SumL += Eval;

            // ---- Korrektur der Werte bei großem ElemCount ----
            if (ElemCount == 64) {
               ElemCount = 32;

               SumH = ((SumH - UnitDelta) >> 1) - 1;

               SumL /= 2;
               if (SumL % 2 != 0) {
                  SumL--;
               }
            }

            // ---- Hunit ermitteln ----
            SetHunit4SumAndElemcount(SumH, ElemCount, true);

            // ---- EncodeMode neu setzen ----
            if (HunitValue > 0)
               EncodeMode = EncodeMode.Hybrid;
            else
               EncodeMode = SumL > 0 ? EncodeMode.Length0 : EncodeMode.Length2;
         }

      }

      class CodingTypePlateauFollowerZero : CodingTypeStd {

         /// <summary>
         /// bildet <see cref="CodingTypeStd"/> für die Codierung der Plateaufollower mit ddiff=0
         /// </summary>
         /// <param name="maxheighdiff">max. Höhendiff.</param>
         public CodingTypePlateauFollowerZero(int maxheightdiff) : base(maxheightdiff, false) { }

         override public void AddValue(int data) {

            // ---- SumH aktualisieren ----
            if (data > 0)
               SumH += data;
            else
               SumH += 1 - data;
            if (SumH + UnitDelta + 1 >= 0xFFFF)
               SumH -= 0x10000;

            Eval = data <= 0 ? -1 : 1;
            SumL += Eval;

            // ---- ElemCount aktualisieren ----
            ElemCount++;

            // ---- Korrektur der Werte bei großem ElemCount ----
            if (ElemCount == 64) {
               ElemCount = 32;

               SumH = ((SumH - UnitDelta) >> 1) - 1;

               SumL /= 2;
               if (SumL % 2 != 0) {
                  SumL++;
               }
            }

            // ---- Hunit ermitteln ----
            SetHunit4SumAndElemcount(SumH, ElemCount, false);

            // ---- EncodeMode neu setzen ----
            if (HunitValue > 0)
               EncodeMode = EncodeMode.Hybrid;
            else
               EncodeMode = SumL < 0 ? EncodeMode.Length0 : EncodeMode.Length1;

         }

      }

      public class BitStream {

         /// <summary>
         /// der eigentliche Bitstream
         /// </summary>
         public MemoryStream Stream { get; private set; }

         byte actbyte;
         int bitpos;

         /// <summary>
         /// Anzahl der registrierten Bits
         /// </summary>
         public int BitCount { get { return (int)Stream.Position * 8 + bitpos; } }


         public BitStream() {
            Stream = new MemoryStream(10240);
            actbyte = 0;
            bitpos = 0;
         }

         /// <summary>
         /// fügt ein 1- oder 0-Bit hinzu
         /// </summary>
         /// <param name="one"></param>
         public void Add(bool one) {
            if (one) {
               actbyte |= (byte)(1 << (7 - bitpos));
            }
            bitpos++;
            if (bitpos > 7) {
               bitpos = 0;
               Stream.WriteByte(actbyte);
               actbyte = 0;
            }
         }

         /// <summary>
         /// fügt mehrere 1- oder 0-Bits hinzu
         /// </summary>
         /// <param name="one"></param>
         /// <param name="count"></param>
         public void Add(bool one, int count) {
            while (count-- > 0)
               Add(one);
         }

         /// <summary>
         /// liefert alle Bits; das letzte Byte wird mit 1-Bits aufgefüllt
         /// </summary>
         /// <returns></returns>
         public byte[] GetBytes() {
            while (bitpos != 0)
               Add(true); // mit 1-Bits füllen
            return Stream.ToArray();
         }

      }

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
         /// Längencodierung Variante 2
         /// </summary>
         Length2,
         /// <summary>
         /// Spezialcodierung für Plateaulänge
         /// </summary>
         Plateau,
         /// <summary>
         /// Codierung im "festen" Binärformat (für große Zahlen an Stelle der <see cref="Hybrid"/> oder <see cref="Length0"/>-Codierung)
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
      /// max. zulässige Höhe der Kachel
      /// </summary>
      public int MaxHeight { get; protected set; }

      /// <summary>
      /// Kachelbreite
      /// </summary>
      public int Width { get; protected set; }

      /// <summary>
      /// Kachelhöhe
      /// </summary>
      public int Height { get; protected set; }

      /// <summary>
      /// die für alle <see cref="BitEncoding"/> der Kachel gültige Tabelle
      /// </summary>
      PlateauTable PlateauTable4Tile = new PlateauTable();

      /// <summary>
      /// zur Bestimmung der Heightunit für die Gruppe der Standardwerte
      /// </summary>
      CodingTypeStd ct_std;

      /// <summary>
      /// zur Bestimmung der Heightunit für die Gruppe der Plateau-Nachfolger mit ddiff=0
      /// </summary>
      CodingTypePlateauFollowerZero ct_pf_0;

      /// <summary>
      /// zur Bestimmung der Heightunit für die Gruppe der Plateau-Nachfolger mit ddiff!=0
      /// </summary>
      CodingTypePlateauFollowerNotZero ct_pf_not0;

      /// <summary>
      /// für das Wrapping
      /// </summary>
      ValueFitting ValueWrap;

      /// <summary>
      /// für die verschiedenen Encodiermethoden
      /// </summary>
      BitEncoding BitEnc;

      /// <summary>
      /// alle Höhenwerte
      /// </summary>
      readonly List<int> HeightValues;

      /// <summary>
      /// codierter Bitstream
      /// </summary>
      BitStream bits;

      /// <summary>
      /// Encodierung für Kachel mit verkleinerten Werten
      /// </summary>
      readonly int shrink;

      ValueRanges valrange;



      /// <summary>
      /// erzeugt einen Encoder für eine Kachel
      /// <para>Mit dem wiederholten Aufruf von <see cref="ComputeNext"/> werden die gelieferten Daten schrittweise codiert. Wenn diese Funktion 0 liefert, ist
      /// die Codierung abgeschlossen. Die fertige Bytefolge knn mit <see cref="GetCodedBytes"/> abgerufen werden.
      /// </para>
      /// </summary>
      /// <param name="maxheight">max. Höhe</param>
      /// <param name="shrink"></param>
      /// <param name="width">Breite der Kachel</param>
      /// <param name="height">Höhe der Kachel</param>
      /// <param name="normalizedheight">Liste der Höhendaten (Anzahl normalerweise <see cref="tilesize"/> * <see cref="tilesize"/>)</param>
      public TileEncoderProd(int maxheight, int shrink, int width, int height, IList<int> normalizedheight) {
         MaxHeight = maxheight;
         this.shrink = shrink;
         Width = width;
         Height = height;

         HeightValues = new List<int>(normalizedheight);    // Kopie (eigentlich nicht nötig)
      }

      public byte[] Encode() {
         bits = new BitStream();

         int maxrealheight = MaxHeight;
         MaxHeight = MaxHeight / shrink + (MaxHeight % shrink > 0 ? 1 : 0);

         valrange = new ValueRanges(MaxHeight, maxrealheight, shrink);
         BitEnc = new BitEncoding(MaxHeight, bits, valrange);

         ct_std = new CodingTypeStd(MaxHeight, shrink > 1);
         ct_pf_0 = new CodingTypePlateauFollowerZero(MaxHeight);
         ct_pf_not0 = new CodingTypePlateauFollowerNotZero(MaxHeight);

         ValueWrap = new ValueFitting(MaxHeight, valrange);

         int col = 0;
         int row = 0;

         do {
            int heightupper = ValidHeight(col, row - 1);
            int heightleft = ValidHeight(col - 1, row);

            try {

               if (heightupper == heightleft) { // die Diagonale hat konstante Höhe (gilt auch für die 1. Spalte) -> immer Plateau (ev. auch mit Länge 0)

                  int length = GetPlateauLength(col, row);
                  // akt. Pos ist am Anfang des Plateaus bzw., bei Länge 0, schon auf dem Follower; also zeigt col+length auf die Pos. des Followers

                  // Plateaulänge codieren
                  BitEnc.Plateau(length, col, row, Width, PlateauTable4Tile);

                  bool bLineFilled = col + length >= Width;
                  IncrementPosition(ref col, ref row, length);

                  if (!bLineFilled &&
                      !PositionIsBehindEnd(col, row)) {
                     WritePlateauFollower(col, row);
                     IncrementPosition(ref col, ref row);
                  }

               } else {

                  int data = 0;

                  int actualheight = ValidHeight(col, row);
                  int hdiff_up = heightupper - ValidHeight(col - 1, row - 1);       // horiz. Diff. der Höhe in der Zeile "über" der akt. Pos.
                  int sgnddiff = Math.Sign(heightupper - heightleft);

                  if (hdiff_up >= MaxHeight - heightleft) {

                     data = -sgnddiff * (actualheight + 1);

                  } else if (hdiff_up <= -heightleft) {

                     data = -sgnddiff * actualheight;

                  } else {

                     data = -sgnddiff * (actualheight - heightleft - hdiff_up);

                  }

                  EncodeMode em = ct_std.EncodeMode; // wird ev. verändert auf BigBin
                  data = ValueWrap.Fit(data, ref em, ct_std.HunitValue, -1);

                  EncodeValue(data, em, ct_std);

                  IncrementPosition(ref col, ref row);
               }

            } catch (Exception ex) {
               throw new Exception(string.Format("interner Fehler bei Position [{0},{1}], Höhe {2}: {3}", col, row, ValidHeight(col, row), ex.Message));
            }
         } while (!PositionIsBehindEnd(col, row));

         byte[] ret = bits.GetBytes();
         bits.Stream.Dispose();
         return ret;
      }


      void WritePlateauFollower(int col, int row) {
         int follower = ValidHeight(col, row);
         int followerupper = ValidHeight(col, row - 1);
         int followerleft = ValidHeight(col - 1, row);

         int follower_ddiff = col == 0 ?
                                 0 : // wegen virt. Spalte
                                 followerupper - followerleft;
         int follower_vdiff = follower - followerupper;

         CodingTypeStd ct = follower_ddiff != 0 ? ct = ct_pf_not0 : ct = ct_pf_0;
         EncodeMode em = ct.EncodeMode;

         // Nachfolger codieren
         if (follower_ddiff != 0) {

            if (follower_ddiff > 0)
               follower_vdiff = -follower_vdiff;

            follower_vdiff = ValueWrap.Fit(follower_vdiff, ref em, ct.HunitValue, PlateauTable4Tile.Bits());

         } else {

            follower_vdiff = ValueWrap.Fit(follower_vdiff, ref em, ct.HunitValue, PlateauTable4Tile.Bits());
            if (follower_vdiff < 0)
               follower_vdiff++;

         }

         EncodeValue(follower_vdiff, em, ct, true);
      }

      /// <summary>
      /// encodiert den Wert im gewünschten Modus und registriert in ihn
      /// </summary>
      /// <param name="data"></param>
      /// <param name="em"></param>
      /// <param name="ct"></param>
      /// <param name="plateaufollower"></param>
      void EncodeValue(int data, EncodeMode em, CodingTypeStd ct, bool plateaufollower = false) {
         switch (em) {
            case EncodeMode.Hybrid:
               BitEnc.Hybrid(data, ct.HunitValue);
               break;

            case EncodeMode.Length0:
               BitEnc.Length0(data);
               break;

            case EncodeMode.Length1:
               BitEnc.Length1(data);
               break;

            case EncodeMode.Length2:
               BitEnc.Length2(data);
               break;

            case EncodeMode.BigBinary:
               BitEnc.BigBin4HL0(data, plateaufollower);
               break;

            case EncodeMode.BigBinaryL1:
               BitEnc.BigBin4L1(data, plateaufollower);
               break;

            case EncodeMode.BigBinaryL2:
               BitEnc.BigBin4L2(data);
               break;
         }
         ct.AddValue(data);
      }

      /// <summary>
      /// inkrementiert die akt. Position
      /// </summary>
      /// <param name="col"></param>
      /// <param name="row"></param>
      /// <param name="length"></param>
      void IncrementPosition(ref int col, ref int row, int length = 1) {
         if (length > 0) {
            col += length;
            while (col >= Width) {
               row++;
               col -= Width;
            }
         }
      }

      /// <summary>
      /// Liegt die Pos. hinter dem Ende des Datenarrays?
      /// </summary>
      /// <param name="col"></param>
      /// <param name="row"></param>
      /// <returns></returns>
      bool PositionIsBehindEnd(int col, int row) {
         //return col + row * Width >= Width * Height;
         return col >= Width * (Height - row);
      }

      /// <summary>
      /// ermittelt die Länge eines Plateaus ab der Startposition (ev. auch 0)
      /// </summary>
      /// <param name="col"></param>
      /// <param name="row"></param>
      /// <returns></returns>
      int GetPlateauLength(int col, int row) {
         int length = 0;
         int value = ValidHeight(col - 1, row);
         while (col + length < Width) {
            if (value == ValidHeight(col + length, row)) {
               length++;
            } else
               break;

         }
         return length;
      }

      /// <summary>
      /// liefert auch für ungültige Spalten und Zeilen eine verarbeitbare Höhe, d.h. außerhalb von <see cref="Width"/> bzw. <see cref="Height"/> immer 0 
      /// bzw. bei Spalte -1 die virtuelle Spalte (Spalte 0 der Vorgängerzeile)
      /// </summary>
      /// <param name="column"></param>
      /// <param name="line"></param>
      /// <returns></returns>
      int ValidHeight(int column, int line) {
         if (line < 0)     // virtuelle Zeile -> immer 0
            return 0;
         if (column < 0)   // virtuelle Spalte
            return line > 0 ? HeightValues[Width * (line - 1)] : 0;
         return HeightValues[Width * line + column];
      }


      public override string ToString() {
         return string.Format("MaxHeight={0}, TileSize={1}x{2}",
                              MaxHeight,
                              Width,
                              Height);


      }

   }
}

