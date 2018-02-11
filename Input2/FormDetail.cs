using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Input2 {
   public partial class FormDetail : Form {
      public FormDetail() {
         InitializeComponent();
      }

      /// <summary>
      /// max. Höhendiff.
      /// </summary>
      public int HeightDiff;

      /// <summary>
      /// max. Höhendiff. für Encoder
      /// </summary>
      public int HeightDiffEncoder;

      /// <summary>
      /// 
      /// </summary>
      public byte Codingtype;

      /// <summary>
      /// 
      /// </summary>
      public int Shrink;

      /// <summary>
      /// Kachelbreite (i.a. 64)
      /// </summary>
      public int TileSizeHorz;

      /// <summary>
      /// Kachelhöhe (i.a. 64)
      /// </summary>
      public int TileSizeVert;

      /// <summary>
      /// gewünschte Höhendaten
      /// </summary>
      public List<int> HeightData;


      private void FormDetail_Shown(object sender, EventArgs e) {

         this.KeyPreview = true;

         Encoder.TileEncoder enc = new Encoder.TileEncoder(HeightDiff, HeightDiffEncoder, Codingtype, Shrink, TileSizeHorz, TileSizeVert, HeightData);
         textBox_Encoder.AppendText(string.Format("{0}{1}", enc, Environment.NewLine));

         textBox_Encoder.Visible = false;
         textBox_Data.Visible = false;
         textBox_CodingTypePlateauFollowerZero.Visible = false;
         textBox_CodingTypePlateauFollowerNotZero.Visible = false;
         textBox_CodingTypeStd.Visible = false;

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
                        textBox_Encoder.AppendText(Environment.NewLine);
                        textBox_Data.AppendText(Environment.NewLine);
                     }

                     StringBuilder sb;

                     switch (enc.Elements[i].ElementTyp) {
                        case Encoder.TileEncoder.HeightElement.Typ.Plateau:
                           textBox_Encoder.AppendText(string.Format("Idx={0}, Plateau Length={1} TableIdx={2} Bits={3} [{4}] <{5}>{6}",
                                                                     enc.Elements[i].Column,
                                                                     enc.Elements[i].Data,
                                                                     enc.Elements[i].PlateauTableIdx,
                                                                     enc.Elements[i].PlateauBinBits,
                                                                     enc.Elements[i].GetBinText(),
                                                                     enc.Elements[i].GetPlateauUnitsText(),
                                                                     Environment.NewLine));

                           sb = new StringBuilder();
                           for (int j = 0; j < enc.Elements[i].Data; j++) {
                              if (enc.Elements[i].Column > 0 || j > 0)
                                 sb.Append("\t");
                              sb.Append("*");
                           }
                           textBox_Data.AppendText(sb.ToString());
                           break;

                        case Encoder.TileEncoder.HeightElement.Typ.PlateauFollower:
                           textBox_Encoder.AppendText(string.Format("Idx={0}, PlateauFollower ActualHeigth={1}, Value={2}{3} {4} [{5}] {6}{7} ddiff={8}{9}",
                                                                     enc.Elements[i].Column,
                                                                     enc.ActualHeight,
                                                                     enc.Elements[i].Data,
                                                                     enc.Elements[i].WrappedValue ? " (Wrap)" : "",
                                                                     enc.Elements[i].CalculationType,
                                                                     enc.Elements[i].GetBinText(),
                                                                     enc.Elements[i].Encoding,
                                                                     enc.Elements[i].Encoding == Encoder.TileEncoder.EncodeMode.Hybrid ?
                                                                           enc.Elements[i].HUnit.ToString() :
                                                                           "",
                                                                     enc.Elements[i].PlateauFollowerDdiff,
                                                                     Environment.NewLine));
                           if (enc.Elements[i].Column > 0)
                              textBox_Data.AppendText("\t");

                           textBox_Data.AppendText("[" + enc.Elements[i].Data.ToString() + "]");
                           break;

                        case Encoder.TileEncoder.HeightElement.Typ.Value:
                           textBox_Encoder.AppendText(string.Format("Idx={0}, ActualHeigth={1}, Value={2}{3} {4} [{5}] {6}{7}{8}",
                                                                     enc.Elements[i].Column,
                                                                     enc.ActualHeight,
                                                                     enc.Elements[i].Data,
                                                                     enc.Elements[i].WrappedValue ? " (Wrap)" : "",
                                                                     enc.Elements[i].CalculationType,
                                                                     enc.Elements[i].GetBinText(),
                                                                     enc.Elements[i].Encoding,
                                                                     enc.Elements[i].Encoding == Encoder.TileEncoder.EncodeMode.Hybrid ?
                                                                           enc.Elements[i].HUnit.ToString() :
                                                                           "",
                                                                     Environment.NewLine));
                           if (enc.Elements[i].Column > 0)
                              textBox_Data.AppendText("\t");

                           textBox_Data.AppendText(enc.Elements[i].Data.ToString());
                           break;

                     }
                  }
               }
            }

            for (int i = 0; i < enc.CodingTypeStd_Info.Count; i++) {
               textBox_CodingTypeStd.AppendText(enc.CodingTypeStd_Info[i]);
               textBox_CodingTypeStd.AppendText(Environment.NewLine);
            }
            for (int i = 0; i < enc.CodingTypePlateauFollowerNotZero_Info.Count; i++) {
               textBox_CodingTypePlateauFollowerNotZero.AppendText(enc.CodingTypePlateauFollowerNotZero_Info[i]);
               textBox_CodingTypePlateauFollowerNotZero.AppendText(Environment.NewLine);
            }
            for (int i = 0; i < enc.CodingTypePlateauFollowerZero_Info.Count; i++) {
               textBox_CodingTypePlateauFollowerZero.AppendText(enc.CodingTypePlateauFollowerZero_Info[i]);
               textBox_CodingTypePlateauFollowerZero.AppendText(Environment.NewLine);
            }

         } catch (Exception ex) {
            textBox_Encoder.AppendText("Exception: " + ex.Message);
         } finally {
            tabControl1.SelectedIndex = 0;
            Caret2End(textBox_Encoder);
            tabControl1.SelectedIndex = 1;
            Caret2End(textBox_Data);
            tabControl1.SelectedIndex = 2;
            Caret2End(textBox_CodingTypeStd);
            tabControl1.SelectedIndex = 3;
            Caret2End(textBox_CodingTypePlateauFollowerNotZero);
            tabControl1.SelectedIndex = 4;
            Caret2End(textBox_CodingTypePlateauFollowerZero);

            tabControl1.SelectedIndex = 0;
         }

      }

      void Caret2End(TextBox tb) {
         tb.Visible = true;
         tb.Select(tb.TextLength, 0);
         tb.ScrollToCaret();
         tb.Focus();
      }

      protected override void OnKeyDown(KeyEventArgs e) {
         base.OnKeyDown(e);
         if (Keys.Escape == e.KeyData)
            Close();
      }

   }
}
