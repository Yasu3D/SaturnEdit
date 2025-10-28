using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.LayerOperations;

public class LayerRemoveOperation(Layer layer, int index) : IOperation
{
    public void Revert()
    {
        if (index < 0 || index >= ChartSystem.Chart.Bookmarks.Count)
        {
            ChartSystem.Chart.Layers.Add(layer);
        }
        else
        {
            ChartSystem.Chart.Layers.Insert(index, layer);
        }
    }

    public void Apply()
    {
        ChartSystem.Chart.Layers.Remove(layer);
    }
}