using System;
using System.Threading;
using SaturnData.Notation.Core;
using SaturnData.Notation.Events;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;

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
        ChartSystem.EntryChanged += OnEntryChanged;
        OnEntryChanged(null, EventArgs.Empty);

        PlaybackStateChanged += OnPlaybackStateChanged;
        OnPlaybackStateChanged(null, EventArgs.Empty);
    }
    
    public static event EventHandler? UpdateTick;
    public static event EventHandler? TimestampChanged;
    public static event EventHandler? TimestampSeeked;
    public static event EventHandler? PlaybackStateChanged;
    public static event EventHandler? PlaybackSpeedChanged;
    public static event EventHandler? DivisionChanged;
    public static event EventHandler? LoopChanged;
    
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
            timestamp = value;
            TimestampChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static Timestamp timestamp = Timestamp.Zero;

    /// <summary>
    /// <see cref="Timestamp"/> offset by <see cref="SaturnData.Notation.Core.Entry.AudioOffset"/>
    /// </summary>
    public static float AudioTime => Timestamp.Time - ChartSystem.Entry.AudioOffset;

    /// <summary>
    /// <see cref="Timestamp"/> with compensation for the sound card <see cref="AudioSystem.Latency"/>
    /// </summary>
    public static float HitsoundTime => Timestamp.Time + HitsoundOffset;
    public static float HitsoundOffset => PlaybackSpeed / 100.0f * AudioSystem.Latency * 2;
    
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

    /// <summary>
    /// The beginning of the playback loop.
    /// </summary>
    public static float LoopStart
    {
        get => loopStart;
        set
        {
            float val = Math.Max(value, 0);
            if (loopStart == val) return;

            loopStart = val;
            LoopChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static float loopStart = -1;
    
    /// <summary>
    /// The end of the playback loop.
    /// </summary>
    public static float LoopEnd
    {
        get => loopEnd;
        set
        {
            if (loopEnd == value) return;

            loopEnd = value;
            LoopChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static float loopEnd = -1;
    
    public const float TickInterval = 1000.0f / 120.0f;
    public const int DefaultDivision = 8;
    
    public static readonly Timer UpdateTimer = new(UpdateTimer_OnTick, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(TickInterval));

#region Methods
    public static void SeekMeasureTick(int measure, int tick)
    {
        SeekFullTick(measure * 1920 + tick);
    }
    
    public static void SeekFullTick(int fullTick)
    {
        Timestamp t = new(fullTick);
        t.Time = Timestamp.TimeFromTimestamp(ChartSystem.Chart, t);

        Timestamp = t;
        TimestampSeeked?.Invoke(null, EventArgs.Empty);
    }

    public static void SeekTime(float time, int div)
    {
        Timestamp t = Timestamp.TimestampFromTime(ChartSystem.Chart, time, div);

        Timestamp = t;
        TimestampSeeked?.Invoke(null, EventArgs.Empty);
    }
    
    public static void Quantize(AudioSettings.QuantizationOption quantizationOption)
    {
        float time = Timestamp.Time;
        float currentBeatTime = Timestamp.TimeFromTimestamp(ChartSystem.Chart, Timestamp);
        float nextBeatTime = Timestamp.TimeFromTimestamp(ChartSystem.Chart, Timestamp + DivisionInterval);

        float currentBeatDelta = Math.Abs(time - currentBeatTime);
        float nextBeatDelta = Math.Abs(time - nextBeatTime);
        
        switch (quantizationOption)
        {
            case AudioSettings.QuantizationOption.Off: break;

            case AudioSettings.QuantizationOption.Nearest:
            {
                if (currentBeatDelta < nextBeatDelta)
                {
                    SeekTime(currentBeatTime, Division);
                }
                else
                {
                    SeekTime(nextBeatTime, Division);
                }
                
                break;
            }

            case AudioSettings.QuantizationOption.Previous:
            {
                SeekTime(currentBeatTime, Division);
                
                break;
            }

            case AudioSettings.QuantizationOption.Next:
            {
                if (currentBeatDelta != 0)
                {
                    SeekTime(nextBeatTime, Division);
                } 
                
                break;
            }
        }
    }
    
    public static void Navigate_MoveBeatForward()
    {
        SeekFullTick(Timestamp.FullTick + DivisionInterval);
    }
    
    public static void Navigate_MoveBeatBack()
    {
        SeekFullTick(Math.Max(0, Timestamp.FullTick - DivisionInterval));
    }
    
    public static void Navigate_MoveMeasureForward()
    {
        SeekFullTick(Timestamp.FullTick + 1920);
    }
    
    public static void Navigate_MoveMeasureBack()
    {
        SeekFullTick(Math.Max(0, Timestamp.FullTick - 1920));
    }
    
    public static void Navigate_JumpToNextObject()
    {
        int startTick = Timestamp.FullTick;
        int nextTick = int.MaxValue;

        if (EditorSystem.Mode == EditorMode.ObjectMode)
        {
            foreach (Bookmark bookmark in ChartSystem.Chart.Bookmarks)
            {
                if (bookmark.Timestamp.FullTick <= startTick) continue;
                if (bookmark.Timestamp.FullTick >= nextTick) continue;

                nextTick = bookmark.Timestamp.FullTick;
            }

            foreach (Event globalEvent in ChartSystem.Chart.Events)
            {
                if (globalEvent.Timestamp.FullTick <= startTick) continue;
                if (globalEvent.Timestamp.FullTick >= nextTick) continue;

                nextTick = globalEvent.Timestamp.FullTick;
            }

            foreach (Note laneToggle in ChartSystem.Chart.LaneToggles)
            {
                if (laneToggle.Timestamp.FullTick <= startTick) continue;
                if (laneToggle.Timestamp.FullTick >= nextTick) continue;

                nextTick = laneToggle.Timestamp.FullTick;
            }

            foreach (Layer layer in ChartSystem.Chart.Layers)
            {
                foreach (Event layerEvent in layer.Events)
                {
                    if (layerEvent.Timestamp.FullTick <= startTick) continue;
                    if (layerEvent.Timestamp.FullTick >= nextTick) continue;

                    nextTick = layerEvent.Timestamp.FullTick;
                }

                foreach (Note note in layer.Notes)
                {
                    if (note.Timestamp.FullTick <= startTick) continue;
                    if (note.Timestamp.FullTick >= nextTick) continue;

                    nextTick = note.Timestamp.FullTick;
                }
            }
        }
        else if (EditorSystem.Mode == EditorMode.EditMode)
        {
            if (EditorSystem.ActiveObjectGroup is HoldNote holdNote)
            {
                foreach (HoldPointNote point in holdNote.Points)
                {
                    if (point.Timestamp.FullTick <= startTick) continue;
                    if (point.Timestamp.FullTick >= nextTick) continue;

                    nextTick = point.Timestamp.FullTick;
                }
            }
            else if (EditorSystem.ActiveObjectGroup is StopEffectEvent stopEffectEvent)
            {
                foreach (EffectSubEvent subEvent in stopEffectEvent.SubEvents)
                {
                    if (subEvent.Timestamp.FullTick <= startTick) continue;
                    if (subEvent.Timestamp.FullTick >= nextTick) continue;

                    nextTick = subEvent.Timestamp.FullTick;
                }
            }
            else if (EditorSystem.ActiveObjectGroup is ReverseEffectEvent reverseEffectEvent)
            {
                foreach (EffectSubEvent subEvent in reverseEffectEvent.SubEvents)
                {
                    if (subEvent.Timestamp.FullTick <= startTick) continue;
                    if (subEvent.Timestamp.FullTick >= nextTick) continue;

                    nextTick = subEvent.Timestamp.FullTick;
                }
            }
        }

        if (nextTick == int.MaxValue) return;
        
        SeekFullTick(nextTick);
    }
    
    public static void Navigate_JumpToPreviousObject() 
    {
        int startTick = Timestamp.FullTick;
        int previousTick = int.MinValue;

        if (EditorSystem.Mode == EditorMode.ObjectMode)
        {
            foreach (Bookmark bookmark in ChartSystem.Chart.Bookmarks)
            {
                if (bookmark.Timestamp.FullTick >= startTick) continue;
                if (bookmark.Timestamp.FullTick <= previousTick) continue;

                previousTick = bookmark.Timestamp.FullTick;
            }

            foreach (Event globalEvent in ChartSystem.Chart.Events)
            {
                if (globalEvent.Timestamp.FullTick >= startTick) continue;
                if (globalEvent.Timestamp.FullTick <= previousTick) continue;

                previousTick = globalEvent.Timestamp.FullTick;
            }

            foreach (Note laneToggle in ChartSystem.Chart.LaneToggles)
            {
                if (laneToggle.Timestamp.FullTick >= startTick) continue;
                if (laneToggle.Timestamp.FullTick <= previousTick) continue;

                previousTick = laneToggle.Timestamp.FullTick;
            }

            foreach (Layer layer in ChartSystem.Chart.Layers)
            {
                foreach (Event layerEvent in layer.Events)
                {
                    if (layerEvent.Timestamp.FullTick >= startTick) continue;
                    if (layerEvent.Timestamp.FullTick <= previousTick) continue;

                    previousTick = layerEvent.Timestamp.FullTick;
                }

                foreach (Note note in layer.Notes)
                {
                    if (note.Timestamp.FullTick >= startTick) continue;
                    if (note.Timestamp.FullTick <= previousTick) continue;

                    previousTick = note.Timestamp.FullTick;
                }
            }
        }
        else if (EditorSystem.Mode == EditorMode.EditMode)
        {
            if (EditorSystem.ActiveObjectGroup is HoldNote holdNote)
            {
                foreach (HoldPointNote point in holdNote.Points)
                {
                    if (point.Timestamp.FullTick >= startTick) continue;
                    if (point.Timestamp.FullTick <= previousTick) continue;

                    previousTick = point.Timestamp.FullTick;
                }
            }
            else if (EditorSystem.ActiveObjectGroup is StopEffectEvent stopEffectEvent)
            {
                foreach (EffectSubEvent subEvent in stopEffectEvent.SubEvents)
                {
                    if (subEvent.Timestamp.FullTick >= startTick) continue;
                    if (subEvent.Timestamp.FullTick <= previousTick) continue;

                    previousTick = subEvent.Timestamp.FullTick;
                }
            }
            else if (EditorSystem.ActiveObjectGroup is ReverseEffectEvent reverseEffectEvent)
            {
                foreach (EffectSubEvent subEvent in reverseEffectEvent.SubEvents)
                {
                    if (subEvent.Timestamp.FullTick >= startTick) continue;
                    if (subEvent.Timestamp.FullTick <= previousTick) continue;

                    previousTick = subEvent.Timestamp.FullTick;
                }
            }
        }

        if (previousTick < 0) return;
        
        SeekFullTick(previousTick);
    }
    
    public static void Navigate_JumpToNextSelection()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        
        int startTick = Timestamp.FullTick;
        int nextTick = int.MaxValue;

        foreach (ITimeable obj in SelectionSystem.SelectedObjects)
        {
            if (obj.Timestamp.FullTick <= startTick) continue;
            if (obj.Timestamp.FullTick >= nextTick) continue;

            nextTick = obj.Timestamp.FullTick;
        }
        
        if (nextTick == int.MaxValue) return;
        
        SeekFullTick(nextTick);
    }
    
    public static void Navigate_JumpToPreviousSelection() 
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;
        
        int startTick = Timestamp.FullTick;
        int previousTick = int.MinValue;

        foreach (ITimeable obj in SelectionSystem.SelectedObjects)
        {
            if (obj.Timestamp.FullTick >= startTick) continue;
            if (obj.Timestamp.FullTick <= previousTick) continue;

            previousTick = obj.Timestamp.FullTick;
        }
        
        if (previousTick < 0) return;
        
        SeekFullTick(previousTick);
    }
    
    public static void Navigate_IncreaseBeatDivision()
    {
        Division = Math.Clamp(Division + 1, 1, 1920);
    }
    
    public static void Navigate_DecreaseBeatDivision() 
    {
        Division = Math.Clamp(Division - 1, 1, 1920);
    }
    
    public static void Navigate_DoubleBeatDivision() 
    {
        Division = Math.Clamp(Division * 2, 1, 1920);
    }
    
    public static void Navigate_HalveBeatDivision() 
    {
        Division = Math.Clamp(Division / 2, 1, 1920);
    }
#endregion Methods

#region System Event Delegates
    private static void OnEntryChanged(object? sender, EventArgs e)
    {
        LoopEnd = Math.Min(LoopEnd, ChartSystem.Entry.ChartEnd.Time);
    }

    private static void OnPlaybackStateChanged(object? sender, EventArgs e) => Quantize(SettingsSystem.AudioSettings.QuantizedPause);
#endregion System Event Delegates

#region Internal Event Delegates
    private static void UpdateTimer_OnTick(object? sender)
    {
        UpdateTick?.Invoke(null, EventArgs.Empty);
        
        // Stop playback and seek to preview begin when preview end is reached.
        if (PlaybackState == PlaybackState.Preview && Timestamp.Time > ChartSystem.Entry.PreviewEnd.Time)
        {
            PlaybackState = PlaybackState.Stopped;
            SeekTime(ChartSystem.Entry.PreviewBegin.Time, Division);
        }
        
        // Seek to PreviewBegin when playback state is preview
        if (PlaybackState == PlaybackState.Preview && Timestamp.Time < ChartSystem.Entry.PreviewBegin.Time)
        {
            SeekTime(ChartSystem.Entry.PreviewBegin.Time, Division);
        }
  
        // Looping
        if (PlaybackState == PlaybackState.Playing && SettingsSystem.AudioSettings.LoopPlayback && LoopStart != -1 && LoopEnd != -1)
        {
            // LoopStart before LoopEnd
            if (LoopStart < LoopEnd)
            {
                if (Timestamp.Time < LoopStart || Timestamp.Time > LoopEnd)
                {
                    SeekTime(LoopStart + 0.1f, Division); // HACKY: +0.1ms to prevent infinite loops.
                }
            }
            
            // LoopEnd before LoopStart
            if (LoopEnd < LoopStart)
            {
                if (Timestamp.Time > ChartSystem.Entry.ChartEnd.Time)
                {
                    SeekTime(0.0f, Division);
                }
                
                if (Timestamp.Time > LoopEnd && Timestamp.Time < LoopStart)
                {
                    SeekTime(LoopStart + 0.1f, Division); // HACKY: +0.1ms to prevent infinite loops.
                }
            }
        }
        else // No explicit looping
        {
            // Stop playback and seek to begin when chart end is reached.
            if (PlaybackState is PlaybackState.Playing or PlaybackState.Preview && Timestamp.Time > ChartSystem.Entry.ChartEnd.Time)
            {
                PlaybackState = PlaybackState.Stopped;

                if (SettingsSystem.AudioSettings.LoopToStart)
                {
                    SeekTime(0.0f, Division);
                }
            }
        }
        
        // No need to handle timers if playback is stopped.
        if (PlaybackState == PlaybackState.Stopped) return;
        
        // Handle keeping UpdateTimer and AudioTimer in-sync.
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
            Timestamp = Timestamp.TimestampFromTime(ChartSystem.Chart, (float)AudioSystem.AudioChannelAudio.Position + ChartSystem.Entry.AudioOffset, Division);
        }
    }
#endregion Internal Event Delegates
}