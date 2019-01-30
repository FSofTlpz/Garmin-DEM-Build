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
      /// Shrink-Faktor (ungerade)
      /// </summary>
      public int Shrink;

      /// <summary>
      /// Höhendaten mit <see cref="Shrink"/> verkleinern
      /// </summary>
      public bool ShrinkHeightData;

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

         Encoder.TileEncoder enc = new Encoder.TileEncoder(HeightDiff, HeightDiffEncoder, Codingtype, Shrink, TileSizeHorz, TileSizeVert, HeightData, ShrinkHeightData);
         textBox_Encoder.AppendText(string.Format("{0}{1}", enc, Environment.NewLine));

         textBox_Encoder.Visible = false;
         textBox_Data.Visible = false;
         textBox_CodingTypePlateauFollowerZero.Visible = false;
         textBox_CodingTypePlateauFollowerNotZero.Visible = false;
         textBox_CodingTypeStd.Visible = false;
         textBox_Hook.Visible = false;
         textBox_Align.Visible = false;
         textBox_Align.AppendText("T: hook>=Max; _: hook<=0; X: Maximalwert; 0: 0; -/t: 0<hook<Max");

         try {

            int line = -1;
            int count = -1;
            bool bTileIsFull = false;
            while (count != 0 && !bTileIsFull) {
               count = enc.ComputeNext(out bTileIsFull);
               if (count > 0) {
                  for (int i = enc.Elements.Count - count; i < enc.Elements.Count; i++) {

                     if (enc.Elements[i].Info.Line >= 0 &&
                         line != enc.Elements[i].Info.Line) {
                        line = enc.Elements[i].Info.Line;
                        textBox_Encoder.AppendText("Line " + line.ToString());
                        textBox_Encoder.AppendText(Environment.NewLine);
                        textBox_Data.AppendText(Environment.NewLine);
                        textBox_Hook.AppendText(Environment.NewLine);
                        textBox_Align.AppendText(Environment.NewLine);
                     }

                     StringBuilder sb;

                     switch (enc.Elements[i].Info.Typ) {
                        case Encoder.TileEncoder.HeightElement.Typ.PlateauLength:
                           //case Encoder.TileEncoder.HeightElement.Typ.Plateau:
                           textBox_Encoder.AppendText(string.Format("Idx={0}, Plateau: (Ddiff={1}, Hook={2} Heigth={3}{4}) Length={5} TableIdx={6} Bits={7} [{8}] <{9}>{10}",
                                                                     enc.Elements[i].Info.Column,
                                                                     enc.Elements[i].Info.Ddiff,
                                                                     enc.Elements[i].Info.Hook,
                                                                     enc.Elements[i].Info.Height,
                                                                     enc.Elements[i].Info.TopAligned ? " TopAligned" : "",
                                                                     enc.Elements[i].Info.Data,
                                                                     enc.Elements[i].PlateauTableIdx,
                                                                     enc.Elements[i].PlateauBinBits,
                                                                     enc.Elements[i].GetBinText(),
                                                                     enc.Elements[i].GetPlateauUnitsText(),
                                                                     Environment.NewLine));

                           sb = new StringBuilder();
                           for (int j = 0; j < enc.Elements[i].Info.Data; j++) {
                              if (enc.Elements[i].Info.Column > 0 || j > 0)
                                 sb.Append("\t");
                              sb.Append("*");
                           }
                           textBox_Data.AppendText(sb.ToString());

                           sb.Clear();
                           for (int j = 0; j < enc.Elements[i].Info.Data; j++) {
                              if (enc.Elements[i].Info.Column > 0 || j > 0)
                                 sb.Append("\t");
                              sb.Append("*");
                           }
                           textBox_Hook.AppendText(sb.ToString());

                           sb.Clear();
                           string symbol = GetTopAlignedSymbol(enc.Elements[i], enc);
                           for (int j = 0; j < enc.Elements[i].Info.Data; j++) {
                              if (enc.Elements[i].Info.Column > 0 || j > 0)
                                 sb.Append("\t");
                              sb.Append(symbol);
                           }
                           textBox_Align.AppendText(sb.ToString());

                           break;

                        case Encoder.TileEncoder.HeightElement.Typ.PlateauFollower:
                        case Encoder.TileEncoder.HeightElement.Typ.PlateauFollower0:
                           textBox_Encoder.AppendText(string.Format("Idx={0}, ActualHeigth={1} {2}, Data={3}, Ddiff={4}, Hook={5}{6}{7}{8} [{9}] {10}{11}{12}",
                                                                     enc.Elements[i].Info.Column,
                                                                     enc.Elements[i].Info.Height,
                                                                     enc.Elements[i].Info.Typ,
                                                                     enc.Elements[i].Info.Data,
                                                                     enc.Elements[i].Info.Ddiff,
                                                                     enc.Elements[i].Info.Hook,
                                                                     enc.Elements[i].Info.Wrapped ? " (Wrap)" : "",
                                                                     enc.Elements[i].Info.TopAligned ? " (TopAligned)" : "",
                                                                     enc.Elements[i].Info.Alignment != Encoder.TileEncoder.Shrink.Align3Type.TA000 ? " (" + enc.Elements[i].Info.Alignment.ToString() + ")" : "",
                                                                     enc.Elements[i].GetBinText(),
                                                                     enc.Elements[i].Info.EncMode,
                                                                     enc.Elements[i].Info.EncMode == Encoder.TileEncoder.EncodeMode.Hybrid ?
                                                                           enc.Elements[i].Info.Hunit.ToString() :
                                                                           "",
                                                                     Environment.NewLine));





                           if (enc.Elements[i].Info.Column > 0) {
                              textBox_Data.AppendText("\t");
                              textBox_Hook.AppendText("\t");
                              textBox_Align.AppendText("\t");
                           }

                           textBox_Data.AppendText("[" + enc.Elements[i].Info.Data.ToString() + "]");
                           textBox_Hook.AppendText(enc.Elements[i].Info.Hook.ToString());
                           textBox_Align.AppendText(GetTopAlignedSymbol(enc.Elements[i], enc));
                           break;

                        case Encoder.TileEncoder.HeightElement.Typ.ValueHookHigh:
                        case Encoder.TileEncoder.HeightElement.Typ.ValueHookMiddle:
                        case Encoder.TileEncoder.HeightElement.Typ.ValueHookLow:
                           textBox_Encoder.AppendText(string.Format("Idx={0}, ActualHeigth={1} {2}, Data={3}, Hook={4}{5}{6}{7} [{8}] {9}{10}{11}",
                                                                     enc.Elements[i].Info.Column,
                                                                     enc.Elements[i].Info.Height,
                                                                     enc.Elements[i].Info.Typ,
                                                                     enc.Elements[i].Info.Data,
                                                                     enc.Elements[i].Info.Hook,
                                                                     enc.Elements[i].Info.Wrapped ? " (Wrap)" : "",
                                                                     enc.Elements[i].Info.TopAligned ? " (TopAligned)" : "",
                                                                     enc.Elements[i].Info.Alignment != Encoder.TileEncoder.Shrink.Align3Type.TA000 ? " (" + enc.Elements[i].Info.Alignment.ToString() + ")" : "",
                                                                     enc.Elements[i].GetBinText(),
                                                                     enc.Elements[i].Info.EncMode,
                                                                     enc.Elements[i].Info.EncMode == Encoder.TileEncoder.EncodeMode.Hybrid ?
                                                                           enc.Elements[i].Info.Hunit.ToString() :
                                                                           "",
                                                                     Environment.NewLine));
                           if (enc.Elements[i].Info.Column > 0) {
                              textBox_Data.AppendText("\t");
                              textBox_Hook.AppendText("\t");
                              textBox_Align.AppendText("\t");
                           }

                           textBox_Data.AppendText(enc.Elements[i].Info.Data.ToString());
                           textBox_Hook.AppendText(enc.Elements[i].Info.Hook.ToString());
                           textBox_Align.AppendText(GetTopAlignedSymbol(enc.Elements[i], enc));
                           break;

                     }
                  }
               }
            }
            textBox_Data.AppendText(Environment.NewLine);
            textBox_Hook.AppendText(Environment.NewLine);
            textBox_Align.AppendText(Environment.NewLine);

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
            tabControl1.SelectedIndex = 6;
            Caret2End(textBox_Align);
            tabControl1.SelectedIndex = 5;
            Caret2End(textBox_Hook);

            tabControl1.SelectedIndex = 0;
         }

      }

      void Caret2End(TextBox tb) {
         tb.Visible = true;
         tb.Select(tb.TextLength, 0);
         tb.ScrollToCaret();
         tb.Focus();
      }

      string GetTopAlignedSymbol(Encoder.TileEncoder.HeightElement he, Encoder.TileEncoder enc) {
         if (he.Info.Typ != Encoder.TileEncoder.HeightElement.Typ.PlateauLength) {
            if (he.Info.Height == 0)
               return "0";
            if (he.Info.Height == enc.MaxHeight)
               return "X";
            return he.Info.TopAligned ?
                        he.Info.Hook >= enc.MaxHeight ? "T" : "t" :
                        he.Info.Hook <= 0 ? "_" : "-";
         } else {
            if (he.Info.Height == 0)
               return "0*";
            if (he.Info.Height == enc.MaxHeight)
               return "M*";
            return he.Info.TopAligned ? "t*" : "-*";
         }
      }

      protected override void OnKeyDown(KeyEventArgs e) {
         base.OnKeyDown(e);
         if (Keys.Escape == e.KeyData)
            Close();
      }

   }
}
