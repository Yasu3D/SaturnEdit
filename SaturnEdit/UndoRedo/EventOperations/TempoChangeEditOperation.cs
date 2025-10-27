using SaturnData.Notation.Events;

namespace SaturnEdit.UndoRedo.EventOperations;

public class TempoChangeEditOperation(TempoChangeEvent tempoChangeEvent, float oldTempo, float newTempo) : IOperation
{
    public void Revert()
    {
        tempoChangeEvent.Tempo = oldTempo;
    }

    public void Apply()
    {
        tempoChangeEvent.Tempo = newTempo;
    }
}