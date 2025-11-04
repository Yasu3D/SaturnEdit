using System.Collections.Generic;

namespace SaturnEdit.UndoRedo;

public class CompositeOperation(List<IOperation> operations) : IOperation
{
    public readonly List<IOperation> Operations = operations;
    
    public void Revert()
    {
        for (int i = Operations.Count - 1; i >= 0; i--)
        {
            IOperation operation = Operations[i];
            operation.Revert();
        }
    }

    public void Apply()
    {
        for (int i = 0; i < Operations.Count; i++)
        {
            IOperation operation = Operations[i];
            operation.Apply();
        }
    }
}