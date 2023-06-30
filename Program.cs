using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
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

    public record WindowInfo(IntPtr Handle, string Title, string? WindowClass);

    public record WindowInfoReport(uint Handle, string Title, string? WindowClass)
    {
        public WindowInfoReport(WindowInfo info) : this((uint)info.Handle, info.Title, info.WindowClass) { }
    };

    public static List<WindowInfo> EnumWindowsWrapper()
    {
        var windowHandles = new List<WindowInfo>();
        EnumWindows( (IntPtr hWnd, IntPtr lParam) => 
        {
            if (IsWindowVisible(hWnd))
            {
                var sb = new StringBuilder(1024);
                GetWindowText(hWnd, sb, sb.Capacity);
                var title = sb.ToString();
                if (title.Length > 0) 
                {
                    windowHandles.Add( new WindowInfo(hWnd, title, GetWindowClassName(hWnd)));
                }
            }
            return true;
        }, IntPtr.Zero);
        return windowHandles;
    }
}

static class Program
{
    static int Main(string[] args)
    {
        var json = !args.Contains("--text");
        var refreshChrome = args.Contains("--refresh-chrome");

        var windows = Windows.EnumWindowsWrapper().OrderBy(x => x.Handle).ThenBy(x => x.Title).ToList();

        if (refreshChrome)
        {
            var curr = Windows.GetForegroundWindow();
            foreach (var handle in windows)
            {
                Console.WriteLine($"{handle.Handle,12} {Windows.GetWindowClassName(handle.Handle)}:{handle.Title}");
                if (args.Length > 0 && args.Contains("--chrome-refresh"))
                {
                    if (handle.Title.EndsWith("Google Chrome"))
                    {
                        Windows.SendKeys(handle.Handle, 0x74); //  https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
                        Console.WriteLine(" --> send: F5");
                    }
                }
            }

            Console.WriteLine("** Done. Returning...");
            Thread.Sleep(200);
            Windows.SetForegroundWindow(curr);
            return 0;
        }

        if (json)
        {
            Console.WriteLine(JsonSerializer.Serialize(windows.Select(x=>new Windows.WindowInfoReport(x)), new JsonSerializerOptions()
            {
                WriteIndented = true
            }));
            
            return 0;
        }
        else
        {
            foreach (var handle in windows)
            {
                Console.WriteLine($"{handle.Handle,12} {Windows.GetWindowClassName(handle.Handle)}:{handle.Title}");
            }

            return 0;

        }

    }
}