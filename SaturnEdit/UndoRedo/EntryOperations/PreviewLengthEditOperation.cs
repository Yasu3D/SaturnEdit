using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class PreviewLengthEditOperation(float oldValue, float newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.PreviewLength = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.PreviewLength = newValue;
    }
}