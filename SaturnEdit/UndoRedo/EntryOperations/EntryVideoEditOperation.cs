using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class EntryVideoEditOperation(string oldValue, string newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.VideoFile = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.VideoFile = newValue;
    }
}