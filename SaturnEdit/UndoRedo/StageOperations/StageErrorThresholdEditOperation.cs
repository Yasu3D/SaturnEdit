using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.StageOperations;

public class StageErrorThresholdEditOperation(JudgementGrade oldErrorThreshold, JudgementGrade newErrorThreshold) : IOperation
{
    public void Revert()
    {
        StageSystem.StageUpStage.ErrorThreshold = oldErrorThreshold;
    }

    public void Apply()
    {
        StageSystem.StageUpStage.ErrorThreshold = newErrorThreshold;
    }
}