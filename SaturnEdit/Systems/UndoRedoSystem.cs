using System;
using System.Collections.Generic;
using SaturnEdit.UndoRedo;

namespace SaturnEdit.Systems;

public static class UndoRedoSystem
{
    public static void Initialize()
    {
        ChartSystem.ChartLoaded += OnChartLoaded;
    }

    public static event EventHandler? OperationHistoryChanged;

    public static bool CanUndo => UndoStack.Count > 0;
    public static bool CanRedo => RedoStack.Count > 0;
    
    private static readonly Stack<IOperation> UndoStack = new();
    private static readonly Stack<IOperation> RedoStack = new();
    
    /// <summary>
    /// Applies an <see cref="IOperation"/> and pushes to the UndoRedo stack.
    /// </summary>
    public static void Push(IOperation operation)
    {
        operation.Apply();
        UndoStack.Push(operation);
        RedoStack.Clear();
        OperationHistoryChanged?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Applies an <see cref="IOperation"/> and silently appends it to the last operation on the UndoRedo stack.
    /// </summary>
    public static void Append(IOperation operation)
    {
        if (UndoStack.Count == 0)
        {
            Push(operation);
            return;
        }

        IOperation sourceOperation = UndoStack.Pop();
        sourceOperation.Revert();

        if (sourceOperation is CompositeOperation sourceCompositeOperation)
        {
            sourceCompositeOperation.Operations.Add(operation);
            sourceCompositeOperation.Apply();
            
            UndoStack.Push(sourceCompositeOperation);
        }
        else
        {
            CompositeOperation compositeOperation = new([sourceOperation, operation]);
            compositeOperation.Apply();

            UndoStack.Push(compositeOperation);
        }
    }
    
    public static IOperation? Undo()
    {
        if (!CanUndo) return null;
        IOperation operation = UndoStack.Pop();
        
        operation.Revert();
        RedoStack.Push(operation);
        OperationHistoryChanged?.Invoke(null, EventArgs.Empty);

        return operation;
    }

    public static IOperation? Redo()
    {
        if (!CanRedo) return null;
        IOperation operation = RedoStack.Pop();
        
        operation.Apply();
        UndoStack.Push(operation);
        OperationHistoryChanged?.Invoke(null, EventArgs.Empty);

        return operation;
    }

#region System Event Delegates
    private static void OnChartLoaded(object? sender, EventArgs e)
    {
        UndoStack.Clear();
        RedoStack.Clear();
        OperationHistoryChanged?.Invoke(null, EventArgs.Empty);
    }
#endregion System Event Delegates
}