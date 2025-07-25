using System;
using System.ComponentModel;
using System.IO;
using SaturnView;
using Tomlyn;

namespace SaturnEdit.Systems;

[Serializable]
public static class SettingsSystem
{
    static SettingsSystem()
    {
        LoadSettings();
        SaveSettings();
        
        RenderSettings.PropertyChanged += OnPropertyChanged;
        EditorSettings.PropertyChanged += OnPropertyChanged;
        AudioSettings.PropertyChanged += OnPropertyChanged;
    }

    private static void OnPropertyChanged(object? sender, EventArgs e)
    {
        SettingsChanged?.Invoke(null, EventArgs.Empty);
        SaveSettings();
        
        Console.WriteLine("SettingsChanged");
    }

    public static RenderSettings RenderSettings { get; set; } = new();
    public static EditorSettings EditorSettings { get; set; } = new();
    public static AudioSettings AudioSettings { get; set; } = new();
    
    public static event EventHandler? SettingsChanged;

    private static string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaturnEdit/Settings");
    
    public static void LoadSettings()
    {
        try
        {
            string editorSettingsPath = Path.Combine(SettingsPath, "editor_settings.toml");
            string editorSettings = File.ReadAllText(editorSettingsPath);
            EditorSettings = Toml.ToModel<EditorSettings>(editorSettings);
        }
        catch
        {
            EditorSettings = new();
        }
        
        try
        {
            string renderSettingsPath = Path.Combine(SettingsPath, "render_settings.toml");
            string renderSettings = File.ReadAllText(renderSettingsPath);
            RenderSettings = Toml.ToModel<RenderSettings>(renderSettings);
        }
        catch
        {
            RenderSettings = new();
        }
        
        try
        {
            string audioSettingsPath = Path.Combine(SettingsPath, "audio_settings.toml");
            string audioSettings = File.ReadAllText(audioSettingsPath);
            AudioSettings = Toml.ToModel<AudioSettings>(audioSettings);
        }
        catch
        {
            AudioSettings = new();
        }
        
        SettingsChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void SaveSettings()
    {
        Directory.CreateDirectory(SettingsPath);

        File.WriteAllText(Path.Combine(SettingsPath, "render_settings.toml"), Toml.FromModel(RenderSettings));
        File.WriteAllText(Path.Combine(SettingsPath, "editor_settings.toml"), Toml.FromModel(EditorSettings));
        File.WriteAllText(Path.Combine(SettingsPath, "audio_settings.toml"), Toml.FromModel(AudioSettings));
    }
}

public class EditorSettings
{
    public event EventHandler? PropertyChanged;
    
    public enum EditorThemeOptions
    {
        Light = 0,
        Dark = 1,
    }
    
    public string Locale
    {
        get => locale;
        set
        {
            if (locale != value)
            {
                locale = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    private string locale = "en-US";

    public EditorThemeOptions Theme
    {
        get => theme;
        set
        {
            if (theme != value)
            {
                theme = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    private EditorThemeOptions theme = EditorThemeOptions.Dark;
}

public class AudioSettings
{
    public event EventHandler? PropertyChanged;

    public bool LoopPlayback
    {
        get => loopPlayback;
        set
        {
            if (loopPlayback != value)
            {
                loopPlayback = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    private bool loopPlayback = false;
    

    public bool Metronome
    {
        get => metronome;
        set
        {
            if (metronome != value)
            {
                metronome = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private bool metronome = false;
    
    
    public int MasterVolume
    {
        get => masterVolume;
        set
        {
            if (masterVolume != value)
            {
                masterVolume = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    private int masterVolume = 0;
    

    public int AudioVolume
    {
        get => audioVolume;
        set
        {
            if (audioVolume != value)
            {
                audioVolume = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    private int audioVolume = 0;
    

    public int GuideVolume
    {
        get => guideVolume;
        set
        {
            if (guideVolume != value)
            {
                guideVolume = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    private int guideVolume = 0;

    public int TouchVolume
    {
        get => touchVolume;
        set
        {
            if (touchVolume != value)
            {
                touchVolume = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private int touchVolume = 0;
    
    public int ChainVolume
    {
        get => chainVolume;
        set
        {
            if (chainVolume != value)
            {
                chainVolume = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private int chainVolume = 0;
    
    public int HoldVolume
    {
        get => holdVolume;
        set
        {
            if (holdVolume != value)
            {
                holdVolume = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private int holdVolume = 0;
    
    public int HoldLoopVolume
    {
        get => holdLoopVolume;
        set
        {
            if (holdLoopVolume != value)
            {
                holdLoopVolume = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private int holdLoopVolume = 0;
    
    public int SlideVolume
    {
        get => slideVolume;
        set
        {
            if (slideVolume != value)
            {
                slideVolume = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private int slideVolume = 0;
    
    public int SnapVolume
    {
        get => snapVolume;
        set
        {
            if (snapVolume != value)
            {
                snapVolume = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    private int snapVolume = 0;
    
    public int BonusVolume
    {
        get => bonusVolume;
        set
        {
            if (bonusVolume != value)
            {
                bonusVolume = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private int bonusVolume = 0;
    
    public int RVolume
    {
        get => rVolume;
        set
        {
            if (rVolume != value)
            {
                rVolume = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private int rVolume = 0;
    
    public int StartClickVolume
    {
        get => startClickVolume;
        set
        {
            if (startClickVolume != value)
            {
                startClickVolume = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private int startClickVolume = 0;
    
    public int MetronomeVolume
    {
        get => metronomeVolume;
        set
        {
            if (metronomeVolume != value)
            {
                metronomeVolume = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    private int metronomeVolume = 0;
}
    
