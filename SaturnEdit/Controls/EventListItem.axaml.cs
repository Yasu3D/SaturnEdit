using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnData.Notation.Core;
using SaturnData.Notation.Events;

namespace SaturnEdit.Controls;

// TODO: Implement all UI Event Delegates
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
    }

    private void TextBoxTick_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
    }

    private void TextBoxFloatValue_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
    }

    private void TextBoxMetreUpper_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
    }

    private void TextBoxMetreLower_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
    }
    
    private void ComboBoxVisibility_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
    }
    
    private void TextBoxStopMeasure0_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
    }

    private void TextBoxStopTick0_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
    }

    private void TextBoxStopMeasure1_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
    }

    private void TextBoxStopTick1_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
    }

    private void TextBoxReverseMeasure0_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
    }

    private void TextBoxReverseTick0_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
    }

    private void TextBoxReverseMeasure1_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
    }

    private void TextBoxReverseTick1_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
    }

    private void TextBoxReverseMeasure2_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
    }

    private void TextBoxReverseTick2_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
    }
#endregion UI Event Delegates
}