using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class EntryAutoReadingEditOperation(bool oldValue, bool newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.AutoReading = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.AutoReading = newValue;
    }
}