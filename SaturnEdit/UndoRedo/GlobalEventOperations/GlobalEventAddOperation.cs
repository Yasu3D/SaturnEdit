using System.Collections.Generic;
using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EventOperations;

public class GlobalEventAddOperation(Event @event, int index) : IOperation
{
    public void Revert()
    {
        ChartSystem.Chart.Events.Remove(@event);
    }

    public void Apply()
    {
        ChartSystem.Chart.Events.Insert(index, @event);
    }
}