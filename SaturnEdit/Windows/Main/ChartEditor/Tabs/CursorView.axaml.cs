using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class CursorView : UserControl
{
    public CursorView()
    {
        InitializeComponent();
    }

    private bool blockEvents = false;
    
#region System Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        TimeSystem.TimestampChanged += OnTimestampChanged;
        OnTimestampChanged(null, EventArgs.Empty);
        
        TimeSystem.DivisionChanged += OnDivisionChanged;
        OnDivisionChanged(null, EventArgs.Empty);
        
        CursorSystem.CursorChanged += OnCursorChanged;
        OnCursorChanged(null, EventArgs.Empty);
        
        base.OnLoaded(e);
    }
    
    private void OnTimestampChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            NumericUpDownMeasure.Value = TimeSystem.Timestamp.Measure;
            NumericUpDownBeat.Value = Timestamp.BeatFromTick(TimeSystem.Timestamp.Tick, TimeSystem.Division);
            
            blockEvents = false;

            NumericUpDownBeat.Minimum = TimeSystem.Timestamp.Measure == 0 ? 0 : -1;
        });
    }

    private void OnDivisionChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
        
            NumericUpDownDivision.Value = TimeSystem.Division;
            NumericUpDownBeat.Maximum = TimeSystem.Division + 1;
            
            bool oddDivision = 1920 % TimeSystem.Division != 0;
            IconOddDivisionWarning.IsVisible = oddDivision;
            
            blockEvents = false;
        });
    }

    private void OnCursorChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            SliderPosition.Value = CursorSystem.Position;
            SliderSize.Value = CursorSystem.Size;
            
            blockEvents = false;
            
            TextBlockPosition.Text = $"{(int)SliderPosition.Value}";
            TextBlockSize.Text = $"{(int)SliderSize.Value}";
        });
    }
#endregion System Event Handlers

#region UI Event Handlers
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        TimeSystem.TimestampChanged -= OnTimestampChanged;
        TimeSystem.DivisionChanged -= OnDivisionChanged;
        CursorSystem.CursorChanged -= OnCursorChanged;
        
        base.OnUnloaded(e);
    }
    private void SliderPosition_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        CursorSystem.Position = (int)SliderPosition.Value;
    }
    
    private void SliderSize_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        CursorSystem.Size = (int)SliderSize.Value;
    }

    private void NumericUpDownMeasure_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        int measure = (int?)NumericUpDownMeasure.Value ?? 0;
        int tick = TimeSystem.Timestamp.Tick;
        
        TimeSystem.SeekMeasureTick(measure, tick);
    }
    
    private void NumericUpDownBeat_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        int measure = TimeSystem.Timestamp.Measure;
        int beat = (int?)NumericUpDownBeat.Value ?? 0;
        
        if (beat == -1)
        {
            beat = TimeSystem.Division - 1;
            measure -= 1;
        }
        
        if (beat >= TimeSystem.Division)
        {
            beat = 0;
            measure += 1;
        }

        int tick = Timestamp.TickFromBeat(beat, TimeSystem.Division);
        
        TimeSystem.SeekMeasureTick(measure, tick);
    }
    
    private void NumericUpDownDivision_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        TimeSystem.Division = (int?)NumericUpDownDivision.Value ?? TimeSystem.DefaultDivision;
    }
#endregion UI Event Handlers
}