using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SaturnEdit.Systems;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ImportArgs;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.CosmeticsEditor;

public partial class CosmeticsEditorView : UserControl
{
    public CosmeticsEditorView()
    {
        InitializeComponent();

        KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
        
        AddHandler(DragDrop.DropEvent, Control_Drop);
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

#region Methods
    // TODO.
    private async Task<bool> File_Open()
    {
        return false;
    }

    // TODO.
    public async Task<bool> File_Save()
    {
        return false;
    }

    // TODO.
    private async Task<bool> File_SaveAs()
    {
        return false;
    }

    // TODO.
    private async Task<bool> File_ReloadFromDisk()
    {
        return false;
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

            if (SettingsSystem.EditorSettings.RecentCosmeticFiles.Count != 0)
            {
                // Reverse loop so most recent file appears at the top.
                for (int i = SettingsSystem.EditorSettings.RecentCosmeticFiles.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        string file = SettingsSystem.EditorSettings.RecentCosmeticFiles[i];
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
    private void Control_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsEnabled) return;
        
        IInputElement? focusedElement = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (KeyDownBlacklist.IsInvalidFocusedElement(focusedElement)) return;
        if (KeyDownBlacklist.IsInvalidKey(e.Key)) return;
        if (KeyDownBlacklist.IsInvalidState()) return;
        
        Shortcut shortcut = new(e.Key, e.KeyModifiers.HasFlag(KeyModifiers.Control), e.KeyModifiers.HasFlag(KeyModifiers.Alt), e.KeyModifiers.HasFlag(KeyModifiers.Shift));
        
        if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["File.Open"]))
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
            UndoRedoSystem.CosmeticBranch.Undo();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Edit.Redo"]))
        {
            UndoRedoSystem.CosmeticBranch.Redo();
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

            // TODO

            e.Handled = true;
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
    }
    
    
    private void MenuItemNewConsoleColor_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO
    }

    private void MenuItemNewEmblem_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO
    }

    private void MenuItemNewIcon_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO
    }

    private void MenuItemNewNavigator_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO
    }

    private void MenuItemNewNoteSound_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO
    }

    private void MenuItemNewPlate_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO
    }

    private void MenuItemNewSystemMusic_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO
    }

    private void MenuItemNewSystemSound_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO
    }

    private void MenuItemNewTitle_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO
    }

    private void MenuItemOpen_OnClick(object? sender, RoutedEventArgs e)
    {
        // TODO
    }

    private void MenuItemClearRecent_OnClick(object? sender, RoutedEventArgs e) => SettingsSystem.EditorSettings.ClearRecentCosmeticFiles();

    private async void MenuItemOpenRecent_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem item) return;
            if (item.Tag is not string path) return;
            
            if (!File.Exists(path))
            {
                SettingsSystem.EditorSettings.RemoveRecentCosmeticFile(path);
                return;
            }
        
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;
            
            // TODO.
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


    private void MenuItemUndo_OnClick(object? sender, RoutedEventArgs e) => UndoRedoSystem.CosmeticBranch.Undo();

    private void MenuItemRedo_OnClick(object? sender, RoutedEventArgs e) => UndoRedoSystem.CosmeticBranch.Redo();
#endregion UI Event Handlers
}