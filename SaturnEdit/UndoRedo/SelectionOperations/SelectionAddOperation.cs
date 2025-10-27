using SaturnData.Notation.Interfaces;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.SelectionOperations;

public class SelectionAddOperation(ITimeable obj, ITimeable? lastSelectedObject) : IOperation
{
    public void Revert()
    {
        SelectionSystem.SelectedObjects.Remove(obj);
        SelectionSystem.LastSelectedObject = lastSelectedObject;
    }

    public void Apply()
    {
        SelectionSystem.SelectedObjects.Add(obj);
        SelectionSystem.LastSelectedObject = obj;
    }
}