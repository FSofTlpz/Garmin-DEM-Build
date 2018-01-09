namespace Input2 {
   partial class Form1 {
      /// <summary>
      /// Erforderliche Designervariable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Verwendete Ressourcen bereinigen.
      /// </summary>
      /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
      protected override void Dispose(bool disposing) {
         if (disposing && (components != null)) {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Vom Windows Form-Designer generierter Code

      /// <summary>
      /// Erforderliche Methode für die Designerunterstützung.
      /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
      /// </summary>
      private void InitializeComponent() {
         this.textBox_PatchPos = new System.Windows.Forms.TextBox();
         this.label1 = new System.Windows.Forms.Label();
         this.checkBox_FillBits = new System.Windows.Forms.CheckBox();
         this.textBox2 = new System.Windows.Forms.TextBox();
         this.label2 = new System.Windows.Forms.Label();
         this.textBox_Patchfile = new System.Windows.Forms.TextBox();
         this.textBox_Protfile = new System.Windows.Forms.TextBox();
         this.label3 = new System.Windows.Forms.Label();
         this.checkBox_ProtNewline = new System.Windows.Forms.CheckBox();
         this.textBox_Extern = new System.Windows.Forms.TextBox();
         this.label4 = new System.Windows.Forms.Label();
         this.button_Start = new System.Windows.Forms.Button();
         this.checkBox_ProtHex = new System.Windows.Forms.CheckBox();
         this.checkBox_ProtBin = new System.Windows.Forms.CheckBox();
         this.textBox1 = new System.Windows.Forms.TextBox();
         this.numericUpDown_MinBytes = new System.Windows.Forms.NumericUpDown();
         this.label5 = new System.Windows.Forms.Label();
         this.textBox_int = new System.Windows.Forms.TextBox();
         this.button_dec2bin = new System.Windows.Forms.Button();
         this.numericUpDown_maxdiff = new System.Windows.Forms.NumericUpDown();
         this.label6 = new System.Windows.Forms.Label();
         this.label7 = new System.Windows.Forms.Label();
         this.button_Detail = new System.Windows.Forms.Button();
         this.label9 = new System.Windows.Forms.Label();
         this.numericUpDown_TilesizeHoriz = new System.Windows.Forms.NumericUpDown();
         this.groupBox1 = new System.Windows.Forms.GroupBox();
         this.button_SingleTest = new System.Windows.Forms.Button();
         this.textBox_SingleTestBin = new System.Windows.Forms.TextBox();
         this.numericUpDown_SingleTest = new System.Windows.Forms.NumericUpDown();
         this.listBox_SingleTest = new System.Windows.Forms.ListBox();
         this.groupBox2 = new System.Windows.Forms.GroupBox();
         this.checkBox_Normalized = new System.Windows.Forms.CheckBox();
         this.groupBox5 = new System.Windows.Forms.GroupBox();
         this.label8 = new System.Windows.Forms.Label();
         this.numericUpDown_Min = new System.Windows.Forms.NumericUpDown();
         this.textBox_BaseHeightAdr = new System.Windows.Forms.TextBox();
         this.checkBoxMinMaxAuto = new System.Windows.Forms.CheckBox();
         this.groupBox4 = new System.Windows.Forms.GroupBox();
         this.label13 = new System.Windows.Forms.Label();
         this.textBox_MaxHeightDiffAdr = new System.Windows.Forms.TextBox();
         this.groupBox3 = new System.Windows.Forms.GroupBox();
         this.numericUpDown_Codingtype = new System.Windows.Forms.NumericUpDown();
         this.label14 = new System.Windows.Forms.Label();
         this.textBox_CodingtypeAdr = new System.Windows.Forms.TextBox();
         this.numericUpDown_TilesizeVert = new System.Windows.Forms.NumericUpDown();
         this.label15 = new System.Windows.Forms.Label();
         this.richTextBox_Bin = new System.Windows.Forms.RichTextBox();
         this.label_Bytes = new System.Windows.Forms.Label();
         this.textBox_PostBits = new System.Windows.Forms.TextBox();
         this.label10 = new System.Windows.Forms.Label();
         this.textBox_ExternArgs = new System.Windows.Forms.TextBox();
         this.label12 = new System.Windows.Forms.Label();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_MinBytes)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_maxdiff)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_TilesizeHoriz)).BeginInit();
         this.groupBox1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_SingleTest)).BeginInit();
         this.groupBox2.SuspendLayout();
         this.groupBox5.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Min)).BeginInit();
         this.groupBox4.SuspendLayout();
         this.groupBox3.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Codingtype)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_TilesizeVert)).BeginInit();
         this.SuspendLayout();
         // 
         // textBox_PatchPos
         // 
         this.textBox_PatchPos.Location = new System.Drawing.Point(141, 38);
         this.textBox_PatchPos.Name = "textBox_PatchPos";
         this.textBox_PatchPos.Size = new System.Drawing.Size(100, 20);
         this.textBox_PatchPos.TabIndex = 3;
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(12, 41);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(114, 13);
         this.label1.TabIndex = 2;
         this.label1.Text = "Patchadr. Daten (hex):";
         // 
         // checkBox_FillBits
         // 
         this.checkBox_FillBits.AutoSize = true;
         this.checkBox_FillBits.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
         this.checkBox_FillBits.Checked = true;
         this.checkBox_FillBits.CheckState = System.Windows.Forms.CheckState.Checked;
         this.checkBox_FillBits.Location = new System.Drawing.Point(12, 133);
         this.checkBox_FillBits.Name = "checkBox_FillBits";
         this.checkBox_FillBits.Size = new System.Drawing.Size(145, 17);
         this.checkBox_FillBits.TabIndex = 13;
         this.checkBox_FillBits.Text = "Auffüllbits sind 1 (sonst 0)";
         this.checkBox_FillBits.UseVisualStyleBackColor = true;
         // 
         // textBox2
         // 
         this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox2.Enabled = false;
         this.textBox2.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.textBox2.Location = new System.Drawing.Point(70, 18);
         this.textBox2.Name = "textBox2";
         this.textBox2.ReadOnly = true;
         this.textBox2.Size = new System.Drawing.Size(612, 23);
         this.textBox2.TabIndex = 0;
         this.textBox2.Text = "0  .   !1  .   !2  .   !3  .   !4  .   !5  .   !6  .   !7  .   !8  .   !9  .   !1" +
    "0 .   !11 .   !12 .   !13 .   !14 .   !15 .   !";
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(12, 15);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(86, 13);
         this.label2.TabIndex = 0;
         this.label2.Text = "gepatchte Datei:";
         // 
         // textBox_Patchfile
         // 
         this.textBox_Patchfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox_Patchfile.Location = new System.Drawing.Point(141, 12);
         this.textBox_Patchfile.Name = "textBox_Patchfile";
         this.textBox_Patchfile.Size = new System.Drawing.Size(562, 20);
         this.textBox_Patchfile.TabIndex = 1;
         // 
         // textBox_Protfile
         // 
         this.textBox_Protfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox_Protfile.Location = new System.Drawing.Point(141, 64);
         this.textBox_Protfile.Name = "textBox_Protfile";
         this.textBox_Protfile.Size = new System.Drawing.Size(562, 20);
         this.textBox_Protfile.TabIndex = 5;
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(12, 67);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(75, 13);
         this.label3.TabIndex = 4;
         this.label3.Text = "Ausgabedatei:";
         // 
         // checkBox_ProtNewline
         // 
         this.checkBox_ProtNewline.AutoSize = true;
         this.checkBox_ProtNewline.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
         this.checkBox_ProtNewline.Checked = true;
         this.checkBox_ProtNewline.CheckState = System.Windows.Forms.CheckState.Checked;
         this.checkBox_ProtNewline.Location = new System.Drawing.Point(241, 90);
         this.checkBox_ProtNewline.Name = "checkBox_ProtNewline";
         this.checkBox_ProtNewline.Size = new System.Drawing.Size(139, 17);
         this.checkBox_ProtNewline.TabIndex = 8;
         this.checkBox_ProtNewline.Text = "Newline nach Ausgabe:";
         this.checkBox_ProtNewline.UseVisualStyleBackColor = true;
         // 
         // textBox_Extern
         // 
         this.textBox_Extern.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox_Extern.Location = new System.Drawing.Point(141, 107);
         this.textBox_Extern.Name = "textBox_Extern";
         this.textBox_Extern.Size = new System.Drawing.Size(321, 20);
         this.textBox_Extern.TabIndex = 10;
         // 
         // label4
         // 
         this.label4.AutoSize = true;
         this.label4.Location = new System.Drawing.Point(12, 110);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(88, 13);
         this.label4.TabIndex = 9;
         this.label4.Text = "externer Prozess:";
         // 
         // button_Start
         // 
         this.button_Start.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.button_Start.Location = new System.Drawing.Point(6, 301);
         this.button_Start.Name = "button_Start";
         this.button_Start.Size = new System.Drawing.Size(676, 27);
         this.button_Start.TabIndex = 20;
         this.button_Start.Text = "&Start";
         this.button_Start.UseVisualStyleBackColor = true;
         this.button_Start.Click += new System.EventHandler(this.button_Start_Click);
         // 
         // checkBox_ProtHex
         // 
         this.checkBox_ProtHex.AutoSize = true;
         this.checkBox_ProtHex.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
         this.checkBox_ProtHex.Location = new System.Drawing.Point(11, 90);
         this.checkBox_ProtHex.Name = "checkBox_ProtHex";
         this.checkBox_ProtHex.Size = new System.Drawing.Size(93, 17);
         this.checkBox_ProtHex.TabIndex = 6;
         this.checkBox_ProtHex.Text = "Hex-Ausgabe:";
         this.checkBox_ProtHex.UseVisualStyleBackColor = true;
         // 
         // checkBox_ProtBin
         // 
         this.checkBox_ProtBin.AutoSize = true;
         this.checkBox_ProtBin.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
         this.checkBox_ProtBin.Checked = true;
         this.checkBox_ProtBin.CheckState = System.Windows.Forms.CheckState.Checked;
         this.checkBox_ProtBin.Location = new System.Drawing.Point(123, 90);
         this.checkBox_ProtBin.Name = "checkBox_ProtBin";
         this.checkBox_ProtBin.Size = new System.Drawing.Size(89, 17);
         this.checkBox_ProtBin.TabIndex = 7;
         this.checkBox_ProtBin.Text = "Bin-Ausgabe:";
         this.checkBox_ProtBin.UseVisualStyleBackColor = true;
         // 
         // textBox1
         // 
         this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox1.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.textBox1.Location = new System.Drawing.Point(70, 73);
         this.textBox1.Name = "textBox1";
         this.textBox1.Size = new System.Drawing.Size(612, 23);
         this.textBox1.TabIndex = 4;
         // 
         // numericUpDown_MinBytes
         // 
         this.numericUpDown_MinBytes.Location = new System.Drawing.Point(312, 130);
         this.numericUpDown_MinBytes.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
         this.numericUpDown_MinBytes.Name = "numericUpDown_MinBytes";
         this.numericUpDown_MinBytes.Size = new System.Drawing.Size(68, 20);
         this.numericUpDown_MinBytes.TabIndex = 15;
         // 
         // label5
         // 
         this.label5.AutoSize = true;
         this.label5.Location = new System.Drawing.Point(199, 134);
         this.label5.Name = "label5";
         this.label5.Size = new System.Drawing.Size(107, 13);
         this.label5.TabIndex = 14;
         this.label5.Text = "Mindestanzahl Bytes:";
         // 
         // textBox_int
         // 
         this.textBox_int.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox_int.Font = new System.Drawing.Font("Courier New", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.textBox_int.Location = new System.Drawing.Point(84, 206);
         this.textBox_int.Multiline = true;
         this.textBox_int.Name = "textBox_int";
         this.textBox_int.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.textBox_int.Size = new System.Drawing.Size(598, 89);
         this.textBox_int.TabIndex = 15;
         this.textBox_int.WordWrap = false;
         this.textBox_int.TextChanged += new System.EventHandler(this.textBox_int_TextChanged);
         // 
         // button_dec2bin
         // 
         this.button_dec2bin.Location = new System.Drawing.Point(9, 204);
         this.button_dec2bin.Name = "button_dec2bin";
         this.button_dec2bin.Size = new System.Drawing.Size(72, 23);
         this.button_dec2bin.TabIndex = 18;
         this.button_dec2bin.Text = "&Dez -> Bin";
         this.button_dec2bin.UseVisualStyleBackColor = true;
         this.button_dec2bin.Click += new System.EventHandler(this.button_dec2bin_Click);
         // 
         // numericUpDown_maxdiff
         // 
         this.numericUpDown_maxdiff.Location = new System.Drawing.Point(7, 19);
         this.numericUpDown_maxdiff.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
         this.numericUpDown_maxdiff.Name = "numericUpDown_maxdiff";
         this.numericUpDown_maxdiff.Size = new System.Drawing.Size(51, 20);
         this.numericUpDown_maxdiff.TabIndex = 0;
         this.numericUpDown_maxdiff.Value = new decimal(new int[] {
            158,
            0,
            0,
            0});
         this.numericUpDown_maxdiff.ValueChanged += new System.EventHandler(this.numericUpDown_maxdiff_ValueChanged);
         // 
         // label6
         // 
         this.label6.AutoSize = true;
         this.label6.Location = new System.Drawing.Point(81, 184);
         this.label6.Name = "label6";
         this.label6.Size = new System.Drawing.Size(80, 13);
         this.label6.TabIndex = 14;
         this.label6.Text = "Höhen de&zimal:";
         // 
         // label7
         // 
         this.label7.AutoSize = true;
         this.label7.Location = new System.Drawing.Point(67, 48);
         this.label7.Name = "label7";
         this.label7.Size = new System.Drawing.Size(21, 13);
         this.label7.TabIndex = 2;
         this.label7.Text = "&bin";
         // 
         // button_Detail
         // 
         this.button_Detail.Location = new System.Drawing.Point(9, 233);
         this.button_Detail.Name = "button_Detail";
         this.button_Detail.Size = new System.Drawing.Size(72, 23);
         this.button_Detail.TabIndex = 19;
         this.button_Detail.Text = "De&tails";
         this.button_Detail.UseVisualStyleBackColor = true;
         this.button_Detail.Click += new System.EventHandler(this.button_Detail_Click);
         // 
         // label9
         // 
         this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.label9.AutoSize = true;
         this.label9.Location = new System.Drawing.Point(498, 109);
         this.label9.Name = "label9";
         this.label9.Size = new System.Drawing.Size(59, 13);
         this.label9.TabIndex = 7;
         this.label9.Text = "Tile-Größe:";
         // 
         // numericUpDown_TilesizeHoriz
         // 
         this.numericUpDown_TilesizeHoriz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.numericUpDown_TilesizeHoriz.Location = new System.Drawing.Point(563, 104);
         this.numericUpDown_TilesizeHoriz.Maximum = new decimal(new int[] {
            512,
            0,
            0,
            0});
         this.numericUpDown_TilesizeHoriz.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
         this.numericUpDown_TilesizeHoriz.Name = "numericUpDown_TilesizeHoriz";
         this.numericUpDown_TilesizeHoriz.Size = new System.Drawing.Size(48, 20);
         this.numericUpDown_TilesizeHoriz.TabIndex = 8;
         this.numericUpDown_TilesizeHoriz.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
         this.numericUpDown_TilesizeHoriz.ValueChanged += new System.EventHandler(this.numericUpDown_TilesizeHoriz_ValueChanged);
         // 
         // groupBox1
         // 
         this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
         this.groupBox1.Controls.Add(this.button_SingleTest);
         this.groupBox1.Controls.Add(this.textBox_SingleTestBin);
         this.groupBox1.Controls.Add(this.numericUpDown_SingleTest);
         this.groupBox1.Controls.Add(this.listBox_SingleTest);
         this.groupBox1.ForeColor = System.Drawing.Color.Blue;
         this.groupBox1.Location = new System.Drawing.Point(15, 156);
         this.groupBox1.Name = "groupBox1";
         this.groupBox1.Size = new System.Drawing.Size(688, 89);
         this.groupBox1.TabIndex = 16;
         this.groupBox1.TabStop = false;
         this.groupBox1.Text = "Codierung einer einzelnen Zahl";
         // 
         // button_SingleTest
         // 
         this.button_SingleTest.Location = new System.Drawing.Point(267, 16);
         this.button_SingleTest.Name = "button_SingleTest";
         this.button_SingleTest.Size = new System.Drawing.Size(98, 23);
         this.button_SingleTest.TabIndex = 2;
         this.button_SingleTest.Text = "&Codieren";
         this.button_SingleTest.UseVisualStyleBackColor = true;
         this.button_SingleTest.Click += new System.EventHandler(this.button_SingleTest_Click);
         // 
         // textBox_SingleTestBin
         // 
         this.textBox_SingleTestBin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox_SingleTestBin.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.textBox_SingleTestBin.Location = new System.Drawing.Point(169, 60);
         this.textBox_SingleTestBin.Name = "textBox_SingleTestBin";
         this.textBox_SingleTestBin.Size = new System.Drawing.Size(513, 23);
         this.textBox_SingleTestBin.TabIndex = 3;
         // 
         // numericUpDown_SingleTest
         // 
         this.numericUpDown_SingleTest.Location = new System.Drawing.Point(169, 19);
         this.numericUpDown_SingleTest.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
         this.numericUpDown_SingleTest.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
         this.numericUpDown_SingleTest.Name = "numericUpDown_SingleTest";
         this.numericUpDown_SingleTest.Size = new System.Drawing.Size(77, 20);
         this.numericUpDown_SingleTest.TabIndex = 1;
         // 
         // listBox_SingleTest
         // 
         this.listBox_SingleTest.FormattingEnabled = true;
         this.listBox_SingleTest.IntegralHeight = false;
         this.listBox_SingleTest.Items.AddRange(new object[] {
            "Längencodierung L0",
            "Längencodierung L1",
            "Längencodierung L2",
            "Hybrid mit HUnit 1",
            "Hybrid mit HUnit 2",
            "Hybrid mit HUnit 4",
            "Hybrid mit HUnit 8",
            "Hybrid mit HUnit 16",
            "Hybrid mit HUnit 32",
            "Hybrid mit HUnit 64",
            "Hybrid mit HUnit 128",
            "Hybrid mit HUnit 256",
            "Hybrid mit HUnit 512",
            "Hybrid mit HUnit 1024",
            "Hybrid mit HUnit 2048",
            "großer Wert (Hybrid)",
            "großer Wert (L0)",
            "großer Wert (L1)"});
         this.listBox_SingleTest.Location = new System.Drawing.Point(6, 19);
         this.listBox_SingleTest.Name = "listBox_SingleTest";
         this.listBox_SingleTest.Size = new System.Drawing.Size(147, 64);
         this.listBox_SingleTest.TabIndex = 0;
         // 
         // groupBox2
         // 
         this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.groupBox2.Controls.Add(this.checkBox_Normalized);
         this.groupBox2.Controls.Add(this.groupBox5);
         this.groupBox2.Controls.Add(this.checkBoxMinMaxAuto);
         this.groupBox2.Controls.Add(this.groupBox4);
         this.groupBox2.Controls.Add(this.groupBox3);
         this.groupBox2.Controls.Add(this.numericUpDown_TilesizeVert);
         this.groupBox2.Controls.Add(this.label15);
         this.groupBox2.Controls.Add(this.richTextBox_Bin);
         this.groupBox2.Controls.Add(this.label_Bytes);
         this.groupBox2.Controls.Add(this.textBox_PostBits);
         this.groupBox2.Controls.Add(this.label10);
         this.groupBox2.Controls.Add(this.textBox2);
         this.groupBox2.Controls.Add(this.button_Start);
         this.groupBox2.Controls.Add(this.numericUpDown_TilesizeHoriz);
         this.groupBox2.Controls.Add(this.textBox1);
         this.groupBox2.Controls.Add(this.label9);
         this.groupBox2.Controls.Add(this.textBox_int);
         this.groupBox2.Controls.Add(this.button_dec2bin);
         this.groupBox2.Controls.Add(this.button_Detail);
         this.groupBox2.Controls.Add(this.label6);
         this.groupBox2.Controls.Add(this.label7);
         this.groupBox2.ForeColor = System.Drawing.SystemColors.ControlText;
         this.groupBox2.Location = new System.Drawing.Point(15, 251);
         this.groupBox2.Name = "groupBox2";
         this.groupBox2.Size = new System.Drawing.Size(688, 334);
         this.groupBox2.TabIndex = 0;
         this.groupBox2.TabStop = false;
         this.groupBox2.Text = "Test";
         // 
         // checkBox_Normalized
         // 
         this.checkBox_Normalized.AutoSize = true;
         this.checkBox_Normalized.Location = new System.Drawing.Point(425, 183);
         this.checkBox_Normalized.Name = "checkBox_Normalized";
         this.checkBox_Normalized.Size = new System.Drawing.Size(228, 17);
         this.checkBox_Normalized.TabIndex = 17;
         this.checkBox_Normalized.Text = "Höhenangaben sind auf Minimum bezogen";
         this.checkBox_Normalized.UseVisualStyleBackColor = true;
         this.checkBox_Normalized.CheckedChanged += new System.EventHandler(this.checkBox_Normalized_CheckedChanged);
         // 
         // groupBox5
         // 
         this.groupBox5.Controls.Add(this.label8);
         this.groupBox5.Controls.Add(this.numericUpDown_Min);
         this.groupBox5.Controls.Add(this.textBox_BaseHeightAdr);
         this.groupBox5.Location = new System.Drawing.Point(9, 131);
         this.groupBox5.Name = "groupBox5";
         this.groupBox5.Size = new System.Drawing.Size(223, 46);
         this.groupBox5.TabIndex = 11;
         this.groupBox5.TabStop = false;
         this.groupBox5.Text = "Min.";
         // 
         // label8
         // 
         this.label8.AutoSize = true;
         this.label8.Location = new System.Drawing.Point(64, 21);
         this.label8.Name = "label8";
         this.label8.Size = new System.Drawing.Size(82, 13);
         this.label8.TabIndex = 1;
         this.label8.Text = "Patchadr. (hex):";
         // 
         // numericUpDown_Min
         // 
         this.numericUpDown_Min.Location = new System.Drawing.Point(7, 19);
         this.numericUpDown_Min.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
         this.numericUpDown_Min.Name = "numericUpDown_Min";
         this.numericUpDown_Min.Size = new System.Drawing.Size(51, 20);
         this.numericUpDown_Min.TabIndex = 0;
         this.numericUpDown_Min.ValueChanged += new System.EventHandler(this.numericUpDown_Min_ValueChanged);
         // 
         // textBox_BaseHeightAdr
         // 
         this.textBox_BaseHeightAdr.Location = new System.Drawing.Point(152, 18);
         this.textBox_BaseHeightAdr.Name = "textBox_BaseHeightAdr";
         this.textBox_BaseHeightAdr.Size = new System.Drawing.Size(61, 20);
         this.textBox_BaseHeightAdr.TabIndex = 2;
         // 
         // checkBoxMinMaxAuto
         // 
         this.checkBoxMinMaxAuto.AutoSize = true;
         this.checkBoxMinMaxAuto.Location = new System.Drawing.Point(193, 183);
         this.checkBoxMinMaxAuto.Name = "checkBoxMinMaxAuto";
         this.checkBoxMinMaxAuto.Size = new System.Drawing.Size(226, 17);
         this.checkBoxMinMaxAuto.TabIndex = 16;
         this.checkBoxMinMaxAuto.Text = "Min./Diff. aus realen Höhendaten ermitteln";
         this.checkBoxMinMaxAuto.UseVisualStyleBackColor = true;
         this.checkBoxMinMaxAuto.CheckedChanged += new System.EventHandler(this.checkBoxMinMaxAuto_CheckedChanged);
         // 
         // groupBox4
         // 
         this.groupBox4.Controls.Add(this.label13);
         this.groupBox4.Controls.Add(this.numericUpDown_maxdiff);
         this.groupBox4.Controls.Add(this.textBox_MaxHeightDiffAdr);
         this.groupBox4.Location = new System.Drawing.Point(238, 131);
         this.groupBox4.Name = "groupBox4";
         this.groupBox4.Size = new System.Drawing.Size(223, 46);
         this.groupBox4.TabIndex = 12;
         this.groupBox4.TabStop = false;
         this.groupBox4.Text = "max. Diff.";
         // 
         // label13
         // 
         this.label13.AutoSize = true;
         this.label13.Location = new System.Drawing.Point(64, 21);
         this.label13.Name = "label13";
         this.label13.Size = new System.Drawing.Size(82, 13);
         this.label13.TabIndex = 1;
         this.label13.Text = "Patchadr. (hex):";
         // 
         // textBox_MaxHeightDiffAdr
         // 
         this.textBox_MaxHeightDiffAdr.Location = new System.Drawing.Point(152, 18);
         this.textBox_MaxHeightDiffAdr.Name = "textBox_MaxHeightDiffAdr";
         this.textBox_MaxHeightDiffAdr.Size = new System.Drawing.Size(61, 20);
         this.textBox_MaxHeightDiffAdr.TabIndex = 2;
         // 
         // groupBox3
         // 
         this.groupBox3.Controls.Add(this.numericUpDown_Codingtype);
         this.groupBox3.Controls.Add(this.label14);
         this.groupBox3.Controls.Add(this.textBox_CodingtypeAdr);
         this.groupBox3.Location = new System.Drawing.Point(467, 131);
         this.groupBox3.Name = "groupBox3";
         this.groupBox3.Size = new System.Drawing.Size(213, 46);
         this.groupBox3.TabIndex = 13;
         this.groupBox3.TabStop = false;
         this.groupBox3.Text = "Codingtype";
         // 
         // numericUpDown_Codingtype
         // 
         this.numericUpDown_Codingtype.Location = new System.Drawing.Point(10, 19);
         this.numericUpDown_Codingtype.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
         this.numericUpDown_Codingtype.Name = "numericUpDown_Codingtype";
         this.numericUpDown_Codingtype.Size = new System.Drawing.Size(36, 20);
         this.numericUpDown_Codingtype.TabIndex = 0;
         this.numericUpDown_Codingtype.ValueChanged += new System.EventHandler(this.numericUpDown_Codingtype_ValueChanged);
         // 
         // label14
         // 
         this.label14.AutoSize = true;
         this.label14.Location = new System.Drawing.Point(52, 21);
         this.label14.Name = "label14";
         this.label14.Size = new System.Drawing.Size(82, 13);
         this.label14.TabIndex = 1;
         this.label14.Text = "Patchadr. (hex):";
         // 
         // textBox_CodingtypeAdr
         // 
         this.textBox_CodingtypeAdr.Location = new System.Drawing.Point(140, 18);
         this.textBox_CodingtypeAdr.Name = "textBox_CodingtypeAdr";
         this.textBox_CodingtypeAdr.Size = new System.Drawing.Size(61, 20);
         this.textBox_CodingtypeAdr.TabIndex = 2;
         // 
         // numericUpDown_TilesizeVert
         // 
         this.numericUpDown_TilesizeVert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.numericUpDown_TilesizeVert.Location = new System.Drawing.Point(631, 104);
         this.numericUpDown_TilesizeVert.Maximum = new decimal(new int[] {
            512,
            0,
            0,
            0});
         this.numericUpDown_TilesizeVert.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
         this.numericUpDown_TilesizeVert.Name = "numericUpDown_TilesizeVert";
         this.numericUpDown_TilesizeVert.Size = new System.Drawing.Size(48, 20);
         this.numericUpDown_TilesizeVert.TabIndex = 10;
         this.numericUpDown_TilesizeVert.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
         this.numericUpDown_TilesizeVert.ValueChanged += new System.EventHandler(this.numericUpDown_TilesizeVert_ValueChanged);
         // 
         // label15
         // 
         this.label15.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.label15.AutoSize = true;
         this.label15.Location = new System.Drawing.Point(615, 109);
         this.label15.Name = "label15";
         this.label15.Size = new System.Drawing.Size(12, 13);
         this.label15.TabIndex = 9;
         this.label15.Text = "x";
         // 
         // richTextBox_Bin
         // 
         this.richTextBox_Bin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.richTextBox_Bin.BackColor = System.Drawing.Color.Yellow;
         this.richTextBox_Bin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.richTextBox_Bin.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.richTextBox_Bin.Location = new System.Drawing.Point(70, 44);
         this.richTextBox_Bin.Multiline = false;
         this.richTextBox_Bin.Name = "richTextBox_Bin";
         this.richTextBox_Bin.Size = new System.Drawing.Size(612, 23);
         this.richTextBox_Bin.TabIndex = 3;
         this.richTextBox_Bin.TabStop = false;
         this.richTextBox_Bin.Text = "";
         this.richTextBox_Bin.WordWrap = false;
         this.richTextBox_Bin.TextChanged += new System.EventHandler(this.textBox_Bin_TextChanged);
         // 
         // label_Bytes
         // 
         this.label_Bytes.AutoSize = true;
         this.label_Bytes.Location = new System.Drawing.Point(6, 48);
         this.label_Bytes.Name = "label_Bytes";
         this.label_Bytes.Size = new System.Drawing.Size(33, 13);
         this.label_Bytes.TabIndex = 1;
         this.label_Bytes.Text = "Bytes";
         // 
         // textBox_PostBits
         // 
         this.textBox_PostBits.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox_PostBits.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.textBox_PostBits.Location = new System.Drawing.Point(70, 102);
         this.textBox_PostBits.Name = "textBox_PostBits";
         this.textBox_PostBits.Size = new System.Drawing.Size(399, 23);
         this.textBox_PostBits.TabIndex = 6;
         this.textBox_PostBits.TextChanged += new System.EventHandler(this.textBox_Bin_TextChanged);
         // 
         // label10
         // 
         this.label10.AutoSize = true;
         this.label10.Location = new System.Drawing.Point(6, 109);
         this.label10.Name = "label10";
         this.label10.Size = new System.Drawing.Size(51, 13);
         this.label10.TabIndex = 5;
         this.label10.Text = "&Post-Bits:";
         // 
         // textBox_ExternArgs
         // 
         this.textBox_ExternArgs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox_ExternArgs.Location = new System.Drawing.Point(542, 107);
         this.textBox_ExternArgs.Name = "textBox_ExternArgs";
         this.textBox_ExternArgs.Size = new System.Drawing.Size(161, 20);
         this.textBox_ExternArgs.TabIndex = 12;
         // 
         // label12
         // 
         this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.label12.AutoSize = true;
         this.label12.Location = new System.Drawing.Point(475, 110);
         this.label12.Name = "label12";
         this.label12.Size = new System.Drawing.Size(61, 13);
         this.label12.TabIndex = 11;
         this.label12.Text = "&Argumente:";
         // 
         // Form1
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(719, 598);
         this.Controls.Add(this.label12);
         this.Controls.Add(this.textBox_ExternArgs);
         this.Controls.Add(this.groupBox2);
         this.Controls.Add(this.groupBox1);
         this.Controls.Add(this.label5);
         this.Controls.Add(this.numericUpDown_MinBytes);
         this.Controls.Add(this.checkBox_ProtBin);
         this.Controls.Add(this.checkBox_ProtHex);
         this.Controls.Add(this.textBox_Extern);
         this.Controls.Add(this.label4);
         this.Controls.Add(this.checkBox_ProtNewline);
         this.Controls.Add(this.textBox_Protfile);
         this.Controls.Add(this.label3);
         this.Controls.Add(this.textBox_Patchfile);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.checkBox_FillBits);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.textBox_PatchPos);
         this.MinimumSize = new System.Drawing.Size(735, 636);
         this.Name = "Form1";
         this.Text = "Input";
         this.Load += new System.EventHandler(this.Form1_Load);
         this.Shown += new System.EventHandler(this.Form1_Shown);
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_MinBytes)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_maxdiff)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_TilesizeHoriz)).EndInit();
         this.groupBox1.ResumeLayout(false);
         this.groupBox1.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_SingleTest)).EndInit();
         this.groupBox2.ResumeLayout(false);
         this.groupBox2.PerformLayout();
         this.groupBox5.ResumeLayout(false);
         this.groupBox5.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Min)).EndInit();
         this.groupBox4.ResumeLayout(false);
         this.groupBox4.PerformLayout();
         this.groupBox3.ResumeLayout(false);
         this.groupBox3.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Codingtype)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_TilesizeVert)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.TextBox textBox_PatchPos;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.CheckBox checkBox_FillBits;
      private System.Windows.Forms.TextBox textBox2;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.TextBox textBox_Patchfile;
      private System.Windows.Forms.TextBox textBox_Protfile;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.CheckBox checkBox_ProtNewline;
      private System.Windows.Forms.TextBox textBox_Extern;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.Button button_Start;
      private System.Windows.Forms.CheckBox checkBox_ProtHex;
      private System.Windows.Forms.CheckBox checkBox_ProtBin;
      private System.Windows.Forms.TextBox textBox1;
      private System.Windows.Forms.NumericUpDown numericUpDown_MinBytes;
      private System.Windows.Forms.Label label5;
      private System.Windows.Forms.TextBox textBox_int;
      private System.Windows.Forms.Button button_dec2bin;
      private System.Windows.Forms.NumericUpDown numericUpDown_maxdiff;
      private System.Windows.Forms.Label label6;
      private System.Windows.Forms.Label label7;
      private System.Windows.Forms.Button button_Detail;
      private System.Windows.Forms.Label label9;
      private System.Windows.Forms.NumericUpDown numericUpDown_TilesizeHoriz;
      private System.Windows.Forms.GroupBox groupBox1;
      private System.Windows.Forms.Button button_SingleTest;
      private System.Windows.Forms.TextBox textBox_SingleTestBin;
      private System.Windows.Forms.NumericUpDown numericUpDown_SingleTest;
      private System.Windows.Forms.ListBox listBox_SingleTest;
      private System.Windows.Forms.GroupBox groupBox2;
      private System.Windows.Forms.Label label10;
      private System.Windows.Forms.TextBox textBox_PostBits;
      private System.Windows.Forms.TextBox textBox_MaxHeightDiffAdr;
      private System.Windows.Forms.Label label_Bytes;
      private System.Windows.Forms.RichTextBox richTextBox_Bin;
      private System.Windows.Forms.TextBox textBox_ExternArgs;
      private System.Windows.Forms.Label label12;
      private System.Windows.Forms.TextBox textBox_CodingtypeAdr;
      private System.Windows.Forms.Label label14;
      private System.Windows.Forms.NumericUpDown numericUpDown_Codingtype;
      private System.Windows.Forms.Label label15;
      private System.Windows.Forms.NumericUpDown numericUpDown_TilesizeVert;
      private System.Windows.Forms.CheckBox checkBoxMinMaxAuto;
      private System.Windows.Forms.GroupBox groupBox4;
      private System.Windows.Forms.Label label13;
      private System.Windows.Forms.GroupBox groupBox3;
      private System.Windows.Forms.GroupBox groupBox5;
      private System.Windows.Forms.Label label8;
      private System.Windows.Forms.NumericUpDown numericUpDown_Min;
      private System.Windows.Forms.TextBox textBox_BaseHeightAdr;
      private System.Windows.Forms.CheckBox checkBox_Normalized;
   }
}

