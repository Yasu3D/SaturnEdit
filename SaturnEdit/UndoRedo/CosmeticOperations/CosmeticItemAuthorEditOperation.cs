using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.CosmeticOperations;

public class CosmeticItemAuthorEditOperation(string oldAuthor, string newAuthor) : IOperation
{
    public void Revert()
    {
        CosmeticSystem.CosmeticItem.Author = oldAuthor;
    }

    public void Apply()
    {
        CosmeticSystem.CosmeticItem.Author = newAuthor;
    }
}