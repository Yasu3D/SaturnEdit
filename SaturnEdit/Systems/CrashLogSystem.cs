using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using SaturnEdit.Utilities;

namespace SaturnEdit.Systems;

public class CrashLogSystem
{
    public static void Initialize()
    {
        Dispatcher.UIThread.UnhandledException += UIThreadOnUnhandledException;
        
        TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
    }

    private static string CrashLogDirectory => Path.Combine(PersistentDataPathHelper.PersistentDataPath, "CrashLogs");
    private static string CrashLogPath => Path.Combine(CrashLogDirectory, $"crash-log_{DateTime.Now:yyyy-MM-dd-hh-mm-ss-fff}.txt");

#region Methods
    public static void WriteCrashLog(string? log)
    {
        try
        {
            Directory.CreateDirectory(CrashLogDirectory);
            File.WriteAllText(CrashLogPath, log);
        }
        catch
        {
            // ignored.
        }
    }
#endregion Methods

#region Exception Event Handlers
    private static void UIThreadOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        WriteCrashLog(e.Exception.ToString());
    }

    private static void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        WriteCrashLog(e.Exception.ToString());
    }
#endregion Exception Event Handlers
}