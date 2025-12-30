using System;
using System.Collections.Generic;
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

public partial class SystemMusicEditorView : UserControl
{
    public SystemMusicEditorView()
    {
        InitializeComponent();
    }

    private bool blockEvents = false;
    
#region System Event Handlers
    private void CosmeticBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        if (CosmeticSystem.CosmeticItem is not SystemMusic systemMusic) return;
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            TextBoxArtist.Text = systemMusic.Artist;
            
            TextBoxAudioAttractPath.Text       = systemMusic.AudioAttractPath;
            TextBoxAudioSelectPath.Text        = systemMusic.AudioSelectPath;
            TextBoxAudioResultPath.Text        = systemMusic.AudioResultPath;
            TextBoxAudioStageUpSelectPath.Text = systemMusic.AudioStageUpSelectPath;
            TextBoxAudioStageUpSecretPath.Text = systemMusic.AudioStageUpSecretPath;
            TextBoxAudioSeeYouPath.Text        = systemMusic.AudioSeeYouPath;
            
            IconFileNotFoundWarningAudioAttract.IsVisible       = systemMusic.AudioAttractPath       != "" && !File.Exists(systemMusic.AudioAttractPath);
            IconFileNotFoundWarningAudioSelect.IsVisible        = systemMusic.AudioSelectPath        != "" && !File.Exists(systemMusic.AudioSelectPath);
            IconFileNotFoundWarningAudioResult.IsVisible        = systemMusic.AudioResultPath        != "" && !File.Exists(systemMusic.AudioResultPath);
            IconFileNotFoundWarningAudioStageUpSelect.IsVisible = systemMusic.AudioStageUpSelectPath != "" && !File.Exists(systemMusic.AudioStageUpSelectPath);
            IconFileNotFoundWarningAudioStageUpSecret.IsVisible = systemMusic.AudioStageUpSecretPath != "" && !File.Exists(systemMusic.AudioStageUpSecretPath);
            IconFileNotFoundWarningAudioSeeYou.IsVisible        = systemMusic.AudioSeeYouPath        != "" && !File.Exists(systemMusic.AudioSeeYouPath);
            
            blockEvents = false;
        });
    }
#endregion System Event Handlers
    
    
#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged += CosmeticBranch_OnOperationHistoryChanged;
        CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged -= CosmeticBranch_OnOperationHistoryChanged;
        
        base.OnUnloaded(e);
    }
    
    private void TextBoxArtist_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not TextBox textBox) return;
        if (CosmeticSystem.CosmeticItem is not SystemMusic systemMusic) return;
        
        string oldValue = systemMusic.Artist;
        string newValue = textBox.Text ?? "";
        if (oldValue == newValue)
        {
            // Refresh UI in case the file changed, but don't push unnecessary operation.
            CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
            return;
        }
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { systemMusic.Artist = value; }, oldValue, newValue));
    }
    
    private void TextBoxPath_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not TextBox textBox) return;
        if (CosmeticSystem.CosmeticItem is not SystemMusic systemMusic) return;
        
        string oldValue = "";
        if      (textBox == TextBoxAudioAttractPath)       { oldValue = systemMusic.AudioAttractPath; }
        else if (textBox == TextBoxAudioSelectPath)        { oldValue = systemMusic.AudioSelectPath; }
        else if (textBox == TextBoxAudioResultPath)        { oldValue = systemMusic.AudioResultPath; }
        else if (textBox == TextBoxAudioStageUpSelectPath) { oldValue = systemMusic.AudioStageUpSelectPath; }
        else if (textBox == TextBoxAudioStageUpSecretPath) { oldValue = systemMusic.AudioStageUpSecretPath; }
        else if (textBox == TextBoxAudioSeeYouPath)        { oldValue = systemMusic.AudioSeeYouPath; }
                
        string newValue = textBox.Text ?? "";
        if (oldValue == newValue)
        {
            // Refresh UI in case the file changed, but don't push unnecessary operation.
            CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
            return;
        }

        Action<string>? action = null;
        if      (textBox == TextBoxAudioAttractPath)       { action = value => { systemMusic.AudioAttractPath = value; }; }
        else if (textBox == TextBoxAudioSelectPath)        { action = value => { systemMusic.AudioSelectPath = value; }; }
        else if (textBox == TextBoxAudioResultPath)        { action = value => { systemMusic.AudioResultPath = value; }; }
        else if (textBox == TextBoxAudioStageUpSelectPath) { action = value => { systemMusic.AudioStageUpSelectPath = value; }; }
        else if (textBox == TextBoxAudioStageUpSecretPath) { action = value => { systemMusic.AudioStageUpSecretPath = value; }; }
        else if (textBox == TextBoxAudioSeeYouPath)        { action = value => { systemMusic.AudioSeeYouPath = value; }; }

        if (action == null) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(action, oldValue, newValue));
    }

    private async void ButtonPickFile_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (sender is not Button button) return;
            if (CosmeticSystem.CosmeticItem is not SystemMusic systemMusic) return;
            
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            string? oldAbsolutePath = null;
            string? oldLocalPath = null;
            if (button == ButtonAudioAttractPath)
            {
                oldAbsolutePath = systemMusic.AbsoluteAudioAttractPath;
                oldLocalPath = systemMusic.AudioAttractPath;
            }
            else if (button == ButtonAudioSelectPath)
            {
                oldAbsolutePath = systemMusic.AbsoluteAudioSelectPath;
                oldLocalPath = systemMusic.AudioSelectPath;
            }
            else if (button == ButtonAudioResultPath)
            {
                oldAbsolutePath = systemMusic.AbsoluteAudioResultPath;
                oldLocalPath = systemMusic.AudioResultPath;
            }
            else if (button == ButtonAudioStageUpSelectPath)
            {
                oldAbsolutePath = systemMusic.AbsoluteAudioStageUpSelectPath;
                oldLocalPath = systemMusic.AudioStageUpSelectPath;
            }
            else if (button == ButtonAudioStageUpSecretPath)
            {
                oldAbsolutePath = systemMusic.AbsoluteAudioStageUpSecretPath;
                oldLocalPath = systemMusic.AudioStageUpSecretPath;
            }
            else if (button == ButtonAudioSeeYouPath)
            {
                oldAbsolutePath = systemMusic.AbsoluteAudioSeeYouPath;
                oldLocalPath = systemMusic.AudioSeeYouPath;
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
            if      (button == ButtonAudioAttractPath)       { action = value => { systemMusic.AudioAttractPath = value; }; }
            else if (button == ButtonAudioSelectPath)        { action = value => { systemMusic.AudioSelectPath = value; }; }
            else if (button == ButtonAudioResultPath)        { action = value => { systemMusic.AudioResultPath = value; }; }
            else if (button == ButtonAudioStageUpSelectPath) { action = value => { systemMusic.AudioStageUpSelectPath = value; }; }
            else if (button == ButtonAudioStageUpSecretPath) { action = value => { systemMusic.AudioStageUpSecretPath = value; }; }
            else if (button == ButtonAudioSeeYouPath)        { action = value => { systemMusic.AudioSeeYouPath = value; }; }
            
            if (action == null) return;
            
            if (systemMusic.AbsoluteSourcePath == "")
            {
                // Define new source path.
                string newSourcePath = Path.Combine(Path.GetDirectoryName(files[0].Path.LocalPath) ?? "", "system_music.toml");
                
                GenericEditOperation<string> op0 = new(value => { systemMusic.AbsoluteSourcePath = value; }, systemMusic.AbsoluteSourcePath, newSourcePath);
                GenericEditOperation<string> op1 = new(action, oldLocalPath, Path.GetFileName(files[0].Path.LocalPath));
                
                UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string directoryPath = Path.GetDirectoryName(systemMusic.AbsoluteSourcePath) ?? "";
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
            LoggingSystem.WriteSessionLog(ex.ToString());
        }
    }
#endregion UI Event Handlers
}