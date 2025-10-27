using SaturnData.Notation.Interfaces;

namespace SaturnEdit.UndoRedo.NoteOperations;

public class LaneToggleEditOperation(ILaneToggle laneToggle, LaneSweepDirection oldDirection, LaneSweepDirection newDirection) : IOperation
{
    public void Revert()
    {
        laneToggle.Direction = oldDirection;
    }

    public void Apply()
    {
        laneToggle.Direction = newDirection;
    }
}