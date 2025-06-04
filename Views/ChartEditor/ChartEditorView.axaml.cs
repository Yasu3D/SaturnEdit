using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Dock.Model;
using Dock.Model.Core;
using Dock.Serializer;

namespace SaturnEdit.Views.ChartEditor;

public partial class ChartEditorView : UserControl
{
    private readonly MainView mainView;
    private readonly DockSerializer serializer;
    private readonly DockState dockState;
    
    public ChartEditorView(MainView mainView)
    {
        InitializeComponent();

        this.mainView = mainView;
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

    private void MenuItemSettings_OnClick(object? sender, RoutedEventArgs e) => mainView.ShowSettings();
}