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
    
#region System Event Delegates
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            ComboBoxLanguage.SelectedIndex = (int)SettingsSystem.EditorSettings.Locale;
            CheckBoxShowSplashScreen.IsChecked = SettingsSystem.EditorSettings.ShowSplashScreen;
            CheckBoxContinueLastSession.IsChecked = SettingsSystem.EditorSettings.ContinueLastSession;
            
            blockEvents = false;
        });
    }
#endregion System Event Delegates

#region UI Event Delegates
    private void ComboBoxLanguage_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxLanguage == null) return;

        SettingsSystem.EditorSettings.Locale = (EditorSettings.LocaleOptions)ComboBoxLanguage.SelectedIndex;
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
#endregion UI Event Delegates
}