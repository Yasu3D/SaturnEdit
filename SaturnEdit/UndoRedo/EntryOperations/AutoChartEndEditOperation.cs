using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EntryOperations;

public class AutoChartEndEditOperation(bool oldValue, bool newValue) : IOperation
{
    public void Revert()
    {
        ChartSystem.Entry.AutoChartEnd = oldValue;
    }

    public void Apply()
    {
        ChartSystem.Entry.AutoChartEnd = newValue;
    }
}