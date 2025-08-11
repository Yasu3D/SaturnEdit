using System;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Dock.Model;
using Dock.Model.Avalonia.Controls;
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
        return;
        Tool tool = new() { Content = userControl };
        
        ToolDock toolDock = new()
        {
            VisibleDockables = DockControl.Factory?.CreateList<IDockable>(tool),
            ActiveDockable = tool,
        };

        DockControl.Factory?.AddDockable(RootDock, toolDock);
        DockControl.Factory?.FloatDockable(toolDock);
    }
}