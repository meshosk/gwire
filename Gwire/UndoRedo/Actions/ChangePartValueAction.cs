using Gwire.Models;

namespace Gwire.UndoRedo.Actions;

public sealed class ChangePartValueAction(CircuitPart part, string after) : IUndoableAction
{
    private readonly string? before = part.CustomTextValue;

    public void Undo() => Restore(before);
    public void Redo() => Restore(after);

    private void Restore(string? value)
    {
        part.CustomTextValue = value;
        part.NotifyChanged();
    }
}
