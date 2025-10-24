using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.LayerOperations;

public class LayerSwapOperation(Layer layerA, Layer layerB, int indexA, int indexB) : IOperation
{
    public void Revert()
    {
        ChartSystem.Chart.Layers[indexA] = layerA;
        ChartSystem.Chart.Layers[indexB] = layerB;
    }

    public void Apply()
    {
        ChartSystem.Chart.Layers[indexA] = layerB;
        ChartSystem.Chart.Layers[indexB] = layerA;
    }
}