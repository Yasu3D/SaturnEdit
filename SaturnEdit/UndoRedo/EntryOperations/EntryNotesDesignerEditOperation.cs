using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class EntryNotesDesignerEditOperation(string oldValue, string newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.NotesDesigner = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.NotesDesigner = newValue;
    }
}