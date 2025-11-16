using System;
using System.Collections.Generic;
using SaturnEdit.UndoRedo;

namespace SaturnEdit.Systems;

public static class UndoRedoSystem
{
    public static void Initialize()
    {
        ChartSystem.ChartLoaded += OnChartLoaded;

        StageSystem.StageLoaded += OnStageLoaded;

        CosmeticSystem.CosmeticLoaded += OnCosmeticLoaded;
    }

    /// <summary>
    /// The branch that keeps track of changes made to a chart.
    /// </summary>
    public static readonly UndoRedoBranch ChartBranch = new();

    /// <summary>
    /// The branch that keeps track of changes made to a stage.
    /// </summary>
    public static readonly UndoRedoBranch StageBranch = new();

    /// <summary>
    /// The branch that keeps track of changes made to a cosmetic item.
    /// </summary>
    public static readonly UndoRedoBranch CosmeticBranch = new();

#region System Event Delegates
    private static void OnChartLoaded(object? sender, EventArgs e) => ChartBranch.Clear();

    private static void OnStageLoaded(object? sender, EventArgs e) => StageBranch.Clear();

    private static void OnCosmeticLoaded(object? sender, EventArgs e) => CosmeticBranch.Clear();
#endregion System Event Delegates
}