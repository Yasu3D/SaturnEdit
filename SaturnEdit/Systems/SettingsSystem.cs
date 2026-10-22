using System;
using System.IO;
using SaturnData.Content.Serialization;
using SaturnEdit.Settings;
using SaturnEdit.Utilities;
using SaturnView;

namespace SaturnEdit.Systems;

public static class SettingsSystem
{
    public static void Initialize()
    {
        LoadSettings();
        SaveSettings();
        
        renderSettings.PropertyChanged += OnPropertyChanged;
        editorSettings.PropertyChanged += OnPropertyChanged;
        audioSettings.PropertyChanged += OnPropertyChanged;
        shortcutSettings.PropertyChanged += OnPropertyChanged;
    }
    
    public static event EventHandler? SettingsChanged;
    public static event EventHandler? VolumeChanged;
    public static event EventHandler? HitsoundsChanged;
    
    public static RenderSettings RenderSettings
    {
        get => renderSettings;
        set
        {
            renderSettings.PropertyChanged -= OnPropertyChanged;
            renderSettings = value;
            renderSettings.PropertyChanged += OnPropertyChanged;
            
            SettingsChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static RenderSettings renderSettings = new();
    
    public static EditorSettings EditorSettings
    {
        get => editorSettings;
        set
        {
            editorSettings.PropertyChanged -= OnPropertyChanged;
            editorSettings = value;
            editorSettings.PropertyChanged += OnPropertyChanged;
            
            SettingsChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static EditorSettings editorSettings = new();
    
    public static AudioSettings AudioSettings
    {
        get => audioSettings;
        set
        {
            audioSettings.PropertyChanged -= OnPropertyChanged;
            audioSettings.VolumeChanged -= OnVolumeChanged;
            audioSettings.HitsoundsChanged -= OnHitsoundsChanged;
            audioSettings = value;
            audioSettings.PropertyChanged += OnPropertyChanged;
            audioSettings.VolumeChanged += OnVolumeChanged;
            audioSettings.HitsoundsChanged += OnHitsoundsChanged;
            
            SettingsChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static AudioSettings audioSettings = new();
    
    public static ShortcutSettings ShortcutSettings
    {
        get => shortcutSettings;
        set
        {
            shortcutSettings.PropertyChanged -= OnPropertyChanged;
            shortcutSettings = value;
            shortcutSettings.PropertyChanged += OnPropertyChanged;
            
            SettingsChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static ShortcutSettings shortcutSettings = new();
    
    private static string SettingsDirectory => Path.Combine(PersistentDataPathHelper.PersistentDataPath, "Settings");

#region Methods
    public static void LoadSettings()
    {
        try
        {
            string editorSettingsPath = Path.Combine(SettingsDirectory, "editor_settings.sct");
            EditorSettings = ContentSerializer.ToContentItem<EditorSettings>(editorSettingsPath) ?? new(); //TomlSerializer.Deserialize<EditorSettings>(editorSettingsData) ?? new();
        }
        catch (Exception ex)
        {
            if (ex is not (FileNotFoundException or DirectoryNotFoundException))
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
            EditorSettings = new();
        }
        
        try
        {
            string renderSettingsPath = Path.Combine(SettingsDirectory, "render_settings.sct");
            RenderSettings = ContentSerializer.ToContentItem<RenderSettings>(renderSettingsPath) ?? new(); //TomlSerializer.Deserialize<RenderSettings>(renderSettingsData) ?? new();
        }
        catch (Exception ex)
        {
            if (ex is not (FileNotFoundException or DirectoryNotFoundException))
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
            RenderSettings = new();
        }
        
        try
        {
            string audioSettingsPath = Path.Combine(SettingsDirectory, "audio_settings.sct");
            AudioSettings = ContentSerializer.ToContentItem<AudioSettings>(audioSettingsPath) ?? new(); //TomlSerializer.Deserialize<AudioSettings>(audioSettingsData) ?? new();
        }
        catch (Exception ex)
        {
            if (ex is not (FileNotFoundException or DirectoryNotFoundException))
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
            AudioSettings = new();
        }
        
        try
        {
            string shortcutSettingsPath = Path.Combine(SettingsDirectory, "shortcut_settings.sct");
            ShortcutSettings = ContentSerializer.ToContentItem<ShortcutSettings>(shortcutSettingsPath) ?? new();//TomlSerializer.Deserialize<ShortcutSettings>(shortcutSettingsData) ?? new();
        }
        catch (Exception ex)
        {
            if (ex is not (FileNotFoundException or DirectoryNotFoundException))
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
            ShortcutSettings = new();
        }
        
        SettingsChanged?.Invoke(null, EventArgs.Empty);
    }

    public static async void SaveSettings()
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);

            await File.WriteAllTextAsync(Path.Combine(SettingsDirectory, "render_settings.sct"), ContentSerializer.ToString(RenderSettings));
            await File.WriteAllTextAsync(Path.Combine(SettingsDirectory, "editor_settings.sct"), ContentSerializer.ToString(EditorSettings));
            await File.WriteAllTextAsync(Path.Combine(SettingsDirectory, "audio_settings.sct"), ContentSerializer.ToString(AudioSettings));
            await File.WriteAllTextAsync(Path.Combine(SettingsDirectory, "shortcut_settings.sct"), ContentSerializer.ToString(ShortcutSettings));
        }
        catch (Exception ex)
        {
            // Don't throw.
            if (ex is not IOException)
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
        }
    }
#endregion Methods
    
#region Internal Event Handlers
    private static void OnPropertyChanged(object? sender, EventArgs e)
    {
        SaveSettings();
        SettingsChanged?.Invoke(null, EventArgs.Empty);
    }
    
    private static void OnVolumeChanged(object? sender, EventArgs e)
    {
        VolumeChanged?.Invoke(null, EventArgs.Empty);
    }
    
    private static void OnHitsoundsChanged(object? sender, EventArgs e)
    {
        HitsoundsChanged?.Invoke(null, EventArgs.Empty);
    }
#endregion Internal Event Handlers
}