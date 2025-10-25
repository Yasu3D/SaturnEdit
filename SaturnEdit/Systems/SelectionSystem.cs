using System;
using System.Collections.Generic;
using System.Linq;
using SaturnData.Notation.Core;
using SaturnData.Notation.Events;
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
    
    public static BoxSelectArgs BoxSelectArgs { get; set; } = new();
    public static SelectByCriteriaArgs SelectByCriteriaArgs { get; set; } = new();
    
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
        BoxSelectArgs.NegativeSelection = negativeSelection;
        BoxSelectArgs.GlobalStartTime = TimeSystem.Timestamp.Time + viewTime;
        BoxSelectArgs.ScaledStartTimes.Clear();
        foreach (Layer layer in ChartSystem.Chart.Layers)
        {
            float scaledTime = Timestamp.ScaledTimeFromTime(layer, TimeSystem.Timestamp.Time);
            BoxSelectArgs.ScaledStartTimes[layer] = scaledTime + viewTime;
        }
    }

    public static void SetBoxSelectionEnd(int position, int size, float viewTime)
    {
        BoxSelectArgs.GlobalEndTime = TimeSystem.Timestamp.Time + viewTime;
        BoxSelectArgs.ScaledEndTimes.Clear();
        foreach (Layer layer in ChartSystem.Chart.Layers)
        {
            float scaledTime = Timestamp.ScaledTimeFromTime(layer, TimeSystem.Timestamp.Time);
            BoxSelectArgs.ScaledEndTimes[layer] = scaledTime + viewTime;
        }

        BoxSelectArgs.Position = position;
        BoxSelectArgs.Size = size;
    }
    
    public static void ApplyBoxSelection()
    {
        if (BoxSelectArgs.GlobalStartTime == null 
            || BoxSelectArgs.GlobalEndTime == null 
            || BoxSelectArgs.ScaledStartTimes.Count == 0 
            || BoxSelectArgs.ScaledEndTimes.Count == 0)
        {
            BoxSelectArgs = new();
            return;
        }
    
        float globalMin = MathF.Min((float)BoxSelectArgs.GlobalStartTime, (float)BoxSelectArgs.GlobalEndTime);
        float globalMax = MathF.Max((float)BoxSelectArgs.GlobalStartTime, (float)BoxSelectArgs.GlobalEndTime);

        List<IOperation> operations = [];
        
        foreach (Event @event in ChartSystem.Chart.Events)
        {
            if (!RenderUtils.IsVisible(@event, SettingsSystem.RenderSettings)) continue;
            if (@event.Timestamp.Time < globalMin) continue;
            if (@event.Timestamp.Time > globalMax) continue;

            if (BoxSelectArgs.NegativeSelection)
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

            if (note is IPositionable positionable && !IPositionable.IsAnyOverlap(positionable.Position, positionable.Size, BoxSelectArgs.Position, BoxSelectArgs.Size)) continue;

            if (BoxSelectArgs.NegativeSelection)
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

                if (BoxSelectArgs.NegativeSelection)
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

                float min = MathF.Min(BoxSelectArgs.ScaledStartTimes[layer], BoxSelectArgs.ScaledEndTimes[layer]);
                float max = MathF.Max(BoxSelectArgs.ScaledStartTimes[layer], BoxSelectArgs.ScaledEndTimes[layer]);

                if (note is HoldNote holdNote && holdNote.Points.Count > 1)
                {
                    if (holdNote.Points[^1].Timestamp.ScaledTime < min) continue;
                    if (holdNote.Points[0].Timestamp.ScaledTime > max) continue;

                    bool overlap = false;
                    foreach (HoldPointNote point in holdNote.Points)
                    {
                        if (point.Timestamp.ScaledTime < min) continue;
                        if (point.Timestamp.ScaledTime > max) continue;
                        if (!IPositionable.IsAnyOverlap(point.Position, point.Size, BoxSelectArgs.Position, BoxSelectArgs.Size)) continue;

                        overlap = true;
                        break;
                    }

                    if (!overlap) continue;
                }
                else
                {
                    if (note.Timestamp.ScaledTime < min) continue;
                    if (note.Timestamp.ScaledTime > max) continue;

                    if (note is IPositionable positionable && !IPositionable.IsAnyOverlap(positionable.Position, positionable.Size, BoxSelectArgs.Position, BoxSelectArgs.Size)) continue;
                }

                if (BoxSelectArgs.NegativeSelection)
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
        
        BoxSelectArgs = new();
    }

    public static void SelectAll()
    {
        List<IOperation> operations = [];

        foreach (Event @event in ChartSystem.Chart.Events)
        {
            if (SelectedObjects.Contains(@event)) continue;
            
            operations.Add(new AddSelectionOperation(@event, LastSelectedObject));
        }

        foreach (Bookmark bookmark in ChartSystem.Chart.Bookmarks)
        {
            if (SelectedObjects.Contains(bookmark)) continue;

            operations.Add(new AddSelectionOperation(bookmark, LastSelectedObject));
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

    public static void SelectByCriteria()
    {
        if (SelectByCriteriaArgs.FilterSelection)
        {
            filterSelection();
        }
        else
        {
            selectByCriteria();
        }

        return;
        
        void selectByCriteria()
        {
            List<IOperation> operations = [];
            
            foreach (Event @event in ChartSystem.Chart.Events)
            {
                if (SelectedObjects.Contains(@event)) continue;
                
                if (@event is TempoChangeEvent    && !SelectByCriteriaArgs.IncludeTempoChangeEvents   ) continue;
                if (@event is MetreChangeEvent    && !SelectByCriteriaArgs.IncludeMetreChangeEvents   ) continue;
                if (@event is TutorialMarkerEvent && !SelectByCriteriaArgs.IncludeTutorialMarkerEvents) continue;
                
                operations.Add(new AddSelectionOperation(@event, LastSelectedObject));
            }

            if (SelectByCriteriaArgs.IncludeBookmarks)
            {
                foreach (Bookmark bookmark in ChartSystem.Chart.Bookmarks)
                {
                    if (SelectedObjects.Contains(bookmark)) continue;

                    operations.Add(new AddSelectionOperation(bookmark, LastSelectedObject));
                }
            }
            
            foreach (Note laneToggle in ChartSystem.Chart.LaneToggles)
            {
                if (SelectedObjects.Contains(laneToggle)) continue;
                
                if (laneToggle is LaneShowNote && !SelectByCriteriaArgs.IncludeLaneShowNotes) continue;
                if (laneToggle is LaneHideNote && !SelectByCriteriaArgs.IncludeLaneHideNotes) continue;
                
                if (laneToggle is IPositionable positionable)
                {
                    int positionDifference = Math.Abs(positionable.Position - SelectByCriteriaArgs.Position);
                    positionDifference = positionDifference > 30 ? 60 - positionDifference : positionDifference;
                    
                    int sizeDifference = Math.Abs(positionable.Size - SelectByCriteriaArgs.Size);
                    
                    if (positionDifference > SelectByCriteriaArgs.PositionVariance) continue;
                    if (sizeDifference > SelectByCriteriaArgs.SizeVariance) continue;
                }
                
                operations.Add(new AddSelectionOperation(laneToggle, LastSelectedObject));
            }
            
            foreach (Layer layer in ChartSystem.Chart.Layers)
            {
                foreach (Event @event in layer.Events)
                {
                    if (SelectedObjects.Contains(@event)) continue;
                    
                    if (@event is SpeedChangeEvent          && !SelectByCriteriaArgs.IncludeSpeedChangeEvents         ) continue;
                    if (@event is VisibilityChangeEvent     && !SelectByCriteriaArgs.IncludeVisibilityChangeEvents    ) continue;
                    if (@event is ReverseEffectEvent        && !SelectByCriteriaArgs.IncludeReverseEffectEvents       ) continue;
                    if (@event is StopEffectEvent           && !SelectByCriteriaArgs.IncludeStopEffectEvents          ) continue;
                    
                    operations.Add(new AddSelectionOperation(@event, LastSelectedObject));
                }
                            
                foreach (Note note in layer.Notes)
                {
                    if (SelectedObjects.Contains(note)) continue;
                    
                    if (note is TouchNote                 && !SelectByCriteriaArgs.IncludeTouchNotes                ) continue;
                    if (note is ChainNote                 && !SelectByCriteriaArgs.IncludeChainNotes                ) continue;
                    if (note is HoldNote                  && !SelectByCriteriaArgs.IncludeHoldNotes                 ) continue;
                    if (note is SlideClockwiseNote        && !SelectByCriteriaArgs.IncludeSlideClockwiseNotes       ) continue;
                    if (note is SlideCounterclockwiseNote && !SelectByCriteriaArgs.IncludeSlideCounterclockwiseNotes) continue;
                    if (note is SnapForwardNote           && !SelectByCriteriaArgs.IncludeSnapForwardNotes          ) continue;
                    if (note is SnapBackwardNote          && !SelectByCriteriaArgs.IncludeSnapBackwardNotes         ) continue;
                    if (note is SyncNote                  && !SelectByCriteriaArgs.IncludeSyncNotes                 ) continue;
                    if (note is MeasureLineNote           && !SelectByCriteriaArgs.IncludeMeasureLineNotes          ) continue;
                    
                    if (note is IPositionable positionable)
                    {
                        int positionDifference = Math.Abs(positionable.Position - SelectByCriteriaArgs.Position);
                        positionDifference = positionDifference > 30 ? 60 - positionDifference : positionDifference;
                        
                        int sizeDifference = Math.Abs(positionable.Size - SelectByCriteriaArgs.Size);
                        
                        if (positionDifference > SelectByCriteriaArgs.PositionVariance) continue;
                        if (sizeDifference > SelectByCriteriaArgs.SizeVariance) continue;
                    }
                    
                    operations.Add(new AddSelectionOperation(note, LastSelectedObject));
                }
            }
            
            UndoRedoSystem.Push(new CompositeOperation(operations));
        }
        
        void filterSelection()
        {
            if (SelectedObjects.Count == 0) return;

            List<IOperation> operations = [];

            foreach (ITimeable obj in SelectedObjects)
            {
                if (obj is Bookmark                  && !SelectByCriteriaArgs.IncludeBookmarks                 ) continue;
                if (obj is TouchNote                 && !SelectByCriteriaArgs.IncludeTouchNotes                ) continue;
                if (obj is ChainNote                 && !SelectByCriteriaArgs.IncludeChainNotes                ) continue;
                if (obj is HoldNote                  && !SelectByCriteriaArgs.IncludeHoldNotes                 ) continue;
                if (obj is SlideClockwiseNote        && !SelectByCriteriaArgs.IncludeSlideClockwiseNotes       ) continue;
                if (obj is SlideCounterclockwiseNote && !SelectByCriteriaArgs.IncludeSlideCounterclockwiseNotes) continue;
                if (obj is SnapForwardNote           && !SelectByCriteriaArgs.IncludeSnapForwardNotes          ) continue;
                if (obj is SnapBackwardNote          && !SelectByCriteriaArgs.IncludeSnapBackwardNotes         ) continue;
                if (obj is SyncNote                  && !SelectByCriteriaArgs.IncludeSyncNotes                 ) continue;
                if (obj is MeasureLineNote           && !SelectByCriteriaArgs.IncludeMeasureLineNotes          ) continue;
                if (obj is LaneShowNote              && !SelectByCriteriaArgs.IncludeLaneShowNotes             ) continue;
                if (obj is LaneHideNote              && !SelectByCriteriaArgs.IncludeLaneHideNotes             ) continue;
                if (obj is TempoChangeEvent          && !SelectByCriteriaArgs.IncludeTempoChangeEvents         ) continue;
                if (obj is MetreChangeEvent          && !SelectByCriteriaArgs.IncludeMetreChangeEvents         ) continue;
                if (obj is TutorialMarkerEvent       && !SelectByCriteriaArgs.IncludeTutorialMarkerEvents      ) continue;
                if (obj is SpeedChangeEvent          && !SelectByCriteriaArgs.IncludeSpeedChangeEvents         ) continue;
                if (obj is VisibilityChangeEvent     && !SelectByCriteriaArgs.IncludeVisibilityChangeEvents    ) continue;
                if (obj is ReverseEffectEvent        && !SelectByCriteriaArgs.IncludeReverseEffectEvents       ) continue;
                if (obj is StopEffectEvent           && !SelectByCriteriaArgs.IncludeStopEffectEvents          ) continue;

                if (obj is IPositionable positionable)
                {
                    int positionDifference = Math.Abs(positionable.Position - SelectByCriteriaArgs.Position);
                    positionDifference = positionDifference > 30 ? 60 - positionDifference : positionDifference;
                    
                    int sizeDifference = Math.Abs(positionable.Size - SelectByCriteriaArgs.Size);
                    
                    if (positionDifference > SelectByCriteriaArgs.PositionVariance) continue;
                    if (sizeDifference > SelectByCriteriaArgs.SizeVariance) continue;
                }
                
                operations.Add(new RemoveSelectionOperation(obj, LastSelectedObject));
            }
            
            UndoRedoSystem.Push(new CompositeOperation(operations));
        }
    }
#endregion Methods
    
#region System Event Delegates
    private static void OnPlaybackStateChanged(object? sender, EventArgs e)
    {
        PointerOverObject = null;
    }
#endregion System Event Delegates
}

public class BoxSelectArgs
{
    public float? GlobalStartTime = null;
    public float? GlobalEndTime = null;
    public readonly Dictionary<Layer, float> ScaledStartTimes = [];
    public readonly Dictionary<Layer, float> ScaledEndTimes = [];
    public bool NegativeSelection = false;

    public int Position = 0;
    public int Size = 0;
}

public class SelectByCriteriaArgs
{
    public int Position = 0;
    public int PositionVariance = 0;
    public int Size = 15;
    public int SizeVariance = 0;

    public bool FilterSelection = false;

    public bool IncludeTouchNotes = false;
    public bool IncludeChainNotes = false;
    public bool IncludeHoldNotes = false;
    public bool IncludeSlideClockwiseNotes = false;
    public bool IncludeSlideCounterclockwiseNotes = false;
    public bool IncludeSnapForwardNotes = false;
    public bool IncludeSnapBackwardNotes = false;
    public bool IncludeSyncNotes = false;
    public bool IncludeMeasureLineNotes = false;
    public bool IncludeLaneShowNotes = false;
    public bool IncludeLaneHideNotes = false;
    public bool IncludeTempoChangeEvents = false;
    public bool IncludeMetreChangeEvents = false;
    public bool IncludeSpeedChangeEvents = false;
    public bool IncludeVisibilityChangeEvents = false;
    public bool IncludeReverseEffectEvents = false;
    public bool IncludeStopEffectEvents = false;
    public bool IncludeTutorialMarkerEvents = false;
    public bool IncludeBookmarks = false;
}