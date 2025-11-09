using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FluentIcons.Common;
using SaturnData.Notation.Core;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnData.Notation.Serialization;
using SaturnEdit.Docking;
using SaturnEdit.Systems;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ChooseMirrorAxis;
using SaturnEdit.Windows.Dialogs.ExportArgs;
using SaturnEdit.Windows.Dialogs.ImportArgs;
using SaturnEdit.Windows.Dialogs.ModalDialog;
using SaturnEdit.Windows.Dialogs.NewChart;
using SaturnEdit.Windows.Dialogs.SelectBookmarkData;
using SaturnEdit.Windows.Dialogs.SelectByCriteria;
using SaturnEdit.Windows.Dialogs.SelectMetre;
using SaturnEdit.Windows.Dialogs.SelectOffset;
using SaturnEdit.Windows.Dialogs.SelectScale;
using SaturnEdit.Windows.Dialogs.SelectSpeed;
using SaturnEdit.Windows.Dialogs.SelectTempo;
using SaturnEdit.Windows.Dialogs.SelectTutorialMarkerKey;
using SaturnEdit.Windows.Dialogs.SelectVisibility;
using SaturnEdit.Windows.Dialogs.ZigZagHoldArgs;
using SaturnEdit.Windows.Main.ChartEditor.Tabs;

namespace SaturnEdit.Windows.ChartEditor;

public partial class ChartEditorView : UserControl
{
    public ChartEditorView()
    {
        InitializeComponent();

        KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
        
        AddHandler(DragDrop.DropEvent, Control_Drop);
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        ChartSystem.EntryChanged += OnEntryChanged;
        OnEntryChanged(null, EventArgs.Empty);
        
        UndoRedoSystem.OperationHistoryChanged += OnOperationHistoryChanged;
        OnOperationHistoryChanged(null, EventArgs.Empty);
    }
    
    private static string LayoutDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaturnEdit/Layout");
    private static string CustomLayoutDirectory => Path.Combine(LayoutDirectory, "Custom");
    private static string PersistedLayoutFile => Path.Combine(LayoutDirectory, "persisted.layout");

    private enum PresetLayoutType
    {
        Classic = 0,
        Advanced = 1,
    }
    
#region Methods
    public async void File_New()
    {
        if (VisualRoot is not Window rootWindow) return;
        
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
        NewChartWindow newChartWindow = new();
        newChartWindow.Position = MainWindow.DialogPopupPosition(newChartWindow.Width, newChartWindow.Height);
        
        await newChartWindow.ShowDialog(rootWindow);

        if (newChartWindow.Result != ModalDialogResult.Primary)
        {
            return;
        }
        
        // Create new chart.
        ChartSystem.NewChart(newChartWindow.Tempo, newChartWindow.MetreUpper, newChartWindow.MetreLower);
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
                importArgsWindow.Position = MainWindow.DialogPopupPosition(importArgsWindow.Width, importArgsWindow.Height);
                
                await importArgsWindow.ShowDialog(rootWindow);

                if (importArgsWindow.Result != ModalDialogResult.Primary) return false;
                
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
            // Redirect to 'Save As' if chart file doesn't have a path yet, or if the format needs to be updated.
            if (!File.Exists(ChartSystem.Entry.ChartPath) || ChartSystem.Entry.FormatVersion != FormatVersion.SatV3)
            {
                return await File_SaveAs();
            }

            // Write chart to file.
            bool updatePath = !File.Exists(ChartSystem.Entry.ChartPath);
            if (!ChartSystem.WriteChart(ChartSystem.Entry.ChartPath, new(), true, updatePath))
            {
                ShowFileWriteError();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ShowFileWriteError();
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
            ShowFileWriteError();
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
            exportArgsWindow.Position = MainWindow.DialogPopupPosition(exportArgsWindow.Width, exportArgsWindow.Height);
            
            await exportArgsWindow.ShowDialog(rootWindow);

            // Return if export was cancelled.
            if (exportArgsWindow.Result != ModalDialogResult.Primary) return false;
            if (exportArgsWindow.NotationWriteArgs.FormatVersion == FormatVersion.Unknown) return false;

            // Open the file picker.
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return false;
            IStorageFile? file = await topLevel.StorageProvider.SaveFilePickerAsync(getOptions(exportArgsWindow.NotationWriteArgs));
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
        
        FilePickerSaveOptions getOptions(NotationWriteArgs args)
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
    }

    public void File_RenderAsImage()
    {
        //if (VisualRoot is not Window rootWindow) return;
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
        
        dialog.Position = MainWindow.DialogPopupPosition(dialog.Width, dialog.Height);

        dialog.InitializeDialog();
        await dialog.ShowDialog(rootWindow);
        return dialog.Result;
    }
    
    public void Edit_Cut()
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard == null) return;

        _ = EditorSystem.Edit_Cut(topLevel.Clipboard);
    }

    public void Edit_Copy()
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard == null) return;

        _ = EditorSystem.Edit_Copy(topLevel.Clipboard, true);
    }

    public void Edit_Paste()
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard == null) return;

        _ = EditorSystem.Edit_Paste(topLevel.Clipboard);
    }
    
    public async void Edit_SelectByCriteria()
    {
        if (VisualRoot is not Window rootWindow) return;

        SelectByCriteriaWindow selectByCriteriaWindow = new();
        selectByCriteriaWindow.Position = MainWindow.DialogPopupPosition(selectByCriteriaWindow.Width, selectByCriteriaWindow.Height);
        
        await selectByCriteriaWindow.ShowDialog(rootWindow);

        if (selectByCriteriaWindow.Result != ModalDialogResult.Primary) return;
        SelectionSystem.SelectByCriteria();
    }

    public async void ChartView_AddTempoChangeEvent()
    {
        if (VisualRoot is not Window rootWindow) return;

        SelectTempoWindow selectTempoWindow = new();
        selectTempoWindow.Position = MainWindow.DialogPopupPosition(selectTempoWindow.Width, selectTempoWindow.Height);
        
        await selectTempoWindow.ShowDialog(rootWindow);

        if (selectTempoWindow.Result != ModalDialogResult.Primary) return;
        EditorSystem.Insert_AddTempoChange(selectTempoWindow.Tempo);
    }
    
    public async void ChartView_AddMetreChangeEvent()
    {
        if (VisualRoot is not Window rootWindow) return;
        
        SelectMetreWindow selectMetreWindow = new();
        selectMetreWindow.Position = MainWindow.DialogPopupPosition(selectMetreWindow.Width, selectMetreWindow.Height);
        
        await selectMetreWindow.ShowDialog(rootWindow);

        if (selectMetreWindow.Result != ModalDialogResult.Primary) return;
        EditorSystem.Insert_AddMetreChange(selectMetreWindow.Upper, selectMetreWindow.Lower);
    }
    
    public async void ChartView_AddTutorialMarkerEvent()
    {
        if (VisualRoot is not Window rootWindow) return;
        
        SelectTutorialMarkerKeyWindow selectTutorialMarkerKeyWindow = new();
        selectTutorialMarkerKeyWindow.Position = MainWindow.DialogPopupPosition(selectTutorialMarkerKeyWindow.Width, selectTutorialMarkerKeyWindow.Height);
        
        await selectTutorialMarkerKeyWindow.ShowDialog(rootWindow);

        if (selectTutorialMarkerKeyWindow.Result != ModalDialogResult.Primary) return;
        EditorSystem.Insert_AddTutorialMarker(selectTutorialMarkerKeyWindow.TutorialMarkerKey);
    }
    
    public async void ChartView_AddSpeedChangeEvent()
    {
        if (VisualRoot is not Window rootWindow) return;

        SelectSpeedWindow selectSpeedWindow = new();
        selectSpeedWindow.Position = MainWindow.DialogPopupPosition(selectSpeedWindow.Width, selectSpeedWindow.Height);
        
        await selectSpeedWindow.ShowDialog(rootWindow);

        if (selectSpeedWindow.Result != ModalDialogResult.Primary) return;
        EditorSystem.Insert_AddSpeedChange(selectSpeedWindow.Speed);
    }
    
    public async void ChartView_AddVisibilityChangeEvent()
    {
        if (VisualRoot is not Window rootWindow) return;
        
        SelectVisibilityWindow selectVisibilityWindow = new();
        selectVisibilityWindow.Position = MainWindow.DialogPopupPosition(selectVisibilityWindow.Width, selectVisibilityWindow.Height);
        
        await selectVisibilityWindow.ShowDialog(rootWindow);

        if (selectVisibilityWindow.Result != ModalDialogResult.Primary) return;
        EditorSystem.Insert_AddVisibilityChange(selectVisibilityWindow.Visibility);
    }
    
    public async void ChartView_AddBookmark()
    {
        if (VisualRoot is not Window rootWindow) return;

        SelectBookmarkDataWindow selectBookmarkDataWindow = new();
        selectBookmarkDataWindow.Position = MainWindow.DialogPopupPosition(selectBookmarkDataWindow.Width, selectBookmarkDataWindow.Height);
        
        await selectBookmarkDataWindow.ShowDialog(rootWindow);

        if (selectBookmarkDataWindow.Result != ModalDialogResult.Primary) return;
        EditorSystem.Insert_AddBookmark(selectBookmarkDataWindow.Message, selectBookmarkDataWindow.Color);
    }
    
    public async void ChartView_AdjustAxis()
    {
        if (VisualRoot is not Window window) return;

        SelectMirrorAxisWindow selectMirrorAxisWindow = new();
        selectMirrorAxisWindow.Position = MainWindow.DialogPopupPosition(selectMirrorAxisWindow.Width, selectMirrorAxisWindow.Height);
        
        await selectMirrorAxisWindow.ShowDialog(window);

        if (selectMirrorAxisWindow.Result == ModalDialogResult.Primary)
        {
            _ = Task.Run(() => EditorSystem.MirrorAxis = selectMirrorAxisWindow.Axis);
        }
    }

    public async void ChartView_ScaleSelection()
    {
        if (VisualRoot is not Window window) return;

        SelectScaleWindow selectScaleWindow = new();
        selectScaleWindow.Position = MainWindow.DialogPopupPosition(selectScaleWindow.Width, selectScaleWindow.Height);
        
        await selectScaleWindow.ShowDialog(window);

        if (selectScaleWindow.Result == ModalDialogResult.Primary)
        {
            _ = Task.Run(() => EditorSystem.Transform_ScaleSelection(selectScaleWindow.Scale));
        }
    }

    public async void ChartView_OffsetChart()
    {
        if (VisualRoot is not Window window) return;

        SelectOffsetWindow selectOffsetWindow = new();
        selectOffsetWindow.Position = MainWindow.DialogPopupPosition(selectOffsetWindow.Width, selectOffsetWindow.Height);
        
        await selectOffsetWindow.ShowDialog(window);

        if (selectOffsetWindow.Result == ModalDialogResult.Primary)
        {
            _ = Task.Run(() => EditorSystem.Transform_OffsetChart(selectOffsetWindow.Offset));
        }
    }

    public async void ChartView_ScaleChart()
    {
        if (VisualRoot is not Window window) return;

        SelectScaleWindow selectScaleWindow = new();
        selectScaleWindow.Position = MainWindow.DialogPopupPosition(selectScaleWindow.Width, selectScaleWindow.Height);
        
        await selectScaleWindow.ShowDialog(window);

        if (selectScaleWindow.Result == ModalDialogResult.Primary)
        {
            _ = Task.Run(() => EditorSystem.Transform_ScaleChart(selectScaleWindow.Scale));
        }
    }

    public async void ChartView_MirrorChart()
    {
        if (VisualRoot is not Window window) return;

        SelectMirrorAxisWindow selectMirrorAxisWindow = new();
        selectMirrorAxisWindow.Position = MainWindow.DialogPopupPosition(selectMirrorAxisWindow.Width, selectMirrorAxisWindow.Height);
        
        await selectMirrorAxisWindow.ShowDialog(window);

        if (selectMirrorAxisWindow.Result == ModalDialogResult.Primary)
        {
            _ = Task.Run(() => EditorSystem.Transform_MirrorChart(selectMirrorAxisWindow.Axis));
        }
    }
    
    public async void ChartView_ZigZagHold()
    {
        if (VisualRoot is not Window window) return;
        
        if (!SelectionSystem.SelectedObjects.Any(x => x is HoldNote))
        {
            ModalDialogWindow modalDialog = new()
            {
                DialogIcon = Icon.Warning,
                WindowTitleKey = "ModalDialog.ZigZagHoldWarning.Title",
                HeaderKey = "ModalDialog.ZigZagHoldWarning.Header",
                ParagraphKey = "ModalDialog.ZigZagHoldWarning.Paragraph",
                ButtonPrimaryKey = "Generic.Ok",
            };
            
            modalDialog.Position = MainWindow.DialogPopupPosition(modalDialog.Width, modalDialog.Height);

            modalDialog.InitializeDialog();
            await modalDialog.ShowDialog(window);
            
            return;
        }

        ZigZagHoldArgsWindow zigZagHoldArgsWindow = new();
        zigZagHoldArgsWindow.Position = MainWindow.DialogPopupPosition(zigZagHoldArgsWindow.Width, zigZagHoldArgsWindow.Height);
        
        await zigZagHoldArgsWindow.ShowDialog(window);

        if (zigZagHoldArgsWindow.Result == ModalDialogResult.Primary)
        {
            _ = Task.Run(() => EditorSystem.Convert_ZigZagHold
            (
                beats:    zigZagHoldArgsWindow.Beats,
                division: zigZagHoldArgsWindow.Division,
                leftEdgeOffsetA:  zigZagHoldArgsWindow.LeftEdgeOffsetA,
                leftEdgeOffsetB:  zigZagHoldArgsWindow.LeftEdgeOffsetB,
                rightEdgeOffsetA: zigZagHoldArgsWindow.RightEdgeOffsetA,
                rightEdgeOffsetB: zigZagHoldArgsWindow.RightEdgeOffsetB
            ));
        }
    }

    
    public static void Dock_CreateNewFloatingTool(UserControl userControl, Icon icon, string key, double width, double height)
    {
        if (DockArea.Instance == null) return;
        
        DockTab tab = new(userControl, icon, key);
        DockArea.Instance.Popup(tab, width, height);
    }
    
    private async void Dock_SaveLayout()
    {
        try
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            // Open file picker.
            Directory.CreateDirectory(CustomLayoutDirectory);
            
            IStorageFolder? folder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(CustomLayoutDirectory);
            IStorageFile? file = await topLevel.StorageProvider.SaveFilePickerAsync(new()
            {
                DefaultExtension = ".layout",
                SuggestedFileName = "custom.layout",
                SuggestedStartLocation = folder,
                FileTypeChoices =
                [
                    new("SaturnEdit Layout File")
                    {
                        Patterns = ["*.layout"],
                    },
                ],
            });

            if (file == null) return;

            // Write layout to file.
            string data = DockSerializer.Serialize();
            await File.WriteAllTextAsync(file.Path.LocalPath, data);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            ShowFileWriteError();
        }
    }

    private async void Dock_LoadLayout()
    {
        try
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            // Open file picker.
            Directory.CreateDirectory(CustomLayoutDirectory);
            
            IStorageFolder? folder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(CustomLayoutDirectory);
            IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new()
            {
                SuggestedStartLocation = folder,
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new("SaturnEdit Layout Files")
                    {
                        Patterns = ["*.layout"],
                    },
                ],
            });
            if (files.Count != 1) return;
            
            // Read layout from file.
            string data = await File.ReadAllTextAsync(files[0].Path.LocalPath);
            DockSerializer.Deserialize(data, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Dock_LoadPreset(PresetLayoutType.Classic);
        }
    }

    private static void Dock_LoadPreset(PresetLayoutType preset)
    {
        const string classicPreset = "Main 1300 950 0 0\nSplit Col 0.64\nGroup\nChartView3D X\nSplit Row 0.73\nGroup\nChartPropertiesView X\nInspectorView\nGroup\nCursorView X";
        const string advancedPreset = "Main 1920 1080 0 0\nSplit Col 0.71\nSplit Col 0.35\nSplit Row 0.75\nGroup\nInspectorView X\nEventListView\nLayerListView\nGroup\nCursorView X\nGroup\nChartView3D X\nSplit Row 0.65\nGroup\nChartPropertiesView X\nChartStatisticsView\nGroup\nProofreaderView X";

        if (preset == PresetLayoutType.Classic)
        {
            DockSerializer.Deserialize(classicPreset, false);
            return;
        }
        
        if (preset == PresetLayoutType.Advanced)
        {
            DockSerializer.Deserialize(advancedPreset, false);
        }
    }

    private void Dock_SavePersistedLayout()
    {
        try
        {
            Directory.CreateDirectory(LayoutDirectory);

            string data = DockSerializer.Serialize();
            File.WriteAllText(PersistedLayoutFile, data);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
    }
    
    private void Dock_LoadPersistedLayout()
    {
        try
        {
            if (File.Exists(PersistedLayoutFile))
            {
                string data = File.ReadAllText(PersistedLayoutFile);
                DockSerializer.Deserialize(data, true);
            }
            else
            {
                Dock_LoadPreset(PresetLayoutType.Classic);
            }
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
            
            Dock_LoadPreset(PresetLayoutType.Classic);
        }
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
        
        dialog.Position = MainWindow.DialogPopupPosition(dialog.Width, dialog.Height);

        dialog.InitializeDialog();
        dialog.ShowDialog(rootWindow);
    }
#endregion Methods

#region System Event Delegates
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            MenuItemNew.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.New"].ToKeyGesture();
            MenuItemOpen.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.Open"].ToKeyGesture();
            MenuItemSave.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.Save"].ToKeyGesture();
            MenuItemSaveAs.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.SaveAs"].ToKeyGesture();
            MenuItemReloadFromDisk.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.ReloadFromDisk"].ToKeyGesture();
            MenuItemExport.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.Export"].ToKeyGesture();
            MenuItemRenderAsImage.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.RenderAsImage"].ToKeyGesture();
            MenuItemSettings.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["QuickCommands.Settings"].ToKeyGesture();
            MenuItemQuit.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.Quit"].ToKeyGesture();

            MenuItemUndo.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Undo"].ToKeyGesture();
            MenuItemRedo.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Redo"].ToKeyGesture();
            MenuItemCut.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Cut"].ToKeyGesture();
            MenuItemCopy.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Copy"].ToKeyGesture();
            MenuItemPaste.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Paste"].ToKeyGesture();
            MenuItemSelectAll.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.SelectAll"].ToKeyGesture();
            MenuItemDeselectAll.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.DeselectAll"].ToKeyGesture();
            MenuItemCheckerDeselect.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.CheckerDeselect"].ToKeyGesture();
            MenuItemSelectByCriteria.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.SelectByCriteria"].ToKeyGesture();

            MenuItemMoveBeatForward.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.MoveBeatForward"].ToKeyGesture();
            MenuItemMoveBeatBack.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.MoveBeatBack"].ToKeyGesture();
            MenuItemMoveMeasureForward.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.MoveMeasureForward"].ToKeyGesture();
            MenuItemMoveMeasureBack.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.MoveMeasureBack"].ToKeyGesture();
            MenuItemJumpToNextObject.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.JumpToNextObject"].ToKeyGesture();
            MenuItemJumpToPreviousObject.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.JumpToPreviousObject"].ToKeyGesture();
            MenuItemIncreaseBeatDivision.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.IncreaseBeatDivision"].ToKeyGesture();
            MenuItemDecreaseBeatDivision.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.DecreaseBeatDivision"].ToKeyGesture();
            MenuItemDoubleBeatDivision.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.DoubleBeatDivision"].ToKeyGesture();
            MenuItemHalveBeatDivision.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.HalveBeatDivision"].ToKeyGesture();
        });
    }
    
    private void OnEntryChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            MenuItemReloadFromDisk.IsEnabled = File.Exists(ChartSystem.Entry.ChartFile);
        });
    }
    
    private void OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            MenuItemUndo.IsEnabled = UndoRedoSystem.CanUndo;
            MenuItemRedo.IsEnabled = UndoRedoSystem.CanRedo;
        });
    }
    
    public void OnClosing(object? sender, EventArgs e)
    {
        Dock_SavePersistedLayout();

        SettingsSystem.EditorSettings.LastSessionPath = SettingsSystem.EditorSettings.ContinueLastSession
            ? ChartSystem.Entry.ChartPath
            : "";
    }
#endregion System Event Delegates
    
#region UI Event Delegates
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        Dock_LoadPersistedLayout();
    }

    private void Control_OnKeyDown(object? sender, KeyEventArgs e)
    {
        IInputElement? focusedElement = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (KeyDownBlacklist.IsInvalidFocusedElement(focusedElement)) return;
        if (KeyDownBlacklist.IsInvalidKey(e.Key)) return;
        if (KeyDownBlacklist.IsInvalidState()) return;
        
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
            Edit_Cut();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Edit.Copy"]))
        {
            Edit_Copy();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Edit.Paste"]))
        {
            Edit_Paste();
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
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Navigate.MoveBeatForward"]))
        { 
            TimeSystem.Navigate_MoveBeatForward();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Navigate.MoveBeatBack"]))
        { 
            TimeSystem.Navigate_MoveBeatBack();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Navigate.MoveMeasureForward"]))
        { 
            TimeSystem.Navigate_MoveMeasureForward();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Navigate.MoveMeasureBack"]))
        { 
            TimeSystem.Navigate_MoveMeasureBack();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Navigate.JumpToNextObject"]))
        {
            TimeSystem.Navigate_JumpToNextObject();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Navigate.JumpToPreviousObject"]))
        {
            TimeSystem.Navigate_JumpToPreviousObject();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Navigate.IncreaseBeatDivision"]))
        {
            TimeSystem.Navigate_IncreaseBeatDivision();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Navigate.DecreaseBeatDivision"]))
        {
            TimeSystem.Navigate_DecreaseBeatDivision();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Navigate.DoubleBeatDivision"]))
        {
            TimeSystem.Navigate_DoubleBeatDivision();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Navigate.HalveBeatDivision"]))
        {
            TimeSystem.Navigate_HalveBeatDivision();
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.Touch"])) 
        {
            CursorSystem.SetType(CursorSystem.TouchNote);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.Chain"])) 
        {
            CursorSystem.SetType(CursorSystem.ChainNote);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.Hold"])) 
        {
            CursorSystem.SetType(CursorSystem.HoldNote);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.SlideClockwise"])) 
        {
            CursorSystem.SetType(CursorSystem.SlideClockwiseNote);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.SlideCounterclockwise"])) 
        {
            CursorSystem.SetType(CursorSystem.SlideCounterclockwiseNote);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.SnapForward"])) 
        {
            CursorSystem.SetType(CursorSystem.SnapForwardNote);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.SnapBackward"])) 
        {
            CursorSystem.SetType(CursorSystem.SnapBackwardNote);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.LaneShow"])) 
        {
            CursorSystem.SetType(CursorSystem.LaneShowNote);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.LaneHide"])) 
        {
            CursorSystem.SetType(CursorSystem.LaneHideNote);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.Sync"])) 
        {
            CursorSystem.SetType(CursorSystem.SyncNote);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.NoteType.MeasureLine"])) 
        {
            CursorSystem.SetType(CursorSystem.MeasureLineNote);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.BonusType.Normal"]))
        {
            CursorSystem.BonusType = BonusType.Normal;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.BonusType.Bonus"]))
        {
            CursorSystem.BonusType = BonusType.Bonus;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.BonusType.R"]))
        {
            CursorSystem.BonusType = BonusType.R;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.JudgementType.Normal"]))
        {
            CursorSystem.JudgementType = JudgementType.Normal;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.JudgementType.Fake"]))
        {
            CursorSystem.JudgementType = JudgementType.Fake;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.JudgementType.Autoplay"]))
        {
            CursorSystem.JudgementType = JudgementType.Autoplay;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.SweepDirection.Center"]))
        {
            CursorSystem.Direction = LaneSweepDirection.Center;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.SweepDirection.Clockwise"]))
        {
            CursorSystem.Direction = LaneSweepDirection.Clockwise;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.SweepDirection.Counterclockwise"]))
        {
            CursorSystem.Direction = LaneSweepDirection.Counterclockwise;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.SweepDirection.Instant"]))
        {
            CursorSystem.Direction = LaneSweepDirection.Instant;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.HoldPointRenderType.Hidden"]))
        {
            CursorSystem.RenderType = HoldPointRenderType.Visible;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["NotePalette.HoldPointRenderType.Visible"]))
        {
            CursorSystem.RenderType = HoldPointRenderType.Hidden;
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Cursor.IncreasePosition"]))
        {
            CursorSystem.Position++;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Cursor.DecreasePosition"]))
        {
            CursorSystem.Position--;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Cursor.IncreaseSize"]))
        {
            CursorSystem.Size++;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Cursor.DecreaseSize"]))
        {
            CursorSystem.Size--;
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Playback.IncreasePlaybackSpeed"]))
        {
            TimeSystem.PlaybackSpeed = Math.Min(300, TimeSystem.PlaybackSpeed + 5);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Playback.DecreasePlaybackSpeed"])) 
        {
            TimeSystem.PlaybackSpeed = Math.Max(5, TimeSystem.PlaybackSpeed - 5);
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Playback.LoopPlayback"]))
        {
            SettingsSystem.AudioSettings.LoopPlayback = !SettingsSystem.AudioSettings.LoopPlayback;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Playback.SetLoopMarkerStart"]))
        {
            TimeSystem.LoopStart = TimeSystem.Timestamp.Time;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Playback.SetLoopMarkerEnd"]))
        {
            TimeSystem.LoopEnd = TimeSystem.Timestamp.Time;
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Playback.Metronome"]))
        {
            SettingsSystem.AudioSettings.Metronome = !SettingsSystem.AudioSettings.Metronome;
            e.Handled = true;
        }
    }
    
    private void Control_OnKeyUp(object? sender, KeyEventArgs e) => e.Handled = true;
    
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

            // Prompt to save an unsaved chart first.
            if (!ChartSystem.IsSaved)
            {
                ModalDialogResult result = await PromptSave();

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

            IStorageItem? file = e.DataTransfer.TryGetFile();

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
            NotationReadArgs args = new();
            FormatVersion formatVersion = NotationSerializer.DetectFormatVersion(file.Path.LocalPath);
            if (formatVersion == FormatVersion.Unknown)
            {
                e.Handled = true;
                return;
            }

            if (formatVersion != FormatVersion.SatV3)
            {
                if (VisualRoot is not Window rootWindow)
                {
                    e.Handled = true;
                    return;
                }

                ImportArgsWindow importArgsWindow = new();
                importArgsWindow.Position = MainWindow.DialogPopupPosition(importArgsWindow.Width, importArgsWindow.Height);
                
                await importArgsWindow.ShowDialog(rootWindow);

                if (importArgsWindow.Result != ModalDialogResult.Primary)
                {
                    e.Handled = true;
                    return;
                }

                args = importArgsWindow.NotationReadArgs;
            }

            ChartSystem.ReadChart(file.Path.LocalPath, args);

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

    private void MenuItemSave_OnClick(object? sender, RoutedEventArgs e) => _ = File_Save();

    private void MenuItemSaveAs_OnClick(object? sender, RoutedEventArgs e) => _ = File_SaveAs();

    private void MenuItemReloadFromDisk_OnClick(object? sender, RoutedEventArgs e) => _ = File_ReloadFromDisk();

    private void MenuItemExport_OnClick(object? sender, RoutedEventArgs e) => _ = File_Export();

    private void MenuItemRenderAsImage_OnClick(object? sender, RoutedEventArgs e) => File_RenderAsImage();

    private void MenuItemSettings_OnClick(object? sender, RoutedEventArgs e) => MainWindow.Instance?.ShowSettingsWindow();

    private void MenuItemQuit_OnClick(object? sender, RoutedEventArgs e) => File_Quit();
    
    
    private void MenuItemUndo_OnClick(object? sender, RoutedEventArgs e) => UndoRedoSystem.Undo();

    private void MenuItemRedo_OnClick(object? sender, RoutedEventArgs e) => UndoRedoSystem.Redo();

    private void MenuItemCut_OnClick(object? sender, RoutedEventArgs e) => Edit_Cut();

    private void MenuItemCopy_OnClick(object? sender, RoutedEventArgs e) => Edit_Copy();

    private void MenuItemPaste_OnClick(object? sender, RoutedEventArgs e) => Edit_Paste();

    private void MenuItemSelectAll_OnClick(object? sender, RoutedEventArgs e) => SelectionSystem.SelectAll();

    private void MenuItemDeselectAll_OnClick(object? sender, RoutedEventArgs e) => SelectionSystem.DeselectAll();

    private void MenuItemCheckerDeselect_OnClick(object? sender, RoutedEventArgs e) => SelectionSystem.CheckerDeselect();

    private void MenuItemSelectByCriteria_OnClick(object? sender, RoutedEventArgs e) => Edit_SelectByCriteria();
    
    
    private void MenuItemToolWindows_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;
        (UserControl?, Icon, string, double, double) tabData = menuItem.Name switch
        {
            "MenuItemChartView3D"     => (new ChartView3D(),         Icon.CircleShadow,           "ChartEditor.ChartView3D"    , 750, 773),
            "MenuItemChartView2D"     => (new ChartView2D(),         Icon.GanttChart,             "ChartEditor.ChartView2D"    , 750, 773),
            "MenuItemChartViewTxt"    => (new ChartViewTxt(),        Icon.TextT,                  "ChartEditor.ChartViewTxt"   , 750, 773),
            "MenuItemChartProperties" => (new ChartPropertiesView(), Icon.TextBulletList,         "ChartEditor.ChartProperties", 500, 535),
            "MenuItemChartStatistics" => (new ChartStatisticsView(), Icon.DataHistogram,          "ChartEditor.ChartStatistics", 500, 535),
            "MenuItemProofreader"     => (new ProofreaderView(),     Icon.ApprovalsApp,           "ChartEditor.Proofreader"    , 800, 485),
            "MenuItemEventList"       => (new EventListView(),       Icon.TextBulletList,         "ChartEditor.EventList"      , 500, 735),
            "MenuItemLayerList"       => (new LayerListView(),       Icon.TextBulletList,         "ChartEditor.LayerList"      , 500, 735),
            "MenuItemInspector"       => (new InspectorView(),       Icon.WrenchScrewdriver,      "ChartEditor.Inspector"      , 500, 735),
            "MenuItemCursor"          => (new CursorView(),          Icon.CircleHintHalfVertical, "ChartEditor.Cursor"         , 350, 225),
            "MenuItemWaveform"        => (new WaveformView(),        Icon.Pulse,                  "ChartEditor.Waveform"       , 250, 773),
            _ => (null, Icon.Warning, "", 200, 200),
        };

        if (tabData.Item1 == null) return;
        Dock_CreateNewFloatingTool(tabData.Item1, tabData.Item2, tabData.Item3, tabData.Item4, tabData.Item5);
    }
    
    private void MenuItemLayoutPresetClassic_OnClick(object? sender, RoutedEventArgs e) => Dock_LoadPreset(PresetLayoutType.Classic);

    private void MenuItemLayoutPresetAdvanced_OnClick(object? sender, RoutedEventArgs e) => Dock_LoadPreset(PresetLayoutType.Advanced);

    private void MenuItemSaveLayout_OnClick(object? sender, RoutedEventArgs e) => Dock_SaveLayout();

    private void MenuItemLoadLayout_OnClick(object? sender, RoutedEventArgs e) => Dock_LoadLayout();
    
    
    private void MenuItemMoveBeatForward_OnClick(object? sender, RoutedEventArgs e) => TimeSystem.Navigate_MoveBeatForward();

    private void MenuItemMoveBeatBack_OnClick(object? sender, RoutedEventArgs e) => TimeSystem.Navigate_MoveBeatBack();

    private void MenuItemMoveMeasureForward_OnClick(object? sender, RoutedEventArgs e) => TimeSystem.Navigate_MoveMeasureForward();

    private void MenuItemMoveMeasureBack_OnClick(object? sender, RoutedEventArgs e) => TimeSystem.Navigate_MoveMeasureBack();

    private void MenuItemJumpToNextObject_OnClick(object? sender, RoutedEventArgs e) => TimeSystem.Navigate_JumpToNextObject();

    private void MenuItemJumpToPreviousObject_OnClick(object? sender, RoutedEventArgs e) => TimeSystem.Navigate_JumpToPreviousObject();

    private void MenuItemIncreaseBeatDivision_OnClick(object? sender, RoutedEventArgs e) => TimeSystem.Navigate_IncreaseBeatDivision();

    private void MenuItemDecreaseBeatDivision_OnClick(object? sender, RoutedEventArgs e) => TimeSystem.Navigate_DecreaseBeatDivision();

    private void MenuItemDoubleBeatDivision_OnClick(object? sender, RoutedEventArgs e) => TimeSystem.Navigate_DoubleBeatDivision();

    private void MenuItemHalveBeatDivision_OnClick(object? sender, RoutedEventArgs e) => TimeSystem.Navigate_HalveBeatDivision();
#endregion UI Event Delegates
}