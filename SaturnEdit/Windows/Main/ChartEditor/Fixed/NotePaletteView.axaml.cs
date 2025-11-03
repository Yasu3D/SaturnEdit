using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnData.Notation.Core;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.EditModeOperations;

namespace SaturnEdit.Windows.Main.ChartEditor.Fixed;

public partial class NotePaletteView : UserControl
{
    public NotePaletteView()
    {
        InitializeComponent();
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        UndoRedoSystem.OperationHistoryChanged += OnOperationHistoryChanged;
        OnOperationHistoryChanged(null, EventArgs.Empty);
    }

    private bool blockEvents;

#region Methods
    private void UpdateNoteTypeIcons()
    {
        Dispatcher.UIThread.Post(() =>
        {
            SvgTouch.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_touch_{(int)SettingsSystem.RenderSettings.TouchNoteColor}.svg";
            SvgChain.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_chain_{(int)SettingsSystem.RenderSettings.ChainNoteColor}.svg";
            SvgHold.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_hold_{(int)SettingsSystem.RenderSettings.HoldNoteColor}.svg";
            SvgSlideClockwise.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_slide_clw_{(int)SettingsSystem.RenderSettings.SlideClockwiseNoteColor}.svg";
            SvgSlideCounterclockwise.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_slide_ccw_{(int)SettingsSystem.RenderSettings.SlideCounterclockwiseNoteColor}.svg";
            SvgSnapForward.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_snap_fwd_{(int)SettingsSystem.RenderSettings.SnapForwardNoteColor}.svg";
            SvgSnapBackward.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_snap_bwd_{(int)SettingsSystem.RenderSettings.SnapBackwardNoteColor}.svg";
        });
    }

    private void UpdateBonusTypeIcons()
    {
        Dispatcher.UIThread.Post(() =>
        {
            int id = CursorSystem.CurrentType switch
            {
                TouchNote => (int)SettingsSystem.RenderSettings.TouchNoteColor,
                ChainNote => (int)SettingsSystem.RenderSettings.ChainNoteColor,
                HoldNote => (int)SettingsSystem.RenderSettings.HoldNoteColor,
                HoldPointNote => (int)SettingsSystem.RenderSettings.HoldNoteColor,
                SlideClockwiseNote => (int)SettingsSystem.RenderSettings.SlideClockwiseNoteColor,
                SlideCounterclockwiseNote => (int)SettingsSystem.RenderSettings.SlideCounterclockwiseNoteColor,
                SnapForwardNote => (int)SettingsSystem.RenderSettings.SnapForwardNoteColor,
                SnapBackwardNote => (int)SettingsSystem.RenderSettings.SnapBackwardNoteColor,
                _ => 0,
            };

            string svgPath = CursorSystem.CurrentType switch
            {
                TouchNote => $"avares://SaturnEdit/Assets/Icons/Color/icon_touch_{id}.svg",
                ChainNote => $"avares://SaturnEdit/Assets/Icons/Color/icon_chain_{id}.svg",
                HoldNote => $"avares://SaturnEdit/Assets/Icons/Color/icon_hold_{id}.svg",
                HoldPointNote => $"avares://SaturnEdit/Assets/Icons/Color/icon_hold_{id}.svg",
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
        });
    }

    private void UpdateShortcuts()
    {
        Dispatcher.UIThread.Post(() =>
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
        });
    }

    private void SetSelection()
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            if (CursorSystem.CurrentType is TouchNote) RadioButtonTouch.IsChecked = true;
            if (CursorSystem.CurrentType is ChainNote) RadioButtonChain.IsChecked = true;
            if (CursorSystem.CurrentType is HoldNote or HoldPointNote) RadioButtonHold.IsChecked = true;
            if (CursorSystem.CurrentType is SlideClockwiseNote) RadioButtonSlideClockwise.IsChecked = true;
            if (CursorSystem.CurrentType is SlideCounterclockwiseNote) RadioButtonSlideCounterclockwise.IsChecked = true;
            if (CursorSystem.CurrentType is SnapForwardNote) RadioButtonSnapForward.IsChecked = true;
            if (CursorSystem.CurrentType is SnapBackwardNote) RadioButtonSnapBackward.IsChecked = true;
            if (CursorSystem.CurrentType is LaneShowNote) RadioButtonLaneShow.IsChecked = true;
            if (CursorSystem.CurrentType is LaneHideNote) RadioButtonLaneHide.IsChecked = true;
            if (CursorSystem.CurrentType is SyncNote) RadioButtonSync.IsChecked = true;
            if (CursorSystem.CurrentType is MeasureLineNote) RadioButtonMeasureLine.IsChecked = true;

            if (CursorSystem.BonusType == BonusType.Normal) RadioButtonBonusTypeNormal.IsChecked = true;
            if (CursorSystem.BonusType == BonusType.R) RadioButtonBonusTypeR.IsChecked = true;
            if (CursorSystem.BonusType == BonusType.Bonus) RadioButtonBonusTypeBonus.IsChecked = true;

            if (CursorSystem.JudgementType == JudgementType.Normal) RadioButtonJudgementTypeNormal.IsChecked = true;
            if (CursorSystem.JudgementType == JudgementType.Fake) RadioButtonJudgementTypeFake.IsChecked = true;
            if (CursorSystem.JudgementType == JudgementType.Autoplay) RadioButtonJudgementTypeAutoplay.IsChecked = true;

            if (CursorSystem.RenderType == HoldPointRenderType.Visible) RadioButtonHoldPointRenderTypeVisible.IsChecked = true;
            if (CursorSystem.RenderType == HoldPointRenderType.Hidden) RadioButtonHoldPointRenderTypeHidden.IsChecked = true;

            if (CursorSystem.Direction == LaneSweepDirection.Center) RadioButtonSweepDirectionCenter.IsChecked = true;
            if (CursorSystem.Direction == LaneSweepDirection.Clockwise) RadioButtonSweepDirectionClockwise.IsChecked = true;
            if (CursorSystem.Direction == LaneSweepDirection.Counterclockwise) RadioButtonSweepDirectionCounterclockwise.IsChecked = true;
            if (CursorSystem.Direction == LaneSweepDirection.Instant) RadioButtonSweepDirectionInstant.IsChecked = true;

            StackPanelBonusTypesJudgementTypes.IsVisible = CursorSystem.CurrentType is IPlayable;
            StackPanelSweepDirections.IsVisible = CursorSystem.CurrentType is ILaneToggle;

            bool editingHoldNote = CursorSystem.CurrentType is HoldPointNote;
            StackPanelHoldPointRenderTypes.IsVisible = editingHoldNote;
            
            blockEvents = false;
        });
    }
#endregion Methods

#region System Event Delegates
    private async void OnSettingsChanged(object? sender, EventArgs e)
    {
        try
        {
            // race conditions.....
            await Task.Delay(1);

            UpdateShortcuts();
            UpdateNoteTypeIcons();
            UpdateBonusTypeIcons();
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
    }

    private void OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        UpdateBonusTypeIcons();
        
        SetSelection();
    }
#endregion System Event Delegates

#region UI Event Delegates
    private void RadioButtonNoteType_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;

        List<IOperation> operations = [];
        
        // Select new Type
        Note? newType = null;
        
        if (button == RadioButtonTouch && CursorSystem.CurrentType is not TouchNote)
        {
            newType = CursorSystem.TouchNote;
        }
        else if (button == RadioButtonChain && CursorSystem.CurrentType is not ChainNote)
        {
            newType = CursorSystem.ChainNote;
        }
        else if (button == RadioButtonHold && CursorSystem.CurrentType is not HoldNote)
        {
            newType = CursorSystem.HoldNote;
        }
        else if (button == RadioButtonSlideClockwise && CursorSystem.CurrentType is not SlideClockwiseNote)
        {
            newType = CursorSystem.SlideClockwiseNote;
        }
        else if (button == RadioButtonSlideCounterclockwise && CursorSystem.CurrentType is not SlideCounterclockwiseNote)
        {
            newType = CursorSystem.SlideCounterclockwiseNote;
        }
        else if (button == RadioButtonSnapForward && CursorSystem.CurrentType is not SnapForwardNote)
        {
            newType = CursorSystem.SnapForwardNote;
        }
        else if (button == RadioButtonSnapBackward && CursorSystem.CurrentType is not SnapBackwardNote)
        {
            newType = CursorSystem.SnapBackwardNote;
        }
        else if (button == RadioButtonLaneShow && CursorSystem.CurrentType is not LaneShowNote)
        {
            newType = CursorSystem.LaneShowNote;
        }
        else if (button == RadioButtonLaneHide && CursorSystem.CurrentType is not LaneHideNote)
        {
            newType = CursorSystem.LaneHideNote;
        }
        else if (button == RadioButtonSync && CursorSystem.CurrentType is not SyncNote)
        {
            newType = CursorSystem.SyncNote;
        }
        else if (button == RadioButtonMeasureLine && CursorSystem.CurrentType is not MeasureLineNote)
        {
            newType = CursorSystem.MeasureLineNote;
        }

        if (newType == null) return;
        operations.Add(new CursorTypeChangeOperation(CursorSystem.CurrentType, newType));

        // Exit edit mode when changing to another type.
        if (button.Name != "RadioButtonHold" && EditorSystem.Mode == EditorMode.EditMode && EditorSystem.ActiveObjectGroup is HoldNote)
        {
            CompositeOperation? op = EditorSystem.GetEditModeChangeOperation(EditorMode.ObjectMode, newType);

            if (op != null)
            {
                operations.Add(op);
            }
        }
        
        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private void RadioButtonBonusType_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;
        
        CursorSystem.BonusType = button.Name switch
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

        CursorSystem.JudgementType = button.Name switch
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

        CursorSystem.RenderType = button.Name switch
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

        CursorSystem.Direction = button.Name switch
        {
            "RadioButtonSweepDirectionCenter" => LaneSweepDirection.Center,
            "RadioButtonSweepDirectionClockwise" => LaneSweepDirection.Clockwise,
            "RadioButtonSweepDirectionCounterclockwise" => LaneSweepDirection.Counterclockwise,
            "RadioButtonSweepDirectionInstant" => LaneSweepDirection.Instant,
            _ => LaneSweepDirection.Center,
        };
    }
#endregion UI Event Delegates
}