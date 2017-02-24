//"c:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools\spyxx.exe"
// "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\Common7\Tools\spyxx.exe"

/*
Fenster: "Marker - Mapsource"
         --> "msctls_statusbar32"
             "GarminMapWindow"

*/


using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

namespace SimulateKeyPress
{
    class Form1 : Form
    {
        private Button button1 = new Button();

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new Form1());
        }

        public Form1()
        {
            button1.Location = new Point(10, 10);
            button1.TabIndex = 0;
            button1.Text = "Click to automate Calculator";
            button1.AutoSize = true;
            button1.Click += new EventHandler(button1_Click);

            this.DoubleClick += new EventHandler(Form1_DoubleClick);
            this.Controls.Add(button1);
        }

        // Get a handle to an application window.
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        // Activate an application window.
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        // Send a series of key presses to the Calculator application.
        private void button1_Click(object sender, EventArgs e)
        {
            // Get a handle to the Calculator application. The window class
            // and window name were obtained using the Spy++ tool.
            IntPtr calculatorHandle = FindWindow("CalcFrame","Calculator");

            // alternativ:
            // Process p = Process.GetProcessesByName("notepad").FirstOrDefault();
            // if( p != null)
               // IntPtr h = p.MainWindowHandle;

            // alternativ mit Programmstart:
            // Process p = Process.Start("notepad.exe");
            // p.WaitForInputIdle();
            // IntPtr h = p.MainWindowHandle;            
            
            
            // Verify that Calculator is a running process.
            if (calculatorHandle == IntPtr.Zero)
            {
                MessageBox.Show("Calculator is not running.");
                return;
            }

            // Make Calculator the foreground application and send it 
            // a set of calculations.
            SetForegroundWindow(calculatorHandle);
            SendKeys.SendWait("111");
            SendKeys.SendWait("*");
            SendKeys.SendWait("11");
            SendKeys.SendWait("=");
        }

        // Send a key to the button when the user double-clicks anywhere 
        // on the form.
        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            // Send the enter key to the button, which raises the click 
            // event for the button. This works because the tab stop of 
            // the button is 0.
            SendKeys.Send("{ENTER}");
        }
    }
}





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            List<WinText> windows = new List<WinText>();

            //find the "first" window
            IntPtr hWnd = FindWindow("notepad", null);

            while (hWnd != IntPtr.Zero)
            {
                //find the control window that has the text
                IntPtr hEdit = FindWindowEx(hWnd, IntPtr.Zero, "edit", null);

                //initialize the buffer.  using a StringBuilder here
                System.Text.StringBuilder sb = new System.Text.StringBuilder(255);  // or length from call with GETTEXTLENGTH

                //get the text from the child control
                int RetVal = SendMessage(hEdit, WM_GETTEXT, sb.Capacity, sb);

                windows.Add(new WinText() { hWnd = hWnd, Text = sb.ToString() });

                //find the next window
                hWnd = FindWindowEx(IntPtr.Zero, hWnd, "notepad", null);
            }

            //do something clever
            windows.OrderBy(x => x.Text).ToList().ForEach(y => Console.Write("{0} = {1}\n", y.hWnd, y.Text));

            Console.Write("\n\nFound {0} window(s).", windows.Count);
            Console.ReadKey();
        }

        private struct WinText
        {
            public IntPtr hWnd;
            public string Text;
        }

        const int WM_GETTEXT = 0x0D;
        const int WM_GETTEXTLENGTH = 0x0E;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int Param, System.Text.StringBuilder text);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

    }
}






delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

[DllImport("user32.dll")]
static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
{
    var handles = new List<IntPtr>();

    foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
        EnumThreadWindows(thread.Id, (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);

    return handles;
}

private const uint WM_GETTEXT = 0x000D;

[DllImport("user32.dll", CharSet = CharSet.Auto)]
static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);

[STAThread]
static void Main(string[] args)
{
    foreach (var handle in EnumerateProcessWindowHandles(Process.GetProcessesByName("explorer").First().Id))
    {
        StringBuilder message = new StringBuilder(1000);
        SendMessage(handle, WM_GETTEXT, message.Capacity, message);
        Console.WriteLine(message);
    }
}




[DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
public static extern bool EnumWindows(EnumThreadWindowsCallback callback, IntPtr extraData);

[DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
public static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);





using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Enumerate top-level and child windows
/// </summary>
/// <example>
/// WindowsEnumerator enumerator = new WindowsEnumerator(); 
/// foreach (ApiWindow top in enumerator.GetTopLevelWindows()) 
/// { 
///    Console.WriteLine(top.MainWindowTitle); 
///        foreach (ApiWindow child in enumerator.GetChildWindows(top.hWnd))  
///            Console.WriteLine(" " + child.MainWindowTitle); 
/// } 
/// </example>
public class WindowsEnumerator
{

    private delegate int EnumCallBackDelegate(int hWnd, int lParam);

    [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    private static extern int EnumWindows(EnumCallBackDelegate lpEnumFunc, int lParam);

    [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    private static extern int EnumChildWindows(int hWndParent, EnumCallBackDelegate lpEnumFunc, int lParam);

    [DllImport("user32", EntryPoint = "GetClassNameA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    private static extern int GetClassName(int hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    private static extern int IsWindowVisible(int hWnd);

    [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    private static extern int GetParent(int hWnd);

    [DllImport("user32", EntryPoint = "SendMessageA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    private static extern Int32 SendMessage(Int32 hWnd, Int32 wMsg, Int32 wParam, Int32 lParam);

    [DllImport("user32", EntryPoint = "SendMessageA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    private static extern Int32 SendMessage(Int32 hWnd, Int32 wMsg, Int32 wParam, StringBuilder lParam);


    // Top-level windows.
    // Child windows.
    // Get the window class.
    // Test if the window is visible--only get visible ones.
    // Test if the window's parent--only get the one's without parents.
    // Get window text length signature.
    // Get window text signature.

    private List<ApiWindow> _listChildren = new List<ApiWindow>();
    private List<ApiWindow> _listTopLevel = new List<ApiWindow>();

    private string _topLevelClass = "";
    private string _childClass = "";

    /// <summary>
    /// Get all top-level window information
    /// </summary>
    /// <returns>List of window information objects</returns>
    public List<ApiWindow> GetTopLevelWindows()
    {
        EnumWindows(EnumWindowProc, 0);
        return _listTopLevel;
    }

    public List<ApiWindow> GetTopLevelWindows(string className)
    {
        _topLevelClass = className;
        return this.GetTopLevelWindows();
    }

    /// <summary>
    /// Get all child windows for the specific windows handle (hwnd).
    /// </summary>
    /// <returns>List of child windows for parent window</returns>
    public List<ApiWindow> GetChildWindows(Int32 hwnd)
    {
        // Clear the window list.
        _listChildren = new List<ApiWindow>();
        // Start the enumeration process.
        EnumChildWindows(hwnd, EnumChildWindowProc, 0);
        // Return the children list when the process is completed.
        return _listChildren;
    }

    public List<ApiWindow> GetChildWindows(Int32 hwnd, string childClass)
    {
        // Set the search
        _childClass = childClass;
        return this.GetChildWindows(hwnd);
    }

    /// <summary>
    /// Callback function that does the work of enumerating top-level windows.
    /// </summary>
    /// <param name="hwnd">Discovered Window handle</param>
    /// <returns>1=keep going, 0=stop</returns>
    private Int32 EnumWindowProc(Int32 hwnd, Int32 lParam)
    {
        // Eliminate windows that are not top-level.
        if (GetParent(hwnd) == 0 && Convert.ToBoolean(IsWindowVisible(hwnd)))
        {
            // Get the window title / class name.
            ApiWindow window = GetWindowIdentification(hwnd);
            // Match the class name if searching for a specific window class.
            if (_topLevelClass.Length == 0 || window.ClassName.ToLower() == _topLevelClass.ToLower())
                _listTopLevel.Add(window);
        }

        // To continue enumeration, return True (1), and to stop enumeration 
        // return False (0).
        // When 1 is returned, enumeration continues until there are no 
        // more windows left.

        return 1;

    }

    /// <summary>
    /// Callback function that does the work of enumerating child windows.
    /// </summary>
    /// <param name="hwnd">Discovered Window handle</param>
    /// <returns>1=keep going, 0=stop</returns>
    private Int32 EnumChildWindowProc(Int32 hwnd, Int32 lParam)
    {
        ApiWindow window = GetWindowIdentification(hwnd);
        // Attempt to match the child class, if one was specified, otherwise
        // enumerate all the child windows.
        if (_childClass.Length == 0 || window.ClassName.ToLower() == _childClass.ToLower())
            _listChildren.Add(window);
        return 1;
    }

    /// <summary>
    /// Build the ApiWindow object to hold information about the Window object.
    /// </summary>
    private ApiWindow GetWindowIdentification(int hwnd)
    {
        const Int32 WM_GETTEXT = 13;
        const Int32 WM_GETTEXTLENGTH = 14;

        ApiWindow window = new ApiWindow();
        StringBuilder title = new StringBuilder();
        // Get the size of the string required to hold the window title.
        Int32 size = SendMessage(hwnd, WM_GETTEXTLENGTH, 0, 0);

        // If the return is 0, there is no title.
        if (size > 0)
        {
            title = new StringBuilder(size + 1);
            SendMessage(hwnd, WM_GETTEXT, title.Capacity, title);
        }

        // Get the class name for the window.
        StringBuilder classBuilder = new StringBuilder(64);
        GetClassName(hwnd, classBuilder, 64);

        // Set the properties for the ApiWindow object.
        window.ClassName = classBuilder.ToString();
        window.MainWindowTitle = title.ToString();
        window.hWnd = hwnd;

        return window;
    }

}

public class ApiWindow
{
    public string MainWindowTitle = "";
    public string ClassName = "";
    public int hWnd;
}