using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaturnData.Notation.Core;
using SaturnData.Notation.Events;
using SaturnData.Notation.Serialization;

namespace SaturnEdit.Systems;

public static class ChartSystem
{
    public static void Initialize()
    {
        Entry.EntryChanged += OnInternalEntryChanged;
        Entry.AudioChanged += OnInternalAudioChanged;
        Entry.JacketChanged += OnInternalJacketChanged;
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        UndoRedoSystem.OperationHistoryChanged += OnOperationHistoryChanged;
        OnOperationHistoryChanged(null, EventArgs.Empty);

        IsSaved = true;
    }
    
    public static event EventHandler? ChartLoaded;
    public static event EventHandler? EntryChanged;
    public static event EventHandler? JacketChanged;
    public static event EventHandler? AudioChanged;
    
    /// <summary>
    /// The chart being edited/displayed.
    /// </summary>
    public static Chart Chart { get; private set; } = new()
    {
        Events = 
        [ 
            new TempoChangeEvent(Timestamp.Zero, 120), 
            new MetreChangeEvent(Timestamp.Zero, 4, 4),
        ],
        
        Layers = [ new("Main Layer") ],
    };
    
    /// <summary>
    /// The entry being edited/displayed.
    /// </summary>
    public static Entry Entry { get; private set; } = new();
    
    /// <summary>
    /// Determines if the editor will prompt the user to save when a chart is closed.
    /// </summary>
    public static bool IsSaved { get; private set; } = true;
    
    private static bool saturnJudgeAreas;
    
#region Methods
    /// <summary>
    /// Creates a new chart to work on by resetting the <see cref="Chart"/> and <see cref="Entry"/> objects, then invokes <see cref="ChartChanged"/> and <see cref="EntryChanged"/>
    /// </summary>
    public static void NewChart()
    {
        Entry.EntryChanged -= OnInternalEntryChanged;
        Entry.AudioChanged -= OnInternalAudioChanged;
        Entry.JacketChanged -= OnInternalJacketChanged;
        
        Chart = new();
        Entry = new();
        
        Entry.EntryChanged += OnInternalEntryChanged;
        Entry.AudioChanged += OnInternalAudioChanged;
        Entry.JacketChanged += OnInternalJacketChanged;
        
        ChartLoaded?.Invoke(null, EventArgs.Empty);
        EntryChanged?.Invoke(null, EventArgs.Empty);
        AudioChanged?.Invoke(null, EventArgs.Empty);
        JacketChanged?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Creates a new chart to work on by reading data from a file, then invokes <see cref="ChartChanged"/>, <see cref="EntryChanged"/>, <see cref="AudioChanged"/> and <see cref="JacketChanged"/>
    /// </summary>
    /// <param name="path">Path to the file to read from.</param>
    /// <param name="args">Arguments for how the chart should be read.</param>
    public static void ReadChart(string path, NotationReadArgs args)
    {
        Entry.EntryChanged -= OnInternalEntryChanged;
        Entry.AudioChanged -= OnInternalAudioChanged;
        Entry.JacketChanged -= OnInternalJacketChanged;

        Entry = NotationSerializer.ToEntry(path, args, out _);
        Chart = NotationSerializer.ToChart(path, args, out _);
        
        Entry.EntryChanged += OnInternalEntryChanged;
        Entry.AudioChanged += OnInternalAudioChanged;
        Entry.JacketChanged += OnInternalJacketChanged;
        
        ChartLoaded?.Invoke(null, EventArgs.Empty);
        EntryChanged?.Invoke(null, EventArgs.Empty);
        AudioChanged?.Invoke(null, EventArgs.Empty);
        JacketChanged?.Invoke(null, EventArgs.Empty);
    }
    
    /// <summary>
    /// Updates a chart to  work on by reading data from ChartViewTxt, then invokes <see cref="ChartChanged"/>, <see cref="EntryChanged"/>, <see cref="AudioChanged"/> and <see cref="JacketChanged"/>
    /// </summary>
    /// <param name="text"></param>
    /// <param name="args"></param>
    /// <param name="exceptions"></param>
    public static void ReadChartEditorTxt(string text, string rootDirectory, NotationReadArgs args, out List<Exception> exceptions)
    {
        string[] data = text.Split('\n');

        Entry entry = NotationSerializer.ToEntry(data, args, out List<Exception> entryExceptions);
        Chart chart = NotationSerializer.ToChart(data, args, out List<Exception> chartExceptions);
        exceptions = entryExceptions.Concat(chartExceptions).ToList();
        
        if (exceptions.Count == 0)
        {
            Entry.EntryChanged -= OnInternalEntryChanged;
            Entry.AudioChanged -= OnInternalAudioChanged;
            Entry.JacketChanged -= OnInternalJacketChanged;

            Entry = entry;
            Chart = chart;

            Entry.RootDirectory = rootDirectory;
            
            Entry.EntryChanged += OnInternalEntryChanged;
            Entry.AudioChanged += OnInternalAudioChanged;
            Entry.JacketChanged += OnInternalJacketChanged;
        
            ChartLoaded?.Invoke(null, EventArgs.Empty);
            EntryChanged?.Invoke(null, EventArgs.Empty);
            AudioChanged?.Invoke(null, EventArgs.Empty);
            JacketChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Writes a chart to a file.
    /// </summary>
    /// <param name="path">Path to the file to write to.</param>
    /// <param name="args">Arguments for how the chart should be written.</param>
    /// <param name="markAsSaved">Should the chart be marked as saved?</param>
    /// <param name="updatePath">Should the <c>RootDirectory</c> and <c>ChartFile</c> paths get updated?</param>
    public static bool WriteChart(string path, NotationWriteArgs args, bool markAsSaved, bool updatePath)
    {
        try
        {
            NotationSerializer.ToFile(path, Entry, Chart, args);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
            return false;
        }
        
        if (updatePath)
        {
            Entry.RootDirectory = Path.GetDirectoryName(path) ?? "";
            Entry.ChartFile = Path.GetFileName(path);
        }
        
        IsSaved = markAsSaved || IsSaved;
        return true;
    }
#endregion Methods
    
#region System Event Delegates
    private static void OnInternalEntryChanged(object? sender, EventArgs e) => EntryChanged?.Invoke(sender, e);
    private static void OnInternalAudioChanged(object? sender, EventArgs e) => AudioChanged?.Invoke(sender, e);
    private static void OnInternalJacketChanged(object? sender, EventArgs e) => JacketChanged?.Invoke(sender, e);
    
    private static void OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        Timestamp oldChartEnd = Entry.ChartEnd;
        
        Chart.Build(Entry, (float?)AudioSystem.AudioChannelAudio?.Length ?? 0, SettingsSystem.RenderSettings.SaturnJudgeAreas);
        
        if (Entry.AutoBpmMessage)
        {
            Entry.BpmMessage = Chart.GetAutoBpmMessage();
        }
        
        IsSaved = false;
    }

    private static void OnSettingsChanged(object? sender, EventArgs e)
    {
        if (SettingsSystem.RenderSettings.SaturnJudgeAreas != saturnJudgeAreas)
        {
            saturnJudgeAreas = SettingsSystem.RenderSettings.SaturnJudgeAreas;
            Chart.Build(Entry, (float?)AudioSystem.AudioChannelAudio?.Length ?? 0, SettingsSystem.RenderSettings.SaturnJudgeAreas);
        }
    }
#endregion System Event Delegates
}