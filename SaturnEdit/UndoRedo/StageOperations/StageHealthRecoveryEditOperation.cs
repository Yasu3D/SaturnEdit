using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.StageOperations;

public class StageHealthRecoveryEditOperation(int oldHealthRecovery, int newHealthRecovery) : IOperation
{
    public void Revert()
    {
        StageSystem.StageUpStage.HealthRecovery = oldHealthRecovery;
    }

    public void Apply()
    {
        StageSystem.StageUpStage.HealthRecovery = newHealthRecovery;
    }
}