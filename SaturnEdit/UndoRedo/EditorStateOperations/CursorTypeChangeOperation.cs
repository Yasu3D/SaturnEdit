using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.EditModeOperations;

public class CursorTypeChangeOperation(Note oldType, Note newType) : IOperation
{
    public void Revert()
    {
        CursorSystem.CurrentType = oldType;
    }

    public void Apply()
    {
        CursorSystem.CurrentType = newType;
    }
}