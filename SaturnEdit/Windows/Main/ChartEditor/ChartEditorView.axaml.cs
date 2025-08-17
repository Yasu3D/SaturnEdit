using System;
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
using SaturnData.Notation.Core;
using SaturnData.Notation.Serialization;
using SaturnEdit.Systems;
using SaturnEdit.Windows.Dialogs.Export;

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
    
    public void FileNew()
    {
        
    }

    public void FileOpen()
    {

    }

    public async Task<bool> FileSave()
    {
        try
        {
            if (!File.Exists(ChartSystem.Entry.ChartPath))
            {
                return await FileSaveAs();
            }

            ChartSystem.WriteChart(ChartSystem.Entry.ChartPath, new(), true, true);
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

            ChartSystem.WriteChart(file.Path.AbsolutePath, new(), true, true);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    public void FileReloadFromDisk()
    {
        
    }

    public async Task<bool> FileExport()
    {
        try
        {
            // Open an export dialog.
            if (VisualRoot is not Window rootWindow) return false;
            ExportWindow exportWindow = new();
            await exportWindow.ShowDialog(rootWindow);

            // Return if export was cancelled.
            if (exportWindow.DialogResult == ExportWindow.ExportDialogResult.Cancel) return false;
            if (exportWindow.NotationWriteArgs.FormatVersion == FormatVersion.Unknown) return false;
            
            // Open the file picker.
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return false;
            IStorageFile? file = await topLevel.StorageProvider.SaveFilePickerAsync(ExportFilePickerSaveOptions(exportWindow.NotationWriteArgs));
            if (file == null) return false;

            // Write the chart to the defined path, with the defined notation arguments.
            ChartSystem.WriteChart(file.Path.AbsolutePath, exportWindow.NotationWriteArgs, true, true);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
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
            Difficulty.None => $"chart.{defaultExtension}",
            Difficulty.Normal => $"0_normal.{defaultExtension}",
            Difficulty.Hard => $"1_hard.{defaultExtension}",
            Difficulty.Expert => $"2_expert.{defaultExtension}",
            Difficulty.Inferno => $"3_inferno.{defaultExtension}",
            Difficulty.WorldsEnd => $"4_worldsend.{defaultExtension}",
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