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
   /// zum Lesen der HGT-Daten:
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
   /// Die "äußeren" Punkte haben jeweils volle Grad als Koordinaten.
   /// 
   /// z.B.
   /// http://dds.cr.usgs.gov/srtm/version2_1
   /// http://www.viewfinderpanoramas.org/dem3.html
   /// http://srtm.csi.cgiar.org/
   /// </summary>
   public class HGTReader : IDisposable {

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
      /// Abstand zwischen 2 Punkten
      /// </summary>
      public double Delta { get; private set; }
      /// <summary>
      /// Anzahl der ungültigen Werte
      /// </summary>
      public long NotValid { get; private set; }

      /// <summary>
      /// Kennung, wenn der Wert fehlt
      /// </summary>
      public const int NOVALUE = -32768;

      string filename;
      short[] data;

      /// <summary>
      /// liest die Daten aus der entsprechenden HGT-Datei ein
      /// </summary>
      /// <param name="left">positiv für östliche Länge, sonst negativ</param>
      /// <param name="bottom">positiv für nördliche Breite, sonst negativ</param>
      /// <param name="directory">Verzeichnis der Datendatei</param>
      /// <param name="dummydataonerror">liefert Dummy-Daten, wenn die Datei nicht ex.</param>
      public HGTReader(int left, int bottom, string directory, bool dummydataonerror) {
         Left = left;
         Bottom = bottom;
         Maximum = short.MinValue;
         Minimum = short.MaxValue;
         filename = Path.Combine(directory, GetFilename());

         if (File.Exists(filename)) {

            Stream dat = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

            ReadFromStream(dat, (int)dat.Length);
            dat.Close();

         } else {

            if (!File.Exists(filename + ".zip")) {

               if (!dummydataonerror)
                  throw new Exception(string.Format("Weder die Datei '{0}' noch die Datei '{0}.zip' existiert.", filename));
               else {
                  Minimum = Maximum = NOVALUE;
                  data = new Int16[1201 * 1201];
                  for (int i = 0; i < data.Length; i++)
                     data[i] = NOVALUE;
                  NotValid = data.Length;
               }

            } else {

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

         }

         Rows = Columns = (int)Math.Sqrt(data.Length);      // sollte im Normalfall immer quadratisch sein
         Delta = 1.0 / (Rows - 1);
      }

      void ReadFromStream(Stream str, int length) {
         data = new Int16[length / 2];       // 2 Byte je Datenpunkt
         NotValid = 0;
         for (int i = 0; i < data.Length; i++) {
            data[i] = (Int16)((str.ReadByte() << 8) + str.ReadByte());
            if (Maximum < data[i])
               Maximum = data[i];
            if (data[i] != NOVALUE) {
               if (Minimum > data[i])
                  Minimum = data[i];
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
            if (data[i] != NOVALUE) {
               if (Minimum > data[i]) Minimum = data[i];
            } else
               NotValid++;
         }
         Rows = Columns = (int)Math.Sqrt(data.Length);      // sollte im Normalfall immer quadratisch sein
      }

      protected string GetFilename() {
         string name = "";
         if (Bottom >= 0) name += string.Format("N{0:d2}", Bottom);
         else name += string.Format("S{0:d2}", -Bottom);
         if (Left >= 0) name += string.Format("E{0:d3}", Left);
         else name += string.Format("W{0:d3}", -Left);
         return name + ".hgt";
      }

      /// <summary>
      /// liefert den Wert der Matrix (Koordinatenursprung links-oben)
      /// </summary>
      /// <param name="row">Zeilennr. (0 ist die nördlichste)</param>
      /// <param name="col">Spaltennr. (0 ist die westlichste)</param>
      /// <returns></returns>
      public int Get(int row, int col) {
         if (row < 0 || Rows <= row ||
             col < 0 || Columns <= col)
            throw new Exception(string.Format("({0}, {1}) außerhalb des Zeilen- und/oder Spaltenbereichs ({2}, {3})", col, row, Columns, Rows));
         return data[row * Columns + col];
      }

      /// <summary>
      /// liefert den Wert der Matrix (Koordinatenursprung links-unten)
      /// </summary>
      /// <param name="x">0 .. unter <see cref="Columns"/> (0 ist der westlichste)</param>
      /// <param name="y">0 .. unter <see cref="Rows"/> (0 ist die südlichste)</param>
      /// <returns></returns>
      public int Get4XY(int x, int y) {
         return Get(Rows - 1 - y, x);
      }

      /// <summary>
      /// liefert einen interpolierten Höhenwert
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <returns></returns>
      public double InterpolatedHeight(double lon, double lat) {
         double h = NOVALUE;
         lon -= Left;
         lat -= Bottom; // Koordinaten auf die Ecke links unten bezogen

         if (0.0 <= lon && lon <= 1.0 &&
             0.0 <= lat && lat <= 1.0) {
            // x-y-Index des Eckpunktes links unten des umschließenden Quadrats bestimmen
            int x = (int)(lon / Delta);
            int y = (int)(lat / Delta);
            if (x == Columns - 1) // liegt auf rechtem Rand
               x--;
            if (y == Rows - 1) // liegt auf oberem Rand
               y--;

            // lon/lat jetzt bzgl. der Ecke links-unten des umschließenden Quadrats bilden (0 .. <Delta)
            double delta_lon = lon - x * Delta;
            double delta_lat = lat - y * Delta;

            if (delta_lon == 0) {            // linker Rand
               if (delta_lat == 0)
                  h = Get4XY(x, y);          // Eckpunkt links unten
               else if (delta_lat >= Delta)
                  h = Get4XY(x, y + 1);      // Eckpunkt links oben (eigentlich nicht möglich)
               else
                  h = LinearInterpolatedHeight(Get4XY(x, y),
                                               Get4XY(x, y + 1),
                                               delta_lat / Delta);
            } else if (delta_lon >= Delta) { // rechter Rand (eigentlich nicht möglich)
               if (delta_lat == 0)
                  h = Get4XY(x + 1, y);      // Eckpunkt rechts unten
               else if (delta_lat >= Delta)
                  h = Get4XY(x + 1, y + 1);  // Eckpunkt rechts oben (eigentlich nicht möglich)
               else
                  h = LinearInterpolatedHeight(Get4XY(x + 1, y),
                                               Get4XY(x + 1, y + 1),
                                               delta_lat / Delta);
            } else if (delta_lat == 0) {     // unterer Rand (außer Eckpunkt)
               h = LinearInterpolatedHeight(Get4XY(x, y),
                                            Get4XY(x + 1, y),
                                            delta_lon / Delta);
            } else if (delta_lat >= Delta) { // oberer Rand (außer Eckpunkt) (eigentlich nicht möglich)
               h = LinearInterpolatedHeight(Get4XY(x, y + 1),
                                            Get4XY(x + 1, y + 1),
                                            delta_lon / Delta);

            } else                           // Punkt innerhalb des Rechtecks
               h = InterpolatedHeightInNormatedRectangle(delta_lon / Delta,
                                                         delta_lat / Delta,
                                                         Get4XY(x, y + 1),
                                                         Get4XY(x + 1, y + 1),
                                                         Get4XY(x + 1, y),
                                                         Get4XY(x, y));
         }

         return h;
      }

      /// <summary>
      /// die Höhe für den Punkt P im umschließenden Quadrat (Seitenlänge 1) aus 4 Eckpunkten wird interpoliert
      /// </summary>
      /// <param name="qx">Abstand P vom linken Rand des Quadrat (Bruchteil 0..1)</param>
      /// <param name="qy">Abstand P vom unteren Rand des Quadrat (Bruchteil 0..1)</param>
      /// <param name="hlt">Höhe links oben</param>
      /// <param name="hrt">Höhe rechts oben</param>
      /// <param name="hrb">Höhe rechts unten</param>
      /// <param name="hlb">Höhe links unten</param>
      /// <returns></returns>
      double InterpolatedHeightInNormatedRectangle(double qx, double qy, int hlt, int hrt, int hrb, int hlb) {
         if (hlb == HGTReader.NOVALUE ||
             hrt == HGTReader.NOVALUE)
            return HGTReader.NOVALUE; // keine Berechnung möglich

         /* In welchem Dreieck liegt der Punkt? 
          *    oben  +-/
          *          |/
          *          
          *    unten  /|
          *          /-+
          */
         if (qy >= qx) { // oberes Dreieck aus hlb, hrt und hlt (Anstieg py/px ist größer als height/width)

            if (hlt == HGTReader.NOVALUE)
               return HGTReader.NOVALUE;

            // hlt als Koordinatenursprung normieren; mit hrt und hlb 3 Punkte einer Ebene (3-Punkt-Gleichung)
            hrt -= hlt;
            hlb -= hlt;
            qy -= 1;

            return hlt + qx * hrt - qy * hlb;

         } else { // unteres Dreieck aus hlb, hrb und hrt

            if (hrb == HGTReader.NOVALUE)
               return HGTReader.NOVALUE;

            // hrb als Koordinatenursprung normieren; mit hrt und hlb 3 Punkte einer Ebene (3-Punkt-Gleichung)
            hrt -= hrb;
            hlb -= hrb;
            qx -= 1;

            return hrb - qx * hlb + qy * hrt;
         }
      }

      /// <summary>
      /// liefert die "gewichtete" Höhe zwischen den beiden Höhen in Relation zum jeweiligen Abstand (alle 3 Höhen auf einer Linie)
      /// </summary>
      /// <param name="h1"></param>
      /// <param name="l1"></param>
      /// <param name="h2"></param>
      /// <param name="q1"></param>
      /// <returns></returns>
      double LinearInterpolatedHeight(int h1, int h2, double q1) {
         if (h1 == NOVALUE)
            return q1 < .5 ? NOVALUE : h1; // wenn dichter am NOVALUE, dann NOVALUE sonst gleiche Höhe wie der andere Punkt
         if (h2 == NOVALUE)
            return q1 > .5 ? NOVALUE : h2;
         return h1 + q1 * (h2 - h1);
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
                  data[y * Columns + x] = NOVALUE;
                  discard++;
               }
         Maximum = Int16.MinValue;
         Minimum = Int16.MaxValue;
         NotValid = 0;
         for (int i = 0; i < data.Length; i++) {
            if (Maximum < data[i]) Maximum = data[i];
            if (data[i] != NOVALUE) {
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

      #region Implementierung der IDisposable-Schnittstelle

      ~HGTReader() {
         Dispose(false);
      }

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// überschreibt die Standard-Methode
      /// <para></para>
      /// </summary>
      /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
      protected virtual void Dispose(bool notfromfinalizer) {
         if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben

               data = null;

            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }
}
