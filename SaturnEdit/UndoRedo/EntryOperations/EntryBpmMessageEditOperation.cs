using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class EntryBpmMessageEditOperation(string oldValue, string newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.BpmMessage = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.BpmMessage = newValue;
    }
}