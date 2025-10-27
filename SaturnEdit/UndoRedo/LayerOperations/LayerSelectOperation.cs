using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.LayerOperations;

public class LayerSelectOperation(Layer? oldLayer, Layer? newLayer) : IOperation
{
    public void Revert()
    {
        SelectionSystem.SelectedLayer = oldLayer;
    }

    public void Apply()
    {
        SelectionSystem.SelectedLayer = newLayer;
    }
}