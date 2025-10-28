using SaturnData.Notation.Core;

namespace SaturnEdit.UndoRedo.EventOperations;

public class EventRemoveOperation(Layer layer, Event @event, int index) : IOperation
{
    public void Revert()
    {
        if (index < 0 || index >= layer.Events.Count)
        {
            layer.Events.Add(@event);
        }
        else
        {
            layer.Events.Insert(index, @event);
        }
    }

    public void Apply()
    {
        layer.Events.Remove(@event);
    }
}