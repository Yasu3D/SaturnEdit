using System;

namespace SaturnEdit.UndoRedo.StageOperations;

public class StageIconPathEditOperation(string oldPath, string newPath) : IOperation
{
    public void Revert()
    {
        throw new NotImplementedException();
    }

    public void Apply()
    {
        throw new NotImplementedException();
    }
}