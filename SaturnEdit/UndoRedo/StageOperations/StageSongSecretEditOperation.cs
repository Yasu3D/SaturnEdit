using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.StageOperations;

public class StageSongSecretEditOperation(int songIndex, bool oldSecret, bool newSecret) : IOperation
{
    public void Revert()
    {
        if (songIndex == 0)
        {
            StageSystem.StageUpStage.Song1.Secret = oldSecret;
        }
        else if (songIndex == 1)
        {
            StageSystem.StageUpStage.Song2.Secret = oldSecret;
        }
        else if (songIndex == 2)
        {
            StageSystem.StageUpStage.Song3.Secret = oldSecret;
        }
    }

    public void Apply()
    {
        if (songIndex == 0)
        {
            StageSystem.StageUpStage.Song1.Secret = newSecret;
        }
        else if (songIndex == 1)
        {
            StageSystem.StageUpStage.Song2.Secret = newSecret;
        }
        else if (songIndex == 2)
        {
            StageSystem.StageUpStage.Song3.Secret = newSecret;
        }
    }
}