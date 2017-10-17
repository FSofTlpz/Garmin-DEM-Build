using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace SimpleProgControl {
   public partial class Form1 : Form {

      // mit "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\Common7\Tools\spyxx.exe" Fensterklassen u.ä. ermitteln


      /// <summary>
      /// Daten eines Windows
      /// </summary>
      public class ApiWindow {
         /// <summary>
         /// Klassenname
         /// </summary>
         public string ClassName { get; private set; }
         /// <summary>
         /// Fenster-Handle
         /// </summary>
         public IntPtr hWnd { get; private set; }

         public ApiWindow(IntPtr hwnd, string classname = "") {
            hWnd = hwnd;
            ClassName = classname;
         }
         /// <summary>
         /// 1. Fenster dieser Klasse und mit dieser Überschrift
         /// </summary>
         /// <param name="classname"></param>
         /// <param name="caption"></param>
         public ApiWindow(string classname, string caption) {
            ClassName = classname;
            hWnd = Win32User.FindWindow(classname, caption);
         }

         /// <summary>
         /// liefert den Text (i.A. Caption) des Fensters
         /// </summary>
         /// <returns></returns>
         public string GetText() {
            Int32 size = Win32User.SendMessage(hWnd, Win32User.WM_GETTEXTLENGTH, 0, 0).ToInt32();
            if (size > 0) {      // sonst kein Text
               StringBuilder title = new StringBuilder(size + 1);
               Win32User.SendMessage(hWnd, Win32User.WM_GETTEXT, title.Capacity, title);
               return title.ToString();
            }
            return "";
         }

         /// <summary>
         /// liefert die Prozess-ID zum Fenster
         /// </summary>
         /// <param name="hWnd"></param>
         /// <param name="bIsProgWindow"></param>
         /// <returns></returns>
         public uint GetProcessId(bool bIsProgWindow = true) {
            IntPtr hWnd = this.hWnd;
            if (!bIsProgWindow)  // Programmfenster ermitteln
               hWnd = Win32User.GetAncestor(hWnd, Win32User.GetAncestorFlags.GetRoot);
            uint processId;
            Win32User.GetWindowThreadProcessId(hWnd, out processId);
            return processId;
         }

         /// <summary>
         /// versucht, das Fenster in den Vordergrund zu setzen
         /// </summary>
         /// <returns></returns>
         public bool SetForeground() {
            return Win32User.SetForegroundWindow(hWnd);
         }

         /// <summary>
         /// liefert die Fenstergröße und Position
         /// </summary>
         /// <returns></returns>
         public Rectangle WindowRect() {
            Win32User.RECT result;
            Win32User.GetWindowRect(hWnd, out result);
            return new Rectangle(result.Left, result.Top, result.Right - result.Left, result.Bottom - result.Top);
         }

         /// <summary>
         /// liefert die Clientgröße (Position ist immer 0,0)
         /// </summary>
         /// <returns></returns>
         public Rectangle ClientRect() {
            Win32User.RECT result;
            Win32User.GetClientRect(hWnd, out result);
            return new Rectangle(result.Left, result.Top, result.Right - result.Left, result.Bottom - result.Top);
         }

         /// <summary>
         /// liefert die Bildschirmkoordinaten zu den Clientkoordinaten
         /// </summary>
         /// <param name="pt"></param>
         /// <returns></returns>
         public Point ClientToScreen(Point pt) {
            Win32User.Point winpt = new Win32User.Point() {
               X = pt.X,
               Y = pt.Y
            };
            if (Win32User.ClientToScreen(hWnd, ref winpt))
               return new Point(winpt.X, winpt.Y);
            return Point.Empty;
         }

         /// <summary>
         /// liefert die Clientkoordinaten zu den Bildschirmkoordinaten
         /// </summary>
         /// <param name="pt"></param>
         /// <returns></returns>
         public Point ScreenToClient(Point pt) {
            Win32User.Point winpt = new Win32User.Point() {
               X = pt.X,
               Y = pt.Y
            };
            if (Win32User.ScreenToClient(hWnd, ref winpt))
               return new Point(winpt.X, winpt.Y);
            return Point.Empty;
         }

         public override string ToString() {
            return string.Format("hWnd=0x{0:X}, classname='{1}', text='{2}'", hWnd, ClassName, GetText());
         }

      }

      /// <summary>
      /// Liste der Child-Windows
      /// </summary>
      public class ChildWindows {
         /// <summary>
         /// Liste der Child-Windows
         /// </summary>
         public List<ApiWindow> WindowList { get; private set; }

         string childClass;


         /// <summary>
         /// erzeugt eine Liste der Child-Windows
         /// </summary>
         /// <param name="hWndParent">Parent-Window</param>
         /// <param name="childClass">Klassenname der Child-Windows</param>
         public ChildWindows(IntPtr hWndParent, string childClass = null) {
            WindowList = new List<ApiWindow>();
            this.childClass = string.IsNullOrEmpty(childClass) ? "" : childClass.ToLower();
            Win32User.EnumChildWindows(hWndParent.ToInt32(), EnumChildWindowProc, 0);
         }

         Int32 EnumChildWindowProc(Int32 hwnd, Int32 lParam) {
            StringBuilder classBuilder = new StringBuilder(64);
            Win32User.GetClassName(hwnd, classBuilder, 64);

            ApiWindow window = new ApiWindow(new IntPtr(hwnd), classBuilder.ToString());

            if (childClass.Length == 0 ||
                window.ClassName.ToLower() == childClass)
               WindowList.Add(window);
            return 1;
         }

         public override string ToString() {
            return string.Format("{0} Fenster", WindowList.Count);
         }

      }

      public class Helper {

         /// <summary>
         /// stoppt die Ausführung für einige ms
         /// </summary>
         /// <param name="ms"></param>
         public static void Wait(int ms = 500) {
            Thread.Sleep(ms);
         }

         static Stopwatch watch = new Stopwatch();

         public static void WaitExt(int ms = 500) {
            long ticks = (long)(Stopwatch.Frequency * ms / 1000);
            watch.Restart();
            while (watch.ElapsedTicks < ticks) ;
            watch.Stop();
         }

         /// <summary>
         /// sendet eine Tastenfolge an das Vordergrundfenster
         /// </summary>
         /// <param name="text">Tastenfolge
         /// <para>     TAB: {TAB}</para>
         /// <para>     ENTER: {ENTER} or ~</para>
         /// <para> </para>
         /// <para>     DOWN ARROW: {DOWN}</para>
         /// <para>     LEFT ARROW: {LEFT}</para>
         /// <para>     RIGHT ARROW: {RIGHT}</para>
         /// <para>     UP ARROW: {UP}</para>
         /// <para>     PAGE DOWN: {PGDN}</para>
         /// <para>     PAGE UP: {PGUP}</para>
         /// <para>     HOME: {HOME}</para>
         /// <para>     END: {END}</para>
         /// <para> </para>
         /// <para>     DELETE: {DELETE} or {DEL}</para>
         /// <para>     BACKSPACE: {BACKSPACE}, {BS}, or {BKSP}</para>
         /// <para> </para>
         /// <para>     INSERT: {INSERT} or {INS}</para>
         /// <para>     BREAK: {BREAK}</para>
         /// <para>     CAPS LOCK: {CAPSLOCK}</para>
         /// <para>     ESC: {ESC}</para>
         /// <para>     HELP: {HELP}</para>
         /// <para>     NUM LOCK: {NUMLOCK}</para>
         /// <para>     SCROLL LOCK: {SCROLLLOCK}</para>
         /// <para>     F1: {F1}</para>
         /// <para>     ...</para>
         /// <para>     F16: {F16}</para>
         /// <para>     Keypad add: {ADD}</para>
         /// <para>     Keypad subtract: {SUBTRACT}</para>
         /// <para>     Keypad multiply: {MULTIPLY}</para>
         /// <para>     Keypad divide: {DIVIDE} </para>
         /// <para> </para>
         /// <para>     SHIFT: +</para>
         /// <para>     CTRL: ^</para>
         /// <para>     ALT: % </para>
         /// <para> </para>
         /// <para>    z.B.: ^E          -> Strg + E</para>
         /// <para>          ^EC         -> Strg + E, C</para>
         /// <para>          ^(EC)       -> Strg + (E C)</para>
         /// <para>          {LEFT 12}   -> 12x Cursor links</para>
         /// </param>
         public static void SendKeys(string text) {
            System.Windows.Forms.SendKeys.SendWait(text);
         }

         /// <summary>
         /// wartet, bis das Vordergrundfenster alles abgearbeitet hat
         /// </summary>
         public static void Wait4Idle() {
            System.Windows.Forms.SendKeys.Flush();
         }

         /// <summary>
         /// wartet, bis für das Programm die Nachrichtenwarteschlange leer ist
         /// </summary>
         /// <param name="hWndProg"></param>
         public static void Wait4Idle(ApiWindow win) {
            Process p = Process.GetProcessById((int)win.GetProcessId(true));
            p.WaitForInputIdle();
         }

         /// <summary>
         /// setzt oder liefert die Mauspos. (globale Koordinaten!)
         /// </summary>
         public static Point MousePosition {
            get {
               return Cursor.Position;
            }
            set {
               Cursor.Position = value;
            }
         }

         /// <summary>
         /// liefert das oberste Fenster under der Position
         /// </summary>
         /// <param name="x"></param>
         /// <param name="y"></param>
         /// <returns></returns>
         public static IntPtr WindowUnderPosition(int x, int y) {
            Win32User.Point p = new Win32User.Point() { X = x, Y = y };
            return Win32User.WindowFromPoint(p);
         }

         /// <summary>
         /// Links-Klick
         /// </summary>
         public static void MouseLeftClick() {
            //uint X = (uint)Cursor.Position.X;
            //uint Y = (uint)Cursor.Position.Y;
            //Win32User.mouse_event(Win32User.MOUSEEVENTF_LEFTDOWN | Win32User.MOUSEEVENTF_LEFTUP, X, Y, 0, 0);

            // oder

            //Win32User.mouse_event(Win32User.MOUSEEVENTF_LEFTDOWN, X, Y, 0, 0);
            //Win32User.mouse_event(Win32User.MOUSEEVENTF_LEFTUP, X, Y, 0, 0);

            Win32User.Input[] MouseEvent = new Win32User.Input[2];
            MouseEvent[0].Type = 0;
            MouseEvent[0].Data = new Win32User.MouseInput() {
               X = 0,
               Y = 0,
               MouseData = 0,
               Time = 0,
               DwFlags = Win32User.MOUSEEVENTF_LEFTDOWN
            };
            MouseEvent[1].Type = 0;
            MouseEvent[1].Data = new Win32User.MouseInput() {
               X = 0,
               Y = 0,
               MouseData = 0,
               Time = 0,
               DwFlags = Win32User.MOUSEEVENTF_LEFTUP
            };
            Win32User.SendInput((uint)MouseEvent.Length, MouseEvent, Marshal.SizeOf(MouseEvent[0].GetType()));
         }

         /// <summary>
         /// Mausbewegung
         /// </summary>
         /// <param name="x">neue Position</param>
         /// <param name="y">neue Position</param>
         public static void MouseMove(int x, int y) {
            x = x * 65535 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            y = y * 65535 / System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

            Win32User.Input[] MouseEvent = new Win32User.Input[1];
            MouseEvent[0].Type = 0;
            MouseEvent[0].Data = new Win32User.MouseInput() {
               X = x,
               Y = y,
               MouseData = 0,
               Time = 0,
               DwFlags = Win32User.MOUSEEVENTF_ABSOLUTE | Win32User.MOUSEEVENTF_MOVE
            };
            Win32User.SendInput((uint)MouseEvent.Length, MouseEvent, Marshal.SizeOf(MouseEvent[0].GetType()));
         }

         //public unsafe static string GetPanelText1(IntPtr hStatusBar, int pos) {
         //   string text = "";
         //   // Programmfenster ermitteln
         //   IntPtr hwnd = GetAncestor(hStatusBar, GetAncestorFlags.GetRoot);
         //   // Prozess-ID ermitteln
         //   uint processId;
         //   uint threadId = GetWindowThreadProcessId(hwnd, out processId);
         //   if (processId > 0) {
         //      // Prozess öffnen
         //      IntPtr hProcess = OpenProcess(ProcessAccessFlags.All, false, processId);
         //      if (hProcess != IntPtr.Zero) {
         //         // Remote-Puffer anlegen
         //         const int BUFFER_SIZE = 0x1000;
         //         IntPtr ipRemoteBuffer = VirtualAllocEx(hProcess, IntPtr.Zero, new IntPtr(BUFFER_SIZE), AllocationType.COMMIT, MemoryProtection.READWRITE);
         //         if (ipRemoteBuffer != IntPtr.Zero) {
         //            // Textlänge und Text ermitteln
         //            int chars = SendMessage(hStatusBar.ToInt32(), SB_GETTEXT, pos, ipRemoteBuffer.ToInt32());
         //            if (chars >= 0) {
         //               byte[] localBuffer = new byte[BUFFER_SIZE];
         //               fixed (byte* pLocalBuffer = localBuffer) {
         //                  IntPtr ipLocalBuffer = new IntPtr(pLocalBuffer);
         //                  Int32 dwBytesRead = 0;
         //                  IntPtr ipBytesRead = new IntPtr(&dwBytesRead);
         //                  // Remote-Puffer in den lokalen Puffer einlesen
         //                  bool b4 = ReadProcessMemory(hProcess, ipRemoteBuffer, localBuffer, BUFFER_SIZE, out ipBytesRead);
         //                  //bool b4 = ReadProcessMemory(hProcess, ipRemoteBuffer, ipLocalBuffer, BUFFER_SIZE, out ipBytesRead);
         //                  if (b4) {
         //                     // Umwandlung in Text
         //                     text = Marshal.PtrToStringUni(ipLocalBuffer, chars);
         //                     if (text == " ")
         //                        text = String.Empty;
         //                  }
         //               }
         //            }
         //            VirtualFreeEx(hProcess, ipRemoteBuffer, 0, FreeType.Release);
         //         }
         //         CloseHandle(hProcess);
         //      }
         //   }
         //   return text;
         //}

         /// <summary>
         /// ermittelt den Text einer Statusbar
         /// </summary>
         /// <param name="hStatusBar"></param>
         /// <param name="pos">Position 0, ...</param>
         /// <returns></returns>
         public static string GetPanelText(IntPtr hStatusBar, int pos) {
            string text = "";
            // Programmfenster ermitteln
            IntPtr hwnd = Win32User.GetAncestor(hStatusBar, Win32User.GetAncestorFlags.GetRoot);
            // Prozess-ID ermitteln
            uint processId;
            uint threadId = Win32User.GetWindowThreadProcessId(hwnd, out processId);
            if (processId > 0) {
               // Prozess öffnen
               IntPtr hProcess = Win32Kernel.OpenProcess(Win32Kernel.ProcessAccessFlags.All, false, processId);
               if (hProcess != IntPtr.Zero) {
                  // Remote-Puffer anlegen
                  const int BUFFER_SIZE = 0x1000;
                  IntPtr ipRemoteBuffer = Win32Kernel.VirtualAllocEx(hProcess, IntPtr.Zero, new IntPtr(BUFFER_SIZE), Win32Kernel.AllocationType.COMMIT, Win32Kernel.MemoryProtection.READWRITE);
                  if (ipRemoteBuffer != IntPtr.Zero) {
                     // Textlänge und Text ermitteln
                     int chars = Win32User.SendMessage(hStatusBar, Win32User.SB_GETTEXT, pos, ipRemoteBuffer.ToInt32()).ToInt32();
                     if (chars >= 0) {
                        byte[] localBuffer = new byte[BUFFER_SIZE];
                        IntPtr ipBytesRead = new IntPtr(0);
                        // Remote-Puffer in den lokalen Puffer einlesen
                        if (Win32Kernel.ReadProcessMemory(hProcess, ipRemoteBuffer, localBuffer, BUFFER_SIZE, out ipBytesRead)) {
                           // Umwandlung in Text
                           text = System.Text.Encoding.Unicode.GetString(localBuffer).Substring(0, chars);
                           if (text == " ")
                              text = String.Empty;
                        }
                     }
                     Win32Kernel.VirtualFreeEx(hProcess, ipRemoteBuffer, 0, Win32Kernel.FreeType.Release);
                  }
                  Win32Kernel.CloseHandle(hProcess);
               }
            }
            return text;
         }

      }


      string[] ProgArgs;


      public Form1(string[] args) {
         InitializeComponent();
         ProgArgs = args;
      }

      private void Form1_Shown(object sender, EventArgs e) {
         string[] args = ProgArgs;

         Point startpt = new Point(0, 0);
         Point endpt = new Point(0, 0);
         int startnox = 0;
         int startnoy = 0;
         int pointsx = 64;
         int pointsy = 64;
         int pointdelay = 5;
         string outfile = "";
         int startdelay = 2000;
         string keys4init = "%AK77777{ENTER}%AzN54.53305 E13.41344{ENTER}";
         string childprog = "%ProgramFiles(x86)%/Garmin/MapSource.exe";
         List<string> childargs = new List<string>();

         if (args != null && args.Length > 0) {
            int arg = 0;

            if (arg < args.Length)
               startpt.X = Convert.ToInt32(args[arg++]);
            if (arg < args.Length)
               startpt.Y = Convert.ToInt32(args[arg++]);

            if (arg < args.Length)
               endpt.X = Convert.ToInt32(args[arg++]);
            if (arg < args.Length)
               endpt.Y = Convert.ToInt32(args[arg++]);

            // Punkt links-oben eperimentell ermitteln!
            //startpt = new Point(120, 117);
            //startpt = new Point(120, 428);
            //startpt = new Point(120, 471);
            // Punkt rechts-unten eperimentell ermitteln!
            //endpt = new Point(1740, 1089);
            //endpt = new Point(197, 559);

            if (arg < args.Length)
               startnox = Convert.ToInt32(args[arg++]);
            if (arg < args.Length)
               startnoy = Convert.ToInt32(args[arg++]);

            if (arg < args.Length)
               pointsx = Convert.ToInt32(args[arg++]);
            if (arg < args.Length)
               pointsy = Convert.ToInt32(args[arg++]);

            if (arg < args.Length)
               pointdelay = Convert.ToInt32(args[arg++]);

            // Ausgabedatei
            if (arg < args.Length)
               outfile = args[arg++];

            if (arg < args.Length)
               startdelay = Convert.ToInt32(args[arg++]);

            // Key-Initfolge für Prog
            if (arg < args.Length)
               keys4init = args[arg++];

            // Prog
            if (arg < args.Length)
               childprog = args[arg++];

            // Parameter für Prog
            for (; arg < args.Length; arg++)
               childargs.Add(args[arg++]);
         }
         DoIt(startpt, endpt, startnox, startnoy, pointsx, pointsy, pointdelay, outfile, startdelay, keys4init, childprog, string.Join(" ", childargs).Trim());
      }

      void StartProg(string prog, string args, int delay) {
         ProcessStartInfo startInfo = new ProcessStartInfo(Environment.ExpandEnvironmentVariables(prog), args);
         startInfo.WindowStyle = ProcessWindowStyle.Maximized;

         Process p = Process.Start(startInfo);
         p.WaitForInputIdle();
         // zusätzliche Zeit für das Laden der Datei usw.
         if (delay > 0)
            Helper.Wait(delay);
      }

      void DoIt(Point startpt, Point endpt,
                int startnox, int startnoy,
                int pointsx, int pointsy,
                int pointdelay,
                string outfile,
                int startdelay,
                string keys4init,
                string childprog, string childargs) {

         //WindowState = FormWindowState.Minimized;

         StartProg(childprog, childargs, startdelay);

         if (childargs.Length == 0)
            childargs = "Unbenannt";
         else
            childargs = childargs.Substring(0, childargs.Length - 4);      // Dateiname ohne '.gpx'

         ApiWindow progwin = new ApiWindow("{7A96B96B-E756-4e42-8274-54CBF24F7944}",
                                           childargs + " - MapSource");
         if (progwin == null || progwin.hWnd.ToInt32() == 0) {
            MessageBox.Show("Programm nicht gefunden!", "Fehler");
            Close();
            return;
         }

         ChildWindows childwin = new ChildWindows(progwin.hWnd, "msctls_statusbar32");
         ApiWindow statuswin = childwin.WindowList.Count > 0 ? childwin.WindowList[0] : null;
         if (statuswin == null) {
            MessageBox.Show("Statusbar nicht gefunden!", "Fehler");
            Close();
            return;
         }

         childwin = new ChildWindows(progwin.hWnd, "GarminMapWindow");
         ApiWindow clientwin = null;
         foreach (ApiWindow win in childwin.WindowList)
            if (win.GetText().Trim() == "") {
               clientwin = win;
               break;
            }

         progwin.SetForeground();
         Helper.Wait4Idle();

         // Regex zum Höhe ermitteln: z.B. "N54.53305 E13.41344, 17 ft"
         Regex r = new Regex(@"N\d+\.\d+\s+E\d+\.\d+,\s+(\d+)");

         StreamWriter wr = null;
         if (outfile.Length > 0) {
            try {
               if (File.Exists(outfile))
                  File.Delete(outfile);
               wr = new StreamWriter(outfile);
            } catch (Exception ex) {
               MessageBox.Show(string.Format("Fehler beim Erzeugen der Datei '{0}'!", outfile) + System.Environment.NewLine + ex.Message, "Fehler");
               Close();
            }
         }

         if (keys4init.Length > 0) {
            Helper.SendKeys(keys4init);
            Helper.Wait4Idle();
         }

         Helper.MousePosition = startpt;
         Helper.Wait(500);

         if (outfile.Length > 0)
            wr.WriteLine(outfile);

         // Spaltenüberschriften
         for (int stepx = 0; stepx < pointsx; stepx++)
            if (wr != null) {
               wr.Write("\t");
               wr.Write(startnox + stepx);
            } else {
               Debug.Write("\t");
               Debug.Write(startnox + stepx);
            }
         if (wr != null)
            wr.WriteLine();
         else
            Debug.WriteLine("");

         for (int stepy = 0; stepy < pointsy; stepy++) {
            for (int stepx = 0; stepx < pointsx; stepx++) {

               // Maus-Position setzen (Rundung halbiert den max. möglichen Fehler)
               Point newpos = new Point(startpt.X + (pointsx <= 1 ? 0 : (int)Math.Round(((double)stepx * (endpt.X - startpt.X)) / (pointsx - 1))),
                                        startpt.Y + (pointsy <= 1 ? 0 : (int)Math.Round(((double)stepy * (endpt.Y - startpt.Y)) / (pointsy - 1))));
               Helper.MousePosition = newpos;
               //Helper.MouseLeftClick();
               Helper.Wait4Idle(progwin);
               //Helper.Wait(1);
               Helper.WaitExt(pointdelay);

               // Höhe ermitteln: z.B. "N54.53305 E13.41344, 17 ft"
               string txt = Helper.GetPanelText(statuswin.hWnd, 2);        // 3. Panel
               Match m = r.Match(txt);

               int h = m.Success ? Convert.ToInt32(m.Groups[1].Value) : 0;
               string v = m.Success ? (h > 0 ? h.ToString() : ".") : "-";

               if (wr != null) {
                  if (stepx == 0)
                     wr.Write(startnoy + stepy);
                  wr.Write("\t");

                  //wr.Write(newpos.X);
                  //wr.Write(";");

                  wr.Write(v);
               } else {
                  if (stepx == 0)
                     Debug.Write(startnoy + stepy);
                  Debug.Write("\t");
                  Debug.Write(v);
               }

               //if (v == "-") {
               //   TopMost = true;
               //   Focus();
               //   MessageBox.Show("Höhe im Paneltext nicht auswertbar:" + System.Environment.NewLine + System.Environment.NewLine +
               //                   "[" + txt + "]",
               //                   "FEHLER",
               //                   MessageBoxButtons.OK,
               //                   MessageBoxIcon.Stop);
               //   TopMost = false;
               //   progwin.SetForeground();

               //   stepx = pointsx;
               //   stepy = pointsy;
               //   if (wr != null) {
               //      wr.Close();
               //      wr = null;
               //      if (File.Exists(outfile))
               //         File.Delete(outfile);
               //   }
               //}

            }
            if (wr != null)
               wr.WriteLine();
            else
               Debug.WriteLine("");
         }

         if (wr != null)
            wr.Close();

         // Programm beenden
         Helper.SendKeys("%{F4}");

         Close();
      }


   }

}
