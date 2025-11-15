using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.StageOperations;

public class StageIdEditOperation(string oldId, string newId) : IOperation
{
    public void Revert()
    {
        StageSystem.StageUpStage.Id = oldId;
    }

    public void Apply()
    {
        StageSystem.StageUpStage.Id = newId;
    }
}