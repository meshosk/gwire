using Gwire.Models;
using Gwire.Models.Base;

namespace Gwire.UndoRedo.Actions;

public sealed class ChangePartStateAction(CircuitPart part, PartStateGroup group, PartState after) : IUndoableAction
{
    private readonly PartState? before = group.ActiveState;

    public void Undo() => Restore(before);
    public void Redo() => Restore(after);

    private void Restore(PartState? state)
    {
        group.ActiveState = state;
        part.NotifyChanged();
    }
}
