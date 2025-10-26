using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using SaturnData.Notation.Core;
using SaturnData.Notation.Events;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnEdit.Systems;

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

        bool sameTimestamp = true;
        Timestamp? sharedTimestamp = null;

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
            sharedTimestamp ??= obj.Timestamp;
            sameTimestamp = sameTimestamp && obj.Timestamp == sharedTimestamp;
            
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
                else if (sharedType == typeof(SnapForwardNote))           { ComboBoxType.SelectedIndex =  1; }
                else if (sharedType == typeof(SnapBackwardNote))          { ComboBoxType.SelectedIndex =  2; }
                else if (sharedType == typeof(SlideClockwiseNote))        { ComboBoxType.SelectedIndex =  3; }
                else if (sharedType == typeof(SlideCounterclockwiseNote)) { ComboBoxType.SelectedIndex =  4; }
                else if (sharedType == typeof(ChainNote))                 { ComboBoxType.SelectedIndex =  5; }
                else if (sharedType == typeof(HoldNote))                  { ComboBoxType.SelectedIndex =  6; }
                else if (sharedType == typeof(LaneShowNote))              { ComboBoxType.SelectedIndex =  7; }
                else if (sharedType == typeof(LaneHideNote))              { ComboBoxType.SelectedIndex =  8; }
                else if (sharedType == typeof(MeasureLineNote))           { ComboBoxType.SelectedIndex =  9; }
                else if (sharedType == typeof(SyncNote))                  { ComboBoxType.SelectedIndex = 10; }
                else if (sharedType == typeof(TempoChangeEvent))          { ComboBoxType.SelectedIndex = 11; }
                else if (sharedType == typeof(MetreChangeEvent))          { ComboBoxType.SelectedIndex = 12; }
                else if (sharedType == typeof(SpeedChangeEvent))          { ComboBoxType.SelectedIndex = 13; }
                else if (sharedType == typeof(VisibilityChangeEvent))     { ComboBoxType.SelectedIndex = 14; }
                else if (sharedType == typeof(ReverseEffectEvent))        { ComboBoxType.SelectedIndex = 15; }
                else if (sharedType == typeof(StopEffectEvent))           { ComboBoxType.SelectedIndex = 16; }
                else if (sharedType == typeof(TutorialMarkerEvent))       { ComboBoxType.SelectedIndex = 17; }
                else if (sharedType == typeof(Bookmark))                  { ComboBoxType.SelectedIndex = 18; }
            }
            else
            {
                ComboBoxType.SelectedIndex = -1;
            }

            if (sameTimestamp && sharedTimestamp != null)
            {
                NumericUpDownMeasure.Value = sharedTimestamp!.Measure;
                NumericUpDownTick.Value = sharedTimestamp!.Tick;
                NumericUpDownFullTick.Value = sharedTimestamp!.FullTick;
            }
            else
            {
                NumericUpDownMeasure.Value = null;
                NumericUpDownTick.Value = null;
                NumericUpDownFullTick.Value = null;
            }
            
            // Set layer group values.
            ComboBoxLayers.SelectedIndex = sameLayer && sharedLayer != null ? ChartSystem.Chart.Layers.IndexOf(sharedLayer!) : -1;

            // Set shape group values.
            TextBoxPosition.Text = samePosition && sharedPosition != null ? sharedPosition.Value.ToString(CultureInfo.InvariantCulture) : null;
            TextBoxSize.Text = sameSize && sharedSize != null ? sharedSize.Value.ToString(CultureInfo.InvariantCulture) : null;
            
            // Set judgement group values.
            ComboBoxBonusType.SelectedIndex = sameBonusType && sharedBonusType != null ? (int)sharedBonusType! : -1;
            ComboBoxJudgementType.SelectedIndex = sameJudgementType && sharedJudgementType != null ? (int)sharedJudgementType! : -1;
            
            // Set hold group values.
            ComboBoxRenderType.SelectedIndex = sameHoldPointRenderType && sharedHoldPointRenderType != null ? (int)sharedHoldPointRenderType! : -1;

            // Set lane toggle group values.
            ComboBoxSweepDirection.SelectedIndex = sameLaneSweepDirection && sharedLaneSweepDirection != null ? (int)sharedLaneSweepDirection! : -1;
            
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
            TextBoxTutorialMarkerKey.Text = sameKey && sharedKey != null ? sharedKey : null;
            
            // Set bookmark group values.
            TextBoxBookmarkColor.Text = sameColor && sharedColor != null ? $"{sharedColor - 0xFF000000:X}" : null;
            TextBoxBookmarkMessage.Text = sameMessage && sharedMessage != null ? sharedMessage : null;
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
        Console.WriteLine("A");
    }

    private void NumericUpDownMeasure_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }

    private void NumericUpDownTick_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }

    private void NumericUpDownFullTick_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }

    private void ComboBoxLayers_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }

    private void TextBoxPosition_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }

    private void TextBoxSize_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }

    private void ComboBoxBonusType_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }

    private void ComboBoxJudgementType_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }

    private void ComboBoxRenderType_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }

    private void ComboBoxSweepDirection_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }

    private void TextBoxTempo_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }

    private void TextBoxUpper_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }

    private void TextBoxLower_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }

    private void TextBoxSpeed_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }

    private void ComboBoxVisibility_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }

    private void TextBoxTutorialMarkerKey_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }

    private void TextBoxBookmarkColor_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }

    private void TextBoxBookmarkMessage_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        Console.WriteLine("A");
    }
#endregion UI Event Delegates
}