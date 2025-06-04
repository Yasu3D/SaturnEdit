using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnEdit.Views;
using SaturnEdit.Views.ChartEditor;
using SaturnEdit.Views.CosmeticsEditor;
using SaturnEdit.Views.StageEditor;

namespace SaturnEdit;

public partial class MainWindow : Window
{
    public SettingsWindow? Settings;
    
    public MainWindow()
    {
        InitializeComponent();
        
        ChartEditor.Content = new ChartEditorView(this);
        StageEditor.Content = new StageEditorView(this);
        CosmeticsEditor.Content = new CosmeticsEditorView(this);
    }
    
    private void EditorTabs_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;

        if (ChartEditor != null)
        {
            ChartEditor.IsEnabled = button.Name == "TabChartEditor";
            ChartEditor.IsVisible = button.Name == "TabChartEditor";
        }
        
        if (StageEditor != null)
        {
            StageEditor.IsEnabled = button.Name == "TabStageEditor";
            StageEditor.IsVisible = button.Name == "TabStageEditor";
        }
        
        if (CosmeticsEditor != null)
        {
            CosmeticsEditor.IsEnabled = button.Name == "TabContentEditor";
            CosmeticsEditor.IsVisible = button.Name == "TabContentEditor";
        }
    }

    public void ShowSettings()
    {
        if (Settings != null) return;
        
        Settings = new();
        Settings.Closed += (_, _) => Settings = null;
        Settings.Show();
    }
}