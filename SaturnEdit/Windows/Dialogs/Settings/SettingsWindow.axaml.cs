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
        renderSettingsBackup   = Toml.FromModel(SettingsSystem.RenderSettings);
        editorSettingsBackup   = Toml.FromModel(SettingsSystem.EditorSettings);
        audioSettingsBackup    = Toml.FromModel(SettingsSystem.AudioSettings);
        shortcutSettingsBackup = Toml.FromModel(SettingsSystem.ShortcutSettings);
        
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
            SettingsSystem.RenderSettings   = Toml.ToModel<RenderSettings>(renderSettingsBackup);
            SettingsSystem.EditorSettings   = Toml.ToModel<EditorSettings>(editorSettingsBackup);
            SettingsSystem.AudioSettings    = Toml.ToModel<AudioSettings>(audioSettingsBackup);
            SettingsSystem.ShortcutSettings = Toml.ToModel<ShortcutSettings>(shortcutSettingsBackup);
        }
    }
#endregion UI Event Handlers
}