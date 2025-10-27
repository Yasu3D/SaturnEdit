using SaturnData.Notation.Events;

namespace SaturnEdit.UndoRedo.EventOperations;

public class MetreChangeEditOperation(MetreChangeEvent metreChangeEvent, int oldUpper, int newUpper, int oldLower, int newLower) : IOperation
{
    public void Revert()
    {
        metreChangeEvent.Upper = oldUpper;
        metreChangeEvent.Lower = oldLower;
    }

    public void Apply()
    {
        metreChangeEvent.Upper = newUpper;
        metreChangeEvent.Lower = newLower;
    }
}