using Gwire.Models;

namespace Gwire.UndoRedo.Actions;

public sealed class AddSourceGroundPairAction(List<SourceGroundPair> pairs, SourceGroundPair pair) : IUndoableAction
{
    private readonly int index = pairs.Count;

    public void Undo() => pairs.Remove(pair);
    public void Redo() => pairs.Insert(Math.Min(index, pairs.Count), pair);
}
