using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Dock.Model;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Serializer;
using FluentIcons.Common;
using SaturnData.Notation.Core;
using SaturnData.Notation.Serialization;
using SaturnEdit.Systems;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ExportArgs;
using SaturnEdit.Windows.Dialogs.ImportArgs;
using SaturnEdit.Windows.Dialogs.ModalDialog;
using SaturnEdit.Windows.Dialogs.SelectByCriteria;
using SaturnEdit.Windows.Main.ChartEditor.Tabs;

namespace SaturnEdit.Windows.ChartEditor;

// TODO: Investigate bug that causes editor to freeze if you close a file dialog.
public partial class ChartEditorView : UserControl
{
    public ChartEditorView()
    {
        InitializeComponent();

        KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        ChartSystem.EntryChanged += OnEntryChanged;
        OnEntryChanged(null, EventArgs.Empty);
        
        UndoRedoSystem.OperationHistoryChanged += OnOperationHistoryChanged;
        OnOperationHistoryChanged(null, EventArgs.Empty);
        
        serializer = new(typeof(AvaloniaList<>));
        dockState = new();

        IDock? layout = DockControl?.Layout;
        if (layout != null)
        {
            dockState.Save(layout);
        }
    }

    private MainWindow? mainWindow;
    private readonly DockSerializer serializer;
    private readonly DockState dockState;
    
#region Methods
    public void SetMainWindow(MainWindow m) => mainWindow = m;

    public async void File_New()
    {
        // Prompt to save an unsaved chart first.
        if (!ChartSystem.IsSaved)
        {
            ModalDialogResult result = await PromptSave();

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

        // Open new chart dialog.

        // Create new chart.
    }

    public async Task<bool> File_Open()
    {
        try
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return false;

            // Prompt to save an unsaved chart first.
            if (!ChartSystem.IsSaved)
            {
                ModalDialogResult result = await PromptSave();

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
                    new("Chart Files")
                    {
                        Patterns = ["*.sat", "*.mer*", "*.map"],
                    },
                ],
            });
            if (files.Count != 1) return false;

            // Get Read Args
            NotationReadArgs args = new();
            FormatVersion formatVersion = NotationSerializer.DetectFormatVersion(files[0].Path.LocalPath);
            if (formatVersion == FormatVersion.Unknown) return false;
            if (formatVersion != FormatVersion.SatV3)
            {
                if (VisualRoot is not Window rootWindow) return false;
                ImportArgsWindow importArgsWindow = new();
                await importArgsWindow.ShowDialog(rootWindow);

                if (importArgsWindow.DialogResult != ModalDialogResult.Primary) return false;
                
                args = importArgsWindow.NotationReadArgs;
            }
            
            // Read chart from file.
            ChartSystem.ReadChart(files[0].Path.LocalPath, args);
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
            // Save As if chart file doesn't have a path yet.
            if (!File.Exists(ChartSystem.Entry.ChartFile))
            {
                return await File_SaveAs();
            }

            // Write chart to file.
            bool updatePath = !File.Exists(ChartSystem.Entry.ChartFile);
            if (!ChartSystem.WriteChart(ChartSystem.Entry.ChartFile, new(), true, updatePath))
            {
                ShowFileWriteError();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    public async Task<bool> File_SaveAs()
    {
        try
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return false;

            // Open file picker.
            IStorageFile? file = await topLevel.StorageProvider.SaveFilePickerAsync(new()
            {
                DefaultExtension = ".sat",
                SuggestedFileName = ChartSystem.Entry.Difficulty switch
                {
                    Difficulty.None => "chart.sat",
                    Difficulty.Normal => "0_normal.sat",
                    Difficulty.Hard => "1_hard.sat",
                    Difficulty.Expert => "2_expert.sat",
                    Difficulty.Inferno => "3_inferno.sat",
                    Difficulty.WorldsEnd => "4_worldsend.sat",
                    _ => "chart.sat",
                },
                FileTypeChoices =
                [
                    new("Saturn Chart File")
                    {
                        Patterns = ["*.sat"],
                    },
                ],
            });

            if (file == null) return false;

            // Write chart to file.
            if (!ChartSystem.WriteChart(file.Path.LocalPath, new(), true, true))
            {
                ShowFileWriteError();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    public async Task<bool> File_ReloadFromDisk()
    {
        try
        {
            if (!File.Exists(ChartSystem.Entry.ChartPath)) return false;

            // Prompt to save an unsaved chart first.
            if (!ChartSystem.IsSaved)
            {
                ModalDialogResult result = await PromptSave();

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

            NotationReadArgs args = new()
            {
                InferClearThresholdFromDifficulty = false,
                OptimizeHoldNotes = false,
                SortCollections = true,
            };

            ChartSystem.ReadChart(ChartSystem.Entry.ChartFile, args);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    public async Task<bool> File_Export()
    {
        try
        {
            // Open an export dialog.
            if (VisualRoot is not Window rootWindow) return false;
            ExportArgsWindow exportArgsWindow = new();
            await exportArgsWindow.ShowDialog(rootWindow);

            // Return if export was cancelled.
            if (exportArgsWindow.DialogResult != ModalDialogResult.Primary) return false;
            if (exportArgsWindow.NotationWriteArgs.FormatVersion == FormatVersion.Unknown) return false;

            // Open the file picker.
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return false;
            IStorageFile? file = await topLevel.StorageProvider.SaveFilePickerAsync(ExportFilePickerSaveOptions(exportArgsWindow.NotationWriteArgs));
            if (file == null) return false;

            // Write the chart to the defined path, with the defined notation arguments.
            if (!ChartSystem.WriteChart(file.Path.LocalPath, exportArgsWindow.NotationWriteArgs, true, true))
            {
                ShowFileWriteError();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    public void File_RenderAsImage()
    {
        throw new NotImplementedException();
    }
    
    public async void File_Quit()
    {
        try
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            // Prompt to save an unsaved chart first.
            if (!ChartSystem.IsSaved)
            {
                ModalDialogResult result = await PromptSave();

                // Cancel
                if (result is ModalDialogResult.Cancel or ModalDialogResult.Tertiary) return;

                // Save
                if (result is ModalDialogResult.Primary)
                {
                    bool success = await File_Save();

                    // Abort quitting if save was unsuccessful.
                    if (!success) return;
                }

                // Don't Save
                // Continue as normal.
            }

            if (VisualRoot is not Window rootWindow) return;
            rootWindow.Close();
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
    }

    public async void Edit_SelectByCriteria()
    {
        if (VisualRoot is not Window rootWindow) return;

        SelectByCriteriaWindow selectByCriteriaWindow = new();
        await selectByCriteriaWindow.ShowDialog(rootWindow);

        if (selectByCriteriaWindow.Result != ModalDialogResult.Primary) return;
        SelectionSystem.SelectByCriteria();
    }
    
    private FilePickerSaveOptions ExportFilePickerSaveOptions(NotationWriteArgs args)
    {
        string defaultExtension = args.FormatVersion switch
        {
            FormatVersion.Mer => ".mer",
            FormatVersion.SatV1 => ".sat",
            FormatVersion.SatV2 => ".sat",
            FormatVersion.SatV3 => ".sat",
            _ => "",
        };

        string suggestedFileName = ChartSystem.Entry.Difficulty switch
        {
            Difficulty.None => $"chart{defaultExtension}",
            Difficulty.Normal => $"0_normal{defaultExtension}",
            Difficulty.Hard => $"1_hard{defaultExtension}",
            Difficulty.Expert => $"2_expert{defaultExtension}",
            Difficulty.Inferno => $"3_inferno{defaultExtension}",
            Difficulty.WorldsEnd => $"4_worldsend{defaultExtension}",
            _ => $"chart.{defaultExtension}",
        };

        string key = args.FormatVersion switch
        {
            FormatVersion.Mer => "Menu.File.MerChartFile",
            FormatVersion.SatV1 => "Menu.File.SaturnChartFile",
            FormatVersion.SatV2 => "Menu.File.SaturnChartFile",
            FormatVersion.SatV3 => "Menu.File.SaturnChartFile",
            _ => "Menu.File.UnknownChartFile",
        };

        TryGetResource(key, ActualThemeVariant, out object? resource);
        string filePickerFileTypeName = resource as string ?? "";

        return new()
        {
            DefaultExtension = defaultExtension,
            SuggestedFileName = suggestedFileName,
            FileTypeChoices =
            [
                new(filePickerFileTypeName)
                {
                    Patterns = [$"*{defaultExtension}"],
                },
            ],
        };
    }

    public async Task<ModalDialogResult> PromptSave()
    {
        if (VisualRoot is not Window rootWindow) return ModalDialogResult.Cancel;

        ModalDialogWindow dialog = new()
        {
            DialogIcon = Icon.Warning,
            WindowTitleKey = "ModalDialog.SavePrompt.Title",
            HeaderKey = "ModalDialog.SavePrompt.Header",
            ParagraphKey = "ModalDialog.SavePrompt.Paragraph",
            ButtonPrimaryKey = "Menu.File.Save",
            ButtonSecondaryKey = "ModalDialog.SavePrompt.DontSave",
            ButtonTertiaryKey = "Generic.Cancel",
        };

        dialog.InitializeDialog();
        await dialog.ShowDialog(rootWindow);
        return dialog.Result;
    }

    private void ShowFileWriteError()
    {
        if (VisualRoot is not Window rootWindow) return;
        
        ModalDialogWindow dialog = new()
        {
            DialogIcon = Icon.Warning,
            WindowTitleKey = "ModalDialog.FileWriteError.Title",
            HeaderKey = "ModalDialog.FileWriteError.Header",
            ParagraphKey = "ModalDialog.FileWriteError.Paragraph",
            ButtonPrimaryKey = "Generic.Ok",
        };

        dialog.InitializeDialog();
        dialog.ShowDialog(rootWindow);
    }
    
    public void CreateNewFloatingTool(UserControl userControl)
    {
        if (DockControl.Factory == null) return;
        if (RootDock.VisibleDockables == null) return;

        Tool tool = new() { Content = userControl };

        ToolDock toolDock = new()
        {
            VisibleDockables = DockControl.Factory?.CreateList<IDockable>(tool),
            ActiveDockable = tool,
        };

        if (RootDock.VisibleDockables.Count != 0)
        {
            Console.WriteLine("RootDock contains dockables!");

            //DockControl.Factory?.AddDockable(RootDock, toolDock);
            //DockControl.Factory?.FloatDockable(toolDock);
        }
        else
        {
            Console.WriteLine("RootDock is empty!");

            //DockControl.Factory?.AddDockable(RootDock, toolDock);
            //DockControl.Factory?.InitLayout(RootDock);
        }
    }
#endregion Methods

#region System Event Delegates
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            MenuItemChartEditorNew.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.New"].ToKeyGesture();
            MenuItemChartEditorOpen.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.Open"].ToKeyGesture();
            MenuItemChartEditorSave.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.Save"].ToKeyGesture();
            MenuItemChartEditorSaveAs.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.SaveAs"].ToKeyGesture();
            MenuItemChartEditorReloadFromDisk.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.ReloadFromDisk"].ToKeyGesture();
            MenuItemChartEditorExport.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.Export"].ToKeyGesture();
            MenuItemChartEditorRenderAsImage.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.RenderAsImage"].ToKeyGesture();
            MenuItemChartEditorSettings.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["QuickCommands.Settings"].ToKeyGesture();
            MenuItemChartEditorQuit.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.Quit"].ToKeyGesture();

            MenuItemChartEditorUndo.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Undo"].ToKeyGesture();
            MenuItemChartEditorRedo.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Redo"].ToKeyGesture();
            MenuItemChartEditorCut.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Cut"].ToKeyGesture();
            MenuItemChartEditorCopy.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Copy"].ToKeyGesture();
            MenuItemChartEditorPaste.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Paste"].ToKeyGesture();
            MenuItemChartEditorSelectAll.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.SelectAll"].ToKeyGesture();
            MenuItemChartEditorDeselectAll.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.DeselectAll"].ToKeyGesture();
            MenuItemChartEditorCheckerDeselect.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.CheckerDeselect"].ToKeyGesture();
            MenuItemChartEditorSelectByCriteria.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.SelectByCriteria"].ToKeyGesture();

            MenuItemChartEditorMoveBeatForward.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.MoveBeatForward"].ToKeyGesture();
            MenuItemChartEditorMoveBeatBack.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.MoveBeatBack"].ToKeyGesture();
            MenuItemChartEditorMoveMeasureForward.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.MoveMeasureForward"].ToKeyGesture();
            MenuItemChartEditorMoveMeasureBack.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.MoveMeasureBack"].ToKeyGesture();
            MenuItemChartEditorJumpToNextNote.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.JumpToNextNote"].ToKeyGesture();
            MenuItemChartEditorJumpToPreviousNote.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.JumpToPreviousNote"].ToKeyGesture();
            MenuItemChartEditorIncreaseBeatDivision.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.IncreaseBeatDivision"].ToKeyGesture();
            MenuItemChartEditorDecreaseBeatDivision.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.DecreaseBeatDivision"].ToKeyGesture();
            MenuItemChartEditorDoubleBeatDivision.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.DoubleBeatDivision"].ToKeyGesture();
            MenuItemChartEditorHalveBeatDivision.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.HalveBeatDivision"].ToKeyGesture();
        });
    }
    
    private void OnEntryChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            MenuItemChartEditorReloadFromDisk.IsEnabled = File.Exists(ChartSystem.Entry.ChartFile);
        });
    }
    
    private void OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            MenuItemChartEditorUndo.IsEnabled = UndoRedoSystem.CanUndo;
            MenuItemChartEditorRedo.IsEnabled = UndoRedoSystem.CanRedo;
        });
    }
#endregion System Event Delegates
    
#region UI Event Delegates
    private void Control_OnKeyDown(object? sender, KeyEventArgs e)
    {
        IInputElement? focusedElement = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (KeyDownBlacklist.IsInvalidFocusedElement(focusedElement)) return;
        if (KeyDownBlacklist.IsInvalidKey(e.Key)) return;
        
        Shortcut shortcut = new(e.Key, e.KeyModifiers.HasFlag(KeyModifiers.Control), e.KeyModifiers.HasFlag(KeyModifiers.Alt), e.KeyModifiers.HasFlag(KeyModifiers.Shift));
        
        if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Playback.Pause"]) && TimeSystem.PlaybackState != PlaybackState.Stopped)
        {
            TimeSystem.PlaybackState = PlaybackState.Stopped;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Playback.Play"]) && TimeSystem.PlaybackState != PlaybackState.Playing)
        {
            TimeSystem.PlaybackState = PlaybackState.Playing;
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["File.New"]))
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
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["File.Export"]))
        {
            _ = File_Export();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["File.RenderAsImage"]))
        {
            File_RenderAsImage();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["File.Quit"]))
        {
            File_Quit();
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Edit.Undo"]))
        {
            UndoRedoSystem.Undo();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Edit.Redo"]))
        {
            UndoRedoSystem.Redo();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Edit.Cut"]))
        {
            EditorSystem.Edit_Cut();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Edit.Copy"]))
        {
            EditorSystem.Edit_Copy();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Edit.Paste"]))
        {
            EditorSystem.Edit_Paste();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Edit.SelectAll"]))
        {
            SelectionSystem.SelectAll();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Edit.DeselectAll"]))
        {
            SelectionSystem.DeselectAll();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Edit.CheckerDeselect"]))
        {
            SelectionSystem.CheckerDeselect();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Edit.SelectByCriteria"]))
        { 
            Edit_SelectByCriteria();
            e.Handled = true;
        }
    }
    
    private void Control_OnKeyUp(object? sender, KeyEventArgs e) => e.Handled = true;
    
    private void MenuItemChartEditorToolWindows_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;
        UserControl? userControl = menuItem.Name switch
        {
            "MenuItemChartEditorChartView3D" => new ChartView3D(),
            "MenuItemChartEditorChartView2D" => new ChartView2D(),
            "MenuItemChartEditorChartViewTxt" => new ChartViewTxt(),
            "MenuItemChartEditorChartProperties" => new ChartPropertiesView(),
            "MenuItemChartEditorChartStatistics" => new ChartStatisticsView(),
            "MenuItemChartEditorProofreader" => new ProofreaderView(),
            "MenuItemChartEditorEventList" => new EventListView(),
            "MenuItemChartEditorLayerList" => new LayerListView(),
            "MenuItemChartEditorInspector" => new InspectorView(),
            "MenuItemChartEditorCursor" => new CursorView(),
            "MenuItemChartEditorAudioMixer" => new AudioMixerView(),
            "MenuItemChartEditorWaveform" => new WaveformView(),
            _ => null,
        };

        if (userControl == null) return;
        CreateNewFloatingTool(userControl);
    }

    private void MenuItemChartEditorNew_OnClick(object? sender, RoutedEventArgs e) => File_New();

    private void MenuItemChartEditorOpen_OnClick(object? sender, RoutedEventArgs e) => _ = File_Open();

    private void MenuItemChartEditorSave_OnClick(object? sender, RoutedEventArgs e) => _ = File_Save();

    private void MenuItemChartEditorSaveAs_OnClick(object? sender, RoutedEventArgs e) => _ = File_SaveAs();

    private void MenuItemChartEditorReloadFromDisk_OnClick(object? sender, RoutedEventArgs e) => _ = File_ReloadFromDisk();

    private void MenuItemChartEditorExport_OnClick(object? sender, RoutedEventArgs e) => _ = File_Export();

    private void MenuItemSettings_OnClick(object? sender, RoutedEventArgs e)
    {
        mainWindow?.ShowSettingsWindow();
    }

    private void MenuItemChartEditorQuit_OnClick(object? sender, RoutedEventArgs e) => File_Quit();
    
    private void MenuItemChartEditorUndo_OnClick(object? sender, RoutedEventArgs e) => UndoRedoSystem.Undo();

    private void MenuItemChartEditorRedo_OnClick(object? sender, RoutedEventArgs e) => UndoRedoSystem.Redo();

    private void MenuItemChartEditorCut_OnClick(object? sender, RoutedEventArgs e) => EditorSystem.Edit_Cut();

    private void MenuItemChartEditorCopy_OnClick(object? sender, RoutedEventArgs e) => EditorSystem.Edit_Copy();

    private void MenuItemChartEditorPaste_OnClick(object? sender, RoutedEventArgs e) => EditorSystem.Edit_Paste();

    private void MenuItemChartEditorSelectAll_OnClick(object? sender, RoutedEventArgs e) => SelectionSystem.SelectAll();

    private void MenuItemChartEditorDeselectAll_OnClick(object? sender, RoutedEventArgs e) => SelectionSystem.DeselectAll();

    private void MenuItemChartEditorCheckerDeselect_OnClick(object? sender, RoutedEventArgs e) => SelectionSystem.CheckerDeselect();

    private void MenuItemChartEditorSelectByCriteria_OnClick(object? sender, RoutedEventArgs e) => Edit_SelectByCriteria();
#endregion UI Event Delegates
}