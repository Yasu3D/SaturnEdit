using SaturnData.Notation.Interfaces;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.Operations;

public class RemoveSelectionOperation(ITimeable obj, ITimeable? lastSelectedObject) : IOperation
{
    public void Revert()
    {
        SelectionSystem.SelectedObjects.Add(obj);
        SelectionSystem.LastSelectedObject = lastSelectedObject;
    }

    public void Apply()
    {
        SelectionSystem.SelectedObjects.Remove(obj);
        SelectionSystem.LastSelectedObject = null;
    }
}