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
using SaturnEdit.UndoRedo.StageOperations;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.StageEditor;

public partial class StageEditorView : UserControl
{
    public StageEditorView()
    {
        InitializeComponent();

        KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
        
        AddHandler(DragDrop.DropEvent, Control_Drop);
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        UndoRedoSystem.StageBranch.OperationHistoryChanged += StageBranch_OnOperationHistoryChanged;
        StageBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
    }

    private bool blockEvents = false;
    
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

            // Update recent file list.
            foreach (object? obj in MenuItemRecent.Items)
            {
                if (obj is not MenuItem item) continue;
                item.Click -= MenuItemOpenRecent_OnClick;
            }
            
            MenuItemRecent.Items.Clear();

            if (SettingsSystem.EditorSettings.RecentStageFiles.Count != 0)
            {
                // Reverse loop so most recent file appears at the top.
                for (int i = SettingsSystem.EditorSettings.RecentStageFiles.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        string file = SettingsSystem.EditorSettings.RecentStageFiles[i];
                        string trimmed = $"{Path.GetFileName(Path.GetDirectoryName(file))}/{Path.GetFileName(file)}";

                        MenuItem item = new()
                        {
                            Icon = (i + 1).ToString(CultureInfo.InvariantCulture),
                            Header = new TextBlock { Text = trimmed },
                            Tag = file,
                        };

                        item.Click += MenuItemOpenRecent_OnClick;
                        
                        MenuItemRecent.Items.Add(item);
                    }
                    catch (Exception ex)
                    {
                        // Don't throw.
                        Console.WriteLine(ex);
                    }
                }
                
                MenuItemRecent.Items.Add(new Separator());
                MenuItemRecent.Items.Add(MenuItemClearRecent);
            }
            else
            {
                MenuItemRecent.IsEnabled = false;
            }
        });
    }
    
    private void StageBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

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
                ImageStageIcon.Source = iconExists ? new Bitmap(StageSystem.StageUpStage.AbsoluteIconPath) : null;
                ImageStageIcon.IsVisible = iconExists;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ImageStageIcon.IsVisible = false;
                IconFileNotFoundWarning.IsVisible = true;
            }
            
            blockEvents = false;
        });
    }
#endregion System Event Handlers
    
#region UI Event Handlers
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

    private void MenuItemClearRecent_OnClick(object? sender, RoutedEventArgs e) => SettingsSystem.EditorSettings.ClearRecentStageFiles();

    private async void MenuItemOpenRecent_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem item) return;
            if (item.Tag is not string path) return;
            
            if (!File.Exists(path))
            {
                SettingsSystem.EditorSettings.RemoveRecentStageFile(path);
                return;
            }
        
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

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
            
            // Read stage from file.
            StageSystem.ReadStage(path);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
    }
    
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
        
        UndoRedoSystem.StageBranch.Push(new StageIdEditOperation(StageSystem.StageUpStage.Id, newId));
    }

    private void ButtonRegenerateStageId_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;

        string oldId = StageSystem.StageUpStage.Id;
        string newId = Guid.NewGuid().ToString();
        if (oldId == newId) return;

        UndoRedoSystem.StageBranch.Push(new StageIdEditOperation(oldId, newId));
    }
    
    private void TextBoxStageName_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxStageName == null) return;

        string oldName = StageSystem.StageUpStage.Name;
        string newName = TextBoxStageName.Text ?? "";
        if (oldName == newName) return;
        
        UndoRedoSystem.StageBranch.Push(new StageNameEditOperation(oldName, newName));
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

        UndoRedoSystem.StageBranch.Push(new StageIconPathEditOperation(oldIconPath, newIconPath));
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

                StageSourcePathEditOperation op0 = new(StageSystem.StageUpStage.AbsoluteSourcePath, newSourcePath);
                StageIconPathEditOperation op1 = new(StageSystem.StageUpStage.IconPath, Path.GetFileName(files[0].Path.LocalPath));
                
                UndoRedoSystem.StageBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string filename = Path.GetFileName(files[0].Path.LocalPath);
                string pathFromRootDirectory = Path.Combine(Path.GetDirectoryName(StageSystem.StageUpStage.AbsoluteSourcePath) ?? "", filename);

                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                if (!await MainWindow.PromptFileMoveAndOverwrite(files[0].Path.LocalPath, pathFromRootDirectory)) return;

                UndoRedoSystem.StageBranch.Push(new StageIconPathEditOperation(StageSystem.StageUpStage.IconPath, filename));
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

        UndoRedoSystem.StageBranch.Push(new StageSongEntryIdEditOperation(index, oldEntryId, newEntryId));
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

            UndoRedoSystem.StageBranch.Push(new StageSongEntryIdEditOperation(index, oldEntryId, newEntryId));
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

        UndoRedoSystem.StageBranch.Push(new StageSongSecretEditOperation(index, oldSecret, newSecret));
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
        
            UndoRedoSystem.StageBranch.Push(new StageHealthEditOperation(oldHealth, newHealth));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.StageBranch.Push(new StageHealthEditOperation(StageSystem.StageUpStage.Health, 100));

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
        
            UndoRedoSystem.StageBranch.Push(new StageHealthRecoveryEditOperation(oldHealthRecovery, newHealthRecovery));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.StageBranch.Push(new StageHealthRecoveryEditOperation(StageSystem.StageUpStage.HealthRecovery, 10));

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
        
        UndoRedoSystem.StageBranch.Push(new StageErrorThresholdEditOperation(oldErrorThreshold, newErrorThreshold));
    }
#endregion UI Event Handlers
}