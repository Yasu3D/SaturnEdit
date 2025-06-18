using System;
using Avalonia.Controls;
using SaturnView;
using SkiaSharp;

namespace SaturnEdit.Views.Main.ChartEditor.Tabs;

public partial class ChartView3D : UserControl
{
    public ChartView3D()
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

    private void RenderCanvas_OnRenderAction(SKCanvas canvas) => Renderer3D.Render(canvas);
}