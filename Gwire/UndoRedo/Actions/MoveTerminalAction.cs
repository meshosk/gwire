using Gwire.Models;
using Gwire.Models.Base;

namespace Gwire.UndoRedo.Actions;

public sealed class MoveTerminalAction(SourceGroundPair pair, CircuitPart terminal,
    IReadOnlyList<SourceGroundPair> pairs, Point before, Point after,
    ConnectionPoint? beforeAttachment, ConnectionPoint? afterAttachment) : IUndoableAction
{
    private readonly MovePartAction movement = new(terminal, before, after);

    public void Undo()
    {
        pair.AttachTo(terminal, null, pairs);
        movement.Undo();
        pair.AttachTo(terminal, beforeAttachment, pairs);
    }

    public void Redo()
    {
        pair.AttachTo(terminal, null, pairs);
        movement.Redo();
        pair.AttachTo(terminal, afterAttachment, pairs);
    }
}
