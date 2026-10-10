namespace Gwire.UndoRedo;

public sealed class UndoRedoHistory
{
    private readonly Stack<IUndoableAction> undoActions = new();
    private readonly Stack<IUndoableAction> redoActions = new();
    private int freezeCount;

    public event Action? Changed;

    public bool IsFrozen => freezeCount > 0;
    public bool CanUndo => !IsFrozen && undoActions.Count > 0;
    public bool CanRedo => !IsFrozen && redoActions.Count > 0;

    public void Execute(IUndoableAction action)
    {
        if (IsFrozen)
        {
            return;
        }

        action.Redo();
        Record(action);
    }

    /// <summary>
    /// Records an edit that has already been applied, such as a completed drag.
    /// </summary>
    public void Record(IUndoableAction action)
    {
        undoActions.Push(action);
        redoActions.Clear();
        Changed?.Invoke();
    }

    public void Undo()
    {
        if (!CanUndo)
        {
            return;
        }

        var action = undoActions.Peek();
        action.Undo();
        undoActions.Pop();
        redoActions.Push(action);
        Changed?.Invoke();
    }

    public void Redo()
    {
        if (!CanRedo)
        {
            return;
        }

        var action = redoActions.Peek();
        action.Redo();
        redoActions.Pop();
        undoActions.Push(action);
        Changed?.Invoke();
    }

    /// <summary>
    /// Temporarily blocks Execute, Undo, and Redo while an edit, such as a drag, is in progress.
    /// Record remains available so the completed edit can be saved before unfreezing.
    /// </summary>
    /// <remarks>Every Freeze call must be balanced by an Unfreeze call, including when an edit is canceled.</remarks>
    public void Freeze()
    {
        freezeCount++;
        Changed?.Invoke();
    }

    /// <summary>
    /// Releases one freeze after an edit is completed or canceled.
    /// Execute, Undo, and Redo become available again when all freezes have been released.
    /// </summary>
    /// <remarks>Calling Unfreeze when the history is already unfrozen has no effect.</remarks>
    public void Unfreeze()
    {
        if (freezeCount > 0)
        {
            freezeCount--;
            Changed?.Invoke();
        }
    }
}
