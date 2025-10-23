using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class RevisionEditOperation(string oldValue, string newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.Revision = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.Revision = newValue;
    }
}