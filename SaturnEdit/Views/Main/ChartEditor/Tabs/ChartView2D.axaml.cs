using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using SaturnEdit.Systems;
using SaturnView;
using SkiaSharp;

namespace SaturnEdit.Views.Main.ChartEditor.Tabs;

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
    
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
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
         
         MenuItemShowMarvelousWindows.IsEnabled = MenuItemShowJudgementWindows.IsChecked;
         MenuItemShowGreatWindows.IsEnabled = MenuItemShowJudgementWindows.IsChecked;
         MenuItemShowGoodWindows.IsEnabled = MenuItemShowJudgementWindows.IsChecked;
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

    private readonly CanvasInfo canvasInfo = new();
    private SKColor clearColor;

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

    private void RenderCanvas_OnRenderAction(SKCanvas canvas) => Renderer2D.Render(canvas, canvasInfo, clearColor);

    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
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
}