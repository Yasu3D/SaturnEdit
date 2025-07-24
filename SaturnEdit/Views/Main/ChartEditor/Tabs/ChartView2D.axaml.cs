using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using SaturnView;
using SkiaSharp;

namespace SaturnEdit.Views.Main.ChartEditor.Tabs;

public partial class ChartView2D : UserControl
{
    public ChartView2D()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
        ActualThemeVariantChanged += OnActualThemeVariantChanged;
    }

    private async void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        try
        {
            await Task.Delay(1);

            if (Application.Current == null) return;
            if (!Application.Current.TryGetResource("BackgroundSecondary", Application.Current.ActualThemeVariant, out object? resource)) return;
            if (resource is not SolidColorBrush brush) return;
        
            clearColor = new(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
        }
        catch (Exception ex)
        {
            // classic error pink
            clearColor = new(0xFF, 0x00, 0xFF, 0xFF);
        }
    }

    private readonly CanvasInfo canvasInfo = new();
    private SKColor clearColor;

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        double minimum = double.Min(PanelCanvasContainer.Bounds.Width, PanelCanvasContainer.Bounds.Height);
        RenderCanvas.Width = minimum;
        RenderCanvas.Height = minimum;

        canvasInfo.Width = (float)RenderCanvas.Width;
        canvasInfo.Height = (float)RenderCanvas.Height;
        canvasInfo.Radius = canvasInfo.Width / 2;
        canvasInfo.Center = new(canvasInfo.Radius, canvasInfo.Radius);
    }

    private void RenderCanvas_OnRenderAction(SKCanvas canvas) => Renderer2D.Render(canvas, canvasInfo, clearColor);
}