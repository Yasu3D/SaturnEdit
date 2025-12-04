using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnEdit.Systems;

namespace SaturnEdit.Windows.Dialogs.Settings.Tabs;

public partial class SettingsGeneralView : UserControl
{
    public SettingsGeneralView()
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
            
            ComboBoxLanguage.SelectedIndex = (int)SettingsSystem.EditorSettings.Locale;
            CheckBoxShowSplashScreen.IsChecked = SettingsSystem.EditorSettings.ShowSplashScreen;
            CheckBoxContinueLastSession.IsChecked = SettingsSystem.EditorSettings.ContinueLastSession;
            CheckBoxCheckForUpdates.IsChecked = SettingsSystem.EditorSettings.CheckForUpdates;
            ComboBoxAutosaveFrequency.SelectedIndex = (int)SettingsSystem.EditorSettings.AutoSaveFrequency;
            
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
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        SettingsSystem.SettingsChanged -= OnSettingsChanged;
        
        base.OnUnloaded(e);
    }
    
    private void ComboBoxLanguage_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxLanguage == null) return;

        SettingsSystem.EditorSettings.Locale = (EditorSettings.LocaleOption)ComboBoxLanguage.SelectedIndex;
    }

    private void CheckBoxShowSplashScreen_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CheckBoxShowSplashScreen == null) return;
        
        SettingsSystem.EditorSettings.ShowSplashScreen = CheckBoxShowSplashScreen.IsChecked ?? true;
    }

    private void CheckBoxContinueLastSession_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CheckBoxContinueLastSession == null) return;
        
        SettingsSystem.EditorSettings.ContinueLastSession = CheckBoxContinueLastSession.IsChecked ?? true;
    }
    
    private void CheckBoxCheckForUpdates_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CheckBoxCheckForUpdates == null) return;
        
        SettingsSystem.EditorSettings.CheckForUpdates = CheckBoxCheckForUpdates.IsChecked ?? true;
    }
    
    private void ComboBoxAutosaveFrequency_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxAutosaveFrequency == null) return;
        
        SettingsSystem.EditorSettings.AutoSaveFrequency = (EditorSettings.AutoSaveFrequencyOption)ComboBoxAutosaveFrequency.SelectedIndex;
    }
    
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