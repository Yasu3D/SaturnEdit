using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using SaturnEdit.Systems;

namespace SaturnEdit.Views.Main.ChartEditor.Tabs;

public partial class CursorView : UserControl
{
    public CursorView()
    {
        InitializeComponent();
        
        TimeSystem.TimestampChanged += OnTimestampChanged;
        TimeSystem.DivisionChanged += OnDivisionChanged;
        CursorSystem.ShapeChanged += OnShapeChanged;
        
        OnTimestampChanged(null, EventArgs.Empty);
        OnDivisionChanged(null, EventArgs.Empty);
        OnShapeChanged(null, EventArgs.Empty);
    }

    private bool blockEvents = false;

    private void OnTimestampChanged(object? sender, EventArgs e)
    {
        blockEvents = true;

        NumericUpDownMeasure.Value = TimeSystem.Timestamp.Measure;
        NumericUpDownBeat.Value = TimeSystem.Timestamp.Tick / TimeSystem.DivisionInterval;
        
        blockEvents = false;

        NumericUpDownBeat.Minimum = TimeSystem.Timestamp.Measure == 0 ? 0 : -1;
    }

    private void OnDivisionChanged(object? sender, EventArgs e)
    {
        blockEvents = true;
        
        NumericUpDownDivision.Value = TimeSystem.Division;

        blockEvents = false;
        
        NumericUpDownBeat.Value = Math.Clamp((int?)NumericUpDownBeat.Value ?? 0, 0, TimeSystem.Division - 1);
        NumericUpDownBeat.Maximum = TimeSystem.Division + 1;
        
        bool oddDivision = 1920 % TimeSystem.Division != 0;
        IconOddDivisionWarning.IsVisible = oddDivision;
    }

    private void OnShapeChanged(object? sender, EventArgs e)
    {
        blockEvents = true;

        SliderPosition.Value = CursorSystem.Position;
        SliderSize.Value = CursorSystem.Size;
        
        blockEvents = false;
        
        TextBlockPosition.Text = $"{(int)SliderPosition.Value}";
        TextBlockSize.Text = $"{(int)SliderSize.Value}";
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

        int value = (int?)NumericUpDownMeasure.Value ?? 0;
        TimeSystem.Timestamp = new(value, TimeSystem.Timestamp.Tick);
    }
    
    private void NumericUpDownBeat_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        int value = (int?)NumericUpDownBeat.Value ?? 0;
        
        if (value == -1)
        {
            value = TimeSystem.Division - 1;
            TimeSystem.Timestamp -= 1920;
        }
        
        if (value >= TimeSystem.Division)
        {
            value = 0;
            TimeSystem.Timestamp += 1920;
        }
        
        TimeSystem.Timestamp = new(TimeSystem.Timestamp.Measure, value * TimeSystem.DivisionInterval);
    }
    
    private void NumericUpDownDivision_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        int value = (int?)NumericUpDownDivision.Value ?? TimeSystem.DefaultDivision;
        TimeSystem.Division = value;
    }
}