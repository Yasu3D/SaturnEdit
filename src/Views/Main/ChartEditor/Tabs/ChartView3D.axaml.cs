using System;
using Avalonia.Controls;
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
        Console.WriteLine("ChartView3D: OnSizeChanged");
        double minimum = double.Min(Bounds.Width, Bounds.Height);
        RenderCanvas.Width = minimum;
        RenderCanvas.Height = minimum;
    }

    private void RenderCanvas_OnRenderAction(SKCanvas canvas)
    {
        SKRect rect = new(0, 0, RenderCanvas.CanvasWidth, RenderCanvas.CanvasHeight);
        canvas.DrawOval(rect, new SKPaint { Color = SKColors.Red });
    }
}