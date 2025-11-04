using SaturnData.Notation.Core;

namespace SaturnEdit.UndoRedo.NoteOperations;

public class NoteAddOperation(Layer layer, Note note, int index) : IOperation
{
    public void Revert()
    {
        layer.Notes.Remove(note);
    }

    public void Apply()
    {
        if (index < 0 || index >= layer.Notes.Count)
        {
            layer.Notes.Add(note);
        }
        else
        {
            layer.Notes.Insert(index, note);
        }
    }
}