using Gwire.Models;

namespace Gwire.UndoRedo.Actions;

public sealed class MovePartAction(CircuitPart part, Point before, Point after) : IUndoableAction
{
    public void Undo() => part.MoveTo(before);
    public void Redo() => part.MoveTo(after);
}
