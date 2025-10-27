using SaturnData.Notation.Events;

namespace SaturnEdit.UndoRedo.EventOperations;

public class VisibilityChangeEditOperation(VisibilityChangeEvent visibilityChangeEvent, bool oldVisibility, bool newVisibility) : IOperation
{
    public void Revert()
    {
        visibilityChangeEvent.Visibility = oldVisibility;
    }

    public void Apply()
    {
        visibilityChangeEvent.Visibility = newVisibility;
    }
}