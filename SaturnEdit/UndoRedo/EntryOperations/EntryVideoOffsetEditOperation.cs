using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class EntryVideoOffsetEditOperation(float oldValue, float newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.VideoOffset = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.VideoOffset = newValue;
    }
}