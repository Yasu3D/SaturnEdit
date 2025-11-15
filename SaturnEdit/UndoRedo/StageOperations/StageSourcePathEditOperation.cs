using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.StageOperations;

public class StageSourcePathEditOperation(string oldPath, string newPath) : IOperation
{
    public void Revert()
    {
        StageSystem.StageUpStage.AbsoluteSourcePath = oldPath;
    }

    public void Apply()
    {
        StageSystem.StageUpStage.AbsoluteSourcePath = newPath;
    }
}