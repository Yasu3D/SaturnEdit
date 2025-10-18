using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using SaturnEdit.Systems;
using SaturnView;
using SkiaSharp;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class ChartView2D : UserControl
{
    public ChartView2D()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
        ActualThemeVariantChanged += OnActualThemeVariantChanged;

        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

    private readonly CanvasInfo canvasInfo = new();
    private bool blockEvents = false;

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        UpdateSettings();
        UpdateShortcuts();
    }

    private async void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        try
        {
            await Task.Delay(1);

            if (Application.Current == null) return;
            if (!Application.Current.TryGetResource("BackgroundSecondary", Application.Current.ActualThemeVariant, out object? resource)) return;
            if (resource is not SolidColorBrush brush) return;

            canvasInfo.BackgroundColor = new(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
        }
        catch (Exception)
        {
            // classic error pink
            canvasInfo.BackgroundColor = new(0xFF, 0x00, 0xFF, 0xFF);
        }
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        double minimum = double.Min(PanelCanvasContainer.Bounds.Width, PanelCanvasContainer.Bounds.Height);
        RenderCanvas.Width = minimum;
        RenderCanvas.Height = minimum;

        canvasInfo.Width = (float)RenderCanvas.Width;
        canvasInfo.Height = (float)RenderCanvas.Height;
        canvasInfo.Radius = canvasInfo.Width / 2;
        canvasInfo.Center = new(canvasInfo.Radius, canvasInfo.Radius);
    }

    private void RenderCanvas_OnRenderAction(SKCanvas canvas) => Renderer2D.Render(canvas, canvasInfo);

    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not MenuItem menuItem) return;
        
        if (menuItem == MenuItemShowJudgeAreas)
        {
            SettingsSystem.RenderSettings.ShowJudgeAreas = menuItem.IsChecked;

            MenuItemShowMarvelousWindows.IsEnabled = MenuItemShowJudgeAreas.IsChecked;
            MenuItemShowGreatWindows.IsEnabled = MenuItemShowJudgeAreas.IsChecked;
            MenuItemShowGoodWindows.IsEnabled = MenuItemShowJudgeAreas.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowMarvelousWindows)
        {
            SettingsSystem.RenderSettings.ShowMarvelousWindows = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowGreatWindows)
        {
            SettingsSystem.RenderSettings.ShowGreatWindows = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowGoodWindows)
        {
            SettingsSystem.RenderSettings.ShowGoodWindows = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemSaturnJudgeAreas)
        {
            SettingsSystem.RenderSettings.SaturnJudgeAreas = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemVisualizeLaneSweeps)
        {
            SettingsSystem.RenderSettings.VisualizeLaneSweeps = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowTouchNotes)
        {
            SettingsSystem.RenderSettings.ShowTouchNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowChainNotes)
        {
            SettingsSystem.RenderSettings.ShowChainNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowHoldNotes)
        {
            SettingsSystem.RenderSettings.ShowHoldNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowSlideClockwiseNotes)
        {
            SettingsSystem.RenderSettings.ShowSlideClockwiseNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowSlideCounterclockwiseNotes)
        {
            SettingsSystem.RenderSettings.ShowSlideCounterclockwiseNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowSnapForwardNotes)
        {
            SettingsSystem.RenderSettings.ShowSnapForwardNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowSnapBackwardNotes)
        {
            SettingsSystem.RenderSettings.ShowSnapBackwardNotes = menuItem.IsChecked;
            return;
        }

        if (menuItem == MenuItemShowSyncNotes)
        {
            SettingsSystem.RenderSettings.ShowSyncNotes = menuItem.IsChecked;
            return;
        }

        if (menuItem == MenuItemShowMeasureLineNotes)
        {
            SettingsSystem.RenderSettings.ShowMeasureLineNotes = menuItem.IsChecked;
        }
        
        if (menuItem == MenuItemShowBeatLineNotes)
        {
            SettingsSystem.RenderSettings.ShowBeatLineNotes = menuItem.IsChecked;
        }
        
        if (menuItem == MenuItemShowLaneShowNotes)
        {
            SettingsSystem.RenderSettings.ShowLaneShowNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowLaneHideNotes)
        {
            SettingsSystem.RenderSettings.ShowLaneHideNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowTempoChangeEvents)
        {
            SettingsSystem.RenderSettings.ShowTempoChangeEvents = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowMetreChangeEvents)
        {
            SettingsSystem.RenderSettings.ShowMetreChangeEvents = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowSpeedChangeEvents)
        {
            SettingsSystem.RenderSettings.ShowSpeedChangeEvents = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowVisibilityChangeEvents)
        {
            SettingsSystem.RenderSettings.ShowVisibilityChangeEvents = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowReverseEffectEvents)
        {
            SettingsSystem.RenderSettings.ShowReverseEffectEvents = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowStopEffectEvents)
        {
            SettingsSystem.RenderSettings.ShowStopEffectEvents = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowTutorialMarkerEvents)
        {
            SettingsSystem.RenderSettings.ShowTutorialMarkerEvents = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemHideEventMarkers)
        {
            SettingsSystem.RenderSettings.HideEventMarkersDuringPlayback = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemHideLaneToggleNotes)
        {
            SettingsSystem.RenderSettings.HideLaneToggleNotesDuringPlayback = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemHideHoldControlPoints)
        {
            SettingsSystem.RenderSettings.HideHoldControlPointsDuringPlayback = menuItem.IsChecked;
        }
    }

    private void UpdateSettings()
    {
        blockEvents = true;
        
        MenuItemShowJudgeAreas.IsChecked = SettingsSystem.RenderSettings.ShowJudgeAreas;
        MenuItemShowMarvelousWindows.IsChecked = SettingsSystem.RenderSettings.ShowMarvelousWindows;
        MenuItemShowGreatWindows.IsChecked = SettingsSystem.RenderSettings.ShowGreatWindows;
        MenuItemShowGoodWindows.IsChecked = SettingsSystem.RenderSettings.ShowGoodWindows;
        MenuItemSaturnJudgeAreas.IsChecked = SettingsSystem.RenderSettings.SaturnJudgeAreas;
        MenuItemVisualizeLaneSweeps.IsChecked = SettingsSystem.RenderSettings.VisualizeLaneSweeps;
        MenuItemShowTouchNotes.IsChecked = SettingsSystem.RenderSettings.ShowTouchNotes;
        MenuItemShowChainNotes.IsChecked = SettingsSystem.RenderSettings.ShowChainNotes;
        MenuItemShowHoldNotes.IsChecked = SettingsSystem.RenderSettings.ShowHoldNotes;
        MenuItemShowSlideClockwiseNotes.IsChecked = SettingsSystem.RenderSettings.ShowSlideClockwiseNotes;
        MenuItemShowSlideCounterclockwiseNotes.IsChecked = SettingsSystem.RenderSettings.ShowSlideCounterclockwiseNotes;
        MenuItemShowSnapForwardNotes.IsChecked = SettingsSystem.RenderSettings.ShowSnapForwardNotes;
        MenuItemShowSnapBackwardNotes.IsChecked = SettingsSystem.RenderSettings.ShowSnapBackwardNotes;
        MenuItemShowSyncNotes.IsChecked = SettingsSystem.RenderSettings.ShowSyncNotes;
        MenuItemShowMeasureLineNotes.IsChecked = SettingsSystem.RenderSettings.ShowMeasureLineNotes;
        MenuItemShowBeatLineNotes.IsChecked = SettingsSystem.RenderSettings.ShowBeatLineNotes;
        MenuItemShowLaneShowNotes.IsChecked = SettingsSystem.RenderSettings.ShowLaneShowNotes;
        MenuItemShowLaneHideNotes.IsChecked = SettingsSystem.RenderSettings.ShowLaneHideNotes;
        MenuItemShowTempoChangeEvents.IsChecked = SettingsSystem.RenderSettings.ShowTempoChangeEvents;
        MenuItemShowMetreChangeEvents.IsChecked = SettingsSystem.RenderSettings.ShowMetreChangeEvents;
        MenuItemShowSpeedChangeEvents.IsChecked = SettingsSystem.RenderSettings.ShowSpeedChangeEvents;
        MenuItemShowVisibilityChangeEvents.IsChecked = SettingsSystem.RenderSettings.ShowVisibilityChangeEvents;
        MenuItemShowReverseEffectEvents.IsChecked = SettingsSystem.RenderSettings.ShowReverseEffectEvents;
        MenuItemShowStopEffectEvents.IsChecked = SettingsSystem.RenderSettings.ShowStopEffectEvents;
        MenuItemShowTutorialMarkerEvents.IsChecked = SettingsSystem.RenderSettings.ShowTutorialMarkerEvents;

        MenuItemHideEventMarkers.IsChecked = SettingsSystem.RenderSettings.HideEventMarkersDuringPlayback;
        MenuItemHideLaneToggleNotes.IsChecked = SettingsSystem.RenderSettings.HideLaneToggleNotesDuringPlayback;
        MenuItemHideHoldControlPoints.IsChecked = SettingsSystem.RenderSettings.HideHoldControlPointsDuringPlayback;
        
        MenuItemShowMarvelousWindows.IsEnabled = MenuItemShowJudgeAreas.IsChecked;
        MenuItemShowGreatWindows.IsEnabled = MenuItemShowJudgeAreas.IsChecked;
        MenuItemShowGoodWindows.IsEnabled = MenuItemShowJudgeAreas.IsChecked;

        blockEvents = false;
    }

    private void UpdateShortcuts()
    {
        TextBlockShortcutBoxSelect.Text = SettingsSystem.ShortcutSettings.Shortcuts["Edit.BoxSelect"].ToString();
        TextBlockShortcutEditType.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.EditType"].ToString();
        TextBlockShortcutEditShape.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.EditShape"].ToString();
        TextBlockShortcutDeleteSelection.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"].ToString();
        TextBlockShortcutInsertNote.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.Insert"].ToString();
        
        MenuItemMoveSelectionBeatForward.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveBeatForward"].ToKeyGesture();
        MenuItemMoveSelectionBeatBack.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveBeatBack"].ToKeyGesture();
        MenuItemMoveSelectionMeasureForward.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveMeasureForward"].ToKeyGesture();
        MenuItemMoveSelectionMeasureBack.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveMeasureBack"].ToKeyGesture();
        MenuItemMoveClockwise.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveClockwise"].ToKeyGesture();
        MenuItemMoveCounterclockwise.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveCounterclockwise"].ToKeyGesture();
        MenuItemIncreaseSize.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.IncreaseSize"].ToKeyGesture();
        MenuItemDecreaseSize.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.DecreaseSize"].ToKeyGesture();
        MenuItemMoveClockwiseIterative.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveClockwiseIterative"].ToKeyGesture();
        MenuItemMoveCounterclockwiseIterative.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveCounterclockwiseIterative"].ToKeyGesture();
        MenuItemIncreaseSizeIterative.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.IncreaseSizeIterative"].ToKeyGesture();
        MenuItemDecreaseSizeIterative.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.DecreaseSizeIterative"].ToKeyGesture();
        MenuItemMirrorHorizontal.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MirrorHorizontal"].ToKeyGesture();
        MenuItemMirrorVertical.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MirrorVertical"].ToKeyGesture();
        MenuItemMirrorCustom.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MirrorCustom"].ToKeyGesture();
        MenuItemAdjustAxis.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.AdjustAxis"].ToKeyGesture();
        MenuItemFlipDirection.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.FlipDirection"].ToKeyGesture();
        MenuItemReverseSelection.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.ReverseSelection"].ToKeyGesture();
        MenuItemScaleSelection.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.ScaleSelection"].ToKeyGesture();
        MenuItemOffsetChart.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.OffsetChart"].ToKeyGesture();
        MenuItemScaleChart.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.ScaleChart"].ToKeyGesture();
        MenuItemMirrorChart.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MirrorChart"].ToKeyGesture();
        MenuItemNotesToHold.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.NotesToHold"].ToKeyGesture();
        MenuItemHoldToNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.HoldToNotes"].ToKeyGesture();
        MenuItemHoldToHold.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.HoldToHold"].ToKeyGesture();
        MenuItemSpikeHold.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.SpikeHold"].ToKeyGesture();
        MenuItemSplitHold.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.SplitHold"].ToKeyGesture();
        MenuItemMergeHold.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.MergeHold"].ToKeyGesture();

        MenuItemShowJudgeAreas.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowJudgeAreas"].ToKeyGesture();
        MenuItemShowMarvelousWindows.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowMarvelousWindows"].ToKeyGesture();
        MenuItemShowGreatWindows.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowGreatWindows"].ToKeyGesture();
        MenuItemShowGoodWindows.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowGoodWindows"].ToKeyGesture();
        MenuItemSaturnJudgeAreas.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.SaturnJudgeAreas"].ToKeyGesture();
        MenuItemVisualizeLaneSweeps.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.VisualizeLaneSweeps"].ToKeyGesture();
        MenuItemShowTouchNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Touch"].ToKeyGesture();
        MenuItemShowChainNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SnapForward"].ToKeyGesture();
        MenuItemShowHoldNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SnapBackward"].ToKeyGesture();
        MenuItemShowSlideClockwiseNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SlideClockwise"].ToKeyGesture();
        MenuItemShowSlideCounterclockwiseNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SlideCounterclockwise"].ToKeyGesture();
        MenuItemShowSnapForwardNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Chain"].ToKeyGesture();
        MenuItemShowSnapBackwardNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Hold"].ToKeyGesture();
        MenuItemShowSyncNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Sync"].ToKeyGesture();
        MenuItemShowMeasureLineNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.MeasureLine"].ToKeyGesture();
        MenuItemShowBeatLineNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.BeatLine"].ToKeyGesture();
        MenuItemShowLaneShowNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.LaneShow"].ToKeyGesture();
        MenuItemShowLaneHideNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.LaneHide"].ToKeyGesture();
        MenuItemShowTempoChangeEvents.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.TempoChange"].ToKeyGesture();
        MenuItemShowMetreChangeEvents.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.MetreChange"].ToKeyGesture();
        MenuItemShowSpeedChangeEvents.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SpeedChange"].ToKeyGesture();
        MenuItemShowVisibilityChangeEvents.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.VisibilityChange"].ToKeyGesture();
        MenuItemShowReverseEffectEvents.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.ReverseEffect"].ToKeyGesture();
        MenuItemShowStopEffectEvents.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.StopEffect"].ToKeyGesture();
        MenuItemShowTutorialMarkerEvents.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.TutorialMarker"].ToKeyGesture();

        MenuItemHideEventMarkers.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.HideDuringPlayback.EventMarkers"].ToKeyGesture();
        MenuItemHideLaneToggleNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.HideDuringPlayback.LaneToggleNotes"].ToKeyGesture();
        MenuItemHideHoldControlPoints.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.HideDuringPlayback.HoldControlPoints"].ToKeyGesture();
    }
}