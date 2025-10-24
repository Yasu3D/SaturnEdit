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

    private bool blockEvent = false;

#region System Event Delegates
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
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
        });
    }
#endregion System Event Delegates

#region UI Event Delegates
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
#endregion UI Event Delegates
}