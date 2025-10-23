using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class ClearThresholdEditOperation(float oldValue, float newValue) : IOperation
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