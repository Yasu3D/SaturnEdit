using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace SaturnEdit.Views.Main.ChartEditor.Tabs;

public partial class CursorView : UserControl
{
    public CursorView()
    {
        InitializeComponent();
    }

    private void SliderPosition_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (sender == null) return;
        
        TextBlockPosition.Text = $"{(int)SliderPosition.Value}";
    }
    
    private void SliderSize_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (sender == null) return;
        
        TextBlockSize.Text = $"{(int)SliderSize.Value}";
    }
}