using System;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Dock.Model;
using Dock.Model.Core;
using Dock.Serializer;
using SaturnEdit.Systems;

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

        IDock? layout = Dock?.Layout;
        if (layout != null)
        {
            dockState.Save(layout);
        }
    }
}