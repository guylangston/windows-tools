using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;

public static class Windows
{
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")] public static extern bool   EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
    [DllImport("user32.dll")] public static extern int    GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);
    [DllImport("user32.dll")] public static extern bool   IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool   SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern int    SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName,int nMaxCount);
    
    [DllImport("user32.dll", SetLastError=true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);
    

    const uint WM_KEYDOWN = 0x0100;
    const uint WM_KEYUP = 0x0101;

    public static void SendKey(IntPtr windowHandle, int key)
    {
        SendMessage(windowHandle, WM_KEYDOWN, (int)key, 0);
        SendMessage(windowHandle, WM_KEYUP, (int)key, 0);
    }

    private static readonly object lockerSendKeys = new();
    public static void SendKeys(IntPtr windowHandle, int key)
    {
        lock(lockerSendKeys)
        {
            if (SetForegroundWindow(windowHandle))
            {
                SendKey(windowHandle, key);
            }
        }
    }

    public static string? GetWindowClassName(IntPtr hWin)
    {
        // Pre-allocate 256 characters, since this is the maximum class name length.
        var className = new StringBuilder(256);
        //Get the window class name
        if (GetClassName(hWin, className, className.Capacity) == 0) return null;
        return className.ToString();
    }

    public record WindowInfo([property: JsonIgnore]IntPtr HandleIntPtr, 
        string Title, 
        string? WindowClass,
        int ProcessId,
        [property: JsonIgnore]Process Process)
    {
        public uint Handle => (uint)HandleIntPtr;
        public string? ProcessName => Process?.ProcessName;
        public DateTime? Started => Process?.StartTime;
        
        // https://stackoverflow.com/questions/2633628/can-i-get-command-line-arguments-of-other-processes-from-net-c
        // NOTE: Cannot use StartInfo for an unrelated process (not started by this process)
        // public string? ProcessCmd => Process?.StartInfo.FileName;
    }

   

    public static List<WindowInfo> EnumWindowsWrapper()
    {
        var windowHandles = new List<WindowInfo>();
        EnumWindows( (hWnd, _) => 
        {
            if (IsWindowVisible(hWnd))
            {
                var sb = new StringBuilder(1024);
                if (GetWindowText(hWnd, sb, sb.Capacity) != 0)
                {
                    var title = sb.ToString();
                    if (title.Length > 0)
                    {
                        // Get the Window Handle's Process
                        Process? proc = null;
                        if (GetWindowThreadProcessId(hWnd, out var procId) != 0)
                        {
                            proc = Process.GetProcessById(procId);
                        }

                        windowHandles.Add( new WindowInfo(hWnd, title, GetWindowClassName(hWnd), procId, proc));
                    }
                }
            }
            return true;
        }, IntPtr.Zero);
        return windowHandles;
    }
}