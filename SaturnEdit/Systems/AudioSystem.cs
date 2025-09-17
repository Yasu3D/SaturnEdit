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
        Latency = info.Latency;
        
        ChartSystem.AudioChanged += OnAudioChanged;
        OnAudioChanged(null, EventArgs.Empty);

        ChartSystem.EntryChanged += OnEntryChanged;

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
    public static AudioChannel? AudioChannelGuide { get; private set; }
    public static AudioChannel? AudioChannelTouch { get; private set; }
    public static AudioChannel? AudioChannelChain { get; private set; }
    public static AudioChannel? AudioChannelHold { get; private set; }
    public static AudioChannel? AudioChannelHoldLoop { get; private set; }
    public static AudioChannel? AudioChannelSlide { get; private set; }
    public static AudioChannel? AudioChannelSnap { get; private set; }
    public static AudioChannel? AudioChannelBonus { get; private set; }
    public static AudioChannel? AudioChannelR { get; private set; }
    public static AudioChannel? AudioChannelStartClick { get; private set; }
    public static AudioChannel? AudioChannelMetronome { get; private set; }

    public static float Latency { get; private set; } = 0;

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
        if (AudioChannelGuide != null) AudioChannelGuide.Playing = false;
        if (AudioChannelTouch != null) AudioChannelTouch.Playing = false;
        if (AudioChannelChain != null) AudioChannelChain.Playing = false;
        if (AudioChannelHold != null) AudioChannelHold.Playing = false;
        if (AudioChannelHoldLoop != null) AudioChannelHoldLoop.Playing = false;
        if (AudioChannelSlide != null) AudioChannelSlide.Playing = false;
        if (AudioChannelSnap != null) AudioChannelSnap.Playing = false;
        if (AudioChannelBonus != null) AudioChannelBonus.Playing = false;
        if (AudioChannelR != null) AudioChannelR.Playing = false;
        if (AudioChannelStartClick != null) AudioChannelStartClick.Playing = false;
        if (AudioChannelMetronome != null) AudioChannelMetronome.Playing = false;
        
        try
        {
            AudioChannelGuide      = File.Exists(SettingsSystem.AudioSettings.HitsoundGuidePath)      ? new(SettingsSystem.AudioSettings.HitsoundGuidePath)      : null;
            AudioChannelTouch      = File.Exists(SettingsSystem.AudioSettings.HitsoundTouchPath)      ? new(SettingsSystem.AudioSettings.HitsoundTouchPath)      : null;
            AudioChannelChain      = File.Exists(SettingsSystem.AudioSettings.HitsoundChainPath)      ? new(SettingsSystem.AudioSettings.HitsoundChainPath)      : null;
            AudioChannelHold       = File.Exists(SettingsSystem.AudioSettings.HitsoundHoldPath)       ? new(SettingsSystem.AudioSettings.HitsoundHoldPath)       : null;
            AudioChannelHoldLoop   = File.Exists(SettingsSystem.AudioSettings.HitsoundHoldLoopPath)   ? new(SettingsSystem.AudioSettings.HitsoundHoldLoopPath)   : null;
            AudioChannelSlide      = File.Exists(SettingsSystem.AudioSettings.HitsoundSlidePath)      ? new(SettingsSystem.AudioSettings.HitsoundSlidePath)      : null;
            AudioChannelSnap       = File.Exists(SettingsSystem.AudioSettings.HitsoundSnapPath)       ? new(SettingsSystem.AudioSettings.HitsoundSnapPath)       : null;
            AudioChannelBonus      = File.Exists(SettingsSystem.AudioSettings.HitsoundBonusPath)      ? new(SettingsSystem.AudioSettings.HitsoundBonusPath)      : null;
            AudioChannelR          = File.Exists(SettingsSystem.AudioSettings.HitsoundRPath)          ? new(SettingsSystem.AudioSettings.HitsoundRPath)          : null;
            AudioChannelStartClick = File.Exists(SettingsSystem.AudioSettings.HitsoundStartClickPath) ? new(SettingsSystem.AudioSettings.HitsoundStartClickPath) : null;
            AudioChannelMetronome  = File.Exists(SettingsSystem.AudioSettings.HitsoundMetronomePath)  ? new(SettingsSystem.AudioSettings.HitsoundMetronomePath)  : null;
            
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
        double masterVolume = AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.MasterVolume);
        
        if (AudioChannelAudio      != null) AudioChannelAudio.Volume      = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteAudio      ? 0 : masterVolume * AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.AudioVolume);
        if (AudioChannelGuide      != null) AudioChannelGuide.Volume      = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteGuide      ? 0 : masterVolume * AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.GuideVolume);
        if (AudioChannelTouch      != null) AudioChannelTouch.Volume      = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteTouch      ? 0 : masterVolume * AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.TouchVolume);
        if (AudioChannelChain      != null) AudioChannelChain.Volume      = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteChain      ? 0 : masterVolume * AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.ChainVolume);
        if (AudioChannelHold       != null) AudioChannelHold.Volume       = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteHold       ? 0 : masterVolume * AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.HoldVolume);
        if (AudioChannelHoldLoop   != null) AudioChannelHoldLoop.Volume   = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteHoldLoop   ? 0 : masterVolume * AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.HoldLoopVolume);
        if (AudioChannelSlide      != null) AudioChannelSlide.Volume      = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteSlide      ? 0 : masterVolume * AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.SlideVolume);
        if (AudioChannelSnap       != null) AudioChannelSnap.Volume       = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteSnap       ? 0 : masterVolume * AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.SnapVolume);
        if (AudioChannelBonus      != null) AudioChannelBonus.Volume      = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteBonus      ? 0 : masterVolume * AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.BonusVolume);
        if (AudioChannelR          != null) AudioChannelR.Volume          = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteR          ? 0 : masterVolume * AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.RVolume);
        if (AudioChannelStartClick != null) AudioChannelStartClick.Volume = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteStartClick ? 0 : masterVolume * AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.StartClickVolume);
        if (AudioChannelMetronome  != null) AudioChannelMetronome.Volume  = SettingsSystem.AudioSettings.MuteMaster || SettingsSystem.AudioSettings.MuteMetronome  ? 0 : masterVolume * AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.MetronomeVolume);
    }

    private static void OnEntryChanged(object? sender, EventArgs e) => UpdateAudioPosition();
    
    private static void OnTimestampSeeked(object? sender, EventArgs e) => UpdateAudioPosition();
    
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
            bool beforeAudio = TimeSystem.AudioTime < 0;
            bool afterAudio = TimeSystem.AudioTime > AudioChannelAudio.Length;

            AudioChannelAudio.Playing = !beforeAudio && !afterAudio;
        }
    }

    private static void UpdateAudioPosition()
    {
        if (AudioChannelAudio == null) return;
        AudioChannelAudio.Position = TimeSystem.AudioTime;
    }
}