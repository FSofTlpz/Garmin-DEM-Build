using System;
using System.Windows.Forms;

namespace ShowMousePos {
   public partial class Form1 : Form {


      public Form1() {
         InitializeComponent();
      }

      private void Form1_Shown(object sender, EventArgs e) {
         timer1.Enabled = true;
      }

      private void timer1_Tick(object sender, EventArgs e) {
         textBox1.Text = string.Format("{0}, {1}", Cursor.Position.X, Cursor.Position.Y);
      }

   }

}
