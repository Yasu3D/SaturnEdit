using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using SaturnData.Notation.Core;
using SaturnData.Notation.Events;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnData.Utilities;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo.BookmarkOperations;
using SaturnEdit.UndoRedo.EventOperations;
using SaturnEdit.Utilities;
using SaturnView;
using SkiaSharp;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class ChartView2D : UserControl
{
    public ChartView2D()
    {
        InitializeComponent();

        clickDragLeft = new();
        clickDragRight = new();
        objectDrag = new(clickDragLeft);
        
        KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
        
        SizeChanged += Control_OnSizeChanged;
        ActualThemeVariantChanged += Control_OnActualThemeVariantChanged;
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        UndoRedoSystem.ChartBranch.OperationHistoryChanged += ChartBranch_OnOperationHistoryChanged;
        ChartBranch_OnOperationHistoryChanged(null, EventArgs.Empty);

        EditorSystem.EditModeChangeAttempted += OnEditModeChangeAttempted;
        OnEditModeChangeAttempted(null, EventArgs.Empty);

        SelectionSystem.PointerOverOverlapChanged += OnPointerOverOverlapChanged;
    }

    private static int zoomLevel = 10;
    
    private readonly CanvasInfo canvasInfo = new();
    private bool blockEvents = false;
    private readonly ClickDragHelper clickDragLeft;
    private readonly ClickDragHelper clickDragRight;

    private readonly ObjectDragHelper objectDrag;
    
    private Point? pointerPosition = null;
    private ITimeable? lastClickedObject = null;
    
#region Methods
    private void FindPointerOverObject(float depth, int lane, float viewDistance)
    {
        float threshold = Renderer2D.GetHitTestThreshold(canvasInfo, SettingsSystem.RenderSettings.NoteThickness);

        List<(IPositionable.OverlapResult, ITimeable)> hits = [];

        if (EditorSystem.Mode == EditorMode.ObjectMode)
        {
            foreach (Event @event in ChartSystem.Chart.Events)
            {
                if (!RenderUtils.IsVisible(@event, SettingsSystem.RenderSettings, EditorSystem.ActiveObjectGroup)) continue;

                IPositionable.OverlapResult hitTestResult = Renderer2D.HitTest(@event, depth, lane, TimeSystem.Timestamp.Time, viewDistance, threshold, SettingsSystem.RenderSettings, EditorSystem.ActiveObjectGroup);
                if (hitTestResult == IPositionable.OverlapResult.None) continue;

                hits.Add((hitTestResult, @event));
            }

            foreach (Bookmark bookmark in ChartSystem.Chart.Bookmarks)
            {
                if (!RenderUtils.IsVisible(bookmark, SettingsSystem.RenderSettings, EditorSystem.ActiveObjectGroup)) continue;

                IPositionable.OverlapResult hitTestResult = Renderer2D.HitTest(bookmark, depth, lane, TimeSystem.Timestamp.Time, viewDistance, threshold, SettingsSystem.RenderSettings, EditorSystem.ActiveObjectGroup);
                if (hitTestResult == IPositionable.OverlapResult.None) continue;

                hits.Add((hitTestResult, bookmark));
            }

            foreach (Note laneToggle in ChartSystem.Chart.LaneToggles)
            {
                if (!RenderUtils.IsVisible(laneToggle, SettingsSystem.RenderSettings, EditorSystem.ActiveObjectGroup)) continue;

                IPositionable.OverlapResult hitTestResult = Renderer2D.HitTest(laneToggle, depth, lane, TimeSystem.Timestamp.Time, viewDistance, threshold, SettingsSystem.RenderSettings, EditorSystem.ActiveObjectGroup);
                if (hitTestResult == IPositionable.OverlapResult.None) continue;

                hits.Add((hitTestResult, laneToggle));
            }

            foreach (Layer layer in ChartSystem.Chart.Layers)
            {
                foreach (Event @event in layer.Events)
                {
                    if (!RenderUtils.IsVisible(@event, SettingsSystem.RenderSettings, EditorSystem.ActiveObjectGroup)) continue;

                    IPositionable.OverlapResult hitTestResult = Renderer2D.HitTest(@event, depth, lane, TimeSystem.Timestamp.Time, viewDistance, threshold, SettingsSystem.RenderSettings, EditorSystem.ActiveObjectGroup);
                    if (hitTestResult == IPositionable.OverlapResult.None) continue;

                    hits.Add((hitTestResult, @event));
                }

                foreach (Note note in layer.Notes)
                {
                    if (!RenderUtils.IsVisible(note, SettingsSystem.RenderSettings, EditorSystem.ActiveObjectGroup)) continue;

                    IPositionable.OverlapResult hitTestResult = Renderer2D.HitTest(note, depth, lane, TimeSystem.Timestamp.Time, viewDistance, threshold, SettingsSystem.RenderSettings, EditorSystem.ActiveObjectGroup);
                    if (hitTestResult == IPositionable.OverlapResult.None) continue;

                    hits.Add((hitTestResult, note));
                }
            }
        }
        else if (EditorSystem.Mode == EditorMode.EditMode)
        {
            if (EditorSystem.ActiveObjectGroup is HoldNote holdNote)
            {
                Layer? layer = ChartSystem.Chart.ParentLayer(holdNote);

                if (layer != null)
                {
                    foreach (HoldPointNote holdPointNote in holdNote.Points)
                    {
                        if (!RenderUtils.IsVisible(holdPointNote, SettingsSystem.RenderSettings, EditorSystem.ActiveObjectGroup)) continue;

                        IPositionable.OverlapResult hitTestResult = Renderer2D.HitTest(holdPointNote, depth, lane, TimeSystem.Timestamp.Time, viewDistance, threshold, SettingsSystem.RenderSettings, EditorSystem.ActiveObjectGroup);
                        if (hitTestResult == IPositionable.OverlapResult.None) continue;

                        hits.Add((hitTestResult, holdPointNote));
                    }
                }
            }
            else if (EditorSystem.ActiveObjectGroup is StopEffectEvent stopEffectEvent)
            {
                foreach (EffectSubEvent subEvent in stopEffectEvent.SubEvents)
                {
                    if (!RenderUtils.IsVisible(subEvent, SettingsSystem.RenderSettings, EditorSystem.ActiveObjectGroup)) continue;

                    IPositionable.OverlapResult hitTestResult = Renderer2D.HitTest(subEvent, depth, lane, TimeSystem.Timestamp.Time, viewDistance, threshold, SettingsSystem.RenderSettings, EditorSystem.ActiveObjectGroup);
                    if (hitTestResult == IPositionable.OverlapResult.None) continue;

                    hits.Add((hitTestResult, subEvent));
                }
            }
            else if (EditorSystem.ActiveObjectGroup is ReverseEffectEvent reverseEffectEvent)
            {
                foreach (EffectSubEvent subEvent in reverseEffectEvent.SubEvents)
                {
                    if (!RenderUtils.IsVisible(subEvent, SettingsSystem.RenderSettings, EditorSystem.ActiveObjectGroup)) continue;

                    IPositionable.OverlapResult hitTestResult = Renderer2D.HitTest(subEvent, depth, lane, TimeSystem.Timestamp.Time, viewDistance, threshold, SettingsSystem.RenderSettings, EditorSystem.ActiveObjectGroup);
                    if (hitTestResult == IPositionable.OverlapResult.None) continue;

                    hits.Add((hitTestResult, subEvent));
                }
            }
        }

        hits = hits
            .OrderBy(x => SelectionSystem.SelectedObjects.Contains(x.Item2))
            .ThenBy(x => x.Item2 is ILaneToggle)
            .ThenBy(x => x.Item2 is SyncNote or MeasureLineNote)
            .ThenBy(x => x.Item2 is Event or Bookmark)
            .ThenBy(x => x.Item2 is HoldNote or HoldPointNote)
            .ThenBy(x => (x.Item2 as IPositionable)?.Size ?? 60)
            .ToList();

        if (hits.Count != 0)
        {
            SelectionSystem.PointerOverOverlap = hits[0].Item1;
            SelectionSystem.PointerOverObject = hits[0].Item2;
        }
        else
        {
            SelectionSystem.PointerOverObject = null;
            SelectionSystem.PointerOverOverlap = IPositionable.OverlapResult.None;
        }
    }
    
    private void SetCursorType()
    {
        Dispatcher.UIThread.Post(() =>
        {
            // Default Cursor if position can't be found.
            if (pointerPosition == null)
            {
                RenderCanvas.Cursor = new(StandardCursorType.Arrow);
                return;
            }
            
            // Default Cursor if box selecting.
            if (!objectDrag.IsActive && clickDragLeft.IsActive)
            {
                RenderCanvas.Cursor = new(StandardCursorType.Arrow);
                return;
            }
            
            bool allowDefaultCursor = !objectDrag.IsActive;
            bool allowOmniCursor = !objectDrag.IsActive || objectDrag.DragType == IPositionable.OverlapResult.Body;
            
            // Default Cursor
            // - No PointerOverObject
            // - No PointerOverOverlap
            if (allowDefaultCursor && (SelectionSystem.PointerOverObject == null || SelectionSystem.PointerOverOverlap == IPositionable.OverlapResult.None))
            {
                RenderCanvas.Cursor = new(StandardCursorType.Arrow);
                return;
            }
            
            // Omnidirectional Cursor
            // - PointerOverObject is not IPositionable
            // - PointerOverOverlap is Body
            if (allowOmniCursor && (SelectionSystem.PointerOverObject is not IPositionable || SelectionSystem.PointerOverOverlap == IPositionable.OverlapResult.Body))
            {
                RenderCanvas.Cursor = new(StandardCursorType.SizeAll);
                return;
            }
            
            // Directional Cursor
            if (SelectionSystem.PointerOverOverlap == IPositionable.OverlapResult.LeftEdge)
            {
                RenderCanvas.Cursor = new(StandardCursorType.LeftSide);
            }

            if (SelectionSystem.PointerOverOverlap == IPositionable.OverlapResult.RightEdge)
            {
                RenderCanvas.Cursor = new(StandardCursorType.RightSide);
            }
        });
    }
#endregion Methods
    
#region System Event Handlers
    private void OnSettingsChanged(object? sender, EventArgs e)
    { 
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            MenuItemShowJudgeAreas.IsChecked = SettingsSystem.RenderSettings.ShowJudgeAreas;
            MenuItemShowMarvelousArea.IsChecked = SettingsSystem.RenderSettings.ShowMarvelousArea;
            MenuItemShowGreatArea.IsChecked = SettingsSystem.RenderSettings.ShowGreatArea;
            MenuItemShowGoodArea.IsChecked = SettingsSystem.RenderSettings.ShowGoodArea;
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
        
            MenuItemShowMarvelousArea.IsEnabled = MenuItemShowJudgeAreas.IsChecked;
            MenuItemShowGreatArea.IsEnabled = MenuItemShowJudgeAreas.IsChecked;
            MenuItemShowGoodArea.IsEnabled = MenuItemShowJudgeAreas.IsChecked;

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

            MenuItemShowJudgeAreas.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowJudgeAreas"].ToKeyGesture();
            MenuItemShowMarvelousArea.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowMarvelousArea"].ToKeyGesture();
            MenuItemShowGreatArea.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowGreatArea"].ToKeyGesture();
            MenuItemShowGoodArea.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowGoodArea"].ToKeyGesture();
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
        SetCursorType();
    }
    
    private void ChartBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        // End object drag if anything else changes.
        objectDrag.End();
        
        bool holdEditModeAvailable = EditorSystem.Mode == EditorMode.EditMode || EditorSystem.EditModeAvailable;
        
        Dispatcher.UIThread.Post(() =>
        {
            ComboBoxEditMode.SelectedIndex = EditorSystem.Mode switch
            {
                EditorMode.ObjectMode => 0,
                EditorMode.EditMode => 1,
                _ => 0,
            };
            
            ComboBoxItemHoldEditMode.IsEnabled = holdEditModeAvailable;
        });
    }

    private void OnEditModeChangeAttempted(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ComboBoxEditMode.SelectedIndex = EditorSystem.Mode switch
            {
                EditorMode.ObjectMode => 0,
                EditorMode.EditMode => 1,
                _ => 0,
            };
        });
    }
#endregion System Event Handlers

#region UI Event Handlers
    private void Control_OnKeyDown(object? sender, KeyEventArgs e)
    {
        IInputElement? focusedElement = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (KeyDownBlacklist.IsInvalidFocusedElement(focusedElement)) return;
        if (KeyDownBlacklist.IsInvalidKey(e.Key)) return;
        if (KeyDownBlacklist.IsInvalidState()) return;
        
        Shortcut shortcut = new(e.Key, e.KeyModifiers.HasFlag(KeyModifiers.Control), e.KeyModifiers.HasFlag(KeyModifiers.Alt), e.KeyModifiers.HasFlag(KeyModifiers.Shift));

        if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.AutoMode"]))
        {
            Task.Run(EditorSystem.ChangeEditMode);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.ObjectMode"]))
        {
            Task.Run(() => EditorSystem.ChangeEditMode(EditorMode.ObjectMode));
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.EditMode"]))
        {
            Task.Run(() => EditorSystem.ChangeEditMode(EditorMode.EditMode));
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.Insert"]))
        {
            Task.Run(EditorSystem.ToolBar_Insert);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"]))
        {
            Task.Run(EditorSystem.ToolBar_Delete);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.EditShape"]))
        {
            Task.Run(EditorSystem.ToolBar_EditShape);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.EditType"]))
        {
            Task.Run(EditorSystem.ToolBar_EditType);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.EditBoth"]))
        {
            Task.Run(EditorSystem.ToolBar_EditBoth);
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.TempoChange"]))
        {
            MainWindow.Instance?.ChartEditor.ChartView_AddTempoChangeEvent();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.MetreChange"]))
        {
            MainWindow.Instance?.ChartEditor.ChartView_AddMetreChangeEvent();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.TutorialMarker"]))
        {
            MainWindow.Instance?.ChartEditor.ChartView_AddTutorialMarkerEvent();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.SpeedChange"]))
        {
            MainWindow.Instance?.ChartEditor.ChartView_AddSpeedChangeEvent();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.VisibilityChange"]))
        {
            MainWindow.Instance?.ChartEditor.ChartView_AddVisibilityChangeEvent();
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
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.Bookmark"]))
        {
            MainWindow.Instance?.ChartEditor.ChartView_AddBookmark();
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
            MainWindow.Instance?.ChartEditor.ChartView_AdjustAxis();
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
            MainWindow.Instance?.ChartEditor.ChartView_ScaleSelection();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.OffsetChart"])) 
        {
            MainWindow.Instance?.ChartEditor.ChartView_OffsetChart();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.ScaleChart"])) 
        {
            MainWindow.Instance?.ChartEditor.ChartView_ScaleChart();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MirrorChart"])) 
        {
            MainWindow.Instance?.ChartEditor.ChartView_MirrorChart();
            e.Handled = true;
        }
            
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.ZigZagHold"]))
        {
            MainWindow.Instance?.ChartEditor.ChartView_ZigZagHold();
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
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.VisualizeLaneSweeps"]))
        {
            SettingsSystem.RenderSettings.VisualizeLaneSweeps = !SettingsSystem.RenderSettings.VisualizeLaneSweeps;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowJudgeAreas"]))
        {
            SettingsSystem.RenderSettings.ShowJudgeAreas = !SettingsSystem.RenderSettings.ShowJudgeAreas;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowMarvelousArea"]))
        {
            SettingsSystem.RenderSettings.ShowMarvelousArea = !SettingsSystem.RenderSettings.ShowMarvelousArea;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowGreatArea"]))
        {
            SettingsSystem.RenderSettings.ShowGreatArea = !SettingsSystem.RenderSettings.ShowGreatArea;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowGoodArea"]))
        {
            SettingsSystem.RenderSettings.ShowGoodArea = !SettingsSystem.RenderSettings.ShowGoodArea;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.SaturnJudgeAreas"]))
        {
            SettingsSystem.RenderSettings.SaturnJudgeAreas = !SettingsSystem.RenderSettings.SaturnJudgeAreas;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Touch"]))
        {
            SettingsSystem.RenderSettings.ShowTouchNotes = !SettingsSystem.RenderSettings.ShowTouchNotes;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SnapForward"]))
        {
            SettingsSystem.RenderSettings.ShowSnapForwardNotes = !SettingsSystem.RenderSettings.ShowSnapForwardNotes;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SnapBackward"]))
        {
            SettingsSystem.RenderSettings.ShowSnapBackwardNotes = !SettingsSystem.RenderSettings.ShowSnapBackwardNotes;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SlideClockwise"]))
        {
            SettingsSystem.RenderSettings.ShowSlideClockwiseNotes = !SettingsSystem.RenderSettings.ShowSlideClockwiseNotes;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SlideCounterclockwise"]))
        {
            SettingsSystem.RenderSettings.ShowSlideCounterclockwiseNotes = !SettingsSystem.RenderSettings.ShowSlideCounterclockwiseNotes;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Chain"]))
        {
            SettingsSystem.RenderSettings.ShowChainNotes = !SettingsSystem.RenderSettings.ShowChainNotes;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Hold"]))
        {
            SettingsSystem.RenderSettings.ShowHoldNotes = !SettingsSystem.RenderSettings.ShowHoldNotes;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Sync"]))
        {
            SettingsSystem.RenderSettings.ShowSyncNotes = !SettingsSystem.RenderSettings.ShowSyncNotes;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.MeasureLine"]))
        {
            SettingsSystem.RenderSettings.ShowMeasureLineNotes = !SettingsSystem.RenderSettings.ShowMeasureLineNotes;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.BeatLine"]))
        {
            SettingsSystem.RenderSettings.ShowBeatLineNotes = !SettingsSystem.RenderSettings.ShowBeatLineNotes;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.LaneShow"]))
        {
            SettingsSystem.RenderSettings.ShowLaneShowNotes = !SettingsSystem.RenderSettings.ShowLaneShowNotes;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.LaneHide"]))
        {
            SettingsSystem.RenderSettings.ShowLaneHideNotes = !SettingsSystem.RenderSettings.ShowLaneHideNotes;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.TempoChange"]))
        {
            SettingsSystem.RenderSettings.ShowTempoChangeEvents = !SettingsSystem.RenderSettings.ShowTempoChangeEvents;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.MetreChange"]))
        {
            SettingsSystem.RenderSettings.ShowMetreChangeEvents = !SettingsSystem.RenderSettings.ShowMetreChangeEvents;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SpeedChange"]))
        {
            SettingsSystem.RenderSettings.ShowSpeedChangeEvents = !SettingsSystem.RenderSettings.ShowSpeedChangeEvents;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.VisibilityChange"]))
        {
            SettingsSystem.RenderSettings.ShowVisibilityChangeEvents = !SettingsSystem.RenderSettings.ShowVisibilityChangeEvents;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.ReverseEffect"]))
        {
            SettingsSystem.RenderSettings.ShowReverseEffectEvents = !SettingsSystem.RenderSettings.ShowReverseEffectEvents;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.StopEffect"]))
        {
            SettingsSystem.RenderSettings.ShowStopEffectEvents = !SettingsSystem.RenderSettings.ShowStopEffectEvents;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.TutorialMarker"]))
        {
            SettingsSystem.RenderSettings.ShowTutorialMarkerEvents = !SettingsSystem.RenderSettings.ShowTutorialMarkerEvents;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.HideDuringPlayback.EventMarkers"]))
        {
            SettingsSystem.RenderSettings.HideEventMarkersDuringPlayback = !SettingsSystem.RenderSettings.HideEventMarkersDuringPlayback;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.HideDuringPlayback.LaneToggleNotes"]))
        {
            SettingsSystem.RenderSettings.HideLaneToggleNotesDuringPlayback = !SettingsSystem.RenderSettings.HideLaneToggleNotesDuringPlayback;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.HideDuringPlayback.HoldControlPoints"]))
        {
            SettingsSystem.RenderSettings.HideHoldControlPointsDuringPlayback = !SettingsSystem.RenderSettings.HideHoldControlPointsDuringPlayback;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.HideDuringPlayback.Bookmarks"]))
        {
            SettingsSystem.RenderSettings.HideBookmarksDuringPlayback = !SettingsSystem.RenderSettings.HideBookmarksDuringPlayback;
            e.Handled = true;
        }
    }

    private void Control_OnKeyUp(object? sender, KeyEventArgs e) => e.Handled = true;
    
    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        RenderCanvas.Width = PanelCanvasContainer.Bounds.Width;
        RenderCanvas.Height = PanelCanvasContainer.Bounds.Height;

        canvasInfo.Width = (float)RenderCanvas.Width;
        canvasInfo.Height = (float)RenderCanvas.Height;
        canvasInfo.Radius = canvasInfo.Width / 2;
        canvasInfo.Center = new(canvasInfo.Radius, canvasInfo.Radius);
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

        Renderer2D.Render
        (
            canvas: canvas,
            canvasInfo: canvasInfo,
            settings: SettingsSystem.RenderSettings,
            chart: ChartSystem.Chart,
            time: TimeSystem.Timestamp.Time,
            playing: playing,
            zoomLevel: zoomLevel,
            selectedObjects: SelectionSystem.SelectedObjects,
            pointerOverObject: objectDrag.IsActive ? null : SelectionSystem.PointerOverObject,
            activeObjectGroup: EditorSystem.ActiveObjectGroup,
            boxSelect: new(SelectionSystem.BoxSelectArgs.GlobalStartTime, SelectionSystem.BoxSelectArgs.GlobalEndTime, SelectionSystem.BoxSelectArgs.Position, SelectionSystem.BoxSelectArgs.Size),
            cursorNote: CursorSystem.CurrentType
        );

        if (playing && clickDragLeft.IsActive && !objectDrag.IsActive)
        {
            _ = Task.Run(() =>
            {
                float depth = Renderer2D.GetHitTestPointerDepth(canvasInfo, (float)clickDragLeft.EndPoint!.Value.Position.Y);
                float viewDistance = Renderer2D.GetViewDistance(zoomLevel, canvasInfo);
                float viewTime = SaturnMath.Lerp(viewDistance, 0, depth);
                
                SelectionSystem.SetBoxSelectionEnd(clickDragLeft.Position, clickDragLeft.Size, viewTime);
            });
        }
    }

    private void RenderCanvas_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        PointerPoint point = e.GetCurrentPoint(sender as Control);
        pointerPosition = point.Position;
        
        _ = Task.Run(() =>
        {
            float depth = Renderer2D.GetHitTestPointerDepth(canvasInfo, (float)point.Position.Y);
            int lane = Renderer2D.GetHitTestPointerLane(canvasInfo, (float)point.Position.X);
            float viewDistance = Renderer2D.GetViewDistance(zoomLevel, canvasInfo);

            FindPointerOverObject(depth, lane, viewDistance);
            SetCursorType();
            
            onLeftDrag();
            onRightDrag();

            return;
            
            void onLeftDrag()
            {
                if (!e.Properties.IsLeftButtonPressed) return;

                clickDragLeft.EndPoint = point;
                clickDragLeft.EndLane = lane;
                
                if (!clickDragLeft.IsActive) return;
                
                if (objectDrag.IsActive)
                {
                    float time = TimeSystem.Timestamp.Time + SaturnMath.Lerp(viewDistance, 0, depth);
        
                    Task.Run(() => objectDrag.Update(time));
                }
                else
                {
                    // Box Select
                    float viewTime = SaturnMath.Lerp(viewDistance, 0, depth);

                    Task.Run(() => SelectionSystem.SetBoxSelectionEnd(clickDragLeft.Position, clickDragLeft.Size, viewTime));
                }
            }

            void onRightDrag()
            {
                if (!e.Properties.IsRightButtonPressed) return;

                clickDragRight.EndPoint = point;
                clickDragRight.EndLane = lane;
                
                if (!clickDragRight.IsActive) return;
                
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
            float depth = Renderer2D.GetHitTestPointerDepth(canvasInfo, (float)point.Position.Y);
            int lane = Renderer2D.GetHitTestPointerLane(canvasInfo, (float)point.Position.X);
            float viewDistance = Renderer2D.GetViewDistance(zoomLevel, canvasInfo);

            onLeftClick();
            onRightClick();
                
            return;
                
            void onLeftClick()
            {
                if (!e.Properties.IsLeftButtonPressed) return;
                clickDragLeft.Reset(point, point, lane);

                float viewTime = SaturnMath.Lerp(viewDistance, 0, depth);
                
                boxSelect();
                normalSelect();
                
                int fullTick = Timestamp.TimestampFromTime(ChartSystem.Chart, TimeSystem.Timestamp.Time + viewTime).FullTick;
                float m = 1920.0f / TimeSystem.Division;
                fullTick = (int)(Math.Round(fullTick / m) * m);
                
                objectDrag.Start(fullTick);

                return;
                    
                void boxSelect()
                {
                    SelectionSystem.SetBoxSelectionStart(
                        negativeSelection: e.KeyModifiers.HasFlag(KeyModifiers.Alt),
                        viewTime: viewTime);
                }

                void normalSelect()
                {
                    bool control = e.KeyModifiers.HasFlag(KeyModifiers.Control);
                    bool shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
                    
                    if (SelectionSystem.PointerOverObject != null && SelectionSystem.SelectedObjects.Contains(SelectionSystem.PointerOverObject))
                    {
                        // When clicking a selected object, only re-run selection on pointer released.
                        lastClickedObject = SelectionSystem.PointerOverObject;
                        return;
                    }
                    
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
                
                SelectionSystem.AttemptBoxSelection();
                
                if ((objectDrag.IsActive == false || clickDragLeft.IsActive == false)
                    && SelectionSystem.PointerOverObject != null 
                    && SelectionSystem.PointerOverObject == lastClickedObject
                    && SelectionSystem.SelectedObjects.Contains(SelectionSystem.PointerOverObject))
                {
                    bool control = e.KeyModifiers.HasFlag(KeyModifiers.Control);
                    bool shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
                    
                    // When clicking a selected object, only re-run selection on pointer released.
                    SelectionSystem.SetSelection(control, shift);
                }
                
                objectDrag.End();
                
                lastClickedObject = null;
                clickDragLeft.Reset(null, null, 0);
                
                SetCursorType();
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
        pointerPosition = null;
    }

    private void RenderCanvas_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        onScrollUp();
        onScrollDown();

        return;

        void onScrollUp()
        {
            if (e.Delta.Y <= 0) return;

            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                zoomLevel = Math.Clamp(zoomLevel - 1, 1, 20);
            }
            else
            {
                TimeSystem.SeekFullTick(TimeSystem.Timestamp.FullTick + TimeSystem.DivisionInterval);
            }
        }

        void onScrollDown()
        {
            if (e.Delta.Y >= 0) return;
            
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                zoomLevel = Math.Clamp(zoomLevel + 1, 1, 20);
            }
            else
            {
                TimeSystem.SeekFullTick(Math.Max(0, TimeSystem.Timestamp.FullTick - TimeSystem.DivisionInterval));
            }
        }
    }

    private async void RenderCanvas_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            if (MainWindow.Instance == null) return;
            if (SelectionSystem.PointerOverObject == null) return;
            if (!SelectionSystem.SelectedObjects.Contains(SelectionSystem.PointerOverObject)) return;

            if (SelectionSystem.PointerOverObject is TempoChangeEvent tempoChangeEvent)
            {
                float? tempo = await MainWindow.Instance.ChartEditor.ShowTempoDialog(tempoChangeEvent.Tempo);
                if (tempo == null) return;
                
                UndoRedoSystem.ChartBranch.Push(new TempoChangeEditOperation(tempoChangeEvent, tempoChangeEvent.Tempo, tempo.Value));
            }
            else if (SelectionSystem.PointerOverObject is MetreChangeEvent metreChangeEvent)
            {
                (int?, int?) metre = await MainWindow.Instance.ChartEditor.ShowMetreDialog(metreChangeEvent.Upper, metreChangeEvent.Lower);
                if (metre.Item1 == null || metre.Item2 == null) return;
                
                UndoRedoSystem.ChartBranch.Push(new MetreChangeEditOperation(metreChangeEvent, metreChangeEvent.Upper, metre.Item1.Value, metreChangeEvent.Lower, metre.Item2.Value));
            }
            else if (SelectionSystem.PointerOverObject is TutorialMarkerEvent tutorialMarkerEvent)
            {
                string? key = await MainWindow.Instance.ChartEditor.ShowTutorialMarkerDialog(tutorialMarkerEvent.Key);
                if (key == null) return;
                
                UndoRedoSystem.ChartBranch.Push(new TutorialMarkerEditOperation(tutorialMarkerEvent, tutorialMarkerEvent.Key, key));
            }
            else if (SelectionSystem.PointerOverObject is SpeedChangeEvent speedChangeEvent)
            {
                float? speed = await MainWindow.Instance.ChartEditor.ShowSpeedDialog(speedChangeEvent.Speed);
                if (speed == null) return;
                
                UndoRedoSystem.ChartBranch.Push(new SpeedChangeEditOperation(speedChangeEvent, speedChangeEvent.Speed, speed.Value));
            }
            else if (SelectionSystem.PointerOverObject is VisibilityChangeEvent visibilityChangeEvent)
            {
                bool? visibility = await MainWindow.Instance.ChartEditor.ShowVisibilityDialog(visibilityChangeEvent.Visibility);
                if (visibility == null) return;
                
                UndoRedoSystem.ChartBranch.Push(new VisibilityChangeEditOperation(visibilityChangeEvent, visibilityChangeEvent.Visibility, visibility.Value));
            }
            else if (SelectionSystem.PointerOverObject is Bookmark bookmark)
            { 
                (string?, uint?) data = await MainWindow.Instance.ChartEditor.ShowBookmarkDialog(bookmark.Message, bookmark.Color);
                if (data.Item1 == null || data.Item2 == null) return;
                
                UndoRedoSystem.ChartBranch.Push(new BookmarkEditOperation(bookmark, bookmark.Color, data.Item2.Value, bookmark.Message, data.Item1));
            }
            else if (SelectionSystem.PointerOverObject is HoldNote holdNote)
            {
                EditorSystem.ChangeEditMode(EditorMode.EditMode, holdNote);
            }
            else if (SelectionSystem.PointerOverObject is StopEffectEvent stopEffectEvent)
            {
                EditorSystem.ChangeEditMode(EditorMode.EditMode, stopEffectEvent);
            }
            else if (SelectionSystem.PointerOverObject is ReverseEffectEvent reverseEffectEvent)
            {
                EditorSystem.ChangeEditMode(EditorMode.EditMode, reverseEffectEvent);
            }
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
    }

    
    private void ComboBoxEditMode_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxEditMode == null) return;

        Task.Run(() => EditorSystem.ChangeEditMode((EditorMode)ComboBoxEditMode.SelectedIndex));
    }
    
    private void MenuItemSettings_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not MenuItem menuItem) return;
        
        if (menuItem == MenuItemShowJudgeAreas)
        {
            SettingsSystem.RenderSettings.ShowJudgeAreas = menuItem.IsChecked;

            MenuItemShowMarvelousArea.IsEnabled = MenuItemShowJudgeAreas.IsChecked;
            MenuItemShowGreatArea.IsEnabled = MenuItemShowJudgeAreas.IsChecked;
            MenuItemShowGoodArea.IsEnabled = MenuItemShowJudgeAreas.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowMarvelousArea)
        {
            SettingsSystem.RenderSettings.ShowMarvelousArea = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowGreatArea)
        {
            SettingsSystem.RenderSettings.ShowGreatArea = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowGoodArea)
        {
            SettingsSystem.RenderSettings.ShowGoodArea = menuItem.IsChecked;
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

    
    private void MenuItemAddTempoChange_OnClick(object? sender, RoutedEventArgs e) => MainWindow.Instance?.ChartEditor.ChartView_AddTempoChangeEvent();

    private void MenuItemAddMetreChange_OnClick(object? sender, RoutedEventArgs e) => MainWindow.Instance?.ChartEditor.ChartView_AddMetreChangeEvent();

    private void MenuItemAddTutorialMarker_OnClick(object? sender, RoutedEventArgs e) => MainWindow.Instance?.ChartEditor.ChartView_AddTutorialMarkerEvent();

    private void MenuItemAddSpeedChange_OnClick(object? sender, RoutedEventArgs e) => MainWindow.Instance?.ChartEditor.ChartView_AddSpeedChangeEvent();

    private void MenuItemAddVisibilityChange_OnClick(object? sender, RoutedEventArgs e) => MainWindow.Instance?.ChartEditor.ChartView_AddVisibilityChangeEvent();

    private void MenuItemAddStopEffect_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Insert_AddStopEffect);

    private void MenuItemAddReverseEffect_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Insert_AddReverseEffect);

    private void MenuItemAddBookmark_OnClick(object? sender, RoutedEventArgs e) => MainWindow.Instance?.ChartEditor.ChartView_AddBookmark();
    
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

    private void MenuItemAdjustAxis_OnClick(object? sender, RoutedEventArgs e) => MainWindow.Instance?.ChartEditor.ChartView_AdjustAxis();

    private void MenuItemFlipDirection_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_FlipDirection);

    private void MenuItemReverseSelection_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Transform_ReverseSelection);

    private void MenuItemScaleSelection_OnClick(object? sender, RoutedEventArgs e) => MainWindow.Instance?.ChartEditor.ChartView_ScaleSelection();

    private void MenuItemOffsetChart_OnClick(object? sender, RoutedEventArgs e) => MainWindow.Instance?.ChartEditor.ChartView_OffsetChart();

    private void MenuItemScaleChart_OnClick(object? sender, RoutedEventArgs e) => MainWindow.Instance?.ChartEditor.ChartView_ScaleChart();

    private void MenuItemMirrorChart_OnClick(object? sender, RoutedEventArgs e) => MainWindow.Instance?.ChartEditor.ChartView_MirrorChart();

    private void MenuItemZigZagHold_OnClick(object? sender, RoutedEventArgs e) => MainWindow.Instance?.ChartEditor.ChartView_ZigZagHold();

    private void MenuItemCutHold_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Convert_CutHold);

    private void MenuItemJoinHold_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.Convert_JoinHold);
    
    
    private void ButtonInsert_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.ToolBar_Insert);

    private void ButtonDelete_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.ToolBar_Delete);

    private void ButtonEditShape_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.ToolBar_EditShape);

    private void ButtonEditType_OnClick(object? sender, RoutedEventArgs e) => Task.Run(EditorSystem.ToolBar_EditType);
#endregion UI Event Handlers
}