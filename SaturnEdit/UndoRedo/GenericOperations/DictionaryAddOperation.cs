using System;
using System.Collections.Generic;

namespace SaturnEdit.UndoRedo.GenericOperations;

public class DictionaryAddOperation<T1, T2>(Func<Dictionary<T1, T2>> dictionary, T1 key, T2 value) : IOperation where T1 : notnull
{
    public void Revert()
    {
        dictionary.Invoke().Remove(key);
    }

    public void Apply()
    {
        dictionary.Invoke()[key] = value;
    }
}