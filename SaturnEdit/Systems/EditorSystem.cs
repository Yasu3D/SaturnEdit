using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input.Platform;
using SaturnData.Notation.Core;
using SaturnData.Notation.Events;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnData.Notation.Serialization;
using SaturnData.Utilities;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.BookmarkOperations;
using SaturnEdit.UndoRedo.EditModeOperations;
using SaturnEdit.UndoRedo.EventOperations;
using SaturnEdit.UndoRedo.HoldNoteOperations;
using SaturnEdit.UndoRedo.LayerOperations;
using SaturnEdit.UndoRedo.NoteOperations;
using SaturnEdit.UndoRedo.PositionableOperations;
using SaturnEdit.UndoRedo.SelectionOperations;
using SaturnEdit.UndoRedo.TimeableOperations;

namespace SaturnEdit.Systems;

public enum EditorMode
{
    ObjectMode = 0,
    EditMode = 1,
}

public static class EditorSystem
{
    public static void Initialize()
    {
        ChartSystem.ChartLoaded += OnChartLoaded;
        OnChartLoaded(null, EventArgs.Empty);
    }

    public static event EventHandler? EditModeChangeAttempted;
    
    public static int MirrorAxis { get; set; } = 0;
    public static EditorMode Mode { get; set; } = EditorMode.ObjectMode;
    public static ITimeable? ActiveObjectGroup { get; set; } = null;

    public static bool EditModeAvailable => SelectionSystem.SelectedObjects.Any(x => x is HoldNote or StopEffectEvent or ReverseEffectEvent);

#region Methods
    public static void ChangeEditMode()
    {
        if (Mode == EditorMode.EditMode)
        {
            ChangeEditMode(EditorMode.ObjectMode);
            return;
        }
        
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is not (HoldNote or StopEffectEvent or ReverseEffectEvent)) continue;
            
            ChangeEditMode(EditorMode.EditMode);
            return;
        }
    }
    
    public static void ChangeEditMode(EditorMode newMode)
    {
        EditModeChangeAttempted?.Invoke(null, EventArgs.Empty);
        
        CompositeOperation? op = GetEditModeChangeOperation(newMode, null, null);
        if (op == null || op.Operations.Count == 0) return;

        UndoRedoSystem.ChartBranch.Push(op);
    }

    public static void ChangeEditMode(EditorMode newMode, ITimeable? newActiveObjectGroup)
    {
        EditModeChangeAttempted?.Invoke(null, EventArgs.Empty);
        
        CompositeOperation? op = GetEditModeChangeOperation(newMode, newActiveObjectGroup, null);
        if (op == null || op.Operations.Count == 0) return;

        UndoRedoSystem.ChartBranch.Push(op);
    }
    
    public static CompositeOperation? GetEditModeChangeOperation(EditorMode newMode, ITimeable? newActiveObjectGroup, Note? newType)
    {
        if (Mode == newMode) return null;
        
        List<IOperation> operations = [];
        List<ITimeable> objects = SelectionSystem.OrderedSelectedObjects;
        
        // Changing to Object Mode:
        if (newMode is EditorMode.ObjectMode)
        {
            // Delete edited Hold Note if it has less than 2 points.
            if (Mode == EditorMode.EditMode && ActiveObjectGroup is HoldNote holdNote && holdNote.Points.Count < 2)
            {
                Layer? layer = ChartSystem.Chart.ParentLayer(holdNote);

                if (layer != null)
                {
                    int index = layer.Notes.IndexOf(holdNote);
                    operations.Add(new NoteRemoveOperation(layer, holdNote, index));
                }
            }
            
            // Clear selection.
            foreach (ITimeable obj in objects)
            {
                operations.Add(new SelectionRemoveOperation(obj, SelectionSystem.LastSelectedObject));
            }
            
            // Re-select the previously active object group.
            if (ActiveObjectGroup != null)
            {
                operations.Add(new SelectionAddOperation(ActiveObjectGroup, SelectionSystem.LastSelectedObject));
            }

            // Change cursor to a different type again.
            if (ActiveObjectGroup is HoldNote)
            {
                operations.Add(new CursorTypeChangeOperation(CursorSystem.CurrentType, newType ?? CursorSystem.HoldNote));
            }
        }
        
        // Changing to Edit Mode:
        else if (newMode is EditorMode.EditMode)
        {
            if (newActiveObjectGroup == null)
            {
                // Find the earliest selected object group.
                HoldNote? holdNote = null;
                StopEffectEvent? stopEffectEvent = null;
                ReverseEffectEvent? reverseEffectEvent = null;

                foreach (ITimeable obj in objects)
                {
                    if (obj is HoldNote h)
                    {
                        holdNote ??= h;
                    }

                    if (obj is StopEffectEvent s)
                    {
                        stopEffectEvent ??= s;
                    }
                    else if (obj is ReverseEffectEvent r)
                    {
                        reverseEffectEvent ??= r;
                    }

                    if (holdNote != null && stopEffectEvent != null && reverseEffectEvent != null) break;
                }

                // Cancel edit mode change if no object group is found.
                if (holdNote == null && stopEffectEvent == null && reverseEffectEvent == null) return null;

                // Make the earliest object the new active object.
                ITimeable? min = null;
                if (holdNote != null)
                {
                    min ??= holdNote;
                    min = holdNote.Timestamp.FullTick < min.Timestamp.FullTick ? holdNote : min;
                }

                if (stopEffectEvent != null)
                {
                    min ??= stopEffectEvent;
                    min = stopEffectEvent.Timestamp.FullTick < min.Timestamp.FullTick ? stopEffectEvent : min;
                }

                if (reverseEffectEvent != null)
                {
                    min ??= reverseEffectEvent;
                    min = reverseEffectEvent.Timestamp.FullTick < min.Timestamp.FullTick ? reverseEffectEvent : min;
                }

                newActiveObjectGroup = min;
            }

            // Switch to inserting hold point notes.
            if (newActiveObjectGroup is HoldNote)
            {
                operations.Add(new CursorTypeChangeOperation(CursorSystem.CurrentType, CursorSystem.HoldPointNote));
            }
            
            // Clear selection.
            foreach (ITimeable obj in objects)
            {
                operations.Add(new SelectionRemoveOperation(obj, SelectionSystem.LastSelectedObject));
            }
        }
        
        operations.Add(new EditorModeChangeOperation(Mode, newMode, ActiveObjectGroup, newActiveObjectGroup));
        
        return new(operations);
    }

    
    public static void ToolBar_Insert()
    {
        List<IOperation> operations = [];
        
        if (Mode == EditorMode.ObjectMode)
        {
            Note? note = null;

            Timestamp timestamp = new(TimeSystem.Timestamp.FullTick);
            
            if (CursorSystem.CurrentType is TouchNote)
            {
                note = new TouchNote
                (
                    timestamp:     timestamp,
                    position:      CursorSystem.Position,
                    size:          CursorSystem.Size,
                    bonusType:     CursorSystem.BonusType,
                    judgementType: CursorSystem.JudgementType
                );
            }
            else if (CursorSystem.CurrentType is ChainNote)
            {
                note = new ChainNote
                (
                    timestamp:     timestamp,
                    position:      CursorSystem.Position,
                    size:          CursorSystem.Size,
                    bonusType:     CursorSystem.BonusType,
                    judgementType: CursorSystem.JudgementType
                );
            }
            else if (CursorSystem.CurrentType is HoldNote)
            {
                note = new HoldNote
                (
                    bonusType:     CursorSystem.BonusType,
                    judgementType: CursorSystem.JudgementType
                );

                HoldNote parent = (HoldNote)note;

                HoldPointNote point = new
                (
                    timestamp:  timestamp,
                    position:   CursorSystem.Position,
                    size:       CursorSystem.Size,
                    parent:     parent,
                    renderType: HoldPointRenderType.Visible
                );
                
                parent.Points.Add(point);
                
                operations.Add(new CursorTypeChangeOperation(CursorSystem.CurrentType, CursorSystem.HoldPointNote));
                operations.Add(new EditorModeChangeOperation(Mode, EditorMode.EditMode, ActiveObjectGroup, parent));
            }
            else if (CursorSystem.CurrentType is SlideClockwiseNote)
            {
                note = new SlideClockwiseNote
                (
                    timestamp:     timestamp,
                    position:      CursorSystem.Position,
                    size:          CursorSystem.Size,
                    bonusType:     CursorSystem.BonusType,
                    judgementType: CursorSystem.JudgementType
                );
            }
            else if (CursorSystem.CurrentType is SlideCounterclockwiseNote)
            {
                note = new SlideCounterclockwiseNote
                (
                    timestamp:     timestamp,
                    position:      CursorSystem.Position,
                    size:          CursorSystem.Size,
                    bonusType:     CursorSystem.BonusType,
                    judgementType: CursorSystem.JudgementType
                );
            }
            else if (CursorSystem.CurrentType is SnapForwardNote)
            {
                note = new SnapForwardNote
                (
                    timestamp:     timestamp,
                    position:      CursorSystem.Position,
                    size:          CursorSystem.Size,
                    bonusType:     CursorSystem.BonusType,
                    judgementType: CursorSystem.JudgementType
                );
            }
            else if (CursorSystem.CurrentType is SnapBackwardNote)
            {
                note = new SnapBackwardNote
                (
                    timestamp:     timestamp,
                    position:      CursorSystem.Position,
                    size:          CursorSystem.Size,
                    bonusType:     CursorSystem.BonusType,
                    judgementType: CursorSystem.JudgementType
                );
            }
            else if (CursorSystem.CurrentType is LaneShowNote)
            {
                note = new LaneShowNote
                (
                    timestamp: timestamp,
                    position:  CursorSystem.Position,
                    size:      CursorSystem.Size,
                    direction: CursorSystem.Direction
                );
            }
            else if (CursorSystem.CurrentType is LaneHideNote)
            {
                note = new LaneHideNote
                (
                    timestamp: timestamp,
                    position:  CursorSystem.Position,
                    size:      CursorSystem.Size,
                    direction: CursorSystem.Direction
                );
            }
            else if (CursorSystem.CurrentType is SyncNote)
            {
                note = new SyncNote
                (
                    timestamp: timestamp,
                    position:  CursorSystem.Position,
                    size:      CursorSystem.Size
                );
            }
            else if (CursorSystem.CurrentType is MeasureLineNote)
            {
                note = new MeasureLineNote
                (
                    timestamp: timestamp,
                    isBeatLine: false
                );
            }

            if (note == null) return;

            if (note is ILaneToggle)
            {
                operations.Add(new LaneToggleAddOperation(note, ChartSystem.Chart.LaneToggles.Count));
            }
            else if (SelectionSystem.SelectedLayer != null)
            {
                operations.Add(new NoteAddOperation(SelectionSystem.SelectedLayer, note, SelectionSystem.SelectedLayer.Notes.Count));
            }
        }
        else if (Mode == EditorMode.EditMode && ActiveObjectGroup is HoldNote holdNote)
        {
            HoldPointNote point = new
            (
                timestamp:  new(TimeSystem.Timestamp.FullTick),
                position:   CursorSystem.Position,
                size:       CursorSystem.Size,
                parent:     holdNote,
                renderType: CursorSystem.RenderType
            );
            
            operations.Add(new HoldPointNoteAddOperation(holdNote, point, holdNote.Points.Count));
        }
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
    }

    public static void ToolBar_Delete()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];

        for (int i = 0; i < ChartSystem.Chart.Bookmarks.Count; i++)
        {
            Bookmark bookmark = ChartSystem.Chart.Bookmarks[i];
            if (!SelectionSystem.SelectedObjects.Contains(bookmark)) continue;

            operations.Add(new SelectionRemoveOperation(bookmark, SelectionSystem.LastSelectedObject));
            operations.Add(new BookmarkRemoveOperation(bookmark, i));
        }
        
        for (int i = 0; i < ChartSystem.Chart.Events.Count; i++)
        {
            Event @event  = ChartSystem.Chart.Events[i];
            if (!SelectionSystem.SelectedObjects.Contains(@event)) continue;
            
            operations.Add(new SelectionRemoveOperation(@event, SelectionSystem.LastSelectedObject));
            operations.Add(new GlobalEventRemoveOperation(@event, i));
        }
        
        for (int i = 0; i < ChartSystem.Chart.LaneToggles.Count; i++)
        {
            Note laneToggle = ChartSystem.Chart.LaneToggles[i];
            if (!SelectionSystem.SelectedObjects.Contains(laneToggle)) continue;
            
            operations.Add(new SelectionRemoveOperation(laneToggle, SelectionSystem.LastSelectedObject));
            operations.Add(new LaneToggleRemoveOperation(laneToggle, i));
        }

        foreach (Layer layer in ChartSystem.Chart.Layers)
        {
            for (int i = 0; i < layer.Events.Count; i++)
            {
                Event @event = layer.Events[i];
                if (!SelectionSystem.SelectedObjects.Contains(@event)) continue;
                
                operations.Add(new SelectionRemoveOperation(@event, SelectionSystem.LastSelectedObject));
                operations.Add(new EventRemoveOperation(layer, @event, i));
            }
            
            for (int i = 0; i < layer.Notes.Count; i++)
            {
                Note note = layer.Notes[i];

                if (note is HoldNote holdNote)
                {
                    for (int j = 0; j < holdNote.Points.Count; j++)
                    {
                        HoldPointNote point = holdNote.Points[j];
                        if (!SelectionSystem.SelectedObjects.Contains(point)) continue;

                        operations.Add(new SelectionRemoveOperation(point, SelectionSystem.LastSelectedObject));
                        operations.Add(new HoldPointNoteRemoveOperation(holdNote, point, j));
                    }
                }
                
                if (!SelectionSystem.SelectedObjects.Contains(note)) continue;
                
                operations.Add(new SelectionRemoveOperation(note, SelectionSystem.LastSelectedObject));
                operations.Add(new NoteRemoveOperation(layer, note, i));
            }
        }
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
    }

    public static void ToolBar_EditShape()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        
        List<IOperation> operations = [];

        foreach (ITimeable obj in SelectionSystem.SelectedObjects)
        {
            if (obj is not IPositionable positionable) continue;
            if (positionable.Position == CursorSystem.Position && positionable.Size == CursorSystem.Size) continue;
            
            operations.Add(new PositionableEditOperation(positionable, positionable.Position, CursorSystem.Position, positionable.Size, CursorSystem.Size));
        }
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
    }

    public static void ToolBar_EditType()
    {
        if (Mode == EditorMode.EditMode) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        if (ChartSystem.Chart.Layers.Count == 0) return;
        
        List<IOperation> operations = [];
        List<ITimeable> objects = SelectionSystem.OrderedSelectedObjects;

        if (CursorSystem.CurrentType is HoldNote)
        {
            // Convert to Hold Note
            if (SelectionSystem.SelectedLayer == null) return;
            
            BonusType bonusType = CursorSystem.BonusType;
            JudgementType judgementType = CursorSystem.JudgementType;
            
            HoldNote holdNote = new(bonusType, judgementType);
            foreach (ITimeable obj in objects)
            {
                Layer layer = ChartSystem.Chart.ParentLayer(obj) ?? ChartSystem.Chart.Layers[0];
                
                // Remove original object.
                removeObject(obj, layer);

                if (obj is HoldNote sourceHoldNote)
                {
                    foreach (HoldPointNote point in sourceHoldNote.Points)
                    {
                        HoldPointNote newHoldPoint = new(new(point.Timestamp.FullTick), point.Position, point.Size, holdNote, point.RenderType);
                        holdNote.Points.Add(newHoldPoint);
                    }
                }
                else if (obj is StopEffectEvent sourceStopEffectEvent)
                {
                    foreach (EffectSubEvent subEvent in sourceStopEffectEvent.SubEvents)
                    {
                        holdNote.Points.Add(new(new(subEvent.Timestamp.FullTick), 0, 60, holdNote, HoldPointRenderType.Visible));
                    }
                }
                else if (obj is ReverseEffectEvent sourceReverseEffectEvent)
                {
                    foreach (EffectSubEvent subEvent in sourceReverseEffectEvent.SubEvents)
                    {
                        holdNote.Points.Add(new(new(subEvent.Timestamp.FullTick), 0, 60, holdNote, HoldPointRenderType.Visible));
                    }
                }
                else
                {
                    int position = 0;
                    int size = 60;
                    HoldPointRenderType renderType = HoldPointRenderType.Visible;

                    if (obj is IPositionable positionable)
                    {
                        position = positionable.Position;
                        size = positionable.Size;
                    }

                    if (obj is HoldPointNote holdPointNote)
                    {
                        renderType = holdPointNote.RenderType;
                    }

                    HoldPointNote newHoldPoint = new(new(obj.Timestamp.FullTick), position, size, holdNote, renderType);
                    holdNote.Points.Add(newHoldPoint);
                }
            }

            holdNote.Points = holdNote.Points.OrderBy(x => x.Timestamp.FullTick).ToList();

            if (holdNote.Points.Count < 2) return;
            
            if (!objects.Any(x => x is HoldPointNote))
            {
                operations.Add(new SelectionAddOperation(holdNote, SelectionSystem.LastSelectedObject));
            }

            operations.Add(new NoteAddOperation(SelectionSystem.SelectedLayer, holdNote, 0));
        }
        else
        {
            // Convert to all other types.
            foreach (ITimeable obj in objects)
            {
                Layer layer = ChartSystem.Chart.ParentLayer(obj) ?? ChartSystem.Chart.Layers[0];
                
                // Remove original object.
                removeObject(obj, layer);
                
                // Add new object(s).
                if (obj is HoldNote holdNote)
                {
                    // Convert all hold note points to new objects.
                    foreach (HoldPointNote point in holdNote.Points)
                    {
                        Timestamp timestamp = new(point.Timestamp.FullTick);
                        ITimeable? newObject = CursorSystem.CurrentType switch
                        {
                            TouchNote => new TouchNote(timestamp, point.Position, point.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            ChainNote => new ChainNote(timestamp, point.Position, point.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            // Hold Note is skipped.
                            SlideClockwiseNote        => new SlideClockwiseNote(timestamp, point.Position, point.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            SlideCounterclockwiseNote => new SlideCounterclockwiseNote(timestamp, point.Position, point.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            SnapForwardNote  => new SnapForwardNote(timestamp, point.Position, point.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            SnapBackwardNote => new SnapBackwardNote(timestamp, point.Position, point.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            LaneShowNote => new LaneShowNote(timestamp, point.Position, point.Size, LaneSweepDirection.Instant),
                            LaneHideNote => new LaneHideNote(timestamp, point.Position, point.Size, LaneSweepDirection.Instant),
                            SyncNote        => new SyncNote(timestamp, point.Position, point.Size),
                            MeasureLineNote => new MeasureLineNote(timestamp, false),
                            _  => null,
                        };

                        if (newObject == null) continue;

                        operations.Add(new SelectionAddOperation(newObject, SelectionSystem.LastSelectedObject));
                        
                        if (newObject is ILaneToggle and Note newLaneToggle)
                        {
                            operations.Add(new LaneToggleAddOperation(newLaneToggle, 0));
                        }
                        else if (newObject is Note newNote)
                        {
                            operations.Add(new NoteAddOperation(layer, newNote, 0));
                        }
                    }
                }
                else
                {
                    // Convert standard object to new object.
                    int position = 0;
                    int size = 60;
                    
                    if (obj is IPositionable positionable)
                    {
                        position = positionable.Position;
                        size = positionable.Size;
                    }
                
                    Timestamp timestamp = new(obj.Timestamp.FullTick);
                    ITimeable? newObject = CursorSystem.CurrentType switch
                    {
                        TouchNote => new TouchNote(timestamp, position, size, CursorSystem.BonusType, CursorSystem.JudgementType),
                        ChainNote => new ChainNote(timestamp, position, size, CursorSystem.BonusType, CursorSystem.JudgementType),
                        // Hold Note is skipped.
                        SlideClockwiseNote        => new SlideClockwiseNote(timestamp, position, size, CursorSystem.BonusType, CursorSystem.JudgementType),
                        SlideCounterclockwiseNote => new SlideCounterclockwiseNote(timestamp, position, size, CursorSystem.BonusType, CursorSystem.JudgementType),
                        SnapForwardNote  => new SnapForwardNote(timestamp, position, size, CursorSystem.BonusType, CursorSystem.JudgementType),
                        SnapBackwardNote => new SnapBackwardNote(timestamp, position, size, CursorSystem.BonusType, CursorSystem.JudgementType),
                        LaneShowNote => new LaneShowNote(timestamp, position, size, CursorSystem.Direction),
                        LaneHideNote => new LaneHideNote(timestamp, position, size, CursorSystem.Direction),
                        SyncNote        => new SyncNote(timestamp, position, size),
                        MeasureLineNote => new MeasureLineNote(timestamp, false),
                        _  => null,
                    };
                    
                    if (newObject == null) continue;
                    
                    operations.Add(new SelectionAddOperation(newObject, SelectionSystem.LastSelectedObject));
                    
                    if (newObject is ILaneToggle and Note newLaneToggle)
                    {
                        operations.Add(new LaneToggleAddOperation(newLaneToggle, 0));
                    }
                    else if (newObject is Note newNote)
                    {
                        operations.Add(new NoteAddOperation(layer, newNote, 0));
                    }
                }
            }
        }

        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
        return;
        
        void removeObject(ITimeable obj, Layer layer)
        {
            operations.Add(new SelectionRemoveOperation(obj, SelectionSystem.LastSelectedObject));
            if (obj is (TempoChangeEvent or MetreChangeEvent or TutorialMarkerEvent) and Event globalEvent)
            {
                int index = ChartSystem.Chart.Events.IndexOf(globalEvent);
                if (index == -1) return;
                
                operations.Add(new GlobalEventRemoveOperation(globalEvent, index));
            }
            else if (obj is ILaneToggle and Note laneToggle)
            {
                int index = ChartSystem.Chart.LaneToggles.IndexOf(laneToggle);
                if (index == -1) return;
                
                operations.Add(new LaneToggleRemoveOperation(laneToggle, index));
            }
            else if (obj is Bookmark bookmark)
            {
                int index = ChartSystem.Chart.Bookmarks.IndexOf(bookmark);
                if (index == -1) return;
                
                operations.Add(new BookmarkRemoveOperation(bookmark, index));
            }
            else if (obj is HoldPointNote holdPointNote)
            {
                int index = holdPointNote.Parent.Points.IndexOf(holdPointNote);
                if (index == -1) return;
                
                operations.Add(new HoldPointNoteRemoveOperation(holdPointNote.Parent, holdPointNote, index));
            }
            else if (obj is Event @event)
            {
                int index = layer.Events.IndexOf(@event);
                if (index == -1) return;
                
                operations.Add(new EventRemoveOperation(layer, @event, index));
            }
            else if (obj is Note note)
            {
                int index = layer.Notes.IndexOf(note);
                if (index == -1) return;
                
                operations.Add(new NoteRemoveOperation(layer, note, index));
            }
        }
    }

    public static void ToolBar_EditBoth()
    {
        if (Mode == EditorMode.EditMode) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        if (ChartSystem.Chart.Layers.Count == 0) return;
        
        List<IOperation> operations = [];
        List<ITimeable> objects = SelectionSystem.OrderedSelectedObjects;

        if (CursorSystem.CurrentType is HoldNote)
        {
            // Convert to Hold Note
            if (SelectionSystem.SelectedLayer == null) return;
            
            BonusType bonusType = CursorSystem.BonusType;
            JudgementType judgementType = CursorSystem.JudgementType;
            
            HoldNote holdNote = new(bonusType, judgementType);
            foreach (ITimeable obj in objects)
            {
                Layer layer = ChartSystem.Chart.ParentLayer(obj) ?? ChartSystem.Chart.Layers[0];
                
                // Remove original object.
                removeObject(obj, layer);

                if (obj is HoldNote sourceHoldNote)
                {
                    foreach (HoldPointNote point in sourceHoldNote.Points)
                    {
                        HoldPointNote newHoldPoint = new(new(point.Timestamp.FullTick), CursorSystem.Position, CursorSystem.Size, holdNote, point.RenderType);
                        holdNote.Points.Add(newHoldPoint);
                    }
                }
                else if (obj is StopEffectEvent sourceStopEffectEvent)
                {
                    foreach (EffectSubEvent subEvent in sourceStopEffectEvent.SubEvents)
                    {
                        holdNote.Points.Add(new(new(subEvent.Timestamp.FullTick), CursorSystem.Position, CursorSystem.Size, holdNote, HoldPointRenderType.Visible));
                    }
                }
                else if (obj is ReverseEffectEvent sourceReverseEffectEvent)
                {
                    foreach (EffectSubEvent subEvent in sourceReverseEffectEvent.SubEvents)
                    {
                        holdNote.Points.Add(new(new(subEvent.Timestamp.FullTick), CursorSystem.Position, CursorSystem.Size, holdNote, HoldPointRenderType.Visible));
                    }
                }
                else
                {
                    HoldPointRenderType renderType = HoldPointRenderType.Visible;

                    if (obj is HoldPointNote holdPointNote)
                    {
                        renderType = holdPointNote.RenderType;
                    }

                    HoldPointNote newHoldPoint = new(new(obj.Timestamp.FullTick), CursorSystem.Position, CursorSystem.Size, holdNote, renderType);
                    holdNote.Points.Add(newHoldPoint);
                }
            }

            holdNote.Points = holdNote.Points.OrderBy(x => x.Timestamp.FullTick).ToList();

            if (holdNote.Points.Count < 2) return;
            
            if (!objects.Any(x => x is HoldPointNote))
            {
                operations.Add(new SelectionAddOperation(holdNote, SelectionSystem.LastSelectedObject));
            }

            operations.Add(new NoteAddOperation(SelectionSystem.SelectedLayer, holdNote, 0));
        }
        else
        {
            // Convert to all other types.
            foreach (ITimeable obj in objects)
            {
                Layer layer = ChartSystem.Chart.ParentLayer(obj) ?? ChartSystem.Chart.Layers[0];
                
                // Remove original object.
                removeObject(obj, layer);
                
                // Add new object(s).
                if (obj is HoldNote holdNote)
                {
                    // Convert all hold note points to new objects.
                    foreach (HoldPointNote point in holdNote.Points)
                    {
                        Timestamp timestamp = new(point.Timestamp.FullTick);
                        ITimeable? newObject = CursorSystem.CurrentType switch
                        {
                            TouchNote => new TouchNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            ChainNote => new ChainNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            // Hold Note is skipped.
                            SlideClockwiseNote        => new SlideClockwiseNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            SlideCounterclockwiseNote => new SlideCounterclockwiseNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            SnapForwardNote  => new SnapForwardNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            SnapBackwardNote => new SnapBackwardNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            LaneShowNote => new LaneShowNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.Direction),
                            LaneHideNote => new LaneHideNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.Direction),
                            SyncNote        => new SyncNote(timestamp, CursorSystem.Position, CursorSystem.Size),
                            MeasureLineNote => new MeasureLineNote(timestamp, false),
                            _  => null,
                        };

                        if (newObject == null) continue;

                        operations.Add(new SelectionAddOperation(newObject, SelectionSystem.LastSelectedObject));
                        
                        if (newObject is ILaneToggle and Note newLaneToggle)
                        {
                            operations.Add(new LaneToggleAddOperation(newLaneToggle, 0));
                        }
                        else if (newObject is Note newNote)
                        {
                            operations.Add(new NoteAddOperation(layer, newNote, 0));
                        }
                    }
                }
                else
                {
                    // Convert standard object to new object.
                    Timestamp timestamp = new(obj.Timestamp.FullTick);
                    ITimeable? newObject = CursorSystem.CurrentType switch
                    {
                        TouchNote => new TouchNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                        ChainNote => new ChainNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                        // Hold Note is skipped.
                        SlideClockwiseNote        => new SlideClockwiseNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                        SlideCounterclockwiseNote => new SlideCounterclockwiseNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                        SnapForwardNote  => new SnapForwardNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                        SnapBackwardNote => new SnapBackwardNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                        LaneShowNote => new LaneShowNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.Direction),
                        LaneHideNote => new LaneHideNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.Direction),
                        SyncNote        => new SyncNote(timestamp, CursorSystem.Position, CursorSystem.Size),
                        MeasureLineNote => new MeasureLineNote(timestamp, false),
                        _  => null,
                    };
                    
                    if (newObject == null) continue;
                    
                    operations.Add(new SelectionAddOperation(newObject, SelectionSystem.LastSelectedObject));
                    
                    if (newObject is ILaneToggle and Note newLaneToggle)
                    {
                        operations.Add(new LaneToggleAddOperation(newLaneToggle, 0));
                    }
                    else if (newObject is Note newNote)
                    {
                        operations.Add(new NoteAddOperation(layer, newNote, 0));
                    }
                }
            }
        }

        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
        return;
        
        void removeObject(ITimeable obj, Layer layer)
        {
            operations.Add(new SelectionRemoveOperation(obj, SelectionSystem.LastSelectedObject));
            if (obj is (TempoChangeEvent or MetreChangeEvent or TutorialMarkerEvent) and Event globalEvent)
            {
                int index = ChartSystem.Chart.Events.IndexOf(globalEvent);
                if (index == -1) return;
                
                operations.Add(new GlobalEventRemoveOperation(globalEvent, index));
            }
            else if (obj is ILaneToggle and Note laneToggle)
            {
                int index = ChartSystem.Chart.LaneToggles.IndexOf(laneToggle);
                if (index == -1) return;
                
                operations.Add(new LaneToggleRemoveOperation(laneToggle, index));
            }
            else if (obj is Bookmark bookmark)
            {
                int index = ChartSystem.Chart.Bookmarks.IndexOf(bookmark);
                if (index == -1) return;
                
                operations.Add(new BookmarkRemoveOperation(bookmark, index));
            }
            else if (obj is HoldPointNote holdPointNote)
            {
                int index = holdPointNote.Parent.Points.IndexOf(holdPointNote);
                if (index == -1) return;
                
                operations.Add(new HoldPointNoteRemoveOperation(holdPointNote.Parent, holdPointNote, index));
            }
            else if (obj is Event @event)
            {
                int index = layer.Events.IndexOf(@event);
                if (index == -1) return;
                
                operations.Add(new EventRemoveOperation(layer, @event, index));
            }
            else if (obj is Note note)
            {
                int index = layer.Notes.IndexOf(note);
                if (index == -1) return;
                
                operations.Add(new NoteRemoveOperation(layer, note, index));
            }
        }
    }
    
    
    public static async Task Edit_Cut(IClipboard clipboard)
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        await Edit_Copy(clipboard, false);
        ToolBar_Delete();
    }

    public static async Task Edit_Copy(IClipboard clipboard, bool deselect)
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<ITimeable> objects = SelectionSystem.OrderedSelectedObjects;

        int startFullTick = objects[0].Timestamp.FullTick;
        Chart chart = new() { Layers = [new("Clipboard")] };

        if (Mode == EditorMode.ObjectMode)
        {
            foreach (ITimeable obj in objects)
            {
                if (obj is not ICloneable cloneable) continue;

                object clone = cloneable.Clone();

                if (clone is HoldNote holdNote)
                {
                    foreach (HoldPointNote point in holdNote.Points)
                    {
                        point.Timestamp.FullTick -= startFullTick;
                    }
                }
                else if (clone is StopEffectEvent stopEffectEvent)
                {
                    foreach (EffectSubEvent subEvent in stopEffectEvent.SubEvents)
                    {
                        subEvent.Timestamp.FullTick -= startFullTick;
                    }
                }
                else if (clone is ReverseEffectEvent reverseEffectEvent)
                {
                    foreach (EffectSubEvent subEvent in reverseEffectEvent.SubEvents)
                    {
                        subEvent.Timestamp.FullTick -= startFullTick;
                    }
                }
                else
                {
                    ((ITimeable)clone).Timestamp.FullTick -= startFullTick;
                }
                
                if (clone is Bookmark bookmark)
                {
                    chart.Bookmarks.Add(bookmark);
                }
                else if (clone is Event globalEvent && clone is TempoChangeEvent or MetreChangeEvent or TutorialMarkerEvent)
                {
                    chart.Events.Add(globalEvent);
                }
                else if (clone is ILaneToggle laneToggle)
                {
                    chart.LaneToggles.Add((Note)laneToggle);
                }
                else if (clone is Event layerEvent && clone is SpeedChangeEvent or VisibilityChangeEvent or StopEffectEvent or ReverseEffectEvent)
                {
                    chart.Layers[0].Events.Add(layerEvent);
                }
                else if (clone is Note note)
                {
                    chart.Layers[0].Notes.Add(note);
                }
            }
        }
        else if (Mode == EditorMode.EditMode)
        {
            if (ActiveObjectGroup is not HoldNote holdNote) return;

            HoldNote clone = (HoldNote)holdNote.Clone();

            for (int i = holdNote.Points.Count - 1; i >= 0; i--)
            {
                if (!SelectionSystem.SelectedObjects.Contains(holdNote.Points[i]))
                {
                    clone.Points.RemoveAt(i);
                    continue;
                }
                
                clone.Points[i].Timestamp.FullTick -= startFullTick;
            }

            chart.Layers[0].Notes.Add(clone);
        }

        string data = NotationSerializer.ToString(chart, new());
        await clipboard.SetTextAsync(data);

        if (deselect)
        {
            SelectionSystem.DeselectAll();
        }
    }

    public static async Task Edit_Paste(IClipboard clipboard)
    {
        string? clipboardText = await clipboard.TryGetTextAsync();
        if (clipboardText == null) return;

        clipboardText = clipboardText.Replace("\r", "");
        string[] data = clipboardText.Split("\n", StringSplitOptions.RemoveEmptyEntries);

        Chart chart = NotationSerializer.ToChart(data, new(), out List<Exception> exceptions);

        if (exceptions.Count > 0) return;

        List<IOperation> operations = [];

        if (Mode == EditorMode.ObjectMode)
        {
            foreach (Bookmark bookmark in chart.Bookmarks)
            {
                bookmark.Timestamp.FullTick += TimeSystem.Timestamp.FullTick;
                
                operations.Add(new BookmarkAddOperation(bookmark, ChartSystem.Chart.Bookmarks.Count));
                operations.Add(new SelectionAddOperation(bookmark, SelectionSystem.LastSelectedObject));
            }

            foreach (Event globalEvent in chart.Events)
            {
                globalEvent.Timestamp.FullTick += TimeSystem.Timestamp.FullTick;
                
                operations.Add(new GlobalEventAddOperation(globalEvent, ChartSystem.Chart.Events.Count));
                operations.Add(new SelectionAddOperation(globalEvent, SelectionSystem.LastSelectedObject));
            }

            foreach (Note laneToggle in chart.LaneToggles)
            {
                laneToggle.Timestamp.FullTick += TimeSystem.Timestamp.FullTick;
                
                operations.Add(new LaneToggleAddOperation(laneToggle, ChartSystem.Chart.LaneToggles.Count));
                operations.Add(new SelectionAddOperation(laneToggle, SelectionSystem.LastSelectedObject));
            }

            if (SelectionSystem.SelectedLayer != null)
            {
                foreach (Layer layer in chart.Layers)
                {
                    foreach (Event layerEvent in layer.Events)
                    {
                        if (layerEvent is StopEffectEvent stopEffectEvent)
                        {
                            foreach (EffectSubEvent subEvent in stopEffectEvent.SubEvents)
                            {
                                subEvent.Timestamp.FullTick += TimeSystem.Timestamp.FullTick;
                            }
                        }
                        else if (layerEvent is ReverseEffectEvent reverseEffectEvent)
                        {
                            foreach (EffectSubEvent subEvent in reverseEffectEvent.SubEvents)
                            {
                                subEvent.Timestamp.FullTick += TimeSystem.Timestamp.FullTick;
                            }
                        }
                        else
                        {
                            layerEvent.Timestamp.FullTick += TimeSystem.Timestamp.FullTick;
                        }
                        
                        operations.Add(new EventAddOperation(SelectionSystem.SelectedLayer, layerEvent, SelectionSystem.SelectedLayer.Events.Count));
                        operations.Add(new SelectionAddOperation(layerEvent, SelectionSystem.LastSelectedObject));
                    }

                    foreach (Note note in layer.Notes)
                    {
                        if (note is HoldNote holdNote)
                        {
                            if (holdNote.Points.Count < 2) continue;
                            
                            foreach (HoldPointNote point in holdNote.Points)
                            {
                                point.Timestamp.FullTick += TimeSystem.Timestamp.FullTick;
                            }
                        }
                        else
                        {
                            note.Timestamp.FullTick += TimeSystem.Timestamp.FullTick;
                        }
                        
                        operations.Add(new NoteAddOperation(SelectionSystem.SelectedLayer, note, SelectionSystem.SelectedLayer.Notes.Count));
                        operations.Add(new SelectionAddOperation(note, SelectionSystem.LastSelectedObject));
                    }
                }
            }
        }
        else if (Mode == EditorMode.EditMode)
        {
            if (ActiveObjectGroup is not HoldNote holdNote) return;
            if (chart.Layers.Count == 0) return;

            foreach (Note note in chart.Layers[0].Notes)
            {
                if (note is not HoldNote sourceHold) continue;
                
                foreach (HoldPointNote sourcePoint in sourceHold.Points)
                {
                    sourcePoint.Timestamp.FullTick += TimeSystem.Timestamp.FullTick;
                        
                    operations.Add(new HoldPointNoteAddOperation(holdNote, sourcePoint, holdNote.Points.Count));
                    operations.Add(new SelectionAddOperation(sourcePoint, SelectionSystem.LastSelectedObject));
                }
                    
                break;
            }
        }
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
    }


    public static void Insert_AddTempoChange(float tempo)
    {
        TempoChangeEvent tempoChangeEvent = new(new(TimeSystem.Timestamp.FullTick), tempo);

        GlobalEventAddOperation op0 = new(tempoChangeEvent, ChartSystem.Chart.Events.Count);
        SelectionAddOperation op1 = new(tempoChangeEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }

    public static void Insert_AddMetreChange(int upper, int lower)
    {
        MetreChangeEvent metreChangeEvent = new(new(TimeSystem.Timestamp.FullTick), upper, lower);

        GlobalEventAddOperation op0 = new(metreChangeEvent, ChartSystem.Chart.Events.Count);
        SelectionAddOperation op1 = new(metreChangeEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }

    public static void Insert_AddTutorialMarker(string key)
    {
        TutorialMarkerEvent tutorialMarkerEvent = new(new(TimeSystem.Timestamp.FullTick), key);

        GlobalEventAddOperation op0 = new(tutorialMarkerEvent, ChartSystem.Chart.Events.Count);
        SelectionAddOperation op1 = new(tutorialMarkerEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }
    
    public static void Insert_AddSpeedChange(float speed)
    {
        if (SelectionSystem.SelectedLayer == null) return;
        
        SpeedChangeEvent speedChangeEvent = new(new(TimeSystem.Timestamp.FullTick), speed);

        EventAddOperation op0 = new(SelectionSystem.SelectedLayer, speedChangeEvent, SelectionSystem.SelectedLayer.Events.Count);
        SelectionAddOperation op1 = new(speedChangeEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }

    public static void Insert_AddVisibilityChange(bool visibility)
    {
        if (SelectionSystem.SelectedLayer == null) return;
        
        VisibilityChangeEvent visibilityChangeEvent = new(new(TimeSystem.Timestamp.FullTick), visibility);

        EventAddOperation op0 = new(SelectionSystem.SelectedLayer, visibilityChangeEvent, SelectionSystem.SelectedLayer.Events.Count);
        SelectionAddOperation op1 = new(visibilityChangeEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }

    public static void Insert_AddReverseEffect()
    {
        if (SelectionSystem.SelectedLayer == null) return;

        Timestamp start = new(TimeSystem.Timestamp.FullTick);
        Timestamp middle = new(start.FullTick + 1920);
        Timestamp end = new(middle.FullTick + 1920);
        
        ReverseEffectEvent reverseEffectEvent = new();
        reverseEffectEvent.SubEvents[0] = new(start, reverseEffectEvent);
        reverseEffectEvent.SubEvents[1] = new(middle, reverseEffectEvent);
        reverseEffectEvent.SubEvents[2] = new(end, reverseEffectEvent);
        
        EventAddOperation op0 = new(SelectionSystem.SelectedLayer, reverseEffectEvent, SelectionSystem.SelectedLayer.Events.Count);
        SelectionAddOperation op1 = new(reverseEffectEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }

    public static void Insert_AddStopEffect()
    {
        if (SelectionSystem.SelectedLayer == null) return;

        Timestamp start = new(TimeSystem.Timestamp.FullTick);
        Timestamp end = new(start.FullTick + 1920);
        
        StopEffectEvent stopEffectEvent = new();
        stopEffectEvent.SubEvents[0] = new(start, stopEffectEvent);
        stopEffectEvent.SubEvents[1] = new(end, stopEffectEvent);
        
        EventAddOperation op0 = new(SelectionSystem.SelectedLayer, stopEffectEvent, SelectionSystem.SelectedLayer.Events.Count);
        SelectionAddOperation op1 = new(stopEffectEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }

    public static void Insert_AddBookmark(string message, uint color)
    {
        Bookmark bookmark = new(new(TimeSystem.Timestamp.FullTick), color, message);

        BookmarkAddOperation op0 = new(bookmark, ChartSystem.Chart.Bookmarks.Count);
        SelectionAddOperation op1 = new(bookmark, SelectionSystem.LastSelectedObject);

        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }

    
    public static void Transform_MoveSelectionBeatForward()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];

        foreach (ITimeable obj in SelectionSystem.SelectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                foreach (HoldPointNote point in holdNote.Points)
                {
                    addOperation(point);
                }
            }
            else if (obj is StopEffectEvent stopEffectEvent)
            {
                addOperation(stopEffectEvent.SubEvents[0]);
                addOperation(stopEffectEvent.SubEvents[1]);
            }
            else if (obj is ReverseEffectEvent reverseEffectEvent)
            {
                addOperation(reverseEffectEvent.SubEvents[0]);
                addOperation(reverseEffectEvent.SubEvents[1]);
                addOperation(reverseEffectEvent.SubEvents[2]);
            }
            else
            {
                addOperation(obj);
            }
        }

        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));

        return;

        void addOperation(ITimeable obj)
        {
            int oldFullTick = obj.Timestamp.FullTick;
            int newFullTick = oldFullTick + TimeSystem.DivisionInterval;

            newFullTick = Math.Max(0, newFullTick);
            
            operations.Add(new TimeableEditOperation(obj, oldFullTick, newFullTick));
        }
    }

    public static void Transform_MoveSelectionBeatBack()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];

        foreach (ITimeable obj in SelectionSystem.SelectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                foreach (HoldPointNote point in holdNote.Points)
                {
                    addOperation(point);
                }
            }
            else if (obj is StopEffectEvent stopEffectEvent)
            {
                addOperation(stopEffectEvent.SubEvents[0]);
                addOperation(stopEffectEvent.SubEvents[1]);
            }
            else if (obj is ReverseEffectEvent reverseEffectEvent)
            {
                addOperation(reverseEffectEvent.SubEvents[0]);
                addOperation(reverseEffectEvent.SubEvents[1]);
                addOperation(reverseEffectEvent.SubEvents[2]);
            }
            else
            {
                addOperation(obj);
            }
        }

        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));

        return;

        void addOperation(ITimeable obj)
        {
            int oldFullTick = obj.Timestamp.FullTick;
            int newFullTick = oldFullTick - TimeSystem.DivisionInterval;

            newFullTick = Math.Max(0, newFullTick);
            
            operations.Add(new TimeableEditOperation(obj, oldFullTick, newFullTick));
        }
    }

    public static void Transform_MoveSelectionMeasureForward()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];

        foreach (ITimeable obj in SelectionSystem.SelectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                foreach (HoldPointNote point in holdNote.Points)
                {
                    addOperation(point);
                }
            }
            else if (obj is StopEffectEvent stopEffectEvent)
            {
                addOperation(stopEffectEvent.SubEvents[0]);
                addOperation(stopEffectEvent.SubEvents[1]);
            }
            else if (obj is ReverseEffectEvent reverseEffectEvent)
            {
                addOperation(reverseEffectEvent.SubEvents[0]);
                addOperation(reverseEffectEvent.SubEvents[1]);
                addOperation(reverseEffectEvent.SubEvents[2]);
            }
            else
            {
                addOperation(obj);
            }
        }

        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));

        return;

        void addOperation(ITimeable obj)
        {
            int oldFullTick = obj.Timestamp.FullTick;
            int newFullTick = oldFullTick + 1920;

            newFullTick = Math.Max(0, newFullTick);
            
            operations.Add(new TimeableEditOperation(obj, oldFullTick, newFullTick));
        }
    }

    public static void Transform_MoveSelectionMeasureBack()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];

        foreach (ITimeable obj in SelectionSystem.SelectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                foreach (HoldPointNote point in holdNote.Points)
                {
                    addOperation(point);
                }
            }
            else if (obj is StopEffectEvent stopEffectEvent)
            {
                addOperation(stopEffectEvent.SubEvents[0]);
                addOperation(stopEffectEvent.SubEvents[1]);
            }
            else if (obj is ReverseEffectEvent reverseEffectEvent)
            {
                addOperation(reverseEffectEvent.SubEvents[0]);
                addOperation(reverseEffectEvent.SubEvents[1]);
                addOperation(reverseEffectEvent.SubEvents[2]);
            }
            else
            {
                addOperation(obj);
            }
        }

        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));

        return;

        void addOperation(ITimeable obj)
        {
            int oldFullTick = obj.Timestamp.FullTick;
            int newFullTick = oldFullTick - 1920;

            newFullTick = Math.Max(0, newFullTick);
            
            operations.Add(new TimeableEditOperation(obj, oldFullTick, newFullTick));
        }
    }

    public static void Transform_MoveClockwise()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];

        foreach (ITimeable obj in SelectionSystem.SelectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                foreach (HoldPointNote point in holdNote.Points)
                {
                    addOperation(point);
                }
            }
            else if (obj is IPositionable positionable)
            {
                addOperation(positionable);
            }
        }

        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));

        return;

        void addOperation(IPositionable positionable)
        {
            int oldPosition = positionable.Position;
            int newPosition = oldPosition - 1;

            operations.Add(new PositionableEditOperation(positionable, oldPosition, newPosition, positionable.Size, positionable.Size));
        }
    }

    public static void Transform_MoveCounterclockwise()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];

        foreach (ITimeable obj in SelectionSystem.SelectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                foreach (HoldPointNote point in holdNote.Points)
                {
                    addOperation(point);
                }
            }
            else if (obj is IPositionable positionable)
            {
                addOperation(positionable);
            }
        }

        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));

        return;

        void addOperation(IPositionable positionable)
        {
            int oldPosition = positionable.Position;
            int newPosition = oldPosition + 1;

            operations.Add(new PositionableEditOperation(positionable, oldPosition, newPosition, positionable.Size, positionable.Size));
        }
    }

    public static void Transform_IncreaseSize()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];

        foreach (ITimeable obj in SelectionSystem.SelectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                foreach (HoldPointNote point in holdNote.Points)
                {
                    addOperation(point);
                }
            }
            else if (obj is IPositionable positionable)
            {
                addOperation(positionable);
            }
        }

        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));

        return;

        void addOperation(IPositionable positionable)
        {
            int oldSize = positionable.Size;
            int newSize = oldSize + 1;

            operations.Add(new PositionableEditOperation(positionable, positionable.Position, positionable.Position, oldSize, newSize));
        }
    }

    public static void Transform_DecreaseSize()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];

        foreach (ITimeable obj in SelectionSystem.SelectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                foreach (HoldPointNote point in holdNote.Points)
                {
                    addOperation(point);
                }
            }
            else if (obj is IPositionable positionable)
            {
                addOperation(positionable);
            }
        }

        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));

        return;

        void addOperation(IPositionable positionable)
        {
            int oldSize = positionable.Size;
            int newSize = oldSize - 1;

            operations.Add(new PositionableEditOperation(positionable, positionable.Position, positionable.Position, oldSize, newSize));
        }
    }

    public static void Transform_MoveClockwiseIterative()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];

        int step = 0;
        int lastFullTick = -1;
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                if (holdNote.Timestamp.FullTick != lastFullTick)
                {
                    step++;
                    lastFullTick = holdNote.Timestamp.FullTick;
                }

                foreach (HoldPointNote point in holdNote.Points)
                {
                    addOperation(point, step);
                }
            }
            else if (obj is IPositionable positionable)
            {
                if (obj.Timestamp.FullTick != lastFullTick)
                {
                    step++;
                    lastFullTick = obj.Timestamp.FullTick;
                }

                addOperation(positionable, step);
            }
        }

        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));

        return;

        void addOperation(IPositionable positionable, int offset)
        {
            int oldPosition = positionable.Position;
            int newPosition = oldPosition - offset;

            operations.Add(new PositionableEditOperation(positionable, oldPosition, newPosition, positionable.Size, positionable.Size));
        }
    }

    public static void Transform_MoveCounterclockwiseIterative()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];

        int step = 0;
        int lastFullTick = -1;
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                if (holdNote.Timestamp.FullTick != lastFullTick)
                {
                    step++;
                    lastFullTick = holdNote.Timestamp.FullTick;
                }

                foreach (HoldPointNote point in holdNote.Points)
                {
                    addOperation(point, step);
                }
            }
            else if (obj is IPositionable positionable)
            {
                if (obj.Timestamp.FullTick != lastFullTick)
                {
                    step++;
                    lastFullTick = obj.Timestamp.FullTick;
                }

                addOperation(positionable, step);
            }
        }

        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));

        return;

        void addOperation(IPositionable positionable, int offset)
        {
            int oldPosition = positionable.Position;
            int newPosition = oldPosition + offset;

            operations.Add(new PositionableEditOperation(positionable, oldPosition, newPosition, positionable.Size, positionable.Size));
        }
    }

    public static void Transform_IncreaseSizeIterative()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];

        int step = 0;
        int lastFullTick = -1;
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                if (holdNote.Timestamp.FullTick != lastFullTick)
                {
                    step++;
                    lastFullTick = holdNote.Timestamp.FullTick;
                }

                foreach (HoldPointNote point in holdNote.Points)
                {
                    addOperation(point, step);
                }
            }
            else if (obj is IPositionable positionable)
            {
                if (obj.Timestamp.FullTick != lastFullTick)
                {
                    step++;
                    lastFullTick = obj.Timestamp.FullTick;
                }

                addOperation(positionable, step);
            }
        }

        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));

        return;

        void addOperation(IPositionable positionable, int offset)
        {
            int oldSize = positionable.Size;
            int newSize = oldSize + offset;

            operations.Add(new PositionableEditOperation(positionable, positionable.Position, positionable.Position, oldSize, newSize));
        }
    }

    public static void Transform_DecreaseSizeIterative()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];

        int step = 0;
        int lastFullTick = -1;
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                if (holdNote.Timestamp.FullTick != lastFullTick)
                {
                    step++;
                    lastFullTick = holdNote.Timestamp.FullTick;
                }

                foreach (HoldPointNote point in holdNote.Points)
                {
                    addOperation(point, step);
                }
            }
            else if (obj is IPositionable positionable)
            {
                if (obj.Timestamp.FullTick != lastFullTick)
                {
                    step++;
                    lastFullTick = obj.Timestamp.FullTick;
                }

                addOperation(positionable, step);
            }
        }

        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));

        return;

        void addOperation(IPositionable positionable, int offset)
        {
            int oldSize = positionable.Size;
            int newSize = oldSize - offset;

            operations.Add(new PositionableEditOperation(positionable, positionable.Position, positionable.Position, oldSize, newSize));
        }
    }

    public static void Transform_MirrorHorizontal()
    {
        Mirror(30);
    }

    public static void Transform_MirrorVertical()
    {
        Mirror(0);
    }

    public static void Transform_MirrorCustom()
    {
        Mirror(MirrorAxis);
    }

    private static void Mirror(int axis)
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];

        foreach (ITimeable obj in SelectionSystem.SelectedObjects)
        {
            if (obj is not IPositionable positionable) continue;
            
            if (obj is SlideClockwiseNote sourceClw)
            {
                Layer? layer = ChartSystem.Chart.ParentLayer(sourceClw);
                if (layer == null) continue;

                int index = layer.Notes.IndexOf(sourceClw);
                int newPosition = axis - sourceClw.Size - sourceClw.Position;
                
                SlideCounterclockwiseNote newNote = new
                (
                    timestamp:     new(sourceClw.Timestamp.FullTick),
                    position:      newPosition,
                    size:          sourceClw.Size,
                    bonusType:     sourceClw.BonusType,
                    judgementType: sourceClw.JudgementType
                );
                
                operations.Add(new NoteRemoveOperation(layer, sourceClw, index));
                operations.Add(new SelectionRemoveOperation(sourceClw, SelectionSystem.LastSelectedObject));
                
                operations.Add(new NoteAddOperation(layer, newNote, index));
                operations.Add(new SelectionAddOperation(newNote, SelectionSystem.LastSelectedObject));
            }
            else if (obj is SlideCounterclockwiseNote sourceCcw)
            {
                Layer? layer = ChartSystem.Chart.ParentLayer(sourceCcw);
                if (layer == null) continue;

                int index = layer.Notes.IndexOf(sourceCcw);
                int newPosition = axis - sourceCcw.Size - sourceCcw.Position;
                
                SlideClockwiseNote newNote = new
                (
                    timestamp:     new(sourceCcw.Timestamp.FullTick),
                    position:      newPosition,
                    size:          sourceCcw.Size,
                    bonusType:     sourceCcw.BonusType,
                    judgementType: sourceCcw.JudgementType
                );
                
                operations.Add(new NoteRemoveOperation(layer, sourceCcw, index));
                operations.Add(new SelectionRemoveOperation(sourceCcw, SelectionSystem.LastSelectedObject));
                
                operations.Add(new NoteAddOperation(layer, newNote, index));
                operations.Add(new SelectionAddOperation(newNote, SelectionSystem.LastSelectedObject));
            }
            else if (obj is HoldNote sourceHoldNote)
            {
                foreach (HoldPointNote holdPointNote in sourceHoldNote.Points)
                {
                    int newPosition = axis - holdPointNote.Size - holdPointNote.Position;
                    operations.Add(new PositionableEditOperation(holdPointNote, holdPointNote.Position, newPosition, holdPointNote.Size, holdPointNote.Size));
                }
            }
            else
            {
                int newPosition = axis - positionable.Size - positionable.Position;
                operations.Add(new PositionableEditOperation(positionable, positionable.Position, newPosition, positionable.Size, positionable.Size));
            }

            if (obj is ILaneToggle laneToggle)
            {
                if (laneToggle.Direction == LaneSweepDirection.Clockwise)
                {
                    operations.Add(new LaneToggleEditOperation(laneToggle, laneToggle.Direction, LaneSweepDirection.Counterclockwise));
                }
                else if (laneToggle.Direction == LaneSweepDirection.Counterclockwise)
                {
                    operations.Add(new LaneToggleEditOperation(laneToggle, laneToggle.Direction, LaneSweepDirection.Clockwise));
                }
            }
        }
        
        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
    }

    public static void Transform_FlipDirection()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];

        foreach (ITimeable obj in SelectionSystem.SelectedObjects)
        {
            if (obj is SlideClockwiseNote sourceClw)
            {
                Layer? layer = ChartSystem.Chart.ParentLayer(sourceClw);
                if (layer == null) continue;

                int index = layer.Notes.IndexOf(sourceClw);

                SlideCounterclockwiseNote newNote = new
                (
                    timestamp:     new(sourceClw.Timestamp.FullTick),
                    position:      sourceClw.Position,
                    size:          sourceClw.Size,
                    bonusType:     sourceClw.BonusType,
                    judgementType: sourceClw.JudgementType
                );
                
                operations.Add(new NoteRemoveOperation(layer, sourceClw, index));
                operations.Add(new SelectionRemoveOperation(sourceClw, SelectionSystem.LastSelectedObject));
                
                operations.Add(new NoteAddOperation(layer, newNote, index));
                operations.Add(new SelectionAddOperation(newNote, SelectionSystem.LastSelectedObject));
            }
            else if (obj is SlideCounterclockwiseNote sourceCcw)
            {
                Layer? layer = ChartSystem.Chart.ParentLayer(sourceCcw);
                if (layer == null) continue;

                int index = layer.Notes.IndexOf(sourceCcw);

                SlideClockwiseNote newNote = new
                (
                    timestamp:     new(sourceCcw.Timestamp.FullTick),
                    position:      sourceCcw.Position,
                    size:          sourceCcw.Size,
                    bonusType:     sourceCcw.BonusType,
                    judgementType: sourceCcw.JudgementType
                );
                
                operations.Add(new NoteRemoveOperation(layer, sourceCcw, index));
                operations.Add(new SelectionRemoveOperation(sourceCcw, SelectionSystem.LastSelectedObject));
                
                operations.Add(new NoteAddOperation(layer, newNote, index));
                operations.Add(new SelectionAddOperation(newNote, SelectionSystem.LastSelectedObject));
            }
            else if (obj is ILaneToggle laneToggle)
            {
                if (laneToggle.Direction == LaneSweepDirection.Clockwise)
                {
                    operations.Add(new LaneToggleEditOperation(laneToggle, laneToggle.Direction, LaneSweepDirection.Counterclockwise));
                }
                else if (laneToggle.Direction == LaneSweepDirection.Counterclockwise)
                {
                    operations.Add(new LaneToggleEditOperation(laneToggle, laneToggle.Direction, LaneSweepDirection.Clockwise));
                }
            }
        }
        
        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
    }

    public static void Transform_ReverseSelection()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];
        List<ITimeable> objects = SelectionSystem.OrderedSelectedObjects;

        int min = objects[0].Timestamp.FullTick;
        int max = objects[0].Timestamp.FullTick;
        
        foreach (ITimeable obj in objects)
        {
            if (obj is HoldNote holdNote)
            {
                max = holdNote.Points[^1].Timestamp.FullTick;
            }
            else if (obj is StopEffectEvent stopEffectEvent)
            {
                max = Math.Max(max, stopEffectEvent.SubEvents[1].Timestamp.FullTick);
            }
            else if (obj is ReverseEffectEvent reverseEffectEvent)
            {
                max = Math.Max(max, reverseEffectEvent.SubEvents[2].Timestamp.FullTick);
            }
            else
            {
                max = Math.Max(max, obj.Timestamp.FullTick);
            }
        }

        foreach (ITimeable obj in objects)
        {
            if (obj is HoldNote holdNote)
            {
                foreach (HoldPointNote point in holdNote.Points)
                {
                    int newFullTick = min + max - point.Timestamp.FullTick;
                    operations.Add(new TimeableEditOperation(point, point.Timestamp.FullTick, newFullTick));
                }
            }
            else if (obj is StopEffectEvent stopEffectEvent)
            {
                int newFullTick = min + max - stopEffectEvent.SubEvents[0].Timestamp.FullTick;
                operations.Add(new TimeableEditOperation(stopEffectEvent.SubEvents[0], stopEffectEvent.SubEvents[0].Timestamp.FullTick, newFullTick));
                
                newFullTick = min + max - stopEffectEvent.SubEvents[1].Timestamp.FullTick;
                operations.Add(new TimeableEditOperation(stopEffectEvent.SubEvents[1], stopEffectEvent.SubEvents[1].Timestamp.FullTick, newFullTick));
            }
            else if (obj is ReverseEffectEvent reverseEffectEvent)
            {
                int newFullTick = min + max - reverseEffectEvent.SubEvents[0].Timestamp.FullTick;
                operations.Add(new TimeableEditOperation(reverseEffectEvent.SubEvents[0], reverseEffectEvent.SubEvents[0].Timestamp.FullTick, newFullTick));
                
                newFullTick = min + max - reverseEffectEvent.SubEvents[1].Timestamp.FullTick;
                operations.Add(new TimeableEditOperation(reverseEffectEvent.SubEvents[1], reverseEffectEvent.SubEvents[1].Timestamp.FullTick, newFullTick));
                
                newFullTick = min + max - reverseEffectEvent.SubEvents[2].Timestamp.FullTick;
                operations.Add(new TimeableEditOperation(reverseEffectEvent.SubEvents[2], reverseEffectEvent.SubEvents[2].Timestamp.FullTick, newFullTick));
            }
            else
            {
                int newFullTick = min + max - obj.Timestamp.FullTick;
                operations.Add(new TimeableEditOperation(obj, obj.Timestamp.FullTick, newFullTick));
            }
        }
        
        if (operations.Count < 2) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
    }

    public static void Transform_ScaleSelection(double scale)
    {
        if (scale == 1) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];
        List<ITimeable> objects = SelectionSystem.OrderedSelectedObjects;
        
        int min = objects[0].Timestamp.FullTick;
        
        foreach (ITimeable obj in objects)
        {
            if (obj is HoldNote holdNote)
            {
                foreach (HoldPointNote point in holdNote.Points)
                {
                    int newFullTick = (int)Math.Round(min + (point.Timestamp.FullTick - min) * scale);
                    operations.Add(new TimeableEditOperation(point, point.Timestamp.FullTick, newFullTick));
                }
            }
            else if (obj is StopEffectEvent stopEffectEvent)
            {
                int newFullTick = (int)Math.Round(min + (stopEffectEvent.SubEvents[0].Timestamp.FullTick - min) * scale);
                operations.Add(new TimeableEditOperation(stopEffectEvent.SubEvents[0], stopEffectEvent.SubEvents[0].Timestamp.FullTick, newFullTick));
                
                newFullTick = (int)Math.Round(min + (stopEffectEvent.SubEvents[1].Timestamp.FullTick - min) * scale);
                operations.Add(new TimeableEditOperation(stopEffectEvent.SubEvents[1], stopEffectEvent.SubEvents[1].Timestamp.FullTick, newFullTick));
            }
            else if (obj is ReverseEffectEvent reverseEffectEvent)
            {
                int newFullTick = (int)Math.Round(min + (reverseEffectEvent.SubEvents[0].Timestamp.FullTick - min) * scale);
                operations.Add(new TimeableEditOperation(reverseEffectEvent.SubEvents[0], reverseEffectEvent.SubEvents[0].Timestamp.FullTick, newFullTick));
                
                newFullTick = (int)Math.Round(min + (reverseEffectEvent.SubEvents[1].Timestamp.FullTick - min) * scale);
                operations.Add(new TimeableEditOperation(reverseEffectEvent.SubEvents[1], reverseEffectEvent.SubEvents[1].Timestamp.FullTick, newFullTick));
                
                newFullTick = (int)Math.Round(min + (reverseEffectEvent.SubEvents[2].Timestamp.FullTick - min) * scale);
                operations.Add(new TimeableEditOperation(reverseEffectEvent.SubEvents[2], reverseEffectEvent.SubEvents[2].Timestamp.FullTick, newFullTick));
            }
            else
            {
                int newFullTick = (int)Math.Round(min + (obj.Timestamp.FullTick - min) * scale);
                operations.Add(new TimeableEditOperation(obj, obj.Timestamp.FullTick, newFullTick));
            }
        }
        
        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
    }

    public static void Transform_OffsetChart(int offset)
    {
        if (offset == 0) return;
        
        List<IOperation> operations = [];

        foreach (Event @event in ChartSystem.Chart.Events)
        {
            if (@event is TempoChangeEvent && @event.Timestamp.FullTick == 0) continue;
            if (@event is MetreChangeEvent && @event.Timestamp.FullTick == 0) continue;
            
            addOperation(@event);
        }

        foreach (Note laneToggle in ChartSystem.Chart.LaneToggles)
        {
            addOperation(laneToggle);
        }

        foreach (Bookmark bookmark in ChartSystem.Chart.Bookmarks)
        {
            addOperation(bookmark);
        }

        foreach (Layer layer in ChartSystem.Chart.Layers)
        {
            foreach (Event @event in layer.Events)
            {
                if (@event is StopEffectEvent stopEffectEvent)
                {
                    addOperation(stopEffectEvent.SubEvents[0]);
                    addOperation(stopEffectEvent.SubEvents[1]);
                }
                else if (@event is ReverseEffectEvent reverseEffectEvent)
                {
                    addOperation(reverseEffectEvent.SubEvents[0]);
                    addOperation(reverseEffectEvent.SubEvents[1]);
                    addOperation(reverseEffectEvent.SubEvents[2]);
                }
                else
                {
                    addOperation(@event);
                }
            }

            foreach (Note note in layer.Notes)
            {
                if (note is HoldNote holdNote)
                {
                    foreach (HoldPointNote point in holdNote.Points)
                    {
                        addOperation(point);
                    }
                }
                else
                {
                    addOperation(note);
                }
            }
        }
        
        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));

        return;

        void addOperation(ITimeable obj)
        {
            int oldFullTick = obj.Timestamp.FullTick;
            int newFullTick = oldFullTick + offset;

            newFullTick = Math.Max(0, newFullTick);
            
            operations.Add(new TimeableEditOperation(obj, oldFullTick, newFullTick));
        }
    }

    public static void Transform_ScaleChart(double scale)
    {
        if (scale == 1) return;
        
        List<IOperation> operations = [];

        foreach (Event @event in ChartSystem.Chart.Events)
        {
            addOperation(@event);
        }

        foreach (Note laneToggle in ChartSystem.Chart.LaneToggles)
        {
            addOperation(laneToggle);
        }

        foreach (Bookmark bookmark in ChartSystem.Chart.Bookmarks)
        {
            addOperation(bookmark);
        }

        foreach (Layer layer in ChartSystem.Chart.Layers)
        {
            foreach (Event @event in layer.Events)
            {
                if (@event is StopEffectEvent stopEffectEvent)
                {
                    addOperation(stopEffectEvent.SubEvents[0]);
                    addOperation(stopEffectEvent.SubEvents[1]);
                }
                else if (@event is ReverseEffectEvent reverseEffectEvent)
                {
                    addOperation(reverseEffectEvent.SubEvents[0]);
                    addOperation(reverseEffectEvent.SubEvents[1]);
                    addOperation(reverseEffectEvent.SubEvents[2]);
                }
                else
                {
                    addOperation(@event);
                }
            }

            foreach (Note note in layer.Notes)
            {
                if (note is HoldNote holdNote)
                {
                    foreach (HoldPointNote point in holdNote.Points)
                    {
                        addOperation(point);
                    }
                }
                else
                {
                    addOperation(note);
                }
            }
        }
        
        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));

        return;

        void addOperation(ITimeable obj)
        {
            int oldFullTick = obj.Timestamp.FullTick;
            int newFullTick = (int)Math.Round(oldFullTick * scale);

            newFullTick = Math.Max(0, newFullTick);
            
            operations.Add(new TimeableEditOperation(obj, oldFullTick, newFullTick));
        }
    }

    public static void Transform_MirrorChart(int axis)
    {
        List<IOperation> operations = [];
                
        foreach (Note laneToggle in ChartSystem.Chart.LaneToggles)
        {
            if (laneToggle is not IPositionable positionable) continue;
            
            addOperation(positionable);
        }

        foreach (Layer layer in ChartSystem.Chart.Layers)
        foreach (Note note in layer.Notes)
        {
            if (note is HoldNote holdNote)
            {
                foreach (HoldPointNote point in holdNote.Points)
                {
                    addOperation(point);
                }
            }
            else if (note is IPositionable positionable)
            {
                addOperation(positionable);
            }
        }
        
        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));

        return;

        void addOperation(IPositionable positionable)
        {
            if (positionable is SlideClockwiseNote sourceClw)
            {
                Layer? layer = ChartSystem.Chart.ParentLayer(sourceClw);
                if (layer == null) return;

                int index = layer.Notes.IndexOf(sourceClw);
                int newPosition = axis - sourceClw.Size - sourceClw.Position;
                
                SlideCounterclockwiseNote newNote = new
                (
                    timestamp:     new(sourceClw.Timestamp.FullTick),
                    position:      newPosition,
                    size:          sourceClw.Size,
                    bonusType:     sourceClw.BonusType,
                    judgementType: sourceClw.JudgementType
                );
                
                operations.Add(new NoteRemoveOperation(layer, sourceClw, index));
                operations.Add(new NoteAddOperation(layer, newNote, index));

                if (SelectionSystem.SelectedObjects.Contains(sourceClw))
                {
                    operations.Add(new SelectionRemoveOperation(sourceClw, SelectionSystem.LastSelectedObject));
                    operations.Add(new SelectionAddOperation(newNote, SelectionSystem.LastSelectedObject));
                }
            }
            else if (positionable is SlideCounterclockwiseNote sourceCcw)
            {
                Layer? layer = ChartSystem.Chart.ParentLayer(sourceCcw);
                if (layer == null) return;

                int index = layer.Notes.IndexOf(sourceCcw);
                int newPosition = axis - sourceCcw.Size - sourceCcw.Position;
                
                SlideClockwiseNote newNote = new
                (
                    timestamp:     new(sourceCcw.Timestamp.FullTick),
                    position:      newPosition,
                    size:          sourceCcw.Size,
                    bonusType:     sourceCcw.BonusType,
                    judgementType: sourceCcw.JudgementType
                );
                
                operations.Add(new NoteRemoveOperation(layer, sourceCcw, index));
                operations.Add(new NoteAddOperation(layer, newNote, index));

                if (SelectionSystem.SelectedObjects.Contains(sourceCcw))
                {
                    operations.Add(new SelectionRemoveOperation(sourceCcw, SelectionSystem.LastSelectedObject));
                    operations.Add(new SelectionAddOperation(newNote, SelectionSystem.LastSelectedObject));
                }
            }
            else if (positionable is HoldNote sourceHoldNote)
            {
                foreach (HoldPointNote holdPointNote in sourceHoldNote.Points)
                {
                    int newPosition = axis - holdPointNote.Size - holdPointNote.Position;
                    operations.Add(new PositionableEditOperation(holdPointNote, holdPointNote.Position, newPosition, holdPointNote.Size, holdPointNote.Size));
                }
            }
            else
            {
                int newPosition = axis - positionable.Size - positionable.Position;
                operations.Add(new PositionableEditOperation(positionable, positionable.Position, newPosition, positionable.Size, positionable.Size));
            }

            if (positionable is ILaneToggle laneToggle)
            {
                if (laneToggle.Direction == LaneSweepDirection.Clockwise)
                {
                    operations.Add(new LaneToggleEditOperation(laneToggle, laneToggle.Direction, LaneSweepDirection.Counterclockwise));
                }
                else if (laneToggle.Direction == LaneSweepDirection.Counterclockwise)
                {
                    operations.Add(new LaneToggleEditOperation(laneToggle, laneToggle.Direction, LaneSweepDirection.Clockwise));
                }
            }
        }
    }

    
    public static void Convert_ZigZagHold(int beats, int division, int leftEdgeOffsetA, int leftEdgeOffsetB, int rightEdgeOffsetA,  int rightEdgeOffsetB)
    {
        List<IOperation> operations = [];

        int interval = 1920 * beats / division;
        
        foreach (ITimeable obj in SelectionSystem.SelectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                for (int i = 0; i < holdNote.Points.Count - 1; i++)
                {
                    processHoldSection(holdNote.Points[i], holdNote.Points[i + 1]);
                }
            }

            if (obj is HoldPointNote start)
            {
                int index = start.Parent.Points.IndexOf(start);
                if (index == start.Parent.Points.Count - 1) continue;

                HoldPointNote end = start.Parent.Points[index + 1];

                processHoldSection(start, end);
            }
        }
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));

        return;

        void processHoldSection(HoldPointNote start, HoldPointNote end)
        {
            if (start.Parent != end.Parent) return;
            if (start.Timestamp.FullTick == end.Timestamp.FullTick) return;

            bool isA = true;
            
            for (int i = start.Timestamp.FullTick + interval; i < end.Timestamp.FullTick; i += interval)
            {
                float t = (float)(i - start.Timestamp.FullTick) / (end.Timestamp.FullTick - start.Timestamp.FullTick);
                int position = (int)Math.Round(SaturnMath.LerpCyclic(start.Position, end.Position, t, 60));
                int size = (int)Math.Round(SaturnMath.Lerp(start.Size, end.Size, t));

                position = isA
                    ? position - leftEdgeOffsetA
                    : position - leftEdgeOffsetB;

                size = isA
                    ? size + leftEdgeOffsetA + rightEdgeOffsetA 
                    : size + leftEdgeOffsetB + rightEdgeOffsetB;
                
                HoldPointNote point = new
                (
                    timestamp:  new(i),
                    position:   position,
                    size:       size,
                    parent:     start.Parent,
                    renderType: HoldPointRenderType.Visible
                );
                
                operations.Add(new HoldPointNoteAddOperation(start.Parent, point, 0));
                isA = !isA;
            }
        }
    }

    public static void Convert_CutHold()
    {
        if (Mode != EditorMode.EditMode) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        if (ActiveObjectGroup is not HoldNote sourceHoldNote) return;

        List<IOperation> operations = [];

        Layer? layer = ChartSystem.Chart.ParentLayer(sourceHoldNote);
        if (layer == null) return;
        
        HoldNote newHoldNote = new(sourceHoldNote.BonusType, sourceHoldNote.JudgementType);
        for (int i = 0; i < sourceHoldNote.Points.Count; i++)
        {
            HoldPointNote point = sourceHoldNote.Points[i];
            
            // when encountering first point:
            if (i == 0)
            {
                // - add point to existing hold, force visible
                newHoldNote.Points.Add(new
                (
                    timestamp: new(point.Timestamp.FullTick),
                    position: point.Position,
                    size: point.Size,
                    parent: newHoldNote,
                    renderType: HoldPointRenderType.Visible
                ));
            }
            // when encountering cut point:
            else if (SelectionSystem.SelectedObjects.Contains(point))
            {
                // - add point to existing hold, force visible
                newHoldNote.Points.Add(new
                (
                    timestamp: new(point.Timestamp.FullTick),
                    position: point.Position,
                    size: point.Size,
                    parent: newHoldNote,
                    renderType: HoldPointRenderType.Visible
                ));

                // - push hold if points > 1
                if (newHoldNote.Points.Count > 1)
                {
                    operations.Add(new NoteAddOperation(layer, newHoldNote, 0));
                }
                
                // - create new hold
                newHoldNote = new(sourceHoldNote.BonusType, sourceHoldNote.JudgementType);
                
                // - add point to new hold, force visible
                newHoldNote.Points.Add(new
                (
                    timestamp: new(point.Timestamp.FullTick),
                    position: point.Position,
                    size: point.Size,
                    parent: newHoldNote,
                    renderType: HoldPointRenderType.Visible
                ));
            }
            // when encountering end of hold:
            else if (i == sourceHoldNote.Points.Count - 1)
            {
                // - add point to existing hold, force visible
                newHoldNote.Points.Add(new
                (
                    timestamp: new(point.Timestamp.FullTick),
                    position: point.Position,
                    size: point.Size,
                    parent: newHoldNote,
                    renderType: HoldPointRenderType.Visible
                ));
                
                // - push hold if points > 1
                if (newHoldNote.Points.Count > 1)
                {
                    operations.Add(new NoteAddOperation(layer, newHoldNote, 0));
                }
            }
            // when encountering normal point:
            else
            {
                // - add point to existing hold
                newHoldNote.Points.Add(new
                (
                    timestamp: new(point.Timestamp.FullTick),
                    position: point.Position,
                    size: point.Size,
                    parent: newHoldNote,
                    renderType: point.RenderType
                ));
            }
        }

        if (operations.Count == 0) return;
        
        // Set EditMode to NoteEditMode.
        CompositeOperation? op = GetEditModeChangeOperation(EditorMode.ObjectMode, null, null);
        if (op == null) return;

        operations.Add(op);
        
        // Remove original hold note.
        int index = layer.Notes.IndexOf(sourceHoldNote);
        operations.Add(new NoteRemoveOperation(layer, sourceHoldNote, index));

        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
    }

    public static void Convert_JoinHold()
    {
        if (Mode != EditorMode.ObjectMode) return;

        List<IOperation> operations = [];
        List<ITimeable> objects = SelectionSystem.OrderedSelectedObjects;

        if (objects.Count < 2) return;
        
        bool bonusTypeSet = false;

        Layer? rootLayer = null;
        HoldNote newHoldNote = new(BonusType.Normal, JudgementType.Normal);

        HashSet<(int, int, int)> alreadyExistingPoints = [];
        
        foreach (ITimeable obj in objects)
        {
            if (obj is not HoldNote h) continue;
            
            Layer? l = ChartSystem.Chart.ParentLayer(h);
            if (l == null) continue;

            rootLayer ??= l;
            
            if (!bonusTypeSet)
            {
                newHoldNote.BonusType = h.BonusType;
                newHoldNote.JudgementType = h.JudgementType;
                bonusTypeSet = true;
            }
            
            foreach (HoldPointNote p in h.Points)
            {
                if (!alreadyExistingPoints.Add((p.Timestamp.FullTick, p.Position, p.Size))) continue;
                
                newHoldNote.Points.Add(new
                (
                    timestamp:  new(p.Timestamp.FullTick),
                    position:   p.Position,
                    size:       p.Size,
                    parent:     newHoldNote,
                    renderType: p.RenderType
                ));
            }
            
            int i = l.Notes.IndexOf(h);
            operations.Add(new NoteRemoveOperation(l, h, i));
            operations.Add(new SelectionRemoveOperation(h, SelectionSystem.LastSelectedObject));
        }

        if (rootLayer == null) return;

        operations.Add(new NoteAddOperation(rootLayer, newHoldNote, 0));
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
    }

    
    public static void EventList_DeleteGlobalEvents()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];
        
        foreach (Event @event in ChartSystem.Chart.Events)
        {
            if (!SelectionSystem.SelectedObjects.Contains(@event)) continue;

            int index = ChartSystem.Chart.Events.IndexOf(@event);
            if (index == -1) continue;
            
            operations.Add(new SelectionRemoveOperation(@event, SelectionSystem.LastSelectedObject));
            operations.Add(new GlobalEventRemoveOperation(@event, index));
        }

        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
    }


    public static void LayerList_MoveLayerUp()
    {
        if (SelectionSystem.SelectedLayer == null) return;

        int indexA = ChartSystem.Chart.Layers.IndexOf(SelectionSystem.SelectedLayer);
        if (indexA == -1) return;

        int indexB = indexA - 1;
        if (indexB < 0) return;

        Layer layerA = ChartSystem.Chart.Layers[indexA];
        Layer layerB = ChartSystem.Chart.Layers[indexB];

        UndoRedoSystem.ChartBranch.Push(new LayerSwapOperation(layerA, layerB, indexA, indexB));
    }

    public static void LayerList_MoveLayerDown()
    {
        if (SelectionSystem.SelectedLayer == null) return;

        int indexA = ChartSystem.Chart.Layers.IndexOf(SelectionSystem.SelectedLayer);
        if (indexA == -1) return;

        int indexB = indexA + 1;
        if (indexB >= ChartSystem.Chart.Layers.Count) return;

        Layer layerA = ChartSystem.Chart.Layers[indexA];
        Layer layerB = ChartSystem.Chart.Layers[indexB];

        UndoRedoSystem.ChartBranch.Push(new LayerSwapOperation(layerA, layerB, indexA, indexB));
    }
    
    public static void LayerList_DeleteLayer()
    {
        if (SelectionSystem.SelectedLayer == null) return;

        int index = ChartSystem.Chart.Layers.IndexOf(SelectionSystem.SelectedLayer);
        
        Layer? newSelection = null;

        if (ChartSystem.Chart.Layers.Count > 1)
        {
            newSelection = index == 0
                ? ChartSystem.Chart.Layers[1]
                : ChartSystem.Chart.Layers[index - 1];
        }

        LayerRemoveOperation op0 = new(SelectionSystem.SelectedLayer, index);
        LayerSelectOperation op1 = new(SelectionSystem.SelectedLayer, newSelection);

        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }
    
    public static void LayerList_AddLayer()
    {
        Layer layer = ChartSystem.Chart.Layers.Count == 0 
            ? new("Main Layer") 
            : new("New Layer");
        
        int index = ChartSystem.Chart.Layers.Count;

        LayerAddOperation op0 = new(layer, index);
        LayerSelectOperation op1 = new(SelectionSystem.SelectedLayer, layer); 
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }

    public static void LayerList_DeleteEvents()
    {
        if (SelectionSystem.SelectedLayer == null) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];
        
        foreach (Event @event in SelectionSystem.SelectedLayer.Events)
        {
            if (!SelectionSystem.SelectedObjects.Contains(@event)) continue;

            int index = SelectionSystem.SelectedLayer.Events.IndexOf(@event);
            if (index == -1) continue;
            
            operations.Add(new SelectionRemoveOperation(@event, SelectionSystem.LastSelectedObject));
            operations.Add(new EventRemoveOperation(SelectionSystem.SelectedLayer, @event, index));
        }

        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
    }

#endregion Methods

#region System Event Handlers
    private static void OnChartLoaded(object? sender, EventArgs e)
    {
        Mode = EditorMode.ObjectMode;
        ActiveObjectGroup = null;
    }
#endregion System Event Handlers
}