using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class EntryTitleEditOperation(string oldValue, string newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.Title = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.Title = newValue;
    }
}