using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SaturnEdit.Audio;
using SaturnEdit.Systems;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class AudioMixerView : UserControl
{
    public AudioMixerView()
    {
        InitializeComponent();
        InitChannels();
        
        TimeSystem.UpdateTimer.Tick += UpdateTimer_OnTick;
    }

    private async void InitChannels()
    {
        ChannelMaster.SliderVolume.ValueChanged     += ChannelSliderVolume_OnValueChanged;
        ChannelAudio.SliderVolume.ValueChanged      += ChannelSliderVolume_OnValueChanged;
        ChannelGuide.SliderVolume.ValueChanged      += ChannelSliderVolume_OnValueChanged;
        ChannelTouch.SliderVolume.ValueChanged      += ChannelSliderVolume_OnValueChanged;
        ChannelChain.SliderVolume.ValueChanged      += ChannelSliderVolume_OnValueChanged;
        ChannelHold.SliderVolume.ValueChanged       += ChannelSliderVolume_OnValueChanged;
        ChannelHoldLoop.SliderVolume.ValueChanged   += ChannelSliderVolume_OnValueChanged;
        ChannelSnap.SliderVolume.ValueChanged       += ChannelSliderVolume_OnValueChanged;
        ChannelSlide.SliderVolume.ValueChanged      += ChannelSliderVolume_OnValueChanged;
        ChannelBonus.SliderVolume.ValueChanged      += ChannelSliderVolume_OnValueChanged;
        ChannelR.SliderVolume.ValueChanged          += ChannelSliderVolume_OnValueChanged;
        ChannelStartClick.SliderVolume.ValueChanged += ChannelSliderVolume_OnValueChanged;
        ChannelMetronome.SliderVolume.ValueChanged  += ChannelSliderVolume_OnValueChanged;
        
        ChannelMaster.ButtonMute.IsCheckedChanged     += ChannelButtonMute_OnIsCheckedChanged;
        ChannelAudio.ButtonMute.IsCheckedChanged      += ChannelButtonMute_OnIsCheckedChanged;
        ChannelGuide.ButtonMute.IsCheckedChanged      += ChannelButtonMute_OnIsCheckedChanged;
        ChannelTouch.ButtonMute.IsCheckedChanged      += ChannelButtonMute_OnIsCheckedChanged;
        ChannelChain.ButtonMute.IsCheckedChanged      += ChannelButtonMute_OnIsCheckedChanged;
        ChannelHold.ButtonMute.IsCheckedChanged       += ChannelButtonMute_OnIsCheckedChanged;
        ChannelHoldLoop.ButtonMute.IsCheckedChanged   += ChannelButtonMute_OnIsCheckedChanged;
        ChannelSnap.ButtonMute.IsCheckedChanged       += ChannelButtonMute_OnIsCheckedChanged;
        ChannelSlide.ButtonMute.IsCheckedChanged      += ChannelButtonMute_OnIsCheckedChanged;
        ChannelBonus.ButtonMute.IsCheckedChanged      += ChannelButtonMute_OnIsCheckedChanged;
        ChannelR.ButtonMute.IsCheckedChanged          += ChannelButtonMute_OnIsCheckedChanged;
        ChannelStartClick.ButtonMute.IsCheckedChanged += ChannelButtonMute_OnIsCheckedChanged;
        ChannelMetronome.ButtonMute.IsCheckedChanged  += ChannelButtonMute_OnIsCheckedChanged;
        
        ChannelMaster.ButtonSound.Click     += ChannelButtonSound_OnClick;
        ChannelAudio.ButtonSound.Click      += ChannelButtonSound_OnClick;
        ChannelGuide.ButtonSound.Click      += ChannelButtonSound_OnClick;
        ChannelTouch.ButtonSound.Click      += ChannelButtonSound_OnClick;
        ChannelChain.ButtonSound.Click      += ChannelButtonSound_OnClick;
        ChannelHold.ButtonSound.Click       += ChannelButtonSound_OnClick;
        ChannelHoldLoop.ButtonSound.Click   += ChannelButtonSound_OnClick;
        ChannelSnap.ButtonSound.Click       += ChannelButtonSound_OnClick;
        ChannelSlide.ButtonSound.Click      += ChannelButtonSound_OnClick;
        ChannelBonus.ButtonSound.Click      += ChannelButtonSound_OnClick;
        ChannelR.ButtonSound.Click          += ChannelButtonSound_OnClick;
        ChannelStartClick.ButtonSound.Click += ChannelButtonSound_OnClick;
        ChannelMetronome.ButtonSound.Click  += ChannelButtonSound_OnClick;
        
        // race conditions, part 2 (:
        await Task.Delay(1);
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

    private bool blockEvents = false;
    
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        blockEvents = true;
        
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
        
        ChannelMaster.ButtonMute.IsChecked     = SettingsSystem.AudioSettings.MuteMaster;
        ChannelAudio.ButtonMute.IsChecked      = SettingsSystem.AudioSettings.MuteAudio;
        ChannelGuide.ButtonMute.IsChecked      = SettingsSystem.AudioSettings.MuteGuide;
        ChannelTouch.ButtonMute.IsChecked      = SettingsSystem.AudioSettings.MuteTouch;
        ChannelChain.ButtonMute.IsChecked      = SettingsSystem.AudioSettings.MuteChain;
        ChannelHold.ButtonMute.IsChecked       = SettingsSystem.AudioSettings.MuteHold;
        ChannelHoldLoop.ButtonMute.IsChecked   = SettingsSystem.AudioSettings.MuteHoldLoop;
        ChannelSnap.ButtonMute.IsChecked       = SettingsSystem.AudioSettings.MuteSnap;
        ChannelSlide.ButtonMute.IsChecked      = SettingsSystem.AudioSettings.MuteSlide;
        ChannelBonus.ButtonMute.IsChecked      = SettingsSystem.AudioSettings.MuteBonus;
        ChannelR.ButtonMute.IsChecked          = SettingsSystem.AudioSettings.MuteR;
        ChannelStartClick.ButtonMute.IsChecked = SettingsSystem.AudioSettings.MuteStartClick;
        ChannelMetronome.ButtonMute.IsChecked  = SettingsSystem.AudioSettings.MuteMetronome;
        
        string noSound = "";
        object? resource = null;
        
        Application.Current?.TryGetResource("ChartEditor.AudioMixer.Placeholder.NoSound", Application.Current.ActualThemeVariant, out resource);
        if (resource is string s) noSound = s;
        
        ChannelGuide.TextBlockSound.Text      = File.Exists(SettingsSystem.AudioSettings.HitsoundGuidePath)      ? Path.GetFileNameWithoutExtension(SettingsSystem.AudioSettings.HitsoundGuidePath)      : noSound;
        ChannelTouch.TextBlockSound.Text      = File.Exists(SettingsSystem.AudioSettings.HitsoundTouchPath)      ? Path.GetFileNameWithoutExtension(SettingsSystem.AudioSettings.HitsoundTouchPath)      : noSound;
        ChannelChain.TextBlockSound.Text      = File.Exists(SettingsSystem.AudioSettings.HitsoundChainPath)      ? Path.GetFileNameWithoutExtension(SettingsSystem.AudioSettings.HitsoundChainPath)      : noSound;
        ChannelHold.TextBlockSound.Text       = File.Exists(SettingsSystem.AudioSettings.HitsoundHoldPath)       ? Path.GetFileNameWithoutExtension(SettingsSystem.AudioSettings.HitsoundHoldPath)       : noSound;
        ChannelHoldLoop.TextBlockSound.Text   = File.Exists(SettingsSystem.AudioSettings.HitsoundHoldLoopPath)   ? Path.GetFileNameWithoutExtension(SettingsSystem.AudioSettings.HitsoundHoldLoopPath)   : noSound;
        ChannelSnap.TextBlockSound.Text       = File.Exists(SettingsSystem.AudioSettings.HitsoundSnapPath)       ? Path.GetFileNameWithoutExtension(SettingsSystem.AudioSettings.HitsoundSnapPath)       : noSound;
        ChannelSlide.TextBlockSound.Text      = File.Exists(SettingsSystem.AudioSettings.HitsoundSlidePath)      ? Path.GetFileNameWithoutExtension(SettingsSystem.AudioSettings.HitsoundSlidePath)      : noSound;
        ChannelBonus.TextBlockSound.Text      = File.Exists(SettingsSystem.AudioSettings.HitsoundBonusPath)      ? Path.GetFileNameWithoutExtension(SettingsSystem.AudioSettings.HitsoundBonusPath)      : noSound;
        ChannelR.TextBlockSound.Text          = File.Exists(SettingsSystem.AudioSettings.HitsoundRPath)          ? Path.GetFileNameWithoutExtension(SettingsSystem.AudioSettings.HitsoundRPath)          : noSound;
        ChannelStartClick.TextBlockSound.Text = File.Exists(SettingsSystem.AudioSettings.HitsoundStartClickPath) ? Path.GetFileNameWithoutExtension(SettingsSystem.AudioSettings.HitsoundStartClickPath) : noSound;
        ChannelMetronome.TextBlockSound.Text  = File.Exists(SettingsSystem.AudioSettings.HitsoundMetronomePath)  ? Path.GetFileNameWithoutExtension(SettingsSystem.AudioSettings.HitsoundMetronomePath)  : noSound;

        blockEvents = false;
    }

    private void UpdateTimer_OnTick(object? sender, EventArgs e)
    {
        double audioVolume = AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.AudioVolume);
        ChannelAudio.MixerVolumeBarLeft.Height = getLevel(AudioSystem.AudioChannelAudio?.LevelLeft, audioVolume, ChannelAudio.MixerVolumeBarLeft.Height);
        ChannelAudio.MixerVolumeBarRight.Height = getLevel(AudioSystem.AudioChannelAudio?.LevelRight, audioVolume, ChannelAudio.MixerVolumeBarRight.Height);
        
        double guideVolume = AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.AudioVolume);
        ChannelGuide.MixerVolumeBarLeft.Height = getLevel(AudioSystem.AudioChannelGuide?.LevelLeft, guideVolume, ChannelGuide.MixerVolumeBarLeft.Height);
        ChannelGuide.MixerVolumeBarRight.Height = getLevel(AudioSystem.AudioChannelGuide?.LevelRight, guideVolume, ChannelGuide.MixerVolumeBarRight.Height);
        
        double touchVolume = AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.AudioVolume);
        ChannelTouch.MixerVolumeBarLeft.Height = getLevel(AudioSystem.AudioChannelTouch?.LevelLeft, touchVolume, ChannelTouch.MixerVolumeBarLeft.Height);
        ChannelTouch.MixerVolumeBarRight.Height = getLevel(AudioSystem.AudioChannelTouch?.LevelRight, touchVolume, ChannelTouch.MixerVolumeBarRight.Height);
        
        double chainVolume = AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.AudioVolume);
        ChannelChain.MixerVolumeBarLeft.Height = getLevel(AudioSystem.AudioChannelChain?.LevelLeft, chainVolume, ChannelChain.MixerVolumeBarLeft.Height);
        ChannelChain.MixerVolumeBarRight.Height = getLevel(AudioSystem.AudioChannelChain?.LevelRight, chainVolume, ChannelChain.MixerVolumeBarRight.Height);
        
        double holdVolume = AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.AudioVolume);
        ChannelHold.MixerVolumeBarLeft.Height = getLevel(AudioSystem.AudioChannelHold?.LevelLeft, holdVolume, ChannelHold.MixerVolumeBarLeft.Height);
        ChannelHold.MixerVolumeBarRight.Height = getLevel(AudioSystem.AudioChannelHold?.LevelRight, holdVolume, ChannelHold.MixerVolumeBarRight.Height);
        
        double holdLoopVolume = AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.AudioVolume);
        ChannelHoldLoop.MixerVolumeBarLeft.Height = getLevel(AudioSystem.AudioChannelHoldLoop?.LevelLeft, holdLoopVolume, ChannelHoldLoop.MixerVolumeBarLeft.Height);
        ChannelHoldLoop.MixerVolumeBarRight.Height = getLevel(AudioSystem.AudioChannelHoldLoop?.LevelRight, holdLoopVolume, ChannelHoldLoop.MixerVolumeBarRight.Height);
        
        double snapVolume = AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.AudioVolume);
        ChannelSnap.MixerVolumeBarLeft.Height = getLevel(AudioSystem.AudioChannelSnap?.LevelLeft, snapVolume, ChannelSnap.MixerVolumeBarLeft.Height);
        ChannelSnap.MixerVolumeBarRight.Height = getLevel(AudioSystem.AudioChannelSnap?.LevelRight, snapVolume, ChannelSnap.MixerVolumeBarRight.Height);
        
        double slideVolume = AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.AudioVolume);
        ChannelSlide.MixerVolumeBarLeft.Height = getLevel(AudioSystem.AudioChannelSlide?.LevelLeft, slideVolume, ChannelSlide.MixerVolumeBarLeft.Height);
        ChannelSlide.MixerVolumeBarRight.Height = getLevel(AudioSystem.AudioChannelSlide?.LevelRight, slideVolume, ChannelSlide.MixerVolumeBarRight.Height);
        
        double bonusVolume = AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.AudioVolume);
        ChannelBonus.MixerVolumeBarLeft.Height = getLevel(AudioSystem.AudioChannelBonus?.LevelLeft, bonusVolume, ChannelBonus.MixerVolumeBarLeft.Height);
        ChannelBonus.MixerVolumeBarRight.Height = getLevel(AudioSystem.AudioChannelBonus?.LevelRight, bonusVolume, ChannelBonus.MixerVolumeBarRight.Height);
        
        double rVolume = AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.AudioVolume);
        ChannelR.MixerVolumeBarLeft.Height = getLevel(AudioSystem.AudioChannelR?.LevelLeft, rVolume, ChannelR.MixerVolumeBarLeft.Height);
        ChannelR.MixerVolumeBarRight.Height = getLevel(AudioSystem.AudioChannelR?.LevelRight, rVolume, ChannelR.MixerVolumeBarRight.Height);
        
        double startClickVolume = AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.AudioVolume);
        ChannelStartClick.MixerVolumeBarLeft.Height = getLevel(AudioSystem.AudioChannelStartClick?.LevelLeft, startClickVolume, ChannelStartClick.MixerVolumeBarLeft.Height);
        ChannelStartClick.MixerVolumeBarRight.Height = getLevel(AudioSystem.AudioChannelStartClick?.LevelRight, startClickVolume, ChannelStartClick.MixerVolumeBarRight.Height);
        
        double metronomeVolume = AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.AudioVolume);
        ChannelMetronome.MixerVolumeBarLeft.Height = getLevel(AudioSystem.AudioChannelMetronome?.LevelLeft, metronomeVolume, ChannelMetronome.MixerVolumeBarLeft.Height);
        ChannelMetronome.MixerVolumeBarRight.Height = getLevel(AudioSystem.AudioChannelMetronome?.LevelRight, metronomeVolume, ChannelMetronome.MixerVolumeBarRight.Height);
        
        double masterVolume = AudioChannel.DecibelToVolume(SettingsSystem.AudioSettings.MasterVolume);
        double masterRmsLeft = ChannelAudio.MixerVolumeBarLeft.Height * ChannelAudio.MixerVolumeBarLeft.Height;
        masterRmsLeft += ChannelGuide.MixerVolumeBarLeft.Height * ChannelGuide.MixerVolumeBarLeft.Height;
        masterRmsLeft += ChannelTouch.MixerVolumeBarLeft.Height * ChannelTouch.MixerVolumeBarLeft.Height;
        masterRmsLeft += ChannelChain.MixerVolumeBarLeft.Height * ChannelChain.MixerVolumeBarLeft.Height;
        masterRmsLeft += ChannelHold.MixerVolumeBarLeft.Height * ChannelHold.MixerVolumeBarLeft.Height;
        masterRmsLeft += ChannelHoldLoop.MixerVolumeBarLeft.Height * ChannelHoldLoop.MixerVolumeBarLeft.Height;
        masterRmsLeft += ChannelSnap.MixerVolumeBarLeft.Height * ChannelSnap.MixerVolumeBarLeft.Height;
        masterRmsLeft += ChannelSlide.MixerVolumeBarLeft.Height * ChannelSlide.MixerVolumeBarLeft.Height;
        masterRmsLeft += ChannelBonus.MixerVolumeBarLeft.Height * ChannelBonus.MixerVolumeBarLeft.Height;
        masterRmsLeft += ChannelR.MixerVolumeBarLeft.Height * ChannelR.MixerVolumeBarLeft.Height;
        masterRmsLeft += ChannelStartClick.MixerVolumeBarLeft.Height * ChannelStartClick.MixerVolumeBarLeft.Height;
        masterRmsLeft += ChannelMetronome.MixerVolumeBarLeft.Height * ChannelMetronome.MixerVolumeBarLeft.Height;
        masterRmsLeft = Math.Sqrt(masterRmsLeft);
        
        double masterRmsRight = ChannelAudio.MixerVolumeBarRight.Height * ChannelAudio.MixerVolumeBarRight.Height;
        masterRmsRight += ChannelGuide.MixerVolumeBarRight.Height * ChannelGuide.MixerVolumeBarRight.Height;
        masterRmsRight += ChannelTouch.MixerVolumeBarRight.Height * ChannelTouch.MixerVolumeBarRight.Height;
        masterRmsRight += ChannelChain.MixerVolumeBarRight.Height * ChannelChain.MixerVolumeBarRight.Height;
        masterRmsRight += ChannelHold.MixerVolumeBarRight.Height * ChannelHold.MixerVolumeBarRight.Height;
        masterRmsRight += ChannelHoldLoop.MixerVolumeBarRight.Height * ChannelHoldLoop.MixerVolumeBarRight.Height;
        masterRmsRight += ChannelSnap.MixerVolumeBarRight.Height * ChannelSnap.MixerVolumeBarRight.Height;
        masterRmsRight += ChannelSlide.MixerVolumeBarRight.Height * ChannelSlide.MixerVolumeBarRight.Height;
        masterRmsRight += ChannelBonus.MixerVolumeBarRight.Height * ChannelBonus.MixerVolumeBarRight.Height;
        masterRmsRight += ChannelR.MixerVolumeBarRight.Height * ChannelR.MixerVolumeBarRight.Height;
        masterRmsRight += ChannelStartClick.MixerVolumeBarRight.Height * ChannelStartClick.MixerVolumeBarRight.Height;
        masterRmsRight += ChannelMetronome.MixerVolumeBarRight.Height * ChannelMetronome.MixerVolumeBarRight.Height;
        masterRmsRight = Math.Sqrt(masterRmsRight);
        
        ChannelMaster.MixerVolumeBarLeft.Height = masterRmsLeft * masterVolume;
        ChannelMaster.MixerVolumeBarRight.Height = masterRmsRight * masterVolume;
        
        return;

        double getLevel(float? level, double? volume, double currentHeight)
        {
            const double maxHeight = 175;
            
            currentHeight = Math.Max(currentHeight, maxHeight * (level ?? 0) * (volume ?? 0));
            currentHeight -= TimeSystem.TickInterval * 0.2f;
            return Math.Clamp(currentHeight, 0, maxHeight);
        }
    }
    
    private void ChannelSliderVolume_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not Slider slider) return;

        int value = SliderValueToDecibels(slider.Value);
        
        if      (ReferenceEquals(sender, ChannelMaster.SliderVolume))     { SettingsSystem.AudioSettings.MasterVolume     = value; }
        else if (ReferenceEquals(sender, ChannelAudio.SliderVolume))      { SettingsSystem.AudioSettings.AudioVolume      = value; }
        else if (ReferenceEquals(sender, ChannelGuide.SliderVolume))      { SettingsSystem.AudioSettings.GuideVolume      = value; }
        else if (ReferenceEquals(sender, ChannelTouch.SliderVolume))      { SettingsSystem.AudioSettings.TouchVolume      = value; }
        else if (ReferenceEquals(sender, ChannelChain.SliderVolume))      { SettingsSystem.AudioSettings.ChainVolume      = value; }
        else if (ReferenceEquals(sender, ChannelHold.SliderVolume))       { SettingsSystem.AudioSettings.HoldVolume       = value; }
        else if (ReferenceEquals(sender, ChannelHoldLoop.SliderVolume))   { SettingsSystem.AudioSettings.HoldLoopVolume   = value; }
        else if (ReferenceEquals(sender, ChannelSnap.SliderVolume))       { SettingsSystem.AudioSettings.SnapVolume       = value; }
        else if (ReferenceEquals(sender, ChannelSlide.SliderVolume))      { SettingsSystem.AudioSettings.SlideVolume      = value; }
        else if (ReferenceEquals(sender, ChannelBonus.SliderVolume))      { SettingsSystem.AudioSettings.BonusVolume      = value; }
        else if (ReferenceEquals(sender, ChannelR.SliderVolume))          { SettingsSystem.AudioSettings.RVolume          = value; }
        else if (ReferenceEquals(sender, ChannelStartClick.SliderVolume)) { SettingsSystem.AudioSettings.StartClickVolume = value; }
        else if (ReferenceEquals(sender, ChannelMetronome.SliderVolume))  { SettingsSystem.AudioSettings.MetronomeVolume  = value; }
    }

    private void ChannelButtonMute_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not ToggleButton button) return;

        bool value = button.IsChecked ?? false;
        
        if      (ReferenceEquals(sender, ChannelMaster.ButtonMute))     { SettingsSystem.AudioSettings.MuteMaster     = value; }
        else if (ReferenceEquals(sender, ChannelAudio.ButtonMute))      { SettingsSystem.AudioSettings.MuteAudio      = value; }
        else if (ReferenceEquals(sender, ChannelGuide.ButtonMute))      { SettingsSystem.AudioSettings.MuteGuide      = value; }
        else if (ReferenceEquals(sender, ChannelTouch.ButtonMute))      { SettingsSystem.AudioSettings.MuteTouch      = value; }
        else if (ReferenceEquals(sender, ChannelChain.ButtonMute))      { SettingsSystem.AudioSettings.MuteChain      = value; }
        else if (ReferenceEquals(sender, ChannelHold.ButtonMute))       { SettingsSystem.AudioSettings.MuteHold       = value; }
        else if (ReferenceEquals(sender, ChannelHoldLoop.ButtonMute))   { SettingsSystem.AudioSettings.MuteHoldLoop   = value; }
        else if (ReferenceEquals(sender, ChannelSnap.ButtonMute))       { SettingsSystem.AudioSettings.MuteSnap       = value; }
        else if (ReferenceEquals(sender, ChannelSlide.ButtonMute))      { SettingsSystem.AudioSettings.MuteSlide      = value; }
        else if (ReferenceEquals(sender, ChannelBonus.ButtonMute))      { SettingsSystem.AudioSettings.MuteBonus      = value; }
        else if (ReferenceEquals(sender, ChannelR.ButtonMute))          { SettingsSystem.AudioSettings.MuteR          = value; }
        else if (ReferenceEquals(sender, ChannelStartClick.ButtonMute)) { SettingsSystem.AudioSettings.MuteStartClick = value; }
        else if (ReferenceEquals(sender, ChannelMetronome.ButtonMute))  { SettingsSystem.AudioSettings.MuteMetronome  = value; }
    }
    
    private async void ChannelButtonSound_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        string path = "";
        
        try
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            // Open file picker.
            IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new()
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
        
        if      (ReferenceEquals(sender, ChannelGuide.ButtonSound))      { SettingsSystem.AudioSettings.HitsoundGuidePath      = path; }
        else if (ReferenceEquals(sender, ChannelTouch.ButtonSound))      { SettingsSystem.AudioSettings.HitsoundTouchPath      = path; }
        else if (ReferenceEquals(sender, ChannelChain.ButtonSound))      { SettingsSystem.AudioSettings.HitsoundChainPath      = path; }
        else if (ReferenceEquals(sender, ChannelHold.ButtonSound))       { SettingsSystem.AudioSettings.HitsoundHoldPath       = path; }
        else if (ReferenceEquals(sender, ChannelHoldLoop.ButtonSound))   { SettingsSystem.AudioSettings.HitsoundHoldLoopPath   = path; }
        else if (ReferenceEquals(sender, ChannelSnap.ButtonSound))       { SettingsSystem.AudioSettings.HitsoundSnapPath       = path; }
        else if (ReferenceEquals(sender, ChannelSlide.ButtonSound))      { SettingsSystem.AudioSettings.HitsoundSlidePath      = path; }
        else if (ReferenceEquals(sender, ChannelBonus.ButtonSound))      { SettingsSystem.AudioSettings.HitsoundBonusPath      = path; }
        else if (ReferenceEquals(sender, ChannelR.ButtonSound))          { SettingsSystem.AudioSettings.HitsoundRPath          = path; }
        else if (ReferenceEquals(sender, ChannelStartClick.ButtonSound)) { SettingsSystem.AudioSettings.HitsoundStartClickPath = path; }
        else if (ReferenceEquals(sender, ChannelMetronome.ButtonSound))  { SettingsSystem.AudioSettings.HitsoundMetronomePath  = path; }
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