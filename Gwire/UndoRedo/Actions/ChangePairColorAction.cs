using Gwire.Models;

namespace Gwire.UndoRedo.Actions;

public sealed class ChangePairColorAction(SourceGroundPair pair, string before, string after) : IUndoableAction
{
    public void Undo() => pair.LineColor = before;
    public void Redo() => pair.LineColor = after;
}
