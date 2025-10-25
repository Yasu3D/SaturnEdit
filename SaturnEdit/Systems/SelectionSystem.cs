using System;
using System.Collections.Generic;
using System.Linq;
using SaturnData.Notation.Core;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.SelectionOperations;
using SaturnView;

namespace SaturnEdit.Systems;

public static class SelectionSystem
{
    public static void Initialize()
    {
        TimeSystem.PlaybackStateChanged += OnPlaybackStateChanged;
        SelectedLayer = ChartSystem.Chart.Layers.Count == 0 ? null : ChartSystem.Chart.Layers[0];
    }
    
    public static event EventHandler? PointerOverOverlapChanged;
    
    public static Layer? SelectedLayer { get; set; } = null;
    
    public static IPositionable.OverlapResult PointerOverOverlap
    {
        get => pointerOverOverlap;
        set
        {
            if (pointerOverOverlap == value) return;
            
            pointerOverOverlap = value;
            PointerOverOverlapChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static IPositionable.OverlapResult pointerOverOverlap = IPositionable.OverlapResult.None;
    
    public static ITimeable? PointerOverObject { get; set; } = null;
    
    public static ITimeable? LastSelectedObject { get; set; } = null;
    public static HashSet<ITimeable> SelectedObjects { get; } = [];
    
    public static BoxSelectData BoxSelectData { get; set; } = new();

#region Methods
    public static void SetSelection(bool control, bool shift)
    {
        List<IOperation> operations = [];
            
        // None
        // - clear
        // - add pointerObj
        if (!control && !shift)
        {
            foreach (ITimeable obj in SelectedObjects)
            {
                operations.Add(new RemoveSelectionOperation(obj, LastSelectedObject));
            }

            if (PointerOverObject != null)
            {
                operations.Add(new AddSelectionOperation(PointerOverObject, LastSelectedObject));
            }
        }
            
        // Ctrl
        // - toggle pointerObj
        if (control && !shift)
        {
            if (PointerOverObject == null) return;

            if (SelectedObjects.Contains(PointerOverObject))
            {
                operations.Add(new RemoveSelectionOperation(PointerOverObject, LastSelectedObject));
            }
            else
            {
                operations.Add(new AddSelectionOperation(PointerOverObject, LastSelectedObject));
            }
        }
            
        // Shift
        // - clear
        // - add from lastSelected to pointerObj
        if (!control && shift)
        {
            if (PointerOverObject == null) return;

            foreach (ITimeable obj in SelectedObjects)
            {
                operations.Add(new RemoveSelectionOperation(obj, LastSelectedObject));
            }

            Timestamp start;
            Timestamp end;
                
            if (LastSelectedObject == null)
            {
                start = PointerOverObject.Timestamp;
                end = PointerOverObject.Timestamp;
            }
            else
            {
                start = Timestamp.Min(LastSelectedObject.Timestamp, PointerOverObject.Timestamp);
                end = Timestamp.Max(LastSelectedObject.Timestamp, PointerOverObject.Timestamp);
            }
                
            foreach (Event @event in ChartSystem.Chart.Events)
            {
                if (!RenderUtils.IsVisible(@event, SettingsSystem.RenderSettings)) continue;
                if (@event.Timestamp < start) continue;
                if (@event.Timestamp > end) continue;

                operations.Add(new AddSelectionOperation(@event, LastSelectedObject));
            }

            foreach (Note note in ChartSystem.Chart.LaneToggles)
            {
                if (!RenderUtils.IsVisible(note, SettingsSystem.RenderSettings)) continue;
                if (note.Timestamp < start) continue;
                if (note.Timestamp > end) continue;

                operations.Add(new AddSelectionOperation(note, LastSelectedObject));
            }

            foreach (Layer layer in ChartSystem.Chart.Layers)
            {
                foreach (Event @event in layer.Events)
                {
                    if (!RenderUtils.IsVisible(@event, SettingsSystem.RenderSettings)) continue;
                    if (@event.Timestamp < start) continue;
                    if (@event.Timestamp > end) continue;

                    operations.Add(new AddSelectionOperation(@event, LastSelectedObject));
                }

                foreach (Note note in layer.Notes)
                {
                    if (!RenderUtils.IsVisible(note, SettingsSystem.RenderSettings)) continue;
                    if (note.Timestamp < start) continue;
                    if (note.Timestamp > end) continue;

                    operations.Add(new AddSelectionOperation(note, LastSelectedObject));
                }
            }
                
            operations.Add(new AddSelectionOperation(PointerOverObject, LastSelectedObject));

            if (LastSelectedObject != null)
            {
                operations.Add(new AddSelectionOperation(LastSelectedObject, LastSelectedObject));
            }
        }
            
        // Ctrl + Shift
        // - add from lastSelected to pointerObj
        if (control && shift)
        {
            if (PointerOverObject == null) return;

            Timestamp start;
            Timestamp end;
                
            if (LastSelectedObject == null)
            {
                start = PointerOverObject.Timestamp;
                end = PointerOverObject.Timestamp;
            }
            else
            {
                start = Timestamp.Min(LastSelectedObject.Timestamp, PointerOverObject.Timestamp);
                end = Timestamp.Max(LastSelectedObject.Timestamp, PointerOverObject.Timestamp);
            }
                
            foreach (Event @event in ChartSystem.Chart.Events)
            {
                if (!RenderUtils.IsVisible(@event, SettingsSystem.RenderSettings)) continue;
                if (@event.Timestamp < start) continue;
                if (@event.Timestamp > end) continue;

                operations.Add(new AddSelectionOperation(@event, LastSelectedObject));
            }

            foreach (Note note in ChartSystem.Chart.LaneToggles)
            {
                if (!RenderUtils.IsVisible(note, SettingsSystem.RenderSettings)) continue;
                if (note.Timestamp < start) continue;
                if (note.Timestamp > end) continue;

                operations.Add(new AddSelectionOperation(note, LastSelectedObject));
            }

            foreach (Layer layer in ChartSystem.Chart.Layers)
            {
                foreach (Event @event in layer.Events)
                {
                    if (!RenderUtils.IsVisible(@event, SettingsSystem.RenderSettings)) continue;
                    if (@event.Timestamp < start) continue;
                    if (@event.Timestamp > end) continue;

                    operations.Add(new AddSelectionOperation(@event, LastSelectedObject));
                }

                foreach (Note note in layer.Notes)
                {
                    if (!RenderUtils.IsVisible(note, SettingsSystem.RenderSettings)) continue;
                    if (note.Timestamp < start) continue;
                    if (note.Timestamp > end) continue;

                    operations.Add(new AddSelectionOperation(note, LastSelectedObject));
                }
            }
                
            operations.Add(new AddSelectionOperation(PointerOverObject, LastSelectedObject));

            if (LastSelectedObject != null)
            {
                operations.Add(new AddSelectionOperation(LastSelectedObject, LastSelectedObject));
            }
        }
            
        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    public static void SetBoxSelectionStart(bool negativeSelection, float viewTime)
    {
        BoxSelectData.NegativeSelection = negativeSelection;
        BoxSelectData.GlobalStartTime = TimeSystem.Timestamp.Time + viewTime;
        BoxSelectData.ScaledStartTimes.Clear();
        foreach (Layer layer in ChartSystem.Chart.Layers)
        {
            float scaledTime = Timestamp.ScaledTimeFromTime(layer, TimeSystem.Timestamp.Time);
            BoxSelectData.ScaledStartTimes[layer] = scaledTime + viewTime;
        }
    }

    public static void SetBoxSelectionEnd(int position, int size, float viewTime)
    {
        BoxSelectData.GlobalEndTime = TimeSystem.Timestamp.Time + viewTime;
        BoxSelectData.ScaledEndTimes.Clear();
        foreach (Layer layer in ChartSystem.Chart.Layers)
        {
            float scaledTime = Timestamp.ScaledTimeFromTime(layer, TimeSystem.Timestamp.Time);
            BoxSelectData.ScaledEndTimes[layer] = scaledTime + viewTime;
        }

        BoxSelectData.Position = position;
        BoxSelectData.Size = size;
    }
    
    public static void ApplyBoxSelection()
    {
        if (BoxSelectData.GlobalStartTime == null 
            || BoxSelectData.GlobalEndTime == null 
            || BoxSelectData.ScaledStartTimes.Count == 0 
            || BoxSelectData.ScaledEndTimes.Count == 0)
        {
            BoxSelectData = new();
            return;
        }
    
        float globalMin = MathF.Min((float)BoxSelectData.GlobalStartTime, (float)BoxSelectData.GlobalEndTime);
        float globalMax = MathF.Max((float)BoxSelectData.GlobalStartTime, (float)BoxSelectData.GlobalEndTime);

        List<IOperation> operations = [];
        
        foreach (Event @event in ChartSystem.Chart.Events)
        {
            if (!RenderUtils.IsVisible(@event, SettingsSystem.RenderSettings)) continue;
            if (@event.Timestamp.Time < globalMin) continue;
            if (@event.Timestamp.Time > globalMax) continue;

            if (BoxSelectData.NegativeSelection)
            {
                if (!SelectedObjects.Contains(@event)) continue;
                operations.Add(new RemoveSelectionOperation(@event, LastSelectedObject));
            }
            else
            {
                if (SelectedObjects.Contains(@event)) continue;
                operations.Add(new AddSelectionOperation(@event, LastSelectedObject));
            }
        }

        foreach (Note note in ChartSystem.Chart.LaneToggles)
        {
            if (!RenderUtils.IsVisible(note, SettingsSystem.RenderSettings)) continue;
            if (note.Timestamp.Time < globalMin) continue;
            if (note.Timestamp.Time > globalMax) continue;

            if (note is IPositionable positionable && !IPositionable.IsAnyOverlap(positionable.Position, positionable.Size, BoxSelectData.Position, BoxSelectData.Size)) continue;

            if (BoxSelectData.NegativeSelection)
            {
                if (!SelectedObjects.Contains(note)) continue;
                operations.Add(new RemoveSelectionOperation(note, LastSelectedObject));
            }
            else
            {
                if (!SelectedObjects.Contains(note)) continue;
                operations.Add(new AddSelectionOperation(note, LastSelectedObject));
            }
        }

        foreach (Layer layer in ChartSystem.Chart.Layers)
        {
            foreach (Event @event in layer.Events)
            {
                if (!RenderUtils.IsVisible(@event, SettingsSystem.RenderSettings)) continue;
                if (@event.Timestamp.Time < globalMin) continue;
                if (@event.Timestamp.Time > globalMax) continue;

                if (BoxSelectData.NegativeSelection)
                {
                    if (!SelectedObjects.Contains(@event)) continue;
                    operations.Add(new RemoveSelectionOperation(@event, LastSelectedObject));
                }
                else
                {
                    if (SelectedObjects.Contains(@event)) continue;
                    operations.Add(new AddSelectionOperation(@event, LastSelectedObject));
                }
            }

            foreach (Note note in layer.Notes)
            {
                if (!RenderUtils.IsVisible(note, SettingsSystem.RenderSettings)) continue;

                float min = MathF.Min(BoxSelectData.ScaledStartTimes[layer], BoxSelectData.ScaledEndTimes[layer]);
                float max = MathF.Max(BoxSelectData.ScaledStartTimes[layer], BoxSelectData.ScaledEndTimes[layer]);

                if (note is HoldNote holdNote && holdNote.Points.Count > 1)
                {
                    if (holdNote.Points[^1].Timestamp.ScaledTime < min) continue;
                    if (holdNote.Points[0].Timestamp.ScaledTime > max) continue;

                    bool overlap = false;
                    foreach (HoldPointNote point in holdNote.Points)
                    {
                        if (point.Timestamp.ScaledTime < min) continue;
                        if (point.Timestamp.ScaledTime > max) continue;
                        if (!IPositionable.IsAnyOverlap(point.Position, point.Size, BoxSelectData.Position, BoxSelectData.Size)) continue;

                        overlap = true;
                        break;
                    }

                    if (!overlap) continue;
                }
                else
                {
                    if (note.Timestamp.ScaledTime < min) continue;
                    if (note.Timestamp.ScaledTime > max) continue;

                    if (note is IPositionable positionable && !IPositionable.IsAnyOverlap(positionable.Position, positionable.Size, BoxSelectData.Position, BoxSelectData.Size)) continue;
                }

                if (BoxSelectData.NegativeSelection)
                {
                    if (!SelectedObjects.Contains(note)) continue;
                    operations.Add(new RemoveSelectionOperation(note, LastSelectedObject));
                }
                else
                {
                    if (SelectedObjects.Contains(note)) continue;
                    operations.Add(new AddSelectionOperation(note, LastSelectedObject));
                }
            }
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
        
        BoxSelectData = new();
    }

    public static void SelectAll()
    {
        List<IOperation> operations = [];

        foreach (Event @event in ChartSystem.Chart.Events)
        {
            if (SelectedObjects.Contains(@event)) continue;
            
            operations.Add(new AddSelectionOperation(@event, LastSelectedObject));
        }

        foreach (Note laneToggle in ChartSystem.Chart.LaneToggles)
        {
            if (SelectedObjects.Contains(laneToggle)) continue;
            
            operations.Add(new AddSelectionOperation(laneToggle, LastSelectedObject));
        }
        
        foreach (Layer layer in ChartSystem.Chart.Layers)
        {
            foreach (Event @event in layer.Events)
            {
                if (SelectedObjects.Contains(@event)) continue;
                
                operations.Add(new AddSelectionOperation(@event, LastSelectedObject));
            }
                        
            foreach (Note note in layer.Notes)
            {
                if (SelectedObjects.Contains(note)) continue;
                
                operations.Add(new AddSelectionOperation(note, LastSelectedObject));
            }
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    public static void DeselectAll()
    {
        if (SelectedObjects.Count == 0) return;
        List<IOperation> operations = [];

        foreach (ITimeable obj in SelectedObjects)
        {
            operations.Add(new RemoveSelectionOperation(obj, LastSelectedObject));
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    public static void CheckerDeselect()
    {
        if (SelectedObjects.Count == 0) return;
        List<ITimeable> objects = SelectedObjects.OrderBy(x => x.Timestamp.FullTick).ToList();
        List<IOperation> operations = [];
        
        int lastTick = objects[0].Timestamp.FullTick;
        bool keepSelected = true;
        
        for (int i = 0; i < objects.Count; i++)
        {
            ITimeable timeable = objects[i];

            if (timeable.Timestamp.FullTick != lastTick)
            {
                keepSelected = !keepSelected;
                lastTick = timeable.Timestamp.FullTick;
            }

            if (keepSelected) continue;

            operations.Add(new RemoveSelectionOperation(timeable, LastSelectedObject));
        }
        
        UndoRedoSystem.Push(new CompositeOperation(operations));
    }
#endregion Methods
    
#region System Event Delegates
    private static void OnPlaybackStateChanged(object? sender, EventArgs e)
    {
        PointerOverObject = null;
    }
#endregion System Event Delegates
}

public class BoxSelectData
{
    public float? GlobalStartTime = null;
    public float? GlobalEndTime = null;
    public readonly Dictionary<Layer, float> ScaledStartTimes = [];
    public readonly Dictionary<Layer, float> ScaledEndTimes = [];
    public bool NegativeSelection = false;

    public int Position = 0;
    public int Size = 0;
}