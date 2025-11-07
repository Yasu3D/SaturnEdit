using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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

        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
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
    
#region System Event Delegates
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
            
            TextBlockVolumeMaster.Text     = $"{SettingsSystem.AudioSettings.MasterVolume.ToString(CultureInfo.InvariantCulture)}dB";
            TextBlockVolumeAudio.Text      = $"{SettingsSystem.AudioSettings.AudioVolume.ToString(CultureInfo.InvariantCulture)}dB";
            TextBlockVolumeHitsound.Text   = $"{SettingsSystem.AudioSettings.HitsoundVolume.ToString(CultureInfo.InvariantCulture)}dB";
            TextBlockVolumeGuide.Text      = $"{SettingsSystem.AudioSettings.GuideVolume.ToString(CultureInfo.InvariantCulture)}dB";
            TextBlockVolumeTouch.Text      = $"{SettingsSystem.AudioSettings.TouchVolume.ToString(CultureInfo.InvariantCulture)}dB";
            TextBlockVolumeHold.Text       = $"{SettingsSystem.AudioSettings.HoldVolume.ToString(CultureInfo.InvariantCulture)}dB";
            TextBlockVolumeSlide.Text      = $"{SettingsSystem.AudioSettings.SlideVolume.ToString(CultureInfo.InvariantCulture)}dB";
            TextBlockVolumeBonus.Text      = $"{SettingsSystem.AudioSettings.BonusVolume.ToString(CultureInfo.InvariantCulture)}dB";
            TextBlockVolumeR.Text          = $"{SettingsSystem.AudioSettings.RVolume.ToString(CultureInfo.InvariantCulture)}dB";
            TextBlockVolumeStartClick.Text = $"{SettingsSystem.AudioSettings.StartClickVolume.ToString(CultureInfo.InvariantCulture)}dB";
            TextBlockVolumeMetronome.Text  = $"{SettingsSystem.AudioSettings.MetronomeVolume.ToString(CultureInfo.InvariantCulture)}dB";
        
            blockEvents = false;
        });
    }
#endregion System Event Delegates

#region UI Event Handlers
    private void SliderVolume_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not Slider slider) return;
        
        int value = SliderValueToDecibels(slider.Value);
        
        if      (ReferenceEquals(sender, SliderVolumeMaster))     { SettingsSystem.AudioSettings.MasterVolume     = value; }
        else if (ReferenceEquals(sender, SliderVolumeAudio))      { SettingsSystem.AudioSettings.AudioVolume      = value; }
        else if (ReferenceEquals(sender, SliderVolumeHitsound))   { SettingsSystem.AudioSettings.HitsoundVolume   = value; }
        else if (ReferenceEquals(sender, SliderVolumeGuide))      { SettingsSystem.AudioSettings.GuideVolume      = value; }
        else if (ReferenceEquals(sender, SliderVolumeTouch))      { SettingsSystem.AudioSettings.TouchVolume      = value; }
        else if (ReferenceEquals(sender, SliderVolumeHold))       { SettingsSystem.AudioSettings.HoldVolume       = value; }
        else if (ReferenceEquals(sender, SliderVolumeSlide))      { SettingsSystem.AudioSettings.SlideVolume      = value; }
        else if (ReferenceEquals(sender, SliderVolumeBonus))      { SettingsSystem.AudioSettings.BonusVolume      = value; }
        else if (ReferenceEquals(sender, SliderVolumeR))          { SettingsSystem.AudioSettings.RVolume          = value; }
        else if (ReferenceEquals(sender, SliderVolumeStartClick)) { SettingsSystem.AudioSettings.StartClickVolume = value; }
        else if (ReferenceEquals(sender, SliderVolumeMetronome))  { SettingsSystem.AudioSettings.MetronomeVolume  = value; }
    }

    private async void ButtonPickSound_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (sender == null) return;

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
                Console.WriteLine(ex);
            }

            if (!File.Exists(path)) return;

            if      (ReferenceEquals(sender, ButtonPickSoundGuide))
            {
                SettingsSystem.AudioSettings.HitsoundGuidePath = path;
            }
            else if (ReferenceEquals(sender, ButtonPickSoundTouch))
            {
                SettingsSystem.AudioSettings.HitsoundTouchPath = path;
            }
            else if (ReferenceEquals(sender, ButtonPickSoundHold))
            {
                SettingsSystem.AudioSettings.HitsoundHoldPath = path;
            }
            else if (ReferenceEquals(sender, ButtonPickSoundSlide))
            {
                SettingsSystem.AudioSettings.HitsoundSlidePath = path;
            }
            else if (ReferenceEquals(sender, ButtonPickSoundBonus))
            {
                SettingsSystem.AudioSettings.HitsoundBonusPath = path;
            }
            else if (ReferenceEquals(sender, ButtonPickSoundR))
            {
                SettingsSystem.AudioSettings.HitsoundRPath = path;
            }
            else if (ReferenceEquals(sender, ButtonPickSoundStartClick))
            {
                SettingsSystem.AudioSettings.HitsoundStartClickPath = path;
            }
            else if (ReferenceEquals(sender, ButtonPickSoundMetronome))
            {
                SettingsSystem.AudioSettings.HitsoundMetronomePath = path;
            }
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
    }
#endregion UI Event Handlers
}