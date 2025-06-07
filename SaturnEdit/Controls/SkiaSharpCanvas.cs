using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;

namespace SaturnEdit.Controls;

public class SkiaSharpCanvas : UserControl
{
    public event Action<SKCanvas>? RenderAction;
    public float CanvasWidth { get; private set; }
    public float CanvasHeight { get; private set; }
    public float CanvasSize { get; private set; }
    public SKPoint CanvasCenter { get; private set; }
    
    public SkiaSharpCanvas()
    {
        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        CanvasWidth = (float)Bounds.Width;
        CanvasHeight = (float)Bounds.Height;
        CanvasSize = float.Min(CanvasWidth, CanvasHeight);
        CanvasCenter = new(CanvasWidth * 0.5f, CanvasHeight * 0.5f);
    }

    public override void Render(DrawingContext context)
    {
        if (RenderAction == null) return;
        
        context.Custom(new SkiaDrawOperation(new(0, 0, DesiredSize.Width, DesiredSize.Height), RenderAction));
        Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
    }

    private class SkiaDrawOperation : ICustomDrawOperation
    {
        public Rect Bounds { get; }
        private readonly Action<SKCanvas> renderAction;
        
        public SkiaDrawOperation(Rect bounds, Action<SKCanvas> renderAction)
        {
            Bounds = bounds;
            this.renderAction = renderAction;
        }

        public void Dispose() { }

        public bool HitTest(Point p) { return false; }
        
        public bool Equals(ICustomDrawOperation? other) { return false; }

        public void Render(ImmediateDrawingContext context)
        {
            ISkiaSharpApiLeaseFeature? leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null) return;

            using ISkiaSharpApiLease lease = leaseFeature.Lease();
            SKCanvas canvas = lease.SkCanvas;
            renderAction.Invoke(canvas);
        }
    }
}