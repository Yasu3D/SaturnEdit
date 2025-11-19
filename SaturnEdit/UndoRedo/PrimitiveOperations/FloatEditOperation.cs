using System;

namespace SaturnEdit.UndoRedo.PrimitiveOperations;

public class FloatEditOperation(Action<float>? action, float oldValue, float newValue) : IOperation
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