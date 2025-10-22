namespace SaturnEdit.UndoRedo;

public interface IOperation
{
    /// <summary>
    /// Reverts an operation
    /// </summary>
    public void Revert();

    /// <summary>
    /// Applies an operation
    /// </summary>
    public void Apply();
}