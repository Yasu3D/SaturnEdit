using System;
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

        
        
        
        
        
        TimeSystem.UpdateTimer.Tick += UpdateTimer_OnTick;
        
        TimeSystem.PlaybackSpeedChanged += OnPlaybackSpeedChanged;
        OnPlaybackSpeedChanged(null, EventArgs.Empty);
        
        TimeSystem.TimestampChanged += OnTimestampChanged;
        OnTimestampChanged(null, EventArgs.Empty);
    }

    private static float latency = 0;
    
    private static float NormalizeVolume(float decibel) => (decibel + 60.0f) / 60.0f;
    
    private static AudioChannel? audioChannelAudio;
    
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
    
    public static void OnClosed(object? sender, EventArgs e)
    {
        Bass.Free();
    }

    private static void OnAudioChanged(object? sender, EventArgs e)
    {
        Console.WriteLine("AudioChanged");
        
        if (audioChannelAudio != null) audioChannelAudio.Playing = false;
        
        try
        {
            audioChannelAudio = new(ChartSystem.Entry.AudioPath)
            {
                Playing = true,
            };

            //TimeSystem.PlaybackState = false;
            //OnSettingsChanged(null, EventArgs.Empty);
            //OnPlaybackSpeedChanged(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            audioChannelAudio = null;
            
            Console.WriteLine(ex);
        }
    }
    
    private static void OnVolumeChanged(object? sender, EventArgs e)
    {
        if (audioChannelAudio      != null) audioChannelAudio.Volume      = NormalizeVolume(SettingsSystem.AudioSettings.AudioVolume);
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
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    private static void OnPlaybackSpeedChanged(object? sender, EventArgs e)
    {
        if (audioChannelAudio == null) return;
        audioChannelAudio.Speed = TimeSystem.PlaybackSpeed;
    }
    
    private static void OnTimestampChanged(object? sender, EventArgs e)
    {
        
    }
    
    private static void UpdateTimer_OnTick(object? sender, EventArgs e)
    {
        
    }
}