using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using ExCSS;
using Kawazu;
using SaturnData.Notation.Core;
using SaturnData.Notation.Serialization;
using SaturnEdit.Systems;
using SaturnEdit.Windows.Dialogs.ImportArgs;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class ChartPropertiesView : UserControl
{
    public ChartPropertiesView()
    {
        InitializeComponent();

        ChartSystem.EntryChanged += OnEntryChanged;
        ChartSystem.ChartChanged += OnChartChanged;
        OnEntryChanged(null, EventArgs.Empty);
        OnChartChanged(null, EventArgs.Empty);
    }

    private bool blockEvent = false;
    
    private async void OnEntryChanged(object? sender, EventArgs e)
    {
        if (ChartSystem.Entry.AutoReading)
        {
            ChartSystem.Entry.Reading = await ChartSystem.Entry.GetAutoReading();
        }

        if (ChartSystem.Entry.AutoClearThreshold)
        {
            ChartSystem.Entry.ClearThreshold = ChartSystem.Entry.GetAutoClearThreshold();
        }
        
        blockEvent = true;

        TextBoxTitle.Text = ChartSystem.Entry.Title;
        TextBoxReading.Text = ChartSystem.Entry.Reading;
        TextBoxArtist.Text = ChartSystem.Entry.Artist;
        TextBoxBpmMessage.Text = ChartSystem.Entry.BpmMessage;
        
        TextBoxRevision.Text = ChartSystem.Entry.Revision;
        TextBoxNotesDesigner.Text = ChartSystem.Entry.NotesDesigner;
        ComboBoxDifficulty.SelectedIndex = (int)ChartSystem.Entry.Difficulty;
        TextBoxLevel.Text = ChartSystem.Entry.Level.ToString("F1", CultureInfo.InvariantCulture);
        TextBoxClearThreshold.Text = ChartSystem.Entry.ClearThreshold.ToString("F2", CultureInfo.InvariantCulture);
        NumericUpDownChartEndMeasure.Value = ChartSystem.Entry.ChartEnd.Measure;
        NumericUpDownChartEndTick.Value = ChartSystem.Entry.ChartEnd.Tick;
        
        TextBoxPreviewBegin.Text = (ChartSystem.Entry.PreviewBegin / 1000).ToString("F3", CultureInfo.InvariantCulture);
        TextBoxPreviewLength.Text = (ChartSystem.Entry.PreviewLength / 1000).ToString("F3", CultureInfo.InvariantCulture);
        ComboBoxBackground.SelectedIndex = (int)ChartSystem.Entry.Background;
        ComboBoxTutorialMode.SelectedIndex = ChartSystem.Entry.TutorialMode ? 1 : 0;

        TextBoxJacket.Text = Path.GetFileName(ChartSystem.Entry.JacketPath);
        TextBoxAudio.Text = Path.GetFileName(ChartSystem.Entry.AudioPath);
        TextBoxVideo.Text = Path.GetFileName(ChartSystem.Entry.VideoPath);
        TextBoxAudioOffset.Text = (ChartSystem.Entry.AudioOffset / 1000).ToString("F3", CultureInfo.InvariantCulture);
        TextBoxVideoOffset.Text = (ChartSystem.Entry.VideoOffset / 1000).ToString("F3", CultureInfo.InvariantCulture);
        
        ToggleButtonAutoReading.IsChecked = ChartSystem.Entry.AutoReading;
        ToggleButtonAutoBpmMessage.IsChecked = ChartSystem.Entry.AutoBpmMessage;
        ToggleButtonAutoClearThreshold.IsChecked = ChartSystem.Entry.AutoClearThreshold;
        ToggleButtonAutoChartEnd.IsChecked = ChartSystem.Entry.AutoChartEnd;

        TextBoxReading.IsEnabled = !ChartSystem.Entry.AutoReading;
        TextBoxBpmMessage.IsEnabled = !ChartSystem.Entry.AutoBpmMessage;
        TextBoxClearThreshold.IsEnabled = !ChartSystem.Entry.AutoClearThreshold;
        NumericUpDownChartEndMeasure.IsEnabled = !ChartSystem.Entry.AutoChartEnd;
        NumericUpDownChartEndTick.IsEnabled = !ChartSystem.Entry.AutoChartEnd;
        
        blockEvent = false;
        
        try
        {
            bool jacketExists = File.Exists(ChartSystem.Entry.JacketPath);
            ImageJacket.Source = jacketExists ? new Bitmap(ChartSystem.Entry.JacketPath) : null;
            
            ImageJacketPlaceholder.IsVisible = !jacketExists;
            ImageJacket.IsVisible = jacketExists;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ImageJacketPlaceholder.IsVisible = true;
            ImageJacket.IsVisible = false;
        }
    }

    private void OnChartChanged(object? sender, EventArgs e)
    {
        // TODO: Implement Auto BPM
    }
    
    private void TextBoxTitle_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        try
        {
            if (blockEvent) return;
            if (sender == null) return;
        
            ChartSystem.Entry.Title = TextBoxTitle.Text ?? "";
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void TextBoxReading_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;

        ChartSystem.Entry.Reading = TextBoxReading.Text ?? "";
    }

    private void ToggleButtonAutoReading_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;

        ChartSystem.Entry.AutoReading = ToggleButtonAutoReading.IsChecked ?? false;
    }

    private void TextBoxArtist_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;

        ChartSystem.Entry.Artist = TextBoxArtist.Text ?? "";
    }

    private void TextBoxBpmMessage_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;

        ChartSystem.Entry.BpmMessage = TextBoxBpmMessage.Text ?? "";
    }

    private void ToggleButtonAutoBpmMessage_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;

        ChartSystem.Entry.AutoBpmMessage = ToggleButtonAutoBpmMessage.IsChecked ?? false;
    }

    private void TextBoxRevision_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;

        ChartSystem.Entry.Revision = TextBoxRevision.Text ?? "";
    }

    private void TextBoxNotesDesigner_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;

        ChartSystem.Entry.NotesDesigner = TextBoxNotesDesigner.Text ?? "";
    }

    private void ComboBoxDifficulty_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;

        ChartSystem.Entry.Difficulty = (Difficulty)ComboBoxDifficulty.SelectedIndex;
    }

    private void TextBoxLevel_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;
        
        try
        {
            ChartSystem.Entry.Level = Convert.ToSingle(TextBoxLevel.Text ?? "", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            // Reset Value
            ChartSystem.Entry.Level = 0;
            OnEntryChanged(null, EventArgs.Empty);

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void TextBoxClearThreshold_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;

        try
        {
            ChartSystem.Entry.ClearThreshold = Convert.ToSingle(TextBoxClearThreshold.Text ?? "", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            // Reset Value
            ChartSystem.Entry.ClearThreshold = ChartSystem.Entry.GetAutoClearThreshold();
            OnEntryChanged(null, EventArgs.Empty);

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void ToggleButtonAutoClearThreshold_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;

        ChartSystem.Entry.AutoClearThreshold = ToggleButtonAutoClearThreshold.IsChecked ?? false;
    }

    private void NumericUpDownChartEndMeasure_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;
        
        ChartSystem.Entry.ChartEnd = new((int?)NumericUpDownChartEndMeasure.Value ?? 0, (int?)NumericUpDownChartEndTick.Value ?? 0);
    }

    private void NumericUpDownChartEndTick_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;

        blockEvent = true;
        if (NumericUpDownChartEndTick.Value == -1)
        {
            NumericUpDownChartEndMeasure.Value -= 1;
            NumericUpDownChartEndTick.Value = 1919;
        }

        if (NumericUpDownChartEndTick.Value == 1920)
        {
            NumericUpDownChartEndMeasure.Value += 1;
            NumericUpDownChartEndTick.Value = 0;
        }
        blockEvent = false;
        
        ChartSystem.Entry.ChartEnd = new((int?)NumericUpDownChartEndMeasure.Value ?? 0, (int?)NumericUpDownChartEndTick.Value ?? 0);
    }

    private void ToggleButtonAutoChartEnd_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;

        ChartSystem.Entry.AutoChartEnd = ToggleButtonAutoChartEnd.IsChecked ?? false;
    }

    private void TextBoxPreviewBegin_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;
        
        try
        {
            ChartSystem.Entry.PreviewBegin = 1000 * Convert.ToSingle(TextBoxPreviewBegin.Text ?? "", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            // Reset Value
            ChartSystem.Entry.PreviewBegin = 0;
            OnEntryChanged(null, EventArgs.Empty);

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void TextBoxPreviewLength_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;
        
        try
        {
            ChartSystem.Entry.PreviewLength = 1000 * Convert.ToSingle(TextBoxPreviewLength.Text ?? "", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            // Reset Value
            ChartSystem.Entry.PreviewLength = 10000;
            OnEntryChanged(null, EventArgs.Empty);

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void ComboBoxBackground_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;

        ChartSystem.Entry.Background = (BackgroundOption)ComboBoxBackground.SelectedIndex;
    }

    private void ComboBoxTutorialMode_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;

        ChartSystem.Entry.TutorialMode = ComboBoxTutorialMode.SelectedIndex == 1;
    }

    private void TextBoxJacket_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;

        ChartSystem.Entry.JacketPath = Path.Combine(Path.GetDirectoryName(ChartSystem.Entry.ChartPath) ?? "", TextBoxJacket.Text ?? "");
    }

    private void TextBoxAudio_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;
        
        ChartSystem.Entry.AudioPath = Path.Combine(Path.GetDirectoryName(ChartSystem.Entry.ChartPath) ?? "", TextBoxAudio.Text ?? "");
    }

    private void TextBoxVideo_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;
        
        ChartSystem.Entry.VideoPath = Path.Combine(Path.GetDirectoryName(ChartSystem.Entry.ChartPath) ?? "", TextBoxVideo.Text ?? "");
    }

    private void TextBoxAudioOffset_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;

        try
        {
            ChartSystem.Entry.AudioOffset = 1000 * Convert.ToSingle(TextBoxAudioOffset.Text ?? "", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            // Reset Value
            ChartSystem.Entry.AudioOffset = 0;
            OnEntryChanged(null, EventArgs.Empty);

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void TextBoxVideoOffset_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (sender == null) return;
        
        try
        {
            ChartSystem.Entry.VideoOffset = 1000 * Convert.ToSingle(TextBoxVideoOffset.Text ?? "", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            // Reset Value
            ChartSystem.Entry.VideoOffset = 0;
            OnEntryChanged(null, EventArgs.Empty);

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }
    
    // TODO: Validate File Existence, prompt to copy to chart directory?
    private async void ButtonPickFileJacket_OnClick(object? sender, RoutedEventArgs e)
    {
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
                    new("Image Files")
                    {
                        Patterns = ["*.png", "*.jpeg*", "*.jpg"],
                    },
                ],
            });
            if (files.Count != 1) return;

            ChartSystem.Entry.JacketPath = files[0].Path.LocalPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private async void ButtonPickFileAudio_OnClick(object? sender, RoutedEventArgs e)
    {
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

            ChartSystem.Entry.AudioPath = files[0].Path.LocalPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private async void ButtonPickFileVideo_OnClick(object? sender, RoutedEventArgs e)
    {
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
                    new("Image Files")
                    {
                        Patterns = ["*.mp4", "*.webm", "*.ogv"],
                    },
                ],
            });
            if (files.Count != 1) return;

            ChartSystem.Entry.VideoPath = files[0].Path.LocalPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}