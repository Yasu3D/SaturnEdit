using SaturnData.Notation.Core;

namespace SaturnEdit.UndoRedo.LayerOperations;

public class LayerRenameOperation(Layer layer, string oldName, string newName) : IOperation
{
    public void Revert()
    {
        layer.Name = oldName;
    }

    public void Apply()
    {
        layer.Name = newName;
    }
}