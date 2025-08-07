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

    private bool blockEvent = false;

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        blockEvent = true;
        
        ComboBoxLanguage.SelectedIndex = (int)SettingsSystem.EditorSettings.Locale;
        CheckBoxShowSplashScreen.IsChecked = SettingsSystem.EditorSettings.ShowSplashScreen;
        CheckBoxContinueLastSession.IsChecked = SettingsSystem.EditorSettings.ContinueLastSession;
        
        blockEvent = false;
    }

    private void ComboBoxLanguage_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvent) return;
        if (ComboBoxLanguage == null) return;

        SettingsSystem.EditorSettings.Locale = (EditorSettings.LocaleOptions)ComboBoxLanguage.SelectedIndex;
    }

    private void CheckBoxShowSplashScreen_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (CheckBoxShowSplashScreen == null) return;
        SettingsSystem.EditorSettings.ShowSplashScreen = CheckBoxShowSplashScreen.IsChecked ?? true;
    }

    private void CheckBoxContinueLastSession_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (CheckBoxContinueLastSession == null) return;
        SettingsSystem.EditorSettings.ContinueLastSession = CheckBoxContinueLastSession.IsChecked ?? true;
    }
}