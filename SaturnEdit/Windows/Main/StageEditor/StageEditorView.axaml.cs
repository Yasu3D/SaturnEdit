using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SaturnData.Notation.Serialization;
using SaturnEdit.Systems;
using SaturnEdit.Windows.Dialogs.ImportArgs;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.StageEditor;

public partial class StageEditorView : UserControl
{
    public StageEditorView()
    {
        InitializeComponent();

        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

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
#endregion System Event Handlers
    
#region UI Event Handlers
    private void MenuItemNew_OnClick(object? sender, RoutedEventArgs e) => File_New();

    private void MenuItemOpen_OnClick(object? sender, RoutedEventArgs e) => _ = File_Open();

    private void MenuItemClearRecent_OnClick(object? sender, RoutedEventArgs e) => SettingsSystem.EditorSettings.ClearRecentStageFiles();

    private async void MenuItemOpenRecent_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem item) return;
            if (item.Tag is not string path) return;
            if (!File.Exists(path)) return;
        
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
    
    
    private void ButtonRegenerateStageId_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }

    private void ButtonStageIcon_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }
    
    private void TextBoxStageId_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        
    }

    private void TextBoxStageName_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        
    }

    private void TextBoxStageIcon_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        
    }

    private void ButtonPickChartFile_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }

    private void ToggleButtonSecret_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        
    }

    private void TextBoxHealth_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        
    }

    private void TextBoxHealthRecovery_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        
    }

    private void ComboBoxErrorThreshold_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        
    }
#endregion UI Event Handlers
}