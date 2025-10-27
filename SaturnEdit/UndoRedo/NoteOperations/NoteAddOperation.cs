using System.Collections.Generic;
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
        layer.Notes.Insert(index, note);
    }
}