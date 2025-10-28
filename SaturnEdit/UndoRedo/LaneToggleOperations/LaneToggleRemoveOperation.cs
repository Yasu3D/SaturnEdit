using System.Collections.Generic;
using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.NoteOperations;

public class LaneToggleRemoveOperation(Note note, int index) : IOperation
{
    public void Revert()
    {
        if (index < 0 || index >= ChartSystem.Chart.LaneToggles.Count)
        {
            ChartSystem.Chart.LaneToggles.Add(note);
        }
        else
        {
            ChartSystem.Chart.LaneToggles.Insert(index, note);
        }
    }

    public void Apply()
    {
        ChartSystem.Chart.LaneToggles.Remove(note);
    }
}