using System;
using System.Collections.Generic;
using System.IO;
using ManagedBass;
using SaturnData.Notation;
using SaturnData.Notation.Core;
using SaturnData.Notation.Events;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnEdit.Audio;

namespace SaturnEdit.Systems;

public static class AudioSystem
{
    public static void Initialize()
    {
        Bass.Init(Flags: DeviceInitFlags.Latency);
        Bass.UpdatePeriod = 20;
        Bass.PlaybackBufferLength = 150;
        
        Bass.GetInfo(out BassInfo info);
        Latency = info.Latency;
        
        ChartSystem.AudioChanged += OnAudioChanged;
        OnAudioChanged(null, EventArgs.Empty);

        ChartSystem.EntryChanged += OnEntryChanged;

        ChartSystem.ChartChanged += OnChartChanged;

        SettingsSystem.AudioSettings.VolumeChanged += OnVolumeChanged;
        OnVolumeChanged(null, EventArgs.Empty);

        SettingsSystem.AudioSettings.HitsoundsChanged += OnHitsoundsChanged;
        OnHitsoundsChanged(null, EventArgs.Empty);

        TimeSystem.TimestampSeeked += OnTimestampSeeked;
        OnTimestampSeeked(null, EventArgs.Empty);
        
        TimeSystem.PlaybackSpeedChanged += OnPlaybackSpeedChanged;
        OnPlaybackSpeedChanged(null, EventArgs.Empty);
        
        TimeSystem.UpdateTimer.Tick += UpdateTimer_OnTick;
    }

    public static event EventHandler? AudioLoaded;
    
    public static AudioChannel? AudioChannelAudio { get; private set; }
    public static AudioSample? AudioSampleGuide { get; private set; }
    public static AudioSample? AudioSampleTouch { get; private set; }
    public static AudioSample? AudioSampleHoldLoop { get; private set; }
    public static AudioSample? AudioSampleSlide { get; private set; }
    public static AudioSample? AudioSampleBonus { get; private set; }
    public static AudioSample? AudioSampleR { get; private set; }
    public static AudioSample? AudioSampleStartClick { get; private set; }
    public static AudioSample? AudioSampleMetronome { get; private set; }

    public static float Latency { get; private set; } = 0;
    private static readonly HashSet<Note> PassedNotes = [];
    private static readonly HashSet<Note> PassedBonusSlides = [];
    private static Timestamp? nextMetronomeClick = Timestamp.Zero;

    public static void OnClosed(object? sender, EventArgs e)
    {
        Bass.Free();
    }

    private static void OnAudioChanged(object? sender, EventArgs e)
    {
        if (AudioChannelAudio != null) AudioChannelAudio.Playing = false;
        
        try
        {
            if (!File.Exists(ChartSystem.Entry.AudioPath))
            {
                AudioChannelAudio = null;
                return;
            }

            AudioChannelAudio = new(ChartSystem.Entry.AudioPath);
            
            OnVolumeChanged(null, EventArgs.Empty);
            OnTimestampSeeked(null, EventArgs.Empty);
            OnPlaybackSpeedChanged(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            AudioChannelAudio = null;
            Console.WriteLine(ex);
        }
        
        AudioLoaded?.Invoke(null, EventArgs.Empty);
    }
    
    private static void OnHitsoundsChanged(object? sender, EventArgs e)
    {
        AudioSampleGuide?.Pause();
        AudioSampleTouch?.Pause();
        AudioSampleHoldLoop?.Pause();
        AudioSampleSlide?.Pause();
        AudioSampleBonus?.Pause();
        AudioSampleR?.Pause();
        AudioSampleStartClick?.Pause();
        AudioSampleMetronome?.Pause();
        
        try
        {
            AudioSampleGuide      = File.Exists(SettingsSystem.AudioSettings.HitsoundGuidePath)      ? new(SettingsSystem.AudioSettings.HitsoundGuidePath)      : null;
            AudioSampleTouch      = File.Exists(SettingsSystem.AudioSettings.HitsoundTouchPath)      ? new(SettingsSystem.AudioSettings.HitsoundTouchPath)      : null;
            AudioSampleHoldLoop   = File.Exists(SettingsSystem.AudioSettings.HitsoundHoldLoopPath)   ? new(SettingsSystem.AudioSettings.HitsoundHoldLoopPath)   : null;
            AudioSampleSlide      = File.Exists(SettingsSystem.AudioSettings.HitsoundSlidePath)      ? new(SettingsSystem.AudioSettings.HitsoundSlidePath)      : null;
            AudioSampleBonus      = File.Exists(SettingsSystem.AudioSettings.HitsoundBonusPath)      ? new(SettingsSystem.AudioSettings.HitsoundBonusPath)      : null;
            AudioSampleR          = File.Exists(SettingsSystem.AudioSettings.HitsoundRPath)          ? new(SettingsSystem.AudioSettings.HitsoundRPath)          : null;
            AudioSampleStartClick = File.Exists(SettingsSystem.AudioSettings.HitsoundStartClickPath) ? new(SettingsSystem.AudioSettings.HitsoundStartClickPath) : null;
            AudioSampleMetronome  = File.Exists(SettingsSystem.AudioSettings.HitsoundMetronomePath)  ? new(SettingsSystem.AudioSettings.HitsoundMetronomePath)  : null;
            
            OnVolumeChanged(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            AudioChannelAudio = null;
            Console.WriteLine(ex);
        }
        
        AudioLoaded?.Invoke(null, EventArgs.Empty);
    }
    
    private static void OnVolumeChanged(object? sender, EventArgs e)
    {
        double masterVolume = DecibelToVolume(SettingsSystem.AudioSettings.MasterVolume);
        
        if (AudioChannelAudio     != null) AudioChannelAudio.Volume     = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteAudio      ? 0 : masterVolume * DecibelToVolume(SettingsSystem.AudioSettings.AudioVolume);
        if (AudioSampleGuide      != null) AudioSampleGuide.Volume      = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteGuide      ? 0 : masterVolume * DecibelToVolume(SettingsSystem.AudioSettings.GuideVolume);
        if (AudioSampleTouch      != null) AudioSampleTouch.Volume      = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteTouch      ? 0 : masterVolume * DecibelToVolume(SettingsSystem.AudioSettings.TouchVolume);
        if (AudioSampleHoldLoop   != null) AudioSampleHoldLoop.Volume   = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteHoldLoop   ? 0 : masterVolume * DecibelToVolume(SettingsSystem.AudioSettings.HoldLoopVolume);
        if (AudioSampleSlide      != null) AudioSampleSlide.Volume      = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteSlide      ? 0 : masterVolume * DecibelToVolume(SettingsSystem.AudioSettings.SlideVolume);
        if (AudioSampleBonus      != null) AudioSampleBonus.Volume      = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteBonus      ? 0 : masterVolume * DecibelToVolume(SettingsSystem.AudioSettings.BonusVolume);
        if (AudioSampleR          != null) AudioSampleR.Volume          = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteR          ? 0 : masterVolume * DecibelToVolume(SettingsSystem.AudioSettings.RVolume);
        if (AudioSampleStartClick != null) AudioSampleStartClick.Volume = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteStartClick ? 0 : masterVolume * DecibelToVolume(SettingsSystem.AudioSettings.StartClickVolume);
        if (AudioSampleMetronome  != null) AudioSampleMetronome.Volume  = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteMetronome  ? 0 : masterVolume * DecibelToVolume(SettingsSystem.AudioSettings.MetronomeVolume);
    }

    private static void OnEntryChanged(object? sender, EventArgs e)
    {
        if (AudioChannelAudio == null) return;
        AudioChannelAudio.Position = TimeSystem.AudioTime;
    }

    private static void OnChartChanged(object? sender, EventArgs e)
    {
        RefreshHitsounds();
    }
    
    private static void OnTimestampSeeked(object? sender, EventArgs e)
    {
        if (AudioChannelAudio != null)
        {
            AudioChannelAudio.Position = TimeSystem.AudioTime;
        }
        
        RefreshHitsounds();
    }
    
    private static void OnPlaybackSpeedChanged(object? sender, EventArgs e)
    {
        if (AudioChannelAudio == null) return;
        AudioChannelAudio.Speed = TimeSystem.PlaybackSpeed;
    }
    
    private static void UpdateTimer_OnTick(object? sender, EventArgs e)
    {
        TriggerHitsounds();
        
        if (AudioChannelAudio == null) return;

        // Pause audio if playback is stopped.
        if (TimeSystem.PlaybackState is PlaybackState.Stopped)
        {
            AudioChannelAudio.Playing = false;
        }
        
        // Play audio if time is inside playback range. Pause otherwise.
        if (TimeSystem.PlaybackState is PlaybackState.Playing or PlaybackState.Preview)
        {
            bool beforeAudio = TimeSystem.AudioTime < 0;
            bool afterAudio = TimeSystem.AudioTime > AudioChannelAudio.Length;

            AudioChannelAudio.Playing = !beforeAudio && !afterAudio;
        }
    }

    private static void TriggerHitsounds()
    {
        if (TimeSystem.PlaybackState == PlaybackState.Stopped)
        {
            AudioSampleGuide?.Pause();
            AudioSampleTouch?.Pause();
            AudioSampleHoldLoop?.Pause();
            AudioSampleSlide?.Pause();
            AudioSampleBonus?.Pause();
            AudioSampleR?.Pause();
            AudioSampleStartClick?.Pause();
            AudioSampleMetronome?.Pause();

            return;
        }
        
        // Metronome clicks
        if (nextMetronomeClick != null && nextMetronomeClick.Value.Time < TimeSystem.HitsoundTime)
        {
            if (AudioSampleStartClick != null && nextMetronomeClick.Value.Measure < 1)
            {
                // Start clicks
                AudioSampleStartClick?.Play();
            }
            else if (SettingsSystem.AudioSettings.Metronome)
            {
                // Constant metronome
                AudioSampleMetronome?.Play();
            }
            
            nextMetronomeClick = GetNextClick(TimeSystem.HitsoundTime);
        }
        
        // Notes
        foreach (Layer l in ChartSystem.Chart.Layers)
        foreach (Note n in l.Notes)
        {
            Note note = n;
            
            // Hold notes need to play hitsounds for both the hold start and hold end notes.
            // Always check if the end of the hold note is passed before discarding the note entirely.
            if (note is HoldNote holdNote && PassedNotes.Contains(holdNote) && holdNote.Points.Count > 1)
            {
                // Hold Start has already passed
                HoldPointNote holdEnd = holdNote.Points[^1];
                
                // Hold End has not passed yet.
                if (!PassedNotes.Contains(holdEnd))
                {
                    // Make Hold End the subject now.
                    note = holdEnd;
                }
            }
            
            if (note is not ITimeable timeable) continue;
            if (note is IPlayable playable && playable.JudgementType == JudgementType.Fake) continue;
            
            // Bonus effects on slide notes play hitsounds after a fancy animation. Account for the delay here before discarding passed notes.
            if (note is SlideClockwiseNote or SlideCounterclockwiseNote && note is IPlayable playableSlide && playableSlide.BonusType == BonusType.Bonus && !PassedBonusSlides.Contains(note))
            {
                float bpm = NotationUtils.LastTempoChange(ChartSystem.Chart, timeable.Timestamp.Time)?.Tempo ?? 120;
                int effectOffset = bpm > 200 ? 3840 : 1920;
                
                float bonusEffectTime = Timestamp.TimeFromTimestamp(ChartSystem.Chart, timeable.Timestamp + effectOffset);
                
                if (bonusEffectTime < TimeSystem.HitsoundTime)
                {
                    PassedBonusSlides.Add(note);

                    if (AudioSampleBonus != null)
                    {
                        AudioSampleBonus.Play();
                    }
                }
            }

            if (PassedNotes.Contains(note)) continue;
            
            if (timeable.Timestamp.Time < TimeSystem.HitsoundTime)
            {
                PassedNotes.Add(note);
                
                Console.WriteLine(note);
                
                // Always play guide sounds.
                AudioSampleGuide?.Play();
                
                // Touch note sounds
                if (note is TouchNote)
                {
                    AudioSampleTouch?.Play();
                }
                
                // Chain note sounds
                if (note is ChainNote)
                {
                    AudioSampleTouch?.Play();
                }
                
                // Hold start/end note sounds
                if (note is HoldNote or HoldPointNote)
                {
                    AudioSampleTouch?.Play();
                }
                
                // Slide note sounds
                if (note is SlideClockwiseNote or SlideCounterclockwiseNote)
                {
                    AudioSampleSlide?.Play();
                }
                
                // Snap note sounds
                if (note is SnapForwardNote or SnapBackwardNote)
                {
                    AudioSampleSlide?.Play();
                }

                // Bonus sounds
                if (note is IPlayable bonus && bonus.BonusType == BonusType.Bonus && note is not (SlideClockwiseNote or SlideCounterclockwiseNote))
                {
                    AudioSampleBonus?.Play();
                }
                
                // R sounds
                if (note is IPlayable r && r.BonusType == BonusType.R)
                {
                    AudioSampleR?.Play();
                }
            }
        }
    }

    private static void RefreshHitsounds()
    {
        nextMetronomeClick = GetNextClick(TimeSystem.Timestamp.Time);
        
        PassedNotes.Clear();
        PassedBonusSlides.Clear();
        
        foreach (Layer layer in ChartSystem.Chart.Layers)
        foreach (Note note in layer.Notes)
        {
            if (note is not ITimeable timeable) continue;
            
            if (timeable.Timestamp.Time < TimeSystem.HitsoundTime)
            {
                PassedNotes.Add(note);
            }

            if (note is HoldNote holdNote && holdNote.Points.Count > 1)
            {
                if (holdNote.Points[^1].Timestamp.Time < TimeSystem.HitsoundTime)
                {
                    PassedNotes.Add(holdNote.Points[^1]);
                }
            }

            if (note is SlideClockwiseNote or SlideCounterclockwiseNote && note is IPlayable playable && playable.BonusType == BonusType.Bonus)
            {
                float bpm = NotationUtils.LastTempoChange(ChartSystem.Chart, timeable.Timestamp.Time)?.Tempo ?? 120;
                int effectOffset = bpm > 200 ? 3840 : 1920;
                
                float bonusEffectTime = Timestamp.TimeFromTimestamp(ChartSystem.Chart, timeable.Timestamp + effectOffset);

                if (bonusEffectTime < TimeSystem.HitsoundTime)
                {
                    PassedBonusSlides.Add(note);
                }
            }
        }
    }
    
    public static double DecibelToVolume(int decibel)
    {
        double normalized = decibel / 60.0 + 1;
        double scaled = 0.1 * Math.Pow(Math.E, 2.4 * normalized) - 0.1;
        
        return scaled;
    }

    private static Timestamp? GetNextClick(float time)
    {
        if (time == 0) return Timestamp.Zero; // hacky but works!
        
        MetreChangeEvent? metre = NotationUtils.LastMetreChange(ChartSystem.Chart, time);
        if (metre == null) return null;
        
        int clicks = metre.Upper;
        int ticks = 1920 / clicks;
        
        Timestamp nextClick = Timestamp.TimestampFromTime(ChartSystem.Chart, time, clicks);
        nextClick += ticks;
        
        nextClick.Time = Timestamp.TimeFromTimestamp(ChartSystem.Chart, nextClick);
        return nextClick;
    }
}