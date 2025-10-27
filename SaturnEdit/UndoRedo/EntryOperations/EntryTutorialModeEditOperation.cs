using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class EntryTutorialModeEditOperation(bool oldValue, bool newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.TutorialMode = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.TutorialMode = newValue;
    }
}