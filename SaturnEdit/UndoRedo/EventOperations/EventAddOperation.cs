using SaturnData.Notation.Core;

namespace SaturnEdit.UndoRedo.EventOperations;

public class EventAddOperation(Layer layer, Event @event, int index) : IOperation
{
    public void Revert()
    {
        layer.Events.Remove(@event);
    }

    public void Apply()
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
}