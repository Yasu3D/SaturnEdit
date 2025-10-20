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

    // PointerOver
    public static IPositionable.OverlapResult PointerOverOverlap { get; set; } = IPositionable.OverlapResult.None;
    public static ITimeable? PointerOverObject { get; set; } = null;
    
    // Selection
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
    public bool NegativeSelection = false;

    public int Position = 0;
    public int Size = 0;
}