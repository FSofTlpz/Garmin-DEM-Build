using System;

namespace BuildDEMFile {
   /// <summary>
   /// Hier werden die Höhendaten und die Daten des Tabelleneintrages eines Subtiles zusammengefasst.
   /// </summary>
   class Subtile {

      /// <summary>
      /// Werte die gleich (oder größer) sind, werden als "Höhe unbekannt" registriert
      /// </summary>
      public const int UNDEF = short.MaxValue;


      /// <summary>
      /// int-Daten
      /// </summary>
      Data2Dim dat;

      #region zusätzliche Infos (müssen nicht gesetzt sein, wenn die Daten im Konstruktor geliefert werden; werden nur für da "späte" Datenholen benötigt))

      /// <summary>
      /// geplante linke geogr. Länge
      /// </summary>
      public double PlannedLeft { get; private set; }

      /// <summary>
      /// geplante obere geogr. Breite
      /// </summary>
      public double PlannedTop { get; private set; }

      /// <summary>
      /// geplanter Punktabstand als geogr. Länge
      /// </summary>
      public double PlannedLonDistance { get; private set; }

      /// <summary>
      /// geplanter Punktabstand als geogr. Breite
      /// </summary>
      public double PlannedLatDistance { get; private set; }

      /// <summary>
      /// geplanter Punktanzahl waagerecht
      /// </summary>
      public int PlannedWidth { get; private set; }

      /// <summary>
      /// geplanter Punktanzahl senkrecht
      /// </summary>
      public int PlannedHeight { get; private set; }

      #endregion

      /// <summary>
      /// Kachelbreite
      /// </summary>
      public int Width {
         get {
            return dat != null ? dat.Width : PlannedWidth;
         }
      }

      /// <summary>
      /// Kachelhöhe
      /// </summary>
      public int Height {
         get {
            return dat != null ? dat.Height : PlannedHeight;
         }
      }

      /// <summary>
      /// codierte Höhendaten
      /// </summary>
      public byte[] CodedData { get; private set; }

      /// <summary>
      /// Basishöhe
      /// </summary>
      public int BaseHeight {
         get {
            return Tableitem.Baseheight;
         }
      }

      /// <summary>
      /// max. Höhendifferenz
      /// </summary>
      public int MaxDiffHeight {
         get {
            return Tableitem.Diff;
         }
      }

      /// <summary>
      /// Codierungstyp
      /// </summary>
      public byte Codingtype {
         get {
            return Tableitem.Type;
         }
      }

      /// <summary>
      /// Tabelleneintrages des Subtiles
      /// </summary>
      public SubtileTableitem Tableitem { get; set; }

      /// <summary>
      /// Länge der Höhendaten
      /// </summary>
      public int DataLength {
         get {
            return CodedData != null ? CodedData.Length : 0;
         }
      }


      public Subtile(Data2Dim intdata, SubtileTableitem tableitem = null) {
         CodedData = null;
         dat = intdata;
         if (tableitem == null)
            Tableitem = new SubtileTableitem();
         else
            Tableitem = tableitem;
      }

      public Subtile(double left, double top, double londist, double latdist, int loncount, int latcount) : this(null) {
         PlannedLeft = left;
         PlannedTop = top;
         PlannedLonDistance = londist;
         PlannedLatDistance = latdist;
         PlannedWidth = loncount;
         PlannedHeight = latcount;
      }

      /// <summary>
      /// encodiert die Daten (die Daten werden dabei verändert!)
      /// </summary>
      /// <param name="intdata">zu encodierende Daten</param>
      public void Encoding(Data2Dim intdata = null) {
         if (intdata != null)
            dat = new Data2Dim(intdata);

         if (dat == null)
            throw new Exception("Keine Daten zum Encodieren im Subtile vorhanden.");

         int min, max;
         bool bWithIntMax = dat.GetMinMax(out min, out max);

         // Daten normieren
         if (min >= UNDEF) { // alle Werte sind "ungültig"
            dat.ReplaceBigValues(UNDEF, 1);
            Tableitem.Baseheight = 0;
            Tableitem.Diff = 1;
            Tableitem.Type = 2;
         } else {
            Tableitem.Type = 0;  // alle Werte sind gültig
            if (bWithIntMax) { // nicht alle, aber einige Werte sind "ungültig"
               Tableitem.Type = 2;
               dat.ReplaceBigValues(UNDEF, ++max);
            }
            Tableitem.Diff = (ushort)(max - min);
            Tableitem.Baseheight = (short)min;
            dat.AddValue(-BaseHeight);
         }

         if (max == min) { // eine Ebene ist i.A. ohne Daten (MaxDiffHeight == 0)
            CodedData = new byte[0];
         } else {

            Encoder.TileEncoder enc = new Encoder.TileEncoder(MaxDiffHeight, Codingtype, Width, Height, dat.GetAll());
            bool bTileIsFull;
            do {
               enc.ComputeNext(out bTileIsFull);
            } while (!bTileIsFull);
            CodedData = enc.GetCodedBytes();

         }
      }

      public override string ToString() {
         return string.Format("{0} x {1}, {2} Bytes, {3} (geplant NW {4}/{5}, {6}x{7})",
                              Width,
                              Height,
                              CodedData == null ? 0 : CodedData.Length,
                              Tableitem,
                              PlannedLeft,
                              PlannedTop,
                              Width,
                              Height);
      }

   }
}
