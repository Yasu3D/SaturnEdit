using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using SaturnData.Utilities;
using SkiaSharp;

namespace SaturnEdit.Controls;

public partial class SaturnColorPicker : UserControl
{
    public SaturnColorPicker()
    {
        InitializeComponent();
        
        ActualThemeVariantChanged += OnActualThemeVariantChanged;
    }
    
    public event EventHandler? ColorPickFinished;

    public uint Color
    {
        get
        {
            float r = 0;
            float g = 0;
            float b = 0;

            float h = Hue / 360;
            float s = Saturation / 100;
            float v = Value / 100;

            float i = MathF.Floor(h * 6);
            float f = h * 6 - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);

            switch (i % 6)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                case 5: r = v; g = p; b = q; break;
            }

            byte xR = (byte)(r * 255);
            byte xG = (byte)(g * 255);
            byte xB = (byte)(b * 255);
            
            return (uint)(0xFF000000 + (xR << 16) + (xG << 8) + xB);
        }
        set
        {
            SKColor c = new(value);
            c.ToHsv(out float h, out float s, out float v);

            Hue = h;
            Saturation = s;
            Value = v;
        }
    }
    
    public float Hue { get; private set; } = 0;
    public float Saturation { get; private set; } = 100;
    public float Value { get; private set; } = 100;
    
    private readonly SKPaint selectorPaint = new()
    {
        Color = new(0xFFFFFFFF),
        Style = SKPaintStyle.Fill,
        IsAntialias = true,
    };
    
    private readonly SKPaint outlinePaint = new()
    {
        Style = SKPaintStyle.Fill,
        IsAntialias = true,
    };
    
    private readonly SKPaint svPaint = new()
    {
        Color = new(0xFFFFFFFF),
        Style = SKPaintStyle.Fill,
        IsAntialias = true,
    };

    private readonly SKPaint huePaint = new()
    {
        Color = new(0xFFFFFFFF),
        Style = SKPaintStyle.Fill,
        IsAntialias = true,
        Shader = SKShader.CreateLinearGradient
        (
            start: new(0, 0), 
            end: new(220, 0), 
            colors:
            [
                SKColor.FromHsv(0,   100, 100), 
                SKColor.FromHsv(20,  100, 100), 
                SKColor.FromHsv(40,  100, 100), 
                SKColor.FromHsv(60,  100, 100), 
                SKColor.FromHsv(80,  100, 100), 
                SKColor.FromHsv(100, 100, 100), 
                SKColor.FromHsv(120, 100, 100), 
                SKColor.FromHsv(140, 100, 100), 
                SKColor.FromHsv(160, 100, 100), 
                SKColor.FromHsv(180, 100, 100), 
                SKColor.FromHsv(200, 100, 100), 
                SKColor.FromHsv(220, 100, 100), 
                SKColor.FromHsv(240, 100, 100), 
                SKColor.FromHsv(260, 100, 100), 
                SKColor.FromHsv(280, 100, 100), 
                SKColor.FromHsv(300, 100, 100), 
                SKColor.FromHsv(320, 100, 100), 
                SKColor.FromHsv(340, 100, 100), 
                SKColor.FromHsv(360, 100, 100), 
            ], 
            mode: SKShaderTileMode.Clamp
        ),
    };

#region
    private void SetSaturationValue(float x, float y)
    {
        x = Math.Clamp(x - 8, 0, 224);
        y = Math.Clamp(y - 8, 0, 134);

        Saturation = x / 224 * 100;
        Value = 100 - y / 134 * 100;

        BorderColorPreview.Background = new SolidColorBrush(Color);
    }

    private void SetHue(float x)
    {
        x = Math.Clamp(x - 8, 0, 224);

        Hue = x / 224 * 360;

        BorderColorPreview.Background = new SolidColorBrush(Color);
    }
#endregion 
    
#region UI Event Delegates
    private async void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        try
        {
            await Task.Delay(1);

            if (Application.Current != null)
            {
                if (Application.Current.TryGetResource("SeparatorPrimary", Application.Current.ActualThemeVariant, out object? resource2) && resource2 is SolidColorBrush brush2)
                {
                    outlinePaint.Color = new(brush2.Color.R, brush2.Color.G, brush2.Color.B, brush2.Color.A);
                }
            }
        }
        catch (Exception)
        {
            // classic error pink
            outlinePaint.Color = new(0xFF, 0x00, 0xFF, 0xFF);
        }
    }
    
    private void CanvasSaturationValue_OnRenderAction(SKCanvas canvas)
    {
        SKColor[] saturationColors = [new(0xFFFFFFFF), SKColor.FromHsv(Hue, 100, 100)];
        SKShader saturationGradient = SKShader.CreateLinearGradient(new(0, 0), new(220, 0), saturationColors, SKShaderTileMode.Clamp);

        SKColor[] valueColors = [new(0xFFFFFFFF), new(0xFF000000)];
        SKShader valueGradient = SKShader.CreateLinearGradient(new(0, 0), new(0, 150), valueColors, SKShaderTileMode.Clamp);

        SKShader svGradient = SKShader.CreateBlend(SKBlendMode.Modulate, saturationGradient, valueGradient);
        svPaint.Shader = svGradient;

        const int width = 240;
        const int height = 150;
        const int margin = 8;
        
        const int left = margin;
        const int right = width - margin;
        const int top = margin;
        const int bottom = height - margin;
        
        canvas.DrawRoundRect(left - 1, top - 1, right - left + 2, bottom - top + 2, 3, 3, outlinePaint);
        canvas.DrawRoundRect(left, top, right - left, bottom - top, 2, 2, svPaint);

        float x = SaturnMath.Lerp(left, right, Saturation / 100.0f);
        float y = SaturnMath.Lerp(bottom, top, Value / 100.0f);
        
        canvas.DrawCircle(x, y, 8, outlinePaint);
        canvas.DrawCircle(x, y, 7.8f, selectorPaint);
        canvas.DrawCircle(x, y, 4, outlinePaint);
        canvas.DrawCircle(x, y, 3.8f, svPaint);
    }

    private void CanvasHue_OnRenderAction(SKCanvas canvas)
    {
        const int width = 240;
        const int height = 22;
        const int margin = 8;
        
        const int left = margin;
        const int right = width - margin;
        const int top = margin;
        const int bottom = height - margin;
        const int center = (top + bottom) / 2;
        
        canvas.DrawRoundRect(left - 1, top - 1, right - left + 2, bottom - top + 2, 3, 3, outlinePaint);
        canvas.DrawRoundRect(left, top, right - left, bottom - top, 2, 2, huePaint);

        float x = SaturnMath.Lerp(left, right, Hue / 360.0f);
        
        canvas.DrawCircle(x, center, 8, outlinePaint);
        canvas.DrawCircle(x, center, 7.8f, selectorPaint);
        canvas.DrawCircle(x, center, 4, outlinePaint);
        canvas.DrawCircle(x, center, 3.8f, huePaint);
    }
    
    private void CanvasSaturationValue_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.Properties.IsLeftButtonPressed) return;
        
        Point p = e.GetPosition(CanvasSaturationValue);
        SetSaturationValue((float)p.X, (float)p.Y);
    }

    private void CanvasSaturationValue_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!e.Properties.IsLeftButtonPressed) return;
        
        Point p = e.GetPosition(CanvasSaturationValue);
        SetSaturationValue((float)p.X, (float)p.Y);
    }

    private void CanvasHue_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.Properties.IsLeftButtonPressed) return;
        
        Point p = e.GetPosition(CanvasHue);
        SetHue((float)p.X);
    }

    private void CanvasHue_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!e.Properties.IsLeftButtonPressed) return;
        
        Point p = e.GetPosition(CanvasHue);
        SetHue((float)p.X);
    }
    
    private void FlyoutBase_OnClosed(object? sender, EventArgs e) => ColorPickFinished?.Invoke(null, EventArgs.Empty);
#endregion UI Event Delegates
}