using System.IO;

namespace SubTiles2DEM {
   /// <summary>
   /// Hier werden die Höhendaten und die Daten des Tabelleneintrages eines Subtiles zusammengefasst.
   /// </summary>
   class Subtile {

      /// <summary>
      /// Tabelleneintrages des Subtiles
      /// </summary>
      public SubtileTableitem Tableitem { get; set; }
      /// <summary>
      /// codierte Höhendaten
      /// </summary>
      public byte[] Data { get; set; }
      /// <summary>
      /// Länge der Höhendaten
      /// </summary>
      public int DataLength {
         get {
            return Data.Length;
         }
      }


      public Subtile(byte[] data, SubtileTableitem tableitem = null) {
         SetData(data, tableitem);
      }

      public Subtile(string file, SubtileTableitem tableitem = null) {
         using (BinaryReader r = new BinaryReader(File.OpenRead(file))) {
            SetData(r.ReadBytes((int)r.BaseStream.Length), tableitem);
         }
      }

      void SetData(byte[] data, SubtileTableitem tableitem) {
         if (data == null || data.Length == 0)
            Data = new byte[0];
         else {
            Data = new byte[data.Length];
            data.CopyTo(Data, 0);
         }

         if (tableitem == null)
            Tableitem = new SubtileTableitem();
         else
            Tableitem = tableitem;
      }

      public override string ToString() {
         return string.Format("{0}, DataLength {1}", Tableitem, DataLength);
      }

   }
}
