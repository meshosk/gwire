using Gwire.UndoRedo;

namespace Gwire.Services;

public sealed partial class UndoRedoService
{
    private sealed class HistoryState
    {
        public readonly Stack<IUndoableAction> UndoActions = new();
        public readonly Stack<IUndoableAction> RedoActions = new();
        public int FreezeCount;
        public UndoRedoHistoryWrapper? Wrapper;
    }

    private readonly Dictionary<string, HistoryState> histories = new(StringComparer.Ordinal);

    private event Action<string>? Changed;

    public UndoRedoHistoryWrapper getHistory(string historyId)
    {
        var state = GetState(historyId);
        return state.Wrapper ??= new UndoRedoHistoryWrapper(this, historyId);
    }

    private bool HasEntries(string historyId)
    {
        var state = GetState(historyId);
        return state.UndoActions.Count > 0 || state.RedoActions.Count > 0;
    }

    private bool IsFrozen(string historyId) => GetState(historyId).FreezeCount > 0;

    private bool CanUndo(string historyId)
    {
        var state = GetState(historyId);
        return state.FreezeCount == 0 && state.UndoActions.Count > 0;
    }

    private bool CanRedo(string historyId)
    {
        var state = GetState(historyId);
        return state.FreezeCount == 0 && state.RedoActions.Count > 0;
    }

    /// <summary>
    /// Adds an already completed modification without applying it again.
    /// </summary>
    private void addRedoAction(string historyId, IUndoableAction action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var state = GetState(historyId);
        state.UndoActions.Push(action);
        state.RedoActions.Clear();
        Changed?.Invoke(historyId);
    }

    private void Undo(string historyId)
    {
        var state = GetState(historyId);
        if (state.FreezeCount > 0 || state.UndoActions.Count == 0)
        {
            return;
        }

        var action = state.UndoActions.Peek();
        action.Undo();
        state.UndoActions.Pop();
        state.RedoActions.Push(action);
        Changed?.Invoke(historyId);
    }

    private void Redo(string historyId)
    {
        var state = GetState(historyId);
        if (state.FreezeCount > 0 || state.RedoActions.Count == 0)
        {
            return;
        }

        var action = state.RedoActions.Peek();
        action.Redo();
        state.RedoActions.Pop();
        state.UndoActions.Push(action);
        Changed?.Invoke(historyId);
    }

    /// <summary>
    /// Clears actions and frozen state for this document without removing subscriptions.
    /// </summary>
    private void Clear(string historyId)
    {
        var state = GetState(historyId);
        state.UndoActions.Clear();
        state.RedoActions.Clear();
        state.FreezeCount = 0;
        Changed?.Invoke(historyId);
    }

    /// <summary>
    /// Blocks Undo and Redo during an edit. Completed changes can still be added.
    /// Each Freeze call must be balanced by an Unfreeze call.
    /// </summary>
    private void Freeze(string historyId)
    {
        GetState(historyId).FreezeCount++;
        Changed?.Invoke(historyId);
    }

    private void Unfreeze(string historyId)
    {
        var state = GetState(historyId);
        if (state.FreezeCount > 0)
        {
            state.FreezeCount--;
            Changed?.Invoke(historyId);
        }
    }

    private HistoryState GetState(string historyId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(historyId);
        if (!histories.TryGetValue(historyId, out var state))
        {
            state = new HistoryState();
            histories.Add(historyId, state);
        }

        return state;
    }
}
