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
    }

    private bool blockEvents = false;

    private void OnTimestampChanged(object? sender, EventArgs e)
    {
        
    }

    private void OnDivisionChanged(object? sender, EventArgs e)
    {
        int value = 0;
        bool oddDivision = 1920 % value != 0;
        
        IconOddDivisionWarning.IsVisible = oddDivision;
    }
    
    private void SliderPosition_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        TextBlockPosition.Text = $"{(int)SliderPosition.Value}";
    }
    
    private void SliderSize_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        TextBlockSize.Text = $"{(int)SliderSize.Value}";
    }

    private void NumericUpDownDivision_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        int value = (int?)NumericUpDownDivision.Value ?? 8;
        
    }

    private void NumericUpDownBeat_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        int value = (int?)NumericUpDownBeat.Value ?? 0;
    }

    private void NumericUpDownMeasure_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        int value = (int?)NumericUpDownMeasure.Value ?? 0;
    }
}