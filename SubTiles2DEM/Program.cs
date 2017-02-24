using System;
using System.Collections.Generic;
using System.IO;

namespace SubTiles2DEM {

   class Program {

      const string DEMFILE = "DEMFILE=";
      const string TILESIZE = "TILESIZE=";
      const string FOOTFLAG = "FOOTFLAG=";
      const string ZOOMLEVEL = "ZOOMLEVEL=";
      const string UNKNOWN1B = "UNKNOWN1B=";
      const string UNKNOWN25 = "UNKNOWN25=";

      const string BINFILE = "BINFILE=";        // mehrfach (je Subtile)

      // mehrfach (je Zoomlevel)
      const string TILE_HORIZCOUNT = "TILE_HORIZCOUNT=";
      const string TILE_VERTCOUNT = "TILE_VERTCOUNT=";
      const string TILE_UNKNOWN0A = "TILE_UNKNOWN0A=";
      const string TILE_UNKNOWN0E = "TILE_UNKNOWN0E=";
      const string TILE_UNKNOWN12 = "TILE_UNKNOWN12=";
      const string TILE_WEST = "TILE_WEST=";
      const string TILE_NORTH = "TILE_NORTH=";
      const string TILE_POINTDISTANCEHORIZ = "TILE_POINTDISTANCEHORIZ=";
      const string TILE_POINTDISTANCEVERT = "TILE_POINTDISTANCEVERT=";


      // zum Lesen der Binärdaten
      const string BIN_FILE = "FILE=";
      const string BIN_TILESIZE = "TILESIZE=";
      const string BIN_BASE = "BASE=";
      const string BIN_DIFF = "DIFF=";
      const string BIN_TYPE = "TYPE=";
      const string BIN_LENGTH = "LENGTH=";


      static void Main(string[] args) {
         if (args.Length < 1) {
            Console.Error.WriteLine("Es muss eine Konfigurationdatei angegeben werden.");
            return;
         }
         string cfgfilename = args[0];
         if (!Path.IsPathRooted(cfgfilename))
            cfgfilename = Path.GetFullPath(cfgfilename);

         Head head = new Head();
         List<ZoomlevelData> Zoomlevel = new List<ZoomlevelData>();

         List<string> bindatafilename;
         string demfilename = null;
         try {
            demfilename = ReadConfigdata(cfgfilename, ref head, ref Zoomlevel, out bindatafilename);
         } catch (Exception ex) {
            Console.Error.WriteLine("FEHLER: " + ex.Message);
            return;
         }

         try {
            int idx = 0;
            for (int z = 0; z < head.Zoomlevel; z++) {
               for (int i = 0; i < Zoomlevel[z].Tableitem.SubtileCount; i++, idx++) {
                  ReadBinData(bindatafilename[idx], Zoomlevel[z]);
               }
            }
         } catch (Exception ex) {
            Console.Error.WriteLine("FEHLER: " + ex.Message);
            return;
         }

         for (int z = 0; z < head.Zoomlevel; z++) {
            FitTableSize(Zoomlevel[z], z == 0);

            Zoomlevel[z].Tableitem.PtrSubtileTable = z == 0 ?
                           head.Length :
                           Zoomlevel[z - 1].Tableitem.PtrHeightdata + Zoomlevel[z - 1].GetHeightDataAreaSize();
            Zoomlevel[z].Tableitem.PtrHeightdata = (uint)(Zoomlevel[z].Tableitem.PtrSubtileTable + Zoomlevel[z].GetTableAreaSize());
         }

         head.PtrZoomlevelTable = Zoomlevel[head.Zoomlevel - 1].Tableitem.PtrHeightdata + Zoomlevel[head.Zoomlevel - 1].GetHeightDataAreaSize();

         // Jetzt sollten alle Daten gesetzt sein und die Datei kann geschrieben werden.
         try {
            using (BinaryWriter w = new BinaryWriter(File.Create(demfilename)))
               WriteDEM(w, head, Zoomlevel);

         } catch (Exception ex) {
            Console.Error.WriteLine("FEHLER: " + ex.Message);
            return;
         }

      }

      static string ReadConfigdata(string cfgfilename, ref Head head, ref List<ZoomlevelData> Zoomlevel, out List<string> bindatafilename) {
         string demfilename = null;
         int footflag = int.MinValue;
         int zoomlevel = int.MinValue;
         int unknown1B = int.MinValue;
         int unknown25 = int.MinValue;
         int tilesize = int.MinValue;

         bindatafilename = new List<string>();

         List<int> tile_horiz = new List<int>();
         List<int> tile_vert = new List<int>();
         List<int> tile_unknown0A = new List<int>();
         List<int> tile_unknown0E = new List<int>();
         List<int> tile_unknown12 = new List<int>();
         List<double> tile_west = new List<double>();
         List<double> tile_north = new List<double>();
         List<double> tile_pointdistancehoriz = new List<double>();
         List<double> tile_pointdistancevert = new List<double>();

         using (StreamReader sr = new StreamReader(cfgfilename)) {
            string txt = null;
            do {
               txt = sr.ReadLine();
               if (txt != null) {
                  txt = txt.Trim();
                  if (txt.Length > 0) {
                     if (txt[0] != '#') {

                        if (!ReadStringFromLine(txt, DEMFILE, ref demfilename)) {
                           string tmp = "";
                           int itmp = int.MinValue;
                           double ftmp = double.MinValue;

                           if (!ReadStringFromLine(txt, BINFILE, ref tmp)) {
                              if (!ReadNotNegativIntFromLine(txt, FOOTFLAG, ref footflag))
                                 if (!ReadNotNegativIntFromLine(txt, ZOOMLEVEL, ref zoomlevel))
                                    if (!ReadNotNegativIntFromLine(txt, UNKNOWN1B, ref unknown1B))
                                       if (!ReadNotNegativIntFromLine(txt, UNKNOWN25, ref unknown25))
                                          if (!ReadNotNegativIntFromLine(txt, TILESIZE, ref tilesize))
                                             if (!ReadNotNegativIntFromLine(txt, TILE_HORIZCOUNT, ref itmp)) {
                                                if (!ReadNotNegativIntFromLine(txt, TILE_VERTCOUNT, ref itmp)) {
                                                   if (!ReadNotNegativIntFromLine(txt, TILE_UNKNOWN0A, ref itmp)) {
                                                      if (!ReadNotNegativIntFromLine(txt, TILE_UNKNOWN0E, ref itmp)) {
                                                         if (!ReadNotNegativIntFromLine(txt, TILE_UNKNOWN12, ref itmp)) {
                                                            if (!ReadNotNegativDoubleFromLine(txt, TILE_WEST, ref ftmp)) {
                                                               if (!ReadNotNegativDoubleFromLine(txt, TILE_NORTH, ref ftmp)) {
                                                                  if (!ReadNotNegativDoubleFromLine(txt, TILE_POINTDISTANCEHORIZ, ref ftmp)) {
                                                                     if (!ReadNotNegativDoubleFromLine(txt, TILE_POINTDISTANCEVERT, ref ftmp)) {

                                                                     } else
                                                                        tile_pointdistancevert.Add(ftmp);

                                                                  } else
                                                                     tile_pointdistancehoriz.Add(ftmp);
                                                               } else
                                                                  tile_north.Add(ftmp);
                                                            } else
                                                               tile_west.Add(ftmp);
                                                         } else
                                                            tile_unknown12.Add(itmp);
                                                      } else
                                                         tile_unknown0E.Add(itmp);
                                                   } else
                                                      tile_unknown0A.Add(itmp);
                                                } else
                                                   tile_vert.Add(itmp);
                                             } else
                                                tile_horiz.Add(itmp);
                           } else {
                              if (tmp.Length > 0 &&
                                  !Path.IsPathRooted(tmp))
                                 tmp = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(cfgfilename), tmp));
                              bindatafilename.Add(tmp);
                           }
                        }

                     }
                  }
               }
            } while (txt != null);
         }

         if (tilesize <= 0)
            tilesize = 64;
         if (footflag < 0)
            footflag = 1;
         if (zoomlevel <= 0)
            zoomlevel = 1;
         if (unknown1B < 0)
            unknown1B = 0;
         if (unknown25 < 0)
            unknown25 = 1;

         head.Footflag = footflag;
         head.Unknown1B = unknown1B;
         head.Unknown25 = unknown25;
         head.Zoomlevel = (ushort)zoomlevel;

         if (tile_horiz.Count < zoomlevel)
            throw new Exception("Subtile-Layout nicht für jeden Zoomlevel definiert.");
         if (tile_vert.Count < zoomlevel)
            throw new Exception("Subtile-Layout nicht für jeden Zoomlevel definiert.");
         if (tile_unknown0A.Count < zoomlevel)
            throw new Exception("unknown0A nicht für jeden Zoomlevel definiert.");
         if (tile_unknown0E.Count < zoomlevel)
            throw new Exception("unknown0E nicht für jeden Zoomlevel definiert.");
         if (tile_unknown12.Count < zoomlevel)
            throw new Exception("unknown12 nicht für jeden Zoomlevel definiert.");
         if (tile_west.Count < zoomlevel)
            throw new Exception("Westgrenze nicht für jeden Zoomlevel definiert.");
         if (tile_north.Count < zoomlevel)
            throw new Exception("Nordgrenze nicht für jeden Zoomlevel definiert.");
         if (tile_pointdistancehoriz.Count < zoomlevel)
            throw new Exception("Punktabstand nicht für jeden Zoomlevel definiert.");

         int needsubtiles = 0;
         for (int i = 0; i < zoomlevel; i++) {
            needsubtiles += tile_horiz[i] * tile_vert[i];
         }
         if (bindatafilename.Count < needsubtiles)
            throw new Exception("Zu wenig Subtile-Dateien definiert.");
         else if (bindatafilename.Count > needsubtiles)
            bindatafilename.RemoveRange(needsubtiles, bindatafilename.Count - needsubtiles);

         for (int i = 0; i < head.Zoomlevel; i++) {
            Zoomlevel.Add(new ZoomlevelData());
            Zoomlevel[i].Tableitem.No = (ushort)i;
            Zoomlevel[i].Tableitem.PointsHoriz = Zoomlevel[i].Tableitem.PointsVert = tilesize;
            Zoomlevel[i].Tableitem.West = tile_west[i];
            Zoomlevel[i].Tableitem.North = tile_north[i];
            Zoomlevel[i].Tableitem.PointDistanceHoriz = tile_pointdistancehoriz[i];
            Zoomlevel[i].Tableitem.PointDistanceVert = tile_pointdistancevert[i];
            Zoomlevel[i].Tableitem.MaxIdxHoriz = tile_horiz[i] - 1;
            Zoomlevel[i].Tableitem.MaxIdxVert = tile_vert[i] - 1;
            Zoomlevel[i].Tableitem.Unknown0A = tile_unknown0A[i];
            Zoomlevel[i].Tableitem.Unknown0E = tile_unknown0E[i];
            Zoomlevel[i].Tableitem.Unknown12 = (short)tile_unknown12[i];
         }

         return demfilename;
      }

      static void ReadBinData(string binfile, ZoomlevelData zoomleveldata) {

         int bin_base = int.MinValue;
         int bin_diff = int.MinValue;
         int bin_type = int.MinValue;
         int bin_length = int.MinValue;

         if (File.Exists(binfile + ".info")) {
            using (StreamReader sr = new StreamReader(binfile + ".info")) {
               string txt = null;
               do {
                  txt = sr.ReadLine();
                  if (txt != null) {
                     txt = txt.Trim();
                     if (txt.Length > 0) {
                        if (txt[0] != '#') {

                           if (!ReadNotNegativIntFromLine(txt, BIN_BASE, ref bin_base))
                              if (!ReadNotNegativIntFromLine(txt, BIN_DIFF, ref bin_diff))
                                 if (!ReadNotNegativIntFromLine(txt, BIN_TYPE, ref bin_type))
                                    if (!ReadNotNegativIntFromLine(txt, BIN_LENGTH, ref bin_length)) { }
                        }
                     }
                  }
               } while (txt != null);
            }
         }

         if (bin_base < 0)
            bin_base = 0;
         if (bin_diff < 0)
            bin_diff = 0;
         if (bin_type < 0)
            bin_type = 0;
         if (bin_length < 0)
            bin_length = 0;

         SubtileTableitem tableitem = new SubtileTableitem();
         tableitem.Baseheight = (ushort)bin_base;
         tableitem.Diff = (ushort)bin_diff;
         tableitem.Type = (byte)bin_type;

         if (binfile.Length == 0 ||
             !File.Exists(binfile)) {
            tableitem.Diff = 0;
            zoomleveldata.Subtiles.Add(new Subtile((byte[])null, tableitem));
         } else {
            zoomleveldata.Subtiles.Add(new Subtile(binfile, tableitem));
         }
      }

      static void FitTableSize(ZoomlevelData zoomleveldata, bool firstlevel) {
         int minbaseheight = int.MaxValue;
         int maxbaseheight = int.MinValue;
         int maxdiff = int.MinValue;
         uint maxoffset = uint.MinValue;

         for (int i = 0; i < zoomleveldata.Subtiles.Count; i++) {
            minbaseheight = Math.Min(minbaseheight, zoomleveldata.Subtiles[i].Tableitem.Baseheight);
            maxbaseheight = Math.Max(maxbaseheight, zoomleveldata.Subtiles[i].Tableitem.Baseheight);
            maxdiff = Math.Max(maxdiff, zoomleveldata.Subtiles[i].Tableitem.Diff);
            zoomleveldata.Subtiles[i].Tableitem.Offset = i == 0 ?
                              0 :
                              zoomleveldata.Subtiles[i - 1].Tableitem.Offset + (uint)zoomleveldata.Subtiles[i - 1].DataLength;
            maxoffset = Math.Max(maxoffset, zoomleveldata.Subtiles[i].Tableitem.Offset);
         }

         zoomleveldata.Tableitem.MinBaseheight = (ushort)minbaseheight;
         zoomleveldata.Tableitem.MaxDiff = (ushort)maxdiff;

         if (maxoffset < 255)
            zoomleveldata.Tableitem.Structure_OffsetSize = 1;
         else if (maxoffset < 65536)
            zoomleveldata.Tableitem.Structure_OffsetSize = 2;
         else
            zoomleveldata.Tableitem.Structure_OffsetSize = 3;

         if (maxbaseheight < 255)
            zoomleveldata.Tableitem.Structure_BaseheightSize = 1;
         else
            zoomleveldata.Tableitem.Structure_BaseheightSize = 2;

         if (maxdiff < 255)
            zoomleveldata.Tableitem.Structure_DiffSize = 1;
         else
            zoomleveldata.Tableitem.Structure_DiffSize = 2;

         zoomleveldata.Tableitem.Structure_CodingtypeSize = firstlevel ? 1 : 0;

      }

      static void WriteDEM(BinaryWriter w, Head head, List<ZoomlevelData> Zoomlevel) {
         head.Write(w);

         for (int z = 0; z < Zoomlevel.Count; z++) {
            // Subtile-Tabelle schreiben
            for (int i = 0; i < Zoomlevel[z].Subtiles.Count; i++)
               Zoomlevel[z].Subtiles[i].Tableitem.Write(w, 
                                                        Zoomlevel[z].Tableitem.Structure_OffsetSize, 
                                                        Zoomlevel[z].Tableitem.Structure_BaseheightSize, 
                                                        Zoomlevel[z].Tableitem.Structure_DiffSize, 
                                                        Zoomlevel[z].Tableitem.Structure_CodingtypeSize);
            // Datenschreiben
            for (int i = 0; i < Zoomlevel[z].Subtiles.Count; i++)
               w.Write(Zoomlevel[z].Subtiles[i].Data);
         }

         // Zoomlevel-Tabelle schreiben
         for (int z = 0; z < Zoomlevel.Count; z++)
            Zoomlevel[z].Tableitem.Write(w);
      }

      static bool ReadNotNegativIntFromLine(string line, string tag, ref int val) {
         if (line.StartsWith(tag)) {
            if (val < 0)
               val = Convert.ToInt32(RemoveComment(line.Substring(tag.Length).Trim()));
            return true;
         }
         return false;
      }

      static bool ReadNotNegativDoubleFromLine(string line, string tag, ref double val) {
         if (line.StartsWith(tag)) {
            if (val < 0)
               val = Convert.ToDouble(RemoveComment(line.Substring(tag.Length).Trim()));
            return true;
         }
         return false;
      }

      static bool ReadStringFromLine(string line, string tag, ref string val) {
         if (line.StartsWith(tag)) {
            if (string.IsNullOrEmpty(val))
               val = RemoveComment(line.Substring(tag.Length).Trim());
            return true;
         }
         return false;
      }

      static string RemoveComment(string line) {
         int comment = line.IndexOf('#');
         if (comment >= 0)
            return line.Substring(0, comment).Trim();
         return line;
      }

   }
}
