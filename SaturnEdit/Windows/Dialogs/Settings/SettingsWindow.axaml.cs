using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnData.Content.Serialization;
using SaturnEdit.Settings;
using SaturnEdit.Systems;
using SaturnEdit.Windows.Dialogs.Settings.Tabs;
using SaturnView;

namespace SaturnEdit.Windows.Dialogs;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();

        // Lazily deep-copy settings when window opens to keep a backup.
        // This backup is used when the user clicks "cancel" to restore all settings back to before they were changed.
        renderSettingsBackup   = ContentSerializer.ToString(SettingsSystem.RenderSettings);
        editorSettingsBackup   = ContentSerializer.ToString(SettingsSystem.EditorSettings);
        audioSettingsBackup    = ContentSerializer.ToString(SettingsSystem.AudioSettings);
        shortcutSettingsBackup = ContentSerializer.ToString(SettingsSystem.ShortcutSettings);
        
        SettingsTabContainer.Content = settingsGeneralView;
    }

    private readonly string renderSettingsBackup;
    private readonly string editorSettingsBackup;
    private readonly string audioSettingsBackup;
    private readonly string shortcutSettingsBackup;
    
    private readonly SettingsGeneralView settingsGeneralView = new();
    private readonly SettingsAudioView settingsAudioView = new();
    private readonly SettingsRenderingView settingsRenderingView = new();
    private readonly SettingsShortcutsView settingsShortcutsView = new();

    private bool saveSettings = false;
    
#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        Closing += SettingsWindow_OnClosing;
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        Closing -= SettingsWindow_OnClosing;
        
        base.OnUnloaded(e);
    }
    
    private void SettingsTab_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;

        SettingsTabContainer.Content = button.Name switch
        {
            "RadioButtonGeneral"    => settingsGeneralView,
            "RadioButtonAudio"      => settingsAudioView,
            "RadioButtonRendering"  => settingsRenderingView,
            "RadioButtonShortcuts"  => settingsShortcutsView,
            _ => null,
        };

        if (button.Name != "RadioButtonShortcuts")
        {
            settingsShortcutsView.StopDefiningShortcut();
        }
    }

    private void ButtonSave_OnClick(object? sender, RoutedEventArgs e)
    {
        saveSettings = true;
        Close();
    }

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        saveSettings = false;
        Close();
    }
    
    private void SettingsWindow_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!saveSettings)
        {
            // Restore settings from backups.
            string[] split = new[] { "\r\n", "\r", "\n" };
            string[] data = renderSettingsBackup.Split(split, StringSplitOptions.None);
            SettingsSystem.RenderSettings = ContentSerializer.ToContentItem<RenderSettings>(data) ?? SettingsSystem.RenderSettings;
            
            data = editorSettingsBackup.Split(split, StringSplitOptions.None);
            SettingsSystem.EditorSettings = ContentSerializer.ToContentItem<EditorSettings>(data) ?? SettingsSystem.EditorSettings;
            
            data = audioSettingsBackup.Split(split, StringSplitOptions.None);
            SettingsSystem.AudioSettings = ContentSerializer.ToContentItem<AudioSettings>(data) ?? SettingsSystem.AudioSettings;
            
            data = shortcutSettingsBackup.Split(split, StringSplitOptions.None);
            SettingsSystem.ShortcutSettings = ContentSerializer.ToContentItem<ShortcutSettings>(data) ?? SettingsSystem.ShortcutSettings;
        }
    }
#endregion UI Event Handlers
}