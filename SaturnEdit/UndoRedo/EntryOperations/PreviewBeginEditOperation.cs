using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class PreviewBeginEditOperation(float oldValue, float newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.PreviewBegin = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.PreviewBegin = newValue;
    }
}