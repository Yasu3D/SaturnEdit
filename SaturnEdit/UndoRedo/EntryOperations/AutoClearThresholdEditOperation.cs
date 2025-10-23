using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class AutoClearThresholdEditOperation(bool oldValue, bool newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.AutoClearThreshold = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.AutoClearThreshold = newValue;
    }
}