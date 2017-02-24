namespace Input1 {
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
         this.numericUpDown_maxheight = new System.Windows.Forms.NumericUpDown();
         this.label6 = new System.Windows.Forms.Label();
         this.label7 = new System.Windows.Forms.Label();
         this.button_Detail = new System.Windows.Forms.Button();
         this.label8 = new System.Windows.Forms.Label();
         this.label9 = new System.Windows.Forms.Label();
         this.numericUpDown_Tilesize = new System.Windows.Forms.NumericUpDown();
         this.groupBox1 = new System.Windows.Forms.GroupBox();
         this.button_SingleTest = new System.Windows.Forms.Button();
         this.textBox_SingleTestBin = new System.Windows.Forms.TextBox();
         this.numericUpDown_SingleTest = new System.Windows.Forms.NumericUpDown();
         this.listBox_SingleTest = new System.Windows.Forms.ListBox();
         this.groupBox2 = new System.Windows.Forms.GroupBox();
         this.textBox_PatchAddrRangevalue = new System.Windows.Forms.TextBox();
         this.label14 = new System.Windows.Forms.Label();
         this.label13 = new System.Windows.Forms.Label();
         this.numericUpDown_Rangevalue = new System.Windows.Forms.NumericUpDown();
         this.richTextBox_Bin = new System.Windows.Forms.RichTextBox();
         this.label_Bytes = new System.Windows.Forms.Label();
         this.textBox_PatchAddrHeight = new System.Windows.Forms.TextBox();
         this.label11 = new System.Windows.Forms.Label();
         this.textBox_PostBits = new System.Windows.Forms.TextBox();
         this.label10 = new System.Windows.Forms.Label();
         this.textBox_ExternArgs = new System.Windows.Forms.TextBox();
         this.label12 = new System.Windows.Forms.Label();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_MinBytes)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_maxheight)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Tilesize)).BeginInit();
         this.groupBox1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_SingleTest)).BeginInit();
         this.groupBox2.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Rangevalue)).BeginInit();
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
         this.label1.Size = new System.Drawing.Size(120, 13);
         this.label1.TabIndex = 2;
         this.label1.Text = "Patch ab Adresse (hex):";
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
         this.textBox2.Size = new System.Drawing.Size(588, 23);
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
         this.textBox_Patchfile.Size = new System.Drawing.Size(538, 20);
         this.textBox_Patchfile.TabIndex = 1;
         // 
         // textBox_Protfile
         // 
         this.textBox_Protfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox_Protfile.Location = new System.Drawing.Point(141, 64);
         this.textBox_Protfile.Name = "textBox_Protfile";
         this.textBox_Protfile.Size = new System.Drawing.Size(538, 20);
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
         this.textBox_Extern.Size = new System.Drawing.Size(297, 20);
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
         this.button_Start.Location = new System.Drawing.Point(6, 273);
         this.button_Start.Name = "button_Start";
         this.button_Start.Size = new System.Drawing.Size(652, 27);
         this.button_Start.TabIndex = 21;
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
         this.textBox1.Size = new System.Drawing.Size(588, 23);
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
         this.textBox_int.Location = new System.Drawing.Point(84, 162);
         this.textBox_int.Multiline = true;
         this.textBox_int.Name = "textBox_int";
         this.textBox_int.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.textBox_int.Size = new System.Drawing.Size(574, 105);
         this.textBox_int.TabIndex = 18;
         this.textBox_int.WordWrap = false;
         this.textBox_int.TextChanged += new System.EventHandler(this.textBox_int_TextChanged);
         // 
         // button_dec2bin
         // 
         this.button_dec2bin.Location = new System.Drawing.Point(9, 188);
         this.button_dec2bin.Name = "button_dec2bin";
         this.button_dec2bin.Size = new System.Drawing.Size(72, 23);
         this.button_dec2bin.TabIndex = 19;
         this.button_dec2bin.Text = "&Dez -> Bin";
         this.button_dec2bin.UseVisualStyleBackColor = true;
         this.button_dec2bin.Click += new System.EventHandler(this.button_dec2bin_Click);
         // 
         // numericUpDown_maxheight
         // 
         this.numericUpDown_maxheight.Location = new System.Drawing.Point(73, 136);
         this.numericUpDown_maxheight.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
         this.numericUpDown_maxheight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
         this.numericUpDown_maxheight.Name = "numericUpDown_maxheight";
         this.numericUpDown_maxheight.Size = new System.Drawing.Size(66, 20);
         this.numericUpDown_maxheight.TabIndex = 10;
         this.numericUpDown_maxheight.Value = new decimal(new int[] {
            158,
            0,
            0,
            0});
         this.numericUpDown_maxheight.ValueChanged += new System.EventHandler(this.numericUpDown_maxheight_ValueChanged);
         // 
         // label6
         // 
         this.label6.AutoSize = true;
         this.label6.Location = new System.Drawing.Point(6, 162);
         this.label6.Name = "label6";
         this.label6.Size = new System.Drawing.Size(80, 13);
         this.label6.TabIndex = 17;
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
         this.button_Detail.Location = new System.Drawing.Point(9, 217);
         this.button_Detail.Name = "button_Detail";
         this.button_Detail.Size = new System.Drawing.Size(72, 23);
         this.button_Detail.TabIndex = 20;
         this.button_Detail.Text = "De&tails";
         this.button_Detail.UseVisualStyleBackColor = true;
         this.button_Detail.Click += new System.EventHandler(this.button_Detail_Click);
         // 
         // label8
         // 
         this.label8.AutoSize = true;
         this.label8.Location = new System.Drawing.Point(6, 138);
         this.label8.Name = "label8";
         this.label8.Size = new System.Drawing.Size(61, 13);
         this.label8.TabIndex = 9;
         this.label8.Text = "max. Höhe:";
         // 
         // label9
         // 
         this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.label9.AutoSize = true;
         this.label9.Location = new System.Drawing.Point(544, 109);
         this.label9.Name = "label9";
         this.label9.Size = new System.Drawing.Size(59, 13);
         this.label9.TabIndex = 7;
         this.label9.Text = "Tile-Größe:";
         // 
         // numericUpDown_Tilesize
         // 
         this.numericUpDown_Tilesize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.numericUpDown_Tilesize.Location = new System.Drawing.Point(609, 107);
         this.numericUpDown_Tilesize.Maximum = new decimal(new int[] {
            512,
            0,
            0,
            0});
         this.numericUpDown_Tilesize.Minimum = new decimal(new int[] {
            16,
            0,
            0,
            0});
         this.numericUpDown_Tilesize.Name = "numericUpDown_Tilesize";
         this.numericUpDown_Tilesize.Size = new System.Drawing.Size(48, 20);
         this.numericUpDown_Tilesize.TabIndex = 8;
         this.numericUpDown_Tilesize.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
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
         this.groupBox1.Size = new System.Drawing.Size(664, 89);
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
         this.textBox_SingleTestBin.Size = new System.Drawing.Size(489, 23);
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
            "Hybrid mit HUnit 1",
            "Hybrid mit HUnit 2",
            "Hybrid mit HUnit 4",
            "Hybrid mit HUnit 8",
            "Hybrid mit HUnit 16",
            "Hybrid mit HUnit 32",
            "Hybrid mit HUnit 64",
            "Hybrid mit HUnit 128",
            "Hybrid mit HUnit 256",
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
         this.groupBox2.Controls.Add(this.textBox_PatchAddrRangevalue);
         this.groupBox2.Controls.Add(this.label14);
         this.groupBox2.Controls.Add(this.label13);
         this.groupBox2.Controls.Add(this.numericUpDown_Rangevalue);
         this.groupBox2.Controls.Add(this.richTextBox_Bin);
         this.groupBox2.Controls.Add(this.label_Bytes);
         this.groupBox2.Controls.Add(this.textBox_PatchAddrHeight);
         this.groupBox2.Controls.Add(this.label11);
         this.groupBox2.Controls.Add(this.textBox_PostBits);
         this.groupBox2.Controls.Add(this.label10);
         this.groupBox2.Controls.Add(this.textBox2);
         this.groupBox2.Controls.Add(this.button_Start);
         this.groupBox2.Controls.Add(this.numericUpDown_Tilesize);
         this.groupBox2.Controls.Add(this.textBox1);
         this.groupBox2.Controls.Add(this.label9);
         this.groupBox2.Controls.Add(this.textBox_int);
         this.groupBox2.Controls.Add(this.label8);
         this.groupBox2.Controls.Add(this.button_dec2bin);
         this.groupBox2.Controls.Add(this.button_Detail);
         this.groupBox2.Controls.Add(this.numericUpDown_maxheight);
         this.groupBox2.Controls.Add(this.label6);
         this.groupBox2.Controls.Add(this.label7);
         this.groupBox2.ForeColor = System.Drawing.SystemColors.ControlText;
         this.groupBox2.Location = new System.Drawing.Point(15, 251);
         this.groupBox2.Name = "groupBox2";
         this.groupBox2.Size = new System.Drawing.Size(664, 306);
         this.groupBox2.TabIndex = 17;
         this.groupBox2.TabStop = false;
         this.groupBox2.Text = "Test";
         // 
         // textBox_PatchAddrRangevalue
         // 
         this.textBox_PatchAddrRangevalue.Location = new System.Drawing.Point(594, 135);
         this.textBox_PatchAddrRangevalue.Name = "textBox_PatchAddrRangevalue";
         this.textBox_PatchAddrRangevalue.Size = new System.Drawing.Size(61, 20);
         this.textBox_PatchAddrRangevalue.TabIndex = 16;
         // 
         // label14
         // 
         this.label14.AutoSize = true;
         this.label14.Location = new System.Drawing.Point(493, 138);
         this.label14.Name = "label14";
         this.label14.Size = new System.Drawing.Size(101, 13);
         this.label14.TabIndex = 15;
         this.label14.Text = "Patchadresse (hex):";
         // 
         // label13
         // 
         this.label13.AutoSize = true;
         this.label13.Location = new System.Drawing.Point(388, 138);
         this.label13.Name = "label13";
         this.label13.Size = new System.Drawing.Size(57, 13);
         this.label13.TabIndex = 13;
         this.label13.Text = "Codingtyp:";
         // 
         // numericUpDown_Rangevalue
         // 
         this.numericUpDown_Rangevalue.Location = new System.Drawing.Point(451, 136);
         this.numericUpDown_Rangevalue.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
         this.numericUpDown_Rangevalue.Name = "numericUpDown_Rangevalue";
         this.numericUpDown_Rangevalue.Size = new System.Drawing.Size(36, 20);
         this.numericUpDown_Rangevalue.TabIndex = 14;
         this.numericUpDown_Rangevalue.ValueChanged += new System.EventHandler(this.numericUpDown_Rangevalue_ValueChanged);
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
         this.richTextBox_Bin.Size = new System.Drawing.Size(588, 23);
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
         // textBox_PatchAddrHeight
         // 
         this.textBox_PatchAddrHeight.Location = new System.Drawing.Point(246, 135);
         this.textBox_PatchAddrHeight.Name = "textBox_PatchAddrHeight";
         this.textBox_PatchAddrHeight.Size = new System.Drawing.Size(61, 20);
         this.textBox_PatchAddrHeight.TabIndex = 12;
         // 
         // label11
         // 
         this.label11.AutoSize = true;
         this.label11.Location = new System.Drawing.Point(145, 138);
         this.label11.Name = "label11";
         this.label11.Size = new System.Drawing.Size(101, 13);
         this.label11.TabIndex = 11;
         this.label11.Text = "Patchadresse (hex):";
         // 
         // textBox_PostBits
         // 
         this.textBox_PostBits.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox_PostBits.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.textBox_PostBits.Location = new System.Drawing.Point(70, 102);
         this.textBox_PostBits.Name = "textBox_PostBits";
         this.textBox_PostBits.Size = new System.Drawing.Size(427, 23);
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
         this.label10.Text = "Post-Bits:";
         // 
         // textBox_ExternArgs
         // 
         this.textBox_ExternArgs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox_ExternArgs.Location = new System.Drawing.Point(518, 107);
         this.textBox_ExternArgs.Name = "textBox_ExternArgs";
         this.textBox_ExternArgs.Size = new System.Drawing.Size(161, 20);
         this.textBox_ExternArgs.TabIndex = 12;
         // 
         // label12
         // 
         this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.label12.AutoSize = true;
         this.label12.Location = new System.Drawing.Point(451, 110);
         this.label12.Name = "label12";
         this.label12.Size = new System.Drawing.Size(61, 13);
         this.label12.TabIndex = 11;
         this.label12.Text = "Argumente:";
         // 
         // Form1
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(695, 569);
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
         this.MinimumSize = new System.Drawing.Size(500, 510);
         this.Name = "Form1";
         this.Text = "Input";
         this.Load += new System.EventHandler(this.Form1_Load);
         this.Shown += new System.EventHandler(this.Form1_Shown);
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_MinBytes)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_maxheight)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Tilesize)).EndInit();
         this.groupBox1.ResumeLayout(false);
         this.groupBox1.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_SingleTest)).EndInit();
         this.groupBox2.ResumeLayout(false);
         this.groupBox2.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Rangevalue)).EndInit();
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
      private System.Windows.Forms.NumericUpDown numericUpDown_maxheight;
      private System.Windows.Forms.Label label6;
      private System.Windows.Forms.Label label7;
      private System.Windows.Forms.Button button_Detail;
      private System.Windows.Forms.Label label8;
      private System.Windows.Forms.Label label9;
      private System.Windows.Forms.NumericUpDown numericUpDown_Tilesize;
      private System.Windows.Forms.GroupBox groupBox1;
      private System.Windows.Forms.Button button_SingleTest;
      private System.Windows.Forms.TextBox textBox_SingleTestBin;
      private System.Windows.Forms.NumericUpDown numericUpDown_SingleTest;
      private System.Windows.Forms.ListBox listBox_SingleTest;
      private System.Windows.Forms.GroupBox groupBox2;
      private System.Windows.Forms.Label label10;
      private System.Windows.Forms.TextBox textBox_PostBits;
      private System.Windows.Forms.TextBox textBox_PatchAddrHeight;
      private System.Windows.Forms.Label label11;
      private System.Windows.Forms.Label label_Bytes;
      private System.Windows.Forms.RichTextBox richTextBox_Bin;
      private System.Windows.Forms.TextBox textBox_ExternArgs;
      private System.Windows.Forms.Label label12;
      private System.Windows.Forms.TextBox textBox_PatchAddrRangevalue;
      private System.Windows.Forms.Label label14;
      private System.Windows.Forms.Label label13;
      private System.Windows.Forms.NumericUpDown numericUpDown_Rangevalue;
   }
}

