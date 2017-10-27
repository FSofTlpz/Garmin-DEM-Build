using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BuildDEMFile {

   /// <summary>
   /// erzeugt eine einzelne Garmin-DEM-Datei
   /// </summary>
   class Program {
      static void Main(string[] args) {

         Assembly a = Assembly.GetExecutingAssembly();
         string progname = ((AssemblyProductAttribute)(Attribute.GetCustomAttribute(a, typeof(AssemblyProductAttribute)))).Product + ", Version vom " +
                           ((AssemblyInformationalVersionAttribute)(Attribute.GetCustomAttribute(a, typeof(AssemblyInformationalVersionAttribute)))).InformationalVersion + ", " +
                           ((AssemblyCopyrightAttribute)(Attribute.GetCustomAttribute(a, typeof(AssemblyCopyrightAttribute)))).Copyright;
         Console.Error.WriteLine(progname);

         try {
            List<string> datafilenames;
            List<double> left;
            List<double> top;
            List<double> width;
            List<double> height;
            List<double> latdist;
            List<double> londist;
            string hgtpath;
            string hgtout;
            Options opt = CheckOptions(args,
                                        out datafilenames,
                                        out left,
                                        out top,
                                        out width,
                                        out height,
                                        out latdist,
                                        out londist,
                                        out hgtpath,
                                        out hgtout);
            FileBuilder fb = new FileBuilder(datafilenames, hgtpath, left, top, width, height, latdist, londist, hgtout);

            fb.Create(opt.DEMFilename, opt.DataInFoot, opt.LastColStd, opt.OutputOverwrite, opt.UseDummyData);

         } catch (Exception ex) {
            Console.Error.WriteLine(ex.Message);
         }
      }

      static Options CheckOptions(string[] args,
                                  out List<string> datafilenames,
                                  out List<double> left,
                                  out List<double> top,
                                  out List<double> width,
                                  out List<double> height,
                                  out List<double> latdist,
                                  out List<double> londist,
                                  out string hgtpath,
                                  out string hgtout
         ) {
         Options opt = new Options();
         opt.Evaluate(args);

         datafilenames = new List<string>(opt.DataFilename);
         left = new List<double>();
         top = new List<double>();
         width = new List<double>();
         height = new List<double>();
         latdist = new List<double>();
         londist = new List<double>();

         if (string.IsNullOrEmpty(opt.DEMFilename))
            throw new Exception("Es ist keine Zieldatei angegeben.");
         else if (File.Exists(opt.DEMFilename) && !opt.OutputOverwrite)
            throw new Exception("Die DEM-Datei ex. schon, darf aber nicht überschrieben werden.");

         if (!string.IsNullOrEmpty(opt.HGTPath) &&
             !string.IsNullOrEmpty(opt.HGTDataOutput) &&
             !opt.OutputOverwrite)
            throw new Exception("Die Ausgabedatei für die HGT-Daten ex. schon, darf aber nicht überschrieben werden.");

         if (opt.DataFilename.Count == 0 &&
             string.IsNullOrEmpty(opt.HGTPath))
            throw new Exception("Es ist weder eine Höhendatendatei noch ein HGT-Pfad angegeben.");


         if (!double.IsNaN(opt.TRELeft))
            left.Add(opt.TRELeft);
         if (!double.IsNaN(opt.TRETop))
            top.Add(opt.TRETop);
         if (!double.IsNaN(opt.DEMWidth))
            width.Add(opt.DEMWidth);
         if (!double.IsNaN(opt.DEMHeight))
            height.Add(opt.DEMHeight);

         if (!string.IsNullOrEmpty(opt.TREFilename)) {
            double west, north, east, south;
            if (!TREFileHelper.ReadEdges(opt.TREFilename, out west, out north, out east, out south))
               throw new Exception("Es konnten keine Daten aus der TRE-Datei ermittelt werden.");

            Console.WriteLine("Daten aus der TRE-Datei '" + opt.TREFilename + "' gelesen");
            if (double.IsNaN(opt.TRELeft))
               if (left.Count > 0) left[0] = west;
               else left.Add(west);
            if (double.IsNaN(opt.TRETop))
               if (top.Count > 0) top[0] = north;
               else top.Add(north);
            if (double.IsNaN(opt.DEMWidth))
               if (width.Count > 0) width[0] = east - west;
               else width.Add(east - west);
            if (double.IsNaN(opt.DEMHeight))
               if (height.Count > 0) height[0] = north - south;
               else height.Add(north - south);
         }

         if (left.Count == 0)
            throw new Exception("Es konnte kein westlicher Rand ermittelt werden.");

         if (top.Count == 0)
            throw new Exception("Es konnte kein nördlicher Rand ermittelt werden.");

         if (width.Count == 0)
            width.Add(double.NaN);
         if (height.Count == 0)
            height.Add(double.NaN);

         int count = 0; // Anzahl der Zoomlevel
         if (opt.DataFilename.Count > 0) {
            count = opt.DataFilename.Count;
            hgtpath = null;
            hgtout = null;
         } else {
            count = Math.Max(opt.PixelWidth.Count, opt.PixelHeight.Count);
            hgtpath = opt.HGTPath;
            hgtout = opt.HGTDataOutput;
            datafilenames.Add(null);
         }
         if (count == 0)
            throw new Exception("Zu wenig Optionen angegeben.");

         if (opt.PixelWidth.Count < count &&
             opt.PixelHeight.Count < count)
            throw new Exception("Zu wenig Punktabstände angegeben.");

         for (int i = 0; i < opt.PixelWidth.Count && i < count; i++)
            latdist.Add(opt.PixelWidth[i]);

         for (int i = 0; i < opt.PixelHeight.Count && i < count; i++)
            londist.Add(opt.PixelHeight[i]);

         for (int i = latdist.Count; i < count; i++)
            latdist.Add(londist[i]);

         for (int i = londist.Count; i < count; i++)
            londist.Add(latdist[i]);

         while (datafilenames.Count < count)
            datafilenames.Add(datafilenames[0]);

         while (left.Count < count)
            left.Add(left[0]);
         while (top.Count < count)
            top.Add(top[0]);
         while (width.Count < count)
            width.Add(width[0]);
         while (height.Count < count)
            height.Add(height[0]);

         return opt;
      }


   }
}
