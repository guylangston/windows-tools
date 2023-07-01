using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

static class Program
{
    static int Main(string[] args)
    {
        var json = !args.Contains("--text");
        var refreshChrome = args.Contains("--refresh-chrome");

        var windows = Windows.EnumWindowsWrapper()
            .OrderBy(x => x.WindowClass)
            .ThenBy(x=>x.Started)
            .ToList();

        if (refreshChrome)
        {
            var curr = Windows.GetForegroundWindow();
            foreach (var handle in windows)
            {
                Console.WriteLine($"{handle.HandleIntPtr,12} {Windows.GetWindowClassName(handle.HandleIntPtr)}:{handle.Title}");
                if (args.Length > 0 && args.Contains("--chrome-refresh"))
                {
                    if (handle.Title.EndsWith("Google Chrome"))
                    {
                        Windows.SendKeys(handle.HandleIntPtr, 0x74); //  https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
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
            Console.WriteLine(JsonSerializer.Serialize(windows, new JsonSerializerOptions()
            {
                WriteIndented = true
            }));
            
            return 0;
        }
        else
        {
            foreach (var handle in windows)
            {
                Console.WriteLine($"{handle.HandleIntPtr,12} {Windows.GetWindowClassName(handle.HandleIntPtr)}:{handle.Title}");
            }

            return 0;

        }

    }
}