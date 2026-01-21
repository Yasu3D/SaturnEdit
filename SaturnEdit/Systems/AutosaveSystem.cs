using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using SaturnEdit.Utilities;

namespace SaturnEdit.Systems;

public static class AutosaveSystem
{
    public static void Initialize()
    {
        Directory.CreateDirectory(AutosaveDirectory);

        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        UndoRedoSystem.ChartBranch.OperationHistoryChanged += ChartBranch_OnOperationHistoryChanged;
    }
    
    public static string AutosaveDirectory => Path.Combine(PersistentDataPathHelper.PersistentDataPath, "Autosave");
    private static string AutosavePath => Path.Combine(AutosaveDirectory, $"autosave_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}.sat");
    public static string LastSessionPath => Path.Combine(AutosaveDirectory, "last_session.sat");

    private static readonly Timer AutosaveTimer = new(AutosaveTimer_Tick, null, Timeout.Infinite, Timeout.Infinite);

    private static bool autosaved = false;

#region Methods
    private static void Autosave()
    {
        if (ChartSystem.IsSaved) return;
        if (autosaved) return;

        autosaved = true;

        ChartSystem.WriteChart(AutosavePath, new() { ExportWatermark = ChartSystem.ExportWatermarkTemplate }, false, false);

        List<string> files = Directory.EnumerateFiles(AutosaveDirectory, "*", SearchOption.TopDirectoryOnly)
            .Where(x =>
            {
                string filename = Path.GetFileName(x);

                return filename.StartsWith("autosave", StringComparison.OrdinalIgnoreCase) && filename.EndsWith(".sat", StringComparison.OrdinalIgnoreCase);
            })
            .ToList();
        
        if (files.Count < 100) return;

        List<string> orderedFiles = files.OrderBy(File.GetCreationTime).ToList();
        for (int i = 0; i < orderedFiles.Count - 100; i++)
        {
            File.Delete(orderedFiles[i]);
        }
    }
#endregion Methods
    
#region System Event Handlers
    private static void OnSettingsChanged(object? sender, EventArgs e)
    {
        if (SettingsSystem.EditorSettings.AutoSaveFrequency is EditorSettings.AutoSaveFrequencyOption.Always or EditorSettings.AutoSaveFrequencyOption.Never)
        {
            AutosaveTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        else
        {
            int minutes = SettingsSystem.EditorSettings.AutoSaveFrequency switch
            {
                EditorSettings.AutoSaveFrequencyOption.Minutes10 => 10,
                EditorSettings.AutoSaveFrequencyOption.Minutes5 => 5,
                EditorSettings.AutoSaveFrequencyOption.Minutes1 => 1,
                _ => 10,
            };

            AutosaveTimer.Change(TimeSpan.FromMinutes(minutes), TimeSpan.FromMinutes(minutes));
        }
    }
    
    private static void ChartBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        autosaved = false;

        if (SettingsSystem.EditorSettings.AutoSaveFrequency == EditorSettings.AutoSaveFrequencyOption.Always)
        {
            Autosave();
        }
    }

    public static void OnClosed(object? sender, EventArgs eventArgs)
    {
        ChartSystem.WriteChart(LastSessionPath, new() { ExportWatermark = ChartSystem.ExportWatermarkTemplate }, false, false);
    }
#endregion System Event Handlers

#region Internal Event Handlers
    private static void AutosaveTimer_Tick(object? state)
    {
        Autosave();
    }
#endregion Internal Event Handlers
}