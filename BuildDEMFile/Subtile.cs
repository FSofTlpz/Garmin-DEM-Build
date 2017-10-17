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

      /// <summary>
      /// Kachelbreite
      /// </summary>
      public int Width {
         get {
            return dat.Width;
         }
      }

      /// <summary>
      /// Kachelhöhe
      /// </summary>
      public int Height {
         get {
            return dat.Height;
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

      /// <summary>
      /// encodiert die Daten (die Daten werden dabei verändert!)
      /// </summary>
      public void Encoding() {
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
         return string.Format("{0} x {1}, {2} Bytes, {3}", Width, Height, CodedData == null ? 0 : CodedData.Length, Tableitem);
      }

   }
}
