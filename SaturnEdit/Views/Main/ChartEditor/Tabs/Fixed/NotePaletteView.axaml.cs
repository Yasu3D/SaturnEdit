using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SaturnEdit.Systems;

namespace SaturnEdit.Views.Main.ChartEditor.Tabs;

public partial class NotePaletteView : UserControl
{
    public NotePaletteView()
    {
        InitializeComponent();
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

    private async void OnSettingsChanged(object? sender, EventArgs e)
    {
        // race conditions.....
        await Task.Delay(1);

        UpdateNoteTypeIcons();
        UpdateSubOptions("RadioButtonTouch");
        UpdateBonusTypeIcons("RadioButtonTouch");
    }
    
    private void RadioButtonNoteType_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;

        UpdateSubOptions(button.Name ?? "");
        UpdateBonusTypeIcons(button.Name ?? "");
    }

    private void RadioButtonBonusType_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        
    }

    private void RadioButtonSweepDirection_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        
    }

    public void UpdateNoteTypeIcons()
    {
        SvgTouch.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_touch_{(int)SettingsSystem.RenderSettings.TouchNoteColor}.svg";
        SvgChain.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_chain_{(int)SettingsSystem.RenderSettings.ChainNoteColor}.svg";
        SvgHold.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_hold_{(int)SettingsSystem.RenderSettings.HoldNoteColor}.svg";
        SvgSlideClockwise.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_slide_clw_{(int)SettingsSystem.RenderSettings.SlideClockwiseNoteColor}.svg";
        SvgSlideCounterclockwise.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_slide_ccw_{(int)SettingsSystem.RenderSettings.SlideCounterclockwiseNoteColor}.svg";
        SvgSnapForward.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_snap_fwd_{(int)SettingsSystem.RenderSettings.SnapForwardNoteColor}.svg";
        SvgSnapBackward.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_snap_bwd_{(int)SettingsSystem.RenderSettings.SnapBackwardNoteColor}.svg";
    }

    private void UpdateSubOptions(string name)
    {
        if (name is "RadioButtonLaneShow" or "RadioButtonLaneHide")
        {
            StackPanelBonusTypes.IsVisible = false;
            StackPanelSweepDirections.IsVisible = true;
            return;
        }

        StackPanelBonusTypes.IsVisible = true;
        StackPanelSweepDirections.IsVisible = false;
    }

    private void UpdateBonusTypeIcons(string name)
    {
        if (name is "RadioButtonLaneShow" or "RadioButtonLaneHide") return;

        int id = name switch
        {
            "RadioButtonTouch" => (int)SettingsSystem.RenderSettings.TouchNoteColor,
            "RadioButtonChain" => (int)SettingsSystem.RenderSettings.ChainNoteColor,
            "RadioButtonHold" => (int)SettingsSystem.RenderSettings.HoldNoteColor,
            "RadioButtonSlideClockwise" => (int)SettingsSystem.RenderSettings.SlideClockwiseNoteColor,
            "RadioButtonSlideCounterclockwise" => (int)SettingsSystem.RenderSettings.SlideCounterclockwiseNoteColor,
            "RadioButtonSnapForward" => (int)SettingsSystem.RenderSettings.SnapForwardNoteColor,
            "RadioButtonSnapBackward" => (int)SettingsSystem.RenderSettings.SnapBackwardNoteColor,
            _ => 0,
        };

        string type = name switch
        {
            "RadioButtonTouch" => "icon_touch",
            "RadioButtonChain" => "icon_chain",
            "RadioButtonHold" => "icon_hold",
            "RadioButtonSlideClockwise" => "icon_slide_clw",
            "RadioButtonSlideCounterclockwise" => "icon_slide_ccw",
            "RadioButtonSnapForward" => "icon_snap_fwd",
            "RadioButtonSnapBackward" => "icon_snap_bwd",
            _ => "",
        };

        string svgPath = $"avares://SaturnEdit/Assets/Icons/Color/{type}_{id}.svg";

        SvgBonusTypeNormalNote.Path = svgPath;
        SvgBonusTypeBonusNote.Path = svgPath;
        SvgBonusTypeRNote.Path = svgPath;
    }
}