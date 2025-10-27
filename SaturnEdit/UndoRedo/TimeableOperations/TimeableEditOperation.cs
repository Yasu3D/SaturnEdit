using SaturnData.Notation.Interfaces;

namespace SaturnEdit.UndoRedo.TimeableOperations;

public class TimeableEditOperation(ITimeable timeable, int oldFullTick, int newFullTick) : IOperation
{
    public void Revert()
    {
        timeable.Timestamp.FullTick = oldFullTick;
    }

    public void Apply()
    {
        timeable.Timestamp.FullTick = newFullTick;
    }
}