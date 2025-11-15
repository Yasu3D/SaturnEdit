using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.StageOperations;

public class StageSongEntryIdEditOperation(int songIndex, string oldEntryId, string newEntryId) : IOperation
{
    public void Revert()
    {
        if (songIndex == 0)
        {
            StageSystem.StageUpStage.Song1.EntryId = oldEntryId;
        }
        else if (songIndex == 1)
        {
            StageSystem.StageUpStage.Song2.EntryId = oldEntryId;
        }
        else if (songIndex == 2)
        {
            StageSystem.StageUpStage.Song3.EntryId = oldEntryId;
        }
    }

    public void Apply()
    {
        if (songIndex == 0)
        {
            StageSystem.StageUpStage.Song1.EntryId = newEntryId;
        }
        else if (songIndex == 1)
        {
            StageSystem.StageUpStage.Song2.EntryId = newEntryId;
        }
        else if (songIndex == 2)
        {
            StageSystem.StageUpStage.Song3.EntryId = newEntryId;
        }
    }
}