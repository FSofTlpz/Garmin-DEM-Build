using System;

namespace BuildDEMFile {

   /// <summary>
   /// This class hold the DEM data for 1°x1°. The number of columns can be different from the number of rows. 
   /// You need a derived class for filling in the data.
   /// </summary>
   abstract public class DEM1x1 : IDisposable {

      /// <summary>
      /// int value for "no data"
      /// </summary>
      public const int NOVALUE = -32768;

      /// <summary>
      /// double value for "no data" (from interpolation)
      /// </summary>
      public const double NOVALUED = double.MinValue;

      /// <summary>
      /// feet in meter
      /// </summary>
      public const double FOOT = 0.3048;

      public enum InterpolationType {
         standard,
         bicubic_catmull_rom,
      }


      /// <summary>
      /// left border
      /// </summary>
      public double Left { get; protected set; }
      /// <summary>
      /// lower border
      /// </summary>
      public double Bottom { get; protected set; }

      /// <summary>
      /// number of Rows (e.g. 1201, 3601, ...)
      /// </summary>
      public int Rows { get; protected set; }
      /// <summary>
      /// number of Columns (e.g. 1201, 3601, ...)
      /// </summary>
      public int Columns { get; protected set; }
      /// <summary>
      /// horizontal distance between 2 points
      /// </summary>
      public double DeltaX { get { return Width / (Columns - 1); } }
      /// <summary>
      /// vertical distance between 2 points
      /// </summary>
      public double DeltaY { get { return Width / (Rows - 1); } }

      /// <summary>
      /// minimal value
      /// </summary>
      public int Minimum { get; protected set; }
      /// <summary>
      /// maximal value
      /// </summary>
      public int Maximum { get; protected set; }
      /// <summary>
      /// number of unvalid values
      /// </summary>
      public long NotValid { get; protected set; }


      /// <summary>
      /// upper border
      /// </summary>
      public double Top { get { return Bottom + Height; } }
      /// <summary>
      /// right border
      /// </summary>
      public double Right { get { return Left + Width; } }
      /// <summary>
      /// width
      /// </summary>
      public double Width { get { return 1; } }
      /// <summary>
      /// height
      /// </summary>
      public double Height { get { return 1; } }

      /// <summary>
      /// dem data-array
      /// </summary>
      protected short[] data;


      public DEM1x1() {
         Left = 0;
         Bottom = 0;
         data = new short[0];
      }

      public DEM1x1(double left, double bottom) {
         Left = left;
         Bottom = bottom;
         data = new short[0];
      }


      /// <summary>
      /// set the data array
      /// </summary>
      abstract public void SetDataArray();


      /// <summary>
      /// get a value (coodinate origin left-top)
      /// </summary>
      /// <param name="row"></param>
      /// <param name="col"></param>
      /// <returns></returns>
      public int Get(int row, int col) {
         if (row < 0 || Rows <= row ||
             col < 0 || Columns <= col)
            throw new Exception(string.Format("({0}, {1}) unvalid row and/or column ({2}x{3})", col, row, Columns, Rows));
         return data[row * Columns + col];
      }

      /// <summary>
      /// get a value (coodinate origin left-bottom)
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public int Get4XY(int x, int y) {
         return Get(Rows - 1 - y, x);
      }

      //int get4XY(int x, int y) {
      //   return data[(Rows - 1 - y) * Columns + x];
      //}

      /// <summary>
      /// exist 1 or more valid values
      /// </summary>
      /// <returns></returns>
      public bool HasValidValues() {
         return (Minimum != NOVALUE || Maximum != NOVALUE);
      }

      /// <summary>
      /// get the interpolated value (or <see cref="NOVALUED"/>)
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="intpol"></param>
      /// <returns></returns>
      public double InterpolatedHeight(double lon, double lat, InterpolationType intpol) {
         double h = NOVALUED;

         lon -= Left;
         lat -= Bottom; // Koordinaten auf die Ecke links unten bezogen

         switch (intpol) {
            case InterpolationType.standard:
               if (0.0 <= lon && lon <= 1.0 &&
                   0.0 <= lat && lat <= 1.0) {
                  // x-y-Index des Eckpunktes links unten des umschließenden Quadrats bestimmen
                  int x = (int)(lon / DeltaX);
                  int y = (int)(lat / DeltaY);
                  if (x == Columns - 1) // liegt auf rechtem Rand
                     x--;
                  if (y == Rows - 1) // liegt auf oberem Rand
                     y--;

                  // lon/lat jetzt bzgl. der Ecke links-unten des umschließenden Quadrats bilden (0 .. <Delta)
                  double delta_lon = lon - x * DeltaX;
                  double delta_lat = lat - y * DeltaY;

                  if (delta_lon == 0) {            // linker Rand
                     if (delta_lat == 0)
                        h = Get4XY(x, y);          // Eckpunkt links unten
                     else if (delta_lat >= DeltaY)
                        h = Get4XY(x, y + 1);      // Eckpunkt links oben (eigentlich nicht möglich)
                     else
                        h = LinearInterpolatedHeight(Get4XY(x, y),
                                                     Get4XY(x, y + 1),
                                                     delta_lat / DeltaY);
                  } else if (delta_lon >= DeltaX) { // rechter Rand (eigentlich nicht möglich)
                     if (delta_lat == 0)
                        h = Get4XY(x + 1, y);      // Eckpunkt rechts unten
                     else if (delta_lat >= DeltaY)
                        h = Get4XY(x + 1, y + 1);  // Eckpunkt rechts oben (eigentlich nicht möglich)
                     else
                        h = LinearInterpolatedHeight(Get4XY(x + 1, y),
                                                     Get4XY(x + 1, y + 1),
                                                     delta_lat / DeltaY);
                  } else if (delta_lat == 0) {     // unterer Rand (außer Eckpunkt)
                     h = LinearInterpolatedHeight(Get4XY(x, y),
                                                  Get4XY(x + 1, y),
                                                  delta_lon / DeltaX);
                  } else if (delta_lat >= DeltaY) { // oberer Rand (außer Eckpunkt) (eigentlich nicht möglich)
                     h = LinearInterpolatedHeight(Get4XY(x, y + 1),
                                                  Get4XY(x + 1, y + 1),
                                                  delta_lon / DeltaX);

                  } else {                         // Punkt innerhalb des Rechtecks

                     //int leftbottom, rightbottom, righttop, lefttop;
                     //Get4XYSquare(x, y, out leftbottom, out rightbottom, out righttop, out lefttop);  // etwas schneller als die obere Version

                     int idx = (Rows - 1 - y) * Columns; // Anfang der unteren Zeile
                     idx += x;
                     int leftbottom = data[idx++];
                     int rightbottom = data[idx];
                     idx -= Columns;
                     int righttop = data[idx--];
                     int lefttop = data[idx];
                     h = InterpolatedHeightInNormatedRectangle_New(delta_lon / DeltaX,
                                                                   delta_lat / DeltaY,
                                                                   lefttop,
                                                                   righttop,
                                                                   rightbottom,
                                                                   leftbottom);
                  }
               }
               break;

            case InterpolationType.bicubic_catmull_rom:
               if (0.0 <= lon && lon <= 1.0 &&
                   0.0 <= lat && lat <= 1.0) {

                  if (Columns >= 4 && Rows >= 4) {
                     // x-y-Index des Punktes link-unterhalb bestimmen
                     int x = (int)(lon / DeltaX);
                     int y = (int)(lat / DeltaY);
                     // x-y-Index des Punktes link-unterhalb dieses Punktes bestimmen
                     x--;
                     y--;
                     if (x < 0)
                        x = 0;
                     else if (x >= Columns - 4)
                        x = Columns - 4;
                     if (y < 0)
                        y = 0;
                     else if (y >= Rows - 4)
                        y = Rows - 4;

                     double[][] p = new double[4][];
                     for (int i = 0; i < 4; i++)
                        p[i] = new double[] {   Get4XY(x, y + 3-i),
                                             Get4XY(x + 1, y + 3-i),
                                             Get4XY(x + 2, y + 3-i),
                                             Get4XY(x + 3, y + 3-i) };

                     bool allvalid = true;
                     for (int i = 0; i < 4; i++)
                        for (int j = 0; j < 4; j++)
                           if (p[i][j] == NOVALUE) {
                              allvalid = false;
                              i = j = 5;
                           }

                     if (allvalid)
                        h = Dim2CubicInterpolation(p, lon / DeltaX - x, lat / DeltaY - y);
                     else
                        h = InterpolatedHeight(lon + Left, lat + Bottom, InterpolationType.standard);
                  } else
                     h = InterpolatedHeight(lon + Left, lat + Bottom, InterpolationType.standard);
               }
               break;
         }

         return h;
      }

      //protected void Get4XYSquare1(int x, int y,
      //                            out int leftbottom, out int rightbottom, out int righttop, out int lefttop) {
      //   //if (xleft < 0 || Columns <= xleft + 1 ||
      //   //    ybottom < 0 || Rows <= ybottom + 1)
      //   //   throw new Exception(string.Format("({0}, {1}) is out of area ({2}x{3})", xleft, ybottom, Columns, Rows));
      //   int idx = (Rows - 1 - y) * Columns; // Anfang der unteren Zeile
      //   idx += x;
      //   leftbottom = data[idx++];
      //   rightbottom = data[idx];
      //   idx -= Columns;
      //   righttop = data[idx--];
      //   lefttop = data[idx];
      //}

      /// <summary>
      /// resize the internal datatable
      /// </summary>
      /// <param name="newcols"></param>
      /// <param name="newrows"></param>
      /// <param name="intpol"></param>
      /// <returns></returns>
      public bool ResizeDatatable(int newcols, int newrows, InterpolationType intpol) {
         if (newcols < 3 || newrows < 3)
            throw new Exception("New tablesize less 3 not permitted.");

         if (newcols != Columns ||
             newrows != Rows) {
            NotValid = 0;
            short[] newdata = new short[newcols * newcols];
            double deltax = 1.0 / (newcols - 1);
            double deltay = 1.0 / (newrows - 1);

            for (int row = 0; row < newrows; row++) {
               for (int col = 0; col < newcols; col++) {
                  double hi = InterpolatedHeight(Left + col * deltax, Bottom + 1 - row * deltay, intpol);
                  if (hi == NOVALUED) {
                     NotValid++;
                     newdata[row * newcols + col] = NOVALUE;
                  } else {
                     short h = (short)Math.Round(hi);
                     if (Maximum < h)
                        Maximum = h;
                     else if (Minimum > h)
                        Minimum = h;
                     newdata[row * newcols + col] = h;
                  }
               }
            }
            data = newdata;
            Columns = newcols;
            Rows = newrows;
            return true;
         }
         return false;
      }

      #region Bilinear Interpolation

      /// <summary>
      /// get surrounding 4 point
      /// </summary>
      /// <param name="xleft"></param>
      /// <param name="ybottom"></param>
      /// <param name="leftbottom"></param>
      /// <param name="rightbottom"></param>
      /// <param name="righttop"></param>
      /// <param name="lefttop"></param>
      protected void Get4XYSquare(int xleft, int ybottom,
                                  out int leftbottom, out int rightbottom, out int righttop, out int lefttop) {
         //if (xleft < 0 || Columns <= xleft + 1 ||
         //    ybottom < 0 || Rows <= ybottom + 1)
         //   throw new Exception(string.Format("({0}, {1}) is out of area ({2}x{3})", xleft, ybottom, Columns, Rows));
         int idx = (Rows - 1 - ybottom) * Columns; // Anfang der unteren Zeile
         idx += xleft;
         leftbottom = data[idx++];
         rightbottom = data[idx];
         idx -= Columns;
         righttop = data[idx--];
         lefttop = data[idx];
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
            return q1 < .5 ? NOVALUED : // wenn dichter am NOVALUE, dann NOVALUE sonst gleiche Höhe wie der andere Punkt
                     h2 == NOVALUE ? NOVALUED : h2;
         if (h2 == NOVALUE)
            return q1 > .5 ? NOVALUED :
                     h1 == NOVALUE ? NOVALUED : h1;
         return h1 + q1 * (h2 - h1);
      }

      /// <summary>
      /// die Höhe für den Punkt P im umschließenden Rechteck (normierte Seitenlänge 1) aus 4 Eckpunkten wird interpoliert
      /// </summary>
      /// <param name="qx">Abstand P vom linken Rand des Rechtecks (Bruchteil 0..1)</param>
      /// <param name="qy">Abstand P vom unteren Rand des Rechtecks (Bruchteil 0..1)</param>
      /// <param name="hlt">Höhe links oben</param>
      /// <param name="hrt">Höhe rechts oben</param>
      /// <param name="hrb">Höhe rechts unten</param>
      /// <param name="hlb">Höhe links unten</param>
      /// <returns></returns>
      double InterpolatedHeightInNormatedRectangle_New(double qx, double qy, int hlt, int hrt, int hrb, int hlb) {
         int novalue = 0;
         if (hlb == NOVALUE)
            novalue++;
         if (hlt == NOVALUE)
            novalue++;
         if (hrt == NOVALUE)
            novalue++;
         if (hrb == NOVALUE)
            novalue++;

         switch (novalue) {
            case 0: // bilinear, standard
               return (1 - qy) * (hlb + qx * (hrb - hlb)) + qy * (hlt + qx * (hrt - hlt));

            case 1:
               if (hlb == NOVALUE) {            //    valid triangle \|

                  if (qx >= 1 - qy)
                     // z = z1 + (1 - nx) * (z3 - z1) + (1 - ny) * (z2 - z1)
                     return hrt + (1 - qx) * (hlt - hrt) + (1 - qy) * (hrb - hrt);

               } else if (hlt == NOVALUE) {     //    valid triangle /|

                  if (qx <= qy)
                     // z = z1 + (1 - nx) * (z2 - z1) + ny * (z3 - z1)
                     return hrb + (1 - qx) * (hlb - hrb) + qy * (hrt - hrb);

               } else if (hrt == NOVALUE) {     //    valid triangle |\

                  if (qx <= 1 - qy)
                     //z = z1 + nx * (z3 - z1) + ny * (z2 - z1)
                     return hlb + qx * (hrb - hlb) + qy * (hlt - hlb);

               } else if (hrb == NOVALUE) {     //    valid triangle |/

                  if (qx <= qy)
                     // z = z1 + nx * (z2 - z1) + (1 - ny) * (z3 - z1)
                     return hlt + qx * (hrt - hlt) + (1 - qy) * (hlb - hlt);

               }
               break;

            case 2:
               if (hlb != NOVALUE && hrt != NOVALUE)  // diagonal
                  return (hlb + hrt) / 2.0;
               if (hlt != NOVALUE && hrb != NOVALUE)  // diagonal
                  return (hlt + hrb) / 2.0;
               return NOVALUED;

            default:
               return NOVALUED;
         }

         return NOVALUED;
      }

      double InterpolatedHeightInNormatedRectangle_Old(double qx, double qy, int hlt, int hrt, int hrb, int hlb) {
         if (hlb == NOVALUE ||
             hrt == NOVALUE)
            return NOVALUED; // keine Berechnung möglich

         /* In welchem Dreieck liegt der Punkt? 
          *    oben  +-/
          *          |/
          *          
          *    unten  /|
          *          /-+
          */
         if (qy >= qx) { // oberes Dreieck aus hlb, hrt und hlt (Anstieg py/px ist größer als height/width)

            if (hlt == NOVALUED)
               return NOVALUED;

            // hlt als Koordinatenursprung normieren; mit hrt und hlb 3 Punkte einer Ebene (3-Punkt-Gleichung)
            hrt -= hlt;
            hlb -= hlt;
            qy -= 1;

            return hlt + qx * hrt - qy * hlb;

         } else { // unteres Dreieck aus hlb, hrb und hrt

            if (hrb == NOVALUED)
               return NOVALUED;

            // hrb als Koordinatenursprung normieren; mit hrt und hlb 3 Punkte einer Ebene (3-Punkt-Gleichung)
            hrt -= hrb;
            hlb -= hrb;
            qx -= 1;

            return hrb - qx * hlb + qy * hrt;
         }
      }

      #endregion

      #region Cubic Interpolation (Catmull–Rom spline)

      /// <summary>
      /// 1-dimensionale kubische Interpolation mit Catmull–Rom Spline
      /// <para>
      /// Die Gleichung vereinfacht nur den folgenden Zusammenhang:
      /// 
      /// p0  p1   p2    p3
      /// -------------------------
      /// 0    1    0     0     q^0 
      /// -t   0    t     0     q^1 
      /// 2t  t-3  3-2t  -t     q^2 
      /// -t  2-t  t-2    t     q^3
      /// 
      /// üblich mit t=0.5 (auch mit 1 gesehen)
      /// 
      /// q^3 * (-0.5*p0 + 1.5*p1 - 1.5*p2 + 0.5*p3) +
      /// q^2 * (     p0 - 1.5*p1 + 2.5*p2 - 0.5*p3) +
      /// q^1 * (-0.5*p0          + 0.5*p2         ) +
      ///                      p1
      /// </para>
      /// </summary>
      /// <param name="p">4 Werte (bekannte Stützpunkte), die den Spline definieren (abgesehen von t).</param>
      /// <param name="q">Für diesen Wert (als Faktor 0..1) zwischen den Stützpunkten ist der Funktionswert gesucht.</param>
      /// <returns></returns>
      static double Dim1CubicInterpolation(double[] p, double q) {
         return p[1] + 0.5 * q * (p[2] - p[0] + q * (2 * p[0] - 5 * p[1] + 4 * p[2] - p[3] + q * (3 * (p[1] - p[2]) + p[3] - p[0])));
      }

      /// <summary>
      /// 2-dimensionale (bi)kubische Interpolation mit Catmull–Rom Spline
      /// </summary>
      /// <param name="p">4x4 Stützpunkte</param>
      /// <param name="qx">Faktor 0..1 des x-Wertes der Position der gesuchten Höhe</param>
      /// <param name="qy">Faktor 0..1 des y-Wertes der Position der gesuchten Höhe</param>
      /// <returns></returns>
      static double Dim2CubicInterpolation(double[][] p, double qx, double qy) {
         return Dim1CubicInterpolation(new double[] {
                                          Dim1CubicInterpolation(p[0], qy),   // Array der y-Werte für x=0
                                          Dim1CubicInterpolation(p[1], qy),
                                          Dim1CubicInterpolation(p[2], qy),
                                          Dim1CubicInterpolation(p[3], qy)},  // Array der y-Werte für x=1
                                       qx);
      }

      //double BiCubicInterpolation(int x, int y, double qx, double qy) {
      double BiCubicInterpolation(double lon, double lat) {
         double[][] p = new double[4][];


         return 0; // Dim2CubicInterpolation(p, qx, qy);
      }



      #endregion

      /// <summary>
      /// get the standard basefilename for this object with upper chars (without extension)
      /// </summary>
      /// <returns></returns>
      public string GetStandardBasefilename() {
         return GetStandardBasefilename((int)Math.Round(Left), (int)Math.Round(Bottom));
      }

      /// <summary>
      /// get the standard basefilename with upper chars (without extension)
      /// </summary>
      /// <param name="left"></param>
      /// <param name="bottom"></param>
      /// <returns></returns>
      static public string GetStandardBasefilename(int left, int bottom) {
         string name;
         if (left >= 0)
            name = string.Format("N{0:d2}", bottom);
         else
            name = string.Format("S{0:d2}", -bottom);

         if (left >= 0)
            name += string.Format("E{0:d3}", left);
         else
            name += string.Format("W{0:d3}", -left);

         return name;
      }


      #region Implementierung der IDisposable-Schnittstelle

      ~DEM1x1() {
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

      public override string ToString() {
         return string.Format("DEM1x1: {0}{1}° {2}{3}°, {4}x{5}, {6}m..{7}m, unvalid values: {8} ({9}%)",
            Bottom >= 0 ? "N" : "S", Bottom >= 0 ? Bottom : -Bottom,
            Left >= 0 ? "E" : "W", Left >= 0 ? Left : -Left,
            Rows, Columns,
            Minimum, Maximum,
            NotValid, (100.0 * NotValid) / (Rows * Columns));
      }

   }
}
