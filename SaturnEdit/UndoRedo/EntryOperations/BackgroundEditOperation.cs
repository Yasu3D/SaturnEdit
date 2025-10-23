using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class BackgroundEditOperation(BackgroundOption oldValue, BackgroundOption newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.Background = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.Background = newValue;
    }
}