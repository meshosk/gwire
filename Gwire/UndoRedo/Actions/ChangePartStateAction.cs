using Gwire.Models;
using Gwire.Models.Base;

namespace Gwire.UndoRedo.Actions;

public sealed class ChangePartStateAction(CircuitPart part, PartState after) : IUndoableAction
{
    private readonly PartState? before = part.ActiveState;

    public void Undo() => Restore(before);
    public void Redo() => Restore(after);

    private void Restore(PartState? state)
    {
        part.ActiveState = state;
        part.NotifyChanged();
    }
}
