using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class EntryJacketEditOperation(string oldValue, string newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.JacketFile = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.JacketFile = newValue;
    }
}