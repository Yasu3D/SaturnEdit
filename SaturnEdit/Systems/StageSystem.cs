using System;
using System.IO;
using SaturnData.Content.Lists;
using SaturnData.Content.StageUp;
using Tomlyn;

namespace SaturnEdit.Systems;

public static class StageSystem
{
    public static event EventHandler? StageLoaded;
    
    /// <summary>
    /// The stage being edited/displayed.
    /// </summary>
    public static StageUpStage StageUpStage { get; private set; } = new() { Id = Guid.NewGuid().ToString() };
    
    /// <summary>
    /// Determines if the editor will prompt the user to save when a stage is closed.
    /// </summary>
    public static bool IsSaved { get; private set; } = true;

    /// <summary>
    /// A collection of entries for displaying song data in a stage up stage.
    /// </summary>
    public static MusicList MusicData { get; private set; } = new();

#region Methods
    /// <summary>
    /// Creates a new stage to work on by resetting the <see cref="StageUpStage"/>, then invokes <see cref="StageLoaded"/>.
    /// </summary>
    public static void NewStage()
    {
        StageUpStage = new() { Id = Guid.NewGuid().ToString() };
        
        StageLoaded?.Invoke(null, EventArgs.Empty);
        
        IsSaved = true;
    }

    /// <summary>
    /// Creates a new stage to work on by reading data from a file, then invokes <see cref="StageLoaded"/>.
    /// </summary>
    /// <param name="path">Path to the file to read from.</param>
    public static void ReadStage(string path)
    {
        try
        {
            string data = File.ReadAllText(path);
            
            StageUpStage = Toml.ToModel<StageUpStage>(data);
            StageUpStage.AbsoluteSourcePath = path;
            
            StageLoaded?.Invoke(null, EventArgs.Empty);
            
            IsSaved = true;
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
    }
    
    /// <summary>
    /// Writes a stage to a file.
    /// </summary>
    /// <param name="path">Path to the file to write to.</param>
    /// <param name="markAsSaved">Should the stage be marked as saved?</param>
    /// <param name="updatePath">Should the <see cref="StageUpStage.AbsoluteSourcePath"/> get updated?</param>
    public static bool WriteStage(string path, bool markAsSaved, bool updatePath)
    {
        try
        {
            string data = Toml.FromModel(StageUpStage);
            File.WriteAllText(path, data);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
            return false;
        }
        
        if (updatePath)
        {
            StageUpStage.AbsoluteSourcePath = path;
        }
        
        IsSaved = markAsSaved || IsSaved;
        return true;
    }
#endregion Methods
}