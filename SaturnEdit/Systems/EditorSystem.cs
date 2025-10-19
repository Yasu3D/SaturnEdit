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
    public float? GlobalStartTime = null;
    public float? GlobalEndTime = null;
    public readonly Dictionary<Layer, float> ScaledStartTimes = [];
    public readonly Dictionary<Layer, float> ScaledEndTimes = [];
    public int? StartLane = null;
    public int? EndLane = null;
    public bool PassedThroughSeam = false;
    public bool NegativeSelection = false;

    public int? Position
    {
        get
        {
            if (StartLane == null) return null;
            if (EndLane == null) return null;
            
            return PassedThroughSeam
                ? Math.Max(StartLane.Value, EndLane.Value)
                : Math.Min(StartLane.Value, EndLane.Value);
        }
    }

    public int? Size
    {
        get
        {
            if (StartLane == null) return null;
            if (EndLane == null) return null;
            
            int size = StartLane < EndLane
                ? (60 + EndLane.Value - StartLane.Value) % 60 + 1
                : (60 + StartLane.Value - EndLane.Value) % 60 + 1;
            
            size = PassedThroughSeam ? 62 - size : size;
            return Math.Min(size, 60);
        }
    }
}