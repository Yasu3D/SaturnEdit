using SaturnData.Notation.Core;

namespace SaturnEdit.UndoRedo.LayerOperations;

public class LayerVisibilityEditOperation(Layer layer, bool oldVisibility, bool newVisibility) : IOperation
{
    public void Revert()
    {
        layer.Visible = oldVisibility;
    }

    public void Apply()
    {
        layer.Visible = newVisibility;
    }
}