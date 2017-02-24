using System.IO;

namespace SubTiles2DEM {
   /// <summary>
   /// Tabelleneintrag für die Zoomlevel-Tabelle
   /// </summary>
   class ZoomlevelTableitem {

      const ulong DEG_UNIT_FACTOR = 1UL << 32;

      /// <summary>
      /// Nummer des Eintrages (0, ...)
      /// </summary>
      public ushort No { get; set; }
      /// <summary>
      /// Anzahl der Datenpunkte waagerecht
      /// </summary>
      public int PointsHoriz { get; set; }
      /// <summary>
      /// Anzahl der Datenpunkte senkrecht
      /// </summary>
      public int PointsVert { get; set; }
      /// <summary>
      /// unbekannt auf 0x0A
      /// </summary>
      public int Unknown0A { get; set; }
      /// <summary>
      /// unbekannt auf 0x0E
      /// </summary>
      public int Unknown0E { get; set; }
      /// <summary>
      /// unbekannt auf 0x12
      /// </summary>
      public short Unknown12 { get; set; }
      /// <summary>
      /// größter Subtile-Index waagerecht (Anzahl -1)
      /// </summary>
      public int MaxIdxHoriz { get; set; }
      /// <summary>
      /// größter Subtile-Index senkrecht (Anzahl -1)
      /// </summary>
      public int MaxIdxVert { get; set; }
      /// <summary>
      /// Struktur des Subtile-Tabelleneintrags (Länge der einzelnen Elemente)
      /// </summary>
      public short Structure { get; private set; }
      /// <summary>
      /// 1..3
      /// </summary>
      public int Structure_OffsetSize {
         get {
            return 1 + (Structure & 0x3);
         }
         set {
            switch (value) {
               case 1: Structure = (short)((Structure & 0xFFFC)); break;
               case 2: Structure = (short)((Structure & 0xFFFC) | 0x1); break;
               case 3: Structure = (short)((Structure & 0xFFFC) | 0x2); break;
            }
         }
      }
      /// <summary>
      /// 1, 2
      /// </summary>
      public int Structure_BaseheightSize {
         get {
            return 1 + ((Structure >> 2) & 0x1);
         }
         set {
            switch (value) {
               case 1: Structure = (short)((Structure & 0xFFFB)); break;
               case 2: Structure = (short)((Structure & 0xFFFB) | 0x4); break;
            }
         }
      }
      /// <summary>
      /// 1, 2
      /// </summary>
      public int Structure_DiffSize {
         get {
            return 1 + ((Structure >> 3) & 0x1);
         }
         set {
            switch (value) {
               case 1: Structure = (short)((Structure & 0xFFF7)); break;
               case 2: Structure = (short)((Structure & 0xFFF7) | 0x8); break;
            }
         }
      }
      /// <summary>
      /// 0, 1
      /// </summary>
      public int Structure_CodingtypeSize {
         get {
            return (Structure & 0x10) >> 4;
         }
         set {
            switch (value) {
               case 0: Structure = (short)((Structure & 0xFFEF)); break;
               case 1: Structure = (short)((Structure & 0xFFEF) | 0x10); break;
            }
         }
      }
      /// <summary>
      /// Länge des Subtile-Tabelleneintrags
      /// </summary>
      public short SubtileTableitemSize {
         get {
            return (short)(Structure_OffsetSize + Structure_BaseheightSize + Structure_DiffSize + Structure_CodingtypeSize);
         }
      }
      /// <summary>
      /// Pointer auf die Subtile-Tabelle (bezogen auf den Dateianfang)
      /// </summary>
      public uint PtrSubtileTable { get; set; }
      /// <summary>
      /// Pointer auf den Höhendatenbereich (bezogen auf den Dateianfang)
      /// </summary>
      public uint PtrHeightdata { get; set; }

      int _West = 0;
      /// <summary>
      /// westliche Grenze der Kachel
      /// </summary>
      public double West {
         get {
            return Unit2Degree(_West);
         }
         set {
            _West = Degree2Unit(value);
         }
      }

      int _North = 0;
      /// <summary>
      /// nördliche Grenze der Kachel
      /// </summary>
      public double North {
         get {
            return Unit2Degree(_North);
         }
         set {
            _North = Degree2Unit(value);
         }
      }

      int _PointDistanceHoriz = 0;
      /// <summary>
      /// waagerechter Abstand zwischen den Datenpunkten
      /// </summary>
      public double PointDistanceHoriz {
         get {
            return Unit2Degree(_PointDistanceHoriz);
         }
         set {
            _PointDistanceHoriz = Degree2Unit(value);
         }
      }

      int _PointDistanceVert = 0;
      /// <summary>
      /// senkrechter Abstand zwischen den Datenpunkten
      /// </summary>
      public double PointDistanceVert {
         get {
            return Unit2Degree(_PointDistanceVert);
         }
         set {
            _PointDistanceVert = Degree2Unit(value);
         }
      }

      /// <summary>
      /// kleinste Basishöhe eines Subtiles
      /// </summary>
      public ushort MinBaseheight { get; set; }
      /// <summary>
      /// größte Diff eines Subtiles (??? oder Maximalhöhe bezogen auf MinBaseheight ???)
      /// </summary>
      public ushort MaxDiff { get; set; }

      public int SubtileCount {
         get {
            return (1 + MaxIdxHoriz) * (1 + MaxIdxVert);
         }
      }


      public ZoomlevelTableitem() {
         No = 0;
         PointsHoriz = PointsVert = 64;
         Unknown0A = 35;
         Unknown0E = 35;
         Unknown12 = 0;
         MaxIdxHoriz = MaxIdxVert = 0;
         Structure = 0;
         Structure_OffsetSize = 3;
         Structure_BaseheightSize = 2;
         Structure_DiffSize = 2;
         Structure_CodingtypeSize = 1;
         West = 12.0;
         North = 54.0;
         PointDistanceHoriz = PointDistanceVert = 0.00028;
         MinBaseheight = 0;
         MaxDiff = 0;
      }

      public void Write(BinaryWriter w) {
         w.Write(No);
         w.Write(PointsHoriz);
         w.Write(PointsVert);
         w.Write(Unknown0A);
         w.Write(Unknown0E);
         w.Write(Unknown12);
         w.Write(MaxIdxHoriz);
         w.Write(MaxIdxVert);
         w.Write(Structure);
         w.Write(SubtileTableitemSize);
         w.Write(PtrSubtileTable);
         w.Write(PtrHeightdata);
         w.Write(_West);
         w.Write(_North);
         w.Write(_PointDistanceVert);
         w.Write(_PointDistanceHoriz);
         w.Write(MinBaseheight);
         w.Write(MaxDiff);
      }

      public static int Degree2Unit(double degree) {
         return (int)(degree / 360.0 * DEG_UNIT_FACTOR);
      }

      public static double Unit2Degree(int unit) {
         return unit * 360.0 / DEG_UNIT_FACTOR;
      }

   }
}
