using SaturnData.Notation.Notes;

namespace SaturnEdit.UndoRedo.HoldNoteOperations;

public class HoldPointNoteAddOperation(HoldNote holdNote, HoldPointNote holdPointNote, int index) : IOperation
{
    public void Revert()
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

    public void Apply()
    {
        holdNote.Points.Remove(holdPointNote);
    }
}