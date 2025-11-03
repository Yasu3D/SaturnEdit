using SaturnData.Notation.Interfaces;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EditModeOperations;

public class EditModeChangeOperation(EditorMode oldMode, EditorMode newMode, ITimeable? oldSubObjectGroup, ITimeable? newSubObjectGroup) : IOperation
{
    public void Revert()
    {
        EditorSystem.Mode = oldMode;
        EditorSystem.ActiveObjectGroup = oldSubObjectGroup;
    }

    public void Apply()
    {
        EditorSystem.Mode = newMode;
        EditorSystem.ActiveObjectGroup = newSubObjectGroup;
    }
}