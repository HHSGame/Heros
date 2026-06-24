using Avalonia;
using System;

namespace HHSEditor;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Console.WriteLine("HHS Game Editor starting...");
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
