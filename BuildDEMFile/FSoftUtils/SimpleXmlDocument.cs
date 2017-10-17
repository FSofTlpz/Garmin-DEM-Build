/* Erweiterung der Klasse XmlDocument
 * zur einfacheren Verarbeitung und zur Verwendung f�r Konfigurations- und Datendateien
 * 
 * copyright by FSofT
 * 
 * letzte �nderung
 * 9.12.2014
 * 
 */
#define NO_DEBUGOUTPUT
#define HIDE_OBSOLETE

using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;
using System.Text;
using System.Collections.Generic;

namespace FSoftUtils {


   /// <summary>
   /// Erweiterung von XmlDocument um insbesondere per XPath Daten lesen und ver�ndern zu k�nnen
   /// 
   ///      ExistXPath("/*/node1/@attribute1")
   ///         Testet die Existenz des XPATH
   /// 
   /// Lesen von Daten mit Read...(), z.B.:
   /// 
   ///      ReadValue("/*/node1[@attribut1=wert1]", 999)
   ///         Der Ergebnistyp h�ngt vom Typ des default-Wertes ab. Der default-Wert wird immer dann geliefert, wenn der XPATH nicht existiert, eine Konvertierung in
   ///         den default-Typ nicht m�glich ist oder es mehr als 1 Ergebnis gibt.
   ///         
   ///      ReadInt("/*/node1", 0), ReadBool usw.
   ///         Liefert ein Ergebnis-Array oder null, wenn der XPATH nicht ex.. Ist die Konvertierung nicht m�glich wird der default-Wert geliefert.
   ///         
   ///      ReadAttributes("/*/node1")
   ///         Liefert alle Attribut-Wert-Paare f�r (jeden) Knoten "node1"
   /// 
   /// �ndern von Daten (ACHTUNG: Der XPath kann prinzipiell mehrere Zielpositionen adressieren.):
   ///         
   ///      Change("/*/node1", "wert1")
   ///         F�r (jeden) "node1" wird der Wert neu gesetzt.
   ///      Change("/*/node1/@attribut1", "wert2")
   ///         F�r (jeden) "node1" wird das Attribut neu gesetzt.
   ///         Mehrere Attribute eines Knotens k�nnen gleichzeitig mit Append() gesetzt werden.
   ///      Change("/*/node1[1]", "wert1")
   ///         F�r den 1. "node1" wird der Wert neu gesetzt.
   ///
   ///      Remove("/*/node1/@attribut1")
   ///         Das Attribut wird in (jedem) "node1" gel�scht.
   ///      Remove("/*/node1")
   ///         (Jeder) "node1" mit allen untergeordneten Knoten/Attributen wird gel�scht.
   ///      Remove("/*/node1[3]")
   ///         Der 3. "node1" wird mit allen untergeordneten Knoten/Attributen gel�scht.
   ///         
   ///      Append("/*/node1", "node2", "wert2")
   ///         An (jeden) "node1" wird ein neuer untergeordneter Knoten "node2" mit dem Wert "wert2" angeh�ngt.
   ///      Append("/*/node1", "node2", "wert2", new Dictionary<string, string>() { { "attribut1", "wert3" }, { "attribut2", null } })
   ///         An (jeden) "node1" wird ein neuer untergeordneter Knoten "node2" mit dem Wert "wert2" und den 2 Attributen angeh�ngt.
   ///      Append("/*/node1", "node2", null, new Dictionary<string, string>() { { "attribut1", "wert3" }, { "attribut2", null } })
   ///         An (jeden) "node1" wird ein neuer untergeordneter Knoten "node2" ohne Wert aber mit den 2 Attributen angeh�ngt.
   ///      Append("/*/node1", "node2", "wert2", null, true)
   ///         An (jeden) "node1" wird ein neuer untergeordneter Knoten "node2" angeh�ngt, wenn er nicht schon ex. Auf jeden Fall wird der Wert von "node2" auf "wert2" gesetzt.
   ///      Append("/*/node1", null, null, new Dictionary<string, string>() { { "attribut1", "wert3" }, { "attribut2", null } })
   ///         An (jeden) "node1" werden die 2 Attributen angeh�ngt bzw. ge�ndert.
   ///         Ein einzelnes Attribut kann auch mit Change() gesetzt werden.
   /// 
   ///      InsertNode(string xpath4node, string nodename, string nodevalue = null, Dictionary<string, string> attributes = null, bool afterxpath = true)
   ///      
   ///      InsertNode("/*/node1[3]", "node1", "wert1", new Dictionary<string, string>() { { "attribut1", "wert3" }, { "attribut2", null } }, false)
   ///         Vor dem 3. "node1" wird ein neuer "node1" mit dem Wert "wert1" und den 2 Attributen eingef�gt.
   ///      InsertNode("/*/node1[3]", "node1")
   ///         Nach dem 3. "node1" wird ein neuer "node1" ohne Wert eingef�gt.
   /// 
   /// </summary>
   public class SimpleXmlDocument2 : XmlDocument {

      /// <summary>
      /// letzter Schreibzeitpunkt der Datei
      /// </summary>
      DateTime lastwrite;
      /// <summary>
      /// interner Standard-Navigator
      /// </summary>
      XPathNavigator navigator;
      /// <summary>
      /// interner NamespaceManager
      /// </summary>
      XmlNamespaceManager NsMng;


      /// <summary>
      /// 
      /// </summary>
      /// <param name="sFile">XML-Datei</param>
      /// <param name="sRoot">Wurzelname</param>
      /// <param name="sXsdFile">XSD-Datei</param>
      /// <param name="sEncoding">Codierung:
      /// utf-16, Unicode;
      /// windows-1250, Mitteleurop�isch (Windows);
      /// Windows-1252, Westeurop�isch (Windows);
      /// us-ascii, US-ASCII;
      /// IBM273, IBM EBCDIC (Deutschland);
      /// iso-8859-1, Westeurop�isch (ISO);
      /// iso-8859-2, Mitteleurop�isch (ISO);
      /// utf-7, Unicode (UTF-7);
      /// utf-8, Unicode (UTF-8);
      /// utf-32, Unicode (UTF-32);
      /// utf-32BE, Unicode (UTF-32-Big-Endian);
      /// </param>
      public SimpleXmlDocument2(string sFile = null, string sRoot = null, string sXsdFile = null, string sEncoding = null) {
         CheckNewFile = false;
         Validating = true;
         NsMng = null;
         navigator = null;
         Declaration = CreateXmlDeclaration("1.0", sEncoding != null ? sEncoding : "Windows-1252", null);
         Rootname = string.IsNullOrEmpty(sRoot) ? "dummy" : sRoot;
         XmlFilename = string.IsNullOrEmpty(sFile) ? "dummy.xml" : sFile;
         XsdFilename = string.IsNullOrEmpty(sXsdFile) ? "" : sXsdFile;
         lastwrite = DateTime.MinValue;
      }

      string _sXmlFile;
      /// <summary>
      /// setzt oder liefert den XML-Dateinamen
      /// </summary>
      public string XmlFilename {
         get {
            return _sXmlFile;
         }
         set {
            _sXmlFile = value;
            XsdFilename = Path.GetFileNameWithoutExtension(_sXmlFile) + ".xsd";
         }
      }

      /// <summary>
      /// setzt oder liefert den XSD-Dateinamen
      /// </summary>
      public string XsdFilename { get; set; }

      string _sXmlRootName;
      /// <summary>
      /// setzt oder liefert den Name des Wurzelknotens
      /// </summary>
      public string Rootname {
         get {
            return _sXmlRootName;
         }
         set {
            _sXmlRootName = value;
            if (string.IsNullOrEmpty(_sXmlRootName))
               throw new Exception("Kein Name f�r die XML-Root angegeben!");
         }
      }

      /// <summary>
      /// setzt oder liefert die XML-Deklaration
      /// </summary>
      public XmlDeclaration Declaration { get; set; }

      /// <summary>
      /// Erfolgt beim Lesen und der Schreiben der XML-Datei eine Validierung?
      /// </summary>
      public bool Validating { get; set; }

      /// <summary>
      /// 'true', wenn die XML-Datei existiert
      /// </summary>
      /// <returns></returns>
      public bool FileExist() {
         return File.Exists(XmlFilename);
      }

      /// <summary>
      /// Soll beim Datenlesen jeweils getestet werden, ob die Datei ge�ndert wurde?
      /// </summary>
      public bool CheckNewFile { get; set; }

      /// <summary>
      /// falls die Datei einen j�ngeren Zeitstempel als beim letzten Einlesen hat werden die Daten neu eingelesen
      /// </summary>
      void ReadNewerFile() {
         if (CheckNewFile &&
             File.GetLastWriteTime(XmlFilename) > lastwrite)
            LoadData();
      }

      /// <summary>
      /// Die Konfigurationsdatei wird neu (und ev. mit Validierung!) gelesen. Bei einem Fehler wird eine Exception ausgel�st.
      /// </summary>
      public void LoadData() {
         if (FileExist()) {
            try {
               lastwrite = File.GetLastWriteTime(XmlFilename);
               if (Validating) {
                  XmlReaderSettings xml_rset = new XmlReaderSettings();
                  xml_rset.Schemas.Add(null, XsdFilename);
                  xml_rset.ValidationType = ValidationType.Schema;
                  xml_rset.ValidationEventHandler += new ValidationEventHandler(XmlValidationEventHandler);
                  XmlReader xreader = XmlReader.Create(XmlFilename, xml_rset);
                  Load(xreader);
                  xreader.Close();
                  Rootname = DocumentElement.Name;
               } else
                  Load(XmlFilename);
               navigator = CreateNavigator();
            } catch (Exception ex) {
               throw new Exception(string.Format("Fehler beim Lesen der Datei '{0}': {1}", XmlFilename, ex.Message));
            }
         } else
            throw new Exception(string.Format("Fehler beim Lesen der Datei '{0}': Die Datei existiert nicht!", XmlFilename));
      }

      /// <summary>
      /// f�gt einen Namespace f�r XPATH an
      /// </summary>
      /// <param name="sPrefix"></param>
      /// <param name="sUrl"></param>
      public void AddNamespace(string sPrefix, string sUrl) {
         if (NsMng == null)
            NsMng = new System.Xml.XmlNamespaceManager(NameTable);
         NsMng.AddNamespace(sPrefix, sUrl);
      }

      static void XmlValidationEventHandler(object sender, ValidationEventArgs e) {
         string ext = "";
         if (e.Exception is XmlSchemaValidationException) {
            XmlSchemaValidationException xmle = (XmlSchemaValidationException)e.Exception;
            ext = xmle.Message + ": " + System.Environment.NewLine;
            if (xmle.SourceObject is XmlAttribute) {
               XmlAttribute attr = xmle.SourceObject as XmlAttribute;
               if (attr.OwnerElement != null)
                  ext += attr.OwnerElement.OuterXml;
            } else
               if (xmle.SourceObject is XmlElement) {
                  XmlElement elem = xmle.SourceObject as XmlElement;
                  if (elem.ParentNode != null)
                     ext += elem.ParentNode.OuterXml;
               } else
                  if (xmle.SourceObject is XmlNode) {
                     XmlNode n = xmle.SourceObject as XmlNode;
                     ext += n.OuterXml;
                  } else
                     if (xmle.SourceObject != null)
                        ext = xmle.SourceObject.ToString();
         }
         if (ext.Length > 300)
            ext = ext.Substring(0, 300) + " ...";
         throw new Exception(ext.Length == 0 ? e.Message : ext);
      }

      /// <summary>
      /// XML-Datei speichern
      /// </summary>
      /// <param name="filename">Name der XML-Datei</param>
      public bool SaveData(string filename = null) {
         if (filename == null)
            filename = XmlFilename;
         if (!ValidateInternData())
            return false;
         try {
            Save(filename);
         } catch (Exception ex) {
            throw new Exception(ex.Message);
         }
         return true;
      }

      /// <summary>
      /// testet die aktuell im Speicher befindlichen XML-Daten, ob sie XML-konform sind
      /// </summary>
      /// <returns></returns>
      public bool ValidateInternData() {
         return ValidateInternDataMsg() == "";
      }

      /// <summary>
      /// testet die aktuell im Speicher befindlichen XML-Daten, ob sie XML-konform sind
      /// </summary>
      /// <returns>leere Zeichenkette oder Fehlertext</returns>
      public string ValidateInternDataMsg() {
         if (!Validating)
            return "";
         try {
            if (this.Schemas.Count == 0)
               this.Schemas.Add(null, this.XsdFilename);
            this.Validate(new ValidationEventHandler(XmlValidationEventHandler));
         } catch (Exception exception) {
            return exception.Message;
         }
         return "";
      }

      /// <summary>
      /// 'true', wenn Daten existieren
      /// </summary>
      /// <returns></returns>
      public bool InternDataExist() {
         return DocumentElement.LocalName == Rootname && DocumentElement.HasChildNodes;
      }

      /// <summary>
      /// ev. schon vorhandene Daten werden gel�scht und eine neue Struktur erzeugt, die nur die Deklaration und das Root-Element hat
      /// </summary>
      /// <param name="sXslStylesheet"></param>
      /// <param name="sComment"></param>
      public void CreateInternData(string sXslStylesheet = null, string sComment = null) {
         if (this.HasChildNodes) this.RemoveAll();
         AppendChild(Declaration);
         AppendChild(CreateNode(XmlNodeType.Element, Rootname, null));
         if (XsdFilename.Length > 0) {
            string tmp = this.InnerXml;
            tmp = tmp.Substring(0, tmp.Length - 2) +
                  " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"" + Path.GetFileName(XsdFilename) + "\" />";
            this.RemoveAll();
            this.LoadXml(tmp);
         }
         navigator = CreateNavigator();
         if (sXslStylesheet != null) {
            //<?xml-stylesheet href=".... .xsl" type="text/xsl" ?>
            XmlProcessingInstruction pi = CreateProcessingInstruction("xml-stylesheet", "type='text/xsl' href='" + sXslStylesheet + "'");
            InsertAfter(pi, ChildNodes.Item(0));
         }
         if (sComment != null) {
            XmlNode comment = CreateNode(XmlNodeType.Comment, null, null);
            comment.Value = sComment;
            DocumentElement.AppendChild(comment);
         }
      }

      #region XPath-Funktionen zur Datenabfrage, zum Daten anf�gen, l�schen und �ndern

      // z.B.:
      // ReadValue("/bookstore/book[@genre=\"novel\"]/author/last-name");
      // ReadValue("/bookstore/book/@genre");

      XPathNodeIterator NavigatorSelect(string xpath) {
         return NsMng != null ?
                     navigator.Select(xpath, NsMng) :
                     navigator.Select(xpath);
      }

      /// <summary>
      /// Test, ob ein Navigator ex.
      /// </summary>
      void CheckNavigator() {
         if (navigator == null)
            throw new Exception("Es muss erst LoadData() oder CreateInternData() aufgerufen werden!");
      }

      /// <summary>
      /// Existiert der XPath?
      /// </summary>
      /// <param name="xpath">XPath</param>
      /// <returns></returns>
      public bool ExistXPath(string xpath) {
         CheckNavigator();
         try {
            return NavigatorSelect(xpath).Count > 0;
         } catch { }
         return false;
      }

      /// <summary>
      /// liefert die Daten entsprechend 'xpath' als Object-Array
      /// </summary>
      /// <param name="xpath"></param>
      /// <returns></returns>
      public object[] ReadValueAsObject(string xpath) {
         ReadNewerFile();
         CheckNavigator();
         object[] ret = null;
         try {
            XPathNodeIterator nodes = NavigatorSelect(xpath);
            if (nodes.Count == 0)
               return null;
            ret = new object[nodes.Count];
            int i = 0;
            while (nodes.MoveNext())
               ret[i++] = nodes.Current.TypedValue;
         } catch { }
         return ret;
      }

      /// <summary>
      /// liefert die Daten entsprechend 'xpath'; es muss genau 1 passender Knoten existieren, sonst wird 'defvalue' geliefert
      /// </summary>
      /// <param name="path">XPath</param>
      /// <param name="defvalue">vordefinierter Wert</param>
      /// <returns></returns>
      public string ReadValue(string xpath, string defvalue) {
         object[] o = ReadValueAsObject(xpath);
         if (o == null || o.Length != 1)
            return defvalue;
         return o[0].ToString();
      }
      /// <summary>
      /// liefert die Daten entsprechend 'xpath'; es muss genau 1 passender Knoten existieren, sonst wird 'defvalue' geliefert
      /// </summary>
      /// <param name="path">XPath</param>
      /// <param name="defvalue">vordefinierter Wert</param>
      /// <returns></returns>
      public bool ReadValue(string xpath, bool defvalue) {
         bool ret = defvalue;
         object[] o = ReadValueAsObject(xpath);
         if (o == null || o.Length != 1)
            return ret;
         try {
            ret = Convert.ToBoolean(o[0]);            // 'true' / 'false'
         } catch {
            try {
               ret = Convert.ToDouble(o[0]) != 0.0;   // alle Zahlen != 0 als true
            } catch { }
         }
         return ret;
      }
      /// <summary>
      /// liefert die Daten entsprechend 'xpath'; es muss genau 1 passender Knoten existieren, sonst wird 'defvalue' geliefert
      /// </summary>
      /// <param name="path">XPath</param>
      /// <param name="defvalue">vordefinierter Wert</param>
      /// <returns></returns>
      public int ReadValue(string xpath, int defvalue) {
         int ret = defvalue;
         object[] o = ReadValueAsObject(xpath);
         if (o == null || o.Length != 1)
            return ret;
         try {
            ret = Convert.ToInt32(o[0]);
         } catch { }
         return ret;
      }
      /// <summary>
      /// liefert die Daten entsprechend 'xpath'; es muss genau 1 passender Knoten existieren, sonst wird 'defvalue' geliefert
      /// </summary>
      /// <param name="path">XPath</param>
      /// <param name="defvalue">vordefinierter Wert</param>
      /// <returns></returns>
      public uint ReadValue(string xpath, uint defvalue) {
         uint ret = defvalue;
         object[] o = ReadValueAsObject(xpath);
         if (o == null || o.Length != 1)
            return ret;
         try {
            ret = Convert.ToUInt32(o[0]);
         } catch { }
         return ret;
      }
      /// <summary>
      /// liefert die Daten entsprechend 'xpath'; es muss genau 1 passender Knoten existieren, sonst wird 'defvalue' geliefert
      /// </summary>
      /// <param name="path">XPath</param>
      /// <param name="defvalue">vordefinierter Wert</param>
      /// <returns></returns>
      public double ReadValue(string xpath, double defvalue) {
         double ret = defvalue;
         object[] o = ReadValueAsObject(xpath);
         if (o == null || o.Length != 1)
            return ret;
         try {
            if (o[0].GetType() == Type.GetType("System.String"))     // Mono erkennt z.Z. NICHT den Typ Double --> Umwandlung aus String
               ret = Convert.ToDouble(o[0], System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            else
               ret = Convert.ToDouble(o[0]);
         } catch { }
         return ret;
      }

      /// <summary>
      /// liefert ein Datenarray entsprechend 'xpath'
      /// </summary>
      /// <param name="path">XPath</param>
      /// <returns></returns>
      public string[] ReadString(string xpath) {
         string[] ret = null;
         object[] o = ReadValueAsObject(xpath);
         if (o != null) {
            ret = new string[o.Length];
            for (int i = 0; i < o.Length; i++)
               ret[i] = o[i].ToString();
         }
         return ret;
      }
      /// <summary>
      /// liefert ein Datenarray entsprechend 'xpath'
      /// </summary>
      /// <param name="path">XPath</param>
      /// <returns></returns>
      public int[] ReadInt(string xpath, int defvalue) {
         int[] ret = null;
         object[] o = ReadValueAsObject(xpath);
         if (o != null) {
            ret = new int[o.Length];
            for (int i = 0; i < o.Length; i++)
               try {
                  ret[i] = Convert.ToInt32(o[i]);
               } catch {
                  ret[i] = defvalue;
               }
         }
         return ret;
      }
      /// <summary>
      /// liefert ein Datenarray entsprechend 'xpath'
      /// </summary>
      /// <param name="path">XPath</param>
      /// <returns></returns>
      public uint[] ReadUInt(string xpath, uint defvalue) {
         uint[] ret = null;
         object[] o = ReadValueAsObject(xpath);
         if (o != null) {
            ret = new uint[o.Length];
            for (int i = 0; i < o.Length; i++)
               try {
                  ret[i] = Convert.ToUInt32(o[i]);
               } catch {
                  ret[i] = defvalue;
               }
         }
         return ret;
      }
      /// <summary>
      /// liefert ein Datenarray entsprechend 'xpath'
      /// </summary>
      /// <param name="path">XPath</param>
      /// <returns></returns>
      public bool[] ReadBool(string xpath, bool defvalue) {
         bool[] ret = null;
         object[] o = ReadValueAsObject(xpath);
         if (o != null) {
            ret = new bool[o.Length];
            for (int i = 0; i < o.Length; i++)
               try {
                  ret[i] = Convert.ToBoolean(o[i]);
               } catch {
                  ret[i] = defvalue;
               }
         }
         return ret;
      }
      /// <summary>
      /// liefert ein Datenarray entsprechend 'xpath'
      /// </summary>
      /// <param name="path">XPath</param>
      /// <returns></returns>
      public double[] ReadDouble(string xpath, double defvalue) {
         double[] ret = null;
         object[] o = ReadValueAsObject(xpath);
         if (o != null) {
            ret = new double[o.Length];
            for (int i = 0; i < o.Length; i++)
               try {
                  ret[i] = Convert.ToDouble(o[i]);
               } catch {
                  ret[i] = defvalue;
               }
         }
         return ret;
      }

      /// <summary>
      /// liefert alle Attribut-Wert-Paare f�r jeden per xpath adressierten Knoten
      /// <para>Das jeweilige Dictionary ist leer, wenn keine Attribute ex.</para>
      /// </summary>
      /// <param name="xpath"></param>
      /// <returns></returns>
      public List<Dictionary<string, string>> ReadAttributes(string xpath) {
         List<Dictionary<string, string>> attr = null;
         ReadNewerFile();
         CheckNavigator();
         try {
            XPathNodeIterator nodes = NavigatorSelect(xpath);
            if (nodes != null && nodes.Count > 0) {
               attr = new List<Dictionary<string, string>>();
               while (nodes.MoveNext()) {
                  attr.Add(new Dictionary<string, string>());
                  navigator.MoveTo(nodes.Current);
                  if (navigator.MoveToFirstAttribute()) {         // es gibt Attribute
                     attr[attr.Count - 1].Add(navigator.Name, navigator.Value);
                     while(navigator.MoveToNextAttribute())
                        attr[attr.Count - 1].Add(navigator.Name, navigator.Value);
                  }
               }
            }
         } catch { }
         return attr;
      }

#if !HIDE_OBSOLETE

      /// <summary>
      /// liefert die Daten entsprechend 'xpath' als Zeichenketten-Array
      /// </summary>
      /// <param name="path">XPath</param>
      /// <returns></returns>
      [Obsolete("besser: ReadString()")]
      public string[] ReadValue(string xpath) {
         object[] o = ReadValueAsObject(xpath);
         if (o == null)
            return null;
         string[] ret = new string[o.Length];
         for (int i = 0; i < o.Length; i++)
            ret[i] = o[i].ToString();
         return ret;
      }

      /// <summary>
      /// einen Knoten (ev. mit Attribut) am XPath-Ziel (als letztes Child) anf�gen
      /// </summary>
      /// <param name="path">XPath</param>
      /// <param name="nodename">Name des neuen untergeordneten Knoten</param>
      /// <param name="nodevalue">Wert des Knotens</param>
      /// <param name="attribute">Attributname des Knotens</param>
      /// <param name="avalue">Wert der Attribute des Knotens</param>
      /// <returns>true, wenn erfolgreich</returns>
      [Obsolete("besser: Append()")]
      public bool AppendNodeOnXpath(string xpath, string nodename, string nodevalue = null, string attribute = null, string avalue = null) {
         return AppendNodeOnXpath(xpath,
                                  nodename,
                                  nodevalue,
                                  attribute != null ? new string[] { attribute } : null,
                                  avalue != null ? new string[] { avalue } : null);
      }
      /// <summary>
      /// einen Knoten mit mehreren Attributen am XPath-Ziel (als letztes Child) anf�gen
      /// </summary>
      /// <param name="path">XPath</param>
      /// <param name="nodename">Name des neuen untergeordneten Knoten; wenn null oder leer, werden die Attribute an xpath angeh�ngt</param>
      /// <param name="nodevalue">Wert des Knotens</param>
      /// <param name="attribute">Attributnamen des Knotens</param>
      /// <param name="avalue">Werte der Attribute des Knotens</param>
      /// <returns>true, wenn erfolgreich</returns>
      [Obsolete("besser: Append()")]
      public bool AppendNodeOnXpath(string xpath, string nodename, string nodevalue, string[] attribute, string[] avalue = null) {
         CheckNavigator();
         if (navigator.CanEdit) {
            XPathNodeIterator nodes = NavigatorSelect(xpath);     // auf xpath positionieren
            if (nodes != null &&
                nodes.Count == 0)                                 // i.A. sollte es genau 1 Knoten als Ergebnismenge sein
               return false;
            while (nodes.MoveNext()) {
               navigator.MoveTo(nodes.Current);                   // auf einen Knoten positionieren, der xpath entspricht
               if (!string.IsNullOrEmpty(nodename)) {
                  navigator.AppendChildElement(navigator.Prefix, nodename, navigator.LookupNamespace(navigator.Prefix), nodevalue);
                  if (attribute != null) {
                     navigator.MoveToFirstChild();
                     while (navigator.MoveToNext(XPathNodeType.Element)) ;
                     appendattribut(attribute, avalue);
                  }
               } else
                  appendattribut(attribute, avalue);
            }
            return true;
         }
         return false;
      }

      /// <summary>
      /// neues Attribut am XPath-Ziel hinzuf�gen
      /// </summary>
      /// <param name="path">XPath</param>
      /// <param name="attribute">Attributname</param>
      /// <param name="avalue">Wert des Attributs</param>
      /// <returns></returns>
      [Obsolete("besser: Append()")]
      public bool AppendAttributeOnXpath(string xpath, string attribute, string avalue = null) {
         CheckNavigator();
         if (navigator.CanEdit) {
            XPathNodeIterator nodes = NavigatorSelect(xpath);
            if (nodes != null &&
                nodes.Count == 0)
               return false;
            while (nodes.MoveNext()) {
               navigator.MoveTo(nodes.Current);
               string[] attr = { attribute };
               string[] aval = { avalue };
               appendattribut(attr, aval);
            }
            return true;
         }
         return false;
      }
      /// <summary>
      /// neue Attribute am XPath-Ziel hinzuf�gen
      /// </summary>
      /// <param name="path">XPath</param>
      /// <param name="attribute">Attributnamen</param>
      /// <param name="avalue">Werte des Attributs</param>
      /// <returns></returns>
      [Obsolete("besser: Append()")]
      public bool AppendAttributeOnXpath(string xpath, string[] attribute = null, string[] avalue = null) {
         CheckNavigator();
         if (navigator.CanEdit) {
            XPathNodeIterator nodes = NavigatorSelect(xpath);
            if (nodes != null &&
                nodes.Count == 0)
               return false;
            while (nodes.MoveNext()) {
               navigator.MoveTo(nodes.Current);
               appendattribut(attribute, avalue);
            }
            return true;
         }
         return false;
      }

      /// <summary>
      /// die Attribute werden an der akt. Navigatorposition angef�gt
      /// </summary>
      /// <param name="attribute"></param>
      /// <param name="avalue"></param>
      [Obsolete("besser: appendattribut(Dictionary)")]
      void appendattribut(string[] attribute, string[] avalue) {
         if (attribute != null) {
            string prefix = navigator.Prefix;
            string ns = navigator.LookupNamespace(prefix);
            for (int i = 0; i < attribute.Length; i++)
               if (!string.IsNullOrEmpty(attribute[i]))
                  navigator.CreateAttribute(prefix,
                                            attribute[i],
                                            ns,
                                            avalue != null && avalue.Length > i ? avalue[i] : null);
         }
      }

#endif

      /// <summary>
      /// An den XPATH wird ein neuer Knoten mit mehreren Attributen als letzter Knoten der Liste angef�gt. Ist kein Knotenname angegeben, werden
      /// die Attribute an den XPATH selbst angef�gt. Schon vorhandene Attribute werden aktualisiert.
      /// </summary>
      /// <param name="xpath">XPATH f�r den neuen Knoten mit Attributen oder XPATH f�r neue Attribute</param>
      /// <param name="nodename">Name des neuen Knotens oder null</param>
      /// <param name="nodevalue">Wert des neuen Knotens oder null</param>
      /// <param name="attributes">Attribut-Werte-Paare (null-Werte werden als leere Zeichenkette interpretiert)</param>
      /// <param name="unique">wenn true, wird ein ev. schon mit dem Namen existierender Knoten verwendet (also ist der Knotename eindeutig)</param>
      /// <returns>false, wenn nichts angef�gt werden konnte (weil z.B. der xpath nicht existiert)</returns>
      public bool Append(string xpath, string nodename, string nodevalue = null, Dictionary<string, string> attributes = null, bool unique = false) {
         CheckNavigator();
         if (navigator.CanEdit) {
            XPathNodeIterator nodes = NavigatorSelect(xpath);     // auf xpath positionieren
            if (nodes != null &&
                nodes.Count == 0)                                 // i.A. sollte es genau 1 Knoten als Ergebnismenge sein
               return false;
            while (nodes.MoveNext()) {
               navigator.MoveTo(nodes.Current);                   // auf einen Knoten positionieren, der xpath entspricht
               if (!string.IsNullOrEmpty(nodename)) {
                  XPathNavigator testnavi = null;
                  if (unique)
                     testnavi = navigator.SelectSingleNode(nodename);

                  if (testnavi == null) {                         // Knoten wird auf jeden Fall angeh�ngt
                     navigator.AppendChildElement(navigator.Prefix,
                                                  nodename,
                                                  navigator.LookupNamespace(navigator.Prefix),
                                                  nodevalue);
                     if (attributes != null) {
                        navigator.MoveToFirstChild();
                        while (navigator.MoveToNext(XPathNodeType.Element)) ;    // Simulation: MoveToLastChild()
                        appendattribut(attributes);
                     }
                  } else {                                        // alter Knoten ex. und soll verwendet werden
                     testnavi.SetValue(nodevalue);
                     appendattribut(attributes, testnavi);
                  }
               } else
                  appendattribut(attributes);                     // nur Attribute am xpath anf�gen/�ndern
            }
            return true;
         }
         return false;
      }

      /// <summary>
      /// Zum XPATH wird nebengeordnet ein neuer Knoten davor oder danach eingef�gt.
      /// </summary>
      /// <param name="xpath">XPATH f�r den neuen Knoten mit Attributen oder XPATH f�r neue Attribute</param>
      /// <param name="nodename">Name des neuen Knotens oder null</param>
      /// <param name="nodevalue">Wert des neuen Knotens oder null</param>
      /// <param name="attributes">Attribut-Werte-Paare (null-Werte werden als leere Zeichenkette interpretiert)</param>
      /// <param name="afterxpath">bei true wird der neue Knoten danach eingef�gt, sonst davor</param>
      /// <returns></returns>
      public bool InsertNode(string xpath4node, string nodename, string nodevalue = null, Dictionary<string, string> attributes = null, bool afterxpath = true) {
         CheckNavigator();
         if (navigator.CanEdit) {
            XPathNodeIterator nodes = NavigatorSelect(xpath4node);     // auf xpath positionieren
            if (nodes != null &&
                nodes.Count == 0)                                 // i.A. sollte es genau 1 Knoten als Ergebnismenge sein
               return false;
            while (nodes.MoveNext()) {
               navigator.MoveTo(nodes.Current);                   // auf einen Knoten positionieren, der xpath entspricht

               if (afterxpath) {
                  navigator.InsertElementAfter(navigator.Prefix,
                                               nodename,
                                               navigator.LookupNamespace(navigator.Prefix),
                                               nodevalue);
                  navigator.MoveToNext();
               } else {
                  navigator.InsertElementBefore(navigator.Prefix,
                                                nodename,
                                                navigator.LookupNamespace(navigator.Prefix),
                                                nodevalue);
                  navigator.MoveToPrevious();
               }
               appendattribut(attributes);
            }
            return true;
         }
         return false;
      }

      /// <summary>
      /// die Attribute werden an der akt. Navigatorposition angef�gt bzw. aktualisiert
      /// </summary>
      /// <param name="attributes">Attribut-Werte-Paare (null-Werte werden als leere Zeichenkette interpretiert)</param>
      /// <param name="parentnavi">Navigator, an dem die Attribute eingef�gt werden sollen oder null f�r den internen Standardnavigator</param>
      void appendattribut(Dictionary<string, string> attributes, XPathNavigator parentnavi = null) {
         if (attributes != null) {
            if (parentnavi == null)
               parentnavi = navigator;
            string prefix = parentnavi.Prefix;
            string ns = parentnavi.LookupNamespace(prefix);
            foreach (var item in attributes) {
               if (!string.IsNullOrEmpty(item.Key)) {
                  XPathNavigator testnavi = parentnavi.SelectSingleNode("@" + item.Key);
                  if (testnavi == null)     // Attr. ex. noch nicht
                     parentnavi.CreateAttribute(prefix,
                                               item.Key,
                                               ns,
                                               !string.IsNullOrEmpty(item.Value) ? item.Value : "");
                  else
                     testnavi.SetValue(!string.IsNullOrEmpty(item.Value) ? item.Value : "");
               }
            }
         }
      }

      /// <summary>
      /// l�scht alle Nodes und deren untergeordnete Nodes auf die der 'xpath' passt
      /// </summary>
      /// <param name="path"></param>
      /// <returns>false, wenn nichts gel�scht wurde</returns>
      public bool Remove(string xpath) {
         CheckNavigator();
         if (navigator.CanEdit) {
            XPathNodeIterator it = NavigatorSelect(xpath);
            if (it != null &&
                it.Count == 0)
               return false;
            XmlNode[] node2del = new XmlNode[it.Count];
            XmlNode[] nodeparent2del = new XmlNode[it.Count];
            int n = 0;
            while (it.MoveNext()) {
               if (it.Current is IHasXmlNode) {
                  node2del[n] = ((IHasXmlNode)it.Current).GetNode();
                  if (node2del[n].NodeType == XmlNodeType.Attribute) {     // Dann wird der Parent expliziet ben�tigt, da er in XmlAttribute null ist!
                     XPathNavigator nodesNavigatorParent = it.Current.Clone();
                     nodesNavigatorParent.MoveToParent();
                     nodeparent2del[n] = ((IHasXmlNode)nodesNavigatorParent).GetNode();
                  }
                  n++;
               }
            }

            navigator.MoveToRoot();

            // jetzt erfolgt das l�schen
            for (int i = 0; i < node2del.Length; i++)
               if (node2del[i] != null) {
                  if (node2del[i].NodeType == XmlNodeType.Attribute)
                     nodeparent2del[i].Attributes.Remove((XmlAttribute)(node2del[i]));
                  else {
                     if (node2del[i].ParentNode != null)                // kann nur bei der Root sein
                        node2del[i].ParentNode.RemoveChild(node2del[i]);
                  }
               }
            return true;
         }
         return false;
      }

      /// <summary>
      /// Knoten oder Attribute �ndern auf die der 'xpath' passt
      /// </summary>
      /// <param name="path">XPath</param>
      /// <param name="value">neuer Wert</param>
      /// <returns>true, wenn der Wert ge�ndert wurde</returns>
      public bool Change(string xpath, string value) {
         CheckNavigator();
         if (navigator.CanEdit) {
            XPathNodeIterator nodes = NavigatorSelect(xpath);
            if (nodes != null &&
                nodes.Count == 0)
               return false;
            while (nodes.MoveNext()) {
               navigator.MoveTo(nodes.Current);
               navigator.SetValue(value);
            }
            return true;
         }
         return false;
      }

      /* Problem:
       * XPATH mag keine (') und (") innerhalb der Argumente. Wenn ein (') auftaucht k�nnte man die Zeichenketten in (") einschlie�en
       * bzw. wenn (") enthalten ist k�nnen die Zeichenkette in (') eingeschlossen werden. Problematisch wird es, wenn sowohl (') als
       * auch (") in einer Zeichenkette auftauchen k�nnen.
       * Dann kann man die Funktion concate() verwenden. Die Originalzeichenkette wird zerlegt, so dass die enthaltenen (') und (") jeweils
       * einzeln behandelt werden. (') werden in (") und (") in (') eingeschlossen. Aufwendig, aber wohl die einzige M�glichkeit.
       * ACHTUNG!
       * Sonderzeichen (&) usw. m�ssen NICHT umgewandelt werden.
       */
      public static string GenerateConcatForXPath(string sXPathQueryString) {
         string returnString = string.Empty;
         string searchString = sXPathQueryString;
         char[] quoteChars = new char[] { '\'', '"' };

         int quotePos = searchString.IndexOfAny(quoteChars);
         if (quotePos == -1)
            returnString = "'" + searchString + "'";
         else {
            returnString = "concat(";
            while (quotePos != -1) {
               string subString = searchString.Substring(0, quotePos);
               returnString += "'" + subString + "', ";
               if (searchString.Substring(quotePos, 1) == "'")
                  returnString += "\"'\", ";
               else
                  returnString += "'\"', ";
               searchString = searchString.Substring(quotePos + 1, searchString.Length - quotePos - 1);
               quotePos = searchString.IndexOfAny(quoteChars);
            }
            returnString += "'" + searchString + "')";
         }
         return returnString;
      }

      #endregion

      #region Funktionen zum Erzeugen und Testen eines gesamten durch ein XmlElement-Array definierten Pfades

      /// <summary>
      /// erzeugt ein Knotenelement mit Attributen (f�r CreateAbsolutePath())
      /// </summary>
      /// <param name="nodename">Knotenname</param>
      /// <param name="nodevalue">Knotenwert</param>
      /// <param name="attribut">Attribute</param>
      /// <param name="avalue">Attributwerte</param>
      /// <returns></returns>
      public XmlElement CreateNodeElement(string nodename, string nodevalue = null, string[] attribut = null, string[] avalue = null) {
         XmlElement newElem = CreateElement(nodename, NamespaceURI);
         if (nodevalue != null)
            newElem.InnerText = nodevalue;
         if (attribut != null) {
            for (int i = 0; i < attribut.Length; i++)
               if (!string.IsNullOrEmpty(attribut[i]) &&
                   avalue[i] != null)
                  newElem.SetAttribute(attribut[i], avalue[i]);
         }
#if DEBUG && !NO_DEBUGOUTPUT
         Console.WriteLine("CreateNodeElement: " + GetXmlElement4Debug(newElem));
#endif
         return newElem;
      }

      /// <summary>
      /// erzeugt einen vollst�ndigen Pfad mit den gew�nschten Knoten, falls der Pfad noch nicht existiert, oder testet nur die Existenz
      /// </summary>
      /// <param name="nodepath">Beschreibung der Knoten (unterhalb (!) der Root); wird intern geklont; ein leerer Value bleibt unber�cksichtigt</param>
      /// <param name="depth">Anzahl der g�ltigen Elemente in nodepath (darf kleiner sein als die Arrayl�nge)</param>
      /// <param name="create">true, wenn der Knoten auch erzeugt werden soll</param>
      /// <param name="no">Nummer des Knotens mit der Beschreibung (es kann mehrere passende Knoten geben!; 1 f�r den 1.)</param>
      /// <returns></returns>
      public XmlElement CreateOrTestAbsolutePath(XmlElement[] nodepath, int depth = -1, bool create = true, int no = 1) {
         XmlNode node = DocumentElement;
         XmlElement childnode = null;
         if (depth < 0)
            depth = nodepath.Length;
         for (int d = 0; d < depth; d++) {
            bool found = false;
            foreach (XmlNode testnode in node) {               // alle Childs testen
               if (testnode.NodeType == XmlNodeType.Element)
                  if (testnode.Name == nodepath[d].Name) {                                   // Stimmt der Knotenname �berein?
                     if (d < depth - 1 ||                                                    // Knotenwert nur beim letzten Knoten interessant
                         nodepath[d].InnerText.Length == 0 ||                                // kein Knotenwert vorgegeben
                         nodepath[d].InnerText == testnode.InnerText) {                      // Knotenwert gleich
                        found = true;
                        // alle Knotenattribute vergleichen
                        foreach (XmlAttribute a in nodepath[d].Attributes) {
                           if (!(((XmlElement)testnode).HasAttribute(a.Name) &&              // nicht existierendes 
                                 ((XmlElement)testnode).GetAttribute(a.Name) == a.Value)) {  // oder nicht �bereinstimmendes Attribut gefunden
                              found = false;
                              break;
                           }
                        }
                        if (found) {
                           if (--no <= 0) {
                              node = testnode;
                              childnode = (XmlElement)node;
                              break;
                           }
                           found = false;
                        }
                     }
                  }
            }
            if (!found) {                          // nicht vorhanden, also ev. erzeugen
               if (!create) {
#if DEBUG && !NO_DEBUGOUTPUT
                  Console.WriteLine("CreateOrTestAbsolutePath: NICHT gefunden und NICHT erzeugt");
#endif
                  return null;           // dann nur zum Test
               }
               XmlNode newnode = nodepath[d].CloneNode(true);
               node.AppendChild(newnode);
               node = newnode;
#if DEBUG && !NO_DEBUGOUTPUT
               Console.WriteLine("CreateOrTestAbsolutePath: NICHT gefunden aber erzeugt");
#endif
            }
         }
#if DEBUG && !NO_DEBUGOUTPUT
         Console.WriteLine("CreateOrTestAbsolutePath: " + GetXmlElement4Debug((XmlElement)node));
#endif
         return (XmlElement)node;                  // tiefstes Element zur�ckliefern
      }

      /// <summary>
      /// Testet, ob der Pfad mit den gew�nschten Knoten existiert; wenn nicht wird null geliefert
      /// </summary>
      /// <param name="nodepath">Beschreibung der Knoten (unterhalb (!) der Root); wird intern geklont; ein leerer Value bleibt unber�cksichtigt</param>
      /// <param name="depth">Anzahl der g�ltigen Elemente in nodepath (darf kleiner sein als die Arrayl�nge)</param>
      public XmlElement ExistAbsolutePath(XmlElement[] nodepath, int depth = -1) {
         return CreateOrTestAbsolutePath(nodepath, depth >= 0 ? depth : nodepath.Length, false);
      }

      #endregion

      #region Hilfsfunktionen f�r das Debugging
#if DEBUG

      /// <summary>
      /// zur vereinfachten Darstellung beim Debugging
      /// </summary>
      /// <param name="nodepath"></param>
      /// <returns></returns>
      protected string GetNodepath4Debug(XmlElement[] nodepath) {
         if (nodepath == null || nodepath.Length == 0)
            return "";
         string[] elemtxt = new string[nodepath.Length];
         for (int i = 0; i < nodepath.Length; i++)
            elemtxt[i] = GetXmlElement4Debug(nodepath[i]);
         return string.Join("/", elemtxt);
      }
      /// <summary>
      /// zur vereinfachten Darstellugn beim Debugging
      /// </summary>
      /// <param name="elem"></param>
      /// <returns></returns>
      protected string GetXmlElement4Debug(XmlElement elem) {
         string txt = "";
         if (elem != null) {
            txt = elem.Name + "[";
            foreach (XmlAttribute attr in elem.Attributes)
               txt += "{" + attr.Name + "=" + attr.Value + "}";
            txt += "]";
         }
         return txt;
      }
      /// <summary>
      /// zur vereinfachten Darstellugn beim Debugging
      /// </summary>
      /// <param name="elem"></param>
      /// <returns></returns>
      protected string GetXmlElementWithPath4Debug(XmlElement elem) {
         string txt = "";
         if (elem != null) {
            txt = GetXmlElement4Debug(elem);
            elem = (XmlElement)elem.ParentNode;
            while (elem != null)
               txt = GetXmlElement4Debug(elem) + "/" + txt;
         }
         return txt;
      }

#endif
      #endregion

      /// <summary>
      /// erzeugt ein Child-Element mit Attribut und Wert
      /// </summary>
      /// <param name="parent">Parent-Element</param>
      /// <param name="nodename">Name</param>
      /// <param name="nodeattribut">Attributname</param>
      /// <param name="nodevalue">Wert</param>
      /// <returns></returns>
      protected XmlElement AppendChildElement(XmlElement parent, string nodename, string nodeattribut, string nodevalue) {
         XmlElement newElem = CreateElement(nodename, parent.NamespaceURI);
         if (nodeattribut.Length > 0)
            newElem.SetAttribute(nodeattribut, nodevalue);
         parent.AppendChild(newElem);
#if DEBUG && !NO_DEBUGOUTPUT
         Console.WriteLine(string.Format("AppendChildElement: parent: {0}, newElem: {1}", GetXmlElementWithPath4Debug(parent), GetXmlElement4Debug(newElem)));
#endif
         return newElem;
      }

      /// <summary>
      /// erzeugt eine Kopie der aktuellen Instanz
      /// </summary>
      /// <returns>null bei Fehler</returns>
      public SimpleXmlDocument2 CloneSimpleXmlDocument() {
         SimpleXmlDocument2 doc = new SimpleXmlDocument2(XmlFilename, Rootname);
         doc.CreateInternData();
         doc.DocumentElement.InnerXml = DocumentElement.InnerXml;
         return doc;
      }

      /// <summary>
      /// liefert den Inhalt als formatierten Text
      /// </summary>
      public string AsFormattedText {
         get {
            MemoryStream ms = new MemoryStream();
            Save(ms);
            ms.Position = 0;
            using (StreamReader reader = new StreamReader(ms)) {
               return reader.ReadToEnd();
            }
         }
      }

      public override string ToString() {
         return string.Format("{0}, XSD: {1}, Root: {2}", XmlFilename, XsdFilename, Rootname);
      }

   }

}

