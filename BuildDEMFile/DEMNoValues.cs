namespace BuildDEMFile {

   public class DEMNoValues : DEM1x1 {

      public DEMNoValues(int left, int bottom) :
         base(left, bottom) {
      }

      public override void SetDataArray() {
         Minimum = Maximum = NOVALUE;
         Rows = Columns = 2;   // 2, damit Delta noch einen Wert erhält
         data = new short[Rows * Columns];
         for (int i = 0; i < data.Length; i++)
            data[i] = NOVALUE;
         NotValid = data.Length;
      }

   }
}
