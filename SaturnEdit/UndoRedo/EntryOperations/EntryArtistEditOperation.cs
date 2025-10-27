using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class EntryArtistEditOperation(string oldValue, string newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.Artist = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.Artist = newValue;
    }
}