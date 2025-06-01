using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SaturnEdit.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        App.SetLocale("en-US");
    }
    
    private void EditorTabs_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;

        if (ChartEditorView != null)
        {
            ChartEditorView.IsEnabled = button.Name == "TabChartEditor";
            ChartEditorView.IsVisible = button.Name == "TabChartEditor";
        }
        
        if (StageEditorView != null)
        {
            StageEditorView.IsEnabled = button.Name == "TabStageEditor";
            StageEditorView.IsVisible = button.Name == "TabStageEditor";
        }
        
        if (ContentEditorView != null)
        {
            ContentEditorView.IsEnabled = button.Name == "TabContentEditor";
            ContentEditorView.IsVisible = button.Name == "TabContentEditor";
        }
    }
}