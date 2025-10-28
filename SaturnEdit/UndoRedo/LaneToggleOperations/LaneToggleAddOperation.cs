using System.Collections.Generic;
using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.NoteOperations;

public class LaneToggleAddOperation(Note note, int index) : IOperation
{
    public void Revert()
    {
        ChartSystem.Chart.LaneToggles.Remove(note);
    }

    public void Apply()
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
}