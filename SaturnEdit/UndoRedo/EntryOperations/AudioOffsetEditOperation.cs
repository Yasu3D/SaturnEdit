using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class AudioOffsetEditOperation(float oldValue, float newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.AudioOffset = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.AudioOffset = newValue;
    }
}