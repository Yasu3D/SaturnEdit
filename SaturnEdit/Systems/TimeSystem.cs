using System;
using Avalonia.Threading;
using SaturnData.Notation.Core;

namespace SaturnEdit.Systems;

public static class TimeSystem
{
    static TimeSystem()
    {
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

    public static readonly DispatcherTimer UpdateTimer = new(TimeSpan.FromMilliseconds(1000.0f / SettingsSystem.RenderSettings.RefreshRate), DispatcherPriority.Render, UpdateTimer_OnTick);
    private static float tickInterval;
    
    public static event EventHandler? PlaybackStateChanged;
    public static event EventHandler? PlaybackSpeedChanged;
    public static event EventHandler? TimestampChanged;
    public static event EventHandler? DivisionChanged;
    
    public const int DefaultDivision = 8;
    
    /// <summary>
    /// The current playback state of the "playhead".
    /// </summary>
    public static bool PlaybackState
    {
        get => playbackState;
        set
        {
            if (playbackState == value) return;
            
            playbackState = value;
            PlaybackStateChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static bool playbackState;

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
        set
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
    private static float audioTime = 0;

    private const float deltaMultiplier = 0.001f;
    private const float forceAlignDelta = 50.0f;
    
    private static void OnSettingsChanged(object? sender, EventArgs e)
    {
        tickInterval = 1000.0f / SettingsSystem.RenderSettings.RefreshRate;
        UpdateTimer.Interval = TimeSpan.FromMilliseconds(tickInterval);
    }

    private static void UpdateTimer_OnTick(object? sender, EventArgs eventArgs)
    {
        // TODO: Handle playback state changes
        // TODO: NAudio doesn't report time accurately enough.
        
        // Handle keeping UpdateTimer and AudioTimer in-sync.
        //if (PlaybackState == PlaybackState.Stopped) return;
        
        /*
        if (AudioSystem.OutputDevice.PlaybackState != NAudio.Wave.PlaybackState.Playing || AudioSystem.AudioFile == null)
        {
            // AudioSystem isn't playing audio, or there's no loaded audio.
            // Continue, but synchronise the AudioTimer to the UpdateTimer since there's no audio to rely on.
            Timestamp = Timestamp.TimestampFromTime(ChartSystem.Chart, Timestamp.Time + tickInterval, Division);
            audioTime = Timestamp.Time;

            timeScale = PlaybackSpeed / 100.0f;
        }
        else
        {
            // AudioSystem is playing audio.
            // Synchronise the UpdateTimer to the AudioTimer to make sure they don't drift apart.
            float time = Timestamp.Time + tickInterval * timeScale;
            //audioTime = ((float)AudioSystem.AudioFile.Position / AudioSystem.AudioFile.Length) * (float)AudioSystem.AudioFile.TotalTime.TotalMilliseconds;

            float delta = time - audioTime;
            if (Math.Abs(delta) >= forceAlignDelta || timeScale == 0)
            {
                time = audioTime;
                Console.WriteLine("SNAP!");
            }

            Timestamp = Timestamp.TimestampFromTime(ChartSystem.Chart, time, Division);
            timeScale = (PlaybackSpeed / 100.0f) - (delta * deltaMultiplier);
        }
        */
        
        //Console.WriteLine($"{Timestamp.Measure} {Timestamp.Tick} | {AudioSystem.AudioFile?.Position} | {Timestamp.Time} {audioTime} | {timeScale}");
        
        //Console.WriteLine($"{Timestamp.Measure} {Timestamp.Tick} | {Timestamp.Time}");
    }
}