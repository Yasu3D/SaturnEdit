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

public partial class ChartView3D : UserControl
{
    public ChartView3D()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
        ActualThemeVariantChanged += OnActualThemeVariantChanged;
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

    private readonly CanvasInfo canvasInfo = new();
    private SKColor clearColor;
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
        
            clearColor = new(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
        }
        catch (Exception ex)
        {
            // classic error pink
            clearColor = new(0xFF, 0x00, 0xFF, 0xFF);
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

    private void RenderCanvas_OnRenderAction(SKCanvas canvas) => Renderer3D.Render(canvas, canvasInfo, clearColor);

    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not MenuItem menuItem) return;

        if (menuItem == MenuItemShowJudgementWindows)
        {
            SettingsSystem.RenderSettings.ShowJudgementWindows = menuItem.IsChecked;

            MenuItemShowMarvelousWindows.IsEnabled = MenuItemShowJudgementWindows.IsChecked;
            MenuItemShowGreatWindows.IsEnabled = MenuItemShowJudgementWindows.IsChecked;
            MenuItemShowGoodWindows.IsEnabled = MenuItemShowJudgementWindows.IsChecked;
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
        
        if (menuItem == MenuItemSaturnJudgementWindows)
        {
            SettingsSystem.RenderSettings.SaturnJudgementWindows = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemVisualizeHoldNoteWindows)
        {
            SettingsSystem.RenderSettings.VisualizeHoldNoteWindows = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemVisualizeSweepAnimations)
        {
            SettingsSystem.RenderSettings.VisualizeSweepAnimations = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowTouch)
        {
            SettingsSystem.RenderSettings.ShowTouchNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowChain)
        {
            SettingsSystem.RenderSettings.ShowChainNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowHold)
        {
            SettingsSystem.RenderSettings.ShowHoldNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowSlideClockwise)
        {
            SettingsSystem.RenderSettings.ShowSlideClockwiseNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowSlideCounterclockwise)
        {
            SettingsSystem.RenderSettings.ShowSlideCounterclockwiseNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowSnapForward)
        {
            SettingsSystem.RenderSettings.ShowSnapForwardNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowSnapBackward)
        {
            SettingsSystem.RenderSettings.ShowSnapBackwardNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowLaneShow)
        {
            SettingsSystem.RenderSettings.ShowLaneShowNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowLaneHide)
        {
            SettingsSystem.RenderSettings.ShowLaneHideNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowTempoChange)
        {
            SettingsSystem.RenderSettings.ShowTempoChanges = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowMetreChange)
        {
            SettingsSystem.RenderSettings.ShowMetreChanges = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowSpeedChange)
        {
            SettingsSystem.RenderSettings.ShowSpeedChanges = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowVisibilityChange)
        {
            SettingsSystem.RenderSettings.ShowVisibilityChanges = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowReverseEffect)
        {
            SettingsSystem.RenderSettings.ShowReverseEffects = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowStopEffect)
        {
            SettingsSystem.RenderSettings.ShowStopEffects = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowTutorialMarker)
        {
            SettingsSystem.RenderSettings.ShowTutorialMarkers = menuItem.IsChecked;
            return;
        }
    }

    private void NumericUpDownNoteSpeed_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        SettingsSystem.RenderSettings.NoteSpeed = (int)Math.Round((e.NewValue * 10) ?? 3);
    }

    private void ComboBoxBackgroundDim_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not ComboBox comboBox) return;
        SettingsSystem.RenderSettings.BackgroundDim = (RenderSettings.BackgroundDimOption)comboBox.SelectedIndex;
    }
    
    private void UpdateSettings()
    {
        blockEvents = true;
        
        MenuItemShowJudgementWindows.IsChecked = SettingsSystem.RenderSettings.ShowJudgementWindows;
        MenuItemShowMarvelousWindows.IsChecked = SettingsSystem.RenderSettings.ShowMarvelousWindows;
        MenuItemShowGreatWindows.IsChecked = SettingsSystem.RenderSettings.ShowGreatWindows;
        MenuItemShowGoodWindows.IsChecked = SettingsSystem.RenderSettings.ShowGoodWindows;
        MenuItemSaturnJudgementWindows.IsChecked = SettingsSystem.RenderSettings.SaturnJudgementWindows;
        MenuItemVisualizeHoldNoteWindows.IsChecked = SettingsSystem.RenderSettings.VisualizeHoldNoteWindows;
        MenuItemVisualizeSweepAnimations.IsChecked = SettingsSystem.RenderSettings.VisualizeSweepAnimations;
        MenuItemShowTouch.IsChecked = SettingsSystem.RenderSettings.ShowTouchNotes;
        MenuItemShowChain.IsChecked = SettingsSystem.RenderSettings.ShowChainNotes;
        MenuItemShowHold.IsChecked = SettingsSystem.RenderSettings.ShowHoldNotes;
        MenuItemShowSlideClockwise.IsChecked = SettingsSystem.RenderSettings.ShowSlideClockwiseNotes;
        MenuItemShowSlideCounterclockwise.IsChecked = SettingsSystem.RenderSettings.ShowSlideCounterclockwiseNotes;
        MenuItemShowSnapForward.IsChecked = SettingsSystem.RenderSettings.ShowSnapForwardNotes;
        MenuItemShowSnapBackward.IsChecked = SettingsSystem.RenderSettings.ShowSnapBackwardNotes;
        MenuItemShowLaneShow.IsChecked = SettingsSystem.RenderSettings.ShowLaneShowNotes;
        MenuItemShowLaneHide.IsChecked = SettingsSystem.RenderSettings.ShowLaneHideNotes;
        MenuItemShowTempoChange.IsChecked = SettingsSystem.RenderSettings.ShowTempoChanges;
        MenuItemShowMetreChange.IsChecked = SettingsSystem.RenderSettings.ShowMetreChanges;
        MenuItemShowSpeedChange.IsChecked = SettingsSystem.RenderSettings.ShowSpeedChanges;
        MenuItemShowVisibilityChange.IsChecked = SettingsSystem.RenderSettings.ShowVisibilityChanges;
        MenuItemShowReverseEffect.IsChecked = SettingsSystem.RenderSettings.ShowReverseEffects;
        MenuItemShowStopEffect.IsChecked = SettingsSystem.RenderSettings.ShowStopEffects;
        MenuItemShowTutorialMarker.IsChecked = SettingsSystem.RenderSettings.ShowTutorialMarkers;

        NumericUpDownNoteSpeed.Value = SettingsSystem.RenderSettings.NoteSpeed / 10.0m;
        ComboBoxBackgroundDim.SelectedIndex = (int)SettingsSystem.RenderSettings.BackgroundDim;
        
        MenuItemShowMarvelousWindows.IsEnabled = MenuItemShowJudgementWindows.IsChecked;
        MenuItemShowGreatWindows.IsEnabled = MenuItemShowJudgementWindows.IsChecked;
        MenuItemShowGoodWindows.IsEnabled = MenuItemShowJudgementWindows.IsChecked;

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
        
        MenuItemShowJudgementWindows.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowJudgementWindows"].ToKeyGesture();
        MenuItemShowMarvelousWindows.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowMarvelousWindows"].ToKeyGesture();
        MenuItemShowGreatWindows.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowGreatWindows"].ToKeyGesture();
        MenuItemShowGoodWindows.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowGoodWindows"].ToKeyGesture();
        MenuItemSaturnJudgementWindows.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.SaturnJudgementWindows"].ToKeyGesture();
        MenuItemVisualizeHoldNoteWindows.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.VisualizeHoldNoteWindows"].ToKeyGesture();
        MenuItemVisualizeSweepAnimations.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.VisualizeSweepAnimations"].ToKeyGesture();
        MenuItemShowTouch.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Touch"].ToKeyGesture();
        MenuItemShowChain.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SnapForward"].ToKeyGesture();
        MenuItemShowHold.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SnapBackward"].ToKeyGesture();
        MenuItemShowSlideClockwise.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SlideClockwise"].ToKeyGesture();
        MenuItemShowSlideCounterclockwise.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SlideCounterclockwise"].ToKeyGesture();
        MenuItemShowSnapForward.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Chain"].ToKeyGesture();
        MenuItemShowSnapBackward.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Hold"].ToKeyGesture();
        MenuItemShowLaneShow.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.LaneShow"].ToKeyGesture();
        MenuItemShowLaneHide.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.LaneHide"].ToKeyGesture();
        MenuItemShowTempoChange.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.TempoChange"].ToKeyGesture();
        MenuItemShowMetreChange.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.MetreChange"].ToKeyGesture();
        MenuItemShowSpeedChange.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SpeedChange"].ToKeyGesture();
        MenuItemShowVisibilityChange.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.VisibilityChange"].ToKeyGesture();
        MenuItemShowReverseEffect.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.ReverseEffect"].ToKeyGesture();
        MenuItemShowStopEffect.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.StopEffect"].ToKeyGesture();
        MenuItemShowTutorialMarker.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.TutorialMarker"].ToKeyGesture();

    }
}