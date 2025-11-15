using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.StageOperations;

public class StageHealthEditOperation(int oldHealth, int newHealth) : IOperation
{
    public void Revert()
    {
        StageSystem.StageUpStage.Health = oldHealth;
    }

    public void Apply()
    {
        StageSystem.StageUpStage.Health = newHealth;
    }
}