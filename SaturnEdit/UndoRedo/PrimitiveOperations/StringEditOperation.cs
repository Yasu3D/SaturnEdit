using System;

namespace SaturnEdit.UndoRedo.PrimitiveOperations;

public class StringEditOperation(Action<string>? action, string oldValue, string newValue) : IOperation
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