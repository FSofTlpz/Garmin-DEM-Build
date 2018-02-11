using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Input2 {
   public partial class Form1 : Form {

      List<bool> bin = new List<bool>();
      List<bool> postbin = new List<bool>();

      bool autostartandend = false;

      string[] Args;
      Options opt;


      public Form1() {
         InitializeComponent();
      }

      public Form1(string[] args) : this() {
         Args = args;
      }


      private void Form1_Load(object sender, EventArgs e) {
         Text += " (" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ")";

         opt = new Options();
         try {
            opt.Evaluate(Args);
            //opt.Evaluate(Environment.GetCommandLineArgs());  // liefert auch den Progname mit
         } catch (Exception ex) {
            MessageBox.Show(ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            Close();
         }
         checkBox_FillBits.Checked = opt.Fillbit1;
         textBox_Patchfile.Text = opt.PatchFilename;
         textBox_PatchPos.Text = opt.PatchAdr.ToString("x");
         richTextBox_Bin.Text = opt.BinInit;

         numericUpDown_MinBytes.Value = opt.PatchRange;

         numericUpDown_TilesizeHoriz.Value = opt.TileWidth;
         numericUpDown_TilesizeVert.Value = opt.TileHeight;

         numericUpDown_Min.Value = opt.BaseHeight;
         textBox_BaseHeightAdr.Text = opt.BaseHeightAdr.ToString("x");
         numericUpDown_maxdiffTabItem.Value = opt.MaxHeightDiff;
         textBox_MaxHeightDiffAdr.Text = opt.MaxHeightDiffAdr.ToString("x");
         numericUpDown_Codingtype.Value = opt.Codingtype;
         textBox_CodingtypeAdr.Text = opt.CodingtypeAdr.ToString("x");

         if (opt.MaxDiffEncoder > 0 && opt.Shrink > 0) {
            numericUpDown_maxdiffEncoder.Value = opt.MaxDiffEncoder;
            numericUpDown_Shrink.Value = opt.Shrink;
         } else if (opt.Shrink > 0) {
            numericUpDown_maxdiffEncoder.Value = (int)Math.Round(opt.MaxHeightDiff / (2 * opt.Shrink + 1.0));
            numericUpDown_Shrink.Value = opt.Shrink;
         } else
            numericUpDown_maxdiffEncoder.Value = opt.MaxHeightDiff;
         textBox_ShrinkAdr.Text = opt.ShrinkAdr.ToString("x");

         checkBox_Normalized.Checked = !opt.RealHeights;
         checkBoxMinMaxAuto.Checked = opt.BaseDiffAuto;

         textBox_Protfile.Text = opt.ProtFilename;
         textBox_Extern.Text = opt.ExternCommand;
         textBox_ExternArgs.Text = string.Join(" ", opt.ExternCommandParams);

         checkBoxMinMaxAuto.Checked = opt.BaseDiffAuto;
         checkBox_Normalized.Checked = !opt.RealHeights;

         autostartandend = opt.AutoStartAndEnd;
      }

      private void Form1_Shown(object sender, EventArgs e) {
         listBox_SingleTest.SelectedIndex = 3;
         if (autostartandend)
            button_Start_Click(button_Start, null);
      }

      private void button_Start_Click(object sender, EventArgs e) {
         string sPatchedFile = textBox_Patchfile.Text.Trim();
         string sPatchAddress = textBox_PatchPos.Text.Trim();

         string sPatchAddressBase = textBox_BaseHeightAdr.Text.Trim();
         string sPatchAddressMaxdiff = textBox_MaxHeightDiffAdr.Text.Trim();
         string sPatchAddressCodingtype = textBox_CodingtypeAdr.Text.Trim();
         string sExternCommand = textBox_Extern.Text.Trim();
         string sExternCommandArgs = textBox_ExternArgs.Text.Trim();
         string sOutfile = textBox_Protfile.Text.Trim();
         bool bOutHex = checkBox_ProtHex.Checked;
         bool bOutBin = checkBox_ProtBin.Checked;
         bool bOutNL = checkBox_ProtNewline.Checked;
         bool bFillBitsTrue = checkBox_FillBits.Checked;
         string sPatchAddressShrink = textBox_ShrinkAdr.Text.Trim();

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

                     if (sPatchAddressBase.Length > 0) {
                        address = ReadHexAs31Int(sPatchAddressBase);
                        if (address < bw.BaseStream.Length) {
                           bw.Seek(address, SeekOrigin.Begin);
                           UInt16 val = Convert.ToUInt16(numericUpDown_Min.Value);
                           bw.Write(val); // wenn in Wirklichkeit nur ein 1-Byte-Wert, wird zwar die MaxDiff überschrieben, aber das mach ja nichts ...
                        }
                     }

                     if (sPatchAddressMaxdiff.Length > 0) {
                        address = ReadHexAs31Int(sPatchAddressMaxdiff);
                        if (address < bw.BaseStream.Length) {
                           bw.Seek(address, SeekOrigin.Begin);
                           UInt16 val = Convert.ToUInt16(numericUpDown_maxdiffTabItem.Value);
                           bw.Write(val); // wenn in Wirklichkeit nur ein 1-Byte-Wert, wird zwar der Codingtype überschrieben, aber das mach ja nichts ...
                        }
                     }

                     if (sPatchAddressCodingtype.Length > 0) {
                        address = ReadHexAs31Int(sPatchAddressCodingtype);
                        if (address < bw.BaseStream.Length) {
                           bw.Seek(address, SeekOrigin.Begin);
                           byte val = Convert.ToByte(numericUpDown_Codingtype.Value);
                           bw.Write(val);
                        }
                     }

                     if (sPatchAddressShrink.Length > 0) {
                        address = ReadHexAs31Int(sPatchAddressShrink);
                        if (address < bw.BaseStream.Length) {
                           bw.Seek(address, SeekOrigin.Begin);
                           byte val = Convert.ToByte(numericUpDown_Shrink.Value);
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
         Encoder.TileEncoder enc = new Encoder.TileEncoder((int)numericUpDown_maxdiffTabItem.Value,
                                                           (int)numericUpDown_maxdiffEncoder.Value,
                                                           (byte)numericUpDown_Codingtype.Value,
                                                           2 * (int)numericUpDown_Shrink.Value + 1,
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
         dlg.HeightDiff = (int)numericUpDown_maxdiffTabItem.Value;
         dlg.HeightDiffEncoder = (int)numericUpDown_maxdiffEncoder.Value;
         dlg.Codingtype = (byte)numericUpDown_Codingtype.Value;
         dlg.Shrink = 2 * (int)numericUpDown_Shrink.Value + 1;
         dlg.TileSizeHorz = (int)numericUpDown_TilesizeHoriz.Value;
         dlg.TileSizeVert = (int)numericUpDown_TilesizeVert.Value;
         dlg.ShowDialog();
      }

      private void button_SingleTest_Click(object sender, EventArgs e) {
         int val = (int)numericUpDown_SingleTest.Value;
         List<byte> bits = new List<byte>();
         /*
            0     Längencodierung L0
            1     Längencodierung L1
            2     Längencodierung L2
            3     Hybrid mit HUnit 1
            4     Hybrid mit HUnit 2
                  Hybrid mit HUnit 4
                  Hybrid mit HUnit 8
                  Hybrid mit HUnit 16
                  Hybrid mit HUnit 32
                  Hybrid mit HUnit 64
                  Hybrid mit HUnit 128
                  Hybrid mit HUnit 256
                  Hybrid mit HUnit 512
                  Hybrid mit HUnit 1024
            14    Hybrid mit HUnit 2048
            15    großer Wert (Hybrid)
            16    großer Wert (L0)
            17    großer Wert (L1)
         */
         try {
            switch (listBox_SingleTest.SelectedIndex) {
               case 0:
                  bits = Encoder.TileEncoder.LengthCoding0(val);
                  break;
               case 1:
                  bits = Encoder.TileEncoder.LengthCoding1(val);
                  break;
               case 2:
                  bits = Encoder.TileEncoder.LengthCoding2(val);
                  break;

               case 15:
                  bits = Encoder.TileEncoder.BigValueCodingHybrid(val, (int)numericUpDown_maxdiffEncoder.Value);
                  break;
               case 16:
                  bits = Encoder.TileEncoder.BigValueCodingLength0(val, (int)numericUpDown_maxdiffEncoder.Value);
                  break;
               case 17:
                  bits = Encoder.TileEncoder.BigValueCodingLength1(val, (int)numericUpDown_maxdiffEncoder.Value);
                  break;

               default:
                  int hunit = (int)Math.Pow(2, listBox_SingleTest.SelectedIndex - 3);
                  if (hunit <= 2048)
                     bits = Encoder.TileEncoder.HybridCoding(val, (int)numericUpDown_maxdiffEncoder.Value, hunit);
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

         int min = int.MaxValue;
         int max = int.MinValue;
         foreach (var item in lst) {
            min = Math.Min(min, item);
            max = Math.Max(max, item);
         }

         /* checkBoxMinMaxAuto.Checked == true bedeuted automatisch, dass es sich um "Realwerte" handelt, die noch normiert werden müssen.
          * checkBox_Normalized.Checked == true bedeuted automatisch, dass es sich NICHT um "Realwerte" handelt.
          * 
          *    checkBoxMinMaxAuto   checkBox_Normalized
          *    true                 true                    Min/Max automat. ermitteln; Höhendaten sind schon auf Min bezogen <-- sinnlos
          *    true                 false                   Min/Max automat. ermitteln; Höhendaten werden auf Min normalisiert
          *    false                true                    Min/Max sind vorgegeben; Höhendaten sind schon auf Min bezogen
          *    false                false                   Min/Max sind vorgegeben; Höhendaten werden auf Min normalisiert
          */

         if (checkBoxMinMaxAuto.Checked) {
            numericUpDown_Min.Value = min;
            numericUpDown_maxdiffEncoder.Value =
            numericUpDown_maxdiffTabItem.Value = max - min;
         }

         if (!checkBox_Normalized.Checked)
            for (int i = 0; i < lst.Count; i++)    // Daten auf min normieren
               lst[i] -= (int)numericUpDown_Min.Value;

         return lst;
      }


      System.Drawing.Color col_bin_error = System.Drawing.Color.Yellow;
      System.Drawing.Color col_bin_ok = System.Drawing.SystemColors.Window;

      private void textBox_int_TextChanged(object sender, EventArgs e) {
         richTextBox_Bin.BackColor = col_bin_error;
      }

      private void numericUpDown_maxdiff_ValueChanged(object sender, EventArgs e) {
         richTextBox_Bin.BackColor = col_bin_error;
      }

      private void numericUpDown_Codingtype_ValueChanged(object sender, EventArgs e) {
         richTextBox_Bin.BackColor = col_bin_error;
      }

      private void numericUpDown_TilesizeHoriz_ValueChanged(object sender, EventArgs e) {
         richTextBox_Bin.BackColor = col_bin_error;
      }

      private void numericUpDown_TilesizeVert_ValueChanged(object sender, EventArgs e) {
         richTextBox_Bin.BackColor = col_bin_error;
      }

      private void numericUpDown_Min_ValueChanged(object sender, EventArgs e) {
         richTextBox_Bin.BackColor = col_bin_error;
      }

      private void checkBoxMinMaxAuto_CheckedChanged(object sender, EventArgs e) {
         numericUpDown_Min.Enabled =
         numericUpDown_maxdiffTabItem.Enabled = !checkBoxMinMaxAuto.Checked;
         if (checkBoxMinMaxAuto.Checked) // nicht beide gleichzeitig true
            checkBox_Normalized.Checked = false;
      }

      private void checkBox_Normalized_CheckedChanged(object sender, EventArgs e) {
         richTextBox_Bin.BackColor = col_bin_error;
         if (checkBox_Normalized.Checked) // nicht beide gleichzeitig true
            checkBoxMinMaxAuto.Checked = false;
      }

      private void numericUpDown_MinBytes_ValueChanged(object sender, EventArgs e) {
         richTextBox_Bin.BackColor = col_bin_error;
      }

      private void numericUpDown_Shrink_ValueChanged(object sender, EventArgs e) {
         richTextBox_Bin.BackColor = col_bin_error;
      }

      private void numericUpDown_maxdiffEncoder_ValueChanged(object sender, EventArgs e) {
         richTextBox_Bin.BackColor = col_bin_error;
      }
   }
}