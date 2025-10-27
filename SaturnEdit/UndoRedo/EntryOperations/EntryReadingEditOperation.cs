using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class EntryReadingEditOperation(string oldValue, string newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.Reading = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.Reading = newValue;
    }
}