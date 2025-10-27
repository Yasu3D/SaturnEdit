using SaturnData.Notation.Interfaces;

namespace SaturnEdit.UndoRedo.PositionableOperations;

public class PositionableEditOperation(IPositionable positionable, int oldPosition, int newPosition, int oldSize, int newSize) : IOperation
{
    public void Revert()
    {
        positionable.Position = oldPosition;
        positionable.Size = oldSize;
    }

    public void Apply()
    {
        positionable.Position = newPosition;
        positionable.Size = newSize;
    }
}