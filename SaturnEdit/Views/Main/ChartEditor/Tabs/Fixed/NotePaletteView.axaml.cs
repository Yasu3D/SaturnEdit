using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnEdit.Systems;

namespace SaturnEdit.Views.Main.ChartEditor.Tabs;

public partial class NotePaletteView : UserControl
{
    public NotePaletteView()
    {
        InitializeComponent();
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        CursorSystem.TypeChanged += OnTypeChanged;
        OnSettingsChanged(null, EventArgs.Empty);
        OnTypeChanged(null, EventArgs.Empty);
    }

    private bool blockEvents;

    private void OnTypeChanged(object? sender, EventArgs e)
    {
        UpdateSubOptions();
        UpdateBonusTypeIcons();
        UpdateSelection();
    }

    private async void OnSettingsChanged(object? sender, EventArgs e)
    {
        // race conditions.....
        await Task.Delay(1);

        UpdateShortcuts();
        UpdateNoteTypeIcons();
        UpdateSubOptions();
        UpdateBonusTypeIcons();
    }
    
    private void RadioButtonNoteType_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;

        CursorSystem.CurrentNoteType = button.Name switch
        {
            "RadioButtonTouch" => typeof(TouchNote),
            "RadioButtonChain" => typeof(ChainNote),
            "RadioButtonHold" => typeof(HoldNote),
            "RadioButtonSlideClockwise" => typeof(SlideClockwiseNote),
            "RadioButtonSlideCounterclockwise" => typeof(SlideCounterclockwiseNote),
            "RadioButtonSnapForward" => typeof(SnapForwardNote),
            "RadioButtonSnapBackward" => typeof(SnapBackwardNote),
            "RadioButtonLaneShow" => typeof(LaneShowNote),
            "RadioButtonLaneHide" => typeof(LaneHideNote),
            "RadioButtonSync" => typeof(SyncNote),
            "RadioButtonMeasureLine" => typeof(MeasureLineNote),
            _ => CursorSystem.CurrentNoteType,
        };
    }

    private void RadioButtonBonusType_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;

        CursorSystem.CurrentBonusType = button.Name switch
        {
            "RadioButtonBonusTypeNormal" => BonusType.Normal,
            "RadioButtonBonusTypeBonus" => BonusType.Bonus,
            "RadioButtonBonusTypeR" => BonusType.R,
            _ => CursorSystem.CurrentBonusType,
        };
    }

    private void RadioButtonJudgementType_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;

        CursorSystem.CurrentJudgementType = button.Name switch
        {
            "RadioButtonJudgementTypeNormal" => JudgementType.Normal,
            "RadioButtonJudgementTypeFake" => JudgementType.Fake,
            "RadioButtonJudgementTypeAutoplay" => JudgementType.Autoplay,
            _ => CursorSystem.CurrentJudgementType,
        };
    }
    
    private void RadioButtonHoldPointRenderType_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;

        CursorSystem.CurrentHoldPointRenderType = button.Name switch
        {
            "RadioButtonHoldPointRenderTypeVisible" => HoldPointRenderType.Visible,
            "RadioButtonHoldPointRenderTypeHidden" => HoldPointRenderType.Hidden,
            _ => CursorSystem.CurrentHoldPointRenderType,
        };
    }

    private void RadioButtonSweepDirection_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;

        CursorSystem.CurrentSweepDirection = button.Name switch
        {
            "RadioButtonSweepDirectionCenter" => LaneSweepDirection.Center,
            "RadioButtonSweepDirectionClockwise" => LaneSweepDirection.Clockwise,
            "RadioButtonSweepDirectionCounterclockwise" => LaneSweepDirection.Counterclockwise,
            "RadioButtonSweepDirectionInstant" => LaneSweepDirection.Instant,
            _ => CursorSystem.CurrentSweepDirection,
        };
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

    private void UpdateSubOptions()
    {
        bool showSweepDirections = CursorSystem.CurrentNoteType == typeof(LaneShowNote) 
                                   || CursorSystem.CurrentNoteType == typeof(LaneHideNote);
        
        bool showHoldPointRenderType = CursorSystem.CurrentNoteType == typeof(HoldNote);

        bool showBonusType =CursorSystem.CurrentNoteType == typeof(TouchNote)
            || CursorSystem.CurrentNoteType == typeof(ChainNote)
            || CursorSystem.CurrentNoteType == typeof(HoldNote)
            || CursorSystem.CurrentNoteType == typeof(SlideClockwiseNote)
            || CursorSystem.CurrentNoteType == typeof(SlideCounterclockwiseNote)
            || CursorSystem.CurrentNoteType == typeof(SnapForwardNote)
            || CursorSystem.CurrentNoteType == typeof(SnapBackwardNote);

        StackPanelBonusTypesJudgementTypes.IsVisible = showBonusType;
        StackPanelHoldPointRenderTypes.IsVisible = showHoldPointRenderType;
        StackPanelSweepDirections.IsVisible = showSweepDirections;
    }

    private void UpdateBonusTypeIcons()
    {
        Type type = CursorSystem.CurrentNoteType;
        int id = type switch
        {
            _ when type == typeof(TouchNote) => (int)SettingsSystem.RenderSettings.TouchNoteColor,
            _ when type == typeof(ChainNote) => (int)SettingsSystem.RenderSettings.ChainNoteColor,
            _ when type == typeof(HoldNote) => (int)SettingsSystem.RenderSettings.HoldNoteColor,
            _ when type == typeof(SlideClockwiseNote) => (int)SettingsSystem.RenderSettings.SlideClockwiseNoteColor,
            _ when type == typeof(SlideCounterclockwiseNote) => (int)SettingsSystem.RenderSettings.SlideCounterclockwiseNoteColor,
            _ when type == typeof(SnapForwardNote) => (int)SettingsSystem.RenderSettings.SnapForwardNoteColor,
            _ when type == typeof(SnapBackwardNote) => (int)SettingsSystem.RenderSettings.SnapBackwardNoteColor,
            _ => 0,
        };

        string svgPath = type switch
        {
            _ when type == typeof(TouchNote) => $"avares://SaturnEdit/Assets/Icons/Color/icon_touch_{id}.svg",
            _ when type == typeof(ChainNote) => $"avares://SaturnEdit/Assets/Icons/Color/icon_chain_{id}.svg",
            _ when type == typeof(HoldNote) => $"avares://SaturnEdit/Assets/Icons/Color/icon_hold_{id}.svg",
            _ when type == typeof(SlideClockwiseNote) => $"avares://SaturnEdit/Assets/Icons/Color/icon_slide_clw_{id}.svg",
            _ when type == typeof(SlideCounterclockwiseNote) => $"avares://SaturnEdit/Assets/Icons/Color/icon_slide_ccw_{id}.svg",
            _ when type == typeof(SnapForwardNote) => $"avares://SaturnEdit/Assets/Icons/Color/icon_snap_fwd_{id}.svg",
            _ when type == typeof(SnapBackwardNote) => $"avares://SaturnEdit/Assets/Icons/Color/icon_snap_bwd_{id}.svg",
            _ when type == typeof(LaneShowNote) => $"avares://SaturnEdit/Assets/Icons/Color/icon_lane_show.svg",
            _ when type == typeof(LaneHideNote) => $"avares://SaturnEdit/Assets/Icons/Color/icon_lane_hide.svg",
            _ when type == typeof(SyncNote) => "avares://SaturnEdit/Assets/Icons/Color/icon_sync.svg",
            _ when type == typeof(MeasureLineNote) => "avares://SaturnEdit/Assets/Icons/Color/icon_measure.svg",
            _ => "",
        };

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

    private void UpdateSelection()
    {
        blockEvents = true;
        
        if (CursorSystem.CurrentNoteType == typeof(TouchNote)) RadioButtonTouch.IsChecked = true;
        if (CursorSystem.CurrentNoteType == typeof(ChainNote)) RadioButtonChain.IsChecked = true;
        if (CursorSystem.CurrentNoteType == typeof(HoldNote)) RadioButtonHold.IsChecked = true;
        if (CursorSystem.CurrentNoteType == typeof(SlideClockwiseNote)) RadioButtonSlideClockwise.IsChecked = true;
        if (CursorSystem.CurrentNoteType == typeof(SlideCounterclockwiseNote)) RadioButtonSlideCounterclockwise.IsChecked = true;
        if (CursorSystem.CurrentNoteType == typeof(SnapForwardNote)) RadioButtonSnapForward.IsChecked = true;
        if (CursorSystem.CurrentNoteType == typeof(SnapBackwardNote)) RadioButtonSnapBackward.IsChecked = true;
        if (CursorSystem.CurrentNoteType == typeof(LaneShowNote)) RadioButtonLaneShow.IsChecked = true;
        if (CursorSystem.CurrentNoteType == typeof(LaneHideNote)) RadioButtonLaneHide.IsChecked = true;
        if (CursorSystem.CurrentNoteType == typeof(SyncNote)) RadioButtonSync.IsChecked = true;
        if (CursorSystem.CurrentNoteType == typeof(MeasureLineNote)) RadioButtonMeasureLine.IsChecked = true;

        if (CursorSystem.CurrentBonusType == BonusType.Normal) RadioButtonBonusTypeNormal.IsChecked = true;
        if (CursorSystem.CurrentBonusType == BonusType.Bonus) RadioButtonBonusTypeBonus.IsChecked = true;
        if (CursorSystem.CurrentBonusType == BonusType.R) RadioButtonBonusTypeR.IsChecked = true;
        
        if (CursorSystem.CurrentJudgementType == JudgementType.Normal) RadioButtonJudgementTypeNormal.IsChecked = true;
        if (CursorSystem.CurrentJudgementType == JudgementType.Fake) RadioButtonJudgementTypeFake.IsChecked = true;
        if (CursorSystem.CurrentJudgementType == JudgementType.Autoplay) RadioButtonJudgementTypeAutoplay.IsChecked = true;
        
        if (CursorSystem.CurrentHoldPointRenderType == HoldPointRenderType.Visible) RadioButtonHoldPointRenderTypeVisible.IsChecked = true;
        if (CursorSystem.CurrentHoldPointRenderType == HoldPointRenderType.Hidden) RadioButtonHoldPointRenderTypeHidden.IsChecked = true;
        
        if (CursorSystem.CurrentSweepDirection == LaneSweepDirection.Center) RadioButtonSweepDirectionCenter.IsChecked = true;
        if (CursorSystem.CurrentSweepDirection == LaneSweepDirection.Clockwise) RadioButtonSweepDirectionClockwise.IsChecked = true;
        if (CursorSystem.CurrentSweepDirection == LaneSweepDirection.Counterclockwise) RadioButtonSweepDirectionCounterclockwise.IsChecked = true;
        if (CursorSystem.CurrentSweepDirection == LaneSweepDirection.Instant) RadioButtonSweepDirectionInstant.IsChecked = true;

        blockEvents = false;
    }
}