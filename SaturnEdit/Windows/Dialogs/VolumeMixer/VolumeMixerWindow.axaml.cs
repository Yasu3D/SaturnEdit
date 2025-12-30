using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SaturnEdit.Systems;

namespace SaturnEdit.Windows.Dialogs.VolumeMixer;

public partial class VolumeMixerWindow : Window
{
    public VolumeMixerWindow()
    {
        InitializeComponent();
    }

    private bool blockEvents = false;

#region Methods
    private static int SliderValueToDecibels(double value)
    {
        return value > 24
            ? (int)(value - 36)
            : (int)(2 * value - 60);
    }

    private static double DecibelsToSliderValue(int value)
    {
        return value < -12
            ? 0.5 * value + 30
            : value + 36;
    }
#endregion Methods
    
#region System Event Handlers
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            SliderVolumeMaster.Value     = DecibelsToSliderValue(SettingsSystem.AudioSettings.MasterVolume);
            SliderVolumeAudio.Value      = DecibelsToSliderValue(SettingsSystem.AudioSettings.AudioVolume);
            SliderVolumeHitsound.Value   = DecibelsToSliderValue(SettingsSystem.AudioSettings.HitsoundVolume);
            SliderVolumeGuide.Value      = DecibelsToSliderValue(SettingsSystem.AudioSettings.GuideVolume);
            SliderVolumeTouch.Value      = DecibelsToSliderValue(SettingsSystem.AudioSettings.TouchVolume);
            SliderVolumeHold.Value       = DecibelsToSliderValue(SettingsSystem.AudioSettings.HoldVolume);
            SliderVolumeSlide.Value      = DecibelsToSliderValue(SettingsSystem.AudioSettings.SlideVolume);
            SliderVolumeBonus.Value      = DecibelsToSliderValue(SettingsSystem.AudioSettings.BonusVolume);
            SliderVolumeR.Value          = DecibelsToSliderValue(SettingsSystem.AudioSettings.RVolume);
            SliderVolumeStartClick.Value = DecibelsToSliderValue(SettingsSystem.AudioSettings.StartClickVolume);
            SliderVolumeMetronome.Value  = DecibelsToSliderValue(SettingsSystem.AudioSettings.MetronomeVolume);
            
            TextBlockVolumeMaster.Text     = getVolumeString(SettingsSystem.AudioSettings.MasterVolume);
            TextBlockVolumeAudio.Text      = getVolumeString(SettingsSystem.AudioSettings.AudioVolume);
            TextBlockVolumeHitsound.Text   = getVolumeString(SettingsSystem.AudioSettings.HitsoundVolume);
            TextBlockVolumeGuide.Text      = getVolumeString(SettingsSystem.AudioSettings.GuideVolume);
            TextBlockVolumeTouch.Text      = getVolumeString(SettingsSystem.AudioSettings.TouchVolume);
            TextBlockVolumeHold.Text       = getVolumeString(SettingsSystem.AudioSettings.HoldVolume);
            TextBlockVolumeSlide.Text      = getVolumeString(SettingsSystem.AudioSettings.SlideVolume);
            TextBlockVolumeBonus.Text      = getVolumeString(SettingsSystem.AudioSettings.BonusVolume);
            TextBlockVolumeR.Text          = getVolumeString(SettingsSystem.AudioSettings.RVolume);
            TextBlockVolumeStartClick.Text = getVolumeString(SettingsSystem.AudioSettings.StartClickVolume);
            TextBlockVolumeMetronome.Text  = getVolumeString(SettingsSystem.AudioSettings.MetronomeVolume);
            
            IconMuteMaster.Icon     = SettingsSystem.AudioSettings.MuteMaster     ? FluentIcons.Common.Icon.SpeakerMute : FluentIcons.Common.Icon.Speaker2;
            IconMuteAudio.Icon      = SettingsSystem.AudioSettings.MuteAudio      ? FluentIcons.Common.Icon.SpeakerMute : FluentIcons.Common.Icon.Speaker2;
            IconMuteHitsound.Icon   = SettingsSystem.AudioSettings.MuteHitsound   ? FluentIcons.Common.Icon.SpeakerMute : FluentIcons.Common.Icon.Speaker2;
            IconMuteGuide.Icon      = SettingsSystem.AudioSettings.MuteGuide      ? FluentIcons.Common.Icon.SpeakerMute : FluentIcons.Common.Icon.Speaker2;
            IconMuteTouch.Icon      = SettingsSystem.AudioSettings.MuteTouch      ? FluentIcons.Common.Icon.SpeakerMute : FluentIcons.Common.Icon.Speaker2;
            IconMuteHold.Icon       = SettingsSystem.AudioSettings.MuteHold       ? FluentIcons.Common.Icon.SpeakerMute : FluentIcons.Common.Icon.Speaker2;
            IconMuteSlide.Icon      = SettingsSystem.AudioSettings.MuteSlide      ? FluentIcons.Common.Icon.SpeakerMute : FluentIcons.Common.Icon.Speaker2;
            IconMuteBonus.Icon      = SettingsSystem.AudioSettings.MuteBonus      ? FluentIcons.Common.Icon.SpeakerMute : FluentIcons.Common.Icon.Speaker2;
            IconMuteR.Icon          = SettingsSystem.AudioSettings.MuteR          ? FluentIcons.Common.Icon.SpeakerMute : FluentIcons.Common.Icon.Speaker2;
            IconMuteStartClick.Icon = SettingsSystem.AudioSettings.MuteStartClick ? FluentIcons.Common.Icon.SpeakerMute : FluentIcons.Common.Icon.Speaker2;
            IconMuteMetronome.Icon  = SettingsSystem.AudioSettings.MuteMetronome  ? FluentIcons.Common.Icon.SpeakerMute : FluentIcons.Common.Icon.Speaker2;
            
            ToggleButtonMuteMaster.IsChecked     = SettingsSystem.AudioSettings.MuteMaster;
            ToggleButtonMuteAudio.IsChecked      = SettingsSystem.AudioSettings.MuteAudio;
            ToggleButtonMuteHitsound.IsChecked   = SettingsSystem.AudioSettings.MuteHitsound;
            ToggleButtonMuteGuide.IsChecked      = SettingsSystem.AudioSettings.MuteGuide;
            ToggleButtonMuteTouch.IsChecked      = SettingsSystem.AudioSettings.MuteTouch;
            ToggleButtonMuteHold.IsChecked       = SettingsSystem.AudioSettings.MuteHold;
            ToggleButtonMuteSlide.IsChecked      = SettingsSystem.AudioSettings.MuteSlide;
            ToggleButtonMuteBonus.IsChecked      = SettingsSystem.AudioSettings.MuteBonus;
            ToggleButtonMuteR.IsChecked          = SettingsSystem.AudioSettings.MuteR;
            ToggleButtonMuteStartClick.IsChecked = SettingsSystem.AudioSettings.MuteStartClick;
            ToggleButtonMuteMetronome.IsChecked  = SettingsSystem.AudioSettings.MuteMetronome;
            
            SliderVolumeMaster.IsEnabled     = !SettingsSystem.AudioSettings.MuteMaster;
            SliderVolumeAudio.IsEnabled      = !SettingsSystem.AudioSettings.MuteAudio;
            SliderVolumeHitsound.IsEnabled   = !SettingsSystem.AudioSettings.MuteHitsound;
            SliderVolumeGuide.IsEnabled      = !SettingsSystem.AudioSettings.MuteGuide;
            SliderVolumeTouch.IsEnabled      = !SettingsSystem.AudioSettings.MuteTouch;
            SliderVolumeHold.IsEnabled       = !SettingsSystem.AudioSettings.MuteHold;
            SliderVolumeSlide.IsEnabled      = !SettingsSystem.AudioSettings.MuteSlide;
            SliderVolumeBonus.IsEnabled      = !SettingsSystem.AudioSettings.MuteBonus;
            SliderVolumeR.IsEnabled          = !SettingsSystem.AudioSettings.MuteR;
            SliderVolumeStartClick.IsEnabled = !SettingsSystem.AudioSettings.MuteStartClick;
            SliderVolumeMetronome.IsEnabled  = !SettingsSystem.AudioSettings.MuteMetronome;
            
            blockEvents = false;
        });
        
        return;

        string getVolumeString(int volume)
        {
            if (volume == 0) return "0dB";
            if (volume > 0) return $"+{volume.ToString()}dB";
            return $"{volume.ToString()}dB";
        }
    }
#endregion System Event Handlers

#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        SettingsSystem.SettingsChanged -= OnSettingsChanged;
        
        base.OnUnloaded(e);
    }
    
    private void SliderVolume_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not Slider slider) return;

        int value = SliderValueToDecibels(slider.Value);
        
        if      (slider == SliderVolumeMaster)     { SettingsSystem.AudioSettings.MasterVolume     = value; }
        else if (slider == SliderVolumeAudio)      { SettingsSystem.AudioSettings.AudioVolume      = value; }
        else if (slider == SliderVolumeHitsound)   { SettingsSystem.AudioSettings.HitsoundVolume   = value; }
        else if (slider == SliderVolumeGuide)      { SettingsSystem.AudioSettings.GuideVolume      = value; }
        else if (slider == SliderVolumeTouch)      { SettingsSystem.AudioSettings.TouchVolume      = value; }
        else if (slider == SliderVolumeHold)       { SettingsSystem.AudioSettings.HoldVolume       = value; }
        else if (slider == SliderVolumeSlide)      { SettingsSystem.AudioSettings.SlideVolume      = value; }
        else if (slider == SliderVolumeBonus)      { SettingsSystem.AudioSettings.BonusVolume      = value; }
        else if (slider == SliderVolumeR)          { SettingsSystem.AudioSettings.RVolume          = value; }
        else if (slider == SliderVolumeStartClick) { SettingsSystem.AudioSettings.StartClickVolume = value; }
        else if (slider == SliderVolumeMetronome)  { SettingsSystem.AudioSettings.MetronomeVolume  = value; }
    }
    
    private void SliderVolume_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not Slider slider) return;

        int value = (int)slider.Value;
        
        onScrollUp();
        onScrollDown();
        
        value = SliderValueToDecibels(value);
        
        if      (slider == SliderVolumeMaster)     { SettingsSystem.AudioSettings.MasterVolume     = value; }
        else if (slider == SliderVolumeAudio)      { SettingsSystem.AudioSettings.AudioVolume      = value; }
        else if (slider == SliderVolumeHitsound)   { SettingsSystem.AudioSettings.HitsoundVolume   = value; }
        else if (slider == SliderVolumeGuide)      { SettingsSystem.AudioSettings.GuideVolume      = value; }
        else if (slider == SliderVolumeTouch)      { SettingsSystem.AudioSettings.TouchVolume      = value; }
        else if (slider == SliderVolumeHold)       { SettingsSystem.AudioSettings.HoldVolume       = value; }
        else if (slider == SliderVolumeSlide)      { SettingsSystem.AudioSettings.SlideVolume      = value; }
        else if (slider == SliderVolumeBonus)      { SettingsSystem.AudioSettings.BonusVolume      = value; }
        else if (slider == SliderVolumeR)          { SettingsSystem.AudioSettings.RVolume          = value; }
        else if (slider == SliderVolumeStartClick) { SettingsSystem.AudioSettings.StartClickVolume = value; }
        else if (slider == SliderVolumeMetronome)  { SettingsSystem.AudioSettings.MetronomeVolume  = value; }
        
        return;

        void onScrollUp()
        {
            if (e.Delta.Y <= 0) return;

            value = (int)Math.Clamp(slider.Value + 1, slider.Minimum, slider.Maximum);
        }

        void onScrollDown()
        {
            if (e.Delta.Y >= 0) return;
            
            value = (int)Math.Clamp(slider.Value - 1, slider.Minimum, slider.Maximum);
        }
    }

    private async void ButtonPickSound_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (sender is not Button button) return;

            string path = "";

            try
            {
                // Open file picker.
                IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(new()
                {
                    AllowMultiple = false,
                    FileTypeFilter =
                    [
                        new("Audio Files")
                        {
                            Patterns = ["*.wav", "*.mp3", "*.ogg", "*.flac"],
                        },
                    ],
                });
                if (files.Count != 1) return;

                path = files[0].Path.LocalPath;
            }
            catch (Exception ex)
            {
                // don't throw
                LoggingSystem.WriteSessionLog(ex.ToString());
            }

            if (!File.Exists(path)) return;

            if      (button == ButtonPickSoundGuide)
            {
                SettingsSystem.AudioSettings.HitsoundGuidePath = path;
            }
            else if (button == ButtonPickSoundTouch)
            {
                SettingsSystem.AudioSettings.HitsoundTouchPath = path;
            }
            else if (button == ButtonPickSoundHold)
            {
                SettingsSystem.AudioSettings.HitsoundHoldPath = path;
            }
            else if (button == ButtonPickSoundSlide)
            {
                SettingsSystem.AudioSettings.HitsoundSlidePath = path;
            }
            else if (button == ButtonPickSoundBonus)
            {
                SettingsSystem.AudioSettings.HitsoundBonusPath = path;
            }
            else if (button == ButtonPickSoundR)
            {
                SettingsSystem.AudioSettings.HitsoundRPath = path;
            }
            else if (button == ButtonPickSoundStartClick)
            {
                SettingsSystem.AudioSettings.HitsoundStartClickPath = path;
            }
            else if (button == ButtonPickSoundMetronome)
            {
                SettingsSystem.AudioSettings.HitsoundMetronomePath = path;
            }
        }
        catch (Exception ex)
        {
            // Don't throw.
            LoggingSystem.WriteSessionLog(ex.ToString());
        }
    }
    
    private void ToggleButtonMute_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not ToggleButton toggleButton) return;

        bool value = toggleButton.IsChecked ?? false;
        
        if      (toggleButton == ToggleButtonMuteMaster)     { SettingsSystem.AudioSettings.MuteMaster     = value; }
        else if (toggleButton == ToggleButtonMuteAudio)      { SettingsSystem.AudioSettings.MuteAudio      = value; }
        else if (toggleButton == ToggleButtonMuteHitsound)   { SettingsSystem.AudioSettings.MuteHitsound   = value; }
        else if (toggleButton == ToggleButtonMuteGuide)      { SettingsSystem.AudioSettings.MuteGuide      = value; }
        else if (toggleButton == ToggleButtonMuteTouch)      { SettingsSystem.AudioSettings.MuteTouch      = value; }
        else if (toggleButton == ToggleButtonMuteHold)       { SettingsSystem.AudioSettings.MuteHold       = value; }
        else if (toggleButton == ToggleButtonMuteSlide)      { SettingsSystem.AudioSettings.MuteSlide      = value; }
        else if (toggleButton == ToggleButtonMuteBonus)      { SettingsSystem.AudioSettings.MuteBonus      = value; }
        else if (toggleButton == ToggleButtonMuteR)          { SettingsSystem.AudioSettings.MuteR          = value; }
        else if (toggleButton == ToggleButtonMuteStartClick) { SettingsSystem.AudioSettings.MuteStartClick = value; }
        else if (toggleButton == ToggleButtonMuteMetronome)  { SettingsSystem.AudioSettings.MuteMetronome  = value; }
    }
#endregion UI Event Handlers
}