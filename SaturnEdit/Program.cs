using Avalonia;
using System;
using System.IO;
using SaturnEdit.Utilities;

namespace SaturnEdit;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            WriteCrashLog(ex.ToString());
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
    
    private static void WriteCrashLog(string? log)
    {
        try
        {
            string logPath = Path.Combine(PersistentDataPathHelper.PersistentDataPath, "crash_log.txt");
            File.WriteAllText(logPath, log);
        }
        catch
        {
            // ignored.
        }
    }
}