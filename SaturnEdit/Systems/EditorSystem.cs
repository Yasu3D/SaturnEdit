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
using SaturnEdit.UndoRedo.EditModeOperations;
using SaturnEdit.UndoRedo.LayerOperations;
using SaturnEdit.UndoRedo.GenericOperations;
using SaturnEdit.UndoRedo.SelectionOperations;

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
            bool holdDeleted = false;
            if (Mode == EditorMode.EditMode && ActiveObjectGroup is HoldNote holdNote && holdNote.Points.Count < 2)
            {
                Layer? layer = ChartSystem.Chart.ParentLayer(holdNote);

                if (layer != null)
                {
                    operations.Add(new ListRemoveOperation<Note>(() => layer.Notes, holdNote));
                    holdDeleted = true;
                }
            }
            
            // Clear selection.
            foreach (ITimeable obj in objects)
            {
                operations.Add(new SelectionRemoveOperation(obj, SelectionSystem.LastSelectedObject));
            }
            
            // Re-select the previously active object group.
            if (ActiveObjectGroup != null && !holdDeleted)
            {
                operations.Add(new SelectionAddOperation(ActiveObjectGroup, SelectionSystem.LastSelectedObject));
            }

            // Change cursor to a different type again.
            if (ActiveObjectGroup is HoldNote)
            {
                operations.Add(new GenericEditOperation<Note>(value => { CursorSystem.CurrentType = value; }, CursorSystem.CurrentType, newType ?? CursorSystem.HoldNote));
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
                operations.Add(new GenericEditOperation<Note>(value => { CursorSystem.CurrentType = value; }, CursorSystem.CurrentType, CursorSystem.HoldPointNote));
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
                
                operations.Add(new GenericEditOperation<Note>(value => { CursorSystem.CurrentType = value; }, CursorSystem.CurrentType, CursorSystem.HoldPointNote));
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
                operations.Add(new ListAddOperation<Note>(() => ChartSystem.Chart.LaneToggles, note));
            }
            else if (SelectionSystem.SelectedLayer != null)
            {
                operations.Add(new ListAddOperation<Note>(() => SelectionSystem.SelectedLayer.Notes, note));
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
            
            operations.Add(new ListAddOperation<HoldPointNote>(() => holdNote.Points, point));
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
            operations.Add(new ListRemoveOperation<Bookmark>(() => ChartSystem.Chart.Bookmarks, bookmark));
        }
        
        for (int i = 0; i < ChartSystem.Chart.Events.Count; i++)
        {
            Event @event  = ChartSystem.Chart.Events[i];
            if (!SelectionSystem.SelectedObjects.Contains(@event)) continue;
            
            operations.Add(new SelectionRemoveOperation(@event, SelectionSystem.LastSelectedObject));
            operations.Add(new ListRemoveOperation<Event>(() => ChartSystem.Chart.Events, @event));
        }
        
        for (int i = 0; i < ChartSystem.Chart.LaneToggles.Count; i++)
        {
            Note laneToggle = ChartSystem.Chart.LaneToggles[i];
            if (!SelectionSystem.SelectedObjects.Contains(laneToggle)) continue;
            
            operations.Add(new SelectionRemoveOperation(laneToggle, SelectionSystem.LastSelectedObject));
            operations.Add(new ListRemoveOperation<Note>(() => ChartSystem.Chart.LaneToggles, laneToggle));
        }

        for (int i = 0; i < ChartSystem.Chart.Layers.Count; i++)
        {
            Layer layer = ChartSystem.Chart.Layers[i];
            for (int j = 0; j < layer.Events.Count; j++)
            {
                Event @event = layer.Events[j];
                if (!SelectionSystem.SelectedObjects.Contains(@event)) continue;

                operations.Add(new SelectionRemoveOperation(@event, SelectionSystem.LastSelectedObject));
                operations.Add(new ListRemoveOperation<Event>(() => layer.Events, @event));
            }

            for (int j = 0; j < layer.Notes.Count; j++)
            {
                Note note = layer.Notes[j];

                if (note is HoldNote holdNote)
                {
                    for (int k = 0; k < holdNote.Points.Count; k++)
                    {
                        HoldPointNote point = holdNote.Points[k];
                        if (!SelectionSystem.SelectedObjects.Contains(point)) continue;

                        operations.Add(new SelectionRemoveOperation(point, SelectionSystem.LastSelectedObject));
                        operations.Add(new ListRemoveOperation<HoldPointNote>(() => holdNote.Points, point));
                    }
                }

                if (!SelectionSystem.SelectedObjects.Contains(note)) continue;

                operations.Add(new SelectionRemoveOperation(note, SelectionSystem.LastSelectedObject));
                operations.Add(new ListRemoveOperation<Note>(() => layer.Notes, note));
            }
        }

        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
    }

    public static void ToolBar_EditShape()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        
        List<IOperation> operations = [];
        ITimeable[] selectedObjects = SelectionSystem.SelectedObjects.ToArray();
        
        foreach (ITimeable obj in selectedObjects)
        {
            if (obj is not IPositionable positionable) continue;

            if (obj is HoldNote holdNote)
            {
                for (int i = 0; i < holdNote.Points.Count; i++)
                {
                    HoldPointNote point = holdNote.Points[i];
                    if (point.Position == CursorSystem.Position && point.Size == CursorSystem.Size) continue;

                    operations.Add(new GenericEditOperation<int>(value => { point.Position = value; }, point.Position, CursorSystem.Position));
                    operations.Add(new GenericEditOperation<int>(value => { point.Size = value; }, point.Size, CursorSystem.Size));
                }

                continue;
            }
            
            if (positionable.Position == CursorSystem.Position && positionable.Size == CursorSystem.Size) continue;
            
            operations.Add(new GenericEditOperation<int>(value => { positionable.Position = value; }, positionable.Position, CursorSystem.Position));
            operations.Add(new GenericEditOperation<int>(value => { positionable.Size = value; }, positionable.Size, CursorSystem.Size));
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
                    for (int i = 0; i < sourceHoldNote.Points.Count; i++)
                    {
                        HoldPointNote point = sourceHoldNote.Points[i];
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

            operations.Add(new ListAddOperation<Note>(() => SelectionSystem.SelectedLayer.Notes, holdNote));
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
                    for (int i = 0; i < holdNote.Points.Count; i++)
                    {
                        HoldPointNote point = holdNote.Points[i];
                        Timestamp timestamp = new(point.Timestamp.FullTick);
                        ITimeable? newObject = CursorSystem.CurrentType switch
                        {
                            TouchNote => new TouchNote(timestamp, point.Position, point.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            ChainNote => new ChainNote(timestamp, point.Position, point.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            // Hold Note is skipped.
                            SlideClockwiseNote => new SlideClockwiseNote(timestamp, point.Position, point.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            SlideCounterclockwiseNote => new SlideCounterclockwiseNote(timestamp, point.Position, point.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            SnapForwardNote => new SnapForwardNote(timestamp, point.Position, point.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            SnapBackwardNote => new SnapBackwardNote(timestamp, point.Position, point.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            LaneShowNote => new LaneShowNote(timestamp, point.Position, point.Size, LaneSweepDirection.Instant),
                            LaneHideNote => new LaneHideNote(timestamp, point.Position, point.Size, LaneSweepDirection.Instant),
                            SyncNote => new SyncNote(timestamp, point.Position, point.Size),
                            MeasureLineNote => new MeasureLineNote(timestamp, false),
                            _ => null,
                        };

                        if (newObject == null) continue;

                        operations.Add(new SelectionAddOperation(newObject, SelectionSystem.LastSelectedObject));

                        if (newObject is ILaneToggle and Note newLaneToggle)
                        {
                            operations.Add(new ListAddOperation<Note>(() => ChartSystem.Chart.LaneToggles, newLaneToggle));
                        }
                        else if (newObject is Note newNote)
                        {
                            operations.Add(new ListAddOperation<Note>(() => layer.Notes, newNote));
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
                        operations.Add(new ListAddOperation<Note>(() => ChartSystem.Chart.LaneToggles, newLaneToggle));
                    }
                    else if (newObject is Note newNote)
                    {
                        operations.Add(new ListAddOperation<Note>(() => layer.Notes, newNote));
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
                operations.Add(new ListRemoveOperation<Event>(() => ChartSystem.Chart.Events, globalEvent));
            }
            else if (obj is ILaneToggle and Note laneToggle)
            {
                operations.Add(new ListRemoveOperation<Note>(() => ChartSystem.Chart.LaneToggles, laneToggle));
            }
            else if (obj is Bookmark bookmark)
            {
                operations.Add(new ListRemoveOperation<Bookmark>(() => ChartSystem.Chart.Bookmarks, bookmark));
            }
            else if (obj is HoldPointNote holdPointNote)
            {
                operations.Add(new ListRemoveOperation<HoldPointNote>(() => holdPointNote.Parent.Points, holdPointNote));
            }
            else if (obj is Event @event)
            {
                operations.Add(new ListRemoveOperation<Event>(() => layer.Events, @event));
            }
            else if (obj is Note note)
            {
                operations.Add(new ListRemoveOperation<Note>(() => layer.Notes, note));
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
                    for (int i = 0; i < sourceHoldNote.Points.Count; i++)
                    {
                        HoldPointNote point = sourceHoldNote.Points[i];
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

            operations.Add(new ListAddOperation<Note>(() => SelectionSystem.SelectedLayer.Notes, holdNote));
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
                    for (int i = 0; i < holdNote.Points.Count; i++)
                    {
                        HoldPointNote point = holdNote.Points[i];
                        Timestamp timestamp = new(point.Timestamp.FullTick);
                        ITimeable? newObject = CursorSystem.CurrentType switch
                        {
                            TouchNote => new TouchNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            ChainNote => new ChainNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            // Hold Note is skipped.
                            SlideClockwiseNote => new SlideClockwiseNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            SlideCounterclockwiseNote => new SlideCounterclockwiseNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            SnapForwardNote => new SnapForwardNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            SnapBackwardNote => new SnapBackwardNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.BonusType, CursorSystem.JudgementType),
                            LaneShowNote => new LaneShowNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.Direction),
                            LaneHideNote => new LaneHideNote(timestamp, CursorSystem.Position, CursorSystem.Size, CursorSystem.Direction),
                            SyncNote => new SyncNote(timestamp, CursorSystem.Position, CursorSystem.Size),
                            MeasureLineNote => new MeasureLineNote(timestamp, false),
                            _ => null,
                        };

                        if (newObject == null) continue;

                        operations.Add(new SelectionAddOperation(newObject, SelectionSystem.LastSelectedObject));

                        if (newObject is ILaneToggle and Note newLaneToggle)
                        {
                            operations.Add(new ListAddOperation<Note>(() => ChartSystem.Chart.LaneToggles, newLaneToggle));
                        }
                        else if (newObject is Note newNote)
                        {
                            operations.Add(new ListAddOperation<Note>(() => layer.Notes, newNote));
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
                        operations.Add(new ListAddOperation<Note>(() => ChartSystem.Chart.LaneToggles, newLaneToggle));
                    }
                    else if (newObject is Note newNote)
                    {
                        operations.Add(new ListAddOperation<Note>(() => layer.Notes, newNote));
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
                operations.Add(new ListRemoveOperation<Event>(() => ChartSystem.Chart.Events, globalEvent));
            }
            else if (obj is ILaneToggle and Note laneToggle)
            {
                operations.Add(new ListRemoveOperation<Note>(() => ChartSystem.Chart.LaneToggles, laneToggle));
            }
            else if (obj is Bookmark bookmark)
            {
                operations.Add(new ListRemoveOperation<Bookmark>(() => ChartSystem.Chart.Bookmarks, bookmark));
            }
            else if (obj is HoldPointNote holdPointNote)
            {
                operations.Add(new ListRemoveOperation<HoldPointNote>(() => holdPointNote.Parent.Points, holdPointNote));
            }
            else if (obj is Event @event)
            {
                operations.Add(new ListRemoveOperation<Event>(() => layer.Events, @event));
            }
            else if (obj is Note note)
            {
                operations.Add(new ListRemoveOperation<Note>(() => layer.Notes, note));
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
                    for (int i = 0; i < holdNote.Points.Count; i++)
                    {
                        HoldPointNote point = holdNote.Points[i];
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
        ITimeable[] selectedObjects = SelectionSystem.SelectedObjects.ToArray();
        
        foreach (ITimeable obj in selectedObjects)
        {
            operations.Add(new SelectionRemoveOperation(obj, SelectionSystem.LastSelectedObject));
        }
        
        if (Mode == EditorMode.ObjectMode)
        {
            for (int i = 0; i < chart.Bookmarks.Count; i++)
            {
                Bookmark bookmark = chart.Bookmarks[i];
                bookmark.Timestamp.FullTick += TimeSystem.Timestamp.FullTick;

                operations.Add(new ListAddOperation<Bookmark>(() => ChartSystem.Chart.Bookmarks, bookmark));
                operations.Add(new SelectionAddOperation(bookmark, SelectionSystem.LastSelectedObject));
            }

            for (int i = 0; i < chart.Events.Count; i++)
            {
                Event globalEvent = chart.Events[i];
                globalEvent.Timestamp.FullTick += TimeSystem.Timestamp.FullTick;

                operations.Add(new ListAddOperation<Event>(() => ChartSystem.Chart.Events, globalEvent));
                operations.Add(new SelectionAddOperation(globalEvent, SelectionSystem.LastSelectedObject));
            }

            for (int i = 0; i < chart.LaneToggles.Count; i++)
            {
                Note laneToggle = chart.LaneToggles[i];
                laneToggle.Timestamp.FullTick += TimeSystem.Timestamp.FullTick;

                operations.Add(new ListAddOperation<Note>(() => ChartSystem.Chart.LaneToggles, laneToggle));
                operations.Add(new SelectionAddOperation(laneToggle, SelectionSystem.LastSelectedObject));
            }

            if (SelectionSystem.SelectedLayer != null)
            {
                for (int i = 0; i < chart.Layers.Count; i++)
                {
                    Layer layer = chart.Layers[i];
                    for (int j = 0; j < layer.Events.Count; j++)
                    {
                        Event layerEvent = layer.Events[j];
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

                        operations.Add(new ListAddOperation<Event>(() => SelectionSystem.SelectedLayer.Events, layerEvent));
                        operations.Add(new SelectionAddOperation(layerEvent, SelectionSystem.LastSelectedObject));
                    }

                    for (int j = 0; j < layer.Notes.Count; j++)
                    {
                        Note note = layer.Notes[j];
                        if (note is HoldNote holdNote)
                        {
                            if (holdNote.Points.Count < 2) continue;

                            for (int k = 0; k < holdNote.Points.Count; k++)
                            {
                                HoldPointNote point = holdNote.Points[k];
                                point.Timestamp.FullTick += TimeSystem.Timestamp.FullTick;
                            }
                        }
                        else
                        {
                            note.Timestamp.FullTick += TimeSystem.Timestamp.FullTick;
                        }

                        operations.Add(new ListAddOperation<Note>(() => SelectionSystem.SelectedLayer.Notes, note));
                        operations.Add(new SelectionAddOperation(note, SelectionSystem.LastSelectedObject));
                    }
                }
            }
        }
        else if (Mode == EditorMode.EditMode)
        {
            if (ActiveObjectGroup is not HoldNote holdNote) return;
            if (chart.Layers.Count == 0) return;

            for (int i = 0; i < chart.Layers[0].Notes.Count; i++)
            {
                Note note = chart.Layers[0].Notes[i];
                if (note is not HoldNote sourceHold) continue;

                for (int j = 0; j < sourceHold.Points.Count; j++)
                {
                    HoldPointNote sourcePoint = sourceHold.Points[j];
                    sourcePoint.Timestamp.FullTick += TimeSystem.Timestamp.FullTick;

                    operations.Add(new ListAddOperation<HoldPointNote>(() => holdNote.Points, sourcePoint));
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

        ListAddOperation<Event> op0 = new(() => ChartSystem.Chart.Events, tempoChangeEvent);
        SelectionAddOperation op1 = new(tempoChangeEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }

    public static void Insert_AddMetreChange(int upper, int lower)
    {
        MetreChangeEvent metreChangeEvent = new(new(TimeSystem.Timestamp.FullTick), upper, lower);

        ListAddOperation<Event> op0 = new(() => ChartSystem.Chart.Events, metreChangeEvent);
        SelectionAddOperation op1 = new(metreChangeEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }

    public static void Insert_AddTutorialMarker(string key)
    {
        TutorialMarkerEvent tutorialMarkerEvent = new(new(TimeSystem.Timestamp.FullTick), key);

        ListAddOperation<Event> op0 = new(() => ChartSystem.Chart.Events, tutorialMarkerEvent);
        SelectionAddOperation op1 = new(tutorialMarkerEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }
    
    public static void Insert_AddSpeedChange(float speed)
    {
        if (SelectionSystem.SelectedLayer == null) return;
        
        SpeedChangeEvent speedChangeEvent = new(new(TimeSystem.Timestamp.FullTick), speed);

        ListAddOperation<Event> op0 = new(() => SelectionSystem.SelectedLayer.Events, speedChangeEvent);
        SelectionAddOperation op1 = new(speedChangeEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }

    public static void Insert_AddVisibilityChange(bool visibility)
    {
        if (SelectionSystem.SelectedLayer == null) return;
        
        VisibilityChangeEvent visibilityChangeEvent = new(new(TimeSystem.Timestamp.FullTick), visibility);

        ListAddOperation<Event> op0 = new(() => SelectionSystem.SelectedLayer.Events, visibilityChangeEvent);
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
        
        ListAddOperation<Event> op0 = new(() => SelectionSystem.SelectedLayer.Events, reverseEffectEvent);
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
        
        ListAddOperation<Event> op0 = new(() => SelectionSystem.SelectedLayer.Events, stopEffectEvent);
        SelectionAddOperation op1 = new(stopEffectEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }

    public static void Insert_AddBookmark(string message, uint color)
    {
        Bookmark bookmark = new(new(TimeSystem.Timestamp.FullTick), color, message);

        ListAddOperation<Bookmark> op0 = new(() => ChartSystem.Chart.Bookmarks, bookmark);
        SelectionAddOperation op1 = new(bookmark, SelectionSystem.LastSelectedObject);

        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }

    
    public static void Transform_MoveSelectionBeatForward()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        
        List<IOperation> operations = []; 
        ITimeable[] selectedObjects = SelectionSystem.SelectedObjects.ToArray();
        
        foreach (ITimeable obj in selectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                for (int i = 0; i < holdNote.Points.Count; i++)
                {
                    HoldPointNote point = holdNote.Points[i];
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
            int currentBeat = Timestamp.BeatFromTick(obj.Timestamp.FullTick, TimeSystem.Division);
            int newBeat = currentBeat + 1;

            int currentBeatFullTick = Timestamp.TickFromBeat(currentBeat, TimeSystem.Division);
            int newBeatFullTick = Timestamp.TickFromBeat(newBeat, TimeSystem.Division);

            int roundingDelta = obj.Timestamp.FullTick - currentBeatFullTick;
            
            operations.Add(new GenericEditOperation<int>(value => { obj.Timestamp.FullTick = value; }, obj.Timestamp.FullTick, newBeatFullTick + roundingDelta));
        }
    }

    public static void Transform_MoveSelectionBeatBack()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];
        ITimeable[] selectedObjects = SelectionSystem.SelectedObjects.ToArray();
        
        foreach (ITimeable obj in selectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                for (int i = 0; i < holdNote.Points.Count; i++)
                {
                    HoldPointNote point = holdNote.Points[i];
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
            int currentBeat = Timestamp.BeatFromTick(obj.Timestamp.FullTick, TimeSystem.Division);
            int newBeat = currentBeat - 1;

            int currentBeatFullTick = Timestamp.TickFromBeat(currentBeat, TimeSystem.Division);
            int newBeatFullTick = Timestamp.TickFromBeat(newBeat, TimeSystem.Division);

            int roundingDelta = obj.Timestamp.FullTick - currentBeatFullTick;
            
            operations.Add(new GenericEditOperation<int>(value => { obj.Timestamp.FullTick = value; }, obj.Timestamp.FullTick, newBeatFullTick + roundingDelta));
        }
    }

    public static void Transform_MoveSelectionMeasureForward()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];
        ITimeable[] selectedObjects = SelectionSystem.SelectedObjects.ToArray();
        
        foreach (ITimeable obj in selectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                for (int i = 0; i < holdNote.Points.Count; i++)
                {
                    HoldPointNote point = holdNote.Points[i];
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
            
            operations.Add(new GenericEditOperation<int>(value => { obj.Timestamp.FullTick = value; }, oldFullTick, newFullTick));
        }
    }

    public static void Transform_MoveSelectionMeasureBack()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];
        ITimeable[] selectedObjects = SelectionSystem.SelectedObjects.ToArray();

        foreach (ITimeable obj in selectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                for (int i = 0; i < holdNote.Points.Count; i++)
                {
                    HoldPointNote point = holdNote.Points[i];
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
            
            operations.Add(new GenericEditOperation<int>(value => { obj.Timestamp.FullTick = value; }, oldFullTick, newFullTick));
        }
    }

    public static void Transform_MoveClockwise()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];
        ITimeable[] selectedObjects = SelectionSystem.SelectedObjects.ToArray();
        
        foreach (ITimeable obj in selectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                for (int i = 0; i < holdNote.Points.Count; i++)
                {
                    HoldPointNote point = holdNote.Points[i];
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

            operations.Add(new GenericEditOperation<int>(value => { positionable.Position = value; }, oldPosition, newPosition));
        }
    }

    public static void Transform_MoveCounterclockwise()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];
        ITimeable[] selectedObjects = SelectionSystem.SelectedObjects.ToArray();
        
        foreach (ITimeable obj in selectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                for (int i = 0; i < holdNote.Points.Count; i++)
                {
                    HoldPointNote point = holdNote.Points[i];
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

            operations.Add(new GenericEditOperation<int>(value => { positionable.Position = value; }, oldPosition, newPosition));
        }
    }

    public static void Transform_IncreaseSize()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];
        ITimeable[] selectedObjects = SelectionSystem.SelectedObjects.ToArray();
        
        foreach (ITimeable obj in selectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                for (int i = 0; i < holdNote.Points.Count; i++)
                {
                    HoldPointNote point = holdNote.Points[i];
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

            operations.Add(new GenericEditOperation<int>(value => { positionable.Size = value; }, oldSize, newSize));
        }
    }

    public static void Transform_DecreaseSize()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];
        ITimeable[] selectedObjects = SelectionSystem.SelectedObjects.ToArray();
        
        foreach (ITimeable obj in selectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                for (int i = 0; i < holdNote.Points.Count; i++)
                {
                    HoldPointNote point = holdNote.Points[i];
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

            operations.Add(new GenericEditOperation<int>(value => { positionable.Size = value; }, oldSize, newSize));
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

                for (int i = 0; i < holdNote.Points.Count; i++)
                {
                    HoldPointNote point = holdNote.Points[i];
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

            operations.Add(new GenericEditOperation<int>(value => { positionable.Position = value; }, oldPosition, newPosition));
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

                for (int i = 0; i < holdNote.Points.Count; i++)
                {
                    HoldPointNote point = holdNote.Points[i];
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

            operations.Add(new GenericEditOperation<int>(value => { positionable.Position = value; }, oldPosition, newPosition));
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

                for (int i = 0; i < holdNote.Points.Count; i++)
                {
                    HoldPointNote point = holdNote.Points[i];
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

            operations.Add(new GenericEditOperation<int>(value => { positionable.Size = value; }, oldSize, newSize));
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

                for (int i = 0; i < holdNote.Points.Count; i++)
                {
                    HoldPointNote point = holdNote.Points[i];
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

            operations.Add(new GenericEditOperation<int>(value => { positionable.Size = value; }, oldSize, newSize));
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
        ITimeable[] selectedObjects = SelectionSystem.SelectedObjects.ToArray();
        
        foreach (ITimeable obj in selectedObjects)
        {
            if (obj is not IPositionable positionable) continue;
            
            if (obj is SlideClockwiseNote sourceClw)
            {
                Layer? layer = ChartSystem.Chart.ParentLayer(sourceClw);
                if (layer == null) continue;
                
                int newPosition = axis - sourceClw.Size - sourceClw.Position;
                
                SlideCounterclockwiseNote newNote = new
                (
                    timestamp:     new(sourceClw.Timestamp.FullTick),
                    position:      newPosition,
                    size:          sourceClw.Size,
                    bonusType:     sourceClw.BonusType,
                    judgementType: sourceClw.JudgementType
                );
                
                operations.Add(new ListRemoveOperation<Note>(() => layer.Notes, sourceClw));
                operations.Add(new SelectionRemoveOperation(sourceClw, SelectionSystem.LastSelectedObject));
                
                operations.Add(new ListAddOperation<Note>(() => layer.Notes, newNote));
                operations.Add(new SelectionAddOperation(newNote, SelectionSystem.LastSelectedObject));
            }
            else if (obj is SlideCounterclockwiseNote sourceCcw)
            {
                Layer? layer = ChartSystem.Chart.ParentLayer(sourceCcw);
                if (layer == null) continue;
                
                int newPosition = axis - sourceCcw.Size - sourceCcw.Position;
                
                SlideClockwiseNote newNote = new
                (
                    timestamp:     new(sourceCcw.Timestamp.FullTick),
                    position:      newPosition,
                    size:          sourceCcw.Size,
                    bonusType:     sourceCcw.BonusType,
                    judgementType: sourceCcw.JudgementType
                );
                
                operations.Add(new ListRemoveOperation<Note>(() => layer.Notes, sourceCcw));
                operations.Add(new SelectionRemoveOperation(sourceCcw, SelectionSystem.LastSelectedObject));
                
                operations.Add(new ListAddOperation<Note>(() => layer.Notes, newNote));
                operations.Add(new SelectionAddOperation(newNote, SelectionSystem.LastSelectedObject));
            }
            else if (obj is HoldNote sourceHoldNote)
            {
                for (int i = 0; i < sourceHoldNote.Points.Count; i++)
                {
                    HoldPointNote holdPointNote = sourceHoldNote.Points[i];
                    int newPosition = axis - holdPointNote.Size - holdPointNote.Position;
                    operations.Add(new GenericEditOperation<int>(value => { holdPointNote.Position = value; }, holdPointNote.Position, newPosition));
                }
            }
            else
            {
                int newPosition = axis - positionable.Size - positionable.Position;
                operations.Add(new GenericEditOperation<int>(value => { positionable.Position = value; }, positionable.Position, newPosition));
            }

            if (obj is ILaneToggle laneToggle)
            {
                if (laneToggle.Direction == LaneSweepDirection.Clockwise)
                {
                    operations.Add(new GenericEditOperation<LaneSweepDirection>(value => { laneToggle.Direction = value; }, laneToggle.Direction, LaneSweepDirection.Counterclockwise));
                }
                else if (laneToggle.Direction == LaneSweepDirection.Counterclockwise)
                {
                    operations.Add(new GenericEditOperation<LaneSweepDirection>(value => { laneToggle.Direction = value; }, laneToggle.Direction, LaneSweepDirection.Clockwise));
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
        ITimeable[] selectedObjects = SelectionSystem.SelectedObjects.ToArray();
        
        foreach (ITimeable obj in selectedObjects)
        {
            if (obj is SlideClockwiseNote sourceClw)
            {
                Layer? layer = ChartSystem.Chart.ParentLayer(sourceClw);
                if (layer == null) continue;
                
                SlideCounterclockwiseNote newNote = new
                (
                    timestamp:     new(sourceClw.Timestamp.FullTick),
                    position:      sourceClw.Position,
                    size:          sourceClw.Size,
                    bonusType:     sourceClw.BonusType,
                    judgementType: sourceClw.JudgementType
                );
                
                operations.Add(new ListRemoveOperation<Note>(() => layer.Notes, sourceClw));
                operations.Add(new SelectionRemoveOperation(sourceClw, SelectionSystem.LastSelectedObject));
                
                operations.Add(new ListAddOperation<Note>(() => layer.Notes, newNote));
                operations.Add(new SelectionAddOperation(newNote, SelectionSystem.LastSelectedObject));
            }
            else if (obj is SlideCounterclockwiseNote sourceCcw)
            {
                Layer? layer = ChartSystem.Chart.ParentLayer(sourceCcw);
                if (layer == null) continue;
                
                SlideClockwiseNote newNote = new
                (
                    timestamp:     new(sourceCcw.Timestamp.FullTick),
                    position:      sourceCcw.Position,
                    size:          sourceCcw.Size,
                    bonusType:     sourceCcw.BonusType,
                    judgementType: sourceCcw.JudgementType
                );
                
                operations.Add(new ListRemoveOperation<Note>(() => layer.Notes, sourceCcw));
                operations.Add(new SelectionRemoveOperation(sourceCcw, SelectionSystem.LastSelectedObject));
                
                operations.Add(new ListAddOperation<Note>(() => layer.Notes, newNote));
                operations.Add(new SelectionAddOperation(newNote, SelectionSystem.LastSelectedObject));
            }
            else if (obj is SnapForwardNote sourceForward)
            {
                Layer? layer = ChartSystem.Chart.ParentLayer(sourceForward);
                if (layer == null) continue;
                
                SnapBackwardNote newNote = new
                (
                    timestamp:     new(sourceForward.Timestamp.FullTick),
                    position:      sourceForward.Position,
                    size:          sourceForward.Size,
                    bonusType:     sourceForward.BonusType,
                    judgementType: sourceForward.JudgementType
                );
                
                operations.Add(new ListRemoveOperation<Note>(() => layer.Notes, sourceForward));
                operations.Add(new SelectionRemoveOperation(sourceForward, SelectionSystem.LastSelectedObject));
                
                operations.Add(new ListAddOperation<Note>(() => layer.Notes, newNote));
                operations.Add(new SelectionAddOperation(newNote, SelectionSystem.LastSelectedObject));
            }
            else if (obj is SnapBackwardNote sourceBackward)
            {
                Layer? layer = ChartSystem.Chart.ParentLayer(sourceBackward);
                if (layer == null) continue;
                
                SnapForwardNote newNote = new
                (
                    timestamp:     new(sourceBackward.Timestamp.FullTick),
                    position:      sourceBackward.Position,
                    size:          sourceBackward.Size,
                    bonusType:     sourceBackward.BonusType,
                    judgementType: sourceBackward.JudgementType
                );
                
                operations.Add(new ListRemoveOperation<Note>(() => layer.Notes, sourceBackward));
                operations.Add(new SelectionRemoveOperation(sourceBackward, SelectionSystem.LastSelectedObject));
                
                operations.Add(new ListAddOperation<Note>(() => layer.Notes, newNote));
                operations.Add(new SelectionAddOperation(newNote, SelectionSystem.LastSelectedObject));
            }
            else if (obj is ILaneToggle laneToggle)
            {
                if (laneToggle.Direction == LaneSweepDirection.Clockwise)
                {
                    operations.Add(new GenericEditOperation<LaneSweepDirection>(value => { laneToggle.Direction = value; }, laneToggle.Direction, LaneSweepDirection.Counterclockwise));
                }
                else if (laneToggle.Direction == LaneSweepDirection.Counterclockwise)
                {
                    operations.Add(new GenericEditOperation<LaneSweepDirection>(value => { laneToggle.Direction = value; }, laneToggle.Direction, LaneSweepDirection.Clockwise));
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
                for (int i = 0; i < holdNote.Points.Count; i++)
                {
                    HoldPointNote point = holdNote.Points[i];
                    int newFullTick = min + max - point.Timestamp.FullTick;
                    operations.Add(new GenericEditOperation<int>(value => { point.Timestamp.FullTick = value; }, point.Timestamp.FullTick, newFullTick));
                }
            }
            else if (obj is StopEffectEvent stopEffectEvent)
            {
                int newFullTick = min + max - stopEffectEvent.SubEvents[0].Timestamp.FullTick;
                operations.Add(new GenericEditOperation<int>(value => { stopEffectEvent.SubEvents[0].Timestamp.FullTick = value; }, stopEffectEvent.SubEvents[0].Timestamp.FullTick, newFullTick));
                
                newFullTick = min + max - stopEffectEvent.SubEvents[1].Timestamp.FullTick;
                operations.Add(new GenericEditOperation<int>(value => { stopEffectEvent.SubEvents[1].Timestamp.FullTick = value; }, stopEffectEvent.SubEvents[1].Timestamp.FullTick, newFullTick));
            }
            else if (obj is ReverseEffectEvent reverseEffectEvent)
            {
                int newFullTick = min + max - reverseEffectEvent.SubEvents[0].Timestamp.FullTick;
                operations.Add(new GenericEditOperation<int>(value => { reverseEffectEvent.SubEvents[0].Timestamp.FullTick = value; }, reverseEffectEvent.SubEvents[0].Timestamp.FullTick, newFullTick));
                
                newFullTick = min + max - reverseEffectEvent.SubEvents[1].Timestamp.FullTick;
                operations.Add(new GenericEditOperation<int>(value => { reverseEffectEvent.SubEvents[1].Timestamp.FullTick = value; }, reverseEffectEvent.SubEvents[1].Timestamp.FullTick, newFullTick));
                
                newFullTick = min + max - reverseEffectEvent.SubEvents[2].Timestamp.FullTick;
                operations.Add(new GenericEditOperation<int>(value => { reverseEffectEvent.SubEvents[2].Timestamp.FullTick = value; }, reverseEffectEvent.SubEvents[2].Timestamp.FullTick, newFullTick));
            }
            else
            {
                int newFullTick = min + max - obj.Timestamp.FullTick;
                operations.Add(new GenericEditOperation<int>(value => { obj.Timestamp.FullTick = value; }, obj.Timestamp.FullTick, newFullTick));
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
                for (int i = 0; i < holdNote.Points.Count; i++)
                {
                    HoldPointNote point = holdNote.Points[i];
                    int newFullTick = (int)Math.Round(min + (point.Timestamp.FullTick - min) * scale);
                    operations.Add(new GenericEditOperation<int>(value => { point.Timestamp.FullTick = value; }, point.Timestamp.FullTick, newFullTick));
                }
            }
            else if (obj is StopEffectEvent stopEffectEvent)
            {
                int newFullTick = (int)Math.Round(min + (stopEffectEvent.SubEvents[0].Timestamp.FullTick - min) * scale);
                operations.Add(new GenericEditOperation<int>(value => { stopEffectEvent.SubEvents[0].Timestamp.FullTick = value; }, stopEffectEvent.SubEvents[0].Timestamp.FullTick, newFullTick));
                
                newFullTick = (int)Math.Round(min + (stopEffectEvent.SubEvents[1].Timestamp.FullTick - min) * scale);
                operations.Add(new GenericEditOperation<int>(value => { stopEffectEvent.SubEvents[1].Timestamp.FullTick = value; }, stopEffectEvent.SubEvents[1].Timestamp.FullTick, newFullTick));
            }
            else if (obj is ReverseEffectEvent reverseEffectEvent)
            {
                int newFullTick = (int)Math.Round(min + (reverseEffectEvent.SubEvents[0].Timestamp.FullTick - min) * scale);
                operations.Add(new GenericEditOperation<int>(value => { reverseEffectEvent.SubEvents[0].Timestamp.FullTick = value; }, reverseEffectEvent.SubEvents[0].Timestamp.FullTick, newFullTick));
                
                newFullTick = (int)Math.Round(min + (reverseEffectEvent.SubEvents[1].Timestamp.FullTick - min) * scale);
                operations.Add(new GenericEditOperation<int>(value => { reverseEffectEvent.SubEvents[1].Timestamp.FullTick = value; }, reverseEffectEvent.SubEvents[1].Timestamp.FullTick, newFullTick));
                
                newFullTick = (int)Math.Round(min + (reverseEffectEvent.SubEvents[2].Timestamp.FullTick - min) * scale);
                operations.Add(new GenericEditOperation<int>(value => { reverseEffectEvent.SubEvents[2].Timestamp.FullTick = value; }, reverseEffectEvent.SubEvents[2].Timestamp.FullTick, newFullTick));
            }
            else
            {
                int newFullTick = (int)Math.Round(min + (obj.Timestamp.FullTick - min) * scale);
                operations.Add(new GenericEditOperation<int>(value => { obj.Timestamp.FullTick = value; }, obj.Timestamp.FullTick, newFullTick));
            }
        }
        
        if (operations.Count == 0) return;
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
    }

    public static void Transform_OffsetChart(int offset)
    {
        if (offset == 0) return;
        
        List<IOperation> operations = [];

        for (int i = 0; i < ChartSystem.Chart.Events.Count; i++)
        {
            Event @event = ChartSystem.Chart.Events[i];
            if (@event is TempoChangeEvent && @event.Timestamp.FullTick == 0) continue;
            if (@event is MetreChangeEvent && @event.Timestamp.FullTick == 0) continue;

            addOperation(@event);
        }

        for (int i = 0; i < ChartSystem.Chart.LaneToggles.Count; i++)
        {
            Note laneToggle = ChartSystem.Chart.LaneToggles[i];
            addOperation(laneToggle);
        }

        for (int i = 0; i < ChartSystem.Chart.Bookmarks.Count; i++)
        {
            Bookmark bookmark = ChartSystem.Chart.Bookmarks[i];
            addOperation(bookmark);
        }

        for (int i = 0; i < ChartSystem.Chart.Layers.Count; i++)
        {
            Layer layer = ChartSystem.Chart.Layers[i];
            for (int j = 0; j < layer.Events.Count; j++)
            {
                Event @event = layer.Events[j];
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

            for (int j = 0; j < layer.Notes.Count; j++)
            {
                Note note = layer.Notes[j];
                if (note is HoldNote holdNote)
                {
                    for (int k = 0; k < holdNote.Points.Count; k++)
                    {
                        HoldPointNote point = holdNote.Points[k];
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
            
            operations.Add(new GenericEditOperation<int>(value => { obj.Timestamp.FullTick = value; }, oldFullTick, newFullTick));
        }
    }

    public static void Transform_ScaleChart(double scale)
    {
        if (scale == 1) return;
        
        List<IOperation> operations = [];

        for (int i = 0; i < ChartSystem.Chart.Events.Count; i++)
        {
            Event @event = ChartSystem.Chart.Events[i];
            addOperation(@event);
        }

        for (int i = 0; i < ChartSystem.Chart.LaneToggles.Count; i++)
        {
            Note laneToggle = ChartSystem.Chart.LaneToggles[i];
            addOperation(laneToggle);
        }

        for (int i = 0; i < ChartSystem.Chart.Bookmarks.Count; i++)
        {
            Bookmark bookmark = ChartSystem.Chart.Bookmarks[i];
            addOperation(bookmark);
        }

        for (int i = 0; i < ChartSystem.Chart.Layers.Count; i++)
        {
            Layer layer = ChartSystem.Chart.Layers[i];
            for (int j = 0; j < layer.Events.Count; j++)
            {
                Event @event = layer.Events[j];
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

            for (int j = 0; j < layer.Notes.Count; j++)
            {
                Note note = layer.Notes[j];
                if (note is HoldNote holdNote)
                {
                    for (int k = 0; k < holdNote.Points.Count; k++)
                    {
                        HoldPointNote point = holdNote.Points[k];
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
            
            operations.Add(new GenericEditOperation<int>(value =>
            { obj.Timestamp.FullTick = value; }, oldFullTick, newFullTick));
        }
    }

    public static void Transform_MirrorChart(int axis)
    {
        List<IOperation> operations = [];

        for (int i = 0; i < ChartSystem.Chart.LaneToggles.Count; i++)
        {
            Note laneToggle = ChartSystem.Chart.LaneToggles[i];
            if (laneToggle is not IPositionable positionable) continue;

            addOperation(positionable);
        }

        for (int i = 0; i < ChartSystem.Chart.Layers.Count; i++)
        {
            Layer layer = ChartSystem.Chart.Layers[i];
            for (int j = 0; j < layer.Notes.Count; j++)
            {
                Note note = layer.Notes[j];
                if (note is HoldNote holdNote)
                {
                    for (int k = 0; k < holdNote.Points.Count; k++)
                    {
                        HoldPointNote point = holdNote.Points[k];
                        addOperation(point);
                    }
                }
                else if (note is IPositionable positionable)
                {
                    addOperation(positionable);
                }
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
                
                int newPosition = axis - sourceClw.Size - sourceClw.Position;
                
                SlideCounterclockwiseNote newNote = new
                (
                    timestamp:     new(sourceClw.Timestamp.FullTick),
                    position:      newPosition,
                    size:          sourceClw.Size,
                    bonusType:     sourceClw.BonusType,
                    judgementType: sourceClw.JudgementType
                );
                
                operations.Add(new ListRemoveOperation<Note>(() => layer.Notes, sourceClw));
                operations.Add(new ListAddOperation<Note>(() => layer.Notes, newNote));

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
                
                int newPosition = axis - sourceCcw.Size - sourceCcw.Position;
                
                SlideClockwiseNote newNote = new
                (
                    timestamp:     new(sourceCcw.Timestamp.FullTick),
                    position:      newPosition,
                    size:          sourceCcw.Size,
                    bonusType:     sourceCcw.BonusType,
                    judgementType: sourceCcw.JudgementType
                );
                
                operations.Add(new ListRemoveOperation<Note>(() => layer.Notes, sourceCcw));
                operations.Add(new ListAddOperation<Note>(() => layer.Notes, newNote));

                if (SelectionSystem.SelectedObjects.Contains(sourceCcw))
                {
                    operations.Add(new SelectionRemoveOperation(sourceCcw, SelectionSystem.LastSelectedObject));
                    operations.Add(new SelectionAddOperation(newNote, SelectionSystem.LastSelectedObject));
                }
            }
            else if (positionable is HoldNote sourceHoldNote)
            {
                for (int i = 0; i < sourceHoldNote.Points.Count; i++)
                {
                    HoldPointNote holdPointNote = sourceHoldNote.Points[i];
                    int newPosition = axis - holdPointNote.Size - holdPointNote.Position;
                    operations.Add(new GenericEditOperation<int>(value => { holdPointNote.Position = value; }, holdPointNote.Position, newPosition));
                }
            }
            else
            {
                int newPosition = axis - positionable.Size - positionable.Position;
                operations.Add(new GenericEditOperation<int>(value => { positionable.Position = value; }, positionable.Position, newPosition));
            }

            if (positionable is ILaneToggle laneToggle)
            {
                if (laneToggle.Direction == LaneSweepDirection.Clockwise)
                {
                    operations.Add(new GenericEditOperation<LaneSweepDirection>(value => { laneToggle.Direction = value; }, laneToggle.Direction, LaneSweepDirection.Counterclockwise));
                }
                else if (laneToggle.Direction == LaneSweepDirection.Counterclockwise)
                {
                    operations.Add(new GenericEditOperation<LaneSweepDirection>(value => { laneToggle.Direction = value; }, laneToggle.Direction, LaneSweepDirection.Clockwise));
                }
            }
        }
    }

    
    public static void Convert_ZigZagHold(int beats, int division, int leftEdgeOffsetA, int leftEdgeOffsetB, int rightEdgeOffsetA,  int rightEdgeOffsetB)
    {
        List<IOperation> operations = [];

        int interval = 1920 * beats / division;
        if (interval <= 0) return;
        
        ITimeable[] selectedObjects = SelectionSystem.SelectedObjects.ToArray();
        
        foreach (ITimeable obj in selectedObjects)
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
            
            bool flip = SaturnMath.FlipHoldInterpolation(start, end);
            
            int startLeft = start.Position;
            int endLeft = end.Position;
            int startRight = start.Position + start.Size;
            int endRight = end.Position + end.Size;
            
            int deltaLeft = Math.Abs(startLeft - endLeft);
            int deltaRight = Math.Abs(startRight - endRight);
            deltaLeft = flip ? 60 - deltaLeft : deltaLeft;
            deltaRight = flip ? 60 - deltaRight : deltaRight;

            int signLeft = Math.Sign(startLeft - endLeft);
            int signRight = Math.Sign(startRight - endRight);
            signLeft = flip ? signLeft : -signLeft;
            signRight = flip ? signRight : -signRight;
            
            bool isA = true;
            
            for (int i = start.Timestamp.FullTick + interval; i < end.Timestamp.FullTick; i += interval)
            {
                float t = SaturnMath.InverseLerp(start.Timestamp.FullTick, end.Timestamp.FullTick, i);
                
                int leftEdge = (int)Math.Round(SaturnMath.Lerp(startLeft, startLeft + deltaLeft * signLeft, t));
                int rightEdge = (int)Math.Round(SaturnMath.Lerp(startRight, startRight + deltaRight * signRight, t));

                leftEdge -= isA ? leftEdgeOffsetA : leftEdgeOffsetB;
                rightEdge += isA ? rightEdgeOffsetA : rightEdgeOffsetB;

                int position;
                int size;
                
                if (leftEdge > rightEdge - 60)
                {
                    position = SaturnMath.Modulo(leftEdge, 60);
                    size = Math.Clamp(rightEdge - leftEdge, 1, 60);
                }
                else
                {
                    // Size 60 Overlap
                    position = SaturnMath.Modulo(30 + (leftEdge + rightEdge) / 2, 60);
                    size = 60;
                }
                
                HoldPointNote point = new
                (
                    timestamp:  new(i),
                    position:   position,
                    size:       size,
                    parent:     start.Parent,
                    renderType: HoldPointRenderType.Visible
                );
                
                operations.Add(new ListAddOperation<HoldPointNote>(() => start.Parent.Points, point));
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
                    operations.Add(new ListAddOperation<Note>(() => layer.Notes, newHoldNote));
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
                    operations.Add(new ListAddOperation<Note>(() => layer.Notes, newHoldNote));
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
        operations.Add(new ListRemoveOperation<Note>(() => layer.Notes, sourceHoldNote));

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

            for (int i = 0; i < h.Points.Count; i++)
            {
                HoldPointNote p = h.Points[i];
                if (!alreadyExistingPoints.Add((p.Timestamp.FullTick, p.Position, p.Size))) continue;

                newHoldNote.Points.Add(new
                (
                    timestamp: new(p.Timestamp.FullTick),
                    position: p.Position,
                    size: p.Size,
                    parent: newHoldNote,
                    renderType: p.RenderType
                ));
            }

            operations.Add(new ListRemoveOperation<Note>(() => l.Notes, h));
            operations.Add(new SelectionRemoveOperation(h, SelectionSystem.LastSelectedObject));
        }

        if (rootLayer == null) return;

        operations.Add(new ListAddOperation<Note>(() => rootLayer.Notes, newHoldNote));
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
    }

    
    public static void EventList_DeleteGlobalEvents()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];

        for (int i = 0; i < ChartSystem.Chart.Events.Count; i++)
        {
            Event @event = ChartSystem.Chart.Events[i];
            if (!SelectionSystem.SelectedObjects.Contains(@event)) continue;

            operations.Add(new SelectionRemoveOperation(@event, SelectionSystem.LastSelectedObject));
            operations.Add(new ListRemoveOperation<Event>(() => ChartSystem.Chart.Events, @event));
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

        ListRemoveOperation<Layer> op0 = new(() => ChartSystem.Chart.Layers, SelectionSystem.SelectedLayer);
        GenericEditOperation<Layer?> op1 = new(value => { SelectionSystem.SelectedLayer = value; }, SelectionSystem.SelectedLayer, newSelection);

        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }
    
    public static void LayerList_AddLayer()
    {
        Layer layer = ChartSystem.Chart.Layers.Count == 0 
            ? new("Main Layer") 
            : new("New Layer");
        
        ListAddOperation<Layer> op0 = new(() => ChartSystem.Chart.Layers, layer);
        GenericEditOperation<Layer?> op1 = new(value => { SelectionSystem.SelectedLayer = value; }, SelectionSystem.SelectedLayer, layer);
        
        UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
    }

    public static void LayerList_DeleteEvents()
    {
        if (SelectionSystem.SelectedLayer == null) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];

        for (int i = 0; i < SelectionSystem.SelectedLayer.Events.Count; i++)
        {
            Event @event = SelectionSystem.SelectedLayer.Events[i];
            if (!SelectionSystem.SelectedObjects.Contains(@event)) continue;

            operations.Add(new SelectionRemoveOperation(@event, SelectionSystem.LastSelectedObject));
            operations.Add(new ListRemoveOperation<Event>(() => SelectionSystem.SelectedLayer.Events, @event));
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