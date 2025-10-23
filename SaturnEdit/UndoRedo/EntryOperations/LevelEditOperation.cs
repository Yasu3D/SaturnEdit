using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class LevelEditOperation(double oldValue, double newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.Level = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.Level = newValue;
    }
}