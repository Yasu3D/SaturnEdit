using System;
using System.Collections.Generic;
using System.IO;
using SaturnData.Content.Items;
using SaturnData.Content.Items.Attributes;
using SaturnEdit.Systems;

namespace SaturnEdit.Settings;

public class EditorSettings : ContentItem
{
    public new event EventHandler? PropertyChanged;

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

    [SctLineBreak]
    [SctKey("LOCALE", 10)]
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

    [SctKey("THEME", 10)]
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

    [SctKey("SHOW_SPLASH", 10)]
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

    [SctKey("CONTINUE_SESSION", 10)]
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

    [SctKey("CHECK_UPDATES", 10)]
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

    [SctKey("LAST_SESSION", 10)]
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

    [SctKey("TXT_SHOW_SPACES", 10)]
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

    [SctKey("TXT_SHOW_SYNTAX", 10)]
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

    [SctKey("AUTO_SAVE", 10)]
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

    [SctKey("CLICK_DRAG", 10)]
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

    [SctLineBreak]
    [SctKey("RECENT_FILES", 10)]
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