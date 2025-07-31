using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnEdit.Views.Settings.Tabs;

namespace SaturnEdit.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();

        SettingsTabContainer.Content = settingsGeneralView;
    }
    
    private readonly SettingsGeneralView settingsGeneralView = new();
    private readonly SettingsAppearanceView settingsAppearanceView = new();
    private readonly SettingsRenderingView settingsRenderingView = new();
    private readonly SettingsShortcutsView settingsShortcutsView = new();

    private void SettingsTab_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;

        SettingsTabContainer.Content = button.Name switch
        {
            "RadioButtonGeneral"    => settingsGeneralView,
            "RadioButtonAppearance" => settingsAppearanceView,
            "RadioButtonRendering"  => settingsRenderingView,
            "RadioButtonShortcuts"  => settingsShortcutsView,
            _ => null,
        };
    }
}