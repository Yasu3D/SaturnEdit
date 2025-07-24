using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SaturnEdit.Views.Main.ChartEditor.Tabs;

public partial class NotePaletteView : UserControl
{
    public NotePaletteView()
    {
        InitializeComponent();

        Init();
    }

    private async void Init()
    {
        // race conditions.....
        await Task.Delay(10);

        UpdateNoteTypeIcons();
        UpdateSubOptions("RadioButtonTouch");
        UpdateBonusTypeIcons("RadioButtonTouch");
    }

    private int touchColorId = 0;
    private int chainColorId = 1;
    private int holdColorId = 6;
    private int slideClockwiseColorId = 2;
    private int slideCounterclockwiseColorId = 3;
    private int snapForwardColorId = 4;
    private int snapBackwardColorId = 5;
    
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
        SvgTouch.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_touch_{touchColorId}.svg";
        SvgChain.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_chain_{chainColorId}.svg";
        SvgHold.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_hold_{holdColorId}.svg";
        SvgSlideClockwise.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_slide_clw_{slideClockwiseColorId}.svg";
        SvgSlideCounterclockwise.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_slide_ccw_{slideCounterclockwiseColorId}.svg";
        SvgSnapForward.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_snap_fwd_{snapForwardColorId}.svg";
        SvgSnapBackward.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_snap_bwd_{snapBackwardColorId}.svg";
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
            "RadioButtonTouch" => touchColorId,
            "RadioButtonChain" => chainColorId,
            "RadioButtonHold" => holdColorId,
            "RadioButtonSlideClockwise" => slideClockwiseColorId,
            "RadioButtonSlideCounterclockwise" => slideCounterclockwiseColorId,
            "RadioButtonSnapForward" => snapForwardColorId,
            "RadioButtonSnapBackward" => snapBackwardColorId,
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