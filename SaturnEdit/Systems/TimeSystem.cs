using System;
using System.ComponentModel.DataAnnotations;
using Avalonia.Threading;
using SaturnData.Notation.Core;

namespace SaturnEdit.Systems;

public enum PlaybackState
{
    Stopped = 0,
    Playing = 1,
    Preview = 2,
}

public static class TimeSystem
{
    public static void Initialize()
    {
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    } 

    public static readonly DispatcherTimer UpdateTimer = new(TimeSpan.FromMilliseconds(1000.0f / SettingsSystem.RenderSettings.RefreshRate), DispatcherPriority.Render, UpdateTimer_OnTick);
    public static float TickInterval { get; private set; }
    
    public static event EventHandler? TimestampChanged;
    public static event EventHandler? TimestampSeeked;
    
    public static event EventHandler? PlaybackStateChanged;
    public static event EventHandler? PlaybackSpeedChanged;
    public static event EventHandler? DivisionChanged;
    
    public const int DefaultDivision = 8;
    
    /// <summary>
    /// The current playback state of the playhead.
    /// </summary>
    public static PlaybackState PlaybackState
    {
        get => playbackState;
        set
        {
            if (playbackState == value) return;
            
            playbackState = value;
            PlaybackStateChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static PlaybackState playbackState;

    public static int PlaybackSpeed
    {
        get => playbackSpeed;
        set
        {
            if (playbackSpeed == value) return;
            
            playbackSpeed = value;
            PlaybackSpeedChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static int playbackSpeed = 100;
    
    /// <summary>
    /// The current timestamp of the "playhead" in measures and ticks.
    /// </summary>
    public static Timestamp Timestamp
    {
        get => timestamp;
        private set
        {
            if (timestamp == value) return;
            
            timestamp = value;
            TimestampChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static Timestamp timestamp;

    /// <summary>
    /// <see cref="Timestamp"/> offset by <see cref="SaturnData.Notation.Core.Entry.AudioOffset"/>
    /// </summary>
    public static float AudioTime => Timestamp.Time + ChartSystem.Entry.AudioOffset;

    /// <summary>
    /// <see cref="Timestamp"/> with compensation for the sound card <see cref="AudioSystem.Latency"/>
    /// </summary>
    public static float HitsoundTime => Timestamp.Time + AudioSystem.Latency + MagicOffset;
    private const float MagicOffset = 50;
    
    /// <summary>
    /// The current beat division to snap to.
    /// </summary>
    public static int Division
    {
        get => division;
        set
        {
            if (division == value) return;
            
            division = Math.Clamp(value, 1, 1920);
            DivisionChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static int division = DefaultDivision;

    /// <summary>
    /// The number of ticks between each beat.
    /// </summary>
    public static int DivisionInterval => 1920 / Math.Max(1, Division);
    
    private static void OnSettingsChanged(object? sender, EventArgs e)
    {
        TickInterval = 1000.0f / SettingsSystem.RenderSettings.RefreshRate;
        UpdateTimer.Interval = TimeSpan.FromMilliseconds(TickInterval);
    }

    private static void UpdateTimer_OnTick(object? sender, EventArgs eventArgs)
    {
        // Stop playback and seek to begin when chart end is reached.
        if (Timestamp.Time > ChartSystem.Entry.ChartEnd.Time)
        {
            PlaybackState = PlaybackState.Stopped;
            Seek(0.0f, Division);
        }

        // Stop playback and seek to preview begin when preview end is reached.
        if (PlaybackState == PlaybackState.Preview && Timestamp.Time > ChartSystem.Entry.PreviewBegin + ChartSystem.Entry.PreviewLength)
        {
            PlaybackState = PlaybackState.Stopped;
            Seek(ChartSystem.Entry.PreviewBegin, Division);
        }
        
        // Seek to PreviewBegin when playback state is preview
        if (PlaybackState == PlaybackState.Preview && Timestamp.Time < ChartSystem.Entry.PreviewBegin)
        {
            Seek(ChartSystem.Entry.PreviewBegin, Division);
        }
        
        // Handle keeping UpdateTimer and AudioTimer in-sync.
        if (PlaybackState == PlaybackState.Stopped) return;
        
        if (AudioSystem.AudioChannelAudio == null || !AudioSystem.AudioChannelAudio.Playing)
        {
            // AudioSystem isn't playing audio, or there's no loaded audio.
            // Keep counting up with UpdateTimer, and synchronise the AudioTimer to it.
            Timestamp = Timestamp.TimestampFromTime(ChartSystem.Chart, Timestamp.Time + TickInterval * PlaybackSpeed * 0.01f, Division);
            if (AudioSystem.AudioChannelAudio != null) AudioSystem.AudioChannelAudio.Position = AudioTime;
        }
        else
        {
            // AudioSystem is playing audio.
            // ManagedBass is absolutely cracked at keeping time, so just sync directly to that.
            Timestamp = Timestamp.TimestampFromTime(ChartSystem.Chart, (float)AudioSystem.AudioChannelAudio.Position, Division);
        }
    }

    public static void Seek(int measure, int tick)
    {
        Timestamp t = new(measure, tick);
        t.Time = Timestamp.TimeFromTimestamp(ChartSystem.Chart, t);

        Timestamp = t;
        TimestampSeeked?.Invoke(null, EventArgs.Empty);
    }

    public static void Seek(float time, int div)
    {
        Timestamp t = Timestamp.TimestampFromTime(ChartSystem.Chart, time, div);

        Timestamp = t;
        TimestampSeeked?.Invoke(null, EventArgs.Empty);
    }
}