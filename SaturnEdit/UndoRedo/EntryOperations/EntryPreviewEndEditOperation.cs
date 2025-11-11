using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class EntryPreviewEndEditOperation(Timestamp oldValue, Timestamp newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.PreviewEnd = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.PreviewEnd = newValue;
    }
}