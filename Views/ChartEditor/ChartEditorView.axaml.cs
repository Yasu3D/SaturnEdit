using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Model;
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

        serializer = new(typeof(AvaloniaList<>));
        dockState = new();

        IDock? layout = Dock?.Layout;
        if (layout != null)
        {
            dockState.Save(layout);
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}