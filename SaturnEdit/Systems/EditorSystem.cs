using System;
using System.Collections.Generic;
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
}