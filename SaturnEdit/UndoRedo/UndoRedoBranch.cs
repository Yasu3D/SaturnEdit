using System;
using System.Collections.Generic;
using SaturnEdit.UndoRedo;

public class UndoRedoBranch
{
    public event EventHandler? OperationHistoryChanged;

    public bool CanUndo => undoStack.Count > 0;
    public bool CanRedo => redoStack.Count > 0;
    
    private readonly Stack<IOperation> undoStack = new();
    private readonly Stack<IOperation> redoStack = new();
    
    /// <summary>
    /// Applies an <see cref="IOperation"/> and pushes to the UndoRedo stack.
    /// </summary>
    public void Push(IOperation operation)
    {
        if (operation is CompositeOperation { Operations.Count: 0 }) return;
        
        operation.Apply();
        undoStack.Push(operation);
        redoStack.Clear();
        OperationHistoryChanged?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Clears the <see cref="undoStack"/> and <see cref="redoStack"/>, then invokes <see cref="OperationHistoryChanged"/>.
    /// </summary>
    public void Clear()
    {
        undoStack.Clear();
        redoStack.Clear();
        OperationHistoryChanged?.Invoke(null, EventArgs.Empty);
    }
    
    public IOperation? Undo()
    {
        if (!CanUndo) return null;
        IOperation operation = undoStack.Pop();
        
        operation.Revert();
        redoStack.Push(operation);
        OperationHistoryChanged?.Invoke(null, EventArgs.Empty);

        return operation;
    }

    public IOperation? Redo()
    {
        if (!CanRedo) return null;
        IOperation operation = redoStack.Pop();
        
        operation.Apply();
        undoStack.Push(operation);
        OperationHistoryChanged?.Invoke(null, EventArgs.Empty);

        return operation;
    }
}