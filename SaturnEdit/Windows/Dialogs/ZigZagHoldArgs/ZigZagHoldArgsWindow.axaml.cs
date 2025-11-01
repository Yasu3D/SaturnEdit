using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using SaturnEdit.Windows.Dialogs.ModalDialog;
using SkiaSharp;

namespace SaturnEdit.Windows.Dialogs.ZigZagHoldArgs;

public partial class ZigZagHoldArgsWindow : Window
{
    public ZigZagHoldArgsWindow()
    {
        InitializeComponent();
        InitializeDialog();

        ActualThemeVariantChanged += Control_OnActualThemeVariantChanged;
        Control_OnActualThemeVariantChanged(null, EventArgs.Empty);
    }

    public int Beats { get; set; } = 1;
    public int Division { get; set; } = 16;
    public int LeftEdgeOffsetA { get; set; } = 0;
    public int LeftEdgeOffsetB { get; set; } = 0;
    public int RightEdgeOffsetA { get; set; } = 0;
    public int RightEdgeOffsetB { get; set; } = 0;

    public ModalDialogResult Result { get; private set; } = ModalDialogResult.Cancel;

    private bool blockEvents = false;
    
    private SKColor backgroundColor = new(0xFF000000);
    
    private readonly SKPaint gridPaint = new()
    {
        Color = 0xFFFFFFFF,
        StrokeWidth = 1,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
    };
    
    private readonly SKPaint guidePaint = new()
    {
        Color = 0xFFFFFFFF,
        StrokeWidth = 2,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke, 
        PathEffect = SKPathEffect.CreateDash([ 4, 4 ], 0),
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
    
#region UI Event Delegates
    private void RenderCanvas_OnRenderAction(SKCanvas canvas)
    {
        canvas.Clear(backgroundColor);

        for (int i = 0; i < 500; i += 25)
        {
            float p = i + 12.5f;
            
            canvas.DrawLine(p, 0, p, 500, gridPaint);
            canvas.DrawLine(0, p, 500, p, gridPaint);
        }
        
        int pixelsPerStep = 200 * Beats / Division;
        int pixelsPerOffset = 7;

        int leftNeutralEdge = 150;
        int rightNeutralEdge = 350;

        List<SKPoint> vertices = [];

        bool stepIsA = false;
        int y;
        for (y = 405; y >= 0; y -= pixelsPerStep)
        {
            addVertices(y, stepIsA);
        }
        
        // Add extra row of vertices
        addVertices(y, stepIsA);
        
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
        
        canvas.DrawVertices(SKVertexMode.Triangles, triangles.ToArray(), null, null, surfacePaint);

        canvas.DrawLine(leftNeutralEdge, 0, leftNeutralEdge, 405, guidePaint);
        canvas.DrawLine(rightNeutralEdge, 0, rightNeutralEdge, 405, guidePaint);
        
        return;

        void addVertices(int height, bool isA)
        {
            float leftX = stepIsA
                ? leftNeutralEdge - pixelsPerOffset * LeftEdgeOffsetA
                : leftNeutralEdge - pixelsPerOffset * LeftEdgeOffsetB;

            float rightX = stepIsA
                ? rightNeutralEdge + pixelsPerOffset * RightEdgeOffsetA
                : rightNeutralEdge + pixelsPerOffset * RightEdgeOffsetB;
            
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
#endregion UI Event Delegates
}