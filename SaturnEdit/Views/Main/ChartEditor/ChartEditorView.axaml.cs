using System;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Dock.Model;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Serializer;

namespace SaturnEdit.Views.ChartEditor;

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
}