using System.IO;

namespace BuildDEMFile {
   /// <summary>
   /// Tabelleneintrag für die Subtile-Tabelle
   /// </summary>
   class SubtileTableitem {
      /// <summary>
      /// Offset auf die Daten (bezogen auf den Anfang des Höhendatenbereichs)
      /// </summary>
      public uint Offset { get; set; }
      /// <summary>
      /// Bezugshöhe
      /// </summary>
      public short Baseheight { get; set; }
      /// <summary>
      /// max. Höhendiff.
      /// </summary>
      public ushort Diff { get; set; }
      /// <summary>
      /// Codiertyp
      /// </summary>
      public byte Type { get; set; }

      public SubtileTableitem() {
         Offset = 0;
         Baseheight = 0;
         Diff = 0;
         Type = 0;
      }

      /// <summary>
      /// schreibt den Tabelleneintrag
      /// </summary>
      /// <param name="w"></param>
      /// <param name="offset_len">Byteanzahl für Offset</param>
      /// <param name="baseheight_len">Byteanzahl für Bezugshöhe</param>
      /// <param name="diff_len">Byteanzahl für Höhendiff</param>
      /// <param name="type_len">Byteanzahl für Codiertyp (hier auch 0 möglich)</param>
      public void Write(BinaryWriter w, int offset_len = 3, int baseheight_len = 2, int diff_len = 2, int type_len = 1) {
         // Offset
         switch (offset_len) {
            case 1:
               w.Write((byte)(Offset & 0xFF));
               break;
            case 2:
               w.Write((byte)(Offset & 0xFF));
               w.Write((byte)((Offset & 0xFF00) >> 8));
               break;
            case 3:
               w.Write((byte)(Offset & 0xFF));
               w.Write((byte)((Offset & 0xFF00) >> 8));
               w.Write((byte)((Offset & 0xFF0000) >> 16));
               break;
         }

         // Basishöhe
         switch (baseheight_len) {
            case 1:
               w.Write((byte)(Baseheight & 0xFF));
               break;
            case 2:
               w.Write((byte)(Baseheight & 0xFF));
               w.Write((byte)((Baseheight & 0xFF00) >> 8));
               break;
         }

         // Diff.
         switch (diff_len) {
            case 1:
               w.Write((byte)(Diff & 0xFF));
               break;
            case 2:
               w.Write((byte)(Diff & 0xFF));
               w.Write((byte)((Diff & 0xFF00) >> 8));
               break;
         }

         // Typ
         if (type_len > 0) {
            w.Write(Type);
         }

      }

      public override string ToString() {
         return string.Format("Offset 0x{0:X}, Baseheight 0x{1:X}, Diff 0x{2:X}, Type 0x{3:X}", Offset, Baseheight, Diff, Type);
      }

   }
}
