using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
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
        ShortcutSettings.PropertyChanged += OnPropertyChanged;
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
    public static ShortcutSettings ShortcutSettings { get; set; } = new();
    
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
        
        try
        {
            string shortcutSettingsPath = Path.Combine(SettingsPath, "shortcut_settings.toml");
            string shortcutSettings = File.ReadAllText(shortcutSettingsPath);
            ShortcutSettings = Toml.ToModel<ShortcutSettings>(shortcutSettings);
        }
        catch
        {
            ShortcutSettings = new();
        }
        
        SettingsChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void SaveSettings()
    {
        Directory.CreateDirectory(SettingsPath);

        File.WriteAllText(Path.Combine(SettingsPath, "render_settings.toml"), Toml.FromModel(RenderSettings));
        File.WriteAllText(Path.Combine(SettingsPath, "editor_settings.toml"), Toml.FromModel(EditorSettings));
        File.WriteAllText(Path.Combine(SettingsPath, "audio_settings.toml"), Toml.FromModel(AudioSettings));
        File.WriteAllText(Path.Combine(SettingsPath, "shortcut_settings.toml"), Toml.FromModel(ShortcutSettings));
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
    
    public bool ChartViewTxtShowSpaces
    {
        get => chartViewTxtShowSpaces;
        set
        {
            if (chartViewTxtShowSpaces != value)
            {
                chartViewTxtShowSpaces = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    private bool chartViewTxtShowSpaces = true;
    
    public bool ChartViewTxtSyntaxHighlighting
    {
        get => chartViewTxtSyntaxHighlighting;
        set
        {
            if (chartViewTxtSyntaxHighlighting != value)
            {
                chartViewTxtSyntaxHighlighting = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    private bool chartViewTxtSyntaxHighlighting = true;
}

public class AudioSettings
{
    public event EventHandler? PropertyChanged;

    public enum QuantizedPauseOptions
    {
        Exact = 0,
        Nearest = 1,
        Previous = 2,
        Next = 3,
    }

    
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
    
    
    public QuantizedPauseOptions QuantizedPause
    {
        get => quantizedPause;
        set
        {
            if (quantizedPause != value)
            {
                quantizedPause = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    private QuantizedPauseOptions quantizedPause = QuantizedPauseOptions.Exact;
    
    
    public bool LoopToStart
    {
        get => loopToStart;
        set
        {
            if (loopToStart != value)
            {
                loopToStart = value;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    private bool loopToStart = false;
    
    
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

public class ShortcutSettings
{
    public event EventHandler? PropertyChanged;

    public Dictionary<string, Shortcut> Shortcuts { get; set; } = new()
    {
        ["Menu.Edit.Cut"]   = new(Key.X, true, false, false),
        ["Menu.Edit.Copy"]  = new(Key.C, true, false, false),
        ["Menu.Edit.Paste"] = new(Key.V, true, false, false),
    };

    public void SetShortcut(string key, Shortcut shortcut)
    {
        if (Shortcuts.TryGetValue(key, out Shortcut value) && value.Equals(shortcut))
        {
            Shortcuts[key] = shortcut;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (Shortcuts.TryAdd(key, shortcut))
        {
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

[Serializable]
public struct Shortcut(Key key, bool control, bool alt, bool shift) : IEquatable<Shortcut>
{
    /// <summary>
    /// The key to press.
    /// </summary>
    public Key Key { get; set; } = key;

    /// <summary>
    /// Is the control key pressed?
    /// </summary>
    public bool Control { get; set; } = control;
    
    /// <summary>
    /// Is the alt key pressed?
    /// </summary>
    public bool Alt { get; set; } = alt;
    
    /// <summary>
    /// Is the shift key pressed?
    /// </summary>
    public bool Shift { get; set; } = shift;

    public override string ToString()
    {
        string result = Control ? "Ctrl+" : "";
        result += Alt ? "Alt+" : "";
        result += Shift ? "Shift+" : "";
        result += $"{Key}";

        return result;
    }

    public bool Equals(Shortcut other)
    {
        return Key == other.Key && Control == other.Control && Alt == other.Alt && Shift == other.Shift;
    }

    public override bool Equals(object? obj)
    {
        return obj is Shortcut other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Key, Control, Alt, Shift);
    }
}
    
