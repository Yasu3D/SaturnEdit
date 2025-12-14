using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ModalDialog;
using SkiaSharp;

namespace SaturnEdit.Windows.Dialogs.ZigZagHoldArgs;

public partial class ZigZagHoldArgsWindow : Window
{
    public ZigZagHoldArgsWindow()
    {
        InitializeComponent();
        InitializeDialog();
    }

    public int Beats { get; set; } = 1;
    public int Division { get; set; } = 16;
    public int LeftEdgeOffsetA { get; set; } = 0;
    public int LeftEdgeOffsetB { get; set; } = 0;
    public int RightEdgeOffsetA { get; set; } = 0;
    public int RightEdgeOffsetB { get; set; } = 0;

    public ModalDialogResult Result { get; private set; } = ModalDialogResult.Cancel;

    private bool blockEvents = false;
    
    private IDisposable? keyDownEventHandler = null;
    private IDisposable? keyUpEventHandler = null;
    
    private SKColor backgroundColor = new(0xFF000000);
    
    private readonly SKPaint gridPaint = new()
    {
        Color = 0xFFFFFFFF,
        StrokeWidth = 1,
        IsAntialias = false,
        Style = SKPaintStyle.Stroke,
    };
    
    private readonly SKPaint guidePaint = new()
    {
        Color = 0xFFFFFFFF,
        StrokeWidth = 1,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke, 
        PathEffect = SKPathEffect.CreateDash([ 1, 1 ], 0),
    };
    
    private readonly SKPaint surfacePaint = new()
    {
        Color = 0xFFFFFFFF,
        Style = SKPaintStyle.Fill,
    };

#region Methods
    private void InitializeDialog()
    {
        blockEvents = true;
        
        NumericUpDownBeats.Value = Beats;
        NumericUpDownDivision.Value = Division;
            
        SliderLeftEdgeOffsetA.Value = LeftEdgeOffsetA;
        SliderLeftEdgeOffsetB.Value = LeftEdgeOffsetB;
        SliderRightEdgeOffsetA.Value = RightEdgeOffsetA;
        SliderRightEdgeOffsetB.Value = RightEdgeOffsetB;

        TextBlockLeftEdgeOffsetA.Text = LeftEdgeOffsetA.ToString(CultureInfo.InvariantCulture);
        TextBlockLeftEdgeOffsetB.Text = LeftEdgeOffsetB.ToString(CultureInfo.InvariantCulture);
        TextBlockRightEdgeOffsetA.Text = RightEdgeOffsetA.ToString(CultureInfo.InvariantCulture);
        TextBlockRightEdgeOffsetB.Text = RightEdgeOffsetB.ToString(CultureInfo.InvariantCulture);

        blockEvents = false;
    }
#endregion Methods
    
#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        ActualThemeVariantChanged += Control_OnActualThemeVariantChanged;
        Control_OnActualThemeVariantChanged(null, EventArgs.Empty);
        
        keyDownEventHandler = KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        keyUpEventHandler = KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        ActualThemeVariantChanged -= Control_OnActualThemeVariantChanged;
        
        keyDownEventHandler?.Dispose();
        keyUpEventHandler?.Dispose();
        
        base.OnUnloaded(e);
    }
    
    private void Control_OnKeyDown(object? sender, KeyEventArgs e)
    {
        IInputElement? focusedElement = GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (KeyDownBlacklist.IsInvalidFocusedElement(focusedElement)) return;
        if (KeyDownBlacklist.IsInvalidKey(e.Key)) return;
        if (KeyDownBlacklist.IsInvalidState()) return;

        if (e.Key == Key.Escape)
        {
            Result = ModalDialogResult.Cancel;
            Close();
        }

        if (e.Key == Key.Enter)
        {
            Result = ModalDialogResult.Primary;
            Close();
        }
    }
    
    private void Control_OnKeyUp(object? sender, KeyEventArgs e) => e.Handled = true;
    
    private void RenderCanvas_OnRenderAction(SKCanvas canvas)
    {
        const int pixelsPerOffset = 4;
        const float leftNeutralEdge = 250 + pixelsPerOffset * 30;
        const float rightNeutralEdge = 250 - pixelsPerOffset * 30;
        float pixelsPerVertexRow = 405.0f * Beats / Division;
        
        // Generate rows of vertices except for the last.
        List<SKPoint> vertices = [];
        bool stepIsA = false;
        float v;
        for (v = 405; v >= 0; v -= pixelsPerVertexRow)
        {
            addVertices(v, stepIsA);
        }
        
        // Add final row of vertices
        addVertices(v, stepIsA);
        
        List<SKPoint> triangles = [];
        int quadCount = (vertices.Count - 2) / 2;

        for (int i = 0; i < quadCount; i++)
        {
            triangles.Add(vertices[i * 2]);
            triangles.Add(vertices[i * 2 + 1]);
            triangles.Add(vertices[i * 2 + 2]);
            triangles.Add(vertices[i * 2 + 3]);
            triangles.Add(vertices[i * 2 + 2]);
            triangles.Add(vertices[i * 2 + 1]);
        }
        
        // Begin drawing.
        canvas.Clear(backgroundColor);

        // Draw grid
        for (float x = 0; x < 500; x += pixelsPerOffset * 5)
        {
            float offsetX = x + pixelsPerOffset * 2.5f;
            canvas.DrawLine(offsetX, 0, offsetX, 405, gridPaint);
        }

        for (float y = 405; y >= 0; y -= pixelsPerOffset * 5)
        {
            canvas.DrawLine(0, y, 500, y, gridPaint);
        }
        
        // Draw surface
        canvas.DrawVertices(SKVertexMode.Triangles, triangles.ToArray(), null, null, surfacePaint);
        
        // Draw guide lines
        canvas.DrawLine(leftNeutralEdge, 0, leftNeutralEdge, 405, guidePaint);
        canvas.DrawLine(rightNeutralEdge, 0, rightNeutralEdge, 405, guidePaint);
        
        return;

        void addVertices(float height, bool isA)
        {
            float leftX = stepIsA
                ? 250 - pixelsPerOffset * (LeftEdgeOffsetA + 30)
                : 250 - pixelsPerOffset * (LeftEdgeOffsetB + 30);

            float rightX = stepIsA
                ? 250 + pixelsPerOffset * (RightEdgeOffsetA + 30)
                : 250 + pixelsPerOffset * (RightEdgeOffsetB + 30);
            
            SKPoint left = new(leftX, height);
            SKPoint right = new(rightX, height);
            
            vertices.Add(left);
            vertices.Add(right);
            
            stepIsA = !stepIsA;
        }
    }
    
    private async void Control_OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        try
        {
            await Task.Delay(1);

            if (Application.Current == null) return;
            if (!Application.Current.TryGetResource("BackgroundSecondary", Application.Current.ActualThemeVariant, out object? resource)) return;
            if (resource is not SolidColorBrush brush) return;
        
            backgroundColor = new(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
            
            if (!Application.Current.TryGetResource("ZigZagHoldGrid", Application.Current.ActualThemeVariant, out resource)) return;
            if (resource is not SolidColorBrush brush2) return;
            
            gridPaint.Color = new(brush2.Color.R, brush2.Color.G, brush2.Color.B, brush2.Color.A);
            
            if (!Application.Current.TryGetResource("ZigZagHoldGuide", Application.Current.ActualThemeVariant, out resource)) return;
            if (resource is not SolidColorBrush brush3) return;
            
            guidePaint.Color = new(brush3.Color.R, brush3.Color.G, brush3.Color.B, brush3.Color.A);
            
            if (!Application.Current.TryGetResource("ZigZagHoldSurface", Application.Current.ActualThemeVariant, out resource)) return;
            if (resource is not SolidColorBrush brush4) return;
            
            surfacePaint.Color = new(brush4.Color.R, brush4.Color.G, brush4.Color.B, brush4.Color.A);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
            
            backgroundColor = new(0xFFFF00FF);
            gridPaint.Color = new(0xFFFF00FF);
            guidePaint.Color = new(0xFFFF00FF);
            surfacePaint.Color = new(0xFFFF00FF);
        }
    }
    
    private void NumericUpDownBeats_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        Beats = (int?)NumericUpDownBeats.Value ?? 16;
    }
    
    private void NumericUpDownDivision_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        Division = (int?)NumericUpDownDivision.Value ?? 16;
    }
    
    private void SliderLeftEdgeOffsetA_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        LeftEdgeOffsetA = (int)SliderLeftEdgeOffsetA.Value;
        
        InitializeDialog();
    }
    
    private void SliderLeftEdgeOffsetB_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        LeftEdgeOffsetB = (int)SliderLeftEdgeOffsetB.Value;
        
        InitializeDialog();
    }
    
    private void SliderRightEdgeOffsetA_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        RightEdgeOffsetA = (int)SliderRightEdgeOffsetA.Value;
        
        InitializeDialog();
    }
    
    private void SliderRightEdgeOffsetB_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        RightEdgeOffsetB = (int)SliderRightEdgeOffsetB.Value;
        
        InitializeDialog();
    }
    
    private void ButtonOk_OnClick(object? sender, RoutedEventArgs e)
    {
        Result = ModalDialogResult.Primary;
        Close();
    }
    
    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Result = ModalDialogResult.Cancel;
        Close();
    }
#endregion UI Event Handlers
}