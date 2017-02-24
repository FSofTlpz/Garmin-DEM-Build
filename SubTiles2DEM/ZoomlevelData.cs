using System.Collections.Generic;

namespace SubTiles2DEM {
   /// <summary>
   /// Zusammenfassung der Daten
   /// </summary>
   class ZoomlevelData {
      /// <summary>
      /// Zoomlevel-Definition (Tabelleneintrag)
      /// </summary>
      public ZoomlevelTableitem Tableitem { get; set; }

      /// <summary>
      /// Liste der Subtiles
      /// </summary>
      public List<Subtile> Subtiles { get; set; }


      public ZoomlevelData() {
         Tableitem = new ZoomlevelTableitem();
         Subtiles = new List<Subtile>();
      }

      public uint GetHeightDataAreaSize() {
         uint size = 0;
         for (int i = 0; i < Subtiles.Count; i++)
            size += (uint)Subtiles[i].DataLength;
         return size;
      }

      public int GetTableAreaSize() {
         return Tableitem.SubtileTableitemSize * Subtiles.Count;
      }

      public override string ToString() {
         return string.Format("Anzahl Subtiles: {0}", Subtiles.Count);
      }
   }

}
