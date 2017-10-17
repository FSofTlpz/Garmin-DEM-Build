using System.Collections.Generic;

namespace BuildDEMFile {
   /// <summary>
   /// Zusammenfassung der Daten
   /// </summary>
   class ZoomlevelData {

      /// <summary>
      /// Definition dieses Zoomlevels (Tabelleneintrag)
      /// </summary>
      public ZoomlevelTableitem Tableitem { get; set; }

      /// <summary>
      /// Liste aller Subtiles dieses Zoomlevels
      /// </summary>
      public List<Subtile> Subtiles { get; set; }


      public ZoomlevelData() {
         Tableitem = new ZoomlevelTableitem();
         Subtiles = new List<Subtile>();
      }

      /// <summary>
      /// liefert die Größe des notwendigen Speicherbereiches für die Höhendaten
      /// </summary>
      /// <returns></returns>
      public uint GetHeightDataAreaSize() {
         uint size = 0;
         for (int i = 0; i < Subtiles.Count; i++)
            size += (uint)Subtiles[i].DataLength;
         return size;
      }

      /// <summary>
      /// liefert die Größe des notwendigen Speicherbereiches für die Tabelle
      /// </summary>
      /// <returns></returns>
      public uint GetTableAreaSize() {
         return (uint)(Tableitem.SubtileTableitemSize * Subtiles.Count);
      }

      public override string ToString() {
         return string.Format("Anzahl Subtiles: {0}", Subtiles.Count);
      }
   }

}
