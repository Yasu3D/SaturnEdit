using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using FluentIcons.Common;
using SaturnData.Notation.Core;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnEdit.Systems;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ChooseMirrorAxis;
using SaturnEdit.Windows.Dialogs.ModalDialog;
using SaturnEdit.Windows.Dialogs.SelectOffset;
using SaturnEdit.Windows.Dialogs.SelectScale;
using SaturnEdit.Windows.Dialogs.ZigZagHoldArgs;
using SaturnView;
using SkiaSharp;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class ChartView3D : UserControl
{
    public ChartView3D()
    {
        InitializeComponent();
        
        KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
        
        SizeChanged += Control_OnSizeChanged;
        ActualThemeVariantChanged += Control_OnActualThemeVariantChanged;
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        UndoRedoSystem.OperationHistoryChanged += OnOperationHistoryChanged;
        OnOperationHistoryChanged(null, EventArgs.Empty);

        SelectionSystem.PointerOverOverlapChanged += OnPointerOverOverlapChanged;
    }

    private readonly CanvasInfo canvasInfo = new();
    private bool blockEvents = false;
    private bool isGrabbingObject = false;
    private readonly ClickDragHelper clickDragLeft = new();
    private readonly ClickDragHelper clickDragRight = new();
   
#region Methods
    private async void AdjustAxis()
    {
        if (VisualRoot is not Window window) return;
            
        SelectMirrorAxisWindow selectMirrorAxisWindow = new();
        await selectMirrorAxisWindow.ShowDialog(window);

        if (selectMirrorAxisWindow.Result == ModalDialogResult.Primary)
        {
            _ = Task.Run(() => EditorSystem.MirrorAxis = selectMirrorAxisWindow.Axis);
        }
    }

    private async void ScaleSelection()
    {
        if (VisualRoot is not Window window) return;
            
        SelectScaleWindow selectScaleWindow = new();
        await selectScaleWindow.ShowDialog(window);

        if (selectScaleWindow.Result == ModalDialogResult.Primary)
        {
            _ = Task.Run(() => EditorSystem.Transform_ScaleSelection(selectScaleWindow.Scale));
        }
    }

    private async void OffsetChart()
    {
        if (VisualRoot is not Window window) return;
            
        SelectOffsetWindow selectOffsetWindow = new();
        await selectOffsetWindow.ShowDialog(window);

        if (selectOffsetWindow.Result == ModalDialogResult.Primary)
        {
            _ = Task.Run(() => EditorSystem.Transform_OffsetChart(selectOffsetWindow.Offset));
        }
    }

    private async void ScaleChart()
    {
        if (VisualRoot is not Window window) return;
            
        SelectScaleWindow selectScaleWindow = new();
        await selectScaleWindow.ShowDialog(window);

        if (selectScaleWindow.Result == ModalDialogResult.Primary)
        {
            _ = Task.Run(() => EditorSystem.Transform_ScaleChart(selectScaleWindow.Scale));
        }
    }

    private async void MirrorChart()
    {
        if (VisualRoot is not Window window) return;
            
        SelectMirrorAxisWindow selectMirrorAxisWindow = new();
        await selectMirrorAxisWindow.ShowDialog(window);

        if (selectMirrorAxisWindow.Result == ModalDialogResult.Primary)
        {
            _ = Task.Run(() => EditorSystem.Transform_MirrorChart(selectMirrorAxisWindow.Axis));
        }
    }
    
    private async void ZigZagHold()
    {
        if (VisualRoot is not Window window) return;
        
        if (!SelectionSystem.SelectedObjects.Any(x => x is HoldNote))
        {
            ModalDialogWindow modalDialog = new()
            {
                DialogIcon = Icon.Warning,
                WindowTitleKey = "ModalDialog.ZigZagHoldWarning.Title",
                HeaderKey = "ModalDialog.ZigZagHoldWarning.Header",
                ParagraphKey = "ModalDialog.ZigZagHoldWarning.Paragraph",
                ButtonPrimaryKey = "Generic.Ok",
            };
            
            modalDialog.InitializeDialog();
            await modalDialog.ShowDialog(window);
            
            return;
        }
            
        ZigZagHoldArgsWindow zigZagHoldArgsWindow = new();
        await zigZagHoldArgsWindow.ShowDialog(window);

        if (zigZagHoldArgsWindow.Result == ModalDialogResult.Primary)
        {
            _ = Task.Run(() => EditorSystem.Convert_ZigZagHold
            (
                beats:    zigZagHoldArgsWindow.Beats,
                division: zigZagHoldArgsWindow.Division,
                leftEdgeOffsetA:  zigZagHoldArgsWindow.LeftEdgeOffsetA,
                leftEdgeOffsetB:  zigZagHoldArgsWindow.LeftEdgeOffsetB,
                rightEdgeOffsetA: zigZagHoldArgsWindow.RightEdgeOffsetA,
                rightEdgeOffsetB: zigZagHoldArgsWindow.RightEdgeOffsetB
            ));
        }
    }
#endregion Methods
    
#region System Event Delegates
    private void OnSettingsChanged(object? sender, EventArgs e)
    { 
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            MenuItemShowSpeedChanges.IsChecked = SettingsSystem.RenderSettings.ShowSpeedChanges;
            MenuItemShowVisibilityChanges.IsChecked = SettingsSystem.RenderSettings.ShowVisibilityChanges;
            MenuItemShowLaneToggleAnimations.IsChecked = SettingsSystem.RenderSettings.ShowLaneToggleAnimations;
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
            MenuItemShowBookmarks.IsChecked = SettingsSystem.RenderSettings.ShowBookmarks;

            MenuItemHideEventMarkers.IsChecked = SettingsSystem.RenderSettings.HideEventMarkersDuringPlayback;
            MenuItemHideLaneToggleNotes.IsChecked = SettingsSystem.RenderSettings.HideLaneToggleNotesDuringPlayback;
            MenuItemHideHoldControlPoints.IsChecked = SettingsSystem.RenderSettings.HideHoldControlPointsDuringPlayback;
            MenuItemHideBookmarks.IsChecked = SettingsSystem.RenderSettings.HideBookmarksDuringPlayback;

            NumericUpDownNoteSpeed.Value = SettingsSystem.RenderSettings.NoteSpeed / 10.0m;
            ComboBoxBackgroundDim.SelectedIndex = (int)SettingsSystem.RenderSettings.BackgroundDim;
        
            MenuItemShowMarvelousWindows.IsEnabled = MenuItemShowJudgeAreas.IsChecked;
            MenuItemShowGreatWindows.IsEnabled = MenuItemShowJudgeAreas.IsChecked;
            MenuItemShowGoodWindows.IsEnabled = MenuItemShowJudgeAreas.IsChecked;

            blockEvents = false;
        
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
            MenuItemZigZagHold.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.ZigZagHold"].ToKeyGesture();
            MenuItemCutHold.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.CutHold"].ToKeyGesture();
            MenuItemJoinHold.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.JoinHold"].ToKeyGesture();

            MenuItemShowSpeedChanges.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowSpeedChanges"].ToKeyGesture();
            MenuItemShowVisibilityChanges.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowVisibilityChanges"].ToKeyGesture();
            MenuItemShowLaneToggleAnimations.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowLaneToggleAnimations"].ToKeyGesture();
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
            MenuItemHideBookmarks.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.HideDuringPlayback.Bookmarks"].ToKeyGesture();
        });
    }
    
    private void OnPointerOverOverlapChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            // Default Cursor
            // - No PointerOverObject
            // - No PointerOverOverlap
            if (SelectionSystem.PointerOverObject == null 
                || SelectionSystem.PointerOverOverlap == IPositionable.OverlapResult.None)
            {
                RenderCanvas.Cursor = new(StandardCursorType.Arrow);
                return;
            }
            
            // Omnidirectional Cursor
            // - PointerOverObject is not IPositionable
            // - PointerOverOverlap is Body
            if (SelectionSystem.PointerOverObject is not IPositionable positionable || SelectionSystem.PointerOverOverlap == IPositionable.OverlapResult.Body)
            {
                RenderCanvas.Cursor = new(StandardCursorType.SizeAll);
                return;
            }
            
            // Directional Cursor
            bool rightEdge = SelectionSystem.PointerOverOverlap == IPositionable.OverlapResult.RightEdge;
            int lane = rightEdge ? (positionable.Position + positionable.Size - 1) % 60 : positionable.Position;
            
            if (lane is >= 0 and <= 3
                or >= 56 and <= 59)
            {
                RenderCanvas.Cursor = rightEdge ? new(StandardCursorType.TopSide) : new(StandardCursorType.BottomSide);
            }

            if (lane is >= 4 and <= 10)
            {
                RenderCanvas.Cursor = rightEdge ? new(StandardCursorType.BottomRightCorner) : new(StandardCursorType.TopLeftCorner);
            }

            if (lane is >= 11 and <= 18)
            {
                RenderCanvas.Cursor = rightEdge ? new(StandardCursorType.LeftSide) : new(StandardCursorType.RightSide);
            }
                
            if (lane is >= 19 and <= 25)
            {
                RenderCanvas.Cursor = rightEdge ? new(StandardCursorType.BottomLeftCorner) : new(StandardCursorType.TopRightCorner);
            }
                
            if (lane is >= 26 and <= 33)
            {
                RenderCanvas.Cursor = rightEdge ? new(StandardCursorType.BottomSide) : new(StandardCursorType.TopSide);
            }
                
            if (lane is >= 34 and <= 40)
            {
                RenderCanvas.Cursor = rightEdge ? new(StandardCursorType.TopLeftCorner) : new(StandardCursorType.BottomRightCorner);
            }
                
            if (lane is >= 41 and <= 48)
            {
                RenderCanvas.Cursor = rightEdge ? new(StandardCursorType.RightSide) : new(StandardCursorType.LeftSide);
            }
                
            if (lane is >= 49 and <= 55)
            {
                RenderCanvas.Cursor = rightEdge ? new(StandardCursorType.TopRightCorner) : new(StandardCursorType.BottomLeftCorner);
            }
        });
    }
    
    private void OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        ComboBoxEditMode.SelectedIndex = EditorSystem.EditMode switch
        {
            EditorEditMode.NoteEditMode => 0,
            EditorEditMode.HoldEditMode => 1,
            EditorEditMode.EventEditMode => 2,
            _ => 0,
        };
    }
#endregion System Event Delegates

#region UI Event Delegates
    private void Control_OnKeyDown(object? sender, KeyEventArgs e)
    {
        IInputElement? focusedElement = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (KeyDownBlacklist.IsInvalidFocusedElement(focusedElement)) return;
        if (KeyDownBlacklist.IsInvalidKey(e.Key)) return;
        
        Shortcut shortcut = new(e.Key, e.KeyModifiers.HasFlag(KeyModifiers.Control), e.KeyModifiers.HasFlag(KeyModifiers.Alt), e.KeyModifiers.HasFlag(KeyModifiers.Shift));
        
        if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.TempoChange"]))
        {
            Task.Run(EditorSystem.Insert_AddTempoChange);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.MetreChange"]))
        {
            Task.Run(EditorSystem.Insert_AddMetreChange);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.TutorialMarker"]))
        {
            Task.Run(EditorSystem.Insert_AddTutorialMarker);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.SpeedChange"]))
        {
            Task.Run(EditorSystem.Insert_AddSpeedChange);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.VisibilityChange"]))
        {
            Task.Run(EditorSystem.Insert_AddVisibilityChange);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.StopEffect"]))
        {
            Task.Run(EditorSystem.Insert_AddStopEffect);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.ReverseEffect"]))
        {
            Task.Run(EditorSystem.Insert_AddReverseEffect);
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveBeatForward"])) 
        {
            Task.Run(EditorSystem.Transform_MoveSelectionBeatForward);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveBeatBack"])) 
        {
            Task.Run(EditorSystem.Transform_MoveSelectionBeatBack);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveMeasureForward"])) 
        {
            Task.Run(EditorSystem.Transform_MoveSelectionMeasureForward);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveMeasureBack"])) 
        {
            Task.Run(EditorSystem.Transform_MoveSelectionMeasureBack);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveClockwise"])) 
        {
            Task.Run(EditorSystem.Transform_MoveClockwise);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveCounterclockwise"])) 
        {
            Task.Run(EditorSystem.Transform_MoveCounterclockwise);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.IncreaseSize"])) 
        {
            Task.Run(EditorSystem.Transform_IncreaseSize);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.DecreaseSize"])) 
        {
            Task.Run(EditorSystem.Transform_DecreaseSize);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveClockwiseIterative"])) 
        {
            Task.Run(EditorSystem.Transform_MoveClockwiseIterative);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveCounterclockwiseIterative"])) 
        {
            Task.Run(EditorSystem.Transform_MoveCounterclockwiseIterative);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.IncreaseSizeIterative"])) 
        {
            Task.Run(EditorSystem.Transform_IncreaseSizeIterative);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.DecreaseSizeIterative"])) 
        {
            Task.Run(EditorSystem.Transform_DecreaseSizeIterative);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MirrorHorizontal"])) 
        {
            Task.Run(EditorSystem.Transform_MirrorHorizontal);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MirrorVertical"])) 
        {
            Task.Run(EditorSystem.Transform_MirrorVertical);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MirrorCustom"])) 
        {
            Task.Run(EditorSystem.Transform_MirrorCustom);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.AdjustAxis"])) 
        {
            AdjustAxis();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.FlipDirection"])) 
        {
            Task.Run(EditorSystem.Transform_FlipDirection);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.ReverseSelection"])) 
        {
            Task.Run(EditorSystem.Transform_ReverseSelection);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.ScaleSelection"])) 
        {
            ScaleSelection();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.OffsetChart"])) 
        {
            OffsetChart();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.ScaleChart"])) 
        {
            ScaleChart();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MirrorChart"])) 
        {
            MirrorChart();
            e.Handled = true;
        }
            
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.ZigZagHold"]))
        {
            ZigZagHold();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.CutHold"]))
        {
            Task.Run(EditorSystem.Convert_CutHold);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.JoinHold"]))
        {
            Task.Run(EditorSystem.Convert_JoinHold);
            e.Handled = true;
        }
            
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.IncreaseNoteSpeed"]))
        {
            
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.DecreaseNoteSpeed"]))
        {
            
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.IncreaseBackgroundDim"]))
        {
            
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.DecreaseBackgroundDim"]))
        {
            
            e.Handled = true;
        }
            
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowSpeedChanges"]))
        {
            
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowVisibilityChanges"]))
        {
            
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowLaneToggleAnimations"]))
        {
            
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.VisualizeLaneSweeps"]))
        {
            
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowJudgeAreas"]))
        {
            
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowMarvelousWindows"]))
        {
            
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowGreatWindows"]))
        {
            
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowGoodWindows"]))
        {
            
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.SaturnJudgeAreas"]))
        {
            
            e.Handled = true;
        }
            
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Touch"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SnapForward"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SnapBackward"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SlideClockwise"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SlideCounterclockwise"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Chain"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Hold"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Sync"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.MeasureLine"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.BeatLine"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.LaneShow"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.LaneHide"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.TempoChange"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.MetreChange"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SpeedChange"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.VisibilityChange"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.ReverseEffect"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.StopEffect"]))
        {
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.TutorialMarker"]))
        {
            e.Handled = true;
        }
            
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.HideDuringPlayback.EventMarkers"]))
        {
            
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.HideDuringPlayback.LaneToggleNotes"]))
        {
            
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.HideDuringPlayback.HoldControlPoints"]))
        {
            
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.HideDuringPlayback.Bookmarks"]))
        {
            
            e.Handled = true;
        }
    }

    private void Control_OnKeyUp(object? sender, KeyEventArgs e) => e.Handled = true;
    
    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        double minimum = double.Min(PanelCanvasContainer.Bounds.Width, PanelCanvasContainer.Bounds.Height);
        RenderCanvas.Width = minimum;
        RenderCanvas.Height = minimum;

        canvasInfo.Width = (float)RenderCanvas.Width;
        canvasInfo.Height = (float)RenderCanvas.Height;
        canvasInfo.Radius = canvasInfo.Width / 2;
        canvasInfo.Center = new(canvasInfo.Radius, canvasInfo.Radius);

        NumericUpDownNoteSpeed.IsVisible = Bounds.Width > 507;
        ComboBoxBackgroundDim.IsVisible = Bounds.Width > 620;
    }

    private async void Control_OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        try
        {
            await Task.Delay(1);

            if (Application.Current == null) return;
            if (!Application.Current.TryGetResource("BackgroundSecondary", Application.Current.ActualThemeVariant, out object? resource)) return;
            if (resource is not SolidColorBrush brush) return;
        
            canvasInfo.BackgroundColor= new(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
        }
        catch (Exception)
        {
            // classic error pink
            canvasInfo.BackgroundColor = new(0xFF, 0x00, 0xFF, 0xFF);
        }
    }
    
    
    private void RenderCanvas_OnRenderAction(SKCanvas canvas)
    {
        bool playing = TimeSystem.PlaybackState is PlaybackState.Playing or PlaybackState.Preview;
        
        Renderer3D.Render
        (
            canvas: canvas, 
            canvasInfo: canvasInfo, 
            settings: SettingsSystem.RenderSettings, 
            chart: ChartSystem.Chart, 
            entry: ChartSystem.Entry, 
            time: TimeSystem.Timestamp.Time, 
            playing: playing,
            selectedObjects: SelectionSystem.SelectedObjects,
            pointerOverObject: SelectionSystem.PointerOverObject,
            boxSelect: new(SelectionSystem.BoxSelectArgs.GlobalStartTime, SelectionSystem.BoxSelectArgs.GlobalEndTime, SelectionSystem.BoxSelectArgs.Position, SelectionSystem.BoxSelectArgs.Size),
            cursorNote: playing ? null : CursorSystem.CurrentNote
        );

        // Hook into render function to update box selection during playback,
        // even when the pointer is not being moved.
        if (playing && clickDragLeft.IsDragActive && !isGrabbingObject)
        {
            _ = Task.Run(() =>
            {
                float radius = Renderer3D.GetHitTestPointerRadius(canvasInfo, (float)clickDragLeft.EndPoint!.Value.Position.X, (float)clickDragLeft.EndPoint!.Value.Position.Y);
                float viewDistance = Renderer3D.GetViewDistance(SettingsSystem.RenderSettings.NoteSpeed);
            
                float t = RenderUtils.InversePerspective(radius);
                float viewTime = RenderUtils.Lerp(viewDistance, 0, t);
                
                SelectionSystem.SetBoxSelectionEnd(clickDragLeft.Position, clickDragLeft.Size, viewTime);
            });
        }
    }

    private void RenderCanvas_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        PointerPoint point = e.GetCurrentPoint(sender as Control);
        
        _ = Task.Run(() =>
        {
            float radius = Renderer3D.GetHitTestPointerRadius(canvasInfo, (float)point.Position.X, (float)point.Position.Y);
            int lane = Renderer3D.GetHitTestPointerLane(canvasInfo, (float)point.Position.X, (float)point.Position.Y);
            float viewDistance = Renderer3D.GetViewDistance(SettingsSystem.RenderSettings.NoteSpeed);
            
            onMove();
            onLeftDrag();
            onRightDrag();

            return;

            void onMove()
            {
                if (radius > 1.1f)
                {
                    SelectionSystem.PointerOverObject = null;
                    return;
                }
            
                float threshold = Renderer3D.GetHitTestThreshold(canvasInfo, SettingsSystem.RenderSettings.NoteThickness);
                
                foreach (Layer layer in ChartSystem.Chart.Layers)
                {
                    float scaledTime = Timestamp.ScaledTimeFromTime(layer, TimeSystem.Timestamp.Time);

                    foreach (Event @event in layer.Events)
                    {
                        if (!RenderUtils.IsVisible(@event, SettingsSystem.RenderSettings)) continue;
                        
                        IPositionable.OverlapResult hitTestResult = Renderer3D.HitTest(@event, radius, lane, TimeSystem.Timestamp.Time, TimeSystem.Timestamp.Time, viewDistance, threshold, false, SettingsSystem.RenderSettings);
                        if (hitTestResult != IPositionable.OverlapResult.None)
                        {
                            SelectionSystem.PointerOverObject = @event;
                            SelectionSystem.PointerOverOverlap = hitTestResult;
                            return;
                        }
                    }
                    
                    foreach (Note note in layer.Notes)
                    {
                        if (!RenderUtils.IsVisible(note, SettingsSystem.RenderSettings)) continue;
                        
                        IPositionable.OverlapResult hitTestResult = Renderer3D.HitTest(note, radius, lane, TimeSystem.Timestamp.Time, scaledTime, viewDistance, threshold, SettingsSystem.RenderSettings.ShowSpeedChanges, SettingsSystem.RenderSettings);
                        if (hitTestResult != IPositionable.OverlapResult.None)
                        {
                            SelectionSystem.PointerOverObject = note;
                            SelectionSystem.PointerOverOverlap = hitTestResult;
                            return;
                        }
                    }
                }
                
                foreach (Note note in ChartSystem.Chart.LaneToggles)
                {
                    if (!RenderUtils.IsVisible(note, SettingsSystem.RenderSettings)) continue;

                    IPositionable.OverlapResult hitTestResult = Renderer3D.HitTest(note, radius, lane, TimeSystem.Timestamp.Time, TimeSystem.Timestamp.Time, viewDistance, threshold, false, SettingsSystem.RenderSettings);
                    if (hitTestResult != IPositionable.OverlapResult.None)
                    {
                        SelectionSystem.PointerOverObject = note;
                        SelectionSystem.PointerOverOverlap = hitTestResult;
                        return;
                    }
                }

                foreach (Bookmark bookmark in ChartSystem.Chart.Bookmarks)
                {
                    if (!RenderUtils.IsVisible(bookmark, SettingsSystem.RenderSettings)) continue;

                    IPositionable.OverlapResult hitTestResult = Renderer3D.HitTest(bookmark, radius, lane, TimeSystem.Timestamp.Time, TimeSystem.Timestamp.Time, viewDistance, threshold, false, SettingsSystem.RenderSettings);
                    if (hitTestResult != IPositionable.OverlapResult.None)
                    {
                        SelectionSystem.PointerOverObject = bookmark;
                        SelectionSystem.PointerOverOverlap = hitTestResult;
                        return;
                    }
                }
                
                foreach (Event @event in ChartSystem.Chart.Events)
                {
                    if (!RenderUtils.IsVisible(@event, SettingsSystem.RenderSettings)) continue;

                    IPositionable.OverlapResult hitTestResult = Renderer3D.HitTest(@event, radius, lane, TimeSystem.Timestamp.Time, TimeSystem.Timestamp.Time, viewDistance, threshold, false, SettingsSystem.RenderSettings);
                    if (hitTestResult != IPositionable.OverlapResult.None)
                    {
                        SelectionSystem.PointerOverObject = @event;
                        SelectionSystem.PointerOverOverlap = hitTestResult;
                        return;
                    }
                }
                
                SelectionSystem.PointerOverObject = null;
                SelectionSystem.PointerOverOverlap = IPositionable.OverlapResult.None;
            }
            
            void onLeftDrag()
            {
                if (!e.Properties.IsLeftButtonPressed) return;

                clickDragLeft.EndPoint = point;
                if (!clickDragLeft.IsDragActive) return;

                clickDragLeft.EndLane = lane;
                
                // Box Select
                if (!isGrabbingObject)
                {
                    float t = RenderUtils.InversePerspective(radius);
                    float viewTime = RenderUtils.Lerp(viewDistance, 0, t);
                    
                    SelectionSystem.SetBoxSelectionEnd(clickDragLeft.Position, clickDragLeft.Size, viewTime);
                    
                    return;
                }

                // Drag Object
                if (isGrabbingObject)
                {
                    Console.WriteLine("Dragging object");
                }
            }

            void onRightDrag()
            {
                if (!e.Properties.IsRightButtonPressed) return;

                clickDragRight.EndPoint = point;
                if (!clickDragRight.IsDragActive) return;
                
                clickDragRight.EndLane = lane;
                CursorSystem.Position = clickDragRight.Position;
                CursorSystem.Size = clickDragRight.Size;
            }
        });
    }

    private void RenderCanvas_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        PointerPoint point = e.GetCurrentPoint(sender as Control);
        
        _ = Task.Run(() =>
        {
            float radius = Renderer3D.GetHitTestPointerRadius(canvasInfo, (float)point.Position.X, (float)point.Position.Y);
            int lane = Renderer3D.GetHitTestPointerLane(canvasInfo, (float)point.Position.X, (float)point.Position.Y);
            float viewDistance = Renderer3D.GetViewDistance(SettingsSystem.RenderSettings.NoteSpeed);

            onLeftClick();
            onRightClick();
                
            return;
                
            void onLeftClick()
            {
                if (!e.Properties.IsLeftButtonPressed) return;
                clickDragLeft.Reset(point, point, lane);

                isGrabbingObject = SelectionSystem.PointerOverObject != null;
                    
                boxSelect();
                normalSelect();

                return;
                    
                void boxSelect()
                {
                    float t = RenderUtils.InversePerspective(radius);
                    float viewTime = RenderUtils.Lerp(viewDistance, 0, t);
                        
                    SelectionSystem.SetBoxSelectionStart(
                        negativeSelection: e.KeyModifiers.HasFlag(KeyModifiers.Alt),
                        viewTime: viewTime);
                }

                void normalSelect()
                {
                    bool control = e.KeyModifiers.HasFlag(KeyModifiers.Control);
                    bool shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

                    SelectionSystem.SetSelection(control, shift);
                }
            }

            void onRightClick()
            {
                if (!e.Properties.IsRightButtonPressed) return;

                clickDragRight.Reset(point, point, lane);
                    
                CursorSystem.Position = clickDragRight.Position;
            }
        });
    }
    
    private void RenderCanvas_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _ = Task.Run(() =>
        {
            onLeftReleased();
            onRightReleased();
                
            return;
                
            void onLeftReleased()
            {
                if (e.InitialPressMouseButton != MouseButton.Left) return;
                clickDragLeft.Reset(null, null, 0);

                isGrabbingObject = false;
                    
                SelectionSystem.ApplyBoxSelection();
            }

            void onRightReleased()
            {
                if (e.InitialPressMouseButton != MouseButton.Right) return;
                clickDragRight.Reset(null, null, 0);
            }
        });
    }
    
    private void RenderCanvas_OnPointerExited(object? sender, PointerEventArgs e)
    {
        SelectionSystem.PointerOverObject = null;
    }
    
    
    private void MenuItemSettings_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not MenuItem menuItem) return;
        
        if (menuItem == MenuItemShowSpeedChanges)
        {
            SettingsSystem.RenderSettings.ShowSpeedChanges = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowVisibilityChanges)
        {
            SettingsSystem.RenderSettings.ShowVisibilityChanges = menuItem.IsChecked;
            return;
        }

        if (menuItem == MenuItemShowLaneToggleAnimations)
        {
            SettingsSystem.RenderSettings.ShowLaneToggleAnimations = menuItem.IsChecked;
            return;
        }
        
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

        if (menuItem == MenuItemShowBookmarks)
        {
            SettingsSystem.RenderSettings.ShowBookmarks = menuItem.IsChecked;
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

        if (menuItem == MenuItemHideBookmarks)
        {
            SettingsSystem.RenderSettings.HideBookmarksDuringPlayback = menuItem.IsChecked;
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

    
    private void MenuItemAddTempoChange_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Insert_AddTempoChange);

    private void MenuItemAddMetreChange_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Insert_AddMetreChange);

    private void MenuItemAddTutorialMarker_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Insert_AddTutorialMarker);

    private void MenuItemAddSpeedChange_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Insert_AddSpeedChange);

    private void MenuItemAddVisibilityChange_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Insert_AddVisibilityChange);

    private void MenuItemAddStopEffect_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Insert_AddStopEffect);

    private void MenuItemAddReverseEffect_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Insert_AddReverseEffect);

    private void MenuItemMoveSelectionBeatForward_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_MoveSelectionBeatForward);

    private void MenuItemMoveSelectionBeatBack_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_MoveSelectionBeatBack);

    private void MenuItemMoveSelectionMeasureForward_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_MoveSelectionMeasureForward);

    private void MenuItemMoveSelectionMeasureBack_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_MoveSelectionMeasureBack);

    private void MenuItemMoveClockwise_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_MoveClockwise);

    private void MenuItemMoveCounterclockwise_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_MoveCounterclockwise);

    private void MenuItemIncreaseSize_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_IncreaseSize);

    private void MenuItemDecreaseSize_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_DecreaseSize);

    private void MenuItemMoveClockwiseIterative_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_MoveClockwiseIterative);

    private void MenuItemMoveCounterclockwiseIterative_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_MoveCounterclockwiseIterative);

    private void MenuItemIncreaseSizeIterative_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_IncreaseSizeIterative);

    private void MenuItemDecreaseSizeIterative_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_DecreaseSizeIterative);

    private void MenuItemMirrorHorizontal_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_MirrorHorizontal);

    private void MenuItemMirrorVertical_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_MirrorVertical);

    private void MenuItemMirrorCustom_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_MirrorCustom);

    private void MenuItemAdjustAxis_OnClick(object? sender, RoutedEventArgs e) => AdjustAxis();

    private void MenuItemFlipDirection_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_FlipDirection);

    private void MenuItemReverseSelection_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_ReverseSelection);

    private void MenuItemScaleSelection_OnClick(object? sender, RoutedEventArgs e) => ScaleSelection();

    private void MenuItemOffsetChart_OnClick(object? sender, RoutedEventArgs e) => OffsetChart();

    private void MenuItemScaleChart_OnClick(object? sender, RoutedEventArgs e) => ScaleChart();

    private void MenuItemMirrorChart_OnClick(object? sender, RoutedEventArgs e) => MirrorChart();

    private void MenuItemZigZagHold_OnClick(object? sender, RoutedEventArgs e) => ZigZagHold();

    private void MenuItemCutHold_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Convert_CutHold);

    private void MenuItemJoinHold_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Convert_JoinHold);
#endregion UI Event Delegates
}