using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SaturnData.Notation.Core;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.GenericOperations;
using SaturnEdit.Utilities;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class ChartPropertiesView : UserControl
{
    public ChartPropertiesView()
    {
        InitializeComponent();
    }

    private bool blockEvents = false;

#region System Event Handlers
    private void ChartBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
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
            
            NumericUpDownPreviewBeginMeasure.Value = ChartSystem.Entry.PreviewBegin.Measure;
            NumericUpDownPreviewBeginTick.Value = ChartSystem.Entry.PreviewBegin.Tick;
            NumericUpDownPreviewEndMeasure.Value = ChartSystem.Entry.PreviewEnd.Measure;
            NumericUpDownPreviewEndTick.Value = ChartSystem.Entry.PreviewEnd.Tick;
            
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
        });
    }

    private void OnJacketChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
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
        });
    }
#endregion System Event Handlers

#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        ChartSystem.JacketChanged += OnJacketChanged;
        OnJacketChanged(null, EventArgs.Empty);

        UndoRedoSystem.ChartBranch.OperationHistoryChanged += ChartBranch_OnOperationHistoryChanged;
        ChartBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        ChartSystem.JacketChanged -= OnJacketChanged;
        UndoRedoSystem.ChartBranch.OperationHistoryChanged -= ChartBranch_OnOperationHistoryChanged;
        
        base.OnUnloaded(e);
    }
    
    private async void TextBoxTitle_OnLostFocus(object? sender, RoutedEventArgs routedEventArgs)
    {
        try
        {
            if (blockEvents) return;
            if (sender == null) return;

            string oldValue = ChartSystem.Entry.Title;
            string newValue = TextBoxTitle.Text ?? "";
            if (oldValue == newValue) return;

            if (ChartSystem.Entry.AutoReading)
            {
                GenericEditOperation<string> op0 = new(value => { ChartSystem.Entry.Title = value; }, oldValue, newValue);
                GenericEditOperation<string> op1 = new(value => { ChartSystem.Entry.Reading = value; }, ChartSystem.Entry.Reading, await ReadingConverter.Convert(newValue));

                UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<string>(value => { ChartSystem.Entry.Title = value; }, oldValue, newValue));
            }
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
    }

    private void TextBoxReading_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        string oldValue = ChartSystem.Entry.Reading;
        string newValue = TextBoxReading.Text ?? "";
        if (oldValue == newValue) return;
        
        UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<string>(value => { ChartSystem.Entry.Reading = value; }, oldValue, newValue));
    }

    private async void ToggleButtonAutoReading_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (sender == null) return;

            bool oldValue = ChartSystem.Entry.AutoReading;
            bool newValue = ToggleButtonAutoReading.IsChecked ?? false;
            if (oldValue == newValue) return;

            if (newValue == true)
            {
                string reading = await ReadingConverter.Convert(ChartSystem.Entry.Title);

                GenericEditOperation<bool> op0 = new(value => { ChartSystem.Entry.AutoReading = value; }, oldValue, newValue);
                GenericEditOperation<string> op1 = new(value => { ChartSystem.Entry.Reading = value; }, ChartSystem.Entry.Reading, reading);

                UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<bool>(value => { ChartSystem.Entry.AutoReading = value; }, oldValue, newValue));
            }
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
    }

    private void TextBoxArtist_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        string oldValue = ChartSystem.Entry.Artist;
        string newValue = TextBoxArtist.Text ?? "";
        if (oldValue == newValue) return;
        
        UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<string>(value => { ChartSystem.Entry.Artist = value; }, oldValue, newValue));
    }

    private void TextBoxBpmMessage_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        string oldValue = ChartSystem.Entry.BpmMessage;
        string newValue = TextBoxBpmMessage.Text ?? "";
        if (oldValue == newValue) return;
        
        UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<string>(value => { ChartSystem.Entry.BpmMessage = value; }, oldValue, newValue));
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
            GenericEditOperation<bool> op0 = new(value => { ChartSystem.Entry.AutoBpmMessage = value; }, oldValue, newValue);
            GenericEditOperation<string> op1 = new(value => { ChartSystem.Entry.BpmMessage = value; }, ChartSystem.Entry.BpmMessage, ChartSystem.Chart.GetAutoBpmMessage());
            
            UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
        }
        else
        {
            UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<bool>(value => { ChartSystem.Entry.AutoBpmMessage = value; }, oldValue, newValue));
        }
    }

    private void TextBoxRevision_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        string oldValue = ChartSystem.Entry.Revision;
        string newValue = TextBoxRevision.Text ?? "";
        if (oldValue == newValue) return;
        
        UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<string>(value => { ChartSystem.Entry.Revision = value; }, oldValue, newValue));
    }

    private void TextBoxNotesDesigner_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        string oldValue = ChartSystem.Entry.NotesDesigner;
        string newValue = TextBoxNotesDesigner.Text ?? "";
        if (oldValue == newValue) return;
        
        UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<string>(value => { ChartSystem.Entry.NotesDesigner = value; }, oldValue, newValue));
    }

    private void ComboBoxDifficulty_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        Difficulty oldValue = ChartSystem.Entry.Difficulty;
        Difficulty newValue = (Difficulty)ComboBoxDifficulty.SelectedIndex;
        if (oldValue == newValue) return;

        if (ChartSystem.Entry.AutoClearThreshold)
        {
            GenericEditOperation<Difficulty> op0 = new(value => { ChartSystem.Entry.Difficulty = value; }, oldValue, newValue);
            GenericEditOperation<float> op1 = new(value => { ChartSystem.Entry.ClearThreshold = value; }, ChartSystem.Entry.ClearThreshold, Entry.GetAutoClearThreshold(newValue));
            
            UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
        }
        else
        {
            UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<Difficulty>(value => { ChartSystem.Entry.Difficulty = value; }, oldValue, newValue));
        }
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
        
            UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<double>(value => { ChartSystem.Entry.Level = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<double>(value => { ChartSystem.Entry.Level = value; }, ChartSystem.Entry.Level, 0));

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
        
            UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<float>(value => { ChartSystem.Entry.ClearThreshold = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<float>(value => { ChartSystem.Entry.ClearThreshold = value; }, ChartSystem.Entry.ClearThreshold, 0));

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
            float clearThreshold = Entry.GetAutoClearThreshold(ChartSystem.Entry.Difficulty);

            GenericEditOperation<bool> op0 = new(value => { ChartSystem.Entry.AutoClearThreshold = value; }, oldValue, newValue);
            GenericEditOperation<float> op1 = new(value => { ChartSystem.Entry.ClearThreshold = value; }, ChartSystem.Entry.ClearThreshold, clearThreshold);

            UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
        }
        else
        {
            UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<bool>(value => { ChartSystem.Entry.AutoClearThreshold = value; }, oldValue, newValue));
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
        
        UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<Timestamp>(value => { ChartSystem.Entry.ChartEnd = value; }, oldValue, newValue));
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
        
        UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<Timestamp>(value => { ChartSystem.Entry.ChartEnd = value; }, oldValue, newValue));
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
            GenericEditOperation<bool> op0 = new(value => { ChartSystem.Entry.AutoChartEnd = value; }, oldValue, newValue);
            GenericEditOperation<Timestamp> op1 = new(value => { ChartSystem.Entry.ChartEnd = value; }, ChartSystem.Entry.ChartEnd, ChartSystem.Chart.FindIdealChartEnd((float?)AudioSystem.AudioChannelAudio?.Length ?? 0));
            
            UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
        }
        else
        {
            UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<bool>(value => { ChartSystem.Entry.AutoChartEnd = value; }, oldValue, newValue));
        }
    }

    private void ButtonPlayPreview_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender == null) return;

        TimeSystem.PlaybackState = PlaybackState.Preview;
        TimeSystem.SeekTime(ChartSystem.Entry.PreviewBegin.Time, TimeSystem.Division);
    }
    
    private void NumericUpDownPreviewBeginMeasure_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        int measure = (int?)NumericUpDownPreviewBeginMeasure.Value ?? 0;
        int tick = (int?)NumericUpDownPreviewBeginTick.Value ?? 0;

        Timestamp oldValue = ChartSystem.Entry.PreviewBegin;
        Timestamp newValue = new(measure, tick);
        if (oldValue == newValue) return;
        
        UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<Timestamp>(value => { ChartSystem.Entry.PreviewBegin = value; }, oldValue, newValue));
    }

    private void NumericUpDownPreviewBeginTick_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        int measure = (int?)NumericUpDownPreviewBeginMeasure.Value ?? 0;
        int tick = (int?)NumericUpDownPreviewBeginTick.Value ?? 0;
        int fullTick = measure * 1920 + tick;
        
        Timestamp oldValue = ChartSystem.Entry.PreviewBegin;
        Timestamp newValue = new(fullTick);
        if (oldValue == newValue) return;
        
        UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<Timestamp>(value => { ChartSystem.Entry.PreviewBegin = value; }, oldValue, newValue));
    }

    private void NumericUpDownPreviewEndMeasure_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        int measure = (int?)NumericUpDownPreviewEndMeasure.Value ?? 0;
        int tick = (int?)NumericUpDownPreviewEndTick.Value ?? 0;

        Timestamp oldValue = ChartSystem.Entry.PreviewEnd;
        Timestamp newValue = new(measure, tick);
        if (oldValue == newValue) return;
        
        UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<Timestamp>(value => { ChartSystem.Entry.PreviewEnd = value; }, oldValue, newValue));
    }

    private void NumericUpDownPreviewEndTick_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        int measure = (int?)NumericUpDownPreviewEndMeasure.Value ?? 0;
        int tick = (int?)NumericUpDownPreviewEndTick.Value ?? 0;
        int fullTick = measure * 1920 + tick;
        
        Timestamp oldValue = ChartSystem.Entry.PreviewEnd;
        Timestamp newValue = new(fullTick);
        if (oldValue == newValue) return;
        
        UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<Timestamp>(value => { ChartSystem.Entry.PreviewEnd = value; }, oldValue, newValue));
    }
    
    private void ComboBoxBackground_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        BackgroundOption oldValue = ChartSystem.Entry.Background;
        BackgroundOption newValue = (BackgroundOption)ComboBoxBackground.SelectedIndex;
        if (oldValue == newValue) return;
        
        UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<BackgroundOption>(value => { ChartSystem.Entry.Background = value; }, oldValue, newValue));
    }

    private void ComboBoxTutorialMode_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        bool oldValue = ChartSystem.Entry.TutorialMode;
        bool newValue = ComboBoxTutorialMode.SelectedIndex == 1;
        if (oldValue == newValue) return;
        
        UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<bool>(value => { ChartSystem.Entry.TutorialMode = value; }, oldValue, newValue));
    }

    private void TextBoxJacket_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        string oldValue = ChartSystem.Entry.JacketFile;
        string newValue = TextBoxJacket.Text ?? "";
        if (oldValue == newValue) return;
        
        UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<string>(value => { ChartSystem.Entry.JacketFile = value; }, oldValue, newValue));
    }

    private void TextBoxAudio_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        string oldValue = ChartSystem.Entry.AudioFile;
        string newValue = TextBoxAudio.Text ?? "";
        if (oldValue == newValue) return;
        
        UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<string>(value => { ChartSystem.Entry.AudioFile = value; }, oldValue, newValue));
    }

    private void TextBoxVideo_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        string oldValue = ChartSystem.Entry.VideoFile;
        string newValue = TextBoxVideo.Text ?? "";
        if (oldValue == newValue) return;
        
        UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<string>(value => { ChartSystem.Entry.VideoFile = value; }, oldValue, newValue));
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
        
            UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<float>(value => { ChartSystem.Entry.AudioOffset = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<float>(value => { ChartSystem.Entry.AudioOffset = value; }, ChartSystem.Entry.AudioOffset, 0));

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
        
            UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<float>(value => { ChartSystem.Entry.VideoOffset = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<float>(value => { ChartSystem.Entry.VideoOffset = value; }, ChartSystem.Entry.VideoOffset, 0));

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
                        Patterns = ["*.png", "*.jpeg", "*.jpg"],
                    },
                ],
            });
            if (files.Count != 1) return;
            
            if (ChartSystem.Entry.RootDirectory == "")
            {
                // Define new root directory.
                GenericEditOperation<string> op0 = new(value => { ChartSystem.Entry.RootDirectory = value; }, ChartSystem.Entry.RootDirectory, Path.GetDirectoryName(files[0].Path.LocalPath) ?? "");
                GenericEditOperation<string> op1 = new(value => { ChartSystem.Entry.JacketFile = value; }, ChartSystem.Entry.JacketFile, Path.GetFileName(files[0].Path.LocalPath));
                UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string directoryPath = ChartSystem.Entry.RootDirectory;
                string localPath = Path.GetRelativePath(directoryPath, files[0].Path.LocalPath);
                string pathFromRootDirectory = Path.Combine(directoryPath, localPath);
                
                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                if (!await MainWindow.PromptFileMoveAndOverwrite(files[0].Path.LocalPath, pathFromRootDirectory)) return;

                UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<string>(value => { ChartSystem.Entry.JacketFile = value; }, ChartSystem.Entry.JacketFile, localPath));
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
                GenericEditOperation<string> op0 = new(value => { ChartSystem.Entry.RootDirectory = value; }, ChartSystem.Entry.RootDirectory, Path.GetDirectoryName(files[0].Path.LocalPath) ?? "");
                GenericEditOperation<string> op1 = new(value => { ChartSystem.Entry.AudioFile = value; }, ChartSystem.Entry.AudioFile, Path.GetFileName(files[0].Path.LocalPath));
                UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string directoryPath = ChartSystem.Entry.RootDirectory;
                string localPath = Path.GetRelativePath(directoryPath, files[0].Path.LocalPath);
                string pathFromRootDirectory = Path.Combine(directoryPath, localPath);

                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                if (!await MainWindow.PromptFileMoveAndOverwrite(files[0].Path.LocalPath, pathFromRootDirectory)) return;

                UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<string>(value => { ChartSystem.Entry.AudioFile = value; }, ChartSystem.Entry.AudioFile, localPath));
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
                GenericEditOperation<string> op0 = new(value => { ChartSystem.Entry.RootDirectory = value; }, ChartSystem.Entry.RootDirectory, Path.GetDirectoryName(files[0].Path.LocalPath) ?? "");
                GenericEditOperation<string> op1 = new(value => { ChartSystem.Entry.VideoFile = value; }, ChartSystem.Entry.VideoFile, Path.GetFileName(files[0].Path.LocalPath));
                UndoRedoSystem.ChartBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string directoryPath = ChartSystem.Entry.RootDirectory;
                string localPath = Path.GetRelativePath(directoryPath, files[0].Path.LocalPath);
                string pathFromRootDirectory = Path.Combine(directoryPath, localPath);

                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                if (!await MainWindow.PromptFileMoveAndOverwrite(files[0].Path.LocalPath, pathFromRootDirectory)) return;
                
                UndoRedoSystem.ChartBranch.Push(new GenericEditOperation<string>(value => { ChartSystem.Entry.VideoFile = value; }, ChartSystem.Entry.VideoFile, localPath));
            }
        }
        catch (Exception ex)
        {
            // Don't throw
            Console.WriteLine(ex);
        }
    }
#endregion UI Event Handlers
}