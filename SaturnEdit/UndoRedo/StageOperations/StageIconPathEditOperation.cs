using System;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.StageOperations;

public class StageIconPathEditOperation(string oldIconPath, string newIconPath) : IOperation
{
    public void Revert()
    {
        StageSystem.StageUpStage.IconPath = oldIconPath;
    }

    public void Apply()
    {
        StageSystem.StageUpStage.IconPath = newIconPath;
    }
}