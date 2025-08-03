using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnEdit.Systems;

namespace SaturnEdit.Views.Settings.Tabs;

public partial class SettingsAppearanceView : UserControl
{
    public SettingsAppearanceView()
    {
        InitializeComponent();
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

    private bool blockEvent = false;

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        blockEvent = true;
        
        if (SettingsSystem.EditorSettings.Theme == EditorSettings.EditorThemeOptions.Light)
        {
            RadioButtonThemeLight.IsChecked = true;
        }
        else
        {
            RadioButtonThemeDark.IsChecked = true;
        }
        
        blockEvent = false;
    }

    private void RadioButtonTheme_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (RadioButtonThemeLight == null || RadioButtonThemeDark == null) return;
        if (sender is not RadioButton button) return;
        if (!button.IsChecked ?? false) return;
        
        SettingsSystem.EditorSettings.Theme = button.Name switch
        {
            "RadioButtonThemeLight" => EditorSettings.EditorThemeOptions.Light,
            "RadioButtonThemeDark" => EditorSettings.EditorThemeOptions.Dark,
            _ => SettingsSystem.EditorSettings.Theme,
        };
    }
}