using System;
using System.IO;
using SaturnData.Content.Cosmetics;
using SaturnData.Content.Items;
using SaturnData.Content.Items.Attributes;
using SaturnData.Content.Lists;
using SaturnData.Content.Serialization;
using SaturnEdit.Utilities;

namespace SaturnEdit.Systems;

public static class NoteSoundSystem
{
    public static void Initialize()
    {
        NoteSounds.Load(NoteSoundsDirectory);
        
        foreach (EditorNoteSound noteSound in NoteSounds.Items)
        {
            noteSound.PropertyChanged += NoteSound_OnPropertyChanged;
        }
    }

    /// <summary>
    /// Invoked whenever a property of any loaded <see cref="NoteSound"/> changes.
    /// </summary>
    public static event EventHandler? NoteSoundPropertyChanged;

    /// <summary>
    /// The collection of loaded <see cref="EditorNoteSound">EditorNoteSounds</see>.
    /// </summary>
    public static ContentList<EditorNoteSound> NoteSounds { get; set; } = new();
    
    /// <summary>
    /// The currently selected <see cref="EditorNoteSound">EditorNoteSounds</see>.
    /// </summary>
    public static EditorNoteSound SelectedNoteSound => NoteSounds.TryGetItem(SettingsSystem.AudioSettings.NoteSoundId) ?? DefaultNoteSound;
    
    /// <summary>
    /// The default <see cref="EditorNoteSound"/>, using the built-in sounds.
    /// </summary>
    public static readonly EditorNoteSound DefaultNoteSound = new()
    {
        Name = "Default",
        Id = "default",
        HitsoundGuidePath      = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "Assets/Sounds/guide.wav"),
        HitsoundTouchPath      = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "Assets/Sounds/touch.wav"),
        HitsoundHoldPath       = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "Assets/Sounds/hold.wav"),
        HitsoundSlidePath      = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "Assets/Sounds/slide.wav"),
        HitsoundBonusPath      = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "Assets/Sounds/bonus.wav"),
        HitsoundRPath          = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "Assets/Sounds/r.wav"),
        HitsoundStartClickPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "Assets/Sounds/click.wav"),
        HitsoundMetronomePath  = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "Assets/Sounds/metronome.wav"),
        HoldLoopStart = 3612,
        HoldLoopEnd = 5893,
    };
    
    private static string NoteSoundsDirectory => Path.Combine(PersistentDataPathHelper.PersistentDataPath, "NoteSounds");

#region Methods
    public static void AddNoteSound(EditorNoteSound pack)
    {
        NoteSounds.Items.Add(pack);
        pack.PropertyChanged += NoteSound_OnPropertyChanged;
        
        WriteNoteSounds();
    }

    public static void RemoveNoteSound(EditorNoteSound pack)
    {
        NoteSounds.Items.Remove(pack);
        pack.PropertyChanged -= NoteSound_OnPropertyChanged;
        
        WriteNoteSounds();
    }

    private static void WriteNoteSounds()
    {
        try
        {
            Directory.CreateDirectory(NoteSoundsDirectory);
            DirectoryInfo directoryInfo = new(NoteSoundsDirectory);

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                if (file.Extension != ".sct") continue;
                
                file.Delete();
            }
        }
        catch (Exception ex)
        {
            // Don't throw.
            LoggingSystem.WriteSessionLog(ex.ToString());
        }
        
        for (int i = 0; i < NoteSounds.Items.Count; i++)
        {
            EditorNoteSound noteSound = NoteSounds.Items[i];

            try
            {
                string filteredName = string.Join("_", noteSound.Name.Split(Path.GetInvalidFileNameChars()));
                string filename = filteredName == ""
                    ? $"{i}.sct"
                    : $"{i}_{filteredName}.sct";

                string path = Path.Combine(NoteSoundsDirectory, filename);
                string data = ContentSerializer.ToString(noteSound);

                File.WriteAllText(path, data);
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
    private static void NoteSound_OnPropertyChanged(object? sender, EventArgs e)
    {
        NoteSoundPropertyChanged?.Invoke(null, EventArgs.Empty);
        WriteNoteSounds();
    }
#endregion Internal Event Handlers
}

public class EditorNoteSound : ContentItem
{
    public new event EventHandler? PropertyChanged;
    
    [SctLineBreak]
    [SctKey("GUIDE", 9)]
    public string HitsoundGuidePath
    {
        get => hitsoundGuidePath;
        set
        {
            if (hitsoundGuidePath == value) return;
            
            hitsoundGuidePath = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string hitsoundGuidePath = "";
    
    [SctKey("TOUCH", 9)]
    public string HitsoundTouchPath
    {
        get => hitsoundTouchPath;
        set
        {
            if (hitsoundTouchPath == value) return;
            
            hitsoundTouchPath = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string hitsoundTouchPath = "";
    
    [SctKey("HOLD", 9)]
    public string HitsoundHoldPath
    {
        get => hitsoundHoldPath;
        set
        {
            if (hitsoundHoldPath == value) return;
            
            hitsoundHoldPath = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string hitsoundHoldPath = "";
    
    [SctKey("SLIDE", 9)]
    public string HitsoundSlidePath
    {
        get => hitsoundSlidePath;
        set
        {
            if (hitsoundSlidePath == value) return;
            
            hitsoundSlidePath = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string hitsoundSlidePath = "";
    
    [SctLineBreak]
    [SctKey("BONUS", 9)]
    public string HitsoundBonusPath
    {
        get => hitsoundBonusPath;
        set
        {
            if (hitsoundBonusPath == value) return;
            
            hitsoundBonusPath = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string hitsoundBonusPath = "";
    
    [SctKey("R", 9)]
    public string HitsoundRPath
    {
        get => hitsoundRPath;
        set
        {
            if (hitsoundRPath == value) return;
            
            hitsoundRPath = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string hitsoundRPath = "";
    
    [SctLineBreak]
    [SctKey("CLICK", 9)]
    public string HitsoundStartClickPath
    {
        get => hitsoundStartClickPath;
        set
        {
            if (hitsoundStartClickPath == value) return;
            
            hitsoundStartClickPath = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string hitsoundStartClickPath = "";
    
    [SctKey("METRONOME", 9)]
    public string HitsoundMetronomePath
    {
        get => hitsoundMetronomePath;
        set
        {
            if (hitsoundMetronomePath == value) return;
            
            hitsoundMetronomePath = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string hitsoundMetronomePath = "";
    
    [SctLineBreak]
    [SctKey("HOLD_LOOP_START", 15)]
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
    private float holdLoopStart = 1000.0f;
    
    [SctKey("HOLD_LOOP_END", 15)]
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
    private float holdLoopEnd = 3000.0f;
}