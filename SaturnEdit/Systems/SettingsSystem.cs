using System;
using System.IO;
using SaturnData.Content.Serialization;
using SaturnEdit.Settings;
using SaturnEdit.Utilities;
using SaturnView;
using Tomlyn;

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
    private static string SettingsBackupDirectory => Path.Combine(SettingsDirectory, "Backup");
    
    private static string EditorSettingsPath => Path.Combine(SettingsDirectory, "editor.sct");
    private static string RenderSettingsPath => Path.Combine(SettingsDirectory, "rendering.sct");
    private static string AudioSettingsPath => Path.Combine(SettingsDirectory, "audio.sct");
    private static string ShortcutSettingsPath => Path.Combine(SettingsDirectory, "shortcuts.sct");
    
    private static string EditorSettingsBackupPath => Path.Combine(SettingsBackupDirectory, "editor.sct");
    private static string RenderSettingsBackupPath => Path.Combine(SettingsBackupDirectory, "rendering.sct");
    private static string AudioSettingsBackupPath => Path.Combine(SettingsBackupDirectory, "audio.sct");
    private static string ShortcutSettingsBackupPath => Path.Combine(SettingsBackupDirectory, "shortcuts.sct");
    
#region Methods
    public static void LoadSettings()
    {
        try
        {
            if (File.Exists(EditorSettingsPath))
            {
                EditorSettings = ContentSerializer.ToContentItem<EditorSettings>(EditorSettingsPath) ?? new();
            }
            else
            {
                string tomlPath = Path.Combine(SettingsDirectory, "editor_settings.toml");
                string data = File.ReadAllText(tomlPath);
                
                EditorSettings = TomlSerializer.Deserialize<EditorSettings>(data) ?? new();
            }
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
            if (File.Exists(RenderSettingsPath))
            {
                RenderSettings = ContentSerializer.ToContentItem<RenderSettings>(RenderSettingsPath) ?? new();
            }
            else
            {
                string tomlPath = Path.Combine(SettingsDirectory, "render_settings.toml");
                string data = File.ReadAllText(tomlPath);
                
                RenderSettings = TomlSerializer.Deserialize<RenderSettings>(data) ?? new();
            }
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
            if (File.Exists(AudioSettingsPath))
            {
                AudioSettings = ContentSerializer.ToContentItem<AudioSettings>(AudioSettingsPath) ?? new();
            }
            else
            {
                string tomlPath = Path.Combine(SettingsDirectory, "audio_settings.toml");
                string data = File.ReadAllText(tomlPath);
                
                AudioSettings = TomlSerializer.Deserialize<AudioSettings>(data) ?? new();
            }
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
            if (File.Exists(ShortcutSettingsPath))
            {
                ShortcutSettings = ContentSerializer.ToContentItem<ShortcutSettings>(ShortcutSettingsPath) ?? new();
            }
            else
            {
                string tomlPath = Path.Combine(SettingsDirectory, "shortcut_settings.toml");
                string data = File.ReadAllText(tomlPath);
                
                ShortcutSettings = TomlSerializer.Deserialize<ShortcutSettings>(data) ?? new();
            }
        }
        catch (Exception ex)
        {
            if (ex is not (FileNotFoundException or DirectoryNotFoundException))
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
            ShortcutSettings = new();
        }
        
        BackupSettings();
        SettingsChanged?.Invoke(null, EventArgs.Empty);
    }

    public static async void SaveSettings()
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);

            await File.WriteAllTextAsync(EditorSettingsPath, ContentSerializer.ToString(RenderSettings));
            await File.WriteAllTextAsync(RenderSettingsPath, ContentSerializer.ToString(EditorSettings));
            await File.WriteAllTextAsync(AudioSettingsPath, ContentSerializer.ToString(AudioSettings));
            await File.WriteAllTextAsync(ShortcutSettingsPath, ContentSerializer.ToString(ShortcutSettings));
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

    public static void BackupSettings()
    {
        Directory.CreateDirectory(SettingsBackupDirectory);

        backup(EditorSettingsPath,   EditorSettingsBackupPath,   true);
        backup(RenderSettingsPath,   RenderSettingsBackupPath,   true);
        backup(AudioSettingsPath,    AudioSettingsBackupPath,    true);
        backup(ShortcutSettingsPath, ShortcutSettingsBackupPath, true);
        
        backup(Path.Combine(SettingsDirectory, "editor_settings.toml"),   Path.Combine(SettingsBackupDirectory, "editor_settings.toml"),   false);
        backup(Path.Combine(SettingsDirectory, "render_settings.toml"),   Path.Combine(SettingsBackupDirectory, "render_settings.toml"),   false);
        backup(Path.Combine(SettingsDirectory, "audio_settings.toml"),    Path.Combine(SettingsBackupDirectory, "audio_settings.toml"),    false);
        backup(Path.Combine(SettingsDirectory, "shortcut_settings.toml"), Path.Combine(SettingsBackupDirectory, "shortcut_settings.toml"), false);

        return;

        void backup(string source, string destination, bool copy)
        {
            if (!File.Exists(source)) return;
            
            try
            {
                if (copy)
                {
                    File.Copy(source, destination, true);
                }
                else
                {
                    File.Move(source, destination, true);
                }
                
            }
            catch (Exception ex)
            {
                // Don't throw.
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