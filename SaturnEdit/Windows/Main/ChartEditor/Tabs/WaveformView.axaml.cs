using Avalonia.Controls;
using SkiaSharp;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class WaveformView : UserControl
{
    public WaveformView()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        double minimum = double.Min(Bounds.Width, Bounds.Height);
        RenderCanvas.Width = minimum;
        RenderCanvas.Height = minimum;
    }

    private void RenderCanvas_OnRenderAction(SKCanvas canvas) { }
}