using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FluentIcons.Common;
using SaturnEdit.Systems;
using SaturnEdit.Windows.Dialogs.ModalDialog;

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

            TextBoxName.Text = SoundpackSystem.SelectedSoundpack.Name;
            
            NumericUpDownHoldLoopStart.Value = (decimal)SoundpackSystem.SelectedSoundpack.HoldLoopStart;
            NumericUpDownHoldLoopEnd.Value = (decimal)SoundpackSystem.SelectedSoundpack.HoldLoopEnd;

            if (SoundpackSystem.SelectedSoundpack == SoundpackSystem.DefaultSoundpack)
            {
                ButtonDeleteSoundpack.IsEnabled = false;
                
                TextBoxGuide.IsEnabled = false;
                TextBoxTouch.IsEnabled = false;
                TextBoxHold.IsEnabled = false;
                TextBoxSlide.IsEnabled = false;
                TextBoxBonus.IsEnabled = false;
                TextBoxR.IsEnabled = false;
                TextBoxStartClick.IsEnabled = false;
                TextBoxMetronome.IsEnabled = false;
                
                ButtonPickSoundGuide.IsEnabled = false;
                ButtonPickSoundTouch.IsEnabled = false;
                ButtonPickSoundHold.IsEnabled = false;
                ButtonPickSoundSlide.IsEnabled = false;
                ButtonPickSoundBonus.IsEnabled = false;
                ButtonPickSoundR.IsEnabled = false;
                ButtonPickSoundStartClick.IsEnabled = false;
                ButtonPickSoundMetronome.IsEnabled = false;
                
                TextBoxGuide.Text      = "-";
                TextBoxTouch.Text      = "-";
                TextBoxHold.Text       = "-";
                TextBoxSlide.Text      = "-";
                TextBoxBonus.Text      = "-";
                TextBoxR.Text          = "-";
                TextBoxStartClick.Text = "-";
                TextBoxMetronome.Text  = "-";
                
                IconSoundGuideNotFound.IsVisible      = false;
                IconSoundTouchNotFound.IsVisible      = false;
                IconSoundHoldNotFound.IsVisible       = false;
                IconSoundSlideNotFound.IsVisible      = false;
                IconSoundBonusNotFound.IsVisible      = false;
                IconSoundRNotFound.IsVisible          = false;
                IconSoundStartClickNotFound.IsVisible = false;
                IconSoundMetronomeNotFound.IsVisible  = false;
            }
            else
            { 
                ButtonDeleteSoundpack.IsEnabled = true;
                
                TextBoxGuide.IsEnabled = true;
                TextBoxTouch.IsEnabled = true;
                TextBoxHold.IsEnabled = true;
                TextBoxSlide.IsEnabled = true;
                TextBoxBonus.IsEnabled = true;
                TextBoxR.IsEnabled = true;
                TextBoxStartClick.IsEnabled = true;
                TextBoxMetronome.IsEnabled = true;
                
                ButtonPickSoundGuide.IsEnabled = true;
                ButtonPickSoundTouch.IsEnabled = true;
                ButtonPickSoundHold.IsEnabled = true;
                ButtonPickSoundSlide.IsEnabled = true;
                ButtonPickSoundBonus.IsEnabled = true;
                ButtonPickSoundR.IsEnabled = true;
                ButtonPickSoundStartClick.IsEnabled = true;
                ButtonPickSoundMetronome.IsEnabled = true;
                
                TextBoxGuide.Text      = SoundpackSystem.SelectedSoundpack.HitsoundGuidePath;
                TextBoxTouch.Text      = SoundpackSystem.SelectedSoundpack.HitsoundTouchPath;
                TextBoxHold.Text       = SoundpackSystem.SelectedSoundpack.HitsoundHoldPath;
                TextBoxSlide.Text      = SoundpackSystem.SelectedSoundpack.HitsoundSlidePath;
                TextBoxBonus.Text      = SoundpackSystem.SelectedSoundpack.HitsoundBonusPath;
                TextBoxR.Text          = SoundpackSystem.SelectedSoundpack.HitsoundRPath;
                TextBoxStartClick.Text = SoundpackSystem.SelectedSoundpack.HitsoundStartClickPath;
                TextBoxMetronome.Text  = SoundpackSystem.SelectedSoundpack.HitsoundMetronomePath;
                
                IconSoundGuideNotFound.IsVisible      = SoundpackSystem.SelectedSoundpack.HitsoundGuidePath      != "" && !File.Exists(SoundpackSystem.SelectedSoundpack.HitsoundGuidePath);
                IconSoundTouchNotFound.IsVisible      = SoundpackSystem.SelectedSoundpack.HitsoundTouchPath      != "" && !File.Exists(SoundpackSystem.SelectedSoundpack.HitsoundTouchPath);
                IconSoundHoldNotFound.IsVisible       = SoundpackSystem.SelectedSoundpack.HitsoundHoldPath       != "" && !File.Exists(SoundpackSystem.SelectedSoundpack.HitsoundHoldPath);
                IconSoundSlideNotFound.IsVisible      = SoundpackSystem.SelectedSoundpack.HitsoundSlidePath      != "" && !File.Exists(SoundpackSystem.SelectedSoundpack.HitsoundSlidePath);
                IconSoundBonusNotFound.IsVisible      = SoundpackSystem.SelectedSoundpack.HitsoundBonusPath      != "" && !File.Exists(SoundpackSystem.SelectedSoundpack.HitsoundBonusPath);
                IconSoundRNotFound.IsVisible          = SoundpackSystem.SelectedSoundpack.HitsoundRPath          != "" && !File.Exists(SoundpackSystem.SelectedSoundpack.HitsoundRPath);
                IconSoundStartClickNotFound.IsVisible = SoundpackSystem.SelectedSoundpack.HitsoundStartClickPath != "" && !File.Exists(SoundpackSystem.SelectedSoundpack.HitsoundStartClickPath);
                IconSoundMetronomeNotFound.IsVisible  = SoundpackSystem.SelectedSoundpack.HitsoundMetronomePath  != "" && !File.Exists(SoundpackSystem.SelectedSoundpack.HitsoundMetronomePath);
            }

            ComboBoxSoundpackList.Items.Clear();
            
            for (int i = -1; i < SoundpackSystem.Soundpacks.Count; i++)
            {
                Soundpack pack = i == -1 
                    ? SoundpackSystem.DefaultSoundpack 
                    : SoundpackSystem.Soundpacks[i];

                ComboBoxItem item = new() { Content = pack.Name, };
                ComboBoxSoundpackList.Items.Add(item);
            }

            // Set selection.
            ComboBoxSoundpackList.SelectedIndex = SettingsSystem.AudioSettings.SelectedSoundpackIndex + 1;
            
            blockEvents = false;
        });
    }
#endregion System Event Handlers

#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        SoundpackSystem.SoundpackPropertyChanged += OnSettingsChanged;
        
        OnSettingsChanged(null, EventArgs.Empty);
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        SettingsSystem.SettingsChanged -= OnSettingsChanged;
        SoundpackSystem.SoundpackPropertyChanged -= OnSettingsChanged;
        
        base.OnUnloaded(e);
    }

    private void ComboBoxSoundpackList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        
        SettingsSystem.AudioSettings.SelectedSoundpackIndex = ComboBoxSoundpackList.SelectedIndex - 1;
    }
    
    private void ButtonAddSoundpack_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            Soundpack pack = new() { Name = "New Sound Pack" };
            SoundpackSystem.AddSoundpack(pack);
            SettingsSystem.AudioSettings.SelectedSoundpackIndex = SoundpackSystem.Soundpacks.Count - 1;
        }
        catch (Exception ex)
        {
            // Don't throw.
            LoggingSystem.WriteSessionLog(ex.ToString());
        }
    }

    private async void ButtonRemoveSoundpack_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (VisualRoot is not Window window) return;
            if (SoundpackSystem.SelectedSoundpack == SoundpackSystem.DefaultSoundpack) return;

            ModalDialogWindow modalDialog = new()
            {
                DialogIcon = Icon.Delete,
                WindowTitleKey = "ModalDialog.DeleteSoundpack.Title",
                HeaderKey = "ModalDialog.DeleteSoundpack.Header",
                ParagraphKey = "ModalDialog.DeleteSoundpack.Paragraph",
                ButtonPrimaryKey = "Generic.Delete",
                ButtonSecondaryKey = "Generic.Cancel",
            };
            
            modalDialog.Position = MainWindow.DialogPopupPosition(modalDialog.Width, modalDialog.Height);

            modalDialog.InitializeDialog();
            await modalDialog.ShowDialog(window);

            if (modalDialog.Result != ModalDialogResult.Primary) return;
            
            Soundpack pack = SoundpackSystem.SelectedSoundpack;
            SoundpackSystem.RemoveSoundpack(pack);

            SettingsSystem.AudioSettings.SelectedSoundpackIndex = Math.Clamp(SettingsSystem.AudioSettings.SelectedSoundpackIndex, -1, SoundpackSystem.Soundpacks.Count - 1);
        }
        catch (Exception ex)
        {
            // Don't throw.
            LoggingSystem.WriteSessionLog(ex.ToString());
        }
    }
    
    private async void ButtonPickSound_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (sender is not Button button) return;
            if (VisualRoot is not Window window) return;

            if (SoundpackSystem.SelectedSoundpack == SoundpackSystem.DefaultSoundpack) return;
            
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
                SoundpackSystem.SelectedSoundpack.HitsoundGuidePath = path;
            }
            else if (button == ButtonPickSoundTouch)
            {
                SoundpackSystem.SelectedSoundpack.HitsoundTouchPath = path;
            }
            else if (button == ButtonPickSoundHold)
            {
                SoundpackSystem.SelectedSoundpack.HitsoundHoldPath = path;
            }
            else if (button == ButtonPickSoundSlide)
            {
                SoundpackSystem.SelectedSoundpack.HitsoundSlidePath = path;
            }
            else if (button == ButtonPickSoundBonus)
            {
                SoundpackSystem.SelectedSoundpack.HitsoundBonusPath = path;
            }
            else if (button == ButtonPickSoundR)
            {
                SoundpackSystem.SelectedSoundpack.HitsoundRPath = path;
            }
            else if (button == ButtonPickSoundStartClick)
            {
                SoundpackSystem.SelectedSoundpack.HitsoundStartClickPath = path;
            }
            else if (button == ButtonPickSoundMetronome)
            {
                SoundpackSystem.SelectedSoundpack.HitsoundMetronomePath = path;
            }
        }
        catch (Exception ex)
        {
            // Don't throw.
            LoggingSystem.WriteSessionLog(ex.ToString());
        }
    }
    
    private void TextBoxName_LostFocus(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (sender is not TextBox textBox) return;
            
            if (SoundpackSystem.SelectedSoundpack == SoundpackSystem.DefaultSoundpack) return;
            SoundpackSystem.SelectedSoundpack.Name = textBox.Text ?? "";
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
            
            if (SoundpackSystem.SelectedSoundpack == SoundpackSystem.DefaultSoundpack) return;
            
            string path = textBox.Text ?? "";
            
            if      (textBox == TextBoxGuide)
            {
                SoundpackSystem.SelectedSoundpack.HitsoundGuidePath = path;
            }
            else if (textBox == TextBoxTouch)
            {
                SoundpackSystem.SelectedSoundpack.HitsoundTouchPath = path;
            }
            else if (textBox == TextBoxHold)
            {
                SoundpackSystem.SelectedSoundpack.HitsoundHoldPath = path;
            }
            else if (textBox == TextBoxSlide)
            {
                SoundpackSystem.SelectedSoundpack.HitsoundSlidePath = path;
            }
            else if (textBox == TextBoxBonus)
            {
                SoundpackSystem.SelectedSoundpack.HitsoundBonusPath = path;
            }
            else if (textBox == TextBoxR)
            {
                SoundpackSystem.SelectedSoundpack.HitsoundRPath = path;
            }
            else if (textBox == TextBoxStartClick)
            {
                SoundpackSystem.SelectedSoundpack.HitsoundStartClickPath = path;
            }
            else if (textBox == TextBoxMetronome)
            {
                SoundpackSystem.SelectedSoundpack.HitsoundMetronomePath = path;
            }
        }
        catch (Exception ex)
        {
            // Don't throw.
            LoggingSystem.WriteSessionLog(ex.ToString());
        }
    }
    
    private void NumericUpDownHoldLoopStart_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (NumericUpDownHoldLoopStart == null) return;
        
        if (SoundpackSystem.SelectedSoundpack == SoundpackSystem.DefaultSoundpack) return;

        SoundpackSystem.SelectedSoundpack.HoldLoopStart = (float?)NumericUpDownHoldLoopStart.Value ?? 0;
    }

    private void NumericUpDownHoldLoopEnd_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (NumericUpDownHoldLoopEnd == null) return;
        
        if (SoundpackSystem.SelectedSoundpack == SoundpackSystem.DefaultSoundpack) return;
        
        SoundpackSystem.SelectedSoundpack.HoldLoopEnd = (float?)NumericUpDownHoldLoopEnd.Value ?? 0;
    }
#endregion UI Event Handlers
}