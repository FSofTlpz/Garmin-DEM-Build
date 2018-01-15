using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

/*
    <Reference Include="PresentationCore" />
    <Reference Include="WindowsBase" />
 */

namespace BuildDEMFile {
   class DEMTiffReader : DEM1x1 {

      string tiffile;


      public DEMTiffReader(int left, int bottom, string tiffile) :
         base(left, bottom) {
         this.tiffile = tiffile;
      }

      public override void SetDataArray() {
         Stream stream = File.Open(tiffile, FileMode.Open, FileAccess.Read, FileShare.Read);
         TiffBitmapDecoder tiffDecoder = new TiffBitmapDecoder(
                                                 stream,
                                                 BitmapCreateOptions.PreservePixelFormat,
                                                 BitmapCacheOption.Default); //.None);
         BitmapFrame firstFrame = tiffDecoder.Frames[0];
         FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap(firstFrame, PixelFormats.Gray16, null, 0);
         //FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap(firstFrame, firstFrame.Format, null, 0);

         Columns = convertedBitmap.PixelWidth;
         Rows = convertedBitmap.PixelHeight;

         data = new short[Columns * Rows];
         convertedBitmap.CopyPixels(data, 
                                    Columns * 2,   // Zeilenlänge in Byte
                                    0);
         stream.Dispose();

         Maximum = short.MinValue;
         Minimum = short.MaxValue;
         NotValid = 0;
         for (int i = 0; i < data.Length; i++) {
            if (Maximum < data[i])
               Maximum = data[i];
            if (data[i] != NOVALUE) {
               if (Minimum > data[i])
                  Minimum = data[i];
            } else {
               NotValid++;
            }
         }

      }

   }
}
