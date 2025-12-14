using System;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using SaturnEdit.Systems;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ModalDialog;
using SaturnView;
using SkiaSharp;

namespace SaturnEdit.Windows.Dialogs.ChooseMirrorAxis;

public partial class SelectMirrorAxisWindow : Window
{
    public SelectMirrorAxisWindow()
    {
        InitializeComponent();
        InitializeDialog();
    }

    public ModalDialogResult Result { get; private set; } = ModalDialogResult.Cancel;
    public int Axis { get; private set; } = 0;

    private bool blockEvents = false;
    
    private IDisposable? keyDownEventHandler = null;
    private IDisposable? keyUpEventHandler = null;
    
    private SKColor backgroundColor = new(0xFF000000);
    
    private readonly SKPaint linePaint = new()
    {
        Color = 0xFFFFFFFF,
        StrokeWidth = 1.5f,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
    };

    private readonly SKPaint axisPaint = new()
    {
        Color = 0xFFFF0000,
        StrokeWidth = 3,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
    };

#region Methods
    private void InitializeDialog()
    {
        blockEvents = true;

        Axis = EditorSystem.MirrorAxis;

        SliderAxis.Value = Axis;
        TextBlockAxis.Text = Axis.ToString(CultureInfo.InvariantCulture);
        
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
    
    private async void Control_OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        try
        {
            await Task.Delay(1);

            if (Application.Current == null) return;
            if (!Application.Current.TryGetResource("BackgroundPrimary", Application.Current.ActualThemeVariant, out object? resource)) return;
            if (resource is not SolidColorBrush brush) return;
        
            backgroundColor = new(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
            
            if (!Application.Current.TryGetResource("ForegroundSecondary", Application.Current.ActualThemeVariant, out resource)) return;
            if (resource is not SolidColorBrush brush2) return;
            
            linePaint.Color = new(brush2.Color.R, brush2.Color.G, brush2.Color.B, brush2.Color.A);
            
            if (!Application.Current.TryGetResource("MirrorAxis", Application.Current.ActualThemeVariant, out resource)) return;
            if (resource is not SolidColorBrush brush3) return;
            
            axisPaint.Color = new(brush3.Color.R, brush3.Color.G, brush3.Color.B, brush3.Color.A);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
            
            backgroundColor = new(0xFFFF00FF);
            linePaint.Color = new(0xFFFF00FF);
            axisPaint.Color = new(0xFFFF00FF);
        }
    }
    
    private void RenderCanvas_OnRenderAction(SKCanvas canvas)
    {
        canvas.Clear(backgroundColor);
        canvas.DrawCircle(150, 150, 140, linePaint);

        for (int i = 0; i < 60; i++)
        {
            float angle = i * -6;
            float radius = i % 5 == 0 ? 120 : 135;
            
            SKPoint p0 = RenderUtils.PointOnArc(150, 150, 145, angle);
            SKPoint p1 = RenderUtils.PointOnArc(150, 150, radius, angle);
            
            canvas.DrawLine(p0, p1, linePaint);
        }

        float axisAngle = Axis * -3;
        
        SKPoint axis0 = RenderUtils.PointOnArc(150, 150, 150, axisAngle);
        SKPoint axis1 = RenderUtils.PointOnArc(150, 150, 150, axisAngle + 180);
        
        canvas.DrawLine(axis0, axis1, axisPaint);
    }
    
    private void SliderAxis_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (SliderAxis == null) return;

        Axis = (int)SliderAxis.Value;
        TextBlockAxis.Text = Axis.ToString(CultureInfo.InvariantCulture);
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