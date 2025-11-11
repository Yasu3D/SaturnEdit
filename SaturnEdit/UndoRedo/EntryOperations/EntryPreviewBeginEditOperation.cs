using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class EntryPreviewBeginEditOperation(Timestamp oldValue, Timestamp newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.PreviewBegin = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.PreviewBegin = newValue;
    }
}