using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaturnData.Notation;
using SaturnData.Notation.Core;
using SaturnData.Notation.Serialization;

namespace SaturnEdit.Systems;

public static class ChartSystem
{
    static ChartSystem()
    {
        Entry.EntryChanged += OnEntryChanged;
        ChartChanged += OnChartChanged;
    }

    private static void OnEntryChanged(object? sender, EventArgs e) => EntryChanged?.Invoke(sender, e);
    
    private static void OnChartChanged(object? sender, EventArgs e)
    {
        NotationUtils.CalculateTime(Entry, Chart);
        NotationUtils.CalculateScaledTime(Chart);
    }
    
    public static event EventHandler? ChartChanged;
    public static event EventHandler? EntryChanged;
    
    public static Chart Chart { get; private set; } = new();
    public static Entry Entry { get; private set; } = new();

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
        
        Chart = new();
        Entry = new();
        
        Entry.EntryChanged += OnEntryChanged;
        
        ChartChanged?.Invoke(null, EventArgs.Empty);
        EntryChanged?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Creates a new chart to work on by reading data from a file, then invokes <see cref="ChartChanged"/> and <see cref="EntryChanged"/>
    /// </summary>
    /// <param name="path">Path to the file to read from.</param>
    /// <param name="args">Arguments for how the chart should be read.</param>
    public static void ReadChart(string path, NotationReadArgs args)
    {
        Entry.EntryChanged -= OnEntryChanged;

        Entry = NotationSerializer.ToEntry(path, args, out _);
        Chart = NotationSerializer.ToChart(path, args, out _);
        
        Entry.EntryChanged += OnEntryChanged;
        
        ChartChanged?.Invoke(null, EventArgs.Empty);
        EntryChanged?.Invoke(null, EventArgs.Empty);
    }
    
    /// <summary>
    /// Updates a chart to 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="args"></param>
    /// <param name="exceptions"></param>
    public static void ReadChartEditorTxt(string text, NotationReadArgs args, out List<Exception> exceptions)
    {
        string[] data = text.Split('\n');

        Entry entry = NotationSerializer.ToEntry(data, args, out List<Exception> entryExceptions);
        Chart chart = NotationSerializer.ToChart(data, args, out List<Exception> chartExceptions);
        exceptions = entryExceptions.Concat(chartExceptions).ToList();
        
        Entry.EntryChanged -= OnEntryChanged;
        
        if (exceptions.Count == 0)
        {
            Entry = entry;
            Chart = chart;
        }
        
        Entry.EntryChanged += OnEntryChanged;
        
        if (exceptions.Count == 0)
        {
            ChartChanged?.Invoke(null, EventArgs.Empty);
            EntryChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Writes a chart to a file.
    /// </summary>
    /// <param name="path">Path to the file to write to.</param>
    /// <param name="args">Arguments for how the chart should be written.</param>
    /// <param name="markAsSaved">Should the chart be marked as saved?</param>
    public static void WriteChart(string path, NotationWriteArgs args, bool markAsSaved, bool updatePath)
    {
        NotationSerializer.ToFile(path, Entry, Chart, args);

        if (updatePath)
        {
            Entry.ChartPath = path;
            Entry.JacketPath = Entry.JacketPath == "" ? "" : Path.Combine(Path.GetDirectoryName(Entry.ChartPath) ?? "", Path.GetFileName(Entry.JacketPath));
            Entry.AudioPath = Entry.AudioPath == "" ? "" : Path.Combine(Path.GetDirectoryName(Entry.ChartPath) ?? "", Path.GetFileName(Entry.AudioPath));
            Entry.VideoPath = Entry.VideoPath == "" ? "" : Path.Combine(Path.GetDirectoryName(Entry.ChartPath) ?? "", Path.GetFileName(Entry.VideoPath));
        }
        
        IsSaved = markAsSaved || IsSaved;
    }
}