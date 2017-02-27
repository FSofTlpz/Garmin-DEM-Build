using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Input1 {
   public partial class Form1 : Form {

      List<bool> bin = new List<bool>();
      List<bool> postbin = new List<bool>();

      bool autostartandend = false;

      public Form1() {
         InitializeComponent();
      }


      private void Form1_Load(object sender, EventArgs e) {
         string[] args = Environment.GetCommandLineArgs();

         int idx = 1;
         if (args.Length > idx) {
            textBox_Patchfile.Text = args[idx++].Trim();

            if (args.Length > idx) {
               textBox_PatchPos.Text = args[idx++].Trim();

               if (args.Length > idx) {
                  richTextBox_Bin.Text = args[idx++].Trim();

                  if (args.Length > idx) {
                     numericUpDown_MinBytes.Value = Convert.ToDecimal(args[idx++].Trim());

                     if (args.Length > idx) {
                        numericUpDown_maxheight.Value = Convert.ToDecimal(args[idx++].Trim());

                        if (args.Length > idx) {
                           textBox_PatchAddrHeight.Text = args[idx++].Trim();

                           if (args.Length > idx) {
                              textBox_PatchAddrRangevalue.Text = args[idx++].Trim();

                              if (args.Length > idx) {
                                 textBox_Protfile.Text = args[idx++].Trim();

                                 if (args.Length > idx) {
                                    textBox_Extern.Text = args[idx++].Trim();

                                    if (args.Length > idx) {
                                       textBox_ExternArgs.Text = args[idx++].Trim();

                                       if (args.Length > idx) {
                                          autostartandend = Convert.ToBoolean(args[idx++].Trim());
                                       }
                                    }
                                 }
                              }
                           }
                        }
                     }
                  }
               }
            }
         }

         Text += " (" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ")";
      }

      private void Form1_Shown(object sender, EventArgs e) {
         listBox_SingleTest.SelectedIndex = 2;
         if (autostartandend)
            button_Start_Click(button_Start, null);
      }

      private void button_Start_Click(object sender, EventArgs e) {
         string sPatchedFile = textBox_Patchfile.Text.Trim();
         string sPatchAddress = textBox_PatchPos.Text.Trim();
         string sPatchAddressMaxheight = textBox_PatchAddrHeight.Text.Trim();
         string sPatchAddressRangevalue = textBox_PatchAddrRangevalue.Text.Trim();
         string sExternCommand = textBox_Extern.Text.Trim();
         string sExternCommandArgs = textBox_ExternArgs.Text.Trim();
         string sOutfile = textBox_Protfile.Text.Trim();
         bool bOutHex = checkBox_ProtHex.Checked;
         bool bOutBin = checkBox_ProtBin.Checked;
         bool bOutNL = checkBox_ProtNewline.Checked;
         bool bFillBitsTrue = checkBox_FillBits.Checked;

         List<bool> binlst = new List<bool>(bin);
         binlst.AddRange(postbin);

         // Ausgabe
         if (binlst.Count > 0) {
            if (sOutfile.Length > 0 &&
                (bOutHex || bOutBin)) {
               try {
                  using (StreamWriter wr = File.AppendText(sOutfile)) {
                     if (bOutHex) {
                        wr.Write(getHexString(binlst, bFillBitsTrue));
                        if (bOutBin)
                           wr.Write(" ");
                     }
                     if (bOutBin) {
                        wr.Write("[");
                        wr.Write(richTextBox_Bin.Text.Trim() + textBox_PostBits.Text.Trim());
                        wr.Write("]");
                     }
                     if (bOutNL)
                        wr.WriteLine();
                  }
               } catch (Exception ex) {
                  MessageBox.Show("Fehler beim Schreiben in die Ausgabedatei: " + ex.Message);
               }
            }


            // patchen
            if (sPatchedFile.Length > 0 &&
                File.Exists(sPatchedFile) &&
                sPatchAddress.Length > 0) {
               try {
                  int address = ReadHexAs31Int(sPatchAddress);
                  List<byte> bytes = getBytes(binlst, bFillBitsTrue, (int)numericUpDown_MinBytes.Value);
                  using (BinaryWriter bw = new BinaryWriter(File.Open(sPatchedFile, FileMode.Open))) {
                     if (address < bw.BaseStream.Length) {
                        bw.Seek(address, SeekOrigin.Begin);
                        for (int j = 0; j < bytes.Count; j++)
                           if (bw.BaseStream.Position < bw.BaseStream.Length)
                              bw.Write(bytes[j]);
                     }

                     if (sPatchAddressMaxheight.Length > 0) {
                        address = ReadHexAs31Int(sPatchAddressMaxheight);
                        if (address < bw.BaseStream.Length) {
                           bw.Seek(address, SeekOrigin.Begin);
                           UInt16 val = Convert.ToUInt16(numericUpDown_maxheight.Value);
                           bw.Write(val);
                        }
                     }

                     if (sPatchAddressRangevalue.Length > 0) {
                        address = ReadHexAs31Int(sPatchAddressRangevalue);
                        if (address < bw.BaseStream.Length) {
                           bw.Seek(address, SeekOrigin.Begin);
                           byte val = Convert.ToByte(numericUpDown_Rangevalue.Value);
                           bw.Write(val);
                        }
                     }
                  }
               } catch (Exception ex) {
                  MessageBox.Show("Fehler beim Patchen: " + ex.Message);
               }
            }
         }

         // Kommando ausführen
         if (sExternCommand.Length > 0) {
            try {
               Process p = Process.Start(sExternCommand, sExternCommandArgs);      // mit Args: Start(String, String)
               p.EnableRaisingEvents = true;
               p.Exited += p_Exited;

               //ProcessStartInfo startInfo = new ProcessStartInfo(sExternCommand);
               //Process p = new Process();
               //p.EnableRaisingEvents = true;
               //p.StartInfo = startInfo;
               //p.Exited += p_Exited;
               //p.Start();

            } catch (Exception ex) {
               MessageBox.Show("Fehler beim Starten des externen Prozesses: " + ex.Message);
            }
         }
      }

      void p_Exited(object sender, EventArgs e) {
         if (autostartandend)
            Close();
      }



      List<byte> getBytes(List<bool> bin, bool fillbitsaretrue, int minbytes) {
         List<byte> ret = new List<byte>();
         for (int b = 0; b < bin.Count; b += 8) {
            byte hex = 0;
            for (int i = 0; i < 8; i++) {
               bool bindigit = b + i < bin.Count ? bin[b + i] : fillbitsaretrue;
               hex <<= 1;
               hex |= (byte)(bindigit ? 1 : 0);
            }
            ret.Add(hex);
         }
         while (ret.Count < minbytes)
            ret.Add((byte)(fillbitsaretrue ? 0xff : 0x00));
         return ret;
      }

      string getHexString(List<bool> binlst, bool fillbitsaretrue) {
         List<bool> bin = new List<bool>(binlst);
         string hex = "";
         int hexdigitcount = 0;
         while (bin.Count > 0) {
            int hexdigit = 0;
            for (int i = 0; i < 4; i++) {
               bool bindigit = bin.Count > 0 ? bin[0] : fillbitsaretrue;
               if (bin.Count > 0)
                  bin.RemoveAt(0);
               hexdigit <<= 1;
               hexdigit |= bindigit ? 1 : 0;
            }
            switch (hexdigit) {
               case 0: hex += "0"; break;
               case 1: hex += "1"; break;
               case 2: hex += "2"; break;
               case 3: hex += "3"; break;
               case 4: hex += "4"; break;
               case 5: hex += "5"; break;
               case 6: hex += "6"; break;
               case 7: hex += "7"; break;
               case 8: hex += "8"; break;
               case 9: hex += "9"; break;
               case 10: hex += "A"; break;
               case 11: hex += "B"; break;
               case 12: hex += "C"; break;
               case 13: hex += "D"; break;
               case 14: hex += "E"; break;
               case 15: hex += "F"; break;
            }
            if (hexdigitcount++ % 2 == 1)
               hex += " ";
         }
         return hex.Trim();
      }

      bool bSetIntern = false;

      /// <summary>
      /// berücksichtigt: '1', '0', '.'
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void textBox_Bin_TextChanged(object sender, EventArgs e) {
         if (!bSetIntern) {
            if (sender is RichTextBox) {
               RichTextBox tb = sender as RichTextBox;
               int pos = tb.SelectionStart;
               bSetIntern = true;
               tb.Text = TextCleaner(true, tb.Text);
               tb.Select(Math.Max(0, Math.Min(pos, tb.Text.Length)), 0);
               tb.BackColor = col_bin_ok;
               bSetIntern = false;
            } else {
               TextBox tb = sender as TextBox;
               int pos = tb.SelectionStart;
               bSetIntern = true;
               tb.Text = TextCleaner(false, tb.Text);
               tb.Select(Math.Max(0, Math.Min(pos, tb.Text.Length)), 0);
               tb.BackColor = col_bin_ok;
               bSetIntern = false;
            }
         }
      }

      string TextCleaner(bool b4Main, string txt) {
         List<char> lst = new List<char>(txt.ToCharArray());
         List<char> lstnew = new List<char>();
         List<bool> binlst = new List<bool>();

         // bereinigen:
         for (int i = 0; i < lst.Count; i++)
            if (lst[i] == '1') {
               lstnew.Add('1');
               binlst.Add(true);
            } else if (lst[i] == '0' ||
                       lst[i] == '.') {
               lstnew.Add('.');
               binlst.Add(false);
            } else if (lst[i] == ' ')
               lstnew.Add(' ');

         if (b4Main)
            bin = binlst;
         else
            postbin = binlst;

         return new string(lstnew.ToArray());
      }

      int ReadAs31Int(string text) {
         int number;
         if (text.Length == 2 && (number = ReadHexAs31Int(text)) >= 0)
            return number;
         if (text.Length == 8 && (number = ReadBinaryAs31Int(text)) >= 0)
            return number;
         if ((number = ReadDecimalAs31Int(text)) >= 0)
            return number;
         return -1;
      }

      /// <summary>
      /// interpretiert einen Text als Hex-Zahl (max. 31-Bit)
      /// </summary>
      /// <param name="text"></param>
      /// <returns>bei Fehler negativer Wert</returns>
      int ReadHexAs31Int(string text) {
         int result;
         if (!int.TryParse(text, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out result))
            return -1;
         return result;
      }

      /// <summary>
      /// interpretiert einen Text als Binär-Zahl (max. 31-Bit)
      /// </summary>
      /// <param name="text"></param>
      /// <returns>bei Fehler negativer Wert</returns>
      int ReadBinaryAs31Int(string text) {
         int result = 0;
         text = text.Trim();
         for (int i = 0; i < text.Length; i++) {
            result <<= 1;
            if (text[i] == '0') {

            } else if (text[i] == '1') {
               result |= 1;
            } else
               return -1;
         }
         return result;
      }

      /// <summary>
      /// interpretiert einen Text als Dezimal-Zahl (max. 31-Bit)
      /// </summary>
      /// <param name="text"></param>
      /// <returns>bei Fehler negativer Wert</returns>
      int ReadDecimalAs31Int(string text) {
         int result;
         if (!int.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.CurrentCulture, out result))
            return -1;
         return result;
      }


      int ReadAs31IntV2(string text) {
         int number;
         if ((number = ReadDecimalAs31Int(text)) >= 0)
            return number;
         if (text.Length == 2 && (number = ReadHexAs31Int(text)) >= 0)
            return number;
         if (text.Length == 8 && (number = ReadBinaryAs31Int(text)) >= 0)
            return number;
         return -1;
      }

      private void button_dec2bin_Click(object sender, EventArgs e) {
         List<int> v = GetDecimalHeights();
         Encoder.TileEncoder enc = new Encoder.TileEncoder((int)numericUpDown_maxheight.Value, 
                                                           (byte)numericUpDown_Rangevalue.Value, 
                                                           (int)numericUpDown_TilesizeHoriz.Value, 
                                                           (int)numericUpDown_TilesizeVert.Value, 
                                                           v);
         int idx = 0;
         try {
            int count = -1;
            bool bTileIsFull = false;
            while (count != 0 && !bTileIsFull) {
               count = enc.ComputeNext(out bTileIsFull);
               idx += count;
            }
            richTextBox_Bin.BackColor = col_bin_ok;
         } catch (Exception ex) {
            MessageBox.Show("Exception bei Index " + idx.ToString() + " mit Wert " + v[idx].ToString() + ": " + ex.Message, "Fehler");
            return;
         }

         richTextBox_Bin.Text = enc.GetBinText();

         int bytes = richTextBox_Bin.TextLength / 8;
         if ((richTextBox_Bin.TextLength % 8) != 0)
            bytes++;
         label_Bytes.Text = bytes.ToString() + " Bytes";
      }

      private void button_Detail_Click(object sender, EventArgs e) {
         FormDetail dlg = new FormDetail();
         dlg.HeightData = GetDecimalHeights();
         dlg.HeightMax = (int)numericUpDown_maxheight.Value;
         dlg.Codingtype = (byte)numericUpDown_Rangevalue.Value;
         dlg.TileSizeHorz = (int)numericUpDown_TilesizeHoriz.Value;
         dlg.TileSizeVert = (int)numericUpDown_TilesizeVert.Value;
         dlg.ShowDialog();
      }

      private void button_SingleTest_Click(object sender, EventArgs e) {
         int val = (int)numericUpDown_SingleTest.Value;
         List<byte> bits = new List<byte>();

         try {
            switch (listBox_SingleTest.SelectedIndex) {
               case 0:
                  bits = Encoder.TileEncoder.LengthCoding0(val);
                  break;
               case 1:
                  bits = Encoder.TileEncoder.LengthCoding1(val);
                  break;
               default:
                  int hunit = (int)Math.Pow(2, listBox_SingleTest.SelectedIndex - 2);
                  if (hunit <= 256)
                     bits = Encoder.TileEncoder.HybridCoding(val, (int)numericUpDown_maxheight.Value, hunit);
                  else if (hunit == 512)
                     bits = Encoder.TileEncoder.BigValueCodingHybrid(val, (int)numericUpDown_maxheight.Value);
                  else if (hunit == 1024)
                     bits = Encoder.TileEncoder.BigValueCodingLength0(val, (int)numericUpDown_maxheight.Value);
                  else
                     bits = Encoder.TileEncoder.BigValueCodingLength1(val, (int)numericUpDown_maxheight.Value);
                  break;
            }
         } catch (Exception ex) {
            MessageBox.Show("Fehler: " + ex.Message, "Fehler");
         }

         textBox_SingleTestBin.Clear();
         foreach (var bit in bits) {
            textBox_SingleTestBin.AppendText(bit > 0 ? "1" : ".");
         }

      }

      private List<int> GetDecimalHeights() {
         List<int> lst = new List<int>();

         string tmp = "";
         foreach (string line in textBox_int.Lines) {
            int comment = line.IndexOf('#');
            if (comment > 0)
               tmp += System.Environment.NewLine + line.Substring(0, comment);
            else if (comment < 0)
               tmp += System.Environment.NewLine + line;
         }

         string[] txt = tmp.Split(new string[] { ",", ";", " ", "\t", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
         for (int i = 0; i < txt.Length; i++) {
            int val = ReadAs31IntV2(txt[i]);
            if (val < 0) {
               MessageBox.Show("Symbol '" + txt[i] + "' kann nicht in eine Zahl >= 0 umgewandelt werden.", "Fehler");
               return lst;
            }
            lst.Add(val);
         }
         return lst;
      }


      System.Drawing.Color col_bin_error = System.Drawing.Color.Yellow;
      System.Drawing.Color col_bin_ok = System.Drawing.SystemColors.Window;

      private void textBox_int_TextChanged(object sender, EventArgs e) {
         richTextBox_Bin.BackColor = col_bin_error;
      }

      private void numericUpDown_maxheight_ValueChanged(object sender, EventArgs e) {
         richTextBox_Bin.BackColor = col_bin_error;
      }

      private void numericUpDown_Rangevalue_ValueChanged(object sender, EventArgs e) {
         richTextBox_Bin.BackColor = col_bin_error;
      }

      private void numericUpDown_TilesizeHoriz_ValueChanged(object sender, EventArgs e) {
         richTextBox_Bin.BackColor = col_bin_error;
      }

      private void numericUpDown_TilesizeVert_ValueChanged(object sender, EventArgs e) {
         richTextBox_Bin.BackColor = col_bin_error;
      }
   }
}