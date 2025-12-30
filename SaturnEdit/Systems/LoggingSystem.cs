using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using SaturnEdit.Utilities;

namespace SaturnEdit.Systems;

public class LoggingSystem
{
    public static void Initialize()
    {
        Dispatcher.UIThread.UnhandledException += UIThreadOnUnhandledException;
        
        TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

        sessionLogFile = $"session-log_{DateTime.Now:yyyy-MM-dd-hh-mm-ss-fff}.txt";
    }

    private static string CrashLogDirectory => Path.Combine(PersistentDataPathHelper.PersistentDataPath, "CrashLogs");
    private static string CrashLogPath => Path.Combine(CrashLogDirectory, $"crash-log_{DateTime.Now:yyyy-MM-dd-hh-mm-ss-fff}.txt");

    private static string SessionLogDirectory => Path.Combine(PersistentDataPathHelper.PersistentDataPath, "SessionLogs");
    private static string SessionLogPath => Path.Combine(SessionLogDirectory, sessionLogFile);
    private static string sessionLogFile = "";

    private static readonly StringBuilder SessionLog = new();
    
#region Methods
    public static void WriteSessionLog(string message)
    {
        SessionLog.Append($"{DateTime.Now:yyyy-MM-dd-hh-mm-ss-fff}:\n");
        SessionLog.Append(message);
        SessionLog.Append("\n\n");

        Directory.CreateDirectory(SessionLogDirectory);
        File.WriteAllText(SessionLogPath, SessionLog.ToString());
        Console.WriteLine(message);
    }
    
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