using System;
using System.Collections.Generic;
using System.Linq;
using SaturnData.Notation.Core;
using SaturnData.Notation.Events;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.EditModeOperations;
using SaturnEdit.UndoRedo.EventOperations;
using SaturnEdit.UndoRedo.HoldNoteOperations;
using SaturnEdit.UndoRedo.LayerOperations;
using SaturnEdit.UndoRedo.NoteOperations;
using SaturnEdit.UndoRedo.PositionableOperations;
using SaturnEdit.UndoRedo.SelectionOperations;
using SaturnEdit.UndoRedo.TimeableOperations;
using SaturnView;

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

    public static bool HoldEditModeAvailable => SelectionSystem.SelectedObjects.Any(x => x is HoldNote);
    public static bool EventEditModeAvailable => SelectionSystem.SelectedObjects.Any(x => x is StopEffectEvent or ReverseEffectEvent);

#region Methods
    public static void ChangeEditMode(EditorMode newMode)
    {
        EditModeChangeAttempted?.Invoke(null, EventArgs.Empty);
        
        CompositeOperation? op = GetEditModeChangeOperation(newMode);
        if (op == null || op.Operations.Count == 0) return;

        UndoRedoSystem.Push(op);
    }
    
    private static CompositeOperation? GetEditModeChangeOperation(EditorMode newMode)
    {
        if (Mode == newMode) return null;
        
        List<IOperation> operations = [];
        List<ITimeable> objects = SelectionSystem.OrderedSelectedObjects;
        
        ITimeable? newActiveObjectGroup = null;
        
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
        }
        // Changing to Edit Mode:
        else if (newMode is EditorMode.EditMode)
        {
            // Find the earliest selected object group.
            HoldNote? holdNote = null;
            StopEffectEvent? stopEffectEvent = null;
            ReverseEffectEvent? reverseEffectEvent = null;
            
            foreach (ITimeable obj in objects)
            {
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
            
            // Clear selection.
            foreach (ITimeable obj in objects)
            {
                operations.Add(new SelectionRemoveOperation(obj, SelectionSystem.LastSelectedObject));
            }
        }
        
        operations.Add(new EditModeChangeOperation(Mode, newMode, ActiveObjectGroup, newActiveObjectGroup));
        
        return new(operations);
    }
    
    public static void Edit_Cut()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;
    }

    public static void Edit_Copy()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;
    }

    public static void Edit_Paste()
    {
    }


    public static void Insert_AddTempoChange()
    {
        TempoChangeEvent tempoChangeEvent = new(new(TimeSystem.Timestamp.FullTick), 120.000000f);
        int index = ChartSystem.Chart.Events.FindLastIndex(x => x.Timestamp.FullTick <= tempoChangeEvent.Timestamp.FullTick);

        GlobalEventAddOperation op0 = new(tempoChangeEvent, index);
        SelectionAddOperation op1 = new(tempoChangeEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    public static void Insert_AddMetreChange()
    {
        MetreChangeEvent metreChangeEvent = new(new(TimeSystem.Timestamp.FullTick), 4, 4);
        int index = ChartSystem.Chart.Events.FindLastIndex(x => x.Timestamp.FullTick <= metreChangeEvent.Timestamp.FullTick);

        GlobalEventAddOperation op0 = new(metreChangeEvent, index);
        SelectionAddOperation op1 = new(metreChangeEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    public static void Insert_AddTutorialMarker()
    {
        TutorialMarkerEvent tutorialMarkerEvent = new(new(TimeSystem.Timestamp.FullTick), "KEY");
        int index = ChartSystem.Chart.Events.FindLastIndex(x => x.Timestamp.FullTick <= tutorialMarkerEvent.Timestamp.FullTick);

        GlobalEventAddOperation op0 = new(tutorialMarkerEvent, index);
        SelectionAddOperation op1 = new(tutorialMarkerEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }
    
    public static void Insert_AddSpeedChange()
    {
        if (SelectionSystem.SelectedLayer == null) return;
        
        SpeedChangeEvent speedChangeEvent = new(new(TimeSystem.Timestamp.FullTick), 1.000000f);
        int index = SelectionSystem.SelectedLayer.Events.FindLastIndex(x => x.Timestamp.FullTick <= speedChangeEvent.Timestamp.FullTick);

        EventAddOperation op0 = new(SelectionSystem.SelectedLayer, speedChangeEvent, index);
        SelectionAddOperation op1 = new(speedChangeEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    public static void Insert_AddVisibilityChange()
    {
        if (SelectionSystem.SelectedLayer == null) return;
        
        VisibilityChangeEvent visibilityChangeEvent = new(new(TimeSystem.Timestamp.FullTick), true);
        int index = SelectionSystem.SelectedLayer.Events.FindLastIndex(x => x.Timestamp.FullTick <= visibilityChangeEvent.Timestamp.FullTick);

        EventAddOperation op0 = new(SelectionSystem.SelectedLayer, visibilityChangeEvent, index);
        SelectionAddOperation op1 = new(visibilityChangeEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
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
        
        int index = SelectionSystem.SelectedLayer.Events.FindLastIndex(x => x.Timestamp.FullTick <= reverseEffectEvent.Timestamp.FullTick);
        
        EventAddOperation op0 = new(SelectionSystem.SelectedLayer, reverseEffectEvent, index);
        SelectionAddOperation op1 = new(reverseEffectEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    public static void Insert_AddStopEffect()
    {
        if (SelectionSystem.SelectedLayer == null) return;

        Timestamp start = new(TimeSystem.Timestamp.FullTick);
        Timestamp end = new(start.FullTick + 1920);
        
        StopEffectEvent stopEffectEvent = new();
        stopEffectEvent.SubEvents[0] = new(start, stopEffectEvent);
        stopEffectEvent.SubEvents[1] = new(end, stopEffectEvent);
        
        int index = SelectionSystem.SelectedLayer.Events.FindLastIndex(x => x.Timestamp.FullTick <= stopEffectEvent.Timestamp.FullTick);
        
        EventAddOperation op0 = new(SelectionSystem.SelectedLayer, stopEffectEvent, index);
        SelectionAddOperation op1 = new(stopEffectEvent, SelectionSystem.LastSelectedObject);
        
        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
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
        UndoRedoSystem.Push(new CompositeOperation(operations));

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
        UndoRedoSystem.Push(new CompositeOperation(operations));

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
        UndoRedoSystem.Push(new CompositeOperation(operations));

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
        UndoRedoSystem.Push(new CompositeOperation(operations));

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
        UndoRedoSystem.Push(new CompositeOperation(operations));

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
        UndoRedoSystem.Push(new CompositeOperation(operations));

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
        UndoRedoSystem.Push(new CompositeOperation(operations));

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
        UndoRedoSystem.Push(new CompositeOperation(operations));

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
        UndoRedoSystem.Push(new CompositeOperation(operations));

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
        UndoRedoSystem.Push(new CompositeOperation(operations));

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
        UndoRedoSystem.Push(new CompositeOperation(operations));

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
        UndoRedoSystem.Push(new CompositeOperation(operations));

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
        UndoRedoSystem.Push(new CompositeOperation(operations));
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
        UndoRedoSystem.Push(new CompositeOperation(operations));
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
        UndoRedoSystem.Push(new CompositeOperation(operations));
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
        UndoRedoSystem.Push(new CompositeOperation(operations));
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
        UndoRedoSystem.Push(new CompositeOperation(operations));

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
        UndoRedoSystem.Push(new CompositeOperation(operations));

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
        UndoRedoSystem.Push(new CompositeOperation(operations));

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
        
        UndoRedoSystem.Push(new CompositeOperation(operations));

        return;

        void processHoldSection(HoldPointNote start, HoldPointNote end)
        {
            if (start.Parent != end.Parent) return;
            if (start.Timestamp.FullTick == end.Timestamp.FullTick) return;

            bool isA = true;
            
            for (int i = start.Timestamp.FullTick + interval; i < end.Timestamp.FullTick; i += interval)
            {
                float t = (float)(i - start.Timestamp.FullTick) / (end.Timestamp.FullTick - start.Timestamp.FullTick);
                int position = (int)Math.Round(RenderUtils.LerpCyclic(start.Position, end.Position, t, 60));
                int size = (int)Math.Round(RenderUtils.Lerp(start.Size, end.Size, t));

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
        CompositeOperation? op = GetEditModeChangeOperation(EditorMode.ObjectMode);
        if (op == null) return;

        operations.Add(op);
        
        // Remove original hold note.
        int index = layer.Notes.IndexOf(sourceHoldNote);
        operations.Add(new NoteRemoveOperation(layer, sourceHoldNote, index));

        UndoRedoSystem.Push(new CompositeOperation(operations));
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
        UndoRedoSystem.Push(new CompositeOperation(operations));
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

        UndoRedoSystem.Push(new CompositeOperation(operations));
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

        UndoRedoSystem.Push(new LayerSwapOperation(layerA, layerB, indexA, indexB));
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

        UndoRedoSystem.Push(new LayerSwapOperation(layerA, layerB, indexA, indexB));
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

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }
    
    public static void LayerList_AddLayer()
    {
        Layer layer = ChartSystem.Chart.Layers.Count == 0 
            ? new("Main Layer") 
            : new("New Layer");
        
        int index = ChartSystem.Chart.Layers.Count;

        LayerAddOperation op0 = new(layer, index);
        LayerSelectOperation op1 = new(SelectionSystem.SelectedLayer, layer); 
        
        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
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

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

#endregion Methods

#region System Event Delegates
    private static void OnChartLoaded(object? sender, EventArgs e)
    {
        Mode = EditorMode.ObjectMode;
        ActiveObjectGroup = null;
    }
#endregion System Event Delegates
}