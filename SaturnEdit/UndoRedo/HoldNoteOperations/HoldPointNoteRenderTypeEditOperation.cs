using SaturnData.Notation.Notes;

namespace SaturnEdit.UndoRedo.HoldNoteOperations;

public class HoldPointNoteRenderTypeEditOperation(HoldPointNote point, HoldPointRenderType oldRenderType, HoldPointRenderType newRenderType) : IOperation
{
    public void Revert()
    {
        point.RenderType = oldRenderType;
    }

    public void Apply()
    {
        point.RenderType = newRenderType;
    }
}