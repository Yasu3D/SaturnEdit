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
    public static event EventHandler? MuteChanged;
    
    public static AudioChannel? AudioChannelAudio { get; set; }
    public static AudioChannel? AudioChannelGuide;
    public static AudioChannel? AudioChannelTouch;
    public static AudioChannel? AudioChannelChain;
    public static AudioChannel? AudioChannelHold;
    public static AudioChannel? AudioChannelHoldLoop;
    public static AudioChannel? AudioChannelSlide;
    public static AudioChannel? AudioChannelSnap;
    public static AudioChannel? AudioChannelBonus;
    public static AudioChannel? AudioChannelR;
    public static AudioChannel? AudioChannelStartClick;
    public static AudioChannel? AudioChannelMetronome;
    
    private static float latency = 0;
    
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