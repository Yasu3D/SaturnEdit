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
    private static float tickInterval;
    
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

    private static float timeScale = 1.0f;

    private const float DeltaMultiplier = 0.01f;
    private const float ForceAlignDelta = 50.0f;
    
    private static void OnSettingsChanged(object? sender, EventArgs e)
    {
        tickInterval = 1000.0f / SettingsSystem.RenderSettings.RefreshRate;
        UpdateTimer.Interval = TimeSpan.FromMilliseconds(tickInterval);
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
            // Continue, but synchronise the AudioTimer to the UpdateTimer since there's no audio to rely on.
            Timestamp = Timestamp.TimestampFromTime(ChartSystem.Chart, Timestamp.Time + tickInterval, Division);
            if (AudioSystem.AudioChannelAudio != null) AudioSystem.AudioChannelAudio.Position = Timestamp.Time;

            timeScale = PlaybackSpeed / 100.0f;
        }
        else
        {
            // AudioSystem is playing audio.
            // Synchronise the UpdateTimer to the AudioTimer to make sure they don't drift apart.
            float time = Timestamp.Time + tickInterval * timeScale;

            float delta = time - (float)AudioSystem.AudioChannelAudio.Position;
            if (Math.Abs(delta) >= ForceAlignDelta || timeScale == 0)
            {
                time = (float)AudioSystem.AudioChannelAudio.Position;
            }

            Timestamp = Timestamp.TimestampFromTime(ChartSystem.Chart, time, Division);
            timeScale = (PlaybackSpeed / 100.0f) - (delta * DeltaMultiplier);
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