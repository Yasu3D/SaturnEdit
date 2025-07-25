using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using SaturnEdit.Systems;

namespace SaturnEdit.Views.Main.ChartEditor.Tabs;

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
    }

    private bool blockChanges = false;
    
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        blockChanges = true;
        
        ChannelMaster.SliderVolume.Value     = VolumeToSliderValue(SettingsSystem.AudioSettings.MasterVolume);
        ChannelAudio.SliderVolume.Value      = VolumeToSliderValue(SettingsSystem.AudioSettings.AudioVolume);
        ChannelGuide.SliderVolume.Value      = VolumeToSliderValue(SettingsSystem.AudioSettings.GuideVolume);
        ChannelTouch.SliderVolume.Value      = VolumeToSliderValue(SettingsSystem.AudioSettings.TouchVolume);
        ChannelChain.SliderVolume.Value      = VolumeToSliderValue(SettingsSystem.AudioSettings.ChainVolume);
        ChannelHold.SliderVolume.Value       = VolumeToSliderValue(SettingsSystem.AudioSettings.HoldVolume);
        ChannelHoldLoop.SliderVolume.Value   = VolumeToSliderValue(SettingsSystem.AudioSettings.HoldLoopVolume);
        ChannelSnap.SliderVolume.Value       = VolumeToSliderValue(SettingsSystem.AudioSettings.SnapVolume);
        ChannelSlide.SliderVolume.Value      = VolumeToSliderValue(SettingsSystem.AudioSettings.SlideVolume);
        ChannelBonus.SliderVolume.Value      = VolumeToSliderValue(SettingsSystem.AudioSettings.BonusVolume);
        ChannelR.SliderVolume.Value          = VolumeToSliderValue(SettingsSystem.AudioSettings.RVolume);
        ChannelStartClick.SliderVolume.Value = VolumeToSliderValue(SettingsSystem.AudioSettings.StartClickVolume);
        ChannelMetronome.SliderVolume.Value  = VolumeToSliderValue(SettingsSystem.AudioSettings.MetronomeVolume);

        blockChanges = false;
    }
    
    private void ChannelMasterSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelMaster.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.MasterVolume = SliderValueToVolume(ChannelMaster.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelAudioSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelAudio.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.AudioVolume = SliderValueToVolume(ChannelAudio.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelGuideSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelGuide.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.GuideVolume = SliderValueToVolume(ChannelGuide.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelTouchSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelTouch.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.TouchVolume = SliderValueToVolume(ChannelTouch.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelChainSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelChain.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.ChainVolume = SliderValueToVolume(ChannelChain.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelHoldSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelHold.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.HoldVolume = SliderValueToVolume(ChannelHold.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelHoldLoopSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelHoldLoop.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.HoldLoopVolume = SliderValueToVolume(ChannelHoldLoop.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelSnapSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelSnap.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.SnapVolume = SliderValueToVolume(ChannelSnap.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelSlideSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelSlide.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.SlideVolume = SliderValueToVolume(ChannelSlide.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelBonusSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelBonus.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.BonusVolume = SliderValueToVolume(ChannelBonus.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelRSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelR.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.RVolume = SliderValueToVolume(ChannelR.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelStartClickSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelStartClick.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.StartClickVolume = SliderValueToVolume(ChannelStartClick.SliderVolume.Value);
        blockChanges = false;
    }
    
    private void ChannelMetronomeSliderVolumeOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockChanges) return;
        if (ChannelMetronome.SliderVolume == null) return;
        
        blockChanges = true;
        SettingsSystem.AudioSettings.MetronomeVolume = SliderValueToVolume(ChannelMetronome.SliderVolume.Value);
        blockChanges = false;
    }

    private int SliderValueToVolume(double value)
    {
        return value > 24
            ? (int)(value - 36)
            : (int)(2 * value - 60);
    }

    private double VolumeToSliderValue(int value)
    {
        return value < -12
            ? 0.5 * value + 30
            : value + 36;
    }
}