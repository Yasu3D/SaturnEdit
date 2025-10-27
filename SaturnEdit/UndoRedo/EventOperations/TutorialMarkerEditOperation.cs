using SaturnData.Notation.Events;

namespace SaturnEdit.UndoRedo.EventOperations;

public class TutorialMarkerEditOperation(TutorialMarkerEvent tutorialMarkerEvent, string oldKey, string newKey) : IOperation
{
    public void Revert()
    {
        tutorialMarkerEvent.Key = oldKey;
    }

    public void Apply()
    {
        tutorialMarkerEvent.Key = newKey;
    }
}