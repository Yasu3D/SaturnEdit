using SaturnData.Notation.Events;

namespace SaturnEdit.UndoRedo.EventOperations;

public class SpeedChangeEditOperation(SpeedChangeEvent speedChangeEvent, float oldSpeed, float newSpeed) : IOperation
{
    public void Revert()
    {
        speedChangeEvent.Speed = oldSpeed;
    }

    public void Apply()
    {
        speedChangeEvent.Speed = newSpeed;
    }
}