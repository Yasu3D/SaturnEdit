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
        
        PlayheadSystem.TimestampChanged += OnTimestampChanged;
        PlayheadSystem.DivisionChanged += OnDivisionChanged;
        PlayheadSystem.ShapeChanged += OnShapeChanged;
        
        OnTimestampChanged(null, EventArgs.Empty);
        OnDivisionChanged(null, EventArgs.Empty);
        OnShapeChanged(null, EventArgs.Empty);
    }

    private bool blockEvents = false;

    private void OnTimestampChanged(object? sender, EventArgs e)
    {
        blockEvents = true;

        NumericUpDownMeasure.Value = PlayheadSystem.Timestamp.Measure;
        NumericUpDownBeat.Value = PlayheadSystem.Timestamp.Tick / PlayheadSystem.DivisionInterval;
        
        blockEvents = false;

        NumericUpDownBeat.Minimum = PlayheadSystem.Timestamp.Measure == 0 ? 0 : -1;
    }

    private void OnDivisionChanged(object? sender, EventArgs e)
    {
        blockEvents = true;
        
        NumericUpDownDivision.Value = PlayheadSystem.Division;

        blockEvents = false;
        
        NumericUpDownBeat.Value = Math.Clamp((int?)NumericUpDownBeat.Value ?? 0, 0, PlayheadSystem.Division - 1);
        NumericUpDownBeat.Maximum = PlayheadSystem.Division + 1;
        
        bool oddDivision = 1920 % PlayheadSystem.Division != 0;
        IconOddDivisionWarning.IsVisible = oddDivision;
    }

    private void OnShapeChanged(object? sender, EventArgs e)
    {
        blockEvents = true;

        SliderPosition.Value = PlayheadSystem.Position;
        SliderSize.Value = PlayheadSystem.Size;
        
        blockEvents = false;
        
        TextBlockPosition.Text = $"{(int)SliderPosition.Value}";
        TextBlockSize.Text = $"{(int)SliderSize.Value}";
    }
    
    private void SliderPosition_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        PlayheadSystem.Position = (int)SliderPosition.Value;
    }
    
    private void SliderSize_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        PlayheadSystem.Size = (int)SliderSize.Value;
    }

    private void NumericUpDownMeasure_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        int value = (int?)NumericUpDownMeasure.Value ?? 0;
        PlayheadSystem.Timestamp = new(value, PlayheadSystem.Timestamp.Tick);
    }
    
    private void NumericUpDownBeat_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        int value = (int?)NumericUpDownBeat.Value ?? 0;
        
        if (value == -1)
        {
            value = PlayheadSystem.Division - 1;
            PlayheadSystem.Timestamp -= 1920;
        }
        
        if (value >= PlayheadSystem.Division)
        {
            value = 0;
            PlayheadSystem.Timestamp += 1920;
        }
        
        PlayheadSystem.Timestamp = new(PlayheadSystem.Timestamp.Measure, value * PlayheadSystem.DivisionInterval);
    }
    
    private void NumericUpDownDivision_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        int value = (int?)NumericUpDownDivision.Value ?? PlayheadSystem.DefaultDivision;
        PlayheadSystem.Division = value;
    }
}