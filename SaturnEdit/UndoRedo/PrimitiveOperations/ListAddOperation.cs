using System;
using System.Collections.Generic;

namespace SaturnEdit.UndoRedo.PrimitiveOperations;

public class ListAddOperation<T>(Func<List<T>>? list, T item) : IOperation
{
    public void Revert()
    {
        list?.Invoke().Remove(item);
    }

    public void Apply()
    {
        list?.Invoke().Add(item);
    }
}
