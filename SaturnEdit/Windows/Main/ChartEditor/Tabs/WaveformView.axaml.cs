using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using SaturnEdit.Audio;
using SaturnEdit.Systems;
using SaturnView;
using SkiaSharp;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class WaveformView : UserControl
{
    public WaveformView()
    {
        InitializeComponent();
        
        SizeChanged += Control_OnSizeChanged;
        ActualThemeVariantChanged += Control_OnActualThemeVariantChanged;
        
        AudioSystem.AudioLoaded += OnAudioLoaded;
        OnAudioLoaded(null, EventArgs.Empty);
    }
    
    private readonly CanvasInfo canvasInfo = new();
    private static float[]? waveform = null;

    private static SKColor waveformColor = new();
    private static SKColor judgeLineColor = new();
    private static SKColor measureLineColor = new();
    private static SKColor beatLineColor = new();

#region System Event Handlers
    private static void OnAudioLoaded(object? sender, EventArgs eventArgs)
    {
        waveform = AudioChannel.GetWaveformData(ChartSystem.Entry.AudioPath, 4000);
    }
#endregion System Event Handlers
    
#region UI Event Handlers
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        SizeChanged -= Control_OnSizeChanged;
        ActualThemeVariantChanged -= Control_OnActualThemeVariantChanged;
        AudioSystem.AudioLoaded -= OnAudioLoaded;
        
        base.OnUnloaded(e);
    }
    
    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        RenderCanvas.Width = Bounds.Width;
        RenderCanvas.Height = Bounds.Height;

        canvasInfo.Width = (float)RenderCanvas.Width;
        canvasInfo.Height = (float)RenderCanvas.Height;
        canvasInfo.Radius = canvasInfo.Width / 2;
        canvasInfo.Center = new(canvasInfo.Radius, canvasInfo.Radius);
    }

    private async void Control_OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        try
        {
            await Task.Delay(1);

            if (Application.Current == null) return;
            
            if (Application.Current.TryGetResource("BackgroundSecondary", Application.Current.ActualThemeVariant, out object? resource1))
            {
                if (resource1 is SolidColorBrush brush)
                {
                    canvasInfo.BackgroundColor = new(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
                }
            }
            
            if (Application.Current.TryGetResource("WaveformWave", Application.Current.ActualThemeVariant, out object? resource2))
            {
                if (resource2 is SolidColorBrush brush)
                {
                    waveformColor = new(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
                }
            }
            
            if (Application.Current.TryGetResource("WaveformJudgeLine", Application.Current.ActualThemeVariant, out object? resource3))
            {
                if (resource3 is SolidColorBrush brush)
                {
                    judgeLineColor = new(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
                }
            }
            
            if (Application.Current.TryGetResource("WaveformMeasureLine", Application.Current.ActualThemeVariant, out object? resource4))
            {
                if (resource4 is SolidColorBrush brush)
                {
                    measureLineColor = new(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
                }
            }
            
            if (Application.Current.TryGetResource("WaveformBeatLine", Application.Current.ActualThemeVariant, out object? resource5))
            {
                if (resource5 is SolidColorBrush brush)
                {
                    beatLineColor = new(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
                }
            }
        }
        catch (Exception)
        {
            // classic error pink
            canvasInfo.BackgroundColor = new(0xFF00FFFF);
            waveformColor = new(0xFF00FFFF);
            judgeLineColor = new(0xFF00FFFF);
            measureLineColor = new(0xFF00FFFF);
            beatLineColor = new(0xFF00FFFF);
        }
    }

    private void RenderCanvas_OnRenderAction(SKCanvas canvas)
    {
        RendererWaveform.RenderWaveform(
            canvas: canvas,
            canvasInfo: canvasInfo,
            settings: SettingsSystem.RenderSettings,
            waveformColor: waveformColor,
            judgelineColor: judgeLineColor,
            measureLineColor: measureLineColor,
            beatLineColor: beatLineColor,
            chart: ChartSystem.Chart,
            waveform: waveform,
            audioOffset: ChartSystem.Entry.AudioOffset,
            audioLength: (float?)AudioSystem.AudioChannelAudio?.Length ?? 0,
            time: TimeSystem.Timestamp.Time
        );
    }
#endregion UI Event Handlers
}