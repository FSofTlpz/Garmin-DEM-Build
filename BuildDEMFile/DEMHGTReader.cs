/*
Copyright (C) 2011 Frank Stinner

This program is free software; you can redistribute it and/or modify it 
under the terms of the GNU General Public License as published by the 
Free Software Foundation; either version 3 of the License, or (at your 
option) any later version. 

This program is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of 
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General 
Public License for more details. 

You should have received a copy of the GNU General Public License along 
with this program; if not, see <http://www.gnu.org/licenses/>. 


Dieses Programm ist freie Software. Sie können es unter den Bedingungen 
der GNU General Public License, wie von der Free Software Foundation 
veröffentlicht, weitergeben und/oder modifizieren, entweder gemäß 
Version 3 der Lizenz oder (nach Ihrer Option) jeder späteren Version. 

Die Veröffentlichung dieses Programms erfolgt in der Hoffnung, daß es 
Ihnen von Nutzen sein wird, aber OHNE IRGENDEINE GARANTIE, sogar ohne 
die implizite Garantie der MARKTREIFE oder der VERWENDBARKEIT FÜR EINEN 
BESTIMMTEN ZWECK. Details finden Sie in der GNU General Public License. 

Sie sollten ein Exemplar der GNU General Public License zusammen mit 
diesem Programm erhalten haben. Falls nicht, siehe 
<http://www.gnu.org/licenses/>. 
*/

using System;
using System.IO;
using System.IO.Compression;

namespace BuildDEMFile {

   /// <summary>
   /// zum Lesen der HGT-Daten:
   /// 
   /// Eine HGT-Datei enthält die Höhendaten für ein "Quadratgrad", also ein Gebiet über 1 Längen- und 1
   /// Breitengrad. Die Ausdehnung in N-S-Richtung ist also etwa 111km in O-W-Richtung je nach Breitengrad
   /// weniger.
   /// Die ursprünglichen SRTM-Daten (Shuttle Radar Topography Mission (SRTM) im Februar 2000) liegen im
   /// Bereich zwischen dem 60. nördlichen und 58. südlichen Breitengrad vor. Für die USA liegen diese Daten
   /// mit einer Auflösung von 1 Bogensekunde vor (--> 3601x3601 Datenpunkte, SRTM-1, etwa 30m), für den Rest 
   /// der Erde in 3 Bogensekunden (1201x1201, SRTM-3, etwa 92m). Die Randpunkte eines Gebietes sind also 
   /// identisch mit den Randpunkten des jeweils benachbarten Gebietes. (Der 1. Punkt einer Zeile oder Spalte
   /// liegt auf dem einen Rand des Gebietes, der letzte Punkt auf dem gegenüberliegenden Rand.)
   /// Der Dateiname leitet sich immer aus der S-W-Ecke (links-unten) des Gebietes ab, z.B. 
   ///      n51e002.hgt --> Gebiet zwischen N 51° E 2° und N 52° E 3°
   ///      s14w077.hgt --> Gebiet zwischen S 14° W 77° und S 13° W 76°
   /// Die Speicherung der Höhe erfolgt jeweils mit 2 Byte in Big-Endian-Bytefolge mit Vorzeichen.
   /// Die Werte sind in Metern angeben.
   /// Punkte ohne gültigen Wert haben den Wert 0x8000 (-32768).
   /// Die Reihenfolge der Daten ist zeilenweise von N nach S, innerhalb der Zeilen von W nach O.
   /// 
   /// Die "äußeren" Punkte haben jeweils volle Grad als Koordinaten.
   /// 
   /// z.B.
   /// http://dds.cr.usgs.gov/srtm/version2_1
   /// http://www.viewfinderpanoramas.org/dem3.html
   /// http://srtm.csi.cgiar.org/
   /// </summary>
   public class DEMHGTReader : DEM1x1 {

      /// <summary>
      /// "nodata" value in hgt-files
      /// </summary>
      const short HGT_NOVALUE = -32768;

      string filename;

      /// <summary>
      /// liest die Daten aus der entsprechenden HGT-Datei ein
      /// </summary>
      /// <param name="left">positiv für östliche Länge, sonst negativ</param>
      /// <param name="bottom">positiv für nördliche Breite, sonst negativ</param>
      /// <param name="directory">Verzeichnis der Datendatei</param>
      public DEMHGTReader(int left, int bottom, string directory) :
         base(left, bottom) {
         filename = Path.Combine(directory, GetStandardBasefilename(left, bottom) + ".hgt");
      }

      /// <summary>
      /// nur zur Erzeugung von Testdaten
      /// </summary>
      /// <param name="left"></param>
      /// <param name="bottom"></param>
      /// <param name="dat"></param>
      public DEMHGTReader(int left, int bottom, short[] dat) :
         base(left, bottom) {
         Maximum = Int16.MinValue;
         Minimum = Int16.MaxValue;
         data = new Int16[dat.Length];
         NotValid = 0;
         for (int i = 0; i < data.Length; i++) {
            data[i] = dat[i];
            if (Maximum < data[i]) Maximum = data[i];
            if (data[i] != DEMNOVALUE) {
               if (Minimum > data[i]) Minimum = data[i];
            } else
               NotValid++;
         }
         Rows = Columns = (int)Math.Sqrt(data.Length);      // sollte im Normalfall immer quadratisch sein
      }

      /// <summary>
      /// read data from file
      /// </summary>
      public override void SetDataArray() {
         Maximum = short.MinValue;
         Minimum = short.MaxValue;

         if (File.Exists(filename)) {

            Stream dat = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            ReadFromStream(dat, dat.Length);
            dat.Close();

         } else {

            if (!File.Exists(filename + ".zip")) {

               throw new Exception(string.Format("file '{0}' nor file '{0}.zip' exist", filename));

            } else {

               using (FileStream zipstream = new FileStream(filename + ".zip", FileMode.Open, FileAccess.Read, FileShare.Read)) {
                  using (ZipArchive zip = new ZipArchive(zipstream, ZipArchiveMode.Read)) {
                     filename = Path.GetFileName(filename).ToUpper();
                     ZipArchiveEntry entry = null;
                     foreach (var item in zip.Entries) {
                        if (filename == item.Name.ToUpper()) {
                           entry = item;
                           break;
                        }
                     }
                     if (entry == null)
                        throw new Exception(string.Format("file '{0}.zip' not include file '{0}'.", filename));
                     Stream dat = entry.Open();
                     ReadFromStream(dat, entry.Length);
                     dat.Close();
                  }
               }
            }

         }
      }

      /// <summary>
      /// read data; set <see cref="Rows"/> and <see cref="Columns"/>
      /// </summary>
      /// <param name="str"></param>
      void ReadFromStream(Stream str, long streamlen) {
         Maximum = short.MinValue;
         Minimum = short.MaxValue;
         Rows = Columns = (int)Math.Sqrt(streamlen / 2);     // standard is square

         data = new short[Rows * Columns];               // 2 byte per value
         NotValid = 0;
         for (int i = 0; i < data.Length; i++) {
            data[i] = (short)((str.ReadByte() << 8) + str.ReadByte());

            if (data[i] != HGT_NOVALUE) {
               if (Maximum < data[i])
                  Maximum = data[i];
               if (Minimum > data[i])
                  Minimum = data[i];
            } else {
               NotValid++;
               data[i] = DEMNOVALUE;
            }
         }
         if (NotValid == data.Length) {
            Maximum =
            Minimum = DEMNOVALUE;
         }
      }

   }
}
