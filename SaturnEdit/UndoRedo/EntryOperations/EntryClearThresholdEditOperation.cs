using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class EntryClearThresholdEditOperation(float oldValue, float newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.ClearThreshold = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.ClearThreshold = newValue;
    }
}