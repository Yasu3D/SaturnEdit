using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        EditorNoteSound selectedNoteSound = NoteSoundSystem.SelectedNoteSound;
        
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            TextBoxName.Text = selectedNoteSound.Name;
            
            NumericUpDownHoldLoopStart.Value = (decimal)selectedNoteSound.HoldLoopStart;
            NumericUpDownHoldLoopEnd.Value = (decimal)selectedNoteSound.HoldLoopEnd;

            if (selectedNoteSound == NoteSoundSystem.DefaultNoteSound)
            {
                ButtonDeleteNoteSound.IsEnabled = false;
                
                TextBoxName.IsEnabled = false;
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

                NumericUpDownHoldLoopStart.IsEnabled = false;
                NumericUpDownHoldLoopEnd.IsEnabled = false;
                
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
                TextBoxName.IsEnabled = true;
                ButtonDeleteNoteSound.IsEnabled = true;
                
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
                
                NumericUpDownHoldLoopStart.IsEnabled = true;
                NumericUpDownHoldLoopEnd.IsEnabled = true;
                
                TextBoxGuide.Text      = selectedNoteSound.HitsoundGuidePath;
                TextBoxTouch.Text      = selectedNoteSound.HitsoundTouchPath;
                TextBoxHold.Text       = selectedNoteSound.HitsoundHoldPath;
                TextBoxSlide.Text      = selectedNoteSound.HitsoundSlidePath;
                TextBoxBonus.Text      = selectedNoteSound.HitsoundBonusPath;
                TextBoxR.Text          = selectedNoteSound.HitsoundRPath;
                TextBoxStartClick.Text = selectedNoteSound.HitsoundStartClickPath;
                TextBoxMetronome.Text  = selectedNoteSound.HitsoundMetronomePath;
                
                IconSoundGuideNotFound.IsVisible      = selectedNoteSound.HitsoundGuidePath      != "" && !File.Exists(selectedNoteSound.HitsoundGuidePath);
                IconSoundTouchNotFound.IsVisible      = selectedNoteSound.HitsoundTouchPath      != "" && !File.Exists(selectedNoteSound.HitsoundTouchPath);
                IconSoundHoldNotFound.IsVisible       = selectedNoteSound.HitsoundHoldPath       != "" && !File.Exists(selectedNoteSound.HitsoundHoldPath);
                IconSoundSlideNotFound.IsVisible      = selectedNoteSound.HitsoundSlidePath      != "" && !File.Exists(selectedNoteSound.HitsoundSlidePath);
                IconSoundBonusNotFound.IsVisible      = selectedNoteSound.HitsoundBonusPath      != "" && !File.Exists(selectedNoteSound.HitsoundBonusPath);
                IconSoundRNotFound.IsVisible          = selectedNoteSound.HitsoundRPath          != "" && !File.Exists(selectedNoteSound.HitsoundRPath);
                IconSoundStartClickNotFound.IsVisible = selectedNoteSound.HitsoundStartClickPath != "" && !File.Exists(selectedNoteSound.HitsoundStartClickPath);
                IconSoundMetronomeNotFound.IsVisible  = selectedNoteSound.HitsoundMetronomePath  != "" && !File.Exists(selectedNoteSound.HitsoundMetronomePath);
            }

            ComboBoxNoteSoundList.Items.Clear();
            
            for (int i = -1; i < NoteSoundSystem.NoteSounds.Items.Count; i++)
            {
                EditorNoteSound pack = i == -1 
                    ? NoteSoundSystem.DefaultNoteSound 
                    : NoteSoundSystem.NoteSounds.Items[i];

                ComboBoxItem item = new() { Content = pack.Name, Tag = pack };
                ComboBoxNoteSoundList.Items.Add(item);
            }

            // Set selection.
            ComboBoxNoteSoundList.SelectedItem = ComboBoxNoteSoundList.Items.FirstOrDefault(x => x is ComboBoxItem item && item.Tag == selectedNoteSound);
            
            blockEvents = false;
        });
    }
#endregion System Event Handlers

#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        NoteSoundSystem.NoteSoundPropertyChanged += OnSettingsChanged;
        
        OnSettingsChanged(null, EventArgs.Empty);
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        SettingsSystem.SettingsChanged -= OnSettingsChanged;
        NoteSoundSystem.NoteSoundPropertyChanged -= OnSettingsChanged;
        
        base.OnUnloaded(e);
    }

    private void ComboBoxNoteSoundList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxNoteSoundList.SelectedItem is not ComboBoxItem comboBoxItem) return;
        if (comboBoxItem.Tag is not EditorNoteSound editorNoteSound) return;

        SettingsSystem.AudioSettings.NoteSoundId = editorNoteSound.Id;
    }
    
    private void ButtonAddNoteSound_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            EditorNoteSound noteSound = new() { Name = "New Note Sound", Id = Guid.NewGuid().ToString() };
            NoteSoundSystem.AddNoteSound(noteSound);
            SettingsSystem.AudioSettings.NoteSoundId = noteSound.Id;
        }
        catch (Exception ex)
        {
            // Don't throw.
            LoggingSystem.WriteSessionLog(ex.ToString());
        }
    }

    private async void ButtonRemoveNoteSound_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (VisualRoot is not Window window) return;
            if (NoteSoundSystem.SelectedNoteSound == NoteSoundSystem.DefaultNoteSound) return;

            ModalDialogWindow modalDialog = new()
            {
                DialogIcon = Icon.Delete,
                WindowTitleKey = "ModalDialog.DeleteNoteSound.Title",
                HeaderKey = "ModalDialog.DeleteNoteSound.Header",
                ParagraphKey = "ModalDialog.DeleteNoteSound.Paragraph",
                ButtonPrimaryKey = "Generic.Delete",
                ButtonSecondaryKey = "Generic.Cancel",
            };
            
            modalDialog.Position = MainWindow.DialogPopupPosition(modalDialog.Width, modalDialog.Height);

            modalDialog.InitializeDialog();
            await modalDialog.ShowDialog(window);

            if (modalDialog.Result != ModalDialogResult.Primary) return;
            
            EditorNoteSound selectedNoteSound = NoteSoundSystem.SelectedNoteSound;
            int index = NoteSoundSystem.NoteSounds.Items.IndexOf(selectedNoteSound);
            
            NoteSoundSystem.RemoveNoteSound(selectedNoteSound);
            index = Math.Clamp(index, -1, NoteSoundSystem.NoteSounds.Items.Count - 1);

            SettingsSystem.AudioSettings.NoteSoundId = index == -1 ? "" : NoteSoundSystem.NoteSounds.Items[index].Id;
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

            EditorNoteSound selectedNoteSound = NoteSoundSystem.SelectedNoteSound;
            
            if (selectedNoteSound == NoteSoundSystem.DefaultNoteSound) return;
            
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
                selectedNoteSound.HitsoundGuidePath = path;
            }
            else if (button == ButtonPickSoundTouch)
            {
                selectedNoteSound.HitsoundTouchPath = path;
            }
            else if (button == ButtonPickSoundHold)
            {
                selectedNoteSound.HitsoundHoldPath = path;
            }
            else if (button == ButtonPickSoundSlide)
            {
                selectedNoteSound.HitsoundSlidePath = path;
            }
            else if (button == ButtonPickSoundBonus)
            {
                selectedNoteSound.HitsoundBonusPath = path;
            }
            else if (button == ButtonPickSoundR)
            {
                selectedNoteSound.HitsoundRPath = path;
            }
            else if (button == ButtonPickSoundStartClick)
            {
                selectedNoteSound.HitsoundStartClickPath = path;
            }
            else if (button == ButtonPickSoundMetronome)
            {
                selectedNoteSound.HitsoundMetronomePath = path;
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
            
            EditorNoteSound selectedNoteSound = NoteSoundSystem.SelectedNoteSound;
            
            if (selectedNoteSound == NoteSoundSystem.DefaultNoteSound) return;
            selectedNoteSound.Name = textBox.Text ?? "";
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
            
            EditorNoteSound selectedNoteSound = NoteSoundSystem.SelectedNoteSound;
            
            if (selectedNoteSound == NoteSoundSystem.DefaultNoteSound) return;
            
            string path = textBox.Text ?? "";
            
            if      (textBox == TextBoxGuide)
            {
                selectedNoteSound.HitsoundGuidePath = path;
            }
            else if (textBox == TextBoxTouch)
            {
                selectedNoteSound.HitsoundTouchPath = path;
            }
            else if (textBox == TextBoxHold)
            {
                selectedNoteSound.HitsoundHoldPath = path;
            }
            else if (textBox == TextBoxSlide)
            {
                selectedNoteSound.HitsoundSlidePath = path;
            }
            else if (textBox == TextBoxBonus)
            {
                selectedNoteSound.HitsoundBonusPath = path;
            }
            else if (textBox == TextBoxR)
            {
                selectedNoteSound.HitsoundRPath = path;
            }
            else if (textBox == TextBoxStartClick)
            {
                selectedNoteSound.HitsoundStartClickPath = path;
            }
            else if (textBox == TextBoxMetronome)
            {
                selectedNoteSound.HitsoundMetronomePath = path;
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
        
        EditorNoteSound selectedNoteSound = NoteSoundSystem.SelectedNoteSound;
        
        if (selectedNoteSound == NoteSoundSystem.DefaultNoteSound) return;
        selectedNoteSound.HoldLoopStart = (float?)NumericUpDownHoldLoopStart.Value ?? 0;
    }

    private void NumericUpDownHoldLoopEnd_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (NumericUpDownHoldLoopEnd == null) return;
        
        EditorNoteSound selectedNoteSound = NoteSoundSystem.SelectedNoteSound;
        
        if (selectedNoteSound == NoteSoundSystem.DefaultNoteSound) return;
        selectedNoteSound.HoldLoopEnd = (float?)NumericUpDownHoldLoopEnd.Value ?? 0;
    }
#endregion UI Event Handlers
}