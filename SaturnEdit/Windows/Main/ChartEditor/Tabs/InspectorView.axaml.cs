using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using SaturnData.Notation.Core;
using SaturnData.Notation.Events;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.BookmarkOperations;
using SaturnEdit.UndoRedo.EventOperations;
using SaturnEdit.UndoRedo.HoldNoteOperations;
using SaturnEdit.UndoRedo.NoteOperations;
using SaturnEdit.UndoRedo.PlayableOperations;
using SaturnEdit.UndoRedo.PositionableOperations;
using SaturnEdit.UndoRedo.SelectionOperations;
using SaturnEdit.UndoRedo.TimeableOperations;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class InspectorView : UserControl
{
    public InspectorView()
    {
        InitializeComponent();

        UndoRedoSystem.OperationHistoryChanged += OnOperationHistoryChanged;
        OnOperationHistoryChanged(null, EventArgs.Empty);
    }

    private bool blockEvents = false;
    
#region System Event Delegates
    private void OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        if (Design.IsDesignMode)
        {
            Dispatcher.UIThread.Post(() =>
            {
                TextBlockNothingSelected.IsVisible = false;
                
                GroupGeneral.IsVisible = true;
                GroupLayers.IsVisible = true;
                GroupShape.IsVisible = true;
                GroupJudgement.IsVisible = true;
                GroupHold.IsVisible = true;
                GroupLaneToggles.IsVisible = true;
                GroupTempoChange.IsVisible = true;
                GroupMetreChange.IsVisible = true;
                GroupSpeedChange.IsVisible = true;
                GroupVisibilityChange.IsVisible = true;
                GroupTutorialMarker.IsVisible = true;
                GroupBookmark.IsVisible = true;
            });
            
            return;
        }
        
        if (SelectionSystem.SelectedObjects.Count == 0)
        {
            Dispatcher.UIThread.Post(() =>
            {
                blockEvents = true;

                TextBlockNothingSelected.IsVisible = true;
                
                GroupGeneral.IsVisible = false;
                GroupLayers.IsVisible = false;
                GroupShape.IsVisible = false;
                GroupJudgement.IsVisible = false;
                GroupHold.IsVisible = false;
                GroupLaneToggles.IsVisible = false;
                GroupTempoChange.IsVisible = false;
                GroupMetreChange.IsVisible = false;
                GroupSpeedChange.IsVisible = false;
                GroupVisibilityChange.IsVisible = false;
                GroupTutorialMarker.IsVisible = false;
                GroupBookmark.IsVisible = false;
                
                blockEvents = false;
            });

            return;
        }
        
        bool showLayers = false;
        bool showShape = false;
        bool showJudgement = false;
        bool showHold = false;
        bool showLaneToggle = false;
        bool showTempo = false;
        bool showMetre = false;
        bool showSpeed = false;
        bool showVisibility = false;
        bool showTutorial = false;
        bool showBookmark = false;

        bool sameType = true;
        Type? sharedType = null;

        bool sameMeasure = true;
        int? sharedMeasure = null;
        
        bool sameTick = true;
        int? sharedTick = null;
        
        bool sameFullTick = true;
        int? sharedFullTick = null;

        bool sameLayer = true;
        Layer? sharedLayer = null;

        bool samePosition = true;
        int? sharedPosition = null;
        
        bool sameSize = true;
        int? sharedSize = null;

        bool sameBonusType = true;
        BonusType? sharedBonusType = null;
        
        bool sameJudgementType = true;
        JudgementType? sharedJudgementType = null;

        bool sameHoldPointRenderType = true;
        HoldPointRenderType? sharedHoldPointRenderType = null;

        bool sameLaneSweepDirection = true;
        LaneSweepDirection? sharedLaneSweepDirection = null;

        bool sameTempo = true;
        float? sharedTempo = null;

        bool sameMetreUpper = true;
        int? sharedMetreUpper = null;

        bool sameMetreLower = true;
        int? sharedMetreLower = null;

        bool sameSpeed = true;
        float? sharedSpeed = null;

        bool sameVisibility = true;
        bool? sharedVisibility = null;

        bool sameKey = true;
        string? sharedKey = null;

        bool sameColor = true;
        uint? sharedColor = null;

        bool sameMessage = true;
        string? sharedMessage = null;
        
        foreach (ITimeable obj in SelectionSystem.SelectedObjects)
        {
            // Check if all objects are the same type.
            sharedType ??= obj.GetType();
            sameType = sameType && obj.GetType() == sharedType;
            
            // Check if all objects are on the same timestamp.
            sharedMeasure ??= obj.Timestamp.Measure;
            sameMeasure = sameMeasure && obj.Timestamp.Measure == sharedMeasure;
            
            sharedTick ??= obj.Timestamp.Tick;
            sameTick = sameTick && obj.Timestamp.Tick == sharedTick;
            
            sharedFullTick ??= obj.Timestamp.FullTick;
            sameFullTick = sameFullTick && obj.Timestamp.FullTick == sharedFullTick;
            
            if (obj is not (TempoChangeEvent or MetreChangeEvent or TutorialMarkerEvent or ILaneToggle or Bookmark))
            {
                showLayers = true;
                
                // Check if all layer-bound objects are on the same layer.
                sharedLayer ??= ChartSystem.Chart.ParentLayer(obj);
                sameLayer = sameLayer && ChartSystem.Chart.ParentLayer(obj) == sharedLayer;
            }

            if (obj is IPositionable positionable)
            {
                showShape = true;
                
                // Check if all positionable objects have the same position and size.
                sharedPosition ??= positionable.Position;
                sharedSize ??= positionable.Size;
                
                samePosition = samePosition && positionable.Position == sharedPosition;
                sameSize = sameSize && positionable.Size == sharedSize;
            }

            if (obj is IPlayable playable)
            {
                showJudgement = true;
                
                // Check if all playable objects have the same bonus and judgement types.
                sharedBonusType ??= playable.BonusType;
                sameBonusType = sameBonusType && playable.BonusType == sharedBonusType;
                
                sharedJudgementType ??= playable.JudgementType;
                sameJudgementType = sameJudgementType && playable.JudgementType == sharedJudgementType;
            }

            if (obj is HoldPointNote holdPoint)
            {
                showHold = true;
                
                // Check if all hold points have the same hold point render type.
                sharedHoldPointRenderType ??= holdPoint.RenderType;
                sameHoldPointRenderType = sameHoldPointRenderType && holdPoint.RenderType == sharedHoldPointRenderType;
            }

            if (obj is ILaneToggle laneToggle)
            {
                showLaneToggle = true;
                
                // Check if all lane toggles have the same lane sweep direction.
                sharedLaneSweepDirection ??= laneToggle.Direction;
                sameLaneSweepDirection = sameLaneSweepDirection && laneToggle.Direction == sharedLaneSweepDirection;
            }

            if (obj is TempoChangeEvent tempoChange)
            {
                showTempo = true;
                
                // Check if all tempo changes have the same tempo.
                sharedTempo ??= tempoChange.Tempo;
                sameTempo = sameTempo && tempoChange.Tempo == sharedTempo;
            }
            
            if (obj is MetreChangeEvent metreChange)
            {
                showMetre = true;
                
                // Check if all metre changes have the same upper and lower.
                sharedMetreLower ??= metreChange.Lower;
                sameMetreLower = sameMetreLower && metreChange.Lower == sharedMetreLower;
                
                sharedMetreUpper ??= metreChange.Upper;
                sameMetreUpper = sameMetreUpper && metreChange.Upper == sharedMetreUpper;
            }
            
            if (obj is SpeedChangeEvent speedChange)
            {
                showSpeed = true;
                
                // Check if all speed changes have the same speed.
                sharedSpeed ??= speedChange.Speed;
                sameSpeed = sameSpeed && speedChange.Speed == sharedSpeed;
            }
            
            if (obj is VisibilityChangeEvent visibilityChange)
            {
                showVisibility = true;
                    
                // Check if all visibility changes have the same visibility.
                sharedVisibility ??= visibilityChange.Visibility;
                sameVisibility = sameVisibility && visibilityChange.Visibility == sharedVisibility;
            }
            
            if (obj is TutorialMarkerEvent tutorialMarker)
            {
                showTutorial = true;
                
                // Check if all tutorial markers have the same key.
                sharedKey ??= tutorialMarker.Key;
                sameKey = sameKey && tutorialMarker.Key == sharedKey;
            }
            
            if (obj is Bookmark bookmark)
            {
                showBookmark = true;
                
                // Check if all bookmarks have the same color and message.
                sharedColor ??= bookmark.Color;
                sameColor = sameColor && bookmark.Color == sharedColor;
                
                sharedMessage ??= bookmark.Message;
                sameMessage = sameMessage && bookmark.Message == sharedMessage;
            }
        }
        
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            // Set Group Visibility
            TextBlockNothingSelected.IsVisible = false;
            
            GroupGeneral.IsVisible = true;
            GroupLayers.IsVisible = showLayers;
            GroupShape.IsVisible = showShape;
            GroupJudgement.IsVisible = showJudgement;
            GroupHold.IsVisible = showHold;
            GroupLaneToggles.IsVisible = showLaneToggle;
            GroupTempoChange.IsVisible = showTempo;
            GroupMetreChange.IsVisible = showMetre;
            GroupSpeedChange.IsVisible = showSpeed;
            GroupVisibilityChange.IsVisible = showVisibility;
            GroupTutorialMarker.IsVisible = showTutorial;
            GroupBookmark.IsVisible = showBookmark;
            
            // Update Layer List
            if (ChartSystem.Chart.Layers.Count == 0)
            {
                ComboBoxLayers.IsEnabled = false;
            }
            else
            {
                ComboBoxLayers.IsEnabled = true;
                
                for (int i = 0; i < ChartSystem.Chart.Layers.Count; i++)
                {
                    Layer layer = ChartSystem.Chart.Layers[i];
                
                    if (i < ComboBoxLayers.Items.Count)
                    {
                        // Modify existing item.
                        if (ComboBoxLayers.Items[i] is not ComboBoxItem item) continue;

                        item.Content = layer.Name;
                    }
                    else
                    {
                        // Create new item.
                        ComboBoxItem item = new()
                        {
                            Content = layer.Name,
                        };
                    
                        ComboBoxLayers.Items.Add(item);
                    }
                }
            
                // Delete redundant items.
                for (int i = ComboBoxLayers.Items.Count - 1; i >= ChartSystem.Chart.Layers.Count; i--)
                {
                    ComboBoxLayers.Items.Remove(ComboBoxLayers.Items[i]);
                }
            }
            
            // Set general group values.
            if (sameType)
            {
                if      (sharedType == typeof(TouchNote))                 { ComboBoxType.SelectedIndex =  0; }
                else if (sharedType == typeof(ChainNote))                 { ComboBoxType.SelectedIndex =  1; }
                else if (sharedType == typeof(HoldNote))                  { ComboBoxType.SelectedIndex =  2; }
                else if (sharedType == typeof(SlideClockwiseNote))        { ComboBoxType.SelectedIndex =  3; }
                else if (sharedType == typeof(SlideCounterclockwiseNote)) { ComboBoxType.SelectedIndex =  4; }
                else if (sharedType == typeof(SnapForwardNote))           { ComboBoxType.SelectedIndex =  5; }
                else if (sharedType == typeof(SnapBackwardNote))          { ComboBoxType.SelectedIndex =  6; }
                else if (sharedType == typeof(LaneShowNote))              { ComboBoxType.SelectedIndex =  7; }
                else if (sharedType == typeof(LaneHideNote))              { ComboBoxType.SelectedIndex =  8; }
                else if (sharedType == typeof(SyncNote))                  { ComboBoxType.SelectedIndex =  9; }
                else if (sharedType == typeof(MeasureLineNote))           { ComboBoxType.SelectedIndex = 10; }
                else if (sharedType == typeof(TempoChangeEvent))          { ComboBoxType.SelectedIndex = 11; }
                else if (sharedType == typeof(MetreChangeEvent))          { ComboBoxType.SelectedIndex = 12; }
                else if (sharedType == typeof(SpeedChangeEvent))          { ComboBoxType.SelectedIndex = 13; }
                else if (sharedType == typeof(VisibilityChangeEvent))     { ComboBoxType.SelectedIndex = 14; }
                else if (sharedType == typeof(StopEffectEvent))           { ComboBoxType.SelectedIndex = 15; }
                else if (sharedType == typeof(ReverseEffectEvent))        { ComboBoxType.SelectedIndex = 16; }
                else if (sharedType == typeof(TutorialMarkerEvent))       { ComboBoxType.SelectedIndex = 17; }
                else if (sharedType == typeof(Bookmark))                  { ComboBoxType.SelectedIndex = 18; }
            }
            else
            {
                ComboBoxType.SelectedIndex = -1;
            }
            
            TextBoxMeasure.Text = sameMeasure && sharedMeasure != null ? sharedMeasure.Value.ToString(CultureInfo.InvariantCulture) : null;
            TextBoxTick.Text = sameTick && sharedTick != null ? sharedTick.Value.ToString(CultureInfo.InvariantCulture) : null;
            TextBoxFullTick.Text = sameFullTick && sharedFullTick != null ? sharedFullTick.Value.ToString(CultureInfo.InvariantCulture) : null;

            
            // Set layer group values.
            ComboBoxLayers.SelectedIndex = sameLayer && sharedLayer != null ? ChartSystem.Chart.Layers.IndexOf(sharedLayer) : -1;

            // Set shape group values.
            TextBoxPosition.Text = samePosition && sharedPosition != null ? sharedPosition.Value.ToString(CultureInfo.InvariantCulture) : null;
            TextBoxSize.Text = sameSize && sharedSize != null ? sharedSize.Value.ToString(CultureInfo.InvariantCulture) : null;
            
            // Set judgement group values.
            ComboBoxBonusType.SelectedIndex = sameBonusType && sharedBonusType != null ? (int)sharedBonusType : -1;
            ComboBoxJudgementType.SelectedIndex = sameJudgementType && sharedJudgementType != null ? (int)sharedJudgementType : -1;
            
            // Set hold group values.
            ComboBoxRenderType.SelectedIndex = sameHoldPointRenderType && sharedHoldPointRenderType != null ? (int)sharedHoldPointRenderType : -1;

            // Set lane toggle group values.
            ComboBoxSweepDirection.SelectedIndex = sameLaneSweepDirection && sharedLaneSweepDirection != null ? (int)sharedLaneSweepDirection : -1;
            
            // Set tempo change group values.
            TextBoxTempo.Text = sameTempo && sharedTempo != null ? sharedTempo.Value.ToString("0.000000", CultureInfo.InvariantCulture) : null;
            
            // Set metre change group values.
            TextBoxUpper.Text = sameMetreUpper && sharedMetreUpper != null ? sharedMetreUpper.Value.ToString(CultureInfo.InvariantCulture) : null;
            TextBoxLower.Text = sameMetreLower && sharedMetreLower != null ? sharedMetreLower.Value.ToString(CultureInfo.InvariantCulture) : null;
            
            // Set speed change group values.
            TextBoxSpeed.Text = sameSpeed && sharedSpeed != null ? sharedSpeed.Value.ToString("0.000000", CultureInfo.InvariantCulture) : null;
            
            // Set visibility change group values.
            ComboBoxVisibility.SelectedIndex = sameVisibility && sharedVisibility != null 
                ? sharedVisibility.Value ? 1 : 0 
                : -1;
            
            // Set tutorial marker group values.
            TextBoxTutorialMarkerKey.Text = sameKey ? sharedKey : null;
            
            // Set bookmark group values.
            TextBoxBookmarkColor.Text = sameColor && sharedColor != null ? $"{sharedColor - 0xFF000000:X6}" : null;
            TextBoxBookmarkMessage.Text = sameMessage ? sharedMessage : null;

            BorderBookmarkColor.IsVisible = sameColor;
            BorderBookmarkColorPlaceholder.IsVisible = !sameColor;
            
            if (sameColor)
            {
                BorderBookmarkColor.Background = new SolidColorBrush(sharedColor ?? 0xFF000000);
            }
            
            blockEvents = false;
        });
    }
#endregion System Event Delegates

#region UI Event Delegates
    private void ComboBoxType_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxType == null) return;
        if (ComboBoxType.SelectedIndex == -1) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        if (ChartSystem.Chart.Layers.Count == 0) return;
        
        List<IOperation> operations = [];
        List<ITimeable> objects = SelectionSystem.OrderedSelectedObjects;

        if (ComboBoxType.SelectedIndex == 2)
        {
            // Convert to Hold Note
            if (objects.Count < 2) return;
            if (SelectionSystem.SelectedLayer == null) return;
            
            BonusType bonusType = BonusType.Normal;
            JudgementType judgementType = JudgementType.Normal;

            if (objects[0] is IPlayable playable)
            {
                bonusType = playable.BonusType;
                judgementType = playable.JudgementType;
            }
            
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

            if (!objects.Any(x => x is HoldPointNote))
            {
                operations.Add(new SelectionAddOperation(holdNote, SelectionSystem.LastSelectedObject));
            }

            operations.Add(new NoteAddOperation(SelectionSystem.SelectedLayer, holdNote, 0));
        }
        else if (ComboBoxType.SelectedIndex == 15)
        {
            // Convert to Stop Effect Event
            if (SelectionSystem.SelectedObjects.Count < 2) return;
            if (SelectionSystem.SelectedLayer == null) return;

            StopEffectEvent stopEffectEvent = new();

            for (int i = 0; i < 2; i++)
            {
                ITimeable obj = objects[i];
                Layer layer = ChartSystem.Chart.ParentLayer(obj) ?? ChartSystem.Chart.Layers[0];
                
                // Remove original object.
                removeObject(obj, layer);
                
                // Add sub-event
                stopEffectEvent.SubEvents[i] = new(new(obj.Timestamp.FullTick), stopEffectEvent);
            }

            operations.Add(new EventAddOperation(SelectionSystem.SelectedLayer, stopEffectEvent, 0));
        }
        else if (ComboBoxType.SelectedIndex == 16)
        {
            // Convert to Reverse Effect Event
            if (SelectionSystem.SelectedObjects.Count < 3) return;
            if (SelectionSystem.SelectedLayer == null) return;

            ReverseEffectEvent reverseEffectEvent = new();

            for (int i = 0; i < 3; i++)
            {
                ITimeable obj = objects[i];
                Layer layer = ChartSystem.Chart.ParentLayer(obj) ?? ChartSystem.Chart.Layers[0];
                
                // Remove original object.
                removeObject(obj, layer);
                
                // Add sub-event
                reverseEffectEvent.SubEvents[i] = new(new(obj.Timestamp.FullTick), reverseEffectEvent);
            }

            operations.Add(new EventAddOperation(SelectionSystem.SelectedLayer, reverseEffectEvent, 0));
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
                        ITimeable? newObject = ComboBoxType.SelectedIndex switch
                        {
                            0  => new TouchNote(timestamp, point.Position, point.Size, BonusType.Normal, JudgementType.Normal),
                            1  => new ChainNote(timestamp, point.Position, point.Size, BonusType.Normal, JudgementType.Normal),
                            // 2 is skipped. (Hold Note)
                            3  => new SlideClockwiseNote(timestamp, point.Position, point.Size, BonusType.Normal, JudgementType.Normal),
                            4  => new SlideCounterclockwiseNote(timestamp, point.Position, point.Size, BonusType.Normal, JudgementType.Normal),
                            5  => new SnapForwardNote(timestamp, point.Position, point.Size, BonusType.Normal, JudgementType.Normal),
                            6  => new SnapBackwardNote(timestamp, point.Position, point.Size, BonusType.Normal, JudgementType.Normal),
                            7  => new LaneShowNote(timestamp, point.Position, point.Size, LaneSweepDirection.Instant),
                            8  => new LaneHideNote(timestamp, point.Position, point.Size, LaneSweepDirection.Instant),
                            9  => new SyncNote(timestamp, point.Position, point.Size),
                            10 => new MeasureLineNote(timestamp, false),
                            11 => new TempoChangeEvent(timestamp, 120),
                            12 => new MetreChangeEvent(timestamp, 4, 4),
                            13 => new SpeedChangeEvent(timestamp, 1),
                            14 => new VisibilityChangeEvent(timestamp, true),
                            // 15 is skipped. (Stop Effect Event)
                            // 16 is skipped. (Reverse Effect Event)
                            17 => new TutorialMarkerEvent(timestamp, ""),
                            18 => new Bookmark(timestamp, 0xFFDDDDDD, ""),
                            _  => null,
                        };

                        if (newObject == null) continue;

                        operations.Add(new SelectionAddOperation(newObject, SelectionSystem.LastSelectedObject));
                        
                        if (newObject is (TempoChangeEvent or MetreChangeEvent or TutorialMarkerEvent) and Event newGlobalEvent)
                        {
                            operations.Add(new GlobalEventAddOperation(newGlobalEvent, 0));
                        }
                        else if (newObject is ILaneToggle and Note newLaneToggle)
                        {
                            operations.Add(new LaneToggleAddOperation(newLaneToggle, 0));
                        }
                        else if (newObject is Bookmark bookmark)
                        {
                            operations.Add(new BookmarkAddOperation(bookmark, 0));
                        }
                        else if (newObject is Event newEvent)
                        {
                            operations.Add(new EventAddOperation(layer, newEvent, 0));
                        }
                        else if (newObject is Note newNote)
                        {
                            operations.Add(new NoteAddOperation(layer, newNote, 0));
                        }
                    }
                }
                else if (obj is StopEffectEvent stopEffectEvent)
                {
                    // Convert all stop effect sub-events to new objects.
                    foreach (EffectSubEvent subEvent in stopEffectEvent.SubEvents)
                    {
                        Timestamp timestamp = new(subEvent.Timestamp.FullTick);
                        ITimeable? newObject = ComboBoxType.SelectedIndex switch
                        {
                            0  => new TouchNote(timestamp, 0, 60, BonusType.Normal, JudgementType.Normal),
                            1  => new ChainNote(timestamp, 0, 60, BonusType.Normal, JudgementType.Normal),
                            // 2 is skipped. (Hold Note)
                            3  => new SlideClockwiseNote(timestamp, 0, 60, BonusType.Normal, JudgementType.Normal),
                            4  => new SlideCounterclockwiseNote(timestamp, 0, 60, BonusType.Normal, JudgementType.Normal),
                            5  => new SnapForwardNote(timestamp, 0, 60, BonusType.Normal, JudgementType.Normal),
                            6  => new SnapBackwardNote(timestamp, 0, 60, BonusType.Normal, JudgementType.Normal),
                            7  => new LaneShowNote(timestamp, 0, 60, LaneSweepDirection.Instant),
                            8  => new LaneHideNote(timestamp, 0, 60, LaneSweepDirection.Instant),
                            9  => new SyncNote(timestamp, 0, 60),
                            10 => new MeasureLineNote(timestamp, false),
                            11 => new TempoChangeEvent(timestamp, 120),
                            12 => new MetreChangeEvent(timestamp, 4, 4),
                            13 => new SpeedChangeEvent(timestamp, 1),
                            14 => new VisibilityChangeEvent(timestamp, true),
                            // 15 is skipped. (Stop Effect Event)
                            // 16 is skipped. (Reverse Effect Event)
                            17 => new TutorialMarkerEvent(timestamp, ""),
                            18 => new Bookmark(timestamp, 0xFFDDDDDD, ""),
                            _  => null,
                        };

                        if (newObject == null) continue;

                        operations.Add(new SelectionAddOperation(newObject, SelectionSystem.LastSelectedObject));
                        
                        if (newObject is (TempoChangeEvent or MetreChangeEvent or TutorialMarkerEvent) and Event newGlobalEvent)
                        {
                            operations.Add(new GlobalEventAddOperation(newGlobalEvent, 0));
                        }
                        else if (newObject is ILaneToggle and Note newLaneToggle)
                        {
                            operations.Add(new LaneToggleAddOperation(newLaneToggle, 0));
                        }
                        else if (newObject is Bookmark bookmark)
                        {
                            operations.Add(new BookmarkAddOperation(bookmark, 0));
                        }
                        else if (newObject is Event newEvent)
                        {
                            operations.Add(new EventAddOperation(layer, newEvent, 0));
                        }
                        else if (newObject is Note newNote)
                        {
                            operations.Add(new NoteAddOperation(layer, newNote, 0));
                        }
                    }
                }
                else if (obj is ReverseEffectEvent reverseEffectEvent)
                {
                    // Convert all reverse effect sub-events to new objects.
                    foreach (EffectSubEvent subEvent in reverseEffectEvent.SubEvents)
                    {
                        Timestamp timestamp = new(subEvent.Timestamp.FullTick);
                        ITimeable? newObject = ComboBoxType.SelectedIndex switch
                        {
                            0  => new TouchNote(timestamp, 0, 60, BonusType.Normal, JudgementType.Normal),
                            1  => new ChainNote(timestamp, 0, 60, BonusType.Normal, JudgementType.Normal),
                            // 2 is skipped. (Hold Note)
                            3  => new SlideClockwiseNote(timestamp, 0, 60, BonusType.Normal, JudgementType.Normal),
                            4  => new SlideCounterclockwiseNote(timestamp, 0, 60, BonusType.Normal, JudgementType.Normal),
                            5  => new SnapForwardNote(timestamp, 0, 60, BonusType.Normal, JudgementType.Normal),
                            6  => new SnapBackwardNote(timestamp, 0, 60, BonusType.Normal, JudgementType.Normal),
                            7  => new LaneShowNote(timestamp, 0, 60, LaneSweepDirection.Instant),
                            8  => new LaneHideNote(timestamp, 0, 60, LaneSweepDirection.Instant),
                            9  => new SyncNote(timestamp, 0, 60),
                            10 => new MeasureLineNote(timestamp, false),
                            11 => new TempoChangeEvent(timestamp, 120),
                            12 => new MetreChangeEvent(timestamp, 4, 4),
                            13 => new SpeedChangeEvent(timestamp, 1),
                            14 => new VisibilityChangeEvent(timestamp, true),
                            // 15 is skipped. (Stop Effect Event)
                            // 16 is skipped. (Reverse Effect Event)
                            17 => new TutorialMarkerEvent(timestamp, ""),
                            18 => new Bookmark(timestamp, 0xFFDDDDDD, ""),
                            _  => null,
                        };

                        if (newObject == null) continue;

                        operations.Add(new SelectionAddOperation(newObject, SelectionSystem.LastSelectedObject));
                        
                        if (newObject is (TempoChangeEvent or MetreChangeEvent or TutorialMarkerEvent) and Event newGlobalEvent)
                        {
                            operations.Add(new GlobalEventAddOperation(newGlobalEvent, 0));
                        }
                        else if (newObject is ILaneToggle and Note newLaneToggle)
                        {
                            operations.Add(new LaneToggleAddOperation(newLaneToggle, 0));
                        }
                        else if (newObject is Bookmark bookmark)
                        {
                            operations.Add(new BookmarkAddOperation(bookmark, 0));
                        }
                        else if (newObject is Event newEvent)
                        {
                            operations.Add(new EventAddOperation(layer, newEvent, 0));
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

                    BonusType bonusType = BonusType.Normal;
                    JudgementType judgementType = JudgementType.Normal;
                    LaneSweepDirection laneSweepDirection = LaneSweepDirection.Instant;

                    if (obj is IPositionable positionable)
                    {
                        position = positionable.Position;
                        size = positionable.Size;
                    }

                    if (obj is IPlayable playable)
                    {
                        bonusType = playable.BonusType;
                        judgementType = playable.JudgementType;
                    }

                    if (obj is ILaneToggle laneToggle)
                    {
                        laneSweepDirection = laneToggle.Direction;
                    }
                
                    Timestamp timestamp = new(obj.Timestamp.FullTick);
                    ITimeable? newObject = ComboBoxType.SelectedIndex switch
                    {
                        0  => new TouchNote(timestamp, position, size, bonusType, judgementType),
                        1  => new ChainNote(timestamp, position, size, bonusType, judgementType),
                        // 2 is skipped. (Hold Note)
                        3  => new SlideClockwiseNote(timestamp, position, size, bonusType, judgementType),
                        4  => new SlideCounterclockwiseNote(timestamp, position, size, bonusType, judgementType),
                        5  => new SnapForwardNote(timestamp, position, size, bonusType, judgementType),
                        6  => new SnapBackwardNote(timestamp, position, size, bonusType, judgementType),
                        7  => new LaneShowNote(timestamp, position, size, laneSweepDirection),
                        8  => new LaneHideNote(timestamp, position, size, laneSweepDirection),
                        9  => new SyncNote(timestamp, position, size),
                        10 => new MeasureLineNote(timestamp, false),
                        11 => new TempoChangeEvent(timestamp, 120),
                        12 => new MetreChangeEvent(timestamp, 4, 4),
                        13 => new SpeedChangeEvent(timestamp, 1),
                        14 => new VisibilityChangeEvent(timestamp, true),
                        // 15 is skipped. (Stop Effect Event)
                        // 16 is skipped. (Reverse Effect Event)
                        17 => new TutorialMarkerEvent(timestamp, ""),
                        18 => new Bookmark(timestamp, 0xFFDDDDDD, ""),
                        _  => null,
                    };
                    
                    if (newObject == null) continue;

                    // Only re-select newly generated object if it wasn't a hold point note or effect sub-event before the conversion.
                    // If it was one of the filtered types before the conversion, then the editor is currently in hold- or event-edit mode
                    // and no selections outside the hold note or event should be made. 
                    if (obj is not (HoldPointNote or EffectSubEvent))
                    {
                        operations.Add(new SelectionAddOperation(newObject, SelectionSystem.LastSelectedObject));
                    }
                    
                    if (newObject is (TempoChangeEvent or MetreChangeEvent or TutorialMarkerEvent) and Event newGlobalEvent)
                    {
                        operations.Add(new GlobalEventAddOperation(newGlobalEvent, 0));
                    }
                    else if (newObject is ILaneToggle and Note newLaneToggle)
                    {
                        operations.Add(new LaneToggleAddOperation(newLaneToggle, 0));
                    }
                    else if (newObject is Bookmark bookmark)
                    {
                        operations.Add(new BookmarkAddOperation(bookmark, 0));
                    }
                    else if (newObject is Event newEvent)
                    {
                        operations.Add(new EventAddOperation(layer, newEvent, 0));
                    }
                    else if (newObject is Note newNote)
                    {
                        operations.Add(new NoteAddOperation(layer, newNote, 0));
                    }
                }
            }
        }
        
        UndoRedoSystem.Push(new CompositeOperation(operations));
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

    private void TextBoxMeasure_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxMeasure == null) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        
        if (string.IsNullOrWhiteSpace(TextBoxMeasure.Text))
        {
            blockEvents = true;

            TextBoxMeasure.Text = null;
            
            blockEvents = false;
            return;
        }

        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxMeasure.Text, CultureInfo.InvariantCulture);
            newValue = Math.Max(0, newValue);
            newValue *= 1920;
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
        
        List<IOperation> operations = [];
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                if (holdNote.Points.Count == 0) continue;
                
                int oldStartFullTick = holdNote.Points[0].Timestamp.FullTick;
                int newStartFullTick = newValue + holdNote.Points[0].Timestamp.Tick;
                
                foreach (HoldPointNote point in holdNote.Points)
                {
                    int oldFullTick = point.Timestamp.FullTick;
                    int newFullTick = oldFullTick + (newStartFullTick - oldStartFullTick);
                    
                    operations.Add(new TimeableEditOperation(point, oldFullTick, newFullTick));
                }
            }
            else if (obj is StopEffectEvent stopEffectEvent)
            {
                int oldStartTick = stopEffectEvent.SubEvents[0].Timestamp.FullTick;
                int newStartTick = newValue + stopEffectEvent.SubEvents[0].Timestamp.Tick;
                
                foreach (EffectSubEvent subEvent in stopEffectEvent.SubEvents)
                {
                    int oldFullTick = subEvent.Timestamp.FullTick;
                    int newFullTick = oldFullTick + (newStartTick - oldStartTick);
                    
                    operations.Add(new TimeableEditOperation(subEvent, oldFullTick, newFullTick));
                }
            }
            else if (obj is ReverseEffectEvent reverseEffectEvent)
            {
                int oldStartTick = reverseEffectEvent.SubEvents[0].Timestamp.FullTick;
                int newStartTick = newValue + reverseEffectEvent.SubEvents[0].Timestamp.Tick;
                
                foreach (EffectSubEvent subEvent in reverseEffectEvent.SubEvents)
                {
                    int oldFullTick = subEvent.Timestamp.FullTick;
                    int newFullTick = oldFullTick + (newStartTick - oldStartTick);
                    
                    operations.Add(new TimeableEditOperation(subEvent, oldFullTick, newFullTick));
                }
            }
            else
            {
                int oldFullTick = obj.Timestamp.FullTick;
                int newFullTick = newValue + obj.Timestamp.Tick;
                
                operations.Add(new TimeableEditOperation(obj, oldFullTick, newFullTick));
            }
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private void TextBoxTick_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxTick == null) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        
        if (string.IsNullOrWhiteSpace(TextBoxTick.Text))
        {
            blockEvents = true;

            TextBoxTick.Text = null;
            
            blockEvents = false;
            return;
        }
        
        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxTick.Text, CultureInfo.InvariantCulture);
            newValue = Math.Max(0, newValue);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
        
        List<IOperation> operations = [];
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is HoldNote holdNote)
            {
                if (holdNote.Points.Count == 0) continue;
                
                int oldStartTick = holdNote.Points[0].Timestamp.FullTick;
                int newStartTick = holdNote.Points[0].Timestamp.Measure * 1920 + newValue;
                
                foreach (HoldPointNote point in holdNote.Points)
                {
                    int oldFullTick = point.Timestamp.FullTick;
                    int newFullTick = oldFullTick + (newStartTick - oldStartTick);
                    
                    operations.Add(new TimeableEditOperation(point, oldFullTick, newFullTick));
                }
            }
            else if (obj is StopEffectEvent stopEffectEvent)
            {
                int oldStartTick = stopEffectEvent.SubEvents[0].Timestamp.FullTick;
                int newStartTick = stopEffectEvent.SubEvents[0].Timestamp.Measure * 1920 + newValue;
                
                foreach (EffectSubEvent subEvent in stopEffectEvent.SubEvents)
                {
                    int oldFullTick = subEvent.Timestamp.FullTick;
                    int newFullTick = oldFullTick + (newStartTick - oldStartTick);
                    
                    operations.Add(new TimeableEditOperation(subEvent, oldFullTick, newFullTick));
                }
            }
            else if (obj is ReverseEffectEvent reverseEffectEvent)
            {
                int oldStartTick = reverseEffectEvent.SubEvents[0].Timestamp.FullTick;
                int newStartTick = reverseEffectEvent.SubEvents[0].Timestamp.Measure * 1920 + newValue;
                
                foreach (EffectSubEvent subEvent in reverseEffectEvent.SubEvents)
                {
                    int oldFullTick = subEvent.Timestamp.FullTick;
                    int newFullTick = oldFullTick + (newStartTick - oldStartTick);
                    
                    operations.Add(new TimeableEditOperation(subEvent, oldFullTick, newFullTick));
                }
            }
            else
            {
                int oldFullTick = obj.Timestamp.FullTick;
                int newFullTick = obj.Timestamp.Measure * 1920 + newValue;
                
                operations.Add(new TimeableEditOperation(obj, oldFullTick, newFullTick));
            }
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private void TextBoxFullTick_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxFullTick == null) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        
        if (string.IsNullOrWhiteSpace(TextBoxFullTick.Text))
        {
            blockEvents = true;

            TextBoxFullTick.Text = null;
            
            blockEvents = false;
            return;
        }
        
        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxFullTick.Text, CultureInfo.InvariantCulture);
            newValue = Math.Max(0, newValue);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
        
        List<IOperation> operations = [];
        foreach (ITimeable timeable in SelectionSystem.OrderedSelectedObjects)
        {
            if (timeable is HoldNote holdNote)
            {
                if (holdNote.Points.Count == 0) continue;
                
                int oldStartTick = holdNote.Points[0].Timestamp.FullTick;
                
                foreach (HoldPointNote point in holdNote.Points)
                {
                    int oldPointFullTick = point.Timestamp.FullTick;
                    int newPointFullTick = oldPointFullTick + (newValue - oldStartTick);
                    
                    operations.Add(new TimeableEditOperation(point, oldPointFullTick, newPointFullTick));
                }
            }
            else if (timeable is StopEffectEvent stopEffectEvent)
            {
                int oldStartTick = stopEffectEvent.SubEvents[0].Timestamp.FullTick;
                
                foreach (EffectSubEvent subEvent in stopEffectEvent.SubEvents)
                {
                    int oldPointFullTick = subEvent.Timestamp.FullTick;
                    int newPointFullTick = oldPointFullTick + (newValue - oldStartTick);
                    
                    operations.Add(new TimeableEditOperation(subEvent, oldPointFullTick, newPointFullTick));
                }
            }
            else if (timeable is ReverseEffectEvent reverseEffectEvent)
            {
                int oldStartTick = reverseEffectEvent.SubEvents[0].Timestamp.FullTick;
                
                foreach (EffectSubEvent subEvent in reverseEffectEvent.SubEvents)
                {
                    int oldPointFullTick = subEvent.Timestamp.FullTick;
                    int newPointFullTick = oldPointFullTick + (newValue - oldStartTick);
                    
                    operations.Add(new TimeableEditOperation(subEvent, oldPointFullTick, newPointFullTick));
                }
            }
            else
            {
                int oldFullTick = timeable.Timestamp.FullTick;
                
                operations.Add(new TimeableEditOperation(timeable, oldFullTick, newValue));
            }
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }
    
    private void ComboBoxLayers_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        if (ComboBoxLayers.SelectedIndex < 0 || ComboBoxLayers.SelectedIndex >= ChartSystem.Chart.Layers.Count) return;
        
        Layer newLayer = ChartSystem.Chart.Layers[ComboBoxLayers.SelectedIndex];

        List<IOperation> operations = [];
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            Layer? parentLayer = ChartSystem.Chart.ParentLayer(obj);
            if (parentLayer == null) continue;
            if (parentLayer == newLayer) continue;

            if (obj is Note note)
            {
                int index = parentLayer.Notes.IndexOf(note);
                operations.Add(new NoteRemoveOperation(parentLayer, note, index));
                operations.Add(new NoteAddOperation(newLayer, note, 0));
            }
            else if (obj is Event @event)
            {
                int index = parentLayer.Events.IndexOf(@event);
                operations.Add(new EventRemoveOperation(parentLayer, @event, index));
                operations.Add(new EventAddOperation(newLayer, @event, index));
            }
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private void TextBoxPosition_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxPosition == null) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        
        if (string.IsNullOrWhiteSpace(TextBoxPosition.Text))
        {
            blockEvents = true;

            TextBoxPosition.Text = null;
            
            blockEvents = false;
            return;
        }
        
        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxPosition.Text, CultureInfo.InvariantCulture);
            newValue = Math.Clamp(newValue, 0, 59);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }

        List<IOperation> operations = [];
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is not IPositionable positionable) continue;

            if (obj is HoldNote holdNote)
            {
                if (holdNote.Points.Count == 0) continue;

                int diff = newValue - holdNote.Points[0].Position;

                foreach (HoldPointNote point in holdNote.Points)
                {
                    int newPosition = (point.Position + diff + 60) % 60;
                    operations.Add(new PositionableEditOperation(point, point.Position, newPosition, point.Size, point.Size));
                }
            }
            else
            {
                operations.Add(new PositionableEditOperation(positionable, positionable.Position, newValue, positionable.Size, positionable.Size));
            }
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private void TextBoxSize_OnLostFocus(object? sender, RoutedEventArgs e)
    { 
        if (blockEvents) return;
        if (TextBoxSize == null) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        
        if (string.IsNullOrWhiteSpace(TextBoxSize.Text))
        {
            blockEvents = true;

            TextBoxSize.Text = null;
            
            blockEvents = false;
            return;
        }
        
        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxSize.Text, CultureInfo.InvariantCulture);
            newValue = Math.Clamp(newValue, 1, 60);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
        
        List<IOperation> operations = [];
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is not IPositionable positionable) continue;
            
            if (obj is HoldNote holdNote)
            {
                if (holdNote.Points.Count == 0) continue;

                int diff = newValue - holdNote.Points[0].Size;

                foreach (HoldPointNote point in holdNote.Points)
                {
                    int newSize = Math.Clamp(point.Size + diff, 1, 60);
                    operations.Add(new PositionableEditOperation(point, point.Position, point.Position, point.Size, newSize));
                }
            }
            else
            {
                operations.Add(new PositionableEditOperation(positionable, positionable.Position, positionable.Position, positionable.Size, newValue));
            }
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private void ComboBoxBonusType_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        if (ComboBoxBonusType == null) return;

        if (ComboBoxBonusType.SelectedIndex == -1) return;

        BonusType newBonusType = (BonusType)ComboBoxBonusType.SelectedIndex;

        List<IOperation> operations = [];
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is not IPlayable playable) continue;

            operations.Add(new PlayableEditOperation(playable, playable.BonusType, newBonusType, playable.JudgementType, playable.JudgementType));
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private void ComboBoxJudgementType_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        if (ComboBoxJudgementType == null) return;

        if (ComboBoxJudgementType.SelectedIndex == -1) return;

        JudgementType newJudgementType = (JudgementType)ComboBoxJudgementType.SelectedIndex;

        List<IOperation> operations = [];
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is not IPlayable playable) continue;

            operations.Add(new PlayableEditOperation(playable, playable.BonusType, playable.BonusType, playable.JudgementType, newJudgementType));
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private void ComboBoxRenderType_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        if (ComboBoxRenderType == null) return;

        if (ComboBoxRenderType.SelectedIndex == -1) return;

        HoldPointRenderType newRenderType = (HoldPointRenderType)ComboBoxRenderType.SelectedIndex;

        List<IOperation> operations = [];
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is not HoldPointNote point) continue;

            operations.Add(new HoldPointNoteRenderTypeEditOperation(point, point.RenderType, newRenderType));
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private void ComboBoxSweepDirection_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        if (ComboBoxSweepDirection == null) return;
        
        if (ComboBoxSweepDirection.SelectedIndex == -1) return;
        
        LaneSweepDirection newDirection = (LaneSweepDirection)ComboBoxSweepDirection.SelectedIndex;

        List<IOperation> operations = [];
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is not ILaneToggle laneToggle) continue;
            
            operations.Add(new LaneToggleEditOperation(laneToggle, laneToggle.Direction, newDirection));
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private void TextBoxTempo_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        if (TextBoxTempo == null) return;
        
        if (string.IsNullOrWhiteSpace(TextBoxTempo.Text))
        {
            blockEvents = true;

            TextBoxTempo.Text = null;
            
            blockEvents = false;
            return;
        }
        
        float newValue = 120;

        try
        {
            newValue = Convert.ToSingle(TextBoxTempo.Text, CultureInfo.InvariantCulture);
            newValue = Math.Max(0, newValue);
        }
        catch (Exception ex)
        {
            // Don't throw.
            if (ex is not FormatException or OverflowException)
            {
                Console.WriteLine(ex);
            }
        }
        
        List<IOperation> operations = [];
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is not TempoChangeEvent tempoChangeEvent) continue;
            
            operations.Add(new TempoChangeEditOperation(tempoChangeEvent, tempoChangeEvent.Tempo, newValue));
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private void TextBoxUpper_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxUpper == null) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        
        if (string.IsNullOrWhiteSpace(TextBoxUpper.Text))
        {
            blockEvents = true;

            TextBoxUpper.Text = null;
            
            blockEvents = false;
            return;
        }
        
        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxUpper.Text, CultureInfo.InvariantCulture);
            newValue = Math.Max(1, newValue);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
        
        List<IOperation> operations = [];
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is not MetreChangeEvent metreChangeEvent) continue;
            
            operations.Add(new MetreChangeEditOperation(metreChangeEvent, metreChangeEvent.Upper, newValue, metreChangeEvent.Lower, metreChangeEvent.Lower));
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private void TextBoxLower_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxLower == null) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        
        if (string.IsNullOrWhiteSpace(TextBoxLower.Text))
        {
            blockEvents = true;

            TextBoxLower.Text = null;
            
            blockEvents = false;
            return;
        }
        
        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxLower.Text, CultureInfo.InvariantCulture);
            newValue = Math.Max(1, newValue);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
        
        List<IOperation> operations = [];
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is not MetreChangeEvent metreChangeEvent) continue;
            
            operations.Add(new MetreChangeEditOperation(metreChangeEvent, metreChangeEvent.Upper, metreChangeEvent.Upper, metreChangeEvent.Lower, newValue));
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private void TextBoxSpeed_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        if (TextBoxSpeed == null) return;
        
        if (string.IsNullOrWhiteSpace(TextBoxSpeed.Text))
        {
            blockEvents = true;

            TextBoxSpeed.Text = null;
            
            blockEvents = false;
            return;
        }
        
        float newValue = 0;

        try
        {
            newValue = Convert.ToSingle(TextBoxSpeed.Text, CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
        
        List<IOperation> operations = [];
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is not SpeedChangeEvent speedChangeEvent) continue;
            
            operations.Add(new SpeedChangeEditOperation(speedChangeEvent, speedChangeEvent.Speed, newValue));
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private void ComboBoxVisibility_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        if (ComboBoxVisibility == null) return;

        if (ComboBoxVisibility.SelectedIndex == -1) return;

        bool newVisibility = ComboBoxVisibility.SelectedIndex != 0;

        List<IOperation> operations = [];
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is not VisibilityChangeEvent visibilityChange) continue;

            operations.Add(new VisibilityChangeEditOperation(visibilityChange, visibilityChange.Visibility, newVisibility));
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private void TextBoxTutorialMarkerKey_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        if (TextBoxTutorialMarkerKey == null) return;
        
        if (TextBoxTutorialMarkerKey.Text == null)
        {
            blockEvents = true;

            TextBoxTutorialMarkerKey.Text = null;
            
            blockEvents = false;
            return;
        }

        string newValue = TextBoxTutorialMarkerKey.Text ?? "";
        
        List<IOperation> operations = [];
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is not TutorialMarkerEvent tutorialMarkerEvent) continue;

            operations.Add(new TutorialMarkerEditOperation(tutorialMarkerEvent, tutorialMarkerEvent.Key, newValue));
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private void TextBoxBookmarkColor_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        if (TextBoxBookmarkColor == null) return;
        
        if (string.IsNullOrWhiteSpace(TextBoxBookmarkColor.Text))
        {
            blockEvents = true;

            TextBoxBookmarkColor.Text = null;
            
            blockEvents = false;
            return;
        }
        
        uint newValue = uint.TryParse(TextBoxBookmarkColor.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result) ? result + 0xFF000000 : 0xFFDDDDDD;
        
        List<IOperation> operations = [];
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is not Bookmark bookmark) continue;
            
            operations.Add(new BookmarkEditOperation(bookmark, bookmark.Color, newValue, bookmark.Message, bookmark.Message));
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private void TextBoxBookmarkMessage_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        if (TextBoxBookmarkMessage == null) return;
        
        if (TextBoxBookmarkMessage.Text == null)
        {
            blockEvents = true;

            TextBoxBookmarkMessage.Text = null;
            
            blockEvents = false;
            return;
        }

        string newValue = TextBoxBookmarkMessage.Text ?? "";
        
        List<IOperation> operations = [];
        foreach (ITimeable obj in SelectionSystem.OrderedSelectedObjects)
        {
            if (obj is not Bookmark bookmark) continue;
            
            operations.Add(new BookmarkEditOperation(bookmark, bookmark.Color, bookmark.Color, bookmark.Message, newValue));
        }

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }
#endregion UI Event Delegates
}