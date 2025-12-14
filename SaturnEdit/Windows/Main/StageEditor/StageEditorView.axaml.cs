using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SaturnData.Notation.Core;
using SaturnData.Notation.Serialization;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.GenericOperations;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.StageEditor;

public partial class StageEditorView : UserControl
{
    public StageEditorView()
    {
        InitializeComponent();
    }

    private bool blockEvents = false;
    
    private IDisposable? keyDownEventHandler = null;
    private IDisposable? keyUpEventHandler = null;
    
#region Methods
    private async void File_New()
    {
        // Prompt to save an unsaved stage first.
        if (!StageSystem.IsSaved)
        {
            ModalDialogResult result = await MainWindow.ShowSavePrompt(SavePromptType.Stage);

            // Cancel
            if (result is ModalDialogResult.Cancel or ModalDialogResult.Tertiary) return;

            // Save
            if (result is ModalDialogResult.Primary)
            {
                bool success = await File_Save();

                // Abort opening new file if save was unsuccessful.
                if (!success) return;
            }

            // Don't Save
            // Continue as normal.
        }
        
        StageSystem.NewStage();
    }

    private async Task<bool> File_Open()
    {
        try
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return false;

            // Prompt to save an unsaved stage first.
            if (!StageSystem.IsSaved)
            {
                ModalDialogResult result = await MainWindow.ShowSavePrompt(SavePromptType.Stage);

                // Cancel
                if (result is ModalDialogResult.Cancel or ModalDialogResult.Tertiary) return false;

                // Save
                if (result is ModalDialogResult.Primary)
                {
                    bool success = await File_Save();

                    // Abort opening new file if save was unsuccessful.
                    if (!success) return false;
                }

                // Don't Save
                // Continue as normal.
            }

            // Open file picker.
            IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new()
            {
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new("Stage Up Stage Files")
                    {
                        Patterns = ["*.toml"],
                    },
                ],
            });
            if (files.Count != 1) return false;
            
            // Read stage from file.
            StageSystem.ReadStage(files[0].Path.LocalPath);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
    
    private async void File_LoadMusicData()
    {
        try
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            // Open file picker.
            IReadOnlyList<IStorageFolder> folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new()
            {
                AllowMultiple = false,
            });
            if (folders.Count != 1) return;

            StageSystem.MusicData.Load(folders[0].Path.LocalPath);
            UpdateStagePreview();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async Task<bool> File_Save()
    {
        try
        {
            // Redirect to 'Save As' if stage doesn't have a file defined yet.
            if (!File.Exists(StageSystem.StageUpStage.AbsoluteSourcePath))
            {
                return await File_SaveAs();
            }

            // Write stage to file.
            if (!StageSystem.WriteStage(StageSystem.StageUpStage.AbsoluteSourcePath, true, false))
            {
                MainWindow.ShowFileWriteError();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            MainWindow.ShowFileWriteError();
            return false;
        }
    }

    private async Task<bool> File_SaveAs()
    {
        try
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return false;

            // Open file picker.
            IStorageFile? file = await topLevel.StorageProvider.SaveFilePickerAsync(new()
            {
                DefaultExtension = ".toml",
                SuggestedFileName = "stage.toml",
                FileTypeChoices =
                [
                    new("Stage Up Stage File")
                    {
                        Patterns = ["*.toml"],
                    },
                ],
            });

            if (file == null) return false;

            // Write stage to file.
            if (!StageSystem.WriteStage(file.Path.LocalPath, true, true))
            {
                MainWindow.ShowFileWriteError();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            MainWindow.ShowFileWriteError();
            return false;
        }
    }

    private async Task<bool> File_ReloadFromDisk()
    {
        try
        {
            if (!File.Exists(StageSystem.StageUpStage.AbsoluteSourcePath)) return false;

            // Prompt to save an unsaved stage first.
            if (!StageSystem.IsSaved)
            {
                ModalDialogResult result = await MainWindow.ShowSavePrompt(SavePromptType.Stage);

                // Cancel
                if (result is ModalDialogResult.Cancel or ModalDialogResult.Tertiary) return false;

                // Save
                if (result is ModalDialogResult.Primary)
                {
                    bool success = await File_Save();

                    // Abort opening new file if save was unsuccessful.
                    if (!success) return false;
                }

                // Don't Save
                // Continue as normal.
            }

            StageSystem.ReadStage(StageSystem.StageUpStage.AbsoluteSourcePath);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    private void File_Quit()
    {
        if (VisualRoot is not Window rootWindow) return;
        rootWindow.Close();
    }

    private void UpdateStagePreview()
    {
        Dispatcher.UIThread.Post(() =>
        {
            TextBlockStageName.Text = StageSystem.StageUpStage.Name;
            
            TextBlockTitle1.Text = getTitle(StageSystem.StageUpStage.Song1.EntryId, StageSystem.StageUpStage.Song1.Secret);
            TextBlockTitle2.Text = getTitle(StageSystem.StageUpStage.Song2.EntryId, StageSystem.StageUpStage.Song2.Secret);
            TextBlockTitle3.Text = getTitle(StageSystem.StageUpStage.Song3.EntryId, StageSystem.StageUpStage.Song3.Secret);
            
            TextBlockLevel1.Text = getLevel(StageSystem.StageUpStage.Song1.EntryId, StageSystem.StageUpStage.Song1.Secret);
            TextBlockLevel2.Text = getLevel(StageSystem.StageUpStage.Song2.EntryId, StageSystem.StageUpStage.Song2.Secret);
            TextBlockLevel3.Text = getLevel(StageSystem.StageUpStage.Song3.EntryId, StageSystem.StageUpStage.Song3.Secret);
            
            Difficulty difficulty1 = getDifficulty(StageSystem.StageUpStage.Song1.EntryId, StageSystem.StageUpStage.Song1.Secret);
            Difficulty difficulty2 = getDifficulty(StageSystem.StageUpStage.Song2.EntryId, StageSystem.StageUpStage.Song2.Secret);
            Difficulty difficulty3 = getDifficulty(StageSystem.StageUpStage.Song3.EntryId, StageSystem.StageUpStage.Song3.Secret);

            ImageDifficultyTopNone.IsVisible = difficulty1 == Difficulty.None; 
            ImageDifficultyTopNormal.IsVisible = difficulty1 == Difficulty.Normal; 
            ImageDifficultyTopHard.IsVisible = difficulty1 == Difficulty.Hard; 
            ImageDifficultyTopExpert.IsVisible = difficulty1 == Difficulty.Expert; 
            ImageDifficultyTopInferno.IsVisible = difficulty1 == Difficulty.Inferno; 
            ImageDifficultyTopWorldsEnd.IsVisible = difficulty1 == Difficulty.WorldsEnd; 
            
            ImageDifficultyMiddleNone.IsVisible = difficulty2 == Difficulty.None; 
            ImageDifficultyMiddleNormal.IsVisible = difficulty2 == Difficulty.Normal; 
            ImageDifficultyMiddleHard.IsVisible = difficulty2 == Difficulty.Hard; 
            ImageDifficultyMiddleExpert.IsVisible = difficulty2 == Difficulty.Expert; 
            ImageDifficultyMiddleInferno.IsVisible = difficulty2 == Difficulty.Inferno; 
            ImageDifficultyMiddleWorldsEnd.IsVisible = difficulty2 == Difficulty.WorldsEnd; 
                
            ImageDifficultyBottomNone.IsVisible = difficulty3 == Difficulty.None; 
            ImageDifficultyBottomNormal.IsVisible = difficulty3 == Difficulty.Normal; 
            ImageDifficultyBottomHard.IsVisible = difficulty3 == Difficulty.Hard; 
            ImageDifficultyBottomExpert.IsVisible = difficulty3 == Difficulty.Expert; 
            ImageDifficultyBottomInferno.IsVisible = difficulty3 == Difficulty.Inferno; 
            ImageDifficultyBottomWorldsEnd.IsVisible = difficulty3 == Difficulty.WorldsEnd; 
        });

        return;

        string getTitle(string id, bool secret)
        {
            if (secret) return "??";
            if (string.IsNullOrEmpty(id)) return "NO ENTRY FOUND";
            if (!StageSystem.MusicData.Entries.TryGetValue(id, out Entry? entry)) return "NO ENTRY FOUND";

            return entry.Title;
        }

        string getLevel(string id, bool secret)
        {
            if (secret) return "??";
            if (string.IsNullOrEmpty(id)) return "??";
            if (!StageSystem.MusicData.Entries.TryGetValue(id, out Entry? entry)) return "??";

            return entry.LevelString;
        }
        
        Difficulty getDifficulty(string id, bool secret)
        {
            if (secret) return Difficulty.None;
            if (string.IsNullOrEmpty(id)) return Difficulty.None;
            if (!StageSystem.MusicData.Entries.TryGetValue(id, out Entry? entry)) return Difficulty.None;

            return entry.Difficulty;
        }
    }
#endregion Methods
    
#region System Event Handlers
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            MenuItemNew.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.New"].ToKeyGesture();
            MenuItemOpen.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.Open"].ToKeyGesture();
            MenuItemSave.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.Save"].ToKeyGesture();
            MenuItemSaveAs.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.SaveAs"].ToKeyGesture();
            MenuItemReloadFromDisk.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.ReloadFromDisk"].ToKeyGesture();
            MenuItemSettings.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["QuickCommands.Settings"].ToKeyGesture();
            MenuItemQuit.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.Quit"].ToKeyGesture();

            MenuItemUndo.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Undo"].ToKeyGesture();
            MenuItemRedo.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Redo"].ToKeyGesture();
        });
    }
    
    private void StageBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            MenuItemReloadFromDisk.IsEnabled = File.Exists(StageSystem.StageUpStage.AbsoluteSourcePath);
            
            TextBoxStageId.Text = StageSystem.StageUpStage.Id;
            TextBoxStageName.Text = StageSystem.StageUpStage.Name;
            TextBoxStageIconPath.Text = StageSystem.StageUpStage.IconPath;

            TextBoxSongId1.Text = StageSystem.StageUpStage.Song1.EntryId;
            ToggleButtonSecret1.IsChecked = StageSystem.StageUpStage.Song1.Secret;
            
            TextBoxSongId2.Text = StageSystem.StageUpStage.Song2.EntryId;
            ToggleButtonSecret2.IsChecked = StageSystem.StageUpStage.Song2.Secret;
            
            TextBoxSongId3.Text = StageSystem.StageUpStage.Song3.EntryId;
            ToggleButtonSecret3.IsChecked = StageSystem.StageUpStage.Song3.Secret;

            TextBoxHealth.Text = StageSystem.StageUpStage.Health.ToString(CultureInfo.InvariantCulture);
            TextBoxHealthRecovery.Text = StageSystem.StageUpStage.HealthRecovery.ToString(CultureInfo.InvariantCulture);
            ComboBoxErrorThreshold.SelectedIndex = StageSystem.StageUpStage.ErrorThreshold switch
            {
                JudgementGrade.Unjudged => 2,
                JudgementGrade.Miss => 2,
                JudgementGrade.Good => 1,
                JudgementGrade.Great => 0,
                JudgementGrade.Marvelous => 0,
                _ => 0,
            };
            
            bool iconExists = File.Exists(StageSystem.StageUpStage.AbsoluteIconPath);
            IconFileNotFoundWarning.IsVisible = StageSystem.StageUpStage.AbsoluteIconPath != "" && !iconExists;
            
            try
            {
                ImageIcon.Source = iconExists ? new Bitmap(StageSystem.StageUpStage.AbsoluteIconPath) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
            blockEvents = false;
        });
        
        UpdateStagePreview();
    }
#endregion System Event Handlers
    
#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        keyDownEventHandler = KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        keyUpEventHandler = KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
        
        AddHandler(DragDrop.DropEvent, Control_Drop);
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        UndoRedoSystem.StageBranch.OperationHistoryChanged += StageBranch_OnOperationHistoryChanged;
        StageBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        RemoveHandler(DragDrop.DropEvent, Control_Drop);
        SettingsSystem.SettingsChanged -= OnSettingsChanged;
        UndoRedoSystem.StageBranch.OperationHistoryChanged -= StageBranch_OnOperationHistoryChanged;
        keyDownEventHandler?.Dispose();
        keyUpEventHandler?.Dispose();
        
        base.OnUnloaded(e);
    }
    
    private void Control_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsEnabled) return;
        
        IInputElement? focusedElement = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (KeyDownBlacklist.IsInvalidFocusedElement(focusedElement)) return;
        if (KeyDownBlacklist.IsInvalidKey(e.Key)) return;
        if (KeyDownBlacklist.IsInvalidState()) return;
        
        Shortcut shortcut = new(e.Key, e.KeyModifiers.HasFlag(KeyModifiers.Control), e.KeyModifiers.HasFlag(KeyModifiers.Alt), e.KeyModifiers.HasFlag(KeyModifiers.Shift));
        
        if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["File.New"]))
        {
            File_New();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["File.Open"]))
        {
            _ = File_Open();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["File.Save"]))
        {
            _ = File_Save();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["File.SaveAs"]))
        {
            _ = File_SaveAs();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["File.ReloadFromDisk"]))
        {
            _ = File_ReloadFromDisk();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["File.Quit"]))
        {
            File_Quit();
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Edit.Undo"]))
        {
            UndoRedoSystem.StageBranch.Undo();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Edit.Redo"]))
        {
            UndoRedoSystem.StageBranch.Redo();
            e.Handled = true;
        }
    }
    
    private void Control_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (!IsEnabled) return;
        
        e.Handled = true;
    }

    private async void Control_Drop(object? sender, DragEventArgs e)
    {
        try
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null)
            {
                e.Handled = true;
                return;
            }
            
            IStorageItem? file = e.DataTransfer.TryGetFile();

            // Prompt to save an unsaved stage first.
            if (!StageSystem.IsSaved)
            {
                ModalDialogResult result = await MainWindow.ShowSavePrompt(SavePromptType.Stage);

                // Cancel
                if (result is ModalDialogResult.Cancel or ModalDialogResult.Tertiary)
                {
                    e.Handled = true;
                    return;
                }

                // Save
                if (result is ModalDialogResult.Primary)
                {
                    bool success = await File_Save();

                    // Abort opening new file if save was unsuccessful.
                    if (!success)
                    {
                        e.Handled = true;
                        return;
                    }
                }

                // Don't Save
                // Continue as normal.
            }

            if (file == null)
            {
                e.Handled = true;
                return;
            }

            if (!File.Exists(file.Path.LocalPath))
            {
                e.Handled = true;
                return;
            }

            // Get Read Args
            StageSystem.ReadStage(file.Path.LocalPath);

            e.Handled = true;
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
    }
    
    
    private void MenuItemNew_OnClick(object? sender, RoutedEventArgs e) => File_New();

    private void MenuItemOpen_OnClick(object? sender, RoutedEventArgs e) => _ = File_Open();
    
    private void MenuItemLoadMusicData_OnClick(object? sender, RoutedEventArgs e) => File_LoadMusicData();
    
    private void MenuItemSave_OnClick(object? sender, RoutedEventArgs e) => _ = File_Save();

    private void MenuItemSaveAs_OnClick(object? sender, RoutedEventArgs e) => _ = File_SaveAs();

    private void MenuItemReloadFromDisk_OnClick(object? sender, RoutedEventArgs e) => _ = File_ReloadFromDisk();

    private void MenuItemSettings_OnClick(object? sender, RoutedEventArgs e) => MainWindow.Instance?.ShowSettingsWindow();

    private void MenuItemQuit_OnClick(object? sender, RoutedEventArgs e) => File_Quit();

    
    private void MenuItemUndo_OnClick(object? sender, RoutedEventArgs e) => UndoRedoSystem.StageBranch.Undo();

    private void MenuItemRedo_OnClick(object? sender, RoutedEventArgs e) => UndoRedoSystem.StageBranch.Redo();
    
    
    private void TextBoxStageId_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxStageId == null) return;
        
        string oldId = StageSystem.StageUpStage.Id;
        string newId = TextBoxStageId.Text ?? "";
        if (oldId == newId) return;
        
        UndoRedoSystem.StageBranch.Push(new GenericEditOperation<string>(value => { StageSystem.StageUpStage.Id = value; }, oldId, newId));
    }

    private void ButtonRegenerateStageId_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;

        string oldId = StageSystem.StageUpStage.Id;
        string newId = Guid.NewGuid().ToString();
        if (oldId == newId) return;

        UndoRedoSystem.StageBranch.Push(new GenericEditOperation<string>(value => { StageSystem.StageUpStage.Id = value; }, oldId, newId));
    }
    
    private void TextBoxStageName_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxStageName == null) return;

        string oldName = StageSystem.StageUpStage.Name;
        string newName = TextBoxStageName.Text ?? "";
        if (oldName == newName) return;
        
        UndoRedoSystem.StageBranch.Push(new GenericEditOperation<string>(value => { StageSystem.StageUpStage.Name = value; }, oldName, newName));
    }
    
    private void TextBoxStageIcon_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxStageIconPath == null) return;

        string oldIconPath = StageSystem.StageUpStage.IconPath;
        string newIconPath = TextBoxStageIconPath.Text ?? "";
        if (oldIconPath == newIconPath)
        {
            // Refresh UI in case the file changed, but don't push unnecessary operation.
            StageBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
            return;
        }

        UndoRedoSystem.StageBranch.Push(new GenericEditOperation<string>(value => { StageSystem.StageUpStage.IconPath = value; }, oldIconPath, newIconPath));
    }
    
    private async void ButtonPickStageIconFile_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            
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
            
            if (StageSystem.StageUpStage.AbsoluteIconPath == files[0].Path.LocalPath)
            {
                // Refresh UI in case the file changed, but don't push unnecessary operation.
                StageBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
                return;
            }
            
            if (StageSystem.StageUpStage.AbsoluteSourcePath == "")
            {
                // Define new source path.
                string newSourcePath = Path.Combine(Path.GetDirectoryName(files[0].Path.LocalPath) ?? "", "stage.toml");

                GenericEditOperation<string> op0 = new(value => { StageSystem.StageUpStage.AbsoluteSourcePath = value; }, StageSystem.StageUpStage.AbsoluteSourcePath, newSourcePath);
                GenericEditOperation<string> op1 = new(value => { StageSystem.StageUpStage.IconPath = value; }, StageSystem.StageUpStage.IconPath, Path.GetFileName(files[0].Path.LocalPath));
                 
                UndoRedoSystem.StageBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string directoryPath = Path.GetDirectoryName(StageSystem.StageUpStage.AbsoluteSourcePath) ?? "";
                string localPath = Path.GetRelativePath(directoryPath, files[0].Path.LocalPath);
                string pathFromRootDirectory = Path.Combine(directoryPath, localPath);

                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                if (!await MainWindow.PromptFileMoveAndOverwrite(files[0].Path.LocalPath, pathFromRootDirectory)) return;

                UndoRedoSystem.StageBranch.Push(new GenericEditOperation<string>(value => { StageSystem.StageUpStage.IconPath = value; }, StageSystem.StageUpStage.IconPath, localPath));
            }
        }
        catch (Exception ex)
        {
            // don't throw
            Console.WriteLine(ex);
        }
    }

    private void TextBoxSongId_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not TextBox textBox) return;

        int index = -1;
        string oldEntryId = "";
        string newEntryId = textBox.Text ?? "";
        
        if      (textBox == TextBoxSongId1)
        {
            index = 0;
            oldEntryId = StageSystem.StageUpStage.Song1.EntryId;
        }
        else if (textBox == TextBoxSongId2)
        {
            index = 1;
            oldEntryId = StageSystem.StageUpStage.Song2.EntryId;
        }
        else if (textBox == TextBoxSongId3)
        {
            index = 2;
            oldEntryId = StageSystem.StageUpStage.Song3.EntryId;
        }

        if (index == -1) return;
        if (oldEntryId == newEntryId) return;

        UndoRedoSystem.StageBranch.Push(new GenericEditOperation<string>(value => { StageSystem.StageUpStage.SongById(index).EntryId = value; }, oldEntryId, newEntryId));
    }
    
    private async void ButtonPickChartFile_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (sender is not Button button) return;

            int index = -1;
            string oldEntryId = "";

            if      (button == ButtonPickChartFile1)
            {
                index = 0;
                oldEntryId = StageSystem.StageUpStage.Song1.EntryId;
            }
            else if (button == ButtonPickChartFile2)
            {
                index = 1;
                oldEntryId = StageSystem.StageUpStage.Song2.EntryId;
            }
            else if (button == ButtonPickChartFile3)
            {
                index = 2;
                oldEntryId = StageSystem.StageUpStage.Song3.EntryId;
            }
            
            if (index == -1) return;
        
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            // Open file picker.
            IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new()
            {
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new("Saturn Chart Files")
                    {
                        Patterns = ["*.sat"],
                    },
                ],
            });
            if (files.Count != 1) return;

            // Get Read Args
            NotationReadArgs args = new();
            FormatVersion formatVersion = NotationSerializer.DetectFormatVersion(files[0].Path.LocalPath);
            if (formatVersion is FormatVersion.Unknown or FormatVersion.Mer) return;
            
            // Get Entry
            Entry entry = NotationSerializer.ToEntry(files[0].Path.LocalPath, args, out _);
            
            string newEntryId = entry.Id;
            if (oldEntryId == newEntryId) return;

            UndoRedoSystem.StageBranch.Push(new GenericEditOperation<string>(value => { StageSystem.StageUpStage.SongById(index).EntryId = value; }, oldEntryId, newEntryId));
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
    }

    private void ToggleButtonSecret_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not ToggleButton button) return;

        int index = -1;
        bool oldSecret = false;
        bool newSecret = button.IsChecked ?? false;
        
        if      (button == ToggleButtonSecret1)
        {
            index = 0;
            oldSecret = StageSystem.StageUpStage.Song1.Secret;
        }
        else if (button == ToggleButtonSecret2)
        {
            index = 1;
            oldSecret = StageSystem.StageUpStage.Song2.Secret;
        }
        else if (button == ToggleButtonSecret3)
        {
            index = 2;
            oldSecret = StageSystem.StageUpStage.Song3.Secret;
        }

        if (index == -1) return;
        if (oldSecret == newSecret) return;

        UndoRedoSystem.StageBranch.Push(new GenericEditOperation<bool>(value => { StageSystem.StageUpStage.SongById(index).Secret = value; }, oldSecret, newSecret));
    }

    private void TextBoxHealth_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxHealth == null) return;

        try
        {
            int oldHealth = StageSystem.StageUpStage.Health;
            int newHealth = Convert.ToInt32(TextBoxHealth.Text ?? "", CultureInfo.InvariantCulture);
            if (oldHealth == newHealth) return;
        
            UndoRedoSystem.StageBranch.Push(new GenericEditOperation<int>(value => { StageSystem.StageUpStage.Health = value; }, oldHealth, newHealth));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.StageBranch.Push(new GenericEditOperation<int>(value => { StageSystem.StageUpStage.Health = value; }, StageSystem.StageUpStage.Health, 100));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void TextBoxHealthRecovery_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxHealthRecovery == null) return;

        try
        {
            int oldHealthRecovery = StageSystem.StageUpStage.HealthRecovery;
            int newHealthRecovery = Convert.ToInt32(TextBoxHealthRecovery.Text ?? "", CultureInfo.InvariantCulture);
            if (oldHealthRecovery == newHealthRecovery) return;
        
            UndoRedoSystem.StageBranch.Push(new GenericEditOperation<int>(value => { StageSystem.StageUpStage.HealthRecovery = value; }, oldHealthRecovery, newHealthRecovery));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.StageBranch.Push(new GenericEditOperation<int>(value => { StageSystem.StageUpStage.HealthRecovery = value; }, StageSystem.StageUpStage.HealthRecovery, 10));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void ComboBoxErrorThreshold_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxErrorThreshold == null) return;

        JudgementGrade oldErrorThreshold = StageSystem.StageUpStage.ErrorThreshold;
        JudgementGrade newErrorThreshold = ComboBoxErrorThreshold.SelectedIndex switch
        {
            0 => JudgementGrade.Great,
            1 => JudgementGrade.Good,
            2 => JudgementGrade.Miss,
            _ => JudgementGrade.Miss,
        };
        
        if (oldErrorThreshold == newErrorThreshold) return;
        
        UndoRedoSystem.StageBranch.Push(new GenericEditOperation<JudgementGrade>(value => { StageSystem.StageUpStage.ErrorThreshold = value; }, oldErrorThreshold, newErrorThreshold));
    }
#endregion UI Event Handlers
}