using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class EntryDifficultyEditOperation(Difficulty oldValue, Difficulty newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.Difficulty = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.Difficulty = newValue;
    }
}