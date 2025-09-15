using System;
using System.IO;
using ManagedBass;
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
        latency = info.Latency;
        
        ChartSystem.AudioChanged += OnAudioChanged;
        OnAudioChanged(null, EventArgs.Empty);

        SettingsSystem.AudioSettings.VolumeChanged += OnVolumeChanged;
        OnVolumeChanged(null, EventArgs.Empty);

        TimeSystem.TimestampSeeked += OnTimestampSeeked;
        OnTimestampSeeked(null, EventArgs.Empty);
        
        TimeSystem.PlaybackSpeedChanged += OnPlaybackSpeedChanged;
        OnPlaybackSpeedChanged(null, EventArgs.Empty);
        
        TimeSystem.UpdateTimer.Tick += UpdateTimer_OnTick;
    }

    public static event EventHandler? AudioLoaded;
    
    public static AudioChannel? AudioChannelAudio;
    
    private static AudioChannel? audioChannelGuide;
    private static AudioChannel? audioChannelTouch;
    private static AudioChannel? audioChannelChain;
    private static AudioChannel? audioChannelHold;
    private static AudioChannel? audioChannelHoldLoop;
    private static AudioChannel? audioChannelSlide;
    private static AudioChannel? audioChannelSnap;
    private static AudioChannel? audioChannelBonus;
    private static AudioChannel? audioChannelR;
    private static AudioChannel? audioChannelStartClick;
    private static AudioChannel? audioChannelMetronome;
    
    private static float latency = 0;
    private static float NormalizeVolume(float decibel) => (decibel + 60.0f) / 60.0f;
    
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
        if (AudioChannelAudio      != null) AudioChannelAudio.Volume      = NormalizeVolume(SettingsSystem.AudioSettings.AudioVolume);
        if (audioChannelGuide      != null) audioChannelGuide.Volume      = NormalizeVolume(SettingsSystem.AudioSettings.GuideVolume);
        if (audioChannelTouch      != null) audioChannelTouch.Volume      = NormalizeVolume(SettingsSystem.AudioSettings.TouchVolume);
        if (audioChannelChain      != null) audioChannelChain.Volume      = NormalizeVolume(SettingsSystem.AudioSettings.ChainVolume);
        if (audioChannelHold       != null) audioChannelHold.Volume       = NormalizeVolume(SettingsSystem.AudioSettings.HoldVolume);
        if (audioChannelHoldLoop   != null) audioChannelHoldLoop.Volume   = NormalizeVolume(SettingsSystem.AudioSettings.HoldLoopVolume);
        if (audioChannelSlide      != null) audioChannelSlide.Volume      = NormalizeVolume(SettingsSystem.AudioSettings.SlideVolume);
        if (audioChannelSnap       != null) audioChannelSnap.Volume       = NormalizeVolume(SettingsSystem.AudioSettings.SnapVolume);
        if (audioChannelBonus      != null) audioChannelBonus.Volume      = NormalizeVolume(SettingsSystem.AudioSettings.BonusVolume);
        if (audioChannelR          != null) audioChannelR.Volume          = NormalizeVolume(SettingsSystem.AudioSettings.RVolume);
        if (audioChannelStartClick != null) audioChannelStartClick.Volume = NormalizeVolume(SettingsSystem.AudioSettings.StartClickVolume);
        if (audioChannelMetronome  != null) audioChannelMetronome.Volume  = NormalizeVolume(SettingsSystem.AudioSettings.MetronomeVolume);
    }
    
    private static void OnTimestampSeeked(object? sender, EventArgs e)
    {
        if (AudioChannelAudio == null) return;
        AudioChannelAudio.Position = TimeSystem.Timestamp.Time;
    }
    
    private static void OnPlaybackSpeedChanged(object? sender, EventArgs e)
    {
        if (AudioChannelAudio == null) return;
        AudioChannelAudio.Speed = TimeSystem.PlaybackSpeed;
    }
    
    private static void UpdateTimer_OnTick(object? sender, EventArgs e)
    {
        if (AudioChannelAudio == null) return;

        // Pause audio if playback is stopped.
        if (TimeSystem.PlaybackState is PlaybackState.Stopped)
        {
            AudioChannelAudio.Playing = false;
        }
        
        // Play audio if time is inside playback range. Pause otherwise.
        if (TimeSystem.PlaybackState is PlaybackState.Playing or PlaybackState.Preview)
        {
            bool beforeAudio = TimeSystem.Timestamp.Time < 0;
            bool afterAudio = TimeSystem.Timestamp.Time > AudioChannelAudio.Length;

            AudioChannelAudio.Playing = !beforeAudio && !afterAudio;
        }
    }
}