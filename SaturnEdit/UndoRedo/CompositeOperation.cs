using System.Collections.Generic;
using System.Linq;

namespace SaturnEdit.UndoRedo;

public class CompositeOperation(IEnumerable<IOperation> operations) : IOperation
{
    public void Revert()
    {
        foreach (IOperation operation in operations.Reverse())
        {
            operation.Revert();
        }
    }

    public void Apply()
    {
        foreach (IOperation operation in operations)
        {
            operation.Apply();
        }
    }
}