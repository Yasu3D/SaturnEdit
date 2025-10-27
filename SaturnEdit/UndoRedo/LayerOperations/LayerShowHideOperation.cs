using SaturnData.Notation.Core;

namespace SaturnEdit.UndoRedo.LayerOperations;

public class LayerShowHideOperation(Layer layer, bool oldVisibility, bool newVisibility) : IOperation
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