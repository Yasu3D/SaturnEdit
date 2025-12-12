using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SaturnData.Content.Cosmetics;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.GenericOperations;

namespace SaturnEdit.Windows.Main.CosmeticsEditor.Tabs;

public partial class NoteSoundEditorView : UserControl
{
    public NoteSoundEditorView()
    {
        InitializeComponent();
        
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged += CosmeticBranch_OnOperationHistoryChanged;
        CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
    }

    private bool blockEvents = false;
    
#region System Event Handlers
    private void CosmeticBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        if (CosmeticSystem.CosmeticItem is not NoteSound noteSound) return;
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            TextBoxArtist.Text = noteSound.Artist;

            TextBoxAudioClickPath.Text = noteSound.AudioClickPath;
            TextBoxAudioGuidePath.Text = noteSound.AudioGuidePath;
            TextBoxAudioTouchMarvelousPath.Text = noteSound.AudioTouchMarvelousPath;
            TextBoxAudioTouchGoodPath.Text = noteSound.AudioTouchGoodPath;
            TextBoxAudioSlideMarvelousPath.Text = noteSound.AudioSlideMarvelousPath;
            TextBoxAudioSlideGoodPath.Text = noteSound.AudioSlideGoodPath;
            TextBoxAudioSnapMarvelousPath.Text = noteSound.AudioSnapMarvelousPath;
            TextBoxAudioSnapGoodPath.Text = noteSound.AudioSnapGoodPath;
            TextBoxAudioHoldPath.Text = noteSound.AudioHoldPath;
            TextBoxAudioReHoldPath.Text = noteSound.AudioReHoldPath;
            TextBoxAudioChainPath.Text = noteSound.AudioChainPath;
            TextBoxAudioBonusPath.Text = noteSound.AudioBonusPath;
            TextBoxAudioRPath.Text = noteSound.AudioRPath;
            
            IconFileNotFoundWarningAudioClick.IsVisible = noteSound.AudioClickPath != "" && !File.Exists(noteSound.AbsoluteAudioClickPath);
            IconFileNotFoundWarningAudioGuide.IsVisible = noteSound.AudioGuidePath != "" && !File.Exists(noteSound.AbsoluteAudioGuidePath);
            IconFileNotFoundWarningAudioTouchMarvelous.IsVisible = noteSound.AudioTouchMarvelousPath != "" && !File.Exists(noteSound.AbsoluteAudioTouchMarvelousPath);
            IconFileNotFoundWarningAudioTouchGood.IsVisible = noteSound.AudioTouchGoodPath != "" && !File.Exists(noteSound.AbsoluteAudioTouchGoodPath);
            IconFileNotFoundWarningAudioSlideMarvelous.IsVisible = noteSound.AudioSlideMarvelousPath != "" && !File.Exists(noteSound.AbsoluteAudioSlideMarvelousPath);
            IconFileNotFoundWarningAudioSlideGood.IsVisible = noteSound.AudioSlideGoodPath != "" && !File.Exists(noteSound.AbsoluteAudioSlideGoodPath);
            IconFileNotFoundWarningAudioSnapMarvelous.IsVisible = noteSound.AudioSnapMarvelousPath != "" && !File.Exists(noteSound.AbsoluteAudioSnapMarvelousPath);
            IconFileNotFoundWarningAudioSnapGood.IsVisible = noteSound.AudioSnapGoodPath != "" && !File.Exists(noteSound.AbsoluteAudioSnapGoodPath);
            IconFileNotFoundWarningAudioHold.IsVisible = noteSound.AudioHoldPath != "" && !File.Exists(noteSound.AbsoluteAudioHoldPath);
            IconFileNotFoundWarningAudioReHold.IsVisible = noteSound.AudioReHoldPath != "" && !File.Exists(noteSound.AbsoluteAudioReHoldPath);
            IconFileNotFoundWarningAudioChain.IsVisible = noteSound.AudioChainPath != "" && !File.Exists(noteSound.AbsoluteAudioChainPath);
            IconFileNotFoundWarningAudioBonus.IsVisible = noteSound.AudioBonusPath != "" && !File.Exists(noteSound.AbsoluteAudioBonusPath);
            IconFileNotFoundWarningAudioR.IsVisible = noteSound.AudioRPath != "" && !File.Exists(noteSound.AbsoluteAudioRPath);

            TextBoxHoldLoopStartTime.Text   = (0.001f * noteSound.HoldLoopStartTime).ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxHoldLoopEndTime.Text     = (0.001f * noteSound.HoldLoopEndTime).ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxReHoldLoopStartTime.Text = (0.001f * noteSound.ReHoldLoopStartTime).ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxReHoldLoopEndTime.Text   = (0.001f * noteSound.ReHoldLoopEndTime).ToString("0.000000", CultureInfo.InvariantCulture);
            
            blockEvents = false;
        });
    }
#endregion System Event Handlers
    
#region UI Event Handlers
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged -= CosmeticBranch_OnOperationHistoryChanged;
        
        base.OnUnloaded(e);
    }
    
    private void TextBoxArtist_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not TextBox textBox) return;
        if (CosmeticSystem.CosmeticItem is not NoteSound noteSound) return;
        
        string oldValue = noteSound.Artist;
        string newValue = textBox.Text ?? "";
        if (oldValue == newValue)
        {
            // Refresh UI in case the file changed, but don't push unnecessary operation.
            CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
            return;
        }
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { noteSound.Artist = value; }, oldValue, newValue));
    }
    
    private void TextBoxPath_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not TextBox textBox) return;
        if (CosmeticSystem.CosmeticItem is not NoteSound noteSound) return;
        
        string oldValue = "";
        if      (textBox == TextBoxAudioTouchMarvelousPath) { oldValue = noteSound.AudioTouchMarvelousPath; }
        else if (textBox == TextBoxAudioClickPath)          { oldValue = noteSound.AudioClickPath; }
        else if (textBox == TextBoxAudioGuidePath)          { oldValue = noteSound.AudioGuidePath; }
        else if (textBox == TextBoxAudioTouchGoodPath)      { oldValue = noteSound.AudioTouchGoodPath; }
        else if (textBox == TextBoxAudioSlideMarvelousPath) { oldValue = noteSound.AudioSlideMarvelousPath; }
        else if (textBox == TextBoxAudioSlideGoodPath)      { oldValue = noteSound.AudioSlideGoodPath; }
        else if (textBox == TextBoxAudioSnapMarvelousPath)  { oldValue = noteSound.AudioSnapMarvelousPath; }
        else if (textBox == TextBoxAudioSnapGoodPath)       { oldValue = noteSound.AudioSnapGoodPath; }
        else if (textBox == TextBoxAudioHoldPath)           { oldValue = noteSound.AudioHoldPath; }
        else if (textBox == TextBoxAudioReHoldPath)         { oldValue = noteSound.AudioReHoldPath; }
        else if (textBox == TextBoxAudioChainPath)          { oldValue = noteSound.AudioChainPath; }
        else if (textBox == TextBoxAudioBonusPath)          { oldValue = noteSound.AudioBonusPath; }
        else if (textBox == TextBoxAudioRPath)              { oldValue = noteSound.AudioRPath; }
                
        string newValue = textBox.Text ?? "";
        if (oldValue == newValue)
        {
            // Refresh UI in case the file changed, but don't push unnecessary operation.
            CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
            return;
        }

        Action<string>? action = null;
        if      (textBox == TextBoxAudioTouchMarvelousPath) { action = value => { noteSound.AudioTouchMarvelousPath = value; }; }
        else if (textBox == TextBoxAudioClickPath)          { action = value => { noteSound.AudioClickPath = value; }; }
        else if (textBox == TextBoxAudioGuidePath)          { action = value => { noteSound.AudioGuidePath = value; }; }
        else if (textBox == TextBoxAudioTouchGoodPath)      { action = value => { noteSound.AudioTouchGoodPath = value; }; }
        else if (textBox == TextBoxAudioSlideMarvelousPath) { action = value => { noteSound.AudioSlideMarvelousPath = value; }; }
        else if (textBox == TextBoxAudioSlideGoodPath)      { action = value => { noteSound.AudioSlideGoodPath = value; }; }
        else if (textBox == TextBoxAudioSnapMarvelousPath)  { action = value => { noteSound.AudioSnapMarvelousPath = value; }; }
        else if (textBox == TextBoxAudioSnapGoodPath)       { action = value => { noteSound.AudioSnapGoodPath = value; }; }
        else if (textBox == TextBoxAudioHoldPath)           { action = value => { noteSound.AudioHoldPath = value; }; }
        else if (textBox == TextBoxAudioReHoldPath)         { action = value => { noteSound.AudioReHoldPath = value; }; }
        else if (textBox == TextBoxAudioChainPath)          { action = value => { noteSound.AudioChainPath = value; }; }
        else if (textBox == TextBoxAudioBonusPath)          { action = value => { noteSound.AudioBonusPath = value; }; }
        else if (textBox == TextBoxAudioRPath)              { action = value => { noteSound.AudioRPath = value; }; }

        if (action == null) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(action, oldValue, newValue));
    }

    private async void ButtonPickFile_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (sender is not Button button) return;
            if (CosmeticSystem.CosmeticItem is not NoteSound noteSound) return;
            
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            string? oldAbsolutePath = null;
            string? oldLocalPath = null;
            if (button == ButtonAudioTouchMarvelousPath)
            {
                oldAbsolutePath = noteSound.AbsoluteAudioTouchMarvelousPath;
                oldLocalPath = noteSound.AudioTouchMarvelousPath;
            }
            else if (button == ButtonAudioClickPath)
            {
                oldAbsolutePath = noteSound.AbsoluteAudioClickPath;
                oldLocalPath = noteSound.AudioClickPath;
            }
            else if (button == ButtonAudioGuidePath)
            {
                oldAbsolutePath = noteSound.AbsoluteAudioGuidePath;
                oldLocalPath = noteSound.AudioGuidePath;
            }
            else if (button == ButtonAudioTouchGoodPath)
            {
                oldAbsolutePath = noteSound.AbsoluteAudioTouchGoodPath;
                oldLocalPath = noteSound.AudioTouchGoodPath;
            }
            else if (button == ButtonAudioSlideMarvelousPath)
            {
                oldAbsolutePath = noteSound.AbsoluteAudioSlideMarvelousPath;
                oldLocalPath = noteSound.AudioSlideMarvelousPath;
            }
            else if (button == ButtonAudioSlideGoodPath)
            {
                oldAbsolutePath = noteSound.AbsoluteAudioSlideGoodPath;
                oldLocalPath = noteSound.AudioSlideGoodPath;
            }
            else if (button == ButtonAudioSnapMarvelousPath)
            {
                oldAbsolutePath = noteSound.AbsoluteAudioSnapMarvelousPath;
                oldLocalPath = noteSound.AudioSnapMarvelousPath;
            }
            else if (button == ButtonAudioSnapGoodPath)
            {
                oldAbsolutePath = noteSound.AbsoluteAudioSnapGoodPath;
                oldLocalPath = noteSound.AudioSnapGoodPath;
            }
            else if (button == ButtonAudioHoldPath)
            {
                oldAbsolutePath = noteSound.AbsoluteAudioHoldPath;
                oldLocalPath = noteSound.AudioHoldPath;
            }
            else if (button == ButtonAudioReHoldPath)
            {
                oldAbsolutePath = noteSound.AbsoluteAudioReHoldPath;
                oldLocalPath = noteSound.AudioReHoldPath;
            }
            else if (button == ButtonAudioChainPath)
            {
                oldAbsolutePath = noteSound.AbsoluteAudioChainPath;
                oldLocalPath = noteSound.AudioChainPath;
            }
            else if (button == ButtonAudioBonusPath)
            {
                oldAbsolutePath = noteSound.AbsoluteAudioBonusPath;
                oldLocalPath = noteSound.AudioBonusPath;
            }
            else if (button == ButtonAudioRPath)
            {
                oldAbsolutePath = noteSound.AbsoluteAudioRPath;
                oldLocalPath = noteSound.AudioRPath;
            }
            
            if (oldAbsolutePath == null) return;
            if (oldLocalPath == null) return;
            
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
            
            if (oldAbsolutePath == files[0].Path.LocalPath)
            {
                // Refresh UI in case the file changed, but don't push unnecessary operation.
                CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
                return;
            }
            
            Action<string>? action = null;
            if      (button == ButtonAudioTouchMarvelousPath) { action = value => { noteSound.AudioTouchMarvelousPath = value; }; }
            else if (button == ButtonAudioClickPath)          { action = value => { noteSound.AudioClickPath = value; }; }
            else if (button == ButtonAudioGuidePath)          { action = value => { noteSound.AudioGuidePath = value; }; }
            else if (button == ButtonAudioTouchGoodPath)      { action = value => { noteSound.AudioTouchGoodPath = value; }; }
            else if (button == ButtonAudioSlideMarvelousPath) { action = value => { noteSound.AudioSlideMarvelousPath = value; }; }
            else if (button == ButtonAudioSlideGoodPath)      { action = value => { noteSound.AudioSlideGoodPath = value; }; }
            else if (button == ButtonAudioSnapMarvelousPath)  { action = value => { noteSound.AudioSnapMarvelousPath = value; }; }
            else if (button == ButtonAudioSnapGoodPath)       { action = value => { noteSound.AudioSnapGoodPath = value; }; }
            else if (button == ButtonAudioHoldPath)           { action = value => { noteSound.AudioHoldPath = value; }; }
            else if (button == ButtonAudioReHoldPath)         { action = value => { noteSound.AudioReHoldPath = value; }; }
            else if (button == ButtonAudioChainPath)          { action = value => { noteSound.AudioChainPath = value; }; }
            else if (button == ButtonAudioBonusPath)          { action = value => { noteSound.AudioBonusPath = value; }; }
            else if (button == ButtonAudioRPath)              { action = value => { noteSound.AudioRPath = value; }; }

            if (action == null) return;
            
            if (noteSound.AbsoluteSourcePath == "")
            {
                // Define new source path.
                string newSourcePath = Path.Combine(Path.GetDirectoryName(files[0].Path.LocalPath) ?? "", "note_sound.toml");
                
                GenericEditOperation<string> op0 = new(value => { noteSound.AbsoluteSourcePath = value; }, noteSound.AbsoluteSourcePath, newSourcePath);
                GenericEditOperation<string> op1 = new(action, oldLocalPath, Path.GetFileName(files[0].Path.LocalPath));
                
                UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string directoryPath = Path.GetDirectoryName(noteSound.AbsoluteSourcePath) ?? "";
                string localPath = Path.GetRelativePath(directoryPath, files[0].Path.LocalPath);
                string pathFromRootDirectory = Path.Combine(directoryPath, localPath);

                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                if (!await MainWindow.PromptFileMoveAndOverwrite(files[0].Path.LocalPath, pathFromRootDirectory)) return;

                UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(action, oldLocalPath, localPath));
            }
        }
        catch (Exception ex)
        {
            // don't throw
            Console.WriteLine(ex);
        }
    }
    
    private void TextBoxHoldLoopStartTime_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxHoldLoopStartTime == null) return;
        if (CosmeticSystem.CosmeticItem is not NoteSound noteSound) return;

        try
        {
            float oldValue = noteSound.HoldLoopStartTime;
            float newValue = 1000 * Convert.ToSingle(TextBoxHoldLoopStartTime.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { noteSound.HoldLoopStartTime = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { noteSound.HoldLoopStartTime = value; }, noteSound.HoldLoopStartTime, 0));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void TextBoxHoldLoopEndTime_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxHoldLoopEndTime == null) return;
        if (CosmeticSystem.CosmeticItem is not NoteSound noteSound) return;

        try
        {
            float oldValue = noteSound.HoldLoopEndTime;
            float newValue = 1000 * Convert.ToSingle(TextBoxHoldLoopEndTime.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { noteSound.HoldLoopEndTime = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { noteSound.HoldLoopEndTime = value; }, noteSound.HoldLoopEndTime, 0));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void TextBoxReHoldLoopStartTime_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxReHoldLoopStartTime == null) return;
        if (CosmeticSystem.CosmeticItem is not NoteSound noteSound) return;

        try
        {
            float oldValue = noteSound.ReHoldLoopStartTime;
            float newValue = 1000 * Convert.ToSingle(TextBoxReHoldLoopStartTime.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { noteSound.ReHoldLoopStartTime = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { noteSound.ReHoldLoopStartTime = value; }, noteSound.ReHoldLoopStartTime, 0));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void TextBoxReHoldLoopEndTime_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxReHoldLoopEndTime == null) return;
        if (CosmeticSystem.CosmeticItem is not NoteSound noteSound) return;

        try
        {
            float oldValue = noteSound.ReHoldLoopEndTime;
            float newValue = 1000 * Convert.ToSingle(TextBoxReHoldLoopEndTime.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { noteSound.ReHoldLoopEndTime = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { noteSound.ReHoldLoopEndTime = value; }, noteSound.ReHoldLoopEndTime, 0));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }
#endregion UI Event Handlers
}