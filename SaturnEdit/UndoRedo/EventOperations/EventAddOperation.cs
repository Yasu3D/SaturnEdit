using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EventOperations;

public class EventAddOperation(Layer layer, Event @event, int index) : IOperation
{
    public void Revert()
    {
        layer.Events.Remove(@event);
    }

    public void Apply()
    {
        layer.Events.Insert(index, @event);
    }
}