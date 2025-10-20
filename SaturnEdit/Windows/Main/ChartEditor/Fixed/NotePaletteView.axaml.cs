using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnData.Notation.Core;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnEdit.Systems;

namespace SaturnEdit.Windows.Main.ChartEditor.Fixed;

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
        if (blockEvents) return;
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;
        
        BonusType bonusType = BonusType.Normal;
        JudgementType judgementType = JudgementType.Fake;
        LaneSweepDirection direction = LaneSweepDirection.Center;

        if (CursorSystem.CursorNote is IPositionable positionable)
        {
            CursorSystem.BackupPosition = positionable.Position;
            CursorSystem.BackupSize = positionable.Size;
        }

        if (CursorSystem.CursorNote is IPlayable playable)
        {
            bonusType = playable.BonusType;
            judgementType = playable.JudgementType;
        }

        if (CursorSystem.CursorNote is ILaneToggle laneToggle)
        {
            direction = laneToggle.Direction;
        }
        
        if (button.Name == "RadioButtonTouch" && CursorSystem.CursorNote is not TouchNote)
        {
            CursorSystem.CursorNote = new TouchNote(Timestamp.Zero, CursorSystem.BackupPosition, CursorSystem.BackupSize, bonusType, judgementType);
            return;
        }
        
        if (button.Name == "RadioButtonChain" && CursorSystem.CursorNote is not ChainNote)
        {
            CursorSystem.CursorNote = new ChainNote(Timestamp.Zero, CursorSystem.BackupPosition, CursorSystem.BackupSize, bonusType, judgementType);
            return;
        }
        
        if (button.Name == "RadioButtonHold" && CursorSystem.CursorNote is not HoldNote)
        {
            HoldNote holdNote = new(bonusType, judgementType);
            holdNote.Points.Add(new(Timestamp.Zero, CursorSystem.BackupPosition, CursorSystem.BackupSize, holdNote, HoldPointRenderType.Visible));
            
            CursorSystem.CursorNote = holdNote;
            return;
        }
        
        if (button.Name == "RadioButtonSlideClockwise" && CursorSystem.CursorNote is not SlideClockwiseNote)
        {
            CursorSystem.CursorNote = new SlideClockwiseNote(Timestamp.Zero, CursorSystem.BackupPosition, CursorSystem.BackupSize, bonusType, judgementType);
            return;
        }
        
        if (button.Name == "RadioButtonSlideCounterclockwise" && CursorSystem.CursorNote is not SlideCounterclockwiseNote)
        {
            CursorSystem.CursorNote = new SlideCounterclockwiseNote(Timestamp.Zero, CursorSystem.BackupPosition, CursorSystem.BackupSize, bonusType, judgementType);
            return;
        }
        
        if (button.Name == "RadioButtonSnapForward" && CursorSystem.CursorNote is not SnapForwardNote)
        {
            CursorSystem.CursorNote = new SnapForwardNote(Timestamp.Zero, CursorSystem.BackupPosition, CursorSystem.BackupSize, bonusType, judgementType);
            return;
        }
        
        if (button.Name == "RadioButtonSnapBackward" && CursorSystem.CursorNote is not SnapBackwardNote)
        {
            CursorSystem.CursorNote = new SnapBackwardNote(Timestamp.Zero, CursorSystem.BackupPosition, CursorSystem.BackupSize, bonusType, judgementType);
            return;
        }
        
        if (button.Name == "RadioButtonLaneShow" && CursorSystem.CursorNote is not LaneShowNote)
        {
            CursorSystem.CursorNote = new LaneShowNote(Timestamp.Zero, CursorSystem.BackupPosition, CursorSystem.BackupSize, direction);
            return;
        }
        
        if (button.Name == "RadioButtonLaneHide" && CursorSystem.CursorNote is not LaneHideNote)
        {
            CursorSystem.CursorNote = new LaneHideNote(Timestamp.Zero, CursorSystem.BackupPosition, CursorSystem.BackupSize, direction);
            return;
        }
        
        if (button.Name == "RadioButtonSync" && CursorSystem.CursorNote is not SyncNote)
        {
            CursorSystem.CursorNote = new SyncNote(Timestamp.Zero, CursorSystem.BackupPosition, CursorSystem.BackupSize);
            return;
        }
        
        if (button.Name == "RadioButtonMeasureLine" && CursorSystem.CursorNote is not MeasureLineNote)
        {
            CursorSystem.CursorNote = new MeasureLineNote(Timestamp.Zero, false);
        }
    }

    private void RadioButtonBonusType_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;

        if (CursorSystem.CursorNote is not IPlayable playable) return;
        
        playable.BonusType = button.Name switch
        {
            "RadioButtonBonusTypeNormal" => BonusType.Normal,
            "RadioButtonBonusTypeBonus" => BonusType.Bonus,
            "RadioButtonBonusTypeR" => BonusType.R,
            _ => BonusType.Normal,
        };
    }

    private void RadioButtonJudgementType_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;

        if (CursorSystem.CursorNote is not IPlayable playable) return;
        
        playable.JudgementType = button.Name switch
        {
            "RadioButtonJudgementTypeNormal" => JudgementType.Normal,
            "RadioButtonJudgementTypeFake" => JudgementType.Fake,
            "RadioButtonJudgementTypeAutoplay" => JudgementType.Autoplay,
            _ => JudgementType.Normal,
        };
    }
    
    private void RadioButtonHoldPointRenderType_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;

        if (CursorSystem.CursorNote is not HoldPointNote holdPointNote) return;
        
        holdPointNote.RenderType = button.Name switch
        {
            "RadioButtonHoldPointRenderTypeVisible" => HoldPointRenderType.Visible,
            "RadioButtonHoldPointRenderTypeHidden" => HoldPointRenderType.Hidden,
            _ => HoldPointRenderType.Visible,
        };
    }

    private void RadioButtonSweepDirection_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;

        if (CursorSystem.CursorNote is not ILaneToggle laneToggle) return;
        
        laneToggle.Direction = button.Name switch
        {
            "RadioButtonSweepDirectionCenter" => LaneSweepDirection.Center,
            "RadioButtonSweepDirectionClockwise" => LaneSweepDirection.Clockwise,
            "RadioButtonSweepDirectionCounterclockwise" => LaneSweepDirection.Counterclockwise,
            "RadioButtonSweepDirectionInstant" => LaneSweepDirection.Instant,
            _ => LaneSweepDirection.Center,
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
        StackPanelBonusTypesJudgementTypes.IsVisible = CursorSystem.CursorNote is IPlayable;
        StackPanelHoldPointRenderTypes.IsVisible = CursorSystem.CursorNote is HoldPointNote;
        StackPanelSweepDirections.IsVisible = CursorSystem.CursorNote is ILaneToggle;
    }

    private void UpdateBonusTypeIcons()
    {
        int id = CursorSystem.CursorNote switch
        {
            TouchNote => (int)SettingsSystem.RenderSettings.TouchNoteColor,
            ChainNote => (int)SettingsSystem.RenderSettings.ChainNoteColor,
            HoldNote => (int)SettingsSystem.RenderSettings.HoldNoteColor,
            SlideClockwiseNote => (int)SettingsSystem.RenderSettings.SlideClockwiseNoteColor,
            SlideCounterclockwiseNote => (int)SettingsSystem.RenderSettings.SlideCounterclockwiseNoteColor,
            SnapForwardNote => (int)SettingsSystem.RenderSettings.SnapForwardNoteColor,
            SnapBackwardNote => (int)SettingsSystem.RenderSettings.SnapBackwardNoteColor,
            _ => 0,
        };

        string svgPath = CursorSystem.CursorNote switch
        {
            TouchNote => $"avares://SaturnEdit/Assets/Icons/Color/icon_touch_{id}.svg",
            ChainNote => $"avares://SaturnEdit/Assets/Icons/Color/icon_chain_{id}.svg",
            HoldNote => $"avares://SaturnEdit/Assets/Icons/Color/icon_hold_{id}.svg",
            SlideClockwiseNote => $"avares://SaturnEdit/Assets/Icons/Color/icon_slide_clw_{id}.svg",
            SlideCounterclockwiseNote => $"avares://SaturnEdit/Assets/Icons/Color/icon_slide_ccw_{id}.svg",
            SnapForwardNote => $"avares://SaturnEdit/Assets/Icons/Color/icon_snap_fwd_{id}.svg",
            SnapBackwardNote => $"avares://SaturnEdit/Assets/Icons/Color/icon_snap_bwd_{id}.svg",
            LaneShowNote => $"avares://SaturnEdit/Assets/Icons/Color/icon_lane_show.svg",
            LaneHideNote => $"avares://SaturnEdit/Assets/Icons/Color/icon_lane_hide.svg",
            SyncNote => "avares://SaturnEdit/Assets/Icons/Color/icon_sync.svg",
            MeasureLineNote => "avares://SaturnEdit/Assets/Icons/Color/icon_measure.svg",
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
        
        if (CursorSystem.CursorNote is TouchNote) RadioButtonTouch.IsChecked = true;
        if (CursorSystem.CursorNote is ChainNote) RadioButtonChain.IsChecked = true;
        if (CursorSystem.CursorNote is HoldNote) RadioButtonHold.IsChecked = true;
        if (CursorSystem.CursorNote is SlideClockwiseNote) RadioButtonSlideClockwise.IsChecked = true;
        if (CursorSystem.CursorNote is SlideCounterclockwiseNote) RadioButtonSlideCounterclockwise.IsChecked = true;
        if (CursorSystem.CursorNote is SnapForwardNote) RadioButtonSnapForward.IsChecked = true;
        if (CursorSystem.CursorNote is SnapBackwardNote) RadioButtonSnapBackward.IsChecked = true;
        if (CursorSystem.CursorNote is LaneShowNote) RadioButtonLaneShow.IsChecked = true;
        if (CursorSystem.CursorNote is LaneHideNote) RadioButtonLaneHide.IsChecked = true;
        if (CursorSystem.CursorNote is SyncNote) RadioButtonSync.IsChecked = true;
        if (CursorSystem.CursorNote is MeasureLineNote) RadioButtonMeasureLine.IsChecked = true;

        if (CursorSystem.CursorNote is IPlayable playable)
        {
            if (playable.BonusType == BonusType.Normal) RadioButtonBonusTypeNormal.IsChecked = true;
            if (playable.BonusType == BonusType.R) RadioButtonBonusTypeR.IsChecked = true;
            if (playable.BonusType == BonusType.Bonus) RadioButtonBonusTypeBonus.IsChecked = true;
            
            if (playable.JudgementType == JudgementType.Normal) RadioButtonJudgementTypeNormal.IsChecked = true;
            if (playable.JudgementType == JudgementType.Fake) RadioButtonJudgementTypeFake.IsChecked = true;
            if (playable.JudgementType == JudgementType.Autoplay) RadioButtonJudgementTypeAutoplay.IsChecked = true;
        }

        if (CursorSystem.CursorNote is HoldPointNote holdPoint)
        {
            if (holdPoint.RenderType == HoldPointRenderType.Visible) RadioButtonHoldPointRenderTypeVisible.IsChecked = true;
            if (holdPoint.RenderType == HoldPointRenderType.Hidden) RadioButtonHoldPointRenderTypeHidden.IsChecked = true;
        }

        if (CursorSystem.CursorNote is ILaneToggle laneToggle)
        {
            if (laneToggle.Direction == LaneSweepDirection.Center) RadioButtonSweepDirectionCenter.IsChecked = true;
            if (laneToggle.Direction == LaneSweepDirection.Clockwise) RadioButtonSweepDirectionClockwise.IsChecked = true;
            if (laneToggle.Direction == LaneSweepDirection.Counterclockwise) RadioButtonSweepDirectionCounterclockwise.IsChecked = true;
            if (laneToggle.Direction == LaneSweepDirection.Instant) RadioButtonSweepDirectionInstant.IsChecked = true;
        }

        blockEvents = false;
    }
}