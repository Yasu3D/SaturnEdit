using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnData.Notation.Core;
using SaturnData.Notation.Events;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.EventOperations;
using SaturnEdit.UndoRedo.TimeableOperations;

namespace SaturnEdit.Controls;

public partial class EventListItem : UserControl
{
    public EventListItem()
    {
        InitializeComponent();
    }
    
    public Event? Event { get; private set; } = null;

    private bool blockEvents = false;

#region Methods
    public void SetEvent(Event @event)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            blockEvents = true;
            
            Event = @event;

            GroupStandardEvent.IsVisible = false;
            GroupStopEffectEvent.IsVisible = false;
            GroupReverseEffectEvent.IsVisible = false;
            
            if (Event is StopEffectEvent stopEffectEvent && stopEffectEvent.SubEvents.Length == 2)
            {
                GroupStopEffectEvent.IsVisible = true;

                TextBoxStopMeasure0.Text = stopEffectEvent.SubEvents[0].Timestamp.Measure.ToString(CultureInfo.InvariantCulture);
                TextBoxStopTick0.Text = stopEffectEvent.SubEvents[0].Timestamp.Tick.ToString(CultureInfo.InvariantCulture);
                
                TextBoxStopMeasure1.Text = stopEffectEvent.SubEvents[1].Timestamp.Measure.ToString(CultureInfo.InvariantCulture);
                TextBoxStopTick1.Text = stopEffectEvent.SubEvents[1].Timestamp.Tick.ToString(CultureInfo.InvariantCulture);
            }
            else if (Event is ReverseEffectEvent reverseEffectEvent && reverseEffectEvent.SubEvents.Length == 3)
            {
                GroupReverseEffectEvent.IsVisible = true;
                
                TextBoxReverseMeasure0.Text = reverseEffectEvent.SubEvents[0].Timestamp.Measure.ToString(CultureInfo.InvariantCulture);
                TextBoxReverseTick0.Text = reverseEffectEvent.SubEvents[0].Timestamp.Tick.ToString(CultureInfo.InvariantCulture);
                
                TextBoxReverseMeasure1.Text = reverseEffectEvent.SubEvents[1].Timestamp.Measure.ToString(CultureInfo.InvariantCulture);
                TextBoxReverseTick1.Text = reverseEffectEvent.SubEvents[1].Timestamp.Tick.ToString(CultureInfo.InvariantCulture);
                
                TextBoxReverseMeasure2.Text = reverseEffectEvent.SubEvents[2].Timestamp.Measure.ToString(CultureInfo.InvariantCulture);
                TextBoxReverseTick2.Text = reverseEffectEvent.SubEvents[2].Timestamp.Tick.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                GroupStandardEvent.IsVisible = true;
                
                TextBoxFloatValue.IsVisible = false;
                GroupMetreInput.IsVisible = false;
                ComboBoxVisibility.IsVisible = false;

                try
                {
                    string key = Event switch
                    {
                        TempoChangeEvent      => "ChartEditor.General.Type.Value.TempoChange",
                        MetreChangeEvent      => "ChartEditor.General.Type.Value.MetreChange",
                        SpeedChangeEvent      => "ChartEditor.General.Type.Value.SpeedChange",
                        VisibilityChangeEvent => "ChartEditor.General.Type.Value.VisibilityChange",
                        StopEffectEvent       => "ChartEditor.General.Type.Value.ReverseEffect",
                        ReverseEffectEvent    => "ChartEditor.General.Type.Value.StopEffect",
                        TutorialMarkerEvent   => "ChartEditor.General.Type.Value.TutorialMarker",
                        _ => throw new(),
                    };
                    
                    if (Application.Current != null && Application.Current.TryGetResource(key, Application.Current.ActualThemeVariant, out object? resource))
                    {
                        if (resource is string s)
                        {
                            TextBlockEventType.Text = s;
                        }
                        else
                        {
                            Console.WriteLine($"Resource '{key}' was not a string.");
                            TextBlockEventType.Text = "UNKNOWN TYPE";
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Resource '{key}' could not be found.");
                        TextBlockEventType.Text = "UNKNOWN TYPE";
                    }
                }
                catch (Exception ex)
                {
                    // Don't throw.
                    Console.WriteLine(ex);
                    
                    TextBlockEventType.Text = "UNKNOWN TYPE";
                }
                
                TextBoxMeasure.Text = Event.Timestamp.Measure.ToString(CultureInfo.InvariantCulture);
                TextBoxTick.Text = Event.Timestamp.Tick.ToString(CultureInfo.InvariantCulture);

                if (Event is TempoChangeEvent tempoChangeEvent)
                {
                    TextBoxFloatValue.IsVisible = true;
                    TextBoxFloatValue.Text = tempoChangeEvent.Tempo.ToString("F6", CultureInfo.InvariantCulture);
                }
                else if (Event is SpeedChangeEvent speedChangeEvent)
                {
                    TextBoxFloatValue.IsVisible = true;
                    TextBoxFloatValue.Text = speedChangeEvent.Speed.ToString("F6", CultureInfo.InvariantCulture);
                }
                else if (Event is MetreChangeEvent metreChangeEvent)
                {
                    GroupMetreInput.IsVisible = true;
                    TextBoxMetreUpper.Text = metreChangeEvent.Upper.ToString(CultureInfo.InvariantCulture);
                    TextBoxMetreLower.Text = metreChangeEvent.Lower.ToString(CultureInfo.InvariantCulture);
                }
                else if (Event is VisibilityChangeEvent visibilityChangeEvent)
                {
                    ComboBoxVisibility.IsVisible = true;
                    ComboBoxVisibility.SelectedIndex = visibilityChangeEvent.Visibility ? 1 : 0;
                }
            }

            blockEvents = false;
        });
    }
#endregion Methods

#region UI Event Delegates
    private void TextBoxMeasure_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (Event == null) return;
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

        int oldFullTick = Event.Timestamp.FullTick;
        int newFullTick = newValue + Event.Timestamp.Tick;
        
        TimeableEditOperation op0 = new(Event, oldFullTick, newFullTick);
        BuildChartOperation op1 = new();

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    private void TextBoxTick_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (Event == null) return;
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
            newValue = Math.Clamp(newValue, 0, 1919);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }

        int oldFullTick = Event.Timestamp.FullTick;
        int newFullTick = newValue + Event.Timestamp.Measure * 1920;
        
        TimeableEditOperation op0 = new(Event, oldFullTick, newFullTick);
        BuildChartOperation op1 = new();

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    private void TextBoxFloatValue_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (Event is not (TempoChangeEvent or SpeedChangeEvent)) return;
        if (string.IsNullOrWhiteSpace(TextBoxFloatValue.Text))
        {
            blockEvents = true;

            TextBoxFloatValue.Text = null;
            
            blockEvents = false;
            return;
        }

        float newValue = 0;

        try
        {
            newValue = Convert.ToSingle(TextBoxFloatValue.Text, CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }

        IOperation op0;

        if (Event is TempoChangeEvent tempoChangeEvent)
        {
            op0 = new TempoChangeEditOperation(tempoChangeEvent, tempoChangeEvent.Tempo, newValue);
        }
        else if (Event is SpeedChangeEvent speedChangeEvent)
        {
            op0 = new SpeedChangeEditOperation(speedChangeEvent, speedChangeEvent.Speed, newValue);
        }
        else
        {
            return;
        }
        
        BuildChartOperation op1 = new();

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    private void TextBoxMetreUpper_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (Event is not MetreChangeEvent metreChangeEvent) return;
        if (string.IsNullOrWhiteSpace(TextBoxMetreUpper.Text))
        {
            blockEvents = true;

            TextBoxMetreUpper.Text = null;
            
            blockEvents = false;
            return;
        }

        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxMetreUpper.Text, CultureInfo.InvariantCulture);
            newValue = Math.Max(1, newValue);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
        
        MetreChangeEditOperation op0 = new(metreChangeEvent, metreChangeEvent.Upper, newValue, metreChangeEvent.Lower, metreChangeEvent.Lower);
        BuildChartOperation op1 = new();

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    private void TextBoxMetreLower_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (Event is not MetreChangeEvent metreChangeEvent) return;
        if (string.IsNullOrWhiteSpace(TextBoxMetreLower.Text))
        {
            blockEvents = true;

            TextBoxMetreLower.Text = null;
            
            blockEvents = false;
            return;
        }

        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxMetreLower.Text, CultureInfo.InvariantCulture);
            newValue = Math.Max(1, newValue);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
        
        MetreChangeEditOperation op0 = new(metreChangeEvent, metreChangeEvent.Upper, metreChangeEvent.Upper, metreChangeEvent.Lower, newValue);
        BuildChartOperation op1 = new();

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }
    
    private void ComboBoxVisibility_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (Event is not VisibilityChangeEvent visibilityChangeEvent) return;

        bool newValue = ComboBoxVisibility.SelectedIndex != 0;

        VisibilityChangeEditOperation op0 = new(visibilityChangeEvent, visibilityChangeEvent.Visibility, newValue);
        BuildChartOperation op1 = new();
        
        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }
    
    private void TextBoxStopMeasure0_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (Event is not StopEffectEvent stopEffectEvent) return;
        if (string.IsNullOrWhiteSpace(TextBoxStopMeasure0.Text))
        {
            blockEvents = true;

            TextBoxStopMeasure0.Text = null;
            
            blockEvents = false;
            return;
        }

        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxStopMeasure0.Text, CultureInfo.InvariantCulture);
            newValue = Math.Max(0, newValue);
            newValue *= 1920;
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }

        int oldFullTick = stopEffectEvent.SubEvents[0].Timestamp.FullTick;
        int newFullTick = newValue + stopEffectEvent.SubEvents[0].Timestamp.Tick;
        
        TimeableEditOperation op0 = new(stopEffectEvent.SubEvents[0], oldFullTick, newFullTick);
        BuildChartOperation op1 = new();

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    private void TextBoxStopTick0_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (Event is not StopEffectEvent stopEffectEvent) return;
        if (string.IsNullOrWhiteSpace(TextBoxStopTick0.Text))
        {
            blockEvents = true;

            TextBoxStopTick0.Text = null;
            
            blockEvents = false;
            return;
        }

        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxStopTick0.Text, CultureInfo.InvariantCulture);
            newValue = Math.Clamp(newValue, 0, 1919);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }

        int oldFullTick = stopEffectEvent.SubEvents[0].Timestamp.FullTick;
        int newFullTick = newValue + stopEffectEvent.SubEvents[0].Timestamp.Measure * 1920;
        
        TimeableEditOperation op0 = new(stopEffectEvent.SubEvents[0], oldFullTick, newFullTick);
        BuildChartOperation op1 = new();

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    private void TextBoxStopMeasure1_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (Event is not StopEffectEvent stopEffectEvent) return;
        if (string.IsNullOrWhiteSpace(TextBoxStopMeasure1.Text))
        {
            blockEvents = true;

            TextBoxStopMeasure1.Text = null;
            
            blockEvents = false;
            return;
        }

        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxStopMeasure1.Text, CultureInfo.InvariantCulture);
            newValue = Math.Max(0, newValue);
            newValue *= 1920;
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }

        int oldFullTick = stopEffectEvent.SubEvents[1].Timestamp.FullTick;
        int newFullTick = newValue + stopEffectEvent.SubEvents[1].Timestamp.Tick;
        
        TimeableEditOperation op0 = new(stopEffectEvent.SubEvents[1], oldFullTick, newFullTick);
        BuildChartOperation op1 = new();

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    private void TextBoxStopTick1_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (Event is not StopEffectEvent stopEffectEvent) return;
        if (string.IsNullOrWhiteSpace(TextBoxStopTick1.Text))
        {
            blockEvents = true;

            TextBoxStopTick1.Text = null;
            
            blockEvents = false;
            return;
        }

        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxStopTick1.Text, CultureInfo.InvariantCulture);
            newValue = Math.Clamp(newValue, 0, 1919);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }

        int oldFullTick = stopEffectEvent.SubEvents[1].Timestamp.FullTick;
        int newFullTick = newValue + stopEffectEvent.SubEvents[1].Timestamp.Measure * 1920;
        
        TimeableEditOperation op0 = new(stopEffectEvent.SubEvents[1], oldFullTick, newFullTick);
        BuildChartOperation op1 = new();

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    private void TextBoxReverseMeasure0_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (Event is not ReverseEffectEvent reverseEffectEvent) return;
        if (string.IsNullOrWhiteSpace(TextBoxReverseMeasure0.Text))
        {
            blockEvents = true;

            TextBoxReverseMeasure0.Text = null;
            
            blockEvents = false;
            return;
        }

        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxReverseMeasure0.Text, CultureInfo.InvariantCulture);
            newValue = Math.Max(0, newValue);
            newValue *= 1920;
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }

        int oldFullTick = reverseEffectEvent.SubEvents[0].Timestamp.FullTick;
        int newFullTick = newValue + reverseEffectEvent.SubEvents[0].Timestamp.Tick;
        
        TimeableEditOperation op0 = new(reverseEffectEvent.SubEvents[0], oldFullTick, newFullTick);
        BuildChartOperation op1 = new();

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    private void TextBoxReverseTick0_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (Event is not ReverseEffectEvent reverseEffectEvent) return;
        if (string.IsNullOrWhiteSpace(TextBoxReverseTick0.Text))
        {
            blockEvents = true;

            TextBoxReverseTick0.Text = null;
            
            blockEvents = false;
            return;
        }

        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxReverseTick0.Text, CultureInfo.InvariantCulture);
            newValue = Math.Clamp(newValue, 0, 1919);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }

        int oldFullTick = reverseEffectEvent.SubEvents[0].Timestamp.FullTick;
        int newFullTick = newValue + reverseEffectEvent.SubEvents[0].Timestamp.Measure * 1920;
        
        TimeableEditOperation op0 = new(reverseEffectEvent.SubEvents[0], oldFullTick, newFullTick);
        BuildChartOperation op1 = new();

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    private void TextBoxReverseMeasure1_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (Event is not ReverseEffectEvent reverseEffectEvent) return;
        if (string.IsNullOrWhiteSpace(TextBoxReverseMeasure1.Text))
        {
            blockEvents = true;

            TextBoxReverseMeasure1.Text = null;
            
            blockEvents = false;
            return;
        }

        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxReverseMeasure1.Text, CultureInfo.InvariantCulture);
            newValue = Math.Max(0, newValue);
            newValue *= 1920;
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }

        int oldFullTick = reverseEffectEvent.SubEvents[1].Timestamp.FullTick;
        int newFullTick = newValue + reverseEffectEvent.SubEvents[1].Timestamp.Tick;
        
        TimeableEditOperation op0 = new(reverseEffectEvent.SubEvents[1], oldFullTick, newFullTick);
        BuildChartOperation op1 = new();

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    private void TextBoxReverseTick1_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (Event is not ReverseEffectEvent reverseEffectEvent) return;
        if (string.IsNullOrWhiteSpace(TextBoxReverseTick1.Text))
        {
            blockEvents = true;

            TextBoxReverseTick1.Text = null;
            
            blockEvents = false;
            return;
        }

        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxReverseTick1.Text, CultureInfo.InvariantCulture);
            newValue = Math.Clamp(newValue, 0, 1919);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }

        int oldFullTick = reverseEffectEvent.SubEvents[1].Timestamp.FullTick;
        int newFullTick = newValue + reverseEffectEvent.SubEvents[1].Timestamp.Measure * 1920;
        
        TimeableEditOperation op0 = new(reverseEffectEvent.SubEvents[1], oldFullTick, newFullTick);
        BuildChartOperation op1 = new();

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    private void TextBoxReverseMeasure2_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (Event is not ReverseEffectEvent reverseEffectEvent) return;
        if (string.IsNullOrWhiteSpace(TextBoxReverseMeasure2.Text))
        {
            blockEvents = true;

            TextBoxReverseMeasure2.Text = null;
            
            blockEvents = false;
            return;
        }

        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxReverseMeasure2.Text, CultureInfo.InvariantCulture);
            newValue = Math.Max(0, newValue);
            newValue *= 1920;
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }

        int oldFullTick = reverseEffectEvent.SubEvents[2].Timestamp.FullTick;
        int newFullTick = newValue + reverseEffectEvent.SubEvents[2].Timestamp.Tick;
        
        TimeableEditOperation op0 = new(reverseEffectEvent.SubEvents[2], oldFullTick, newFullTick);
        BuildChartOperation op1 = new();

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    private void TextBoxReverseTick2_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (Event is not ReverseEffectEvent reverseEffectEvent) return;
        if (string.IsNullOrWhiteSpace(TextBoxReverseTick2.Text))
        {
            blockEvents = true;

            TextBoxReverseTick2.Text = null;
            
            blockEvents = false;
            return;
        }

        int newValue = 0;

        try
        {
            newValue = Convert.ToInt32(TextBoxReverseTick2.Text, CultureInfo.InvariantCulture);
            newValue = Math.Clamp(newValue, 0, 1919);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }

        int oldFullTick = reverseEffectEvent.SubEvents[2].Timestamp.FullTick;
        int newFullTick = newValue + reverseEffectEvent.SubEvents[2].Timestamp.Measure * 1920;
        
        TimeableEditOperation op0 = new(reverseEffectEvent.SubEvents[2], oldFullTick, newFullTick);
        BuildChartOperation op1 = new();

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }
#endregion UI Event Delegates
}