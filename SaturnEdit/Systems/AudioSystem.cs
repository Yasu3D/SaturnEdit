using System;
using System.Collections.Generic;
using System.IO;
using ManagedBass;
using SaturnData.Notation.Core;
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
        
        UndoRedoSystem.ChartBranch.OperationHistoryChanged += ChartBranch_OnOperationHistoryChanged;
        
        SettingsSystem.VolumeChanged += OnVolumeChanged;
        OnVolumeChanged(null, EventArgs.Empty);

        SettingsSystem.HitsoundsChanged += OnHitsoundsChanged;
        OnHitsoundsChanged(null, EventArgs.Empty);

        TimeSystem.TimestampSeeked += OnTimestampSeeked;
        OnTimestampSeeked(null, EventArgs.Empty);
        
        TimeSystem.PlaybackSpeedChanged += OnPlaybackSpeedChanged;
        OnPlaybackSpeedChanged(null, EventArgs.Empty);
        
        TimeSystem.UpdateTick += OnUpdateTick;
    }

    public static event EventHandler? AudioLoaded;
    
    public static AudioChannel? AudioChannelAudio { get; private set; }
    public static AudioChannel? AudioChannelHold { get; private set; }
    public static AudioSample? AudioSampleGuide { get; private set; }
    public static AudioSample? AudioSampleTouch { get; private set; }
    public static AudioSample? AudioSampleSlide { get; private set; }
    public static AudioSample? AudioSampleBonus { get; private set; }
    public static AudioSample? AudioSampleR { get; private set; }
    public static AudioSample? AudioSampleStartClick { get; private set; }
    public static AudioSample? AudioSampleMetronome { get; private set; }
    
    public static float Latency { get; private set; } = 0;
    
    private static readonly HashSet<Note> PassedNotes = [];
    private static readonly HashSet<Note> PassedBonusSlides = [];
    private static readonly HashSet<Note> ActiveHoldNotes = [];
    
    private static float holdLoopVolumeMultiplier = 1;

#region Methods
    private static void TriggerHitsounds()
    {
        lock (PassedNotes) lock (PassedBonusSlides) lock (ActiveHoldNotes) lock (ChartSystem.Chart) lock (ChartSystem.Entry)
        {
            if (TimeSystem.PlaybackState == PlaybackState.Stopped)
            {
                AudioSampleGuide?.Pause();
                AudioSampleTouch?.Pause();
                AudioSampleSlide?.Pause();
                AudioSampleBonus?.Pause();
                AudioSampleR?.Pause();
                AudioSampleStartClick?.Pause();
                AudioSampleMetronome?.Pause();

                if (AudioChannelHold != null)
                {
                    AudioChannelHold.Playing = false;
                    AudioChannelHold.Position = 0;
                }
                
                return;
            }
            
            // Notes
            ActiveHoldNotes.Clear();

            lock (ChartSystem.Chart.Layers)
            {
                foreach (Layer l in ChartSystem.Chart.Layers)
                {
                    lock (l.GeneratedNotes)
                    {
                        foreach (Note note in l.GeneratedNotes)
                        {
                            if (note is not MeasureLineNote) continue;

                            if (PassedNotes.Contains(note)) continue;
                            
                            if (note.Timestamp.Time < TimeSystem.HitsoundTime)
                            {
                                PassedNotes.Add(note);

                                if (note.Timestamp.FullTick < 1920 && TimeSystem.Timestamp.Measure < 1)
                                {
                                    AudioSampleStartClick?.Play();
                                }
                                else
                                {
                                    if (SettingsSystem.AudioSettings.Metronome)
                                    {
                                        AudioSampleMetronome?.Play();
                                    }
                                }
                            }
                        }
                    }

                    lock (l.Notes)
                    {
                        foreach (Note n in l.Notes)
                        {
                            Note note = n;

                            if (note is HoldNote holdNote && holdNote.Points.Count > 1)
                            {
                                if (holdNote.Points[0].Timestamp.Time < TimeSystem.HitsoundTime && holdNote.Points[^1].Timestamp.Time > TimeSystem.HitsoundTime)
                                {
                                    ActiveHoldNotes.Add(holdNote);
                                }

                                // Hold notes need to play hitsounds for both the hold start and hold end notes.
                                // Always check if the end of the hold note is passed before discarding the note entirely.
                                if (PassedNotes.Contains(holdNote))
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
                            }

                            if (note is MeasureLineNote or SyncNote) continue;
                            if (note is IPlayable playable && playable.JudgementType == JudgementType.Fake) continue;

                            // Bonus effects on slide notes play hitsounds after a fancy animation. Account for the delay here before discarding passed notes.
                            if (note is SlideClockwiseNote or SlideCounterclockwiseNote && note is IPlayable playableSlide && playableSlide.BonusType == BonusType.Bonus && !PassedBonusSlides.Contains(note))
                            {
                                float bpm = ChartSystem.Chart.LastTempoChange(note.Timestamp.Time)?.Tempo ?? 120;
                                int effectOffset = bpm > 200 ? 3840 : 1920;

                                float bonusEffectTime = Timestamp.TimeFromTimestamp(ChartSystem.Chart, note.Timestamp + effectOffset);

                                if (bonusEffectTime < TimeSystem.HitsoundTime)
                                {
                                    PassedBonusSlides.Add(note);

                                    AudioSampleBonus?.Play();
                                }
                            }

                            if (PassedNotes.Contains(note)) continue;

                            if (note.Timestamp.Time < TimeSystem.HitsoundTime)
                            {
                                PassedNotes.Add(note);

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
                }
            }

            // Update Hold Loop Audio
            if (AudioChannelHold != null)
            {
                if (ActiveHoldNotes.Count == 0)
                {
                    // Fade out audio when no hold is active.
                    if (AudioChannelHold.Volume > 0)
                    {
                        holdLoopVolumeMultiplier = Math.Max(0, holdLoopVolumeMultiplier - TimeSystem.UpdateInterval * 0.004f);
                    }
                    else
                    {
                        // Then stop playback once the sound fades out completely.
                        AudioChannelHold.Playing = false;
                        AudioChannelHold.Position = 0;
                    }
                }
                else
                {
                    if (AudioChannelHold.Position > SettingsSystem.AudioSettings.HoldLoopEnd)
                    {
                        AudioChannelHold.Playing = true;
                        AudioChannelHold.Position -= SettingsSystem.AudioSettings.HoldLoopEnd - SettingsSystem.AudioSettings.HoldLoopStart;
                    }

                    if (!AudioChannelHold.Playing || holdLoopVolumeMultiplier < 1)
                    {
                        AudioChannelHold.Position = 0;
                        AudioChannelHold.Playing = true;
                    }
                    
                    holdLoopVolumeMultiplier = 1;
                }
                
                AudioChannelHold.Volume = DecibelToVolume(SettingsSystem.AudioSettings.MasterVolume) * DecibelToVolume(SettingsSystem.AudioSettings.HoldVolume) * holdLoopVolumeMultiplier;
            }
        }
    }

    private static void RefreshHitsounds()
    {
        lock (PassedNotes) lock (PassedBonusSlides) lock (ActiveHoldNotes)
        {
            PassedNotes.Clear();
            PassedBonusSlides.Clear();
            ActiveHoldNotes.Clear();
            
            foreach (Layer layer in ChartSystem.Chart.Layers)
            {
                foreach (Note note in layer.GeneratedNotes)
                {
                    if (note is not MeasureLineNote measureLineNote) continue;
                    
                    if (measureLineNote.Timestamp.Time < TimeSystem.Timestamp.Time)
                    {
                        PassedNotes.Add(measureLineNote);
                    }
                }
                
                foreach (Note note in layer.Notes)
                {
                    if (note.Timestamp.Time < TimeSystem.Timestamp.Time)
                    {
                        PassedNotes.Add(note);
                    }

                    if (note is HoldNote holdNote && holdNote.Points.Count > 1)
                    {
                        if (holdNote.Points[^1].Timestamp.Time < TimeSystem.Timestamp.Time)
                        {
                            PassedNotes.Add(holdNote.Points[^1]);
                        }

                        if (holdNote.Points[0].Timestamp.Time < TimeSystem.Timestamp.Time && holdNote.Points[^1].Timestamp.Time > TimeSystem.Timestamp.Time)
                        {
                            ActiveHoldNotes.Add(holdNote);
                        }
                    }

                    if (note is SlideClockwiseNote or SlideCounterclockwiseNote && note is IPlayable playable && playable.BonusType == BonusType.Bonus)
                    {
                        float bpm = ChartSystem.Chart.LastTempoChange(note.Timestamp.Time)?.Tempo ?? 120;
                        float effectOffset = bpm >= 200
                            ? 480000 / bpm
                            : 240000 / bpm;

                        float bonusEffectTime = note.Timestamp.Time + effectOffset;

                        if (bonusEffectTime < TimeSystem.Timestamp.Time)
                        {
                            PassedBonusSlides.Add(note);
                        }
                    }
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
#endregion Methods
    
#region System Event Handlers
    public static void OnClosed(object? sender, EventArgs e)
    {
        Bass.Free();
    }

    private static void OnAudioChanged(object? sender, EventArgs e)
    {
        if (AudioChannelAudio != null)
        {
            AudioChannelAudio.Playing = false;
        }
        
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
        AudioSampleSlide?.Pause();
        AudioSampleBonus?.Pause();
        AudioSampleR?.Pause();
        AudioSampleStartClick?.Pause();
        AudioSampleMetronome?.Pause();

        if (AudioChannelHold != null)
        {
            AudioChannelHold.Playing = false;
            AudioChannelHold.Position = 0;
        }
            
        try
        {
            AudioSampleGuide      = File.Exists(SettingsSystem.AudioSettings.HitsoundGuidePath)      ? new(SettingsSystem.AudioSettings.HitsoundGuidePath)      : null;
            AudioSampleTouch      = File.Exists(SettingsSystem.AudioSettings.HitsoundTouchPath)      ? new(SettingsSystem.AudioSettings.HitsoundTouchPath)      : null;
            AudioSampleSlide      = File.Exists(SettingsSystem.AudioSettings.HitsoundSlidePath)      ? new(SettingsSystem.AudioSettings.HitsoundSlidePath)      : null;
            AudioSampleBonus      = File.Exists(SettingsSystem.AudioSettings.HitsoundBonusPath)      ? new(SettingsSystem.AudioSettings.HitsoundBonusPath)      : null;
            AudioSampleR          = File.Exists(SettingsSystem.AudioSettings.HitsoundRPath)          ? new(SettingsSystem.AudioSettings.HitsoundRPath)          : null;
            AudioSampleStartClick = File.Exists(SettingsSystem.AudioSettings.HitsoundStartClickPath) ? new(SettingsSystem.AudioSettings.HitsoundStartClickPath) : null;
            AudioSampleMetronome  = File.Exists(SettingsSystem.AudioSettings.HitsoundMetronomePath)  ? new(SettingsSystem.AudioSettings.HitsoundMetronomePath)  : null;
            
            AudioChannelHold  = File.Exists(SettingsSystem.AudioSettings.HitsoundHoldPath)   ? new(SettingsSystem.AudioSettings.HitsoundHoldPath)   : null;
            
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
        double hitsoundVolume = masterVolume * DecibelToVolume(SettingsSystem.AudioSettings.HitsoundVolume);
        
        if (AudioChannelAudio     != null) AudioChannelAudio.Volume     = masterVolume   * DecibelToVolume(SettingsSystem.AudioSettings.AudioVolume);
        if (AudioSampleGuide      != null) AudioSampleGuide.Volume      = hitsoundVolume * DecibelToVolume(SettingsSystem.AudioSettings.GuideVolume);
        if (AudioSampleTouch      != null) AudioSampleTouch.Volume      = hitsoundVolume * DecibelToVolume(SettingsSystem.AudioSettings.TouchVolume);
        if (AudioChannelHold      != null) AudioChannelHold.Volume      = hitsoundVolume * DecibelToVolume(SettingsSystem.AudioSettings.HoldVolume) * holdLoopVolumeMultiplier;
        if (AudioSampleSlide      != null) AudioSampleSlide.Volume      = hitsoundVolume * DecibelToVolume(SettingsSystem.AudioSettings.SlideVolume);
        if (AudioSampleBonus      != null) AudioSampleBonus.Volume      = hitsoundVolume * DecibelToVolume(SettingsSystem.AudioSettings.BonusVolume);
        if (AudioSampleR          != null) AudioSampleR.Volume          = hitsoundVolume * DecibelToVolume(SettingsSystem.AudioSettings.RVolume);
        if (AudioSampleStartClick != null) AudioSampleStartClick.Volume = hitsoundVolume * DecibelToVolume(SettingsSystem.AudioSettings.StartClickVolume);
        if (AudioSampleMetronome  != null) AudioSampleMetronome.Volume  = hitsoundVolume * DecibelToVolume(SettingsSystem.AudioSettings.MetronomeVolume);
    }

    private static void OnEntryChanged(object? sender, EventArgs e)
    {
        if (AudioChannelAudio == null) return;
        AudioChannelAudio.Position = TimeSystem.AudioTime;
    }

    private static void ChartBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
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
    
    private static void OnUpdateTick(object? sender, EventArgs e)
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
            bool afterAudio = TimeSystem.AudioTime > AudioChannelAudio.Length - 100; // -100ms because Bass loves to end playback and loop it early...
            
            AudioChannelAudio.Playing = !beforeAudio && !afterAudio;
        }
    }
#endregion System Event Handlers
}