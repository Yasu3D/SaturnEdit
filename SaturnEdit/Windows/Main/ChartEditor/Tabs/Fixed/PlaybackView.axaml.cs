using System;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using FluentIcons.Common;
using SaturnEdit.Audio;
using SaturnEdit.Systems;
using SaturnView;
using SkiaSharp;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class PlaybackView : UserControl
{
    public PlaybackView()
    {
        InitializeComponent();
        
        ActualThemeVariantChanged += OnActualThemeVariantChanged;
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        TimeSystem.TimestampChanged += OnTimestampChanged;
        OnTimestampChanged(null, EventArgs.Empty);

        TimeSystem.PlaybackStateChanged += OnPlaybackStateChanged;
        OnPlaybackStateChanged(null, EventArgs.Empty);

        TimeSystem.PlaybackSpeedChanged += OnPlaybackSpeedChanged;
        OnPlaybackSpeedChanged(null, EventArgs.Empty);

        TimeSystem.LoopChanged += OnLoopChanged;
        OnLoopChanged(null, EventArgs.Empty);

        ChartSystem.ChartChanged += OnEntryChanged;
        ChartSystem.EntryChanged += OnChartChanged;
        AudioSystem.AudioLoaded += OnAudioLoaded;
        RecalculateSeekSlider();
        
        SizeChanged += OnSizeChanged;
        OnSizeChanged(null, new(null));
    }

    private bool blockEvents;

    private readonly CanvasInfo canvasInfo = new();
    private SKColor clearColor;
    private SKColor waveformColor;
    private float sliderMaximum = 0;

    private float[]? waveform = null;
    
    private async void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        try
        {
            await Task.Delay(1);

            if (Application.Current == null) return;
            if (Application.Current.TryGetResource("BackgroundPrimary", Application.Current.ActualThemeVariant, out object? clearColorResource) && clearColorResource is SolidColorBrush clearColorBrush)
            {
                clearColor = new(clearColorBrush.Color.R, clearColorBrush.Color.G, clearColorBrush.Color.B, clearColorBrush.Color.A);
            }
            
            if (Application.Current.TryGetResource("ForegroundPrimary", Application.Current.ActualThemeVariant, out object? waveformColorResource) && waveformColorResource is SolidColorBrush waveformColorBrush)
            {
                waveformColor = new(waveformColorBrush.Color.R, waveformColorBrush.Color.G, waveformColorBrush.Color.B, 0x60);
            }
        }
        catch (Exception ex)
        {
            // classic error pink
            clearColor = new(0xFF, 0x00, 0xFF, 0xFF);
        }
    }

    private void RenderCanvas_OnRenderAction(SKCanvas canvas) => RendererWaveform.RenderSeekSlider(canvas, canvasInfo, clearColor, waveformColor, waveform, ChartSystem.Entry.AudioOffset, (float?)AudioSystem.AudioChannelAudio?.Length ?? 0, sliderMaximum);
    
    private void OnEntryChanged(object? sender, EventArgs e)
    {
        RecalculateSeekSlider();
        UpdateLoopMarkers();
    }

    private void OnChartChanged(object? sender, EventArgs e)
    {
        RecalculateSeekSlider();
        UpdateLoopMarkers();
    }

    private void OnAudioLoaded(object? sender, EventArgs e)
    {
        RecalculateSeekSlider();
        UpdateLoopMarkers();
        waveform = AudioChannel.GetWaveformData(ChartSystem.Entry.AudioPath);
    }

    private void OnLoopChanged(object? sender, EventArgs e)
    {
        UpdateLoopMarkers();
    }
    
    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            double sliderWidth = SliderPlaybackSpeed.Bounds.Width - 10;

            TickPlaybackSpeed25.Margin  = new(sliderWidth * (( 25.0 - 5) / 295), 0, 0, 0);
            TickPlaybackSpeed50.Margin  = new(sliderWidth * (( 50.0 - 5) / 295), 0, 0, 0);
            TickPlaybackSpeed100.Margin = new(sliderWidth * ((100.0 - 5) / 295), 0, 0, 0);
            TickPlaybackSpeed200.Margin = new(sliderWidth * ((200.0 - 5) / 295), 0, 0, 0);
        });
        
        RenderCanvas.Width = PanelCanvasContainer.Bounds.Width;
        RenderCanvas.Height = PanelCanvasContainer.Bounds.Height;

        canvasInfo.Width = (float)PanelCanvasContainer.Bounds.Width;
        canvasInfo.Height = (float)PanelCanvasContainer.Bounds.Height;
        
        UpdateLoopMarkers();
    }
    
    private void RecalculateSeekSlider()
    {
        sliderMaximum = ChartSystem.Entry.ChartEnd.Time;
        
        blockEvents = true;
        
        SliderSeek.Maximum = sliderMaximum;
        SliderSeek.Value = TimeSystem.Timestamp.Time;
        
        blockEvents = false;
    }
    
    private void UpdateLoopMarkers()
    {
        LoopMarkerStart.IsVisible = SettingsSystem.AudioSettings.LoopPlayback;
        LoopMarkerEnd.IsVisible   = SettingsSystem.AudioSettings.LoopPlayback;
        
        if (!SettingsSystem.AudioSettings.LoopPlayback) return;
        
        double max = sliderMaximum;

        double start = Math.Clamp(TimeSystem.LoopStart / max, 0, 1);
        start = start * SliderSeek.Bounds.Width - LoopMarkerEnd.Width * start;
        
        double end   = Math.Clamp(TimeSystem.LoopEnd   / max, 0, 1);
        end = end * SliderSeek.Bounds.Width - LoopMarkerEnd.Width * end;

        LoopMarkerStart.Margin = new(start, 0, 0, 0);
        LoopMarkerEnd.Margin   = new(end,   0, 0, 0);
    }
    
    private void OnTimestampChanged(object? sender, EventArgs e)
    {
        blockEvents = true;
        
        SliderSeek.Value = TimeSystem.Timestamp.Time;
        
        blockEvents = false;
    }
    
    private void OnPlaybackStateChanged(object? sender, EventArgs e)
    {
        blockEvents = true;

        ToggleButtonPlay.IsChecked = TimeSystem.PlaybackState is PlaybackState.Playing or PlaybackState.Preview;
        
        blockEvents = false;
        
        StackPanelToolTipPause.IsVisible = TimeSystem.PlaybackState is PlaybackState.Playing or PlaybackState.Preview;
        StackPanelToolTipPlay.IsVisible = TimeSystem.PlaybackState is PlaybackState.Stopped;

        IconPlay.Icon = TimeSystem.PlaybackState is PlaybackState.Playing or PlaybackState.Preview ? Icon.Stop : Icon.Play;
    }
    
    private void OnPlaybackSpeedChanged(object? sender, EventArgs e)
    {
        blockEvents = true;
        
        TextBlockPlaybackSpeed.Text = $"{TimeSystem.PlaybackSpeed.ToString(CultureInfo.InvariantCulture)}%";
        SliderPlaybackSpeed.Value = TimeSystem.PlaybackSpeed;
        
        blockEvents = false;
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        TextBlockShortcutPlay.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Playback.Play"].ToString();
        TextBlockShortcutPause.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Playback.Pause"].ToString();
        TextBlockShortcutIncreasePlaybackSpeed.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Playback.IncreasePlaybackSpeed"].ToString();
        TextBlockShortcutDecreasePlaybackSpeed.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Playback.DecreasePlaybackSpeed"].ToString();
        TextBlockShortcutLoop.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Playback.LoopPlayback"].ToString();
        TextBlockShortcutSetLoopStart.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Playback.SetLoopMarkerStart"].ToString();
        TextBlockShortcutSetLoopEnd.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Playback.SetLoopMarkerEnd"].ToString();

        UpdateLoopMarkers();
        
        blockEvents = true;

        ToggleButtonLoop.IsChecked = SettingsSystem.AudioSettings.LoopPlayback;
        ToggleButtonMetronome.IsChecked = SettingsSystem.AudioSettings.Metronome;
        
        blockEvents = false;
    }

    private void ToggleButtonPlay_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        
        bool play = ToggleButtonPlay.IsChecked ?? false;
        TimeSystem.PlaybackState = play ? PlaybackState.Playing : PlaybackState.Stopped;
    }
    
    private void SliderPlaybackSpeed_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not Slider) return;

        TimeSystem.PlaybackSpeed = (int)SliderPlaybackSpeed.Value;
    }
    
    private void SliderSeek_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not Slider) return;

        TimeSystem.Seek((float)SliderSeek.Value, TimeSystem.Division);
    }

    private void ToggleButtonLoop_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;

        SettingsSystem.AudioSettings.LoopPlayback = ToggleButtonLoop.IsChecked ?? false;
    }

    private void ToggleButtonMetronome_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;

        SettingsSystem.AudioSettings.Metronome = ToggleButtonMetronome.IsChecked ?? false;
    }
    
    private void ButtonLoopStart_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;

        TimeSystem.LoopStart = TimeSystem.Timestamp.Time;
    }

    private void ButtonLoopEnd_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;

        TimeSystem.LoopEnd = TimeSystem.Timestamp.Time;
    }
    
    // TODO: Playback Settings flyout
}