using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Input1 {
   public partial class FormDetail : Form {
      public FormDetail() {
         InitializeComponent();
      }

      /// <summary>
      /// max. Höhe
      /// </summary>
      public int HeightMax;

      /// <summary>
      /// z.Z. nicht verwendet
      /// </summary>
      public byte Codingtype;

      /// <summary>
      /// Kachelgröße (i.a. 64)
      /// </summary>
      public int TileSize;

      /// <summary>
      /// gewünschte Höhendaten
      /// </summary>
      public List<int> HeightData;


      private void FormDetail_Shown(object sender, EventArgs e) {

         this.KeyPreview = true;

         Encoder.TileEncoder enc = new Encoder.TileEncoder(HeightMax, Codingtype, TileSize, HeightData);
         textBox_Encoder.AppendText(string.Format("{0}{1}", enc, System.Environment.NewLine));

         textBox_Encoder.Visible = false;
         textBox_Data.Visible = false;

         try {

            int line = -1;
            int count = -1;
            bool bTileIsFull = false;
            while (count != 0 && !bTileIsFull) {
               count = enc.ComputeNext(out bTileIsFull);
               if (count > 0) {
                  for (int i = enc.Elements.Count - count; i < enc.Elements.Count; i++) {

                     if (enc.Elements[i].Line >= 0 &&
                         line != enc.Elements[i].Line) {
                        line = enc.Elements[i].Line;
                        textBox_Encoder.AppendText("Line " + line.ToString());
                        textBox_Encoder.AppendText(System.Environment.NewLine);
                        textBox_Data.AppendText(System.Environment.NewLine);
                     }

                     switch (enc.Elements[i].ElementTyp) {
                        case Encoder.TileEncoder.HeightElement.Typ.Plateau:
                           textBox_Encoder.AppendText(string.Format("Idx={0}, Plateau Length={1} TableIdx={2} Bits={3} [{4}] <{5}>{6}",
                                                                     enc.Elements[i].Column,
                                                                     enc.Elements[i].Data,
                                                                     enc.Elements[i].PlateauTableIdx,
                                                                     enc.Elements[i].PlateauBinBits,
                                                                     enc.Elements[i].GetBinText(),
                                                                     enc.Elements[i].GetPlateauUnitsText(),
                                                                     System.Environment.NewLine));

                           StringBuilder sb = new StringBuilder();
                           for (int j = 0; j < enc.Elements[i].Data; j++) {
                              if (enc.Elements[i].Column > 0 || j > 0)
                                 sb.Append("\t");
                              sb.Append("*");
                           }
                           textBox_Data.AppendText(sb.ToString());
                           break;

                        case Encoder.TileEncoder.HeightElement.Typ.PlateauFollower:
                           textBox_Encoder.AppendText(string.Format("Idx={0}, PlateauFollower ActualHeigth={1}, Value={2} [{3}] {4}{5} ddiff={6}{7}",
                                                                     enc.Elements[i].Column,
                                                                     enc.ActualHeigth,
                                                                     enc.Elements[i].Data,
                                                                     enc.Elements[i].GetBinText(),
                                                                     enc.Elements[i].Encoding,
                                                                     enc.Elements[i].Encoding == Encoder.TileEncoder.EncodeMode.Hybrid ?
                                                                           enc.Elements[i].HUnit.ToString() :
                                                                           "",
                                                                     enc.Elements[i].PlateauFollowerDdiff,
                                                                     System.Environment.NewLine));
                           if (enc.Elements[i].Column > 0)
                              textBox_Data.AppendText("\t");

                           textBox_Data.AppendText("[" + enc.Elements[i].Data.ToString() + "]");
                           break;

                        case Encoder.TileEncoder.HeightElement.Typ.Value:
                           textBox_Encoder.AppendText(string.Format("Idx={0}, ActualHeigth={1}, Value={2} [{3}] {4}{5}{6}",
                                                                     enc.Elements[i].Column,
                                                                     enc.ActualHeigth,
                                                                     enc.Elements[i].Data,
                                                                     enc.Elements[i].GetBinText(),
                                                                     enc.Elements[i].Encoding,
                                                                     enc.Elements[i].Encoding == Encoder.TileEncoder.EncodeMode.Hybrid ?
                                                                           enc.Elements[i].HUnit.ToString() :
                                                                           "",
                                                                     System.Environment.NewLine));
                           if (enc.Elements[i].Column > 0)
                              textBox_Data.AppendText("\t");

                           textBox_Data.AppendText(enc.Elements[i].Data.ToString());
                           break;

                     }
                  }
               }
            }

         } catch (Exception ex) {
            textBox_Encoder.AppendText("Exception: " + ex.Message);
         } finally {
            textBox_Encoder.Visible = true;
            textBox_Encoder.Select(textBox_Encoder.TextLength, 0);
            textBox_Encoder.ScrollToCaret();

            textBox_Data.Visible = true;
            textBox_Data.Select(textBox_Data.TextLength, 0);
            textBox_Data.ScrollToCaret();

            textBox_Encoder.Focus();
         }

      }

      protected override void OnKeyDown(KeyEventArgs e) {
         base.OnKeyDown(e);
         if (Keys.Escape == e.KeyData)
            Close();
      }

   }
}
