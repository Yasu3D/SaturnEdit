using SaturnData.Notation.Interfaces;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EditModeOperations;

public class EditModeChangeOperation(EditorEditMode oldEditMode, EditorEditMode newEditMode, ITimeable? oldSubObjectGroup, ITimeable? newSubObjectGroup) : IOperation
{
    public void Revert()
    {
        EditorSystem.EditMode = oldEditMode;
        EditorSystem.ActiveObjectGroup = oldSubObjectGroup;
    }

    public void Apply()
    {
        EditorSystem.EditMode = newEditMode;
        EditorSystem.ActiveObjectGroup = newSubObjectGroup;
    }
}