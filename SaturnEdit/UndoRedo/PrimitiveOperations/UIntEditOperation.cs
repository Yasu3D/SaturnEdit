using System;

namespace SaturnEdit.UndoRedo.PrimitiveOperations;

public class UIntEditOperation(Action<uint>? action, uint oldValue, uint newValue) : IOperation
{
    public void Revert()
    {
        action?.Invoke(oldValue);
    }

    public void Apply()
    {
        action?.Invoke(newValue);
    }
}