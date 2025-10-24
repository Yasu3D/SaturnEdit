using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.LayerOperations;

public class LayerDeleteOperation(Layer layer, int index) : IOperation
{
    public void Revert()
    {
        ChartSystem.Chart.Layers.Insert(index, layer);
    }

    public void Apply()
    {
        ChartSystem.Chart.Layers.Remove(layer);
    }
}