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
        Entry.EntryChanged += OnEntryChanged;
        Entry.AudioChanged += OnAudioChanged;
        Entry.JacketChanged += OnJacketChanged;
        Entry.ChartEndChanged += OnChartEndChanged;
        
        ChartChanged += OnChartChanged;
        
        AudioSystem.AudioLoaded += OnAudioLoaded;
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        
        OnChartChanged(null, EventArgs.Empty);
    }

    private static void OnEntryChanged(object? sender, EventArgs e) => EntryChanged?.Invoke(sender, e);
    private static void OnAudioChanged(object? sender, EventArgs e) => AudioChanged?.Invoke(sender, e);
    private static void OnJacketChanged(object? sender, EventArgs e) => JacketChanged?.Invoke(sender, e);

    private static void OnChartChanged(object? sender, EventArgs e) => Chart.Build(Entry, (float?)AudioSystem.AudioChannelAudio?.Length ?? 0, SettingsSystem.RenderSettings.SaturnJudgeAreas);
    private static void OnAudioLoaded(object? sender, EventArgs e) => Chart.Build(Entry, (float?)AudioSystem.AudioChannelAudio?.Length ?? 0, SettingsSystem.RenderSettings.SaturnJudgeAreas);
    private static void OnChartEndChanged(object? sender, EventArgs e) => Chart.Build(Entry, (float?)AudioSystem.AudioChannelAudio?.Length ?? 0, SettingsSystem.RenderSettings.SaturnJudgeAreas);

    private static void OnSettingsChanged(object? sender, EventArgs e)
    {
        if (SettingsSystem.RenderSettings.SaturnJudgeAreas != saturnJudgeAreas)
        {
            saturnJudgeAreas = SettingsSystem.RenderSettings.SaturnJudgeAreas;
            Chart.Build(Entry, (float?)AudioSystem.AudioChannelAudio?.Length ?? 0, SettingsSystem.RenderSettings.SaturnJudgeAreas);
        }
    }
    
    public static event EventHandler? ChartChanged;
    public static event EventHandler? EntryChanged;
    public static event EventHandler? JacketChanged;
    public static event EventHandler? AudioChanged;
    
    public static Chart Chart { get; private set; } = new() { Events = [ new TempoChangeEvent(Timestamp.Zero, 120), new MetreChangeEvent(Timestamp.Zero, 4, 4) ] };
    public static Entry Entry { get; private set; } = new();

    private static bool saturnJudgeAreas;
    
    /// <summary>
    /// Determines if the editor will prompt the user to save when a chart is closed.
    /// </summary>
    public static bool IsSaved { get; private set; } = true;

    /// <summary>
    /// Creates a new chart to work on by resetting the <see cref="Chart"/> and <see cref="Entry"/> objects, then invokes <see cref="ChartChanged"/> and <see cref="EntryChanged"/>
    /// </summary>
    public static void NewChart()
    {
        Entry.EntryChanged -= OnEntryChanged;
        Entry.AudioChanged -= OnAudioChanged;
        Entry.JacketChanged -= OnJacketChanged;
        Entry.ChartEndChanged -= OnChartEndChanged;
        
        Chart = new();
        Entry = new();
        
        Entry.EntryChanged += OnEntryChanged;
        Entry.AudioChanged += OnAudioChanged;
        Entry.JacketChanged += OnJacketChanged;
        Entry.ChartEndChanged += OnChartEndChanged;
        
        ChartChanged?.Invoke(null, EventArgs.Empty);
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
        Entry.EntryChanged -= OnEntryChanged;
        Entry.AudioChanged -= OnAudioChanged;
        Entry.JacketChanged -= OnJacketChanged;
        Entry.ChartEndChanged -= OnChartEndChanged;

        Entry = NotationSerializer.ToEntry(path, args, out _);
        Chart = NotationSerializer.ToChart(path, args, out _);
        
        Entry.EntryChanged += OnEntryChanged;
        Entry.AudioChanged += OnAudioChanged;
        Entry.JacketChanged += OnJacketChanged;
        Entry.ChartEndChanged += OnChartEndChanged;
        
        ChartChanged?.Invoke(null, EventArgs.Empty);
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
            Entry.EntryChanged -= OnEntryChanged;
            Entry.AudioChanged -= OnAudioChanged;
            Entry.JacketChanged -= OnJacketChanged;
            Entry.ChartEndChanged -= OnChartEndChanged;

            Entry = entry;
            Chart = chart;

            Entry.RootDirectory = rootDirectory;
            
            Entry.EntryChanged += OnEntryChanged;
            Entry.AudioChanged += OnAudioChanged;
            Entry.JacketChanged += OnJacketChanged;
            Entry.ChartEndChanged += OnChartEndChanged;
        
            ChartChanged?.Invoke(null, EventArgs.Empty);
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
    public static void WriteChart(string path, NotationWriteArgs args, bool markAsSaved, bool updatePath)
    {
        NotationSerializer.ToFile(path, Entry, Chart, args);

        // TODO: DONT SILENTLY FAIL WHEN A WRITE DOESNT COMPLETE.
        
        if (updatePath)
        {
            Entry.RootDirectory = Path.GetDirectoryName(path) ?? "";
            Entry.ChartFile = Path.GetFileName(path);
        }
        
        IsSaved = markAsSaved || IsSaved;
    }
}