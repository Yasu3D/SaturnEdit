using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.CosmeticOperations;

public class CosmeticItemNameEditOperation(string oldName, string newName) : IOperation
{
    public void Revert()
    {
        CosmeticSystem.CosmeticItem.Name = oldName;
    }

    public void Apply()
    {
        CosmeticSystem.CosmeticItem.Name = newName;
    }
}