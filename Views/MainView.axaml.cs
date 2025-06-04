using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnEdit.Views.ChartEditor;
using SaturnEdit.Views.CosmeticsEditor;
using SaturnEdit.Views.StageEditor;

namespace SaturnEdit.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        ChartEditorView.Content = new ChartEditorView(this);
        StageEditorView.Content = new StageEditorView(this);
        CosmeticsEditorView.Content = new CosmeticsEditorView(this);
    }

    public SettingsWindow? SettingsWindow { get; private set; }
    
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
        
        if (CosmeticsEditorView != null)
        {
            CosmeticsEditorView.IsEnabled = button.Name == "TabContentEditor";
            CosmeticsEditorView.IsVisible = button.Name == "TabContentEditor";
        }
    }

    public void ShowSettings()
    {
        if (SettingsWindow != null) return;
        
        SettingsWindow = new();
        SettingsWindow.Closed += (_, _) => SettingsWindow = null;
        SettingsWindow.Show();
    }
}