using System.Collections.Generic;
using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EventOperations;

public class GlobalEventRemoveOperation(Event @event, int index) : IOperation
{
    public void Revert()
    {
        if (index < 0 || index >= ChartSystem.Chart.Events.Count)
        {
            ChartSystem.Chart.Events.Add(@event);
        }
        else
        {
            ChartSystem.Chart.Events.Insert(index, @event);
        }
    }

    public void Apply()
    {
        ChartSystem.Chart.Events.Remove(@event);
    }
}