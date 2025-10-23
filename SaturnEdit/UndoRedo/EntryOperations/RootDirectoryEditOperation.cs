using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class RootDirectoryEditOperation(string oldValue, string newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.RootDirectory = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.RootDirectory = newValue;
    }
}