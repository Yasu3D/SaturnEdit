using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Input;
using SaturnEdit.Utilities;
using SaturnView;
using Tomlyn;

namespace SaturnEdit.Systems;

[Serializable]
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
            string editorSettingsPath = Path.Combine(SettingsDirectory, "editor_settings.toml");
            string editorSettingsData = File.ReadAllText(editorSettingsPath);
            EditorSettings = Toml.ToModel<EditorSettings>(editorSettingsData);
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
            string renderSettingsPath = Path.Combine(SettingsDirectory, "render_settings.toml");
            string renderSettingsData = File.ReadAllText(renderSettingsPath);
            RenderSettings = Toml.ToModel<RenderSettings>(renderSettingsData);
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
            string audioSettingsPath = Path.Combine(SettingsDirectory, "audio_settings.toml");
            string audioSettingsData = File.ReadAllText(audioSettingsPath);
            AudioSettings = Toml.ToModel<AudioSettings>(audioSettingsData);
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
            string shortcutSettingsPath = Path.Combine(SettingsDirectory, "shortcut_settings.toml");
            string shortcutSettingsData = File.ReadAllText(shortcutSettingsPath);
            ShortcutSettings = Toml.ToModel<ShortcutSettings>(shortcutSettingsData);
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

            await File.WriteAllTextAsync(Path.Combine(SettingsDirectory, "render_settings.toml"), Toml.FromModel(RenderSettings));
            await File.WriteAllTextAsync(Path.Combine(SettingsDirectory, "editor_settings.toml"), Toml.FromModel(EditorSettings));
            await File.WriteAllTextAsync(Path.Combine(SettingsDirectory, "audio_settings.toml"), Toml.FromModel(AudioSettings));
            await File.WriteAllTextAsync(Path.Combine(SettingsDirectory, "shortcut_settings.toml"), Toml.FromModel(ShortcutSettings));
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
        SettingsChanged?.Invoke(null, EventArgs.Empty);
        SaveSettings();
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

public class EditorSettings
{
    public event EventHandler? PropertyChanged;

#region Enum Definitions

    public enum LocaleOption
    {
        // ReSharper disable InconsistentNaming
        en_US = 0,
        ja_JP = 1,
        zh_CN = 2,
        // ReSharper restore InconsistentNaming
    }
    
    public enum EditorThemeOption
    {
        Light = 0,
        Dark = 1,
    }

    public enum AutoSaveFrequencyOption
    {
        Never = 0,
        Minutes10 = 1,
        Minutes5 = 2,
        Minutes1 = 3,
        Always = 4,
    }
    
#endregion Enum Definitions
    
    public LocaleOption Locale
    {
        get => locale;
        set
        {
            if (locale == value) return;
            
            locale = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private LocaleOption locale = LocaleOption.en_US;

    public EditorThemeOption Theme
    {
        get => theme;
        set
        {
            if (theme == value) return;
            
            theme = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private EditorThemeOption theme = EditorThemeOption.Dark;
    
    public bool ShowSplashScreen
    {
        get => showSplashScreen;
        set
        {
            if (showSplashScreen == value) return;
            
            showSplashScreen = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool showSplashScreen = true;
    
    public bool ContinueLastSession
    {
        get => continueLastSession;
        set
        {
            if (continueLastSession == value) return;
            
            continueLastSession = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool continueLastSession = false;
    
    public bool CheckForUpdates
    {
        get => checkForUpdates;
        set
        {
            if (checkForUpdates == value) return;
            
            checkForUpdates = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool checkForUpdates = true;
    
    public string LastSessionPath
    {
        get => lastSessionPath;
        set
        {
            if (lastSessionPath == value) return;

            lastSessionPath = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string lastSessionPath = "";
    
    public bool ChartViewTxtShowSpaces
    {
        get => chartViewTxtShowSpaces;
        set
        {
            if (chartViewTxtShowSpaces == value) return;
            
            chartViewTxtShowSpaces = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool chartViewTxtShowSpaces = true;
    
    public bool ChartViewTxtSyntaxHighlighting
    {
        get => chartViewTxtSyntaxHighlighting;
        set
        {
            if (chartViewTxtSyntaxHighlighting == value) return;
            
            chartViewTxtSyntaxHighlighting = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool chartViewTxtSyntaxHighlighting = true;

    public AutoSaveFrequencyOption AutoSaveFrequency
    {
        get => autoSaveFrequency;
        set
        {
            if (autoSaveFrequency == value) return;
            
            autoSaveFrequency = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private AutoSaveFrequencyOption autoSaveFrequency = AutoSaveFrequencyOption.Minutes5;

    public int ClickDragThreshold
    {
        get => clickDragThreshold;
        set
        {
            if (clickDragThreshold == value) return;
            
            clickDragThreshold = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int clickDragThreshold = 5;
    
    public List<string> RecentChartFiles { get; set; } = [];
    
    public void AddRecentChartFile(string path)
    {
        // Don't add autosaved files to recent files.
        DirectoryInfo fileDirectory = new(Path.GetDirectoryName(path) ?? "");
        DirectoryInfo autosaveDirectory = new(AutosaveSystem.AutosaveDirectory);
        if (fileDirectory.FullName == autosaveDirectory.FullName) return;
        
        RecentChartFiles.Add(path);

        while (RecentChartFiles.Count > 10)
        {
            RecentChartFiles.RemoveAt(0);
        }
            
        PropertyChanged?.Invoke(null, EventArgs.Empty);
    }

    public void RemoveRecentChartFile(string path)
    {
        RecentChartFiles.Remove(path);
        PropertyChanged?.Invoke(null, EventArgs.Empty);
    }
    
    public void ClearRecentChartFiles()
    {
        RecentChartFiles.Clear();
        PropertyChanged?.Invoke(null, EventArgs.Empty);
    }
}

public class AudioSettings
{
    public event EventHandler? PropertyChanged;

    public event EventHandler? VolumeChanged;
    public event EventHandler? HitsoundsChanged;

#region Enum Definitions

    public enum QuantizationOption
    {
        Off = 0,
        Nearest = 1,
        Previous = 2,
        Next = 3,
    }
    
#endregion
    
    public bool LoopPlayback
    {
        get => loopPlayback;
        set
        {
            if (loopPlayback == value) return;
            
            loopPlayback = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool loopPlayback = false;
    
    public bool Metronome
    {
        get => metronome;
        set
        {
            if (metronome == value) return;
           
            metronome = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool metronome = false;
    
    public QuantizationOption QuantizedPause
    {
        get => quantizedPause;
        set
        {
            if (quantizedPause == value) return;
            
            quantizedPause = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private QuantizationOption quantizedPause = QuantizationOption.Off;

    public QuantizationOption QuantizedSeek
    {
        get => quantizedSeek;
        set
        {
            if (quantizedSeek == value) return;
            
            quantizedSeek = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private QuantizationOption quantizedSeek = QuantizationOption.Off;
    
    public bool LoopToStart
    {
        get => loopToStart;
        set
        {
            if (loopToStart == value) return;
            
            loopToStart = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool loopToStart = false;
    
    public int MasterVolume
    {
        get => masterVolume;
        set
        {
            if (masterVolume == value) return;
            
            masterVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int masterVolume = -30;
    
    public int AudioVolume
    {
        get => audioVolume;
        set
        {
            if (audioVolume == value) return;
            
            audioVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int audioVolume = 0;
    
    public int HitsoundVolume
    {
        get => hitsoundVolume;
        set
        {
            if (hitsoundVolume == value) return;
            
            hitsoundVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int hitsoundVolume = 0;
    
    public int GuideVolume
    {
        get => guideVolume;
        set
        {
            if (guideVolume == value) return;
            
            guideVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int guideVolume = -20;
    
    public int TouchVolume
    {
        get => touchVolume;
        set
        {
            if (touchVolume == value) return;
            
            touchVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int touchVolume = 0;
    
    public int HoldVolume
    {
        get => holdVolume;
        set
        {
            if (holdVolume == value) return;
            
            holdVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int holdVolume = 0;
    
    public int SlideVolume
    {
        get => slideVolume;
        set
        {
            if (slideVolume == value) return;
            
            slideVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int slideVolume = 0;
    
    public int BonusVolume
    {
        get => bonusVolume;
        set
        {
            if (bonusVolume == value) return;
            
            bonusVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int bonusVolume = 0;
    
    public int RVolume
    {
        get => rVolume;
        set
        {
            if (rVolume == value) return;
            
            rVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int rVolume = 0;
    
    public int StartClickVolume
    {
        get => startClickVolume;
        set
        {
            if (startClickVolume == value) return;
            
            startClickVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int startClickVolume = 0;
    
    public int MetronomeVolume
    {
        get => metronomeVolume;
        set
        {
            if (metronomeVolume == value) return;
            
            metronomeVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int metronomeVolume = 0;
    
    public bool MuteMaster
    {
        get => muteMaster;
        set
        {
            if (muteMaster == value) return;
            
            muteMaster = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteMaster = false;
    
    public bool MuteAudio
    {
        get => muteAudio;
        set
        {
            if (muteAudio == value) return;
            
            muteAudio = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteAudio = false;
    
    public bool MuteHitsound
    {
        get => muteHitsound;
        set
        {
            if (muteHitsound == value) return;
            
            muteHitsound = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteHitsound = false;
    
    public bool MuteGuide
    {
        get => muteGuide;
        set
        {
            if (muteGuide == value) return;
            
            muteGuide = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteGuide = false;
    
    public bool MuteTouch
    {
        get => muteTouch;
        set
        {
            if (muteTouch == value) return;
            
            muteTouch = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteTouch = false;
    
    public bool MuteHold
    {
        get => muteHold;
        set
        {
            if (muteHold == value) return;
            
            muteHold = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteHold = false;
    
    public bool MuteSlide
    {
        get => muteSlide;
        set
        {
            if (muteSlide == value) return;
            
            muteSlide = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteSlide = false;
    
    public bool MuteBonus
    {
        get => muteBonus;
        set
        {
            if (muteBonus == value) return;
            
            muteBonus = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteBonus = false;
    
    public bool MuteR
    {
        get => muteR;
        set
        {
            if (muteR == value) return;
            
            muteR = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteR = false;
    
    public bool MuteStartClick
    {
        get => muteStartClick;
        set
        {
            if (muteStartClick == value) return;
            
            muteStartClick = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteStartClick = false;
    
    public bool MuteMetronome
    {
        get => muteMetronome;
        set
        {
            if (muteMetronome == value) return;
            
            muteMetronome = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteMetronome = false;

    public string HitsoundGuidePath
    {
        get => hitsoundGuidePath;
        set
        {
            if (hitsoundGuidePath == value) return;
            
            hitsoundGuidePath = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            HitsoundsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string hitsoundGuidePath = "";
    
    public string HitsoundTouchPath
    {
        get => hitsoundTouchPath;
        set
        {
            if (hitsoundTouchPath == value) return;
            
            hitsoundTouchPath = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            HitsoundsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string hitsoundTouchPath = "";
    
    public string HitsoundHoldPath
    {
        get => hitsoundHoldPath;
        set
        {
            if (hitsoundHoldPath == value) return;
            
            hitsoundHoldPath = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            HitsoundsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string hitsoundHoldPath = "";
    
    public string HitsoundSlidePath
    {
        get => hitsoundSlidePath;
        set
        {
            if (hitsoundSlidePath == value) return;
            
            hitsoundSlidePath = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            HitsoundsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string hitsoundSlidePath = "";
    
    public string HitsoundBonusPath
    {
        get => hitsoundBonusPath;
        set
        {
            if (hitsoundBonusPath == value) return;
            
            hitsoundBonusPath = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            HitsoundsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string hitsoundBonusPath = "";
    
    public string HitsoundRPath
    {
        get => hitsoundRPath;
        set
        {
            if (hitsoundRPath == value) return;
            
            hitsoundRPath = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            HitsoundsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string hitsoundRPath = "";
    
    public string HitsoundStartClickPath
    {
        get => hitsoundStartClickPath;
        set
        {
            if (hitsoundStartClickPath == value) return;
            
            hitsoundStartClickPath = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            HitsoundsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string hitsoundStartClickPath = "";
    
    public string HitsoundMetronomePath
    {
        get => hitsoundMetronomePath;
        set
        {
            if (hitsoundMetronomePath == value) return;
            
            hitsoundMetronomePath = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            HitsoundsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string hitsoundMetronomePath = "";
    
    public float HoldLoopStart
    {
        get => holdLoopStart;
        set
        {
            if (holdLoopStart == value) return;
            
            holdLoopStart = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private float holdLoopStart = 1655.0f;
    
    public float HoldLoopEnd
    {
        get => holdLoopEnd;
        set
        {
            if (holdLoopEnd == value) return;
            
            holdLoopEnd = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private float holdLoopEnd = 3482.0f;
}

public class ShortcutSettings
{
    public event EventHandler? PropertyChanged;

    /// <summary>
    /// A dictionary of all shortcuts.
    /// </summary>
    /// <remarks>
    /// Shortcuts should be set via <see cref="SetShortcut"/>.<br/>
    /// Changes made directly to the dictionary will not invoke <see cref="PropertyChanged"/>.
    /// </remarks>
    public Dictionary<string, Shortcut> Shortcuts { get; } = new()
    {
        ["File.New"]                    = new(Key.N,    true,  false, false, "Menu.File", "Menu.File.New"),
        ["File.Open"]                   = new(Key.O,    true,  false, false, "Menu.File", "Menu.File.Open"),
        ["File.Save"]                   = new(Key.S,    true,  false, false, "Menu.File", "Menu.File.Save"),
        ["File.SaveAs"]                 = new(Key.S,    true,  false, true,  "Menu.File", "Menu.File.SaveAs"),
        ["File.ReloadFromDisk"]         = new(Key.R,    true,  true,  false, "Menu.File", "Menu.File.ReloadFromDisk"),
        ["File.NewDifficultyFromChart"] = new(Key.None, false, false, false, "Menu.File", "Menu.File.NewDifficultyFromChart"),
        ["File.Export"]                 = new(Key.None, false, false, false, "Menu.File", "Menu.File.Export"),
        //["File.RenderAsImage"]          = new(Key.None, false, false, false, "Menu.File", "Menu.File.RenderAsImage"),
        ["File.Quit"]                   = new(Key.None, false, false, false, "Menu.File", "Menu.File.Quit"),

        ["Edit.Undo"]                 = new(Key.Z,    true,  false, false, "Menu.Edit", "Menu.Edit.Undo"),
        ["Edit.Redo"]                 = new(Key.Y,    true,  false, false, "Menu.Edit", "Menu.Edit.Redo"),
        ["Edit.Cut"]                  = new(Key.X,    true,  false, false, "Menu.Edit", "Menu.Edit.Cut"),
        ["Edit.Copy"]                 = new(Key.C,    true,  false, false, "Menu.Edit", "Menu.Edit.Copy"),
        ["Edit.Paste"]                = new(Key.V,    true,  false, false, "Menu.Edit", "Menu.Edit.Paste"),
        ["Edit.SelectAll"]            = new(Key.A,    true,  false, false, "Menu.Edit", "Menu.Edit.SelectAll"),
        ["Edit.DeselectAll"]          = new(Key.A,    false, true,  false, "Menu.Edit", "Menu.Edit.DeselectAll"),
        ["Edit.SelectNearestObject"]  = new(Key.Q,    false, false, true,  "Menu.Edit", "Menu.Edit.SelectNearestObject"),
        ["Edit.SelectNextObject"]     = new(Key.W,    false, false, true,  "Menu.Edit", "Menu.Edit.SelectNextObject"),
        ["Edit.SelectPreviousObject"] = new(Key.S,    false, false, true,  "Menu.Edit", "Menu.Edit.SelectPreviousObject"),
        ["Edit.CheckerDeselect"]      = new(Key.None, false, false, false, "Menu.Edit", "Menu.Edit.CheckerDeselect"),
        ["Edit.SelectByCriteria"]     = new(Key.None, false, false, false, "Menu.Edit", "Menu.Edit.SelectByCriteria"),

        ["Navigate.MoveBeatForward"]      = new(Key.Up,       false, false, false, "Menu.Navigate", "Menu.Navigate.MoveBeatForward"),
        ["Navigate.MoveBeatBack"]         = new(Key.Down,     false, false, false, "Menu.Navigate", "Menu.Navigate.MoveBeatBack"),
        ["Navigate.MoveMeasureForward"]   = new(Key.Right,    false, false, false, "Menu.Navigate", "Menu.Navigate.MoveMeasureForward"),
        ["Navigate.MoveMeasureBack"]      = new(Key.Left,     false, false, false, "Menu.Navigate", "Menu.Navigate.MoveMeasureBack"),
        ["Navigate.JumpToNextObject"]     = new(Key.None,     false, false, false, "Menu.Navigate", "Menu.Navigate.JumpToNextObject"),
        ["Navigate.JumpToPreviousObject"] = new(Key.None,     false, false, false, "Menu.Navigate", "Menu.Navigate.JumpToPreviousObject"),
        ["Navigate.IncreaseBeatDivision"] = new(Key.PageUp,   false, false, true,  "Menu.Navigate", "Menu.Navigate.IncreaseBeatDivision"),
        ["Navigate.DecreaseBeatDivision"] = new(Key.PageDown, false, false, true,  "Menu.Navigate", "Menu.Navigate.DecreaseBeatDivision"),
        ["Navigate.DoubleBeatDivision"]   = new(Key.PageUp,   true,  false, false, "Menu.Navigate", "Menu.Navigate.DoubleBeatDivision"),
        ["Navigate.HalveBeatDivision"]    = new(Key.PageDown, true,  false, false, "Menu.Navigate", "Menu.Navigate.HalveBeatDivision"),

        ["QuickCommands.Settings"]    = new(Key.S,    true,  true,  false, "Menu.QuickCommands", "Main.ToolTip.Settings"),
        ["QuickCommands.VolumeMixer"] = new(Key.None, false, false, false, "Menu.QuickCommands", "Main.ToolTip.VolumeMixer"),
        ["QuickCommands.Search"]      = new(Key.F,    true,  false, false, "Menu.QuickCommands", "Main.ToolTip.Search"),

        ["NotePalette.NoteType.Touch"]                 = new(Key.D1,   false, false, false, "ChartEditor.NotePalette", "ChartEditor.NotePalette.Note.Touch"),
        ["NotePalette.NoteType.Chain"]                 = new(Key.D2,   false, false, false, "ChartEditor.NotePalette", "ChartEditor.NotePalette.Note.Chain"),
        ["NotePalette.NoteType.Hold"]                  = new(Key.D3,   false, false, false, "ChartEditor.NotePalette", "ChartEditor.NotePalette.Note.Hold"),
        ["NotePalette.NoteType.SlideClockwise"]        = new(Key.D4,   false, false, false, "ChartEditor.NotePalette", "ChartEditor.NotePalette.Note.SlideClockwise"),
        ["NotePalette.NoteType.SlideCounterclockwise"] = new(Key.D5,   false, false, false, "ChartEditor.NotePalette", "ChartEditor.NotePalette.Note.SlideCounterclockwise"),
        ["NotePalette.NoteType.SnapForward"]           = new(Key.D6,   false, false, false, "ChartEditor.NotePalette", "ChartEditor.NotePalette.Note.SnapForward"),
        ["NotePalette.NoteType.SnapBackward"]          = new(Key.D7,   false, false, false, "ChartEditor.NotePalette", "ChartEditor.NotePalette.Note.SnapBackward"),
        ["NotePalette.NoteType.LaneShow"]              = new(Key.D8,   false, false, false, "ChartEditor.NotePalette", "ChartEditor.NotePalette.Note.LaneShow"),
        ["NotePalette.NoteType.LaneHide"]              = new(Key.D9,   false, false, false, "ChartEditor.NotePalette", "ChartEditor.NotePalette.Note.LaneHide"),
        ["NotePalette.NoteType.Sync"]                  = new(Key.D0,   false, false, false, "ChartEditor.NotePalette", "ChartEditor.NotePalette.Note.Sync"),
        ["NotePalette.NoteType.MeasureLine"]           = new(Key.Oem4, false, false, false, "ChartEditor.NotePalette", "ChartEditor.NotePalette.Note.MeasureLine"),
        ["NotePalette.BonusType.Normal"] = new(Key.D1, false, false, true, "ChartEditor.NotePalette", "ChartEditor.NotePalette.BonusType.Normal"),
        ["NotePalette.BonusType.Bonus"]  = new(Key.D2, false, false, true, "ChartEditor.NotePalette", "ChartEditor.NotePalette.BonusType.Bonus"),
        ["NotePalette.BonusType.R"]      = new(Key.D3, false, false, true, "ChartEditor.NotePalette", "ChartEditor.NotePalette.BonusType.R"),
        ["NotePalette.JudgementType.Normal"]   = new(Key.D1, true, false, false, "ChartEditor.NotePalette", "ChartEditor.NotePalette.JudgementType.Normal"),
        ["NotePalette.JudgementType.Fake"]     = new(Key.D2, true, false, false, "ChartEditor.NotePalette", "ChartEditor.NotePalette.JudgementType.Fake"),
        ["NotePalette.JudgementType.Autoplay"] = new(Key.D3, true, false, false, "ChartEditor.NotePalette", "ChartEditor.NotePalette.JudgementType.Autoplay"),
        ["NotePalette.SweepDirection.Center"]           = new(Key.D1, false, false, true, "ChartEditor.NotePalette", "ChartEditor.NotePalette.SweepDirection.Center"),
        ["NotePalette.SweepDirection.Clockwise"]        = new(Key.D2, false, false, true, "ChartEditor.NotePalette", "ChartEditor.NotePalette.SweepDirection.Clockwise"),
        ["NotePalette.SweepDirection.Counterclockwise"] = new(Key.D3, false, false, true, "ChartEditor.NotePalette", "ChartEditor.NotePalette.SweepDirection.Counterclockwise"),
        ["NotePalette.SweepDirection.Instant"]          = new(Key.D4, false, false, true, "ChartEditor.NotePalette", "ChartEditor.NotePalette.SweepDirection.Instant"),
        ["NotePalette.HoldPointRenderType.Hidden"]  = new(Key.D1, false, true, false, "ChartEditor.NotePalette", "ChartEditor.NotePalette.HoldPointRenderType.Hidden"),
        ["NotePalette.HoldPointRenderType.Visible"] = new(Key.D2, false, true, false, "ChartEditor.NotePalette", "ChartEditor.NotePalette.HoldPointRenderType.Visible"),

        ["List.MoveItemUp"]   = new(Key.Up,   false, false, true, "ChartEditor.List", "ChartEditor.List.ToolTip.MoveItemUp"),
        ["List.MoveItemDown"] = new(Key.Down, false, false, true, "ChartEditor.List", "ChartEditor.List.ToolTip.MoveItemDown"),
        
        ["Cursor.IncreasePosition"] = new(Key.None, false, false, false, "ChartEditor.Cursor", "ChartEditor.Cursor.IncreasePosition"),
        ["Cursor.DecreasePosition"] = new(Key.None, false, false, false, "ChartEditor.Cursor", "ChartEditor.Cursor.DecreasePosition"),
        ["Cursor.IncreaseSize"] = new(Key.None, false, false, false, "ChartEditor.Cursor", "ChartEditor.Cursor.IncreaseSize"),
        ["Cursor.DecreaseSize"] = new(Key.None, false, false, false, "ChartEditor.Cursor", "ChartEditor.Cursor.DecreaseSize"),
        
        ["Editor.Playback.Play"]                  = new(Key.Space, false, false, false, "ChartEditor.Playback", "ChartEditor.Playback.ToolTip.Play"),
        ["Editor.Playback.Pause"]                 = new(Key.Space, false, false, false, "ChartEditor.Playback", "ChartEditor.Playback.ToolTip.Pause"),
        ["Editor.Playback.IncreasePlaybackSpeed"] = new(Key.Add, false, false, false, "ChartEditor.Playback", "ChartEditor.Playback.ToolTip.IncreasePlaybackSpeed"),
        ["Editor.Playback.DecreasePlaybackSpeed"] = new(Key.Subtract, false, false, false, "ChartEditor.Playback", "ChartEditor.Playback.ToolTip.DecreasePlaybackSpeed"),
        ["Editor.Playback.LoopPlayback"]          = new(Key.L,     true,  false, false, "ChartEditor.Playback", "ChartEditor.Playback.ToolTip.Loop"),
        ["Editor.Playback.SetLoopMarkerStart"]    = new(Key.Home,  false, false, false, "ChartEditor.Playback", "ChartEditor.Playback.ToolTip.LoopStart"),
        ["Editor.Playback.SetLoopMarkerEnd"]      = new(Key.End,   false, false, false, "ChartEditor.Playback", "ChartEditor.Playback.ToolTip.LoopEnd"),
        ["Editor.Playback.Metronome"]             = new(Key.None,  false, false, false, "ChartEditor.Playback", "ChartEditor.Playback.ToolTip.Metronome"),

        ["Editor.Toolbar.EditType"]        = new(Key.E,      true,  false, false, "ChartEditor.ChartView3D.Toolbar", "ChartEditor.ChartView3D.ToolTip.EditType"),
        ["Editor.Toolbar.EditShape"]       = new(Key.E,      false, false, true,  "ChartEditor.ChartView3D.Toolbar", "ChartEditor.ChartView3D.ToolTip.EditShape"),
        ["Editor.Toolbar.EditBoth"]        = new(Key.E,      true,  false, true,  "ChartEditor.ChartView3D.Toolbar", "ChartEditor.ChartView3D.ToolTip.EditBoth"),
        ["Editor.Toolbar.DeleteSelection"] = new(Key.Delete, false, false, false, "ChartEditor.ChartView3D.Toolbar", "ChartEditor.ChartView3D.ToolTip.DeleteSelection"),
        ["Editor.Toolbar.Insert"]          = new(Key.E,      false, false, false, "ChartEditor.ChartView3D.Toolbar", "ChartEditor.ChartView3D.ToolTip.InsertNote"),

        ["Editor.AutoMode"] = new(Key.Tab,  false, false, false, "ChartEditor.General.Editor", "ChartEditor.ChartView3D.Mode.AutoMode"),
        ["Editor.ObjectMode"]   = new(Key.None, false, false, false, "ChartEditor.General.Editor", "ChartEditor.ChartView3D.Mode.ObjectMode"),
        ["Editor.EditMode"]     = new(Key.None, false, false, false, "ChartEditor.General.Editor", "ChartEditor.ChartView3D.Mode.EditMode"),

        ["Editor.Insert.TempoChange"]      = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Insert", "ChartEditor.ChartView3D.Menu.Insert.TempoChange"),
        ["Editor.Insert.MetreChange"]      = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Insert", "ChartEditor.ChartView3D.Menu.Insert.MetreChange"),
        ["Editor.Insert.TutorialMarker"]   = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Insert", "ChartEditor.ChartView3D.Menu.Insert.TutorialMarker"),
        ["Editor.Insert.SpeedChange"]      = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Insert", "ChartEditor.ChartView3D.Menu.Insert.SpeedChange"),
        ["Editor.Insert.VisibilityChange"] = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Insert", "ChartEditor.ChartView3D.Menu.Insert.VisibilityChange"),
        ["Editor.Insert.StopEffect"]       = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Insert", "ChartEditor.ChartView3D.Menu.Insert.StopEffect"),
        ["Editor.Insert.ReverseEffect"]    = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Insert", "ChartEditor.ChartView3D.Menu.Insert.ReverseEffect"),
        ["Editor.Insert.Bookmark"]         = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Insert", "ChartEditor.ChartView3D.Menu.Insert.Bookmark"),
        
        ["Editor.Transform.MoveBeatForward"]               = new(Key.Up,    true,  false, false, "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.MoveSelectionBeatForward"),
        ["Editor.Transform.MoveBeatBack"]                  = new(Key.Down,  true,  false, false, "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.MoveSelectionBeatBack"),
        ["Editor.Transform.MoveMeasureForward"]            = new(Key.Right, true,  false, false, "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.MoveSelectionMeasureForward"),
        ["Editor.Transform.MoveMeasureBack"]               = new(Key.Left,  true,  false, false, "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.MoveSelectionMeasureBack"),
        ["Editor.Transform.MoveClockwise"]                 = new(Key.Left,  false, false, true,  "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.MoveClockwise"),
        ["Editor.Transform.MoveCounterclockwise"]          = new(Key.Right, false, false, true,  "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.MoveCounterclockwise"),
        ["Editor.Transform.IncreaseSize"]                  = new(Key.Up,    false, false, true,  "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.IncreaseSize"),
        ["Editor.Transform.DecreaseSize"]                  = new(Key.Down,  false, false, true,  "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.DecreaseSize"),
        ["Editor.Transform.MoveClockwiseIterative"]        = new(Key.Left,  true,  false, true,  "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.MoveClockwiseIterative"),
        ["Editor.Transform.MoveCounterclockwiseIterative"] = new(Key.Right, true,  false, true,  "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.MoveCounterclockwiseIterative"),
        ["Editor.Transform.IncreaseSizeIterative"]         = new(Key.Up,    true,  false, true,  "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.IncreaseSizeIterative"),
        ["Editor.Transform.DecreaseSizeIterative"]         = new(Key.Down,  true,  false, true,  "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.DecreaseSizeIterative"),
        ["Editor.Transform.MirrorHorizontal"]              = new(Key.M,     false, false, true,  "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.MirrorHorizontal"),
        ["Editor.Transform.MirrorVertical"]                = new(Key.M,     true,  false, false, "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.MirrorVertical"),
        ["Editor.Transform.MirrorCustom"]                  = new(Key.M,     false, true,  false, "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.MirrorCustom"),
        ["Editor.Transform.AdjustAxis"]                    = new(Key.M,     false, true,  true,  "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.AdjustAxis"),
        ["Editor.Transform.FlipDirection"]                 = new(Key.F,     false, false, true,  "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.FlipDirection"),
        ["Editor.Transform.ReverseSelection"]              = new(Key.R,     true,  false, false, "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.ReverseSelection"),
        ["Editor.Transform.ScaleSelection"]                = new(Key.T,     true,  false, false, "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.ScaleSelection"),
        ["Editor.Transform.OffsetChart"]                   = new(Key.None,  false, false, false, "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.OffsetChart"),
        ["Editor.Transform.ScaleChart"]                    = new(Key.None,  false, false, false, "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.ScaleChart"),
        ["Editor.Transform.MirrorChart"]                   = new(Key.None,  false, false, false, "ChartEditor.ChartView3D.Menu.Transform", "ChartEditor.ChartView3D.Menu.Transform.MirrorChart"),
        
        ["Editor.Convert.ZigZagHold"]   = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Convert", "ChartEditor.ChartView3D.Menu.Convert.ZigZagHold"),
        ["Editor.Convert.CutHold"]   = new(Key.H, false, true,  false, "ChartEditor.ChartView3D.Menu.Convert", "ChartEditor.ChartView3D.Menu.Convert.CutHold"),
        ["Editor.Convert.JoinHold"]   = new(Key.H, true,  false, false, "ChartEditor.ChartView3D.Menu.Convert", "ChartEditor.ChartView3D.Menu.Convert.JoinHold"),

        ["Editor.IncreaseNoteSpeed"]     = new(Key.Add,      false, false, true,  "ChartEditor.ChartView3D.Menu.Settings", "ChartEditor.ChartView3D.Menu.IncreaseNoteSpeed"),
        ["Editor.DecreaseNoteSpeed"]     = new(Key.Subtract, false, false, true,  "ChartEditor.ChartView3D.Menu.Settings", "ChartEditor.ChartView3D.Menu.DecreaseNoteSpeed"),
        ["Editor.IncreaseBackgroundDim"] = new(Key.Add,      true,  false, false, "ChartEditor.ChartView3D.Menu.Settings", "ChartEditor.ChartView3D.Menu.IncreaseBackgroundDim"),
        ["Editor.DecreaseBackgroundDim"] = new(Key.Subtract, true,  false, false, "ChartEditor.ChartView3D.Menu.Settings", "ChartEditor.ChartView3D.Menu.DecreaseBackgroundDim"),

        ["Editor.Settings.ShowSpeedChanges"]         = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings", "ChartEditor.ChartView3D.Menu.Settings.ShowSpeedChanges"),
        ["Editor.Settings.ShowVisibilityChanges"]    = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings", "ChartEditor.ChartView3D.Menu.Settings.ShowVisibilityChanges"),
        ["Editor.Settings.ShowLaneToggleAnimations"] = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings", "ChartEditor.ChartView3D.Menu.Settings.ShowLaneToggleAnimations"),
        ["Editor.Settings.VisualizeLaneSweeps"]      = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings", "ChartEditor.ChartView3D.Menu.Settings.VisualizeLaneSweeps"),
        ["Editor.Settings.ShowJudgeAreas"]     = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings", "ChartEditor.ChartView3D.Menu.Settings.ShowJudgeAreas"),
        ["Editor.Settings.ShowMarvelousArea"]     = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings", "ChartEditor.ChartView3D.Menu.Settings.ShowMarvelousArea"),
        ["Editor.Settings.ShowGreatArea"]         = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings", "ChartEditor.ChartView3D.Menu.Settings.ShowGreatArea"),
        ["Editor.Settings.ShowGoodArea"]          = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings", "ChartEditor.ChartView3D.Menu.Settings.ShowGoodArea"),
        ["Editor.Settings.SaturnJudgeAreas"]   = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings", "ChartEditor.ChartView3D.Menu.Settings.SaturnJudgeAreas"),

        ["Editor.Settings.ToggleVisibility.Touch"]                 = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.Touch"),
        ["Editor.Settings.ToggleVisibility.SnapForward"]           = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.SnapForward"),
        ["Editor.Settings.ToggleVisibility.SnapBackward"]          = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.SnapBackward"),
        ["Editor.Settings.ToggleVisibility.SlideClockwise"]        = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.SlideClockwise"),
        ["Editor.Settings.ToggleVisibility.SlideCounterclockwise"] = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.SlideCounterclockwise"),
        ["Editor.Settings.ToggleVisibility.Chain"]                 = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.Chain"),
        ["Editor.Settings.ToggleVisibility.Hold"]                  = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.Hold"),
        ["Editor.Settings.ToggleVisibility.Sync"]                  = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.Sync"),
        ["Editor.Settings.ToggleVisibility.MeasureLine"]           = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.MeasureLine"),
        ["Editor.Settings.ToggleVisibility.BeatLine"]              = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.BeatLine"),
        ["Editor.Settings.ToggleVisibility.LaneShow"]              = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.LaneShow"),
        ["Editor.Settings.ToggleVisibility.LaneHide"]              = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.LaneHide"),
        ["Editor.Settings.ToggleVisibility.TempoChange"]           = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.TempoChange"),
        ["Editor.Settings.ToggleVisibility.MetreChange"]           = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.MetreChange"),
        ["Editor.Settings.ToggleVisibility.SpeedChange"]           = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.SpeedChange"),
        ["Editor.Settings.ToggleVisibility.VisibilityChange"]      = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.VisibilityChange"),
        ["Editor.Settings.ToggleVisibility.ReverseEffect"]         = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.ReverseEffect"),
        ["Editor.Settings.ToggleVisibility.StopEffect"]            = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.StopEffect"),
        ["Editor.Settings.ToggleVisibility.TutorialMarker"]        = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.ToggleVisibility", "ChartEditor.General.Type.Value.TutorialMarker"),
        
        ["Editor.Settings.HideDuringPlayback.EventMarkers"]      = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.HideDuringPlayback", "ChartEditor.ChartView3D.Menu.Settings.HideDuringPlayback.EventMarkers"),
        ["Editor.Settings.HideDuringPlayback.LaneToggleNotes"]   = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.HideDuringPlayback", "ChartEditor.ChartView3D.Menu.Settings.HideDuringPlayback.LaneToggleNotes"),
        ["Editor.Settings.HideDuringPlayback.HoldControlPoints"] = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.HideDuringPlayback", "ChartEditor.ChartView3D.Menu.Settings.HideDuringPlayback.HoldControlPoints"),
        ["Editor.Settings.HideDuringPlayback.Bookmarks"]         = new(Key.None, false, false, false, "ChartEditor.ChartView3D.Menu.Settings.HideDuringPlayback", "ChartEditor.ChartView3D.Menu.Settings.HideDuringPlayback.Bookmarks"),
        
        ["Proofreader.Run"] = new(Key.P, true, false, false, "ChartEditor.Proofreader", "ChartEditor.Proofreader.ToolTip.Run"),
    };

    /// <summary>
    /// Sets a shortcut in <see cref="Shortcuts"/> with the provided key, then invokes <see cref="PropertyChanged"/> if the new shortcut is different from the previous.
    /// </summary>
    /// <param name="key">The key of the action.</param>
    /// <param name="shortcut">The shortcut to be pressed.</param>
    public void SetShortcut(string key, Shortcut shortcut)
    {
        if (Shortcuts.TryGetValue(key, out Shortcut? value) && !value.Equals(shortcut))
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

    public List<KeyValuePair<string, Shortcut>> ShortcutsFilteredByQuery(string query)
    {
        if (Application.Current == null) return [];
        if (query == "") return Shortcuts.ToList();
        
        string[] queryParts = query.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return Shortcuts.Where(x =>
        {
            foreach (string queryPart in queryParts)
            {
                bool group = Application.Current.TryGetResource(x.Value.GroupMessage, Application.Current.ActualThemeVariant, out object? groupResource)
                             && groupResource is string groupName
                             && groupName.Contains(queryPart, StringComparison.OrdinalIgnoreCase);

                bool action = Application.Current.TryGetResource(x.Value.ActionMessage, Application.Current.ActualThemeVariant, out object? actionResource)
                              && actionResource is string actionName
                              && actionName.Contains(queryPart, StringComparison.OrdinalIgnoreCase);

                bool shortcut = x.Value.ToString().Contains(queryPart, StringComparison.OrdinalIgnoreCase);

                if (group || action || shortcut)
                {
                    return true;
                }
            }

            return false;
        }).ToList();   
    }
}

[Serializable]
public class Shortcut
{
    /// <summary>
    /// The key to press.
    /// </summary>
    public Key Key { get; set; }

    /// <summary>
    /// Is the control key pressed?
    /// </summary>
    public bool Control { get; set; }
    
    /// <summary>
    /// Is the alt key pressed?
    /// </summary>
    public bool Alt { get; set; }
    
    /// <summary>
    /// Is the shift key pressed?
    /// </summary>
    public bool Shift { get; set; }

    /// <summary>
    /// The group message to display.
    /// </summary>
    public string GroupMessage;
    
    /// <summary>
    /// The action message to display.
    /// </summary>
    public string ActionMessage;

    public Shortcut(Key key, bool control, bool alt, bool shift)
    {
        Key = key;
        Control = control;
        Alt = alt;
        Shift = shift;
        GroupMessage = "";
        ActionMessage = "";
    }
    
    public Shortcut(Key key, bool control, bool alt, bool shift, string groupMessage, string actionMessage)
    {
        Key = key;
        Control = control;
        Alt = alt;
        Shift = shift;
        GroupMessage = groupMessage;
        ActionMessage = actionMessage;
    }

    public override string ToString()
    {
        if (Key == Key.None) return "";
        
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

    public KeyGesture? ToKeyGesture()
    {
        if (Key == Key.None) return null;

        KeyModifiers modifiers = KeyModifiers.None;
        if (Control) modifiers |= KeyModifiers.Control;
        if (Alt) modifiers |= KeyModifiers.Alt;
        if (Shift) modifiers |= KeyModifiers.Shift;

        return new(Key, modifiers);
    }
}