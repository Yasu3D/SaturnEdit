using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class EntryAutoBpmMessageEditOperation(bool oldValue, bool newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.AutoBpmMessage = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.AutoBpmMessage = newValue;
    }
}