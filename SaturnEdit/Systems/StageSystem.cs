using System;
using System.IO;
using SaturnData.Content.Items;
using SaturnData.Content.Lists;
using SaturnData.Content.Serialization;
using SaturnData.Content.StageUp;

namespace SaturnEdit.Systems;

public static class StageSystem
{
    public static void Initialize()
    {
        UndoRedoSystem.StageBranch.OperationHistoryChanged += StageBranch_OnOperationHistoryChanged;
    }
    
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
            ContentItem? tempContentItem = ContentSerializer.ToContentItem(path);
            if (tempContentItem is not StageUpStage s) return;
            
            StageUpStage = s;
            StageUpStage.AbsoluteSourcePath = path;
            
            StageLoaded?.Invoke(null, EventArgs.Empty);
            
            IsSaved = true;
        }
        catch (Exception ex)
        {
            // Don't throw.
            LoggingSystem.WriteSessionLog(ex.ToString());
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
            string data = ContentSerializer.ToString(StageUpStage);
            File.WriteAllText(path, data);
        }
        catch (Exception ex)
        {
            // Don't throw.
            LoggingSystem.WriteSessionLog(ex.ToString());
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

#region System Event Handlers
    private static void StageBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        IsSaved = false;
    }
#endregion System Event Handlers
}