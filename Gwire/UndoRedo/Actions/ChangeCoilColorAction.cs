using Gwire.Models;

namespace Gwire.UndoRedo.Actions;

public sealed class ChangeCoilColorAction(CircuitPart part, CoilPair coil, string before, string after) : IUndoableAction
{
    public void Undo() => Restore(before);

    public void Redo() => Restore(after);

    private void Restore(string color)
    {
        coil.Color = color;
        part.NotifyChanged();
    }
}
