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
        UpdateSubOptions(GetCheckedButtonInGroup("NoteType"));
        UpdateBonusTypeIcons(GetCheckedButtonInGroup("NoteType"));
        UpdateShortcuts();
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
    
    private void RadioButtonHoldPointRenderType_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
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
        bool showSweepDirections = name is "RadioButtonLaneShow"
            or "RadioButtonLaneHide";
        
        bool showHoldPointRenderType = name is "RadioButtonHold";

        bool showBonusType = name is "RadioButtonTouch"
            or "RadioButtonChain"
            or "RadioButtonHold"
            or "RadioButtonSlideClockwise"
            or "RadioButtonSlideCounterclockwise"
            or "RadioButtonSnapForward"
            or "RadioButtonSnapBackward";

        StackPanelBonusTypesJudgementTypes.IsVisible = showBonusType;
        StackPanelHoldPointRenderTypes.IsVisible = showHoldPointRenderType;
        StackPanelSweepDirections.IsVisible = showSweepDirections;
    }

    private void UpdateBonusTypeIcons(string name)
    {
        if (name is "" or "RadioButtonLaneShow" or "RadioButtonLaneHide" or "RadioButtonSync" or "RadioButtonMeasureLine") return;

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

    private void UpdateShortcuts()
    {
        TextBlockShortcutNoteTouch.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.Touch"].ToString();
        TextBlockShortcutNoteChain.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.Chain"].ToString();
        TextBlockShortcutNoteHold.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.Hold"].ToString();
        TextBlockShortcutNoteSlideClockwise.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.SlideClockwise"].ToString();
        TextBlockShortcutNoteSlideCounterclockwise.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.SlideCounterclockwise"].ToString();
        TextBlockShortcutNoteSnapForward.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.SnapForward"].ToString();
        TextBlockShortcutNoteSnapBackward.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.SnapBackward"].ToString();
        TextBlockShortcutNoteLaneShow.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.LaneShow"].ToString();
        TextBlockShortcutNoteLaneHide.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.LaneHide"].ToString();
        TextBlockShortcutNoteSync.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.Sync"].ToString();
        TextBlockShortcutNoteMeasureLine.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.MeasureLine"].ToString();
        TextBlockShortcutBonusTypeNormal.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.BonusType.Normal"].ToString();
        TextBlockShortcutBonusTypeBonus.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.BonusType.Bonus"].ToString();
        TextBlockShortcutBonusTypeR.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.BonusType.R"].ToString();
        TextBlockShortcutJudgementTypeNormal.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.JudgementType.Normal"].ToString();
        TextBlockShortcutJudgementTypeFake.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.JudgementType.Fake"].ToString();
        TextBlockShortcutJudgementTypeAutoplay.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.JudgementType.Autoplay"].ToString();
        TextBlockShortcutHoldPointRenderTypeVisible.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.SweepDirection.Center"].ToString();
        TextBlockShortcutHoldPointRenderTypeHidden.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.SweepDirection.Clockwise"].ToString();
        TextBlockShortcutSweepDirectionCenter.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.SweepDirection.Counterclockwise"].ToString();
        TextBlockShortcutSweepDirectionClockwise.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.SweepDirection.Instant"].ToString();
        TextBlockShortcutSweepDirectionCounterclockwise.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.HoldPointRenderType.Hidden"].ToString();
        TextBlockShortcutSweepDirectionInstant.Text = SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.HoldPointRenderType.Visible"].ToString();
    }

    private string GetCheckedButtonInGroup(string groupName)
    {
        if (groupName == "NoteType")
        {
            if (RadioButtonTouch.IsChecked ?? false) return "RadioButtonTouch";
            if (RadioButtonChain.IsChecked ?? false) return "RadioButtonChain";
            if (RadioButtonHold.IsChecked ?? false) return "RadioButtonHold";
            if (RadioButtonSlideClockwise.IsChecked ?? false) return "RadioButtonSlideClockwise";
            if (RadioButtonSlideCounterclockwise.IsChecked ?? false) return "RadioButtonSlideCounterclockwise";
            if (RadioButtonSnapForward.IsChecked ?? false) return "RadioButtonSnapForward";
            if (RadioButtonSnapBackward.IsChecked ?? false) return "RadioButtonSnapBackward";
            if (RadioButtonLaneShow.IsChecked ?? false) return "RadioButtonLaneShow";
            if (RadioButtonLaneHide.IsChecked ?? false) return "RadioButtonLaneHide";
        }

        if (groupName == "BonusType")
        {
            if (RadioButtonBonusTypeNormal.IsChecked ?? false) return "RadioButtonBonusTypeNormal";
            if (RadioButtonBonusTypeBonus.IsChecked ?? false) return "RadioButtonBonusTypeBonus";
            if (RadioButtonBonusTypeR.IsChecked ?? false) return "RadioButtonBonusTypeR";
        }
        
        if (groupName == "JudgementType")
        {
            if (RadioButtonJudgementTypeNormal.IsChecked ?? false) return "RadioButtonJudgementTypeNormal";
            if (RadioButtonJudgementTypeFake.IsChecked ?? false) return "RadioButtonJudgementTypeFake";
            if (RadioButtonJudgementTypeAutoplay.IsChecked ?? false) return "RadioButtonJudgementTypeAutoplay";
        }
        
        if (groupName == "HoldPointRenderType")
        {
            if (RadioButtonHoldPointRenderTypeVisible.IsChecked ?? false) return "RadioButtonHoldPointRenderTypeVisible";
            if (RadioButtonHoldPointRenderTypeHidden.IsChecked ?? false) return "RadioButtonHoldPointRenderTypeHidden";
        }
        
        return "";
    }
}