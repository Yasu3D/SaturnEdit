using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.CosmeticOperations;

public class CosmeticItemIdEditOperation(string oldId, string newId) : IOperation
{
    public void Revert()
    {
        CosmeticSystem.CosmeticItem.Id = oldId;
    }

    public void Apply()
    {
        CosmeticSystem.CosmeticItem.Id = newId;
    }
}