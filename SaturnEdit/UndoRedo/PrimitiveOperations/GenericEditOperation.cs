using System;

namespace SaturnEdit.UndoRedo.PrimitiveOperations;

public class GenericEditOperation<T>(Action<T> action, T oldValue, T newValue) : IOperation
{
    public void Revert()
    {
        action.Invoke(oldValue);
    }

    public void Apply()
    {
        action.Invoke(newValue);
    }
}