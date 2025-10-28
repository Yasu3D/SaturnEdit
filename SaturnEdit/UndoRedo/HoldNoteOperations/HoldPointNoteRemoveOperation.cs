using SaturnData.Notation.Notes;

namespace SaturnEdit.UndoRedo.HoldNoteOperations;

public class HoldPointNoteRemoveOperation(HoldNote holdNote, HoldPointNote holdPointNote, int index) : IOperation
{
    public void Revert()
    {
        holdNote.Points.Remove(holdPointNote);
    }

    public void Apply()
    {
        if (index < 0 || index >= holdNote.Points.Count)
        {
            holdNote.Points.Add(holdPointNote);
        }
        else
        {
            holdNote.Points.Insert(index, holdPointNote);
        }
    }
}