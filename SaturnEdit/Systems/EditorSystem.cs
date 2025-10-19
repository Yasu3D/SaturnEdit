using System;
using System.Collections.Generic;
using SaturnData.Notation.Core;
using SaturnData.Notation.Interfaces;

namespace SaturnEdit.Systems;

public static class EditorSystem
{
    public static void Initialize()
    {
        TimeSystem.PlaybackStateChanged += OnPlaybackStateChanged;
    }

    private static void OnPlaybackStateChanged(object? sender, EventArgs e)
    {
        PointerOverObject = null;
    }

    public static ITimeable? PointerOverObject { get; set; } = null;
    public static ITimeable? LastSelectedObject { get; set; } = null;
    public static HashSet<ITimeable> SelectedObjects { get; } = [];
    public static BoxSelectData BoxSelectData { get; set; } = new();
}

public class BoxSelectData()
{
    public readonly Dictionary<Layer, float> StartTimes = [];
    public readonly Dictionary<Layer, float> EndTimes = [];
    public int? StartLane = null;
    public int? EndLane = null;
    public bool PassedThroughSeam = false;
}