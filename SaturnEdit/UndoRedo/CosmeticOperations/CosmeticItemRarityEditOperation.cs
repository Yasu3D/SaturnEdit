using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo.CosmeticOperations;

public class CosmeticItemRarityEditOperation(int oldRarity, int newRarity) : IOperation
{
    public void Revert()
    {
        CosmeticSystem.CosmeticItem.Rarity = oldRarity;
    }

    public void Apply()
    {
        CosmeticSystem.CosmeticItem.Rarity = newRarity;
    }
}