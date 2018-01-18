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
      /// die für alle <see cref="BitEncoding"/> der Kachel gültige Tabelle
      /// </summary>
      public PlateauTable PlateauTable4Tile = new PlateauTable();



      public class BitEncoding {

         int maxheight;

         BitStream bits;

         ValueRange valrange;


         public BitEncoding(int maxheight, BitStream bits) {
            this.maxheight = maxheight;
            this.bits = bits;
            valrange = new ValueRange(maxheight);
         }


         /// <summary>
         /// codiert hybrid
         /// </summary>
         /// <param name="data"></param>
         /// <param name="hunit"></param>
         public void EncodeHybrid(int data, int hunit) {
            int hunitexp = HunitExponent(hunit);
            if (hunitexp < 0)
               throw new Exception(string.Format("Die Hunit {0} für die Codierung {1} ist kein 2er-Potenz.", hunit, EncodeMode.Hybrid));

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
               EncodeLength(m);                       // längencodierten Teil speichern
               EncodeBinary((uint)bin, hunitexp);     // binär codierten Teil speichern
               bits.Add(data > 0);                    // Vorzeichen speichern
            } else
               throw new Exception(string.Format("Der Betrag des Wertes {0} ist für die Codierung {1} bei der Maximalhöhe {2} und mit Heightunit {3} zu groß.",
                                                   data,
                                                   EncodeMode.Hybrid,
                                                   maxheight,
                                                   hunit));
         }

         /// <summary>
         /// speichert den Wert in C0-Codierung
         /// </summary>
         /// <param name="data"></param>
         public void EncodeLength0(int data) {
            EncodeLength(2 * Math.Abs(data) - (Math.Sign(data) + 1) / 2);
         }

         /// <summary>
         /// speichert den Wert in C1-Codierung
         /// </summary>
         /// <param name="data"></param>
         public void EncodeLength1(int data) {
            EncodeLength(2 * Math.Abs(data - 1) + (Math.Sign(data - 1) - 1) / 2);
         }

         /// <summary>
         /// codiert eine "große" Zahl binär mit führender 0-Bitfolge
         /// </summary>
         /// <param name="data"></param>
         /// <param name="follower">für Plateaunachfolger</param>
         public void EncodeBigBin(int data, EncodeMode mode) {
            if (data == 0)
               throw new Exception(string.Format("Der Wert 0 kann nicht in der Codierung {0} erzeugt werden.", EncodeMode.BigBinary));

            int length0 = valrange.MaxLengthZeroBits + 1; // 1 Bit mehr als Max. als BigBin-Kennung
            if (mode == EncodeMode.BigBinaryPF ||
                mode == EncodeMode.BigBinaryPFL1 ||
                mode == EncodeMode.BigBinaryPFL2)
               length0--;
            EncodeLength(length0); // 0-Bits und 1-Bit

            if (mode == EncodeMode.BigBinaryL1 ||
                mode == EncodeMode.BigBinaryPFL1) {
               data = 1 - data; // Umwandlung, um die gleiche Codierfunktion verwendet zu können
            }

            if (data < valrange.MinBigBin || valrange.MaxBigBin < data) {  // Wrap
               if (data > 0)
                  data -= maxheight + 1;
               else
                  data += maxheight + 1;
            }

            int bitcount = valrange.BigBinBits;
            byte sign = (byte)(data > 0 ? 0 : 1); // Vorzeichen (1 für <0)
            data = Math.Abs(data) - 1;
            EncodeBinary((uint)data, bitcount - 1); // pos. Wert codieren
            bits.Add(sign != 0);
         }

         /// <summary>
         /// erzeugt ein Plateau der vorgegebenen Länge, aber nicht über mehrere Zeilen
         /// </summary>
         /// <param name="length">Länge des Plateaus</param>
         /// <param name="startcolumn">Spalte des Startpunktes</param>
         /// <param name="startline">Zeile des Startpunktes</param>
         /// <param name="linelength">Kachelbreite</param>
         /// <param name="plateauTable4Tile"></param>
         public void EncodePlateau(int length, int startcolumn, int startline, int linelength, PlateauTable plateauTable4Tile) {
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
               EncodeBinary((uint)length, plateauTable4Tile.Bits());

         }

         /// <summary>
         /// Hilfsfunktion für die Längencodierung
         /// </summary>
         /// <param name="count0bits"></param>
         void EncodeLength(int count0bits) {
            bits.Add(false, count0bits);
            bits.Add(true);
         }

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
               bits.Add((data & t) != 0);
               t >>= 1;
            }
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

      }

      /// <summary>
      /// liefert gültige Wertebereiche für verschiedene Codierungen
      /// </summary>
      class ValueRange {

         int maxheight;

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


         public ValueRange(int maxheight) {
            this.maxheight = maxheight;
            MaxLengthZeroBits = GetMaxLengthZeroBits();
            BigBinBits = GetBigBinBits();

            int min, max;

            RangeBigBin(out min, out max);
            MinBigBin = min;
            MaxBigBin = max;

            RangeBigBinL1(out min, out max);
            MinBigBinL1 = min;
            MaxBigBinL1 = max;

         }

         /// <summary>
         /// liefert die max. mögliche Anzahl 0-Bits für die Hybrid- oder Längencodierung
         /// </summary>
         /// <returns></returns>
         int GetMaxLengthZeroBits() {
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
         /// <returns></returns>
         int GetBigBinBits() {
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
         /// liefert den kleinsten und den größten verwendbaren Wert bei Hybridcodierung
         /// </summary>
         /// <param name="hunit"></param>
         /// <param name="min">kleinster verwendbarer Wert</param>
         /// <param name="max">größter verwendbarer Wert</param>
         /// <param name="iPlateauLengthBinBits">Anzahl der Binbits für die ev. vorausgehende Plateaulänge</param>
         public void Hybrid(int hunit, out int min, out int max, int iPlateauLengthBinBits) {
            int lbits = MaxLengthZeroBits;
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
         public void Length0(out int min, out int max, int iPlateauLengthBinBits) {
            int lbits = MaxLengthZeroBits;
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
         public void Length1(out int min, out int max, int iPlateauLengthBinBits) {
            int lbits = MaxLengthZeroBits;
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
         public void Length2(out int min, out int max, int iPlateauLengthBinBits) {
            int lbits = MaxLengthZeroBits;
            if (iPlateauLengthBinBits >= 0) // bei Plateaufollower weniger Bit erlaubt
               lbits -= iPlateauLengthBinBits + 1;
            max = lbits / 2;
            min = max - lbits;
         }

      }



      /// <summary>
      /// managed das Wrapping von Werten
      /// </summary>
      class Wraparound {

         /// <summary>
         /// oberer Grenzwert für <see cref="BitEncoding.EncodeMode.Length0"/> der für das Wrapping überschritten werden muss
         /// </summary>
         int L0_wrapdown;
         /// <summary>
         /// unterer Grenzwert für <see cref="BitEncoding.EncodeMode.Length0"/> der für das Wrapping unterschritten werden muss
         /// </summary>
         int L0_wrapup;

         /// <summary>
         /// oberer Grenzwert für <see cref="BitEncoding.EncodeMode.Length1"/> der für das Wrapping überschritten werden muss
         /// </summary>
         int L1_wrapdown;
         /// <summary>
         /// unterer Grenzwert für <see cref="BitEncoding.EncodeMode.Length1"/> der für das Wrapping unterschritten werden muss
         /// </summary>
         int L1_wrapup;

         /// <summary>
         /// oberer Grenzwert für <see cref="BitEncoding.EncodeMode.Length2"/> der für das Wrapping überschritten werden muss
         /// </summary>
         int L2_wrapdown;
         /// <summary>
         /// unterer Grenzwert für <see cref="BitEncoding.EncodeMode.Length2"/> der für das Wrapping unterschritten werden muss
         /// </summary>
         int L2_wrapup;

         /// <summary>
         /// oberer Grenzwert für <see cref="BitEncoding.EncodeMode.Hybrid"/> der für ein eventuelles Wrapping mindestens überschritten werden muss
         /// </summary>
         int H_wrapdown;
         /// <summary>
         /// unterer Grenzwert für <see cref="BitEncoding.EncodeMode.Hybrid"/> der für ein eventuelles Wrapping mindestens unterschritten werden muss
         /// </summary>
         int H_wrapup;

         /// <summary>
         /// Maximalhöhe der Kachel
         /// </summary>
         int max;

         ValueRange Valuerange;



         public Wraparound(int maxheight) {

            Valuerange = new ValueRange(maxheight);

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

         }

         /// <summary>
         /// ein Wert wird bei Bedarf gewrapt, notfalls auch die Codierart auf BiBin gesetzt
         /// </summary>
         /// <param name="data">Wert</param>
         /// <param name="wrapped">gesetzt, wenn ein gewrapter Wert geliefert wird</param>
         /// <param name="em">Codierart des Wertes; danach ev. auf BigBin gesetzt</param>
         /// <param name="hunit">nur für die Codierart <see cref="BitEncoding.EncodeMode.Hybrid"/> nötig</param>
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

                  Valuerange.Length0(out minval, out maxval, iPlateauLengthBinBits);
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

                  Valuerange.Length1(out minval, out maxval, iPlateauLengthBinBits);
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

                  Valuerange.Length2(out minval, out maxval, iPlateauLengthBinBits);
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

                  Valuerange.Hybrid(hunit, out minval, out maxval, iPlateauLengthBinBits);
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
                     if (data > Valuerange.MaxBigBinL1)
                        datawrapped = WrapDown(data);
                     else if (data < Valuerange.MinBigBinL1)
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

         /// <summary>
         /// Codierung im "festen" Binärformat (für große Zahlen)
         /// </summary>
         BigBinaryPF,
         /// <summary>
         /// Codierung im "festen" Binärformat (für große Zahlen an Stelle der <see cref="Length1"/>-Codierung)
         /// </summary>
         BigBinaryPFL1,
         /// <summary>
         /// Codierung im "festen" Binärformat (für große Zahlen an Stelle der <see cref="Length2"/>-Codierung)
         /// </summary>
         BigBinaryPFL2,
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

      Wraparound ValueWrap;

      BitEncoding BitEnc;

      /// <summary>
      /// alle Höhenwerte
      /// </summary>
      List<int> HeightValues;

      public class BitStream {

         public MemoryStream Stream { get; private set; }
         byte actbyte;
         int bitpos;

         public BitStream() {
            Stream = new MemoryStream(10240);
            actbyte = 0;
            bitpos = 0;
         }

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

         public void Add(bool one, int count) {
            while (count > 0)
               Add(one);
         }

         public byte[] GetBytes() {
            while (bitpos != 0)
               Add(true); // mit 1-Bits füllen
            return Stream.ToArray();
         }

      }

      BitStream bits;




      /// <summary>
      /// erzeugt einen Encoder für eine Kachel
      /// <para>Mit dem wiederholten Aufruf von <see cref="ComputeNext"/> werden die gelieferten Daten schrittweise codiert. Wenn diese Funktion 0 liefert, ist
      /// die Codierung abgeschlossen. Die fertige Bytefolge knn mit <see cref="GetCodedBytes"/> abgerufen werden.
      /// </para>
      /// </summary>
      /// <param name="maxheight">max. Höhe</param>
      /// <param name="width">Breite der Kachel</param>
      /// <param name="tilesizevert">Höhe der Kachel</param>
      /// <param name="normalizedheight">Liste der Höhendaten (Anzahl normalerweise <see cref="tilesize"/> * <see cref="tilesize"/>)</param>
      public TileEncoderProd(int maxheight, int width, int height, IList<int> normalizedheight) {
         MaxHeight = maxheight;
         Width = width;
         Height = height;

         HeightValues = new List<int>(normalizedheight);    // Kopie (eigentlich nicht nötig)
      }

      public byte[] Encode() {
         bits = new BitStream();
         BitEnc = new BitEncoding(MaxHeight, bits);

         ct_std = new CodingTypeStd(MaxHeight);
         ct_pf_0 = new CodingTypePlateauFollowerZero(MaxHeight);
         ct_pf_not0 = new CodingTypePlateauFollowerNotZero(MaxHeight);

         ValueWrap = new Wraparound(MaxHeight);

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
                  BitEnc.EncodePlateau(length, col, row, Width, PlateauTable4Tile);

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
                  bool wrapped = false;
                  data = ValueWrap.SimpleWrap(data, out wrapped);
                  data = ValueWrap.Wrap(data, out wrapped, ref em, ct_std.HunitValue, -1);

                  switch (em) {
                     case EncodeMode.Hybrid:
                        BitEnc.EncodeHybrid(data, ct_std.HunitValue);
                        break;

                     case EncodeMode.Length0:
                        BitEnc.EncodeLength0(data);
                        break;
                     case EncodeMode.Length1:
                        BitEnc.EncodeLength1(data);
                        break;

                     case EncodeMode.BigBinary:
                        BitEnc.EncodeBigBin(data, EncodeMode.BigBinary);
                        break;

                     case EncodeMode.BigBinaryL1:
                        BitEnc.EncodeBigBin(data, EncodeMode.BigBinaryL1);
                        break;
                  }

                  ct_std.AddValue(data);

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

         CodingType ct = follower_ddiff != 0 ? ct = ct_pf_not0 : ct = ct_pf_0;
         EncodeMode em = ct.EncodeMode;

         bool wrapped;
         // Nachfolger codieren
         if (follower_ddiff != 0) {

            if (follower_ddiff > 0)
               follower_vdiff = -follower_vdiff;

            follower_vdiff = ValueWrap.SimpleWrap(follower_vdiff, out wrapped);
            follower_vdiff = ValueWrap.Wrap(follower_vdiff, out wrapped, ref em, ct.HunitValue, PlateauTable4Tile.Bits());

         } else {

            follower_vdiff = ValueWrap.SimpleWrap(follower_vdiff, out wrapped);
            follower_vdiff = ValueWrap.Wrap(follower_vdiff, out wrapped, ref em, ct.HunitValue, PlateauTable4Tile.Bits());
            if (follower_vdiff < 0)
               follower_vdiff++;

         }

         switch (em) {
            case EncodeMode.Hybrid:
               BitEnc.EncodeHybrid(follower_vdiff, ct.HunitValue);
               break;

            case EncodeMode.Length0:
               BitEnc.EncodeLength0(follower_vdiff);
               break;

            case EncodeMode.Length1:
               BitEnc.EncodeLength1(follower_vdiff);
               break;

            case EncodeMode.Length2:
               BitEnc.EncodeLength0(-follower_vdiff);
               break;

            case EncodeMode.BigBinary:
               BitEnc.EncodeBigBin(follower_vdiff, EncodeMode.BigBinaryPF);
               break;

            case EncodeMode.BigBinaryL1:
               BitEnc.EncodeBigBin(follower_vdiff, EncodeMode.BigBinaryPFL1);
               break;

            case EncodeMode.BigBinaryL2:
               BitEnc.EncodeBigBin(-follower_vdiff, EncodeMode.BigBinaryPFL2);
               break;
         }
         ct.AddValue(follower_vdiff);
      }

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
         return col + row * Width >= Width * Height;
      }

      /// <summary>
      /// ermittelt die Länge eines Plateaus ab der Startposition (ev. auch 0)
      /// </summary>
      /// <param name="col"></param>
      /// <param name="row"></param>
      /// <returns></returns>
      int GetPlateauLength(int col, int row) {
         int length = 0;
         int value = -1;
         // bool bEnd = false;

         while (col + row * Width < HeightValues.Count) {
            if (value < 0)
               value = ValidHeight(col - 1, row);

            if (ValidHeight(col, row) != value)
               break;

            length++;

            if (col == Width - 1)   // Zeilenende ereicht
               break;

            IncrementPosition(ref col, ref row);
            if (row >= Height) {    // Ende erreicht
                                    // bEnd = true;
               break;
            }
         }

         return length;    // bEnd;
      }

      /// <summary>
      /// liefert auch für ungültige Spalten und Zeilen eine verarbeitbare Höhe, d.h. außerhalb von <see cref="Width"/> bzw. <see cref="Height"/> immer 0 
      /// bzw. bei Spalte -1 die virtuelle Spalte (Spalte 0 der Vorgängerzeile)
      /// </summary>
      /// <param name="column"></param>
      /// <param name="line"></param>
      /// <returns></returns>
      public int ValidHeight(int column, int line) {
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

