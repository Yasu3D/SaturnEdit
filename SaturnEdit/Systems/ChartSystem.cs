using System;
using SaturnData.Notation.Core;
using SaturnData.Notation.Serialization;

namespace SaturnEdit.Systems;

public static class ChartSystem
{
    static ChartSystem()
    {
        Entry.EntryChanged += OnEntryChanged;
    }

    private static void OnEntryChanged(object? sender, EventArgs e) => EntryChanged?.Invoke(sender, e);
    
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

        Entry = NotationSerializer.ToEntry(path, args);
        Chart = NotationSerializer.ToChart(path, args);
        
        Entry.EntryChanged += OnEntryChanged;
        
        ChartChanged?.Invoke(null, EventArgs.Empty);
        EntryChanged?.Invoke(null, EventArgs.Empty);
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
        Entry.ChartPath = updatePath ? path : Entry.ChartPath;
        IsSaved = markAsSaved || IsSaved;
    }
}