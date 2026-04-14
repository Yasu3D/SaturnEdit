using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnEdit.Systems;
using SaturnEdit.Windows.Dialogs.Settings.Tabs;
using SaturnView;
using Tomlyn;

namespace SaturnEdit.Windows.Dialogs;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();

        // Lazily deep-copy settings when window opens to keep a backup.
        // This backup is used when the user clicks "cancel" to restore all settings back to before they were changed.
        renderSettingsBackup   = TomlSerializer.Serialize(SettingsSystem.RenderSettings);
        editorSettingsBackup   = TomlSerializer.Serialize(SettingsSystem.EditorSettings);
        audioSettingsBackup    = TomlSerializer.Serialize(SettingsSystem.AudioSettings);
        shortcutSettingsBackup = TomlSerializer.Serialize(SettingsSystem.ShortcutSettings);
        
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
            SettingsSystem.RenderSettings   = TomlSerializer.Deserialize<RenderSettings>(renderSettingsBackup) ?? SettingsSystem.RenderSettings;
            SettingsSystem.EditorSettings   = TomlSerializer.Deserialize<EditorSettings>(editorSettingsBackup) ?? SettingsSystem.EditorSettings;
            SettingsSystem.AudioSettings    = TomlSerializer.Deserialize<AudioSettings>(audioSettingsBackup) ?? SettingsSystem.AudioSettings;
            SettingsSystem.ShortcutSettings = TomlSerializer.Deserialize<ShortcutSettings>(shortcutSettingsBackup) ?? SettingsSystem.ShortcutSettings;
        }
    }
#endregion UI Event Handlers
}