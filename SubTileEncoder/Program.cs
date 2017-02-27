using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SubTileEncoder {

   class Program {


      static void Main(string[] args) {
         if (args.Length < 1) {
            Console.Error.WriteLine("Es muss mindestens eine Textdatei für die Daten angegeben werden.");
            Console.Error.WriteLine("Es kann außerdem ein Codiertyp, eine Basishöhe, eine Differenzhöhe und die Ausgabe eines Protokolls angegeben werden.");

         } else {
            string txtfile = args[0];
            byte codingtype = args.Length > 1 ? Convert.ToByte(args[1]) : (byte)0;
            int baseheight = args.Length > 2 ? Convert.ToInt32(args[2]) : int.MinValue;
            int diffheight = args.Length > 3 ? Convert.ToInt32(args[3]) : int.MinValue;
            bool bWithProt = args.Length > 4 ? Convert.ToBoolean(args[4]) : false;

            int tilewidth;
            int tileheight;
            int maxheight;
            int minheight;

            List<int> heights = ReadData(txtfile, out tilewidth, out tileheight, out minheight, out maxheight);

            if (heights.Count > 0) {
               if (baseheight == int.MinValue)
                  baseheight = minheight;
               if (diffheight == int.MinValue)
                  diffheight = maxheight - minheight;
               if (baseheight + diffheight < maxheight) {
                  Console.WriteLine("Die Differenzhöhe muss min. " + (maxheight - baseheight).ToString() + " sein.");
                  return;
               }

               if (baseheight > 0) {
                  for (int i = 0; i < heights.Count; i++)
                     heights[i] -= baseheight;
                  maxheight -= baseheight;
               }

               Encoder.TileEncoder enc = new Encoder.TileEncoder(maxheight, codingtype, tilewidth, tileheight, heights);

               StringBuilder sbProtData = new StringBuilder();
               StringBuilder sbProtEncoder = new StringBuilder();

               try {
                  Console.WriteLine("encodiere Daten aus '" + txtfile + "' ...");
                  int line = -1;
                  bool bTileIsFull;
                  do {
                     int count = enc.ComputeNext(out bTileIsFull);
                     if (bWithProt &&
                         count > 0) {

                        for (int i = enc.Elements.Count - count; i < enc.Elements.Count; i++) {

                           if (enc.Elements[i].Line >= 0 &&
                               line != enc.Elements[i].Line) {
                              line = enc.Elements[i].Line;
                              sbProtEncoder.Append("Line " + line.ToString());
                              sbProtEncoder.AppendLine();
                              sbProtData.AppendLine();
                           }

                           switch (enc.Elements[i].ElementTyp) {
                              case Encoder.TileEncoder.HeightElement.Typ.Plateau:
                                 sbProtEncoder.AppendLine(string.Format("Idx={0}, Plateau Length={1} TableIdx={2} Bits={3} [{4}] <{5}>",
                                                                        enc.Elements[i].Column,
                                                                        enc.Elements[i].Data,
                                                                        enc.Elements[i].PlateauTableIdx,
                                                                        enc.Elements[i].PlateauBinBits,
                                                                        enc.Elements[i].GetBinText(),
                                                                        enc.Elements[i].GetPlateauUnitsText()));

                                 StringBuilder sb = new StringBuilder();
                                 for (int j = 0; j < enc.Elements[i].Data; j++) {
                                    if (enc.Elements[i].Column > 0 || j > 0)
                                       sb.Append("\t");
                                    sb.Append("*");
                                 }
                                 sbProtData.Append(sb.ToString());
                                 break;

                              case Encoder.TileEncoder.HeightElement.Typ.PlateauFollower:
                                 sbProtEncoder.AppendLine(string.Format("Idx={0}, PlateauFollower ActualHeigth={1}, Value={2} [{3}] {4}{5} ddiff={6}",
                                                                        enc.Elements[i].Column,
                                                                        enc.ActualHeigth,
                                                                        enc.Elements[i].Data,
                                                                        enc.Elements[i].GetBinText(),
                                                                        enc.Elements[i].Encoding,
                                                                        enc.Elements[i].Encoding == Encoder.TileEncoder.EncodeMode.Hybrid ?
                                                                              enc.Elements[i].HUnit.ToString() :
                                                                              "",
                                                                        enc.Elements[i].PlateauFollowerDdiff));
                                 if (enc.Elements[i].Column > 0)
                                    sbProtData.Append("\t");

                                 sbProtData.Append("[" + enc.Elements[i].Data.ToString() + "]");
                                 break;

                              case Encoder.TileEncoder.HeightElement.Typ.Value:
                                 sbProtEncoder.AppendLine(string.Format("Idx={0}, ActualHeigth={1}, Value={2} [{3}] {4}{5}",
                                                                        enc.Elements[i].Column,
                                                                        enc.ActualHeigth,
                                                                        enc.Elements[i].Data,
                                                                        enc.Elements[i].GetBinText(),
                                                                        enc.Elements[i].Encoding,
                                                                        enc.Elements[i].Encoding == Encoder.TileEncoder.EncodeMode.Hybrid ?
                                                                              enc.Elements[i].HUnit.ToString() :
                                                                              ""));
                                 if (enc.Elements[i].Column > 0)
                                    sbProtData.Append("\t");

                                 sbProtData.Append(enc.Elements[i].Data.ToString());
                                 break;

                           }
                        }
                     }


                  } while (!bTileIsFull);

                  string destfile = Path.GetFileName(txtfile) + ".bin";
                  int datalength = 0;

                  Console.WriteLine("schreibe Binärdaten in '" + destfile + "' ...");
                  using (BinaryWriter w = new BinaryWriter(File.Open(destfile, FileMode.Create))) {
                     byte[] buff = enc.GetCodedBytes();
                     datalength = buff.Length;
                     w.Write(buff);
                  }

                  Console.WriteLine("schreibe Konfiguration in '" + destfile + "'.info ...");
                  using (StreamWriter w = new StreamWriter(Path.GetFullPath(destfile + ".info"))) {
                     w.WriteLine("FILE=" + destfile);
                     w.WriteLine("TILEWIDTH=" + tilewidth.ToString());
                     w.WriteLine("TILEHEIGHT=" + tileheight.ToString());
                     w.WriteLine("BASE=" + baseheight.ToString());
                     w.WriteLine("DIFF=" + diffheight.ToString());
                     w.WriteLine("TYPE=" + codingtype.ToString());
                     w.WriteLine("LENGTH=" + datalength.ToString());
                  }

                  if (bWithProt) {
                     Console.WriteLine("schreibe Protokoll in '" + destfile + "'.prot.txt ...");
                     using (StreamWriter w = new StreamWriter(Path.GetFullPath(destfile + ".prot.txt"))) {
                        w.WriteLine(sbProtEncoder.ToString());
                        w.WriteLine();
                        w.WriteLine(sbProtData.ToString());
                     }
                  }


               } catch (Exception ex) {
                  Console.WriteLine("Fehler:");
                  Console.WriteLine(ex.Message);
               }

            }

         }
      }

      /// <summary>
      /// liest die Höhendaten ein
      /// </summary>
      /// <param name="txtfile"></param>
      /// <param name="tilewidth"></param>
      /// <param name="tileheight"></param>
      /// <param name="minheight"></param>
      /// <param name="maxheight"></param>
      /// <returns></returns>
      static List<int> ReadData(string txtfile, out int tilewidth, out int tileheight, out int minheight, out int maxheight) {
         List<int> heights = new List<int>();
         tilewidth = -1;
         tileheight = 0;
         maxheight = int.MinValue;
         minheight = int.MaxValue;

         try {
            String txt = null;
            using (StreamReader sr = new StreamReader(txtfile)) {
               txt = sr.ReadToEnd();
            }
            string[] lines = txt.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines) {
               string[] sData = line.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

               if (tilewidth < 0) // 1. Datenzeile gibt die Breite vor
                  tilewidth = sData.Length;
               else {
                  if (tilewidth != sData.Length)
                     throw new Exception("Die Datenanzahl (" + sData.Length.ToString() + ") in Datenzeile " + (tileheight + 1).ToString() + " ist nicht identisch zur 1. Zeile (" + tilewidth.ToString() + ").");
               }
               tileheight++;

               for (int i = 0; i < sData.Length; i++) {
                  int val = Convert.ToInt32(sData[i].Trim());
                  if (val < 0)
                     throw new Exception("Es können nur Werte >= 0 verwendet werden.");
                  minheight = Math.Min(minheight, val);
                  maxheight = Math.Max(maxheight, val);
                  heights.Add(val);
               }
            }
         } catch (Exception e) {
            Console.WriteLine("Die Datei '" + txtfile + "' konnte nicht gelesen werden:");
            Console.WriteLine(e.Message);
         }

         return heights;
      }

   }
}
