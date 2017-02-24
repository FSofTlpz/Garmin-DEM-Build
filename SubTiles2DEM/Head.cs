using System.IO;
using System.Text;

namespace SubTiles2DEM {
   /// <summary>
   /// Header der DEM-Datei
   /// </summary>
   class Head {

      /// <summary>
      /// Headerlänge
      /// </summary>
      public ushort Length { get; private set; }
      /// <summary>
      /// Identifizierungstext
      /// </summary>
      public string Identification { get; private set; }
      /// <summary>
      /// unbekannt auf 0x0C
      /// </summary>
      public byte Unknown0C { get; set; }
      /// <summary>
      /// gesperrt
      /// </summary>
      public byte Locked { get; private set; }
      /// <summary>
      /// Datum
      /// </summary>
      public byte[] Date { get; set; }

      /// <summary>
      /// Daten in Fuß oder Meter
      /// </summary>
      public int Footflag { get; set; }
      /// <summary>
      /// Anzahl der Zoomlevel
      /// </summary>
      public ushort Zoomlevel { get; set; }
      /// <summary>
      /// unbekannt auf 0x1B
      /// </summary>
      public int Unknown1B { get; set; }
      /// <summary>
      /// Datensatzgröße für die Zoomlevel
      /// </summary>
      public ushort ZoomlevelDatasetSize { get; set; }
      /// <summary>
      /// Pointer auf Zoomlevel-Tabelle (i.A. am Ende der Datei; bezogen auf den Dateianfang)
      /// </summary>
      public uint PtrZoomlevelTable { get; set; }
      /// <summary>
      /// unbekannt auf 0x25
      /// </summary>
      public int Unknown25 { get; set; }

      public Head() {
         Length = 0x29;
         Identification = "GARMIN DEM";
         Unknown0C = 1;
         Locked = 0;
         Date = new byte[] { 0xE1, 0x7, 0x1, 0x1, 0x1, 0x1, 0x1 }; // 1.1.2017 

         Footflag = 1;
         Unknown1B = 0;
         Zoomlevel = 1;
         ZoomlevelDatasetSize = 0x3C;
         PtrZoomlevelTable = (uint)Length;
         Unknown25 = 1;
      }

      public void Write(BinaryWriter w) {
         w.Write(Length);
         w.Write(Encoding.ASCII.GetBytes(Identification));
         w.Write(Unknown0C);
         w.Write(Locked);
         w.Write(Date);

         w.Write(Footflag);
         w.Write(Zoomlevel);
         w.Write(Unknown1B);
         w.Write(ZoomlevelDatasetSize);
         w.Write(PtrZoomlevelTable);
         w.Write(Unknown25);
      }

   }
}
