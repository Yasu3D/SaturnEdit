using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using SaturnEdit.Systems;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class AudioMixerView : UserControl
{
    public AudioMixerView()
    {
        InitializeComponent();
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
        
        ChannelMaster.SliderVolume.ValueChanged     += ChannelMasterSliderVolumeOnValueChanged;
        ChannelAudio.SliderVolume.ValueChanged      += ChannelAudioSliderVolumeOnValueChanged;
        ChannelGuide.SliderVolume.ValueChanged      += ChannelGuideSliderVolumeOnValueChanged;
        ChannelTouch.SliderVolume.ValueChanged      += ChannelTouchSliderVolumeOnValueChanged;
        ChannelChain.SliderVolume.ValueChanged      += ChannelChainSliderVolumeOnValueChanged;
        ChannelHold.SliderVolume.ValueChanged       += ChannelHoldSliderVolumeOnValueChanged;
        ChannelHoldLoop.SliderVolume.ValueChanged   += ChannelHoldLoopSliderVolumeOnValueChanged;
        ChannelSnap.SliderVolume.ValueChanged       += ChannelSnapSliderVolumeOnValueChanged;
        ChannelSlide.SliderVolume.ValueChanged      += ChannelSlideSliderVolumeOnValueChanged;
        ChannelBonus.SliderVolume.ValueChanged      += ChannelBonusSliderVolumeOnValueChanged;
        ChannelR.SliderVolume.ValueChanged          += ChannelRSliderVolumeOnValueChanged;
        ChannelStartClick.SliderVolume.ValueChanged += ChannelStartClickSliderVolumeOnValueChanged;
        ChannelMetronome.SliderVolume.ValueChanged  += ChannelMetronomeSliderVolumeOnValueChanged;

        TimeSystem.UpdateTimer.Tick += UpdateTimer_OnTick;
    }

    private bool blockChanges = false;
    
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        blockChanges = true;
        
        ChannelMaster.SliderVolume.Value     = DecibelsToSliderValue(SettingsSystem.AudioSettings.MasterVolume);
        ChannelAudio.SliderVolume.Value      = DecibelsToSliderValue(SettingsSystem.AudioSettings.AudioVolume);
        ChannelGuide.SliderVolume.Value      = DecibelsToSliderValue(SettingsSystem.AudioSettings.GuideVolume);
        ChannelTouch.SliderVolume.Value      = DecibelsToSliderValue(SettingsSystem.AudioSettings.TouchVolume);
        ChannelChain.SliderVolume.Value      = DecibelsToSliderValue(SettingsSystem.AudioSettings.ChainVolume);
        ChannelHold.SliderVolume.Value       = DecibelsToSliderValue(SettingsSystem.AudioSettings.HoldVolume);
        ChannelHoldLoop.SliderVolume.Value   = DecibelsToSliderValue(SettingsSystem.AudioSettings.HoldLoopVolume);
        ChannelSnap.SliderVolume.Value       = DecibelsToSliderValue(SettingsSystem.AudioSettings.SnapVolume);
        ChannelSlide.SliderVolume.Value      = DecibelsToSliderValue(SettingsSystem.AudioSettings.SlideVolume);
        ChannelBonus.SliderVolume.Value      = DecibelsToSliderValue(SettingsSystem.AudioSettings.BonusVolume);
        ChannelR.SliderVolume.Value          = DecibelsToSliderValue(SettingsSystem.AudioSettings.RVolume);
        ChannelStartClick.SliderVolume.Value = DecibelsToSliderValue(SettingsSystem.AudioSettings.StartClickVolume);
        ChannelMetronome.SliderVolume.Value  = DecibelsToSliderValue(SettingsSystem.AudioSettings.MetronomeVolume);

        blockChanges = false;
    }
    
    private void ChannelMasterSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelMaster.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.MasterVolume = SliderValueToDecibels(ChannelMaster.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelAudioSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelAudio.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.AudioVolume = SliderValueToDecibels(ChannelAudio.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelGuideSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelGuide.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.GuideVolume = SliderValueToDecibels(ChannelGuide.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelTouchSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelTouch.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.TouchVolume = SliderValueToDecibels(ChannelTouch.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelChainSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelChain.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.ChainVolume = SliderValueToDecibels(ChannelChain.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelHoldSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelHold.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.HoldVolume = SliderValueToDecibels(ChannelHold.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelHoldLoopSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelHoldLoop.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.HoldLoopVolume = SliderValueToDecibels(ChannelHoldLoop.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelSnapSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelSnap.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.SnapVolume = SliderValueToDecibels(ChannelSnap.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelSlideSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelSlide.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.SlideVolume = SliderValueToDecibels(ChannelSlide.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelBonusSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelBonus.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.BonusVolume = SliderValueToDecibels(ChannelBonus.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelRSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelR.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.RVolume = SliderValueToDecibels(ChannelR.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelStartClickSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelStartClick.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.StartClickVolume = SliderValueToDecibels(ChannelStartClick.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelMetronomeSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelMetronome.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.MetronomeVolume = SliderValueToDecibels(ChannelMetronome.SliderVolume.Value);
        blockChanges = false;
    }

    private void UpdateTimer_OnTick(object? sender, EventArgs e)
    {
        ChannelAudio.MixerVolumeBarLeft.Height = getLevel(AudioSystem.AudioChannelAudio?.LevelLeft, AudioSystem.AudioChannelAudio?.Volume, ChannelAudio.MixerVolumeBarLeft.Height);
        ChannelAudio.MixerVolumeBarRight.Height = getLevel(AudioSystem.AudioChannelAudio?.LevelRight, AudioSystem.AudioChannelAudio?.Volume, ChannelAudio.MixerVolumeBarRight.Height);
        
        return;

        double getLevel(float? level, double? volume, double currentHeight)
        {
            const double maxHeight = 175;
            
            currentHeight = Math.Max(currentHeight, maxHeight * (level ?? 0) * (volume ?? 0));
            currentHeight -= TimeSystem.TickInterval * 0.2f;
            return Math.Clamp(currentHeight, 0, maxHeight);
        }
    }
    
    private int SliderValueToDecibels(double value)
    {
        return value > 24
            ? (int)(value - 36)
            : (int)(2 * value - 60);
    }

    private double DecibelsToSliderValue(int value)
    {
        return value < -12
            ? 0.5 * value + 30
            : value + 36;
    }
}