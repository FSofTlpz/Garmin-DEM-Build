using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleProgControl {
   public class Win32User {

      public const int WM_GETTEXT = 0x0D;
      public const int WM_GETTEXTLENGTH = 0x0E;

      public const int SB_GETPARTS = 0x400 + 6;
      public const int SB_GETTEXT = 0x400 + 13;        // WM_USER+13


      //[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
      //public static extern int SendMessage(Int32 hWnd, int msg, int Param, StringBuilder text);

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

      [DllImport("user32.dll")]
      public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam, Int32 lParam);

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, StringBuilder lParam);

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam, StringBuilder lParam);

      //[DllImport("user32", EntryPoint = "SendMessageA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
      //public static extern Int32 SendMessage(Int32 hWnd, Int32 wMsg, Int32 wParam, Int32 lParam);

      //[DllImport("user32", EntryPoint = "SendMessageA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
      //public static extern Int32 SendMessage(Int32 hWnd, Int32 wMsg, Int32 wParam, StringBuilder lParam);

      // Activate an application window.
      [DllImport("USER32.DLL")]
      public static extern bool SetForegroundWindow(IntPtr hWnd);

      [DllImport("USER32.DLL", CharSet = CharSet.Unicode, SetLastError = true)]
      public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

      [DllImport("user32.dll", SetLastError = true)]
      public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

      public delegate int EnumCallBackDelegate(int hWnd, int lParam);

      [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
      public static extern int EnumWindows(EnumCallBackDelegate lpEnumFunc, int lParam);

      [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
      public static extern int EnumChildWindows(int hWndParent, EnumCallBackDelegate lpEnumFunc, int lParam);

      [DllImport("user32", EntryPoint = "GetClassNameA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
      public static extern int GetClassName(int hWnd, StringBuilder lpClassName, int nMaxCount);

      [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
      public static extern int IsWindowVisible(int hWnd);

      [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
      public static extern int GetParent(int hWnd);

      [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);

      public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

      [DllImport("user32.dll")]
      public static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

      /// <summary>
      /// Retrieves the handle to the ancestor of the specified window.
      /// </summary>
      /// <param name="hwnd">A handle to the window whose ancestor is to be retrieved.
      /// If this parameter is the desktop window, the function returns NULL. </param>
      /// <param name="flags">The ancestor to be retrieved.</param>
      /// <returns>The return value is the handle to the ancestor window.</returns>
      [DllImport("user32.dll", ExactSpelling = true)]
      public static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

      public enum GetAncestorFlags {
         /// <summary>
         /// Retrieves the parent window. This does not include the owner, as it does with the GetParent function.
         /// </summary>
         GetParent = 1,
         /// <summary>
         /// Retrieves the root window by walking the chain of parent windows.
         /// </summary>
         GetRoot = 2,
         /// <summary>
         /// Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent.
         /// </summary>
         GetRootOwner = 3
      }

      [DllImport("user32.dll", SetLastError = true)]
      public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

      // When you don't want the ProcessId, use this overload and pass IntPtr.Zero for the second parameter
      [DllImport("user32.dll")]
      public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);


      [DllImport("user32.dll", SetLastError = true)]
      public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

      [StructLayout(LayoutKind.Sequential)]
      public struct RECT {
         public int Left;        // x position of upper-left corner
         public int Top;         // y position of upper-left corner
         public int Right;       // x position of lower-right corner
         public int Bottom;      // y position of lower-right corner
      }

      [DllImport("user32.dll")]
      public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

      //user32 API import
      [DllImport("user32", EntryPoint = "mouse_event")]
      public static extern void mouse_event(UInt32 dwFlags, UInt32 dx, UInt32 dy, UInt32 dwData, IntPtr dwExtraInfo);
      [DllImport("user32.dll")]
      public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

      public const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
      public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
      public const uint MOUSEEVENTF_LEFTUP = 0x0004;
      public const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
      public const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
      public const uint MOUSEEVENTF_MOVE = 0x0001;
      public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
      public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
      public const uint MOUSEEVENTF_XDOWN = 0x0080;
      public const uint MOUSEEVENTF_XUP = 0x0100;
      public const uint MOUSEEVENTF_WHEEL = 0x0800;
      public const uint MOUSEEVENTF_HWHEEL = 0x01000;


      /// <summary>
      /// Sucht das Fenster, das sich unter den angegebenen Koordinatne befindet
      /// </summary>
      /// <param name="point">Die absoluten Bildschirmkoordinaten</param>
      /// <returns>Das Fenster-Handle oder <c>0</c>, falls kein Fenster gefunden wurde</returns>
      [System.Runtime.InteropServices.DllImport("user32")]
      public static extern IntPtr WindowFromPoint(Point point);

      [StructLayout(LayoutKind.Sequential)]
      public struct Point {
         public int X;
         public int Y;
      }

      /// <summary>
      /// Der Code der Windows Message, die gesendet wird, wenn die linke Maustaste gedrückt wird
      /// </summary>
      public const uint WM_LBUTTONDOWN = 0x0201;

      /// <summary>
      /// Der Code der Windows Message, die gesendet wird, wenn die linke Maustaste losgelassen wird
      /// </summary>
      public const uint WM_LBUTTONUP = 0x0202;

      // P/Invoke function for controlling the mouse
      [DllImport("user32.dll", SetLastError = true)]
      public static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

      /// <summary>
      /// structure for mouse data
      /// </summary>
      public struct MouseInput {
         public int X; // X coordinate
         public int Y; // Y coordinate
         public uint MouseData; // mouse data, e.g. for mouse wheel
         public uint DwFlags; // further mouse data, e.g. for mouse buttons
         public uint Time; // time of the event
         public IntPtr DwExtraInfo; // further information
      }

      /// <summary>
      /// super structure for input data of the function SendInput
      /// </summary>
      public struct Input {
         public int Type; // type of the input, 0 for mouse  
         public MouseInput Data; // mouse data
      }

      [DllImport("user32.dll")]
      public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

      [DllImport("user32.dll")]
      public static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

   }
}
