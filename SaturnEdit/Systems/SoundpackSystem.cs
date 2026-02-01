using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaturnEdit.Utilities;
using Tomlyn;

namespace SaturnEdit.Systems;

public static class SoundpackSystem
{
    public static void Initialize()
    {
        Directory.CreateDirectory(SoundpacksDirectory);
        
        string[] files = Directory.EnumerateFiles(SoundpacksDirectory, "*", SearchOption.TopDirectoryOnly).ToArray();
        
        foreach (string file in files)
        {
            try
            {
                string data = File.ReadAllText(file);
                
                Soundpack pack = Toml.ToModel<Soundpack>(data);
                pack.PropertyChanged += Soundpack_OnPropertyChanged;

                Soundpacks.Add(pack);
            }
            catch (Exception ex)
            {
                // Don't throw.
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
        }
    }
    
    public static event EventHandler? SoundpackPropertyChanged;
    
    private static string SoundpacksDirectory => Path.Combine(PersistentDataPathHelper.PersistentDataPath, "Soundpacks");
    
    public static List<Soundpack> Soundpacks { get; set; } = [];
    
    public static Soundpack SelectedSoundpack
    {
        get
        {
            if (SettingsSystem.AudioSettings.SelectedSoundpackIndex < 0) return DefaultSoundpack;
            if (SettingsSystem.AudioSettings.SelectedSoundpackIndex >= Soundpacks.Count) return DefaultSoundpack;

            return Soundpacks[SettingsSystem.AudioSettings.SelectedSoundpackIndex];
        }
    }
    
    public static readonly Soundpack DefaultSoundpack = new()
    {
        Name = "Default",
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
    
#region Methods
    public static void AddSoundpack(Soundpack pack)
    {
        Soundpacks.Add(pack);
        pack.PropertyChanged += Soundpack_OnPropertyChanged;
        
        WriteSoundpacks();
    }

    public static void RemoveSoundpack(Soundpack pack)
    {
        Soundpacks.Remove(pack);
        pack.PropertyChanged -= Soundpack_OnPropertyChanged;
        
        WriteSoundpacks();
    }

    private static void WriteSoundpacks()
    {
        try
        {
            Directory.CreateDirectory(SoundpacksDirectory);
            DirectoryInfo directoryInfo = new(SoundpacksDirectory);

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
            {
                dir.Delete(true);
            }
        }
        catch (Exception ex)
        {
            // Don't throw.
            LoggingSystem.WriteSessionLog(ex.ToString());
        }
        
        for (int i = 0; i < Soundpacks.Count; i++)
        {
            Soundpack pack = Soundpacks[i];

            try
            {
                string filteredName = string.Join("_", pack.Name.Split(Path.GetInvalidFileNameChars()));
                string filename = filteredName == ""
                    ? $"{i}.toml"
                    : $"{i}_{filteredName}.toml";

                string path = Path.Combine(SoundpacksDirectory, filename);
                string data = Toml.FromModel(pack);

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
    private static void Soundpack_OnPropertyChanged(object? sender, EventArgs e)
    {
        SoundpackPropertyChanged?.Invoke(null, EventArgs.Empty);
        WriteSoundpacks();
    }
#endregion Internal Event Handlers
}

[Serializable]
public class Soundpack
{
    public event EventHandler? PropertyChanged;
    
    public string Name
    {
        get => name;
        set
        {
            if (name == value) return;

            name = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private string name = "Note Sounds";
    
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