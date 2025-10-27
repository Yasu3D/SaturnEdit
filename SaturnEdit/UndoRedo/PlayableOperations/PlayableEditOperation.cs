using SaturnData.Notation.Interfaces;

namespace SaturnEdit.UndoRedo.PlayableOperations;

public class PlayableEditOperation(IPlayable playable, BonusType oldBonusType, BonusType newBonusType, JudgementType oldJudgementType, JudgementType newJudgementType) : IOperation
{
    public void Revert()
    {
        playable.BonusType = oldBonusType;
        playable.JudgementType = oldJudgementType;
    }

    public void Apply()
    {
        playable.BonusType = newBonusType;
        playable.JudgementType = newJudgementType;
    }
}