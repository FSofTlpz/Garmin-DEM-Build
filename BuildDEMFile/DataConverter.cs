using System;

namespace BuildDEMFile {

   /// <summary>
   /// zum Einlesen der HGT-Daten und liefern von interpolierten Höhenwerten
   /// </summary>
   class DataConverter {

      /// <summary>
      /// intern: Wert für "Höhe ist undefiniert"
      /// </summary>
      const double UNDEFD = double.NaN;

      /// <summary>
      /// Wert für "Höhe ist undefiniert" in den Ergebnisdaten (beachte <see cref="Subtile.UNDEF"/>)
      /// </summary>
      public const int UNDEF = short.MaxValue;

      /// <summary>
      /// Fuß in Meter
      /// </summary>
      const double FOOT = 0.3048;

      double left, top, width, height;
      HGTReader[,] dat;


      public DataConverter(double left, double top, double width, double height) {
         this.left = left;
         this.top = top;
         this.width = width;
         this.height = height;
      }

      public bool ReadData(string hgtpath, bool dummydataonerror) {
         bool ret = true;
         // Voraussetzung: Die Datendateien liegen im 1-Grad-Raster vor. In den Dateinamen ist die linke untere Ecke enthalten.

         int iLeft = (int)left;
         if (iLeft > left)
            iLeft--;

         int iRight = (int)(left + width);
         if (iRight < left + width)
            iRight++;

         int iTop = (int)top;
         if (iTop < top)
            iTop++;

         int iBottom = (int)(top - height);
         if (iBottom > top - height)
            iBottom--;

         dat = new HGTReader[iRight - iLeft, iTop - iBottom];

         for (int lon = iLeft; lon < iRight; lon++)
            for (int lat = iBottom; lat < iTop; lat++) {
               try {
                  HGTReader hgt = new HGTReader(lon, lat, hgtpath, dummydataonerror);
                  dat[lon - iLeft, lat - iBottom] = hgt;
                  Console.Error.WriteLine(string.Format("Höhen für {0}° .. {1}° / {2}° .. {3}° eingelesen {4}",
                                                        lon, lon + 1,
                                                        lat, lat + 1,
                                                        hgt.Minimum == hgt.Maximum ? " (nur Dummywerte)" : ""));
               } catch (Exception ex) {
                  Console.Error.WriteLine(ex.Message);
                  ret = false;
               }
            }

         return ret;
      }

      /// <summary>
      /// liefert die (interpolierte) Höhe
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <returns></returns>
      double GetHeight(double lon, double lat) {
         // Quell-Gebiet bestimmen
         HGTReader src = null;
         for (int i = 0; i < dat.GetLength(0); i++)
            for (int j = 0; j < dat.GetLength(1); j++) {
               src = dat[i, j];
               if (src != null &&
                   src.Left <= lon && lon < src.Left + 1 &&
                   src.Bottom <= lat && lat < src.Bottom + 1) {
                  i = dat.GetLength(0);
                  break;
               } else
                  src = null;
            }

         if (src != null) {
            // 4 umgebende Punkte bestimmen
            lon -= src.Left;
            lat -= src.Bottom;
            double stepwidth = 1.0 / (src.Columns - 1); // abstand zwischen 2 Punkten
            double stepheight = 1.0 / (src.Rows - 1);
            int xl = (int)(lon / stepwidth);     // x <= gesuchter lon
            int yb = (int)(lat / stepheight);    // y <= gesuchter lat
            int xr = xl + 1;
            int yt = yb + 1;

            // interpolieren
            if (xl == xr) {
               if (yb == yt) // genau die Koordinate getroffen
                  return src.Get(xl, yb);
               else // auf der senkrechten Verbindung der Punkte
                  return GetRelativeHeight(src.Get(xl, yb), lat - yb, src.Get(xl, yt), yt - lat);
            } else if (yb == yt) { // auf der waagerechten Verbindung der Punkte
               return GetRelativeHeight(src.Get(xl, yb), lon - xl, src.Get(xr, yb), xr - lon);
            } else { // Normalfall: mitten drin
               return Interpolation(lon - xl * stepwidth, lat - yb * stepheight, // normieren auf Umgebungspunkt links unten
                                    src.Get4XY(xl, yt), src.Get4XY(xr, yt), src.Get4XY(xr, yb), src.Get4XY(xl, yb),
                                    stepwidth, stepheight);
            }
         }
         return UNDEFD;
      }

      /// <summary>
      /// die Höhe für den Punkt P wird interpoliert
      /// <para>Die Höhen der 4 Eckpunkte und die Breite und Höhe eines Rechteckes sind gegeben.</para>
      /// </summary>
      /// <param name="px">Abstand P vom linken Rand des Rechteckes</param>
      /// <param name="py">Abstand P vom unteren Rand des Rechteckes</param>
      /// <param name="hlt">Höhe links oben</param>
      /// <param name="hrt">Höhe rechts oben</param>
      /// <param name="hrb">Höhe rechts unten</param>
      /// <param name="hlb">Höhe links unten</param>
      /// <param name="width">Breite des Rechteckes</param>
      /// <param name="height">Höhe des Rechteckes</param>
      /// <returns></returns>
      double Interpolation(double px, double py, int hlt, int hrt, int hrb, int hlb, double width, double height) {
         if (hlb == HGTReader.NoValue ||
             hrt == HGTReader.NoValue)
            return UNDEFD; // keine Berechnung möglich

         /* In welchem Dreieck liegt der Punkt? 
          *    oben  +-/
          *          |/
          *          
          *    unten  /|
          *          /-+
          */
         if (width * py >= height * px) { // oberes Dreieck aus hlb, hrt und hlt (Anstieg py/px ist größer als height/width)

            if (hlt == HGTReader.NoValue)
               return UNDEFD;

            // hlt als Koordinatenursprung normieren; mit hrt und hlb 3 Punkte einer Ebene (3-Punkt-Gleichung)
            hrt -= hlt;
            hlb -= hlt;
            py -= height;

            return hlt + px * hrt / width + py * hlb / -height;

         } else { // unteres Dreieck aus hlb, hrb und hrt

            if (hrb == HGTReader.NoValue)
               return UNDEFD;

            // hrb als Koordinatenursprung normieren; mit hrt und hlb 3 Punkte einer Ebene (3-Punkt-Gleichung)
            hrt -= hrb;
            hlb -= hrb;
            px -= width;

            return hrb + px * hlb / -width + py * hrt / height;
         }
      }

      /// <summary>
      /// liefert die "gewichtete" Höhe zwischen den beiden Höhen in Relation zum jeweiligen Abstand (alle 3 Höhen auf einer Linie)
      /// </summary>
      /// <param name="h1"></param>
      /// <param name="l1"></param>
      /// <param name="h2"></param>
      /// <param name="l2"></param>
      /// <returns></returns>
      double GetRelativeHeight(int h1, double l1, int h2, double l2) {
         return l1 * (h2 - h1) / (double)(l1 + l2);
      }

      /// <summary>
      /// berechnet das Array der interpolierten Höhen
      /// </summary>
      /// <param name="left">linken Rand</param>
      /// <param name="top">oberer Rand</param>
      /// <param name="width">Breite</param>
      /// <param name="height">Höhe</param>
      /// <param name="stepwidth">horizontale Schrittweite</param>
      /// <param name="stepheight">vertikale Schrittweite</param>
      /// <param name="foot">Daten in Fuß</param>
      /// <returns></returns>
      public int[,] BuildHeightArray(double left, double top, double width, double height, double stepwidth, double stepheight, bool foot) {
         if (left < this.left ||
             top > this.top ||
             left + width > this.left + this.width ||
             top - height < this.top - this.height)
            throw new Exception("Der gewünschte Bereich überschreitet den Bereich der eingelesenen HGT-Werte.");

         int iCountLon = (int)(width / stepwidth);
         if (iCountLon * stepwidth < width)
            iCountLon++;
         iCountLon++;

         int iCountLat = (int)(height / stepheight);
         if (iCountLat * stepheight < height)
            iCountLat++;
         iCountLat++;

         Console.Error.WriteLine(string.Format("erzeuge {0} x {1} interpolierte Höhenwerte für den Abstand {2}° x {3}°...", iCountLon, iCountLat, stepwidth, stepheight));

         int[,] heights = new int[iCountLon, iCountLat];    // Array darf nicht größer als 2GB-x werden -> 532000000 Elemente fkt. (z.B. 23065 x 23065)
                                                            // Int16-Array benötigt leider genausoviel Speicher!!!
         for (int j = 0; j < iCountLat; j++) {
            double lat = top - j * stepheight;
            for (int i = 0; i < iCountLon; i++) {
               double lon = left + i * stepwidth;

               int iHeight = UNDEF;
               double h = GetHeight(lon, lat);  // interpolierte Höhe
               if (!double.IsNaN(h)) {
                  if (foot)
                     h /= FOOT;
                  iHeight = (int)Math.Round(h, 0);
               }
               heights[i, j] = iHeight;
            }
         }

         return heights;
      }

      public override string ToString() {
         return string.Format("left {0}, top {1}, width {2}, height {3}, HGT-Parts {4}", left, top, width, height, dat != null ? dat.GetLength(0) * dat.GetLength(1) : 0);
      }

   }
}
