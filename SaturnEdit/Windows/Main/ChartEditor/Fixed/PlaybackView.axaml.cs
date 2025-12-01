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
using SaturnData.Notation.Core;
using SaturnEdit.Audio;
using SaturnEdit.Controls;
using SaturnEdit.Systems;
using SaturnView;
using SkiaSharp;

namespace SaturnEdit.Windows.Main.ChartEditor.Fixed;

public partial class PlaybackView : UserControl
{
    public PlaybackView()
    {
        InitializeComponent();
        
        SliderSeek.AddHandler(PointerReleasedEvent, SliderSeek_OnPointerReleased, RoutingStrategies.Tunnel);
        
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

        ChartSystem.EntryChanged += OnEntryChanged;
        AudioSystem.AudioLoaded += OnAudioLoaded;
        OnAudioLoaded(null, EventArgs.Empty);
        
        UndoRedoSystem.ChartBranch.OperationHistoryChanged += ChartBranch_OnOperationHistoryChanged;
        
        ActualThemeVariantChanged += Control_OnActualThemeVariantChanged;
        
        SizeChanged += Control_OnSizeChanged;
        Control_OnSizeChanged(null, new(null));
    }

    private readonly CanvasInfo canvasInfo = new();
    private SKColor waveformColor;
    private float[]? waveform = null;
    private float sliderMaximum = 0;
    
    private bool blockEvents;
    
#region Methods
    private void UpdateSeekSlider()
    {
        sliderMaximum = ChartSystem.Entry.ChartEnd.Time;
        
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            SliderSeek.Maximum = sliderMaximum;
            SliderSeek.Value = TimeSystem.Timestamp.Time;
            
            blockEvents = false;
        });
    }
    
    private void UpdateLoopMarkers()
    {
        Dispatcher.UIThread.Post(() =>
        {
            LoopMarkerStart.IsVisible = TimeSystem.LoopStart != -1 && SettingsSystem.AudioSettings.LoopPlayback;
            LoopMarkerEnd.IsVisible   = TimeSystem.LoopEnd   != -1 && SettingsSystem.AudioSettings.LoopPlayback;
            
            if (!SettingsSystem.AudioSettings.LoopPlayback || (TimeSystem.LoopStart == -1 && TimeSystem.LoopEnd == -1)) return;

            if (TimeSystem.LoopStart != -1)
            {
                double start = Math.Clamp(TimeSystem.LoopStart / sliderMaximum, 0, 1);
                start = start * SliderSeek.Bounds.Width - LoopMarkerEnd.Width * start;
                LoopMarkerStart.Margin = new(start, 0, 0, 0);
            }

            if (TimeSystem.LoopEnd != -1)
            {
                double end = Math.Clamp(TimeSystem.LoopEnd / sliderMaximum, 0, 1);
                end = end * SliderSeek.Bounds.Width - LoopMarkerEnd.Width * end;
                LoopMarkerEnd.Margin = new(end, 0, 0, 0);
            }
        });
    }

    private void UpdateBookmarks()
    {
        Dispatcher.UIThread.Post(() =>
        {
            for (int i = 0; i < ChartSystem.Chart.Bookmarks.Count; i++)
            {
                Bookmark bookmark = ChartSystem.Chart.Bookmarks[i];
                
                if (i < PanelBookmarks.Children.Count)
                {
                    // Modify existing item.
                    if (PanelBookmarks.Children[i] is not BookmarkMarker marker) continue;

                    marker.SetBookmark(bookmark, SliderSeek.Bounds.Width, sliderMaximum);
                }
                else
                {
                    // Create new item.
                    BookmarkMarker marker = new();
                    marker.SetBookmark(bookmark, SliderSeek.Bounds.Width, sliderMaximum);

                    marker.Click += BookmarkMarker_OnClick;
                    
                    PanelBookmarks.Children.Add(marker);
                }
            }
            
            // Delete redundant items.
            for (int i =PanelBookmarks.Children.Count - 1; i >= ChartSystem.Chart.Bookmarks.Count; i--)
            {
                if (PanelBookmarks.Children[i] is not BookmarkMarker marker) continue;

                marker.Click -= BookmarkMarker_OnClick;
                
                PanelBookmarks.Children.Remove(marker);
            }
        });
    }
#endregion Methods

#region System Event Handlers
    private void OnEntryChanged(object? sender, EventArgs e)
    {
        UpdateSeekSlider();
        UpdateLoopMarkers();
        UpdateBookmarks();
    }

    private void ChartBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        UpdateSeekSlider();
        UpdateLoopMarkers();
        UpdateBookmarks();
    }

    private void OnAudioLoaded(object? sender, EventArgs e)
    {
        UpdateSeekSlider();
        UpdateLoopMarkers();
        UpdateBookmarks();
        waveform = AudioChannel.GetWaveformData(ChartSystem.Entry.AudioPath);
    }

    private void OnLoopChanged(object? sender, EventArgs e)
    {
        UpdateLoopMarkers();
    }
    
    private void OnTimestampChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            SliderSeek.Value = TimeSystem.Timestamp.Time;
            
            blockEvents = false;
        });
    }
    
    private void OnPlaybackStateChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            ToggleButtonPlay.IsChecked = TimeSystem.PlaybackState is PlaybackState.Playing or PlaybackState.Preview;
            
            blockEvents = false;
            
            StackPanelToolTipPause.IsVisible = TimeSystem.PlaybackState is PlaybackState.Playing or PlaybackState.Preview;
            StackPanelToolTipPlay.IsVisible = TimeSystem.PlaybackState is PlaybackState.Stopped;

            IconPlay.Icon = TimeSystem.PlaybackState is PlaybackState.Playing or PlaybackState.Preview ? Icon.Stop : Icon.Play;
        });
    }
    
    private void OnPlaybackSpeedChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            TextBlockPlaybackSpeed.Text = $"{TimeSystem.PlaybackSpeed.ToString(CultureInfo.InvariantCulture)}%";
            SliderPlaybackSpeed.Value = TimeSystem.PlaybackSpeed;
            
            blockEvents = false;
        });
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
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

            MenuItemQuantizedPauseOff.IsChecked = SettingsSystem.AudioSettings.QuantizedPause == AudioSettings.QuantizationOption.Off;
            MenuItemQuantizedPauseNearest.IsChecked = SettingsSystem.AudioSettings.QuantizedPause == AudioSettings.QuantizationOption.Nearest;
            MenuItemQuantizedPausePrevious.IsChecked = SettingsSystem.AudioSettings.QuantizedPause == AudioSettings.QuantizationOption.Previous;
            MenuItemQuantizedPauseNext.IsChecked = SettingsSystem.AudioSettings.QuantizedPause == AudioSettings.QuantizationOption.Next;
            
            MenuItemQuantizedSeekOff.IsChecked = SettingsSystem.AudioSettings.QuantizedSeek == AudioSettings.QuantizationOption.Off;
            MenuItemQuantizedSeekNearest.IsChecked = SettingsSystem.AudioSettings.QuantizedSeek == AudioSettings.QuantizationOption.Nearest;
            MenuItemQuantizedSeekPrevious.IsChecked = SettingsSystem.AudioSettings.QuantizedSeek == AudioSettings.QuantizationOption.Previous;
            MenuItemQuantizedSeekNext.IsChecked = SettingsSystem.AudioSettings.QuantizedSeek == AudioSettings.QuantizationOption.Next;
            
            MenuItemLoopToStart.IsChecked = SettingsSystem.AudioSettings.LoopToStart;
            
            blockEvents = false;
        });
    }
#endregion System Event Handlers

#region UI Event Handlers
    private async void Control_OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        try
        {
            await Task.Delay(1);

            if (Application.Current == null) return;
            if (Application.Current.TryGetResource("BackgroundPrimary", Application.Current.ActualThemeVariant, out object? clearColorResource) && clearColorResource is SolidColorBrush clearColorBrush)
            {
                canvasInfo.BackgroundColor = new(clearColorBrush.Color.R, clearColorBrush.Color.G, clearColorBrush.Color.B, clearColorBrush.Color.A);
            }
            
            if (Application.Current.TryGetResource("ForegroundPrimary", Application.Current.ActualThemeVariant, out object? waveformColorResource) && waveformColorResource is SolidColorBrush waveformColorBrush)
            {
                waveformColor = new(waveformColorBrush.Color.R, waveformColorBrush.Color.G, waveformColorBrush.Color.B, 0x60);
            }
        }
        catch (Exception ex)
        {
            // classic error pink
            canvasInfo.BackgroundColor = new(0xFF, 0x00, 0xFF, 0xFF);
            
            Console.WriteLine(ex);
        }
    }
    
    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
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
        UpdateBookmarks();
    }
    
    private void RenderCanvas_OnRenderAction(SKCanvas canvas)
    {
        RendererWaveform.RenderSeekSlider
        (
            canvas: canvas,
            canvasInfo: canvasInfo,
            waveformColor: waveformColor,
            waveform: waveform,
            audioOffset: ChartSystem.Entry.AudioOffset,
            audioLength: (float?)AudioSystem.AudioChannelAudio?.Length ?? 0,
            sliderLength: sliderMaximum
        );
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
        
        TimeSystem.SeekTime((float)SliderSeek.Value, TimeSystem.Division);
    }
    
    private void SliderSeek_OnPointerReleased(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        
        TimeSystem.Quantize(SettingsSystem.AudioSettings.QuantizedSeek);
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
    
    private void MenuItemQuantizedPause_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not MenuItem item) return;

        SettingsSystem.AudioSettings.QuantizedPause = item.Name switch
        {
            "MenuItemQuantizedPauseOff" => AudioSettings.QuantizationOption.Off,
            "MenuItemQuantizedPauseNearest" => AudioSettings.QuantizationOption.Nearest,
            "MenuItemQuantizedPausePrevious" => AudioSettings.QuantizationOption.Previous,
            "MenuItemQuantizedPauseNext" => AudioSettings.QuantizationOption.Next,
            _ => AudioSettings.QuantizationOption.Off,
        };
    }
    
    private void MenuItemQuantizedSeek_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not MenuItem item) return;

        SettingsSystem.AudioSettings.QuantizedSeek = item.Name switch
        {
            "MenuItemQuantizedSeekOff" => AudioSettings.QuantizationOption.Off,
            "MenuItemQuantizedSeekNearest" => AudioSettings.QuantizationOption.Nearest,
            "MenuItemQuantizedSeekPrevious" => AudioSettings.QuantizationOption.Previous,
            "MenuItemQuantizedSeekNext" => AudioSettings.QuantizationOption.Next,
            _ => AudioSettings.QuantizationOption.Off,
        };
    }

    private void MenuItemLoopToStart_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not MenuItem item) return;

        SettingsSystem.AudioSettings.LoopToStart = item.IsChecked;
    }

    private void BookmarkMarker_OnClick(object? sender, EventArgs e)
    {
        if (sender is not BookmarkMarker marker) return;
        if (marker.Bookmark == null) return;

        TimeSystem.SeekTime(marker.Bookmark.Timestamp.Time, TimeSystem.Division);
    }
#endregion UI Event Handlers
}