using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class AutoBpmMessageEditOperation(bool oldValue, bool newValue) : IOperation
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