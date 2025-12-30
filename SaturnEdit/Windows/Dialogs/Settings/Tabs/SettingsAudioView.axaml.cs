using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SaturnEdit.Systems;

namespace SaturnEdit.Windows.Dialogs.Settings.Tabs;

public partial class SettingsAudioView : UserControl
{
    public SettingsAudioView()
    {
        InitializeComponent();
    }
    
    private bool blockEvents = false;
    
#region System Event Handlers
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            NumericUpDownHoldLoopStart.Value = (decimal)SettingsSystem.AudioSettings.HoldLoopStart;
            NumericUpDownHoldLoopEnd.Value = (decimal)SettingsSystem.AudioSettings.HoldLoopEnd;
            
            TextBoxGuide.Text      = SettingsSystem.AudioSettings.HitsoundGuidePath;
            TextBoxTouch.Text      = SettingsSystem.AudioSettings.HitsoundTouchPath;
            TextBoxHold.Text       = SettingsSystem.AudioSettings.HitsoundHoldPath;
            TextBoxSlide.Text      = SettingsSystem.AudioSettings.HitsoundSlidePath;
            TextBoxBonus.Text      = SettingsSystem.AudioSettings.HitsoundBonusPath;
            TextBoxR.Text          = SettingsSystem.AudioSettings.HitsoundRPath;
            TextBoxStartClick.Text = SettingsSystem.AudioSettings.HitsoundStartClickPath;
            TextBoxMetronome.Text  = SettingsSystem.AudioSettings.HitsoundMetronomePath;
            
            IconSoundGuideNotFound.IsVisible      = SettingsSystem.AudioSettings.HitsoundGuidePath      == "" || !File.Exists(SettingsSystem.AudioSettings.HitsoundGuidePath);
            IconSoundTouchNotFound.IsVisible      = SettingsSystem.AudioSettings.HitsoundTouchPath      == "" || !File.Exists(SettingsSystem.AudioSettings.HitsoundTouchPath);
            IconSoundHoldNotFound.IsVisible       = SettingsSystem.AudioSettings.HitsoundHoldPath       == "" || !File.Exists(SettingsSystem.AudioSettings.HitsoundHoldPath);
            IconSoundSlideNotFound.IsVisible      = SettingsSystem.AudioSettings.HitsoundSlidePath      == "" || !File.Exists(SettingsSystem.AudioSettings.HitsoundSlidePath);
            IconSoundBonusNotFound.IsVisible      = SettingsSystem.AudioSettings.HitsoundBonusPath      == "" || !File.Exists(SettingsSystem.AudioSettings.HitsoundBonusPath);
            IconSoundRNotFound.IsVisible          = SettingsSystem.AudioSettings.HitsoundRPath          == "" || !File.Exists(SettingsSystem.AudioSettings.HitsoundRPath);
            IconSoundStartClickNotFound.IsVisible = SettingsSystem.AudioSettings.HitsoundStartClickPath == "" || !File.Exists(SettingsSystem.AudioSettings.HitsoundStartClickPath);
            IconSoundMetronomeNotFound.IsVisible  = SettingsSystem.AudioSettings.HitsoundMetronomePath  == "" || !File.Exists(SettingsSystem.AudioSettings.HitsoundMetronomePath);
            
            blockEvents = false;
        });
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
    
    private void NumericUpDownHoldLoopStart_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (NumericUpDownHoldLoopStart == null) return;

        SettingsSystem.AudioSettings.HoldLoopStart = (float?)NumericUpDownHoldLoopStart.Value ?? 0;
    }

    private void NumericUpDownHoldLoopEnd_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (NumericUpDownHoldLoopEnd == null) return;
        
        SettingsSystem.AudioSettings.HoldLoopEnd = (float?)NumericUpDownHoldLoopEnd.Value ?? 0;
    }
    
    private async void ButtonPickSound_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (sender is not Button button) return;
            if (VisualRoot is not Window window) return;
            
            string path = "";

            try
            {
                // Open file picker.
                IReadOnlyList<IStorageFile> files = await window.StorageProvider.OpenFilePickerAsync(new()
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
    
    private void TextBoxSound_LostFocus(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (sender is not TextBox textBox) return;
            
            string path = textBox.Text ?? "";
            
            if      (textBox == TextBoxGuide)
            {
                SettingsSystem.AudioSettings.HitsoundGuidePath = path;
            }
            else if (textBox == TextBoxTouch)
            {
                SettingsSystem.AudioSettings.HitsoundTouchPath = path;
            }
            else if (textBox == TextBoxHold)
            {
                SettingsSystem.AudioSettings.HitsoundHoldPath = path;
            }
            else if (textBox == TextBoxSlide)
            {
                SettingsSystem.AudioSettings.HitsoundSlidePath = path;
            }
            else if (textBox == TextBoxBonus)
            {
                SettingsSystem.AudioSettings.HitsoundBonusPath = path;
            }
            else if (textBox == TextBoxR)
            {
                SettingsSystem.AudioSettings.HitsoundRPath = path;
            }
            else if (textBox == TextBoxStartClick)
            {
                SettingsSystem.AudioSettings.HitsoundStartClickPath = path;
            }
            else if (textBox == TextBoxMetronome)
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
#endregion UI Event Handlers
}