using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FluentIcons.Common;
using SaturnEdit.Systems;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class PlaybackView : UserControl
{
    public PlaybackView()
    {
        InitializeComponent();
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        TimeSystem.TimestampChanged += OnTimestampChanged;
        OnTimestampChanged(null, EventArgs.Empty);

        TimeSystem.PlaybackStateChanged += OnPlaybackStateChanged;
        OnPlaybackStateChanged(null, EventArgs.Empty);

        TimeSystem.PlaybackSpeedChanged += OnPlaybackSpeedChanged;
        OnPlaybackSpeedChanged(null, EventArgs.Empty);

        ChartSystem.ChartChanged += OnEntryOrChartChanged;
        ChartSystem.EntryChanged += OnEntryOrChartChanged;
        OnEntryOrChartChanged(null, EventArgs.Empty);
        
        SizeChanged += OnSizeChanged;
        OnSizeChanged(null, new(null));
    }

    private bool blockEvents;
    
    private void OnEntryOrChartChanged(object? sender, EventArgs e)
    {
        float chartEnd = ChartSystem.Entry.ChartEnd.Time;

        blockEvents = true;
        
        // TODO: Reimplement this
        
        //if (chartEnd > 1)
        //{
        //    SliderSeek.Maximum = chartEnd;
        //    SliderSeek.IsEnabled = true;
        //    SliderPlaybackSpeed.IsEnabled = true;
        //    ToggleButtonPlay.IsEnabled = true;
        //}
        //else
        //{
        //    SliderSeek.Maximum = 1000;
        //    SliderSeek.Value = 0;
        //    SliderSeek.IsEnabled = false;
        //    SliderPlaybackSpeed.IsEnabled = false;
        //    ToggleButtonPlay.IsEnabled = false;
        //    ToggleButtonPlay.IsChecked = false;
        //}
        
        blockEvents = false;
    }
    
    private void OnTimestampChanged(object? sender, EventArgs e)
    {
        blockEvents = true;

        // TODO: Reimplement this
        
        //SliderSeek.Value = TimeSystem.Timestamp.Time;
        
        blockEvents = false;
    }
    
    private void OnPlaybackStateChanged(object? sender, EventArgs e)
    {
        blockEvents = true;

        ToggleButtonPlay.IsChecked = TimeSystem.PlaybackState == PlaybackState.Playing;
        
        blockEvents = false;
        
        StackPanelToolTipPause.IsVisible = TimeSystem.PlaybackState == PlaybackState.Playing;
        StackPanelToolTipPlay.IsVisible = TimeSystem.PlaybackState != PlaybackState.Playing;

        IconPlay.Icon = TimeSystem.PlaybackState == PlaybackState.Playing ? Icon.Stop : Icon.Play;
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
}