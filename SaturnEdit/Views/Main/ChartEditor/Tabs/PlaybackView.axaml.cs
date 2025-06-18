using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;

namespace SaturnEdit.Views.Main.ChartEditor.Tabs;

public partial class PlaybackView : UserControl
{
    public PlaybackView()
    {
        InitializeComponent();
        SetTickMargins();
    }

    private void SetTickMargins()
    {
        Dispatcher.UIThread.Post(() =>
        {
            double sliderWidth = SliderPlaybackSpeed.Bounds.Width - 10;

            TickPlaybackSpeed25.Margin  = new(sliderWidth * ( 25.0 / 300), 0, 0, 0);
            TickPlaybackSpeed50.Margin  = new(sliderWidth * ( 50.0 / 300), 0, 0, 0);
            TickPlaybackSpeed100.Margin = new(sliderWidth * (100.0 / 300), 0, 0, 0);
            TickPlaybackSpeed200.Margin = new(sliderWidth * (200.0 / 300), 0, 0, 0);
        });
    }

    private void SliderPlaybackSpeed_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (sender is not Slider slider) return;
        
        TextBlockPlaybackSpeed.Text = slider.Value.ToString(CultureInfo.InvariantCulture);
    }
}