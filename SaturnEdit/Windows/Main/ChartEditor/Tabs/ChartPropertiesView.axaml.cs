using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using FluentIcons.Common;
using SaturnData.Notation.Core;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.EntryOperations;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class ChartPropertiesView : UserControl
{
    public ChartPropertiesView()
    {
        InitializeComponent();
        
        ChartSystem.JacketChanged += OnJacketChanged;
        OnJacketChanged(null, EventArgs.Empty);

        UndoRedoSystem.OperationHistoryChanged += OnOperationHistoryChanged;
        OnOperationHistoryChanged(null, EventArgs.Empty);
    }

    private bool blockEvents = false;
    
    private void OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        blockEvents = true;

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

        TextBoxJacket.Text = ChartSystem.Entry.JacketFile;
        TextBoxAudio.Text = ChartSystem.Entry.AudioFile;
        TextBoxVideo.Text = ChartSystem.Entry.VideoFile;
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

        IconJacketFileNotFoundWarning.IsVisible = ChartSystem.Entry.JacketFile != "" && !File.Exists(ChartSystem.Entry.JacketPath);
        IconAudioFileNotFoundWarning.IsVisible =  ChartSystem.Entry.AudioFile  != "" && !File.Exists(ChartSystem.Entry.AudioPath);
        IconVideoFileNotFoundWarning.IsVisible =  ChartSystem.Entry.VideoFile  != "" && !File.Exists(ChartSystem.Entry.VideoPath);
        
        blockEvents = false;
    }

    private void OnJacketChanged(object? sender, EventArgs e)
    {
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
    
    
    
    private void TextBoxTitle_OnLostFocus(object? sender, RoutedEventArgs routedEventArgs)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        string oldValue = ChartSystem.Entry.Title;
        string newValue = TextBoxTitle.Text ?? "";
        if (oldValue == newValue) return;
        
        UndoRedoSystem.Push(new TitleEditOperation(oldValue, newValue));
    }

    private void TextBoxReading_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        string oldValue = ChartSystem.Entry.Reading;
        string newValue = TextBoxReading.Text ?? "";
        if (oldValue == newValue) return;
        
        UndoRedoSystem.Push(new ReadingEditOperation(oldValue, newValue));
    }

    private void ToggleButtonAutoReading_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        bool oldValue = ChartSystem.Entry.AutoReading;
        bool newValue = ToggleButtonAutoReading.IsChecked ?? false;
        if (oldValue == newValue) return;

        if (newValue == true)
        {
            AutoReadingEditOperation op0 = new(oldValue, newValue);
            BuildChartOperation op1 = new();
            UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
        }
        else
        {
            UndoRedoSystem.Push(new AutoReadingEditOperation(oldValue, newValue));
        }
    }

    private void TextBoxArtist_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        string oldValue = ChartSystem.Entry.Artist;
        string newValue = TextBoxArtist.Text ?? "";
        if (oldValue == newValue) return;
        
        UndoRedoSystem.Push(new ArtistEditOperation(oldValue, newValue));
    }

    private void TextBoxBpmMessage_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        string oldValue = ChartSystem.Entry.BpmMessage;
        string newValue = TextBoxBpmMessage.Text ?? "";
        if (oldValue == newValue) return;
        
        UndoRedoSystem.Push(new BpmMessageEditOperation(oldValue, newValue));
    }

    private void ToggleButtonAutoBpmMessage_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        bool oldValue = ChartSystem.Entry.AutoBpmMessage;
        bool newValue = ToggleButtonAutoBpmMessage.IsChecked ?? false;
        if (oldValue == newValue) return;
        
        if (newValue == true)
        {
            AutoBpmMessageEditOperation op0 = new (oldValue, newValue);
            BuildChartOperation op1 = new();
            UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
        }
        else
        {
            UndoRedoSystem.Push(new AutoBpmMessageEditOperation(oldValue, newValue));
        }
    }

    private void TextBoxRevision_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        string oldValue = ChartSystem.Entry.Revision;
        string newValue = TextBoxRevision.Text ?? "";
        if (oldValue == newValue) return;
        
        UndoRedoSystem.Push(new RevisionEditOperation(oldValue, newValue));
    }

    private void TextBoxNotesDesigner_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        string oldValue = ChartSystem.Entry.NotesDesigner;
        string newValue = TextBoxNotesDesigner.Text ?? "";
        if (oldValue == newValue) return;
        
        UndoRedoSystem.Push(new NotesDesignerEditOperation(oldValue, newValue));
    }

    private void ComboBoxDifficulty_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        Difficulty oldValue = ChartSystem.Entry.Difficulty;
        Difficulty newValue = (Difficulty)ComboBoxDifficulty.SelectedIndex;
        if (oldValue == newValue) return;
        
        UndoRedoSystem.Push(new DifficultyEditOperation(oldValue, newValue));
    }

    private void TextBoxLevel_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        try
        {
            double oldValue = ChartSystem.Entry.Level;
            double newValue = Convert.ToDouble(TextBoxLevel.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;
        
            UndoRedoSystem.Push(new LevelEditOperation(oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.Push(new LevelEditOperation(ChartSystem.Entry.Level, 0));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void TextBoxClearThreshold_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        try
        {
            float oldValue = ChartSystem.Entry.ClearThreshold;
            float newValue = Convert.ToSingle(TextBoxClearThreshold.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;
        
            UndoRedoSystem.Push(new ClearThresholdEditOperation(oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.Push(new ClearThresholdEditOperation(ChartSystem.Entry.ClearThreshold, 0));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void ToggleButtonAutoClearThreshold_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        bool oldValue = ChartSystem.Entry.AutoClearThreshold;
        bool newValue = ToggleButtonAutoClearThreshold.IsChecked ?? false;
        if (oldValue == newValue) return;
        
        if (newValue == true)
        {
            AutoClearThresholdEditOperation op0 = new (oldValue, newValue);
            BuildChartOperation op1 = new();
            UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
        }
        else
        {
            UndoRedoSystem.Push(new AutoClearThresholdEditOperation(oldValue, newValue));
        }
    }

    private void NumericUpDownChartEndMeasure_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        int measure = (int?)NumericUpDownChartEndMeasure.Value ?? 0;
        int tick = (int?)NumericUpDownChartEndTick.Value ?? 0;

        Timestamp oldValue = ChartSystem.Entry.ChartEnd;
        Timestamp newValue = new(measure, tick);
        if (oldValue == newValue) return;
        
        UndoRedoSystem.Push(new ChartEndEditOperation(oldValue, newValue));
    }

    private void NumericUpDownChartEndTick_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        int measure = (int?)NumericUpDownChartEndMeasure.Value ?? 0;
        int tick = (int?)NumericUpDownChartEndTick.Value ?? 0;
        int fullTick = measure * 1920 + tick;
        
        Timestamp oldValue = ChartSystem.Entry.ChartEnd;
        Timestamp newValue = new(fullTick);
        if (oldValue == newValue) return;
        
        UndoRedoSystem.Push(new ChartEndEditOperation(oldValue, newValue));
    }

    private void ToggleButtonAutoChartEnd_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        bool oldValue = ChartSystem.Entry.AutoChartEnd;
        bool newValue = ToggleButtonAutoChartEnd.IsChecked ?? false;
        if (oldValue == newValue) return;
        
        if (newValue == true)
        {
            AutoChartEndEditOperation op0 = new (oldValue, newValue);
            BuildChartOperation op1 = new();
            UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
        }
        else
        {
            UndoRedoSystem.Push(new AutoChartEndEditOperation(oldValue, newValue));
        }
    }

    private void ButtonPlayPreview_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender == null) return;

        TimeSystem.PlaybackState = PlaybackState.Preview;
        TimeSystem.SeekTime(ChartSystem.Entry.PreviewBegin, TimeSystem.Division);
    }
    
    private void TextBoxPreviewBegin_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        try
        {
            float oldValue = ChartSystem.Entry.PreviewBegin;
            float newValue = 1000 * Convert.ToSingle(TextBoxPreviewBegin.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;
        
            UndoRedoSystem.Push(new PreviewBeginEditOperation(oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.Push(new PreviewBeginEditOperation(ChartSystem.Entry.PreviewBegin, 0));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void TextBoxPreviewLength_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        try
        {
            float oldValue = ChartSystem.Entry.PreviewLength;
            float newValue = 1000 * Convert.ToSingle(TextBoxPreviewLength.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;
        
            UndoRedoSystem.Push(new PreviewBeginEditOperation(oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.Push(new PreviewBeginEditOperation(ChartSystem.Entry.PreviewLength, 10000));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void ComboBoxBackground_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        BackgroundOption oldValue = ChartSystem.Entry.Background;
        BackgroundOption newValue = (BackgroundOption)ComboBoxBackground.SelectedIndex;
        if (oldValue == newValue) return;
        
        UndoRedoSystem.Push(new BackgroundEditOperation(oldValue, newValue));
    }

    private void ComboBoxTutorialMode_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        bool oldValue = ChartSystem.Entry.TutorialMode;
        bool newValue = ComboBoxTutorialMode.SelectedIndex == 1;
        if (oldValue == newValue) return;
        
        UndoRedoSystem.Push(new TutorialModeEditOperation(oldValue, newValue));
    }

    private void TextBoxJacket_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        string oldValue = ChartSystem.Entry.JacketFile;
        string newValue = TextBoxJacket.Text ?? "";
        if (oldValue == newValue) return;
        
        UndoRedoSystem.Push(new JacketEditOperation(oldValue, newValue));
    }

    private void TextBoxAudio_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        string oldValue = ChartSystem.Entry.AudioFile;
        string newValue = TextBoxAudio.Text ?? "";
        if (oldValue == newValue) return;
        
        UndoRedoSystem.Push(new AudioEditOperation(oldValue, newValue));
    }

    private void TextBoxVideo_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        string oldValue = ChartSystem.Entry.VideoFile;
        string newValue = TextBoxVideo.Text ?? "";
        if (oldValue == newValue) return;
        
        UndoRedoSystem.Push(new VideoEditOperation(oldValue, newValue));
    }

    private void TextBoxAudioOffset_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        try
        {
            float oldValue = ChartSystem.Entry.AudioOffset;
            float newValue = 1000 * Convert.ToSingle(TextBoxAudioOffset.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;
        
            UndoRedoSystem.Push(new AudioOffsetEditOperation(oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.Push(new AudioOffsetEditOperation(ChartSystem.Entry.AudioOffset, 0));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void TextBoxVideoOffset_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        try
        {
            float oldValue = ChartSystem.Entry.VideoOffset;
            float newValue = 1000 * Convert.ToSingle(TextBoxVideoOffset.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;
        
            UndoRedoSystem.Push(new VideoOffsetEditOperation(oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.Push(new VideoOffsetEditOperation(ChartSystem.Entry.VideoOffset, 0));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }
    
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
            
            if (ChartSystem.Entry.RootDirectory == "")
            {
                // Define new root directory.
                RootDirectoryEditOperation op0 = new(ChartSystem.Entry.RootDirectory, Path.GetDirectoryName(files[0].Path.LocalPath) ?? "");
                JacketEditOperation op1 = new(ChartSystem.Entry.JacketFile, Path.GetFileName(files[0].Path.LocalPath));
                UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string filename = Path.GetFileName(files[0].Path.LocalPath);
                string pathFromRootDirectory = Path.Combine(ChartSystem.Entry.RootDirectory, filename);

                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                if (!await PromptFileMoveAndOverwrite(files[0].Path.LocalPath, pathFromRootDirectory)) return;

                UndoRedoSystem.Push(new JacketEditOperation(ChartSystem.Entry.JacketFile, filename));
            }
        }
        catch (Exception ex)
        {
            // don't throw
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

            if (ChartSystem.Entry.RootDirectory == "")
            {
                // Define new root directory.
                RootDirectoryEditOperation op0 = new(ChartSystem.Entry.RootDirectory, Path.GetDirectoryName(files[0].Path.LocalPath) ?? "");
                AudioEditOperation op1 = new(ChartSystem.Entry.AudioFile, Path.GetFileName(files[0].Path.LocalPath));
                UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string filename = Path.GetFileName(files[0].Path.LocalPath);
                string pathFromRootDirectory = Path.Combine(ChartSystem.Entry.RootDirectory, filename);

                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                if (!await PromptFileMoveAndOverwrite(files[0].Path.LocalPath, pathFromRootDirectory)) return;

                UndoRedoSystem.Push(new AudioEditOperation(ChartSystem.Entry.AudioFile, filename));
            }
        }
        catch (Exception ex)
        {
            // don't throw
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

            if (ChartSystem.Entry.RootDirectory == "")
            {
                // Define new root directory.
                RootDirectoryEditOperation op0 = new(ChartSystem.Entry.RootDirectory, Path.GetDirectoryName(files[0].Path.LocalPath) ?? "");
                VideoEditOperation op1 = new(ChartSystem.Entry.VideoFile, Path.GetFileName(files[0].Path.LocalPath));
                UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string filename = Path.GetFileName(files[0].Path.LocalPath);
                string pathFromRootDirectory = Path.Combine(ChartSystem.Entry.RootDirectory, filename);

                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                if (!await PromptFileMoveAndOverwrite(files[0].Path.LocalPath, pathFromRootDirectory)) return;
                
                UndoRedoSystem.Push(new VideoEditOperation(ChartSystem.Entry.VideoFile, filename));
            }
        }
        catch (Exception ex)
        {
            // don't throw
            Console.WriteLine(ex);
        }
    }

    private async Task<bool> PromptFileMoveAndOverwrite(string sourceFilePath, string projectFilePath)
    {
        if (sourceFilePath != projectFilePath)
        {
            ModalDialogResult moveResult = await promptFileMove();
            if (moveResult is ModalDialogResult.Tertiary or ModalDialogResult.Cancel) return false;

            // File already exists in root directory. Prompt to overwrite files.
            if (File.Exists(projectFilePath))
            {
                ModalDialogResult overwriteResult = await promptFileOverwrite();
                if (overwriteResult is ModalDialogResult.Secondary or ModalDialogResult.Cancel) return false;
            }
            
            if (moveResult is ModalDialogResult.Primary)
            {
                // Copy
                File.Copy(sourceFilePath, projectFilePath, true);
            }
            else if (moveResult is ModalDialogResult.Secondary)
            {
                // Move
                File.Move(sourceFilePath, projectFilePath, true);
            }
        }

        return true;
        
        async Task<ModalDialogResult> promptFileMove()
        {
            if (VisualRoot is not Window rootWindow) return ModalDialogResult.Cancel;

            ModalDialogWindow dialog = new()
            {
                DialogIcon = Icon.Warning,
                WindowTitleKey = "ModalDialog.FileMovePrompt.Title",
                HeaderKey = "ModalDialog.FileMovePrompt.Header",
                ParagraphKey = "ModalDialog.FileMovePrompt.Paragraph",
                ButtonPrimaryKey = "ModalDialog.FileMovePrompt.CopyFile",
                ButtonSecondaryKey = "ModalDialog.FileMovePrompt.MoveFile",
                ButtonTertiaryKey = "Generic.Cancel",
            };

            dialog.InitializeDialog();
            await dialog.ShowDialog(rootWindow);
            return dialog.Result;
        }
        
        async Task<ModalDialogResult> promptFileOverwrite()
        {
            if (VisualRoot is not Window rootWindow) return ModalDialogResult.Cancel;

            ModalDialogWindow dialog = new()
            {
                DialogIcon = Icon.Warning,
                WindowTitleKey = "ModalDialog.FileOverwritePrompt.Title",
                HeaderKey = "ModalDialog.FileOverwritePrompt.Header",
                ParagraphKey = "ModalDialog.FileOverwritePrompt.Paragraph",
                ButtonPrimaryKey = "ModalDialog.FileOverwritePrompt.OverwriteFile",
                ButtonSecondaryKey = "Generic.Cancel",
            };

            dialog.InitializeDialog();
            await dialog.ShowDialog(rootWindow);
            return dialog.Result;
        }
    }
}