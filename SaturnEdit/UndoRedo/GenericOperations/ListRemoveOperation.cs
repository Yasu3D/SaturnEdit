using System;
using System.Collections.Generic;

namespace SaturnEdit.UndoRedo.GenericOperations;

public class ListRemoveOperation<T>(Func<List<T>>? list, T item) : IOperation
{
    public void Revert()
    {
        list?.Invoke().Insert(0, item);
    }

    public void Apply()
    {
        list?.Invoke().Remove(item);
    }
}
