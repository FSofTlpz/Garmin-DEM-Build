/*
Copyright (C) 2011 Frank Stinner

This program is free software; you can redistribute it and/or modify it 
under the terms of the GNU General Public License as published by the 
Free Software Foundation; either version 3 of the License, or (at your 
option) any later version. 

This program is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of 
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General 
Public License for more details. 

You should have received a copy of the GNU General Public License along 
with this program; if not, see <http://www.gnu.org/licenses/>. 


Dieses Programm ist freie Software. Sie können es unter den Bedingungen 
der GNU General Public License, wie von der Free Software Foundation 
veröffentlicht, weitergeben und/oder modifizieren, entweder gemäß 
Version 3 der Lizenz oder (nach Ihrer Option) jeder späteren Version. 

Die Veröffentlichung dieses Programms erfolgt in der Hoffnung, daß es 
Ihnen von Nutzen sein wird, aber OHNE IRGENDEINE GARANTIE, sogar ohne 
die implizite Garantie der MARKTREIFE oder der VERWENDBARKEIT FÜR EINEN 
BESTIMMTEN ZWECK. Details finden Sie in der GNU General Public License. 

Sie sollten ein Exemplar der GNU General Public License zusammen mit 
diesem Programm erhalten haben. Falls nicht, siehe 
<http://www.gnu.org/licenses/>. 
*/

using System;
using System.IO;
using System.IO.Compression;

namespace BuildDEMFile {

   /// <summary>
   /// zum lesen der HGT-Daten:
   /// 
   /// Eine HGT-Datei enthält die Höhendaten für ein "Quadratgrad", also ein Gebiet über 1 Längen- und 1
   /// Breitengrad. Die Ausdehnung in N-S-Richtung ist also etwa 111km in O-W-Richtung je nach Breitengrad
   /// weniger.
   /// Die ursprünglichen SRTM-Daten (Shuttle Radar Topography Mission (SRTM) im Februar 2000) liegen im
   /// Bereich zwischen dem 60. nördlichen und 58. südlichen Breitengrad vor. Für die USA liegen diese Daten
   /// mit einer Auflösung von 1 Bogensekunde vor (--> 3601x3601 Datenpunkte, SRTM-1, etwa 30m), für den Rest 
   /// der Erde in 3 Bogensekunden (1201x1201, SRTM-3, etwa 92m). Die Randpunkte eines Gebietes sind also 
   /// identisch mit den Randpunkten des jeweils benachbarten Gebietes. (Der 1. Punkt einer Zeile oder Spalte
   /// liegt auf dem einen Rand des Gebietes, der letzte Punkt auf dem gegenüberliegenden Rand.)
   /// Der Dateiname leitet sich immer aus der S-W-Ecke (links-unten) des Gebietes ab, z.B. 
   ///      n51e002.hgt --> Gebiet zwischen N 51° E 2° und N 52° E 3°
   ///      s14w077.hgt --> Gebiet zwischen S 14° W 77° und S 13° W 76°
   /// Die Speicherung der Höhe erfolgt jeweils mit 2 Byte in Big-Endian-Bytefolge mit Vorzeichen.
   /// Die Werte sind in Metern angeben.
   /// Punkte ohne gültigen Wert haben den Wert 0x8000 (-32768).
   /// Die Reihenfolge der Daten ist zeilenweise von N nach S, innerhalb der Zeilen von W nach O.
   /// 
   /// z.B.
   /// http://dds.cr.usgs.gov/srtm/version2_1
   /// http://www.viewfinderpanoramas.org/dem3.html
   /// http://srtm.csi.cgiar.org/
   /// </summary>
   public class HGTReader {

      /// <summary>
      /// linker Rand in Grad
      /// </summary>
      public int Left { get; private set; }
      /// <summary>
      /// unterer Rand in Grad
      /// </summary>
      public int Bottom { get; private set; }
      /// <summary>
      /// Anzahl der Datenzeilen
      /// </summary>
      public int Rows { get; private set; }
      /// <summary>
      /// Anzahl der Datenspalten
      /// </summary>
      public int Columns { get; private set; }
      /// <summary>
      /// kleinster Wert
      /// </summary>
      public int Minimum { get; private set; }
      /// <summary>
      /// größter Wert
      /// </summary>
      public int Maximum { get; private set; }
      /// <summary>
      /// Anzahl der ungültigen Werte
      /// </summary>
      public long NotValid { get; private set; }

      /// <summary>
      /// Kennung, wenn der Wert fehlt
      /// </summary>
      public const int NoValue = -32768;

      string filename;
      Int16[] data;

      /// <summary>
      /// liest die Daten aus der entsprechenden HGT-Datei ein
      /// </summary>
      /// <param name="left">positiv für östliche Länge, sonst negativ</param>
      /// <param name="bottom">positiv für nördliche Breite, sonst negativ</param>
      /// <param name="directory">Verzeichnis der Datendatei</param>
      public HGTReader(int left, int bottom, string directory) {
         Left = left;
         Bottom = bottom;
         Maximum = Int16.MinValue;
         Minimum = Int16.MaxValue;
         filename = Path.Combine(directory, GetFilename());

         if (File.Exists(filename)) {

            Stream dat = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

            ReadFromStream(dat, (int)dat.Length);
            dat.Close();

         } else {

            if (!File.Exists(filename + ".zip"))
               throw new Exception(string.Format("Weder die Datei '{0}' noch die Datei '{0}.zip' existiert.", filename));

            using (FileStream zipstream = new FileStream(filename + ".zip", FileMode.Open, FileAccess.Read, FileShare.Read)) {
               using (ZipArchive zip = new ZipArchive(zipstream, ZipArchiveMode.Read)) {
                  filename = Path.GetFileName(filename).ToUpper();
                  ZipArchiveEntry entry = null;
                  foreach (var item in zip.Entries) {
                     if (filename == item.Name.ToUpper()) {
                        entry = item;
                        break;
                     }
                  }
                  if (entry == null)
                     throw new Exception(string.Format("Die Datei '{0}' ist nicht in der Datei '{0}.zip' enthalten.", filename));
                  Stream dat = entry.Open();
                  ReadFromStream(entry.Open(), (int)entry.Length);
                  dat.Close();
               }
            }

         }

         Rows = Columns = (int)Math.Sqrt(data.Length);      // sollte im Normalfall immer quadratisch sein
      }

      void ReadFromStream(Stream str, int length) {
         data = new Int16[length / 2];       // 2 Byte je Datenpunkt
         NotValid = 0;
         for (int i = 0; i < data.Length; i++) {
            data[i] = (Int16)((str.ReadByte() << 8) + str.ReadByte());
            if (Maximum < data[i]) Maximum = data[i];
            if (data[i] != NoValue) {
               if (Minimum > data[i]) Minimum = data[i];
            } else
               NotValid++;
         }
      }

      /// <summary>
      /// nur zur Erzeugung von Testdaten
      /// </summary>
      /// <param name="left"></param>
      /// <param name="bottom"></param>
      /// <param name="dat"></param>
      public HGTReader(int left, int bottom, short[] dat) {
         Left = left;
         Bottom = bottom;
         Maximum = Int16.MinValue;
         Minimum = Int16.MaxValue;
         data = new Int16[dat.Length];
         NotValid = 0;
         for (int i = 0; i < data.Length; i++) {
            data[i] = dat[i];
            if (Maximum < data[i]) Maximum = data[i];
            if (data[i] != NoValue) {
               if (Minimum > data[i]) Minimum = data[i];
            } else
               NotValid++;
         }
         Rows = Columns = (int)Math.Sqrt(data.Length);      // sollte im Normalfall immer quadratisch sein
      }

      protected string GetFilename() {
         string name = "";
         if (Bottom >= 0) name += string.Format("n{0:d2}", Bottom);
         else name += string.Format("s{0:d2}", -Bottom);
         if (Left >= 0) name += string.Format("e{0:d3}", Left);
         else name += string.Format("w{0:d3}", -Left);
         return name + ".hgt";
      }

      /// <summary>
      /// liefert den Wert der Matrix
      /// </summary>
      /// <param name="row">Zeilennr. (0 ist die nördlichste)</param>
      /// <param name="col">Spaltennr. (0 ist die westlichste)</param>
      /// <returns></returns>
      public int Get(int row, int col) {
         if (row < 0 || Rows <= row ||
             col < 0 || Columns <= col)
            throw new Exception("außerhalb des Zeilen- und/oder Spaltenbereichs");
         return data[row * Columns + col];
      }

      /// <summary>
      /// liefert den Wert der Matrix
      /// </summary>
      /// <param name="x">0 .. unter <see cref="Columns"/></param>
      /// <param name="y">0 .. unter <see cref="Rows"/></param>
      /// <returns></returns>
      public int Get4XY(int x, int y) {
         return Get(Rows - 1 - y, x);
      }


      /// <summary>
      /// alle Werte bis auf den definierten Bereich ungültig machen
      /// </summary>
      /// <param name="mincol"></param>
      /// <param name="minrow">obere Zeile</param>
      /// <param name="maxcol"></param>
      /// <param name="maxrow">untere Zeile</param>
      /// <returns>Anzahl der NICHT ungültig gemachten Werte</returns>
      public long DiscardExcept(int mincol, int minrow, int maxcol, int maxrow) {
         long discard = 0;
         for (int x = 0; x < Columns; x++)
            for (int y = 0; y < Rows; y++)
               if (!(mincol <= x && x <= maxcol &&
                     minrow <= y && y <= maxrow)) {
                  data[y * Columns + x] = NoValue;
                  discard++;
               }
         Maximum = Int16.MinValue;
         Minimum = Int16.MaxValue;
         NotValid = 0;
         for (int i = 0; i < data.Length; i++) {
            if (Maximum < data[i]) Maximum = data[i];
            if (data[i] != NoValue) {
               if (Minimum > data[i]) Minimum = data[i];
            } else
               NotValid++;
         }
         return Rows * Columns - discard;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="MinLongitude"></param>
      /// <param name="MinLatitude"></param>
      /// <param name="MaxLongitude"></param>
      /// <param name="MaxLatitude"></param>
      /// <returns>Anzahl der NICHT ungültig gemachten Werte</returns>
      public long DiscardExcept(double MinLongitude, double MinLatitude, double MaxLongitude, double MaxLatitude) {
         double lon1 = Math.Max(0.0, Math.Min(1.0, MinLongitude - Left));
         double lon2 = Math.Max(0.0, Math.Min(1.0, MaxLongitude - Left));
         double lat1 = Math.Max(0.0, Math.Min(1.0, MinLatitude - Bottom));
         double lat2 = Math.Max(0.0, Math.Min(1.0, MaxLatitude - Bottom));
         return DiscardExcept((int)(lon1 * Columns), (int)((1 - lat2) * Rows),
                              (int)(lon2 * Columns), (int)((1 - lat1) * Rows));
      }

      public override string ToString() {
         return string.Format("HGT-Daten: {0}{1}° {2}{3}°, {4}x{5}, {6}m..{7}m, ungültige Werte: {8} ({9}%)",
            Bottom >= 0 ? "N" : "S", Bottom >= 0 ? Bottom : -Bottom,
            Left >= 0 ? "E" : "W", Left >= 0 ? Left : -Left,
            Rows, Columns,
            Minimum, Maximum,
            NotValid, (100.0 * NotValid) / (Rows * Columns));
      }

   }
}
