using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class EntryChartEndEditOperation(Timestamp oldValue, Timestamp newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.ChartEnd = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.ChartEnd = newValue;
    }
}