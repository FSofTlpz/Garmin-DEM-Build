namespace Input1 {
   partial class FormDetail {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing) {
         if (disposing && (components != null)) {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent() {
         this.textBox_Encoder = new System.Windows.Forms.TextBox();
         this.tabControl1 = new System.Windows.Forms.TabControl();
         this.tabPage1 = new System.Windows.Forms.TabPage();
         this.tabPage2 = new System.Windows.Forms.TabPage();
         this.textBox_Data = new System.Windows.Forms.TextBox();
         this.tabControl1.SuspendLayout();
         this.tabPage1.SuspendLayout();
         this.tabPage2.SuspendLayout();
         this.SuspendLayout();
         // 
         // textBox_Encoder
         // 
         this.textBox_Encoder.Dock = System.Windows.Forms.DockStyle.Fill;
         this.textBox_Encoder.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.textBox_Encoder.Location = new System.Drawing.Point(3, 3);
         this.textBox_Encoder.Multiline = true;
         this.textBox_Encoder.Name = "textBox_Encoder";
         this.textBox_Encoder.ReadOnly = true;
         this.textBox_Encoder.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.textBox_Encoder.Size = new System.Drawing.Size(1017, 503);
         this.textBox_Encoder.TabIndex = 0;
         this.textBox_Encoder.WordWrap = false;
         // 
         // tabControl1
         // 
         this.tabControl1.Controls.Add(this.tabPage1);
         this.tabControl1.Controls.Add(this.tabPage2);
         this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.tabControl1.Location = new System.Drawing.Point(0, 0);
         this.tabControl1.Name = "tabControl1";
         this.tabControl1.SelectedIndex = 0;
         this.tabControl1.Size = new System.Drawing.Size(1031, 535);
         this.tabControl1.TabIndex = 1;
         // 
         // tabPage1
         // 
         this.tabPage1.Controls.Add(this.textBox_Encoder);
         this.tabPage1.Location = new System.Drawing.Point(4, 22);
         this.tabPage1.Name = "tabPage1";
         this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
         this.tabPage1.Size = new System.Drawing.Size(1023, 509);
         this.tabPage1.TabIndex = 0;
         this.tabPage1.Text = "Encoder";
         this.tabPage1.UseVisualStyleBackColor = true;
         // 
         // tabPage2
         // 
         this.tabPage2.Controls.Add(this.textBox_Data);
         this.tabPage2.Location = new System.Drawing.Point(4, 22);
         this.tabPage2.Name = "tabPage2";
         this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
         this.tabPage2.Size = new System.Drawing.Size(1023, 509);
         this.tabPage2.TabIndex = 1;
         this.tabPage2.Text = "Data";
         this.tabPage2.UseVisualStyleBackColor = true;
         // 
         // textBox_Data
         // 
         this.textBox_Data.Dock = System.Windows.Forms.DockStyle.Fill;
         this.textBox_Data.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.textBox_Data.Location = new System.Drawing.Point(3, 3);
         this.textBox_Data.Multiline = true;
         this.textBox_Data.Name = "textBox_Data";
         this.textBox_Data.ReadOnly = true;
         this.textBox_Data.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.textBox_Data.Size = new System.Drawing.Size(1017, 503);
         this.textBox_Data.TabIndex = 1;
         this.textBox_Data.WordWrap = false;
         // 
         // FormDetail
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(1031, 535);
         this.Controls.Add(this.tabControl1);
         this.MinimizeBox = false;
         this.Name = "FormDetail";
         this.ShowInTaskbar = false;
         this.Text = "Encoder-Details";
         this.Shown += new System.EventHandler(this.FormDetail_Shown);
         this.tabControl1.ResumeLayout(false);
         this.tabPage1.ResumeLayout(false);
         this.tabPage1.PerformLayout();
         this.tabPage2.ResumeLayout(false);
         this.tabPage2.PerformLayout();
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.TextBox textBox_Encoder;
      private System.Windows.Forms.TabControl tabControl1;
      private System.Windows.Forms.TabPage tabPage1;
      private System.Windows.Forms.TabPage tabPage2;
      private System.Windows.Forms.TextBox textBox_Data;
   }
}