using System;
using System.Collections.Generic;
using SaturnData.Notation.Core;
using SaturnData.Notation.Events;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.PositionableOperations;
using SaturnEdit.UndoRedo.TimeableOperations;

namespace SaturnEdit.Utilities;

public class ObjectDragHelper(ClickDragHelper clickDragHelper)
{
    public bool IsActive { get; private set; } = false;
    
    public int StartTick { get; private set; } = -1;
    public int StartLane { get; private set; } = -1;
    public int EndTick { get; private set; } = -1;
    public int EndLane { get; private set; } = -1;
    
    public IPositionable.OverlapResult DragType { get; private set; } = IPositionable.OverlapResult.None;
    public List<ObjectDragItem>? DraggedObjects { get; private set; } = null;

#region Methods
    public void Start(int fullTick)
    {
        IsActive = SelectionSystem.PointerOverObject != null;
        DragType = SelectionSystem.PointerOverOverlap;
        StartTick = fullTick;
        EndTick = fullTick;
        StartLane = clickDragHelper.StartLane;
        EndLane = clickDragHelper.StartLane;
        DraggedObjects = [];

        foreach (ITimeable obj in SelectionSystem.SelectedObjects)
        {
            int position = 0;
            int size = 0;

            if (obj is IPositionable positionable)
            {
                position = positionable.Position;
                size = positionable.Size;
            }

            List<ObjectDragItem>? subItems = null;

            if (obj is HoldNote holdNote)
            {
                subItems = [];

                foreach (HoldPointNote point in holdNote.Points)
                {
                    subItems.Add(new(point, point.Timestamp.FullTick, point.Position, point.Size, null));
                }
            }
            else if (obj is StopEffectEvent stopEffectEvent)
            {
                subItems = [];

                foreach (EffectSubEvent subEvent in stopEffectEvent.SubEvents)
                {
                    subItems.Add(new(subEvent, subEvent.Timestamp.FullTick, 0, 0, null));
                }
            }
            else if (obj is ReverseEffectEvent reverseEffectEvent)
            {
                subItems = [];

                foreach (EffectSubEvent subEvent in reverseEffectEvent.SubEvents)
                {
                    subItems.Add(new(subEvent, subEvent.Timestamp.FullTick, 0, 0, null));
                }
            }
            
            DraggedObjects.Add(new(obj, obj.Timestamp.FullTick, position, size, subItems));
        }
    }

    public void Update(float time)
    {
        if (!IsActive || DraggedObjects == null) return;
        
        int fullTick = Timestamp.TimestampFromTime(ChartSystem.Chart, time).FullTick;

        float m = 1920.0f / TimeSystem.Division;
        fullTick = (int)(Math.Round(fullTick / m) * m);
        
        if (clickDragHelper.EndLane != EndLane || EndTick != fullTick)
        {
            bool rebuild = DragType == IPositionable.OverlapResult.Body && EndTick != fullTick;
            
            EndLane = clickDragHelper.EndLane;
            EndTick = fullTick;
            
            CompositeOperation compositeOperation = GetOperations();
            compositeOperation.Apply();

            if (rebuild)
            {
                ChartSystem.Rebuild();
            }
        }
    }

    public void End()
    {
        if (!IsActive) return;
        
        UndoRedoSystem.Push(GetOperations());
        
        IsActive = false;
        DragType = IPositionable.OverlapResult.None;
        StartTick = -1;
        EndTick = -1;
        EndLane = -1;
        DraggedObjects = null;
    }

    private CompositeOperation GetOperations()
    {
        if (DraggedObjects == null) return new([]);
        
        List<IOperation> operations = [];
        foreach (ObjectDragItem item in DraggedObjects)
        {
            if (item.SubItems != null)
            {
                foreach (ObjectDragItem subItem in item.SubItems)
                {
                    addOperation(subItem);
                }
            }
            else
            {
                addOperation(item);
            }
        }
        
        return new(operations);

        void addOperation(ObjectDragItem item)
        {
            if (DragType == IPositionable.OverlapResult.Body)
            {
                int newFullTick = item.Tick + EndTick - StartTick;
                operations.Add(new TimeableEditOperation(item.Timeable, item.Timeable.Timestamp.FullTick, newFullTick));

                if (item.Timeable is IPositionable positionable)
                {
                    int newPosition = item.Position + clickDragHelper.Tally;
                    operations.Add(new PositionableEditOperation(positionable, positionable.Position, newPosition, positionable.Size, positionable.Size));
                }
            }
            else if (DragType == IPositionable.OverlapResult.LeftEdge)
            {
                if (item.Timeable is not IPositionable positionable) return;

                int newPosition = Math.Min(item.Position + item.Size - 1, item.Position + clickDragHelper.Tally);
                int newSize = Math.Max(1, item.Size - clickDragHelper.Tally);
                operations.Add(new PositionableEditOperation(positionable, positionable.Position, newPosition, positionable.Size, newSize));
            }
            else if (DragType == IPositionable.OverlapResult.RightEdge)
            {
                if (item.Timeable is not IPositionable positionable) return;

                int newSize = Math.Max(1, item.Size + clickDragHelper.Tally);
                operations.Add(new PositionableEditOperation(positionable, positionable.Position, positionable.Position, positionable.Size, newSize));
            }
        }
    }
#endregion Methods
}

public struct ObjectDragItem(ITimeable timeable, int tick, int position, int size, List<ObjectDragItem>? subItems)
{
    public readonly ITimeable Timeable = timeable;
    public readonly int Tick = tick;
    public readonly int Position = position;
    public readonly int Size = size;

    public readonly List<ObjectDragItem>? SubItems = subItems;
}