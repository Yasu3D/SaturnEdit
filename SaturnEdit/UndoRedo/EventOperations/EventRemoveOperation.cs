using SaturnData.Notation.Core;

namespace SaturnEdit.UndoRedo.EventOperations;

public class EventRemoveOperation(Layer layer, Event @event, int index) : IOperation
{
    public void Revert()
    {
        layer.Events.Insert(index, @event);
    }

    public void Apply()
    {
        layer.Events.Remove(@event);
    }
}