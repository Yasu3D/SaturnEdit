using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class AudioEditOperation(string oldValue, string newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.AudioFile = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.AudioFile = newValue;
    }
}