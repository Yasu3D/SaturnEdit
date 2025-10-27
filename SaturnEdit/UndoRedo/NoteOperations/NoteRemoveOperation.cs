using System.Collections.Generic;
using SaturnData.Notation.Core;

namespace SaturnEdit.UndoRedo.NoteOperations;

public class NoteRemoveOperation(Layer layer, Note note, int index) : IOperation
{
    public void Revert()
    {
        layer.Notes.Insert(index, note);
    }

    public void Apply()
    {
        layer.Notes.Remove(note);
    }
}