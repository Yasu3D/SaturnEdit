using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.StageOperations;

public class StageNameEditOperation(string oldName, string newName) : IOperation
{
    public void Revert()
    {
        StageSystem.StageUpStage.Name = oldName;
    }

    public void Apply()
    {
        StageSystem.StageUpStage.Name = newName;
    }
}