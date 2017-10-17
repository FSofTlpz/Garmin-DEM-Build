using System;
using System.Collections.Generic;

namespace BuildDEMFile {

   /// <summary>
   /// eine Klasse für 2-dim-Integer-Arrays
   /// </summary>
   class Data2Dim {

      List<List<int>> rawdata;

      public int Width {
         get {
            return rawdata.Count > 0 ? rawdata[0].Count : 0;
         }
      }

      public int Height {
         get {
            return rawdata.Count;
         }
      }


      public Data2Dim() {
         rawdata = new List<List<int>>();
      }

      public Data2Dim(Data2Dim dat) : this() {
         for (int i = 0; i < dat.Height; i++)
            rawdata.Add(new List<int>(dat.rawdata[i]));
      }

      public Data2Dim(int[,] dat) : this() {
         for (int i = 0; i < dat.GetLength(1); i++) {
            List<int> row = new List<int>();
            for (int j = 0; j < dat.GetLength(0); j++)
               row.Add(dat[j, i]);
            rawdata.Add(row);
         }
      }

      public Data2Dim(List<int> dat, int width) : this() {
         int start = 0;
         while (start + width <= dat.Count) {
            rawdata.Add(new List<int>(dat.GetRange(start, width)));
            start += width;
         }
      }


      /// <summary>
      /// liefert den Wert
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public int GetValue(int x, int y) {
         if (0 <= x && x < Width &&
             0 <= y && y < Height)
            return rawdata[y][x];
         throw new Exception(string.Format("ungültiger Index ({0},{1}) für {2}", x, y, this));
      }

      /// <summary>
      /// setzt den Wert
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <param name="value"></param>
      public void SetValue(int x, int y, int value) {
         if (0 <= x && x < Width &&
             0 <= y && y < Height)
            rawdata[y][x] = value;
         throw new Exception(string.Format("ungültiger Index ({0},{1}) für {2}", x, y, this));
      }

      /// <summary>
      /// liefert den kleinsten und größten Wert sowie als Rückgabewert das Vorhandensein von short.MaxValue
      /// </summary>
      /// <returns></returns>
      public bool GetMinMax(out int min, out int max) {
         bool bWithIntMax = false;
         min = int.MaxValue;
         max = int.MinValue;
         for (int i = 0; i < Height; i++)
            for (int j = 0; j < Width; j++) {
               min = Math.Min(min, rawdata[i][j]);
               if (rawdata[i][j] >= short.MaxValue) {
                  bWithIntMax = true;
               } else
                  max = Math.Max(max, rawdata[i][j]);
            }
         return bWithIntMax;
      }

      /// <summary>
      /// alle Werte, die größer oder gleich sind, ersetzen
      /// </summary>
      /// <param name="oldvalue"></param>
      /// <param name="newvalue"></param>
      public void ReplaceBigValues(int oldvalue, int newvalue) {
         for (int i = 0; i < Height; i++)
            for (int j = 0; j < Width; j++)
               if (rawdata[i][j] >= oldvalue)
                  rawdata[i][j] = newvalue;
      }

      /// <summary>
      /// addiert zu allen Werten v
      /// </summary>
      /// <param name="v"></param>
      public void AddValue(int v) {
         for (int i = 0; i < Height; i++)
            for (int j = 0; j < Width; j++)
               rawdata[i][j] += v;
      }

      /// <summary>
      /// vergrößert die Zeilenlänge und füllt mit dem vorgegebenen Wert auf
      /// </summary>
      /// <param name="increase"></param>
      /// <param name="dummyval"></param>
      public void IncreaseWidth(int increase, int dummyval) {
         if (increase > 0) {
            int[] appdata = new int[increase];
            for (int i = 0; i < appdata.Length; i++)
               appdata[i] = dummyval;
            for (int i = 0; i < rawdata.Count; i++)
               rawdata[i].AddRange(appdata);
         }
      }

      /// <summary>
      /// vergrößert die Zeilenlänge mit dem jeweils letzten Zeilenwert
      /// </summary>
      /// <param name="increase"></param>
      public void IncreaseWidth(int increase) {
         if (increase > 0) {
            int[] appdata = new int[increase];
            for (int i = 0; i < rawdata.Count; i++) {
               int lastval = rawdata[i][rawdata[i].Count - 1];
               for (int j = 0; j < appdata.Length; j++)
                  appdata[j] = lastval;
               rawdata[i].AddRange(appdata);
            }
         }
      }

      /// <summary>
      /// liefert alle Werte
      /// </summary>
      /// <returns></returns>
      public List<int> GetAll() {
         List<int> d = new List<int>();
         for (int i = 0; i < Height; i++)
            d.AddRange(rawdata[i]);
         return d;
      }

      /// <summary>
      /// liefert einen Bereich
      /// </summary>
      /// <param name="startx"></param>
      /// <param name="starty"></param>
      /// <param name="dx"></param>
      /// <param name="dy"></param>
      /// <returns></returns>
      public List<int> GetRange(int startx, int starty, int dx, int dy) {
         if (0 <= startx && dx > 0 && startx + dx <= Width &&
             0 <= starty && dy > 0 && starty + dy <= Height) {
            List<int> d = new List<int>();
            for (int i = starty; i < starty + dy; i++)
               d.AddRange(rawdata[i].GetRange(startx, dx));
            return d;
         } else
            throw new Exception(string.Format("ungültiger Bereich ({0},{1} / {2},{3}) für {4}", startx, starty, dx, dy, this));
      }

      public override string ToString() {
         return string.Format("{0} x {1}", Width, Height);
      }

   }
}
