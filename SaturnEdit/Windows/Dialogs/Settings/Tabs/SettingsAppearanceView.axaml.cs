using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnEdit.Systems;

namespace SaturnEdit.Windows.Dialogs.Settings.Tabs;

public partial class SettingsAppearanceView : UserControl
{
    public SettingsAppearanceView()
    {
        InitializeComponent();
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

    private bool blockEvents = false;

#region System Event Handlers
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
        
            if (SettingsSystem.EditorSettings.Theme == EditorSettings.EditorThemeOption.Light)
            {
                RadioButtonThemeLight.IsChecked = true;
            }
            else
            {
                RadioButtonThemeDark.IsChecked = true;
            }
            
            blockEvents = false;
        });
    }
#endregion System Event Handlers

#region UI Event Handlers
    private void RadioButtonTheme_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (RadioButtonThemeLight == null || RadioButtonThemeDark == null) return;
        if (sender is not RadioButton button) return;
        if (!button.IsChecked ?? false) return;
        
        SettingsSystem.EditorSettings.Theme = button.Name switch
        {
            "RadioButtonThemeLight" => EditorSettings.EditorThemeOption.Light,
            "RadioButtonThemeDark" => EditorSettings.EditorThemeOption.Dark,
            _ => SettingsSystem.EditorSettings.Theme,
        };
    }
#endregion UI Event Handlers
}