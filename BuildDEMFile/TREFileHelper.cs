﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BuildDEMFile {

   class TREFileHelper {

      /// <summary>
      /// MapUnits für 360°
      /// </summary>
      const int MAPUNITS360DEGREE = 0x1000000; // 1 << 24

      /// <summary>
      /// MapUnits je Grad
      /// </summary>
      const double DEGREE_FACTOR = 360.0 / MAPUNITS360DEGREE;


      /// <summary>
      /// liefert die Boundingbox der TRE-Datei
      /// </summary>
      /// <param name="filename"></param>
      /// <param name="west"></param>
      /// <param name="north"></param>
      /// <param name="east"></param>
      /// <param name="south"></param>
      /// <param name="iwest"></param>
      /// <param name="inorth"></param>
      /// <param name="ieast"></param>
      /// <param name="isouth"></param>
      /// <returns></returns>
      public static bool ReadEdges(string filename, 
                                   out double west, out double north, out double east, out double south,
                                   out int iwest, out int inorth, out int ieast, out int isouth) {
         bool ret = false;
         west = north = east = south = 0;
         using (BinaryReader br = new BinaryReader(File.OpenRead(filename))) {
            br.ReadUInt16(); // Headerlänge

            string GarminTyp = ReadString(br, 10);       // z.B. "GARMIN RGN"
            if (GarminTyp != "GARMIN TRE")
               throw new Exception("It's not a valid Garmin-TRE-File.");

            br.ReadByte(); // Unknown_0x0C

            br.ReadByte(); // Locked 

            // Datum/Zeit
            br.ReadInt32();
            br.ReadInt16();
            br.ReadByte();

            inorth = Read3Byte(br);
            ieast = Read3Byte(br);
            isouth = Read3Byte(br);
            iwest = Read3Byte(br);

            north = MapUnits2Degree(inorth); // 0x15
            east = MapUnits2Degree(ieast);
            south = MapUnits2Degree(isouth);
            west = MapUnits2Degree(iwest);

            ret = true;
         }
         return ret;
      }

      /// <summary>
      /// Übernahme aus MKGMAP: Convert an angle in map units to degrees.
      /// </summary>
      /// <param name="mu"></param>
      /// <returns></returns>
      static double MapUnits2Degree(int mu) {
         return mu * DEGREE_FACTOR;
      }

      static int Read3Byte(BinaryReader br) {
         byte[] b = br.ReadBytes(3);
         int number = (b[2] << 16) + (b[1] << 8) + b[0]; // "uint"
         return (number & 0x800000) > 0 ?
                     (number - 0x1000000) :
                     number;
      }

      /// <summary>
      /// Standard-Codierung für Zeichenketten
      /// </summary>
      static Encoding stdencoding = Encoding.GetEncoding(1252); //new ASCIIEncoding();

      /// <summary>
      /// liest eine Zeichenkette bis zum 0-Byte oder bis die max. Länge erreicht ist
      /// </summary>
      /// <param name="br"></param>
      /// <param name="maxlen"></param>
      /// <param name="encoder"></param>
      /// <returns></returns>
      static string ReadString(BinaryReader br, int maxlen = 0, Encoding encoder = null) {
         List<byte> dat = new List<byte>();
         byte b;
         int len = maxlen > 0 ? maxlen : int.MaxValue;
         do {
            b = br.ReadByte();
            if (b != 0)
               dat.Add(b);
            len--;
         } while (b != 0 && len > 0);
         return encoder == null ? stdencoding.GetString(dat.ToArray()) : encoder.GetString(dat.ToArray());
      }

   }
}
