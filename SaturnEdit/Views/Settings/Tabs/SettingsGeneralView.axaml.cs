using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnEdit.Systems;

namespace SaturnEdit.Views.Settings.Tabs;

public partial class SettingsGeneralView : UserControl
{
    public SettingsGeneralView()
    {
        InitializeComponent();
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        ComboBoxLanguage.SelectedIndex = (int)SettingsSystem.EditorSettings.Locale;
        CheckBoxShowSplashScreen.IsChecked = SettingsSystem.EditorSettings.ShowSplashScreen;
        CheckBoxContinueLastSession.IsChecked = SettingsSystem.EditorSettings.ContinueLastSession;
    }

    private void ComboBoxLanguage_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ComboBoxLanguage == null) return;

        SettingsSystem.EditorSettings.Locale = (EditorSettings.LocaleOptions)ComboBoxLanguage.SelectedIndex;
    }

    private void CheckBoxShowSplashScreen_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (CheckBoxShowSplashScreen == null) return;
        SettingsSystem.EditorSettings.ShowSplashScreen = CheckBoxShowSplashScreen.IsChecked ?? true;
    }

    private void CheckBoxContinueLastSession_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (CheckBoxContinueLastSession == null) return;
        SettingsSystem.EditorSettings.ContinueLastSession = CheckBoxContinueLastSession.IsChecked ?? true;
    }
}