using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Dock.Model;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Serializer;
using FluentIcons.Common;
using SaturnData.Notation.Core;
using SaturnData.Notation.Serialization;
using SaturnEdit.Systems;
using SaturnEdit.Windows.Dialogs.ExportArgs;
using SaturnEdit.Windows.Dialogs.ImportArgs;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.ChartEditor;

public partial class ChartEditorView : UserControl
{
    private readonly DockSerializer serializer;
    private readonly DockState dockState;

    public ChartEditorView()
    {
        InitializeComponent();
        AvaloniaXamlLoader.Load(this);

        serializer = new(typeof(AvaloniaList<>));
        dockState = new();

        IDock? layout = DockControl?.Layout;
        if (layout != null)
        {
            dockState.Save(layout);
        }
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

    public async void FileNew()
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
                bool success = await FileSave();

                // Abort opening new file if save was unsuccessful.
                if (!success) return;
            }

            // Don't Save
            // Continue as normal.
        }

        // Open new chart dialog.

        // Create new chart.
    }

    public async Task<bool> FileOpen()
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
                    bool success = await FileSave();

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

    public async Task<bool> FileSave()
    {
        try
        {
            // Save As if chart file doesn't have a path yet.
            if (!File.Exists(ChartSystem.Entry.ChartFile))
            {
                return await FileSaveAs();
            }

            // Write chart to file.
            bool updatePath = !File.Exists(ChartSystem.Entry.ChartFile);
            ChartSystem.WriteChart(ChartSystem.Entry.ChartFile, new(), true, updatePath);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    public async Task<bool> FileSaveAs()
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
            ChartSystem.WriteChart(file.Path.LocalPath, new(), true, true);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    public async Task<bool> FileReloadFromDisk()
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
                    bool success = await FileSave();

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

    public async Task<bool> FileExport()
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
            ChartSystem.WriteChart(file.Path.LocalPath, exportArgsWindow.NotationWriteArgs, true, true);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    public async void FileQuit()
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
                bool success = await FileSave();

                // Abort quitting if save was unsuccessful.
                if (!success) return;
            }

            // Don't Save
            // Continue as normal.
        }

        if (VisualRoot is not Window rootWindow) return;
        rootWindow.Close();
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

    private async Task<ModalDialogResult> PromptSave()
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
}