using Gwire.Models;

namespace Gwire.UndoRedo.Actions;

public sealed class ChangeCableColorAction(CablePart cable, string before, string after) : IUndoableAction
{
    public void Undo() => Restore(before);
    public void Redo() => Restore(after);

    private void Restore(string color)
    {
        cable.Color = color;
        cable.NotifyChanged();
    }
}
