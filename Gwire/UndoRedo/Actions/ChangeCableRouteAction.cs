using Gwire.Models;

namespace Gwire.UndoRedo.Actions;

/// <summary>
/// Restores a cable's bend points when a point is added or moved.
/// </summary>
public sealed class ChangeCableRouteAction : IUndoableAction
{
    private readonly CablePart cable;
    private readonly Point[] before;
    private readonly Point[] after;

    public ChangeCableRouteAction(CablePart cable, IEnumerable<Point> before, IEnumerable<Point> after)
    {
        this.cable = cable;
        this.before = before.ToArray();
        this.after = after.ToArray();
    }

    public bool HasChanges => !before.SequenceEqual(after);

    public void Undo() => Restore(before);
    public void Redo() => Restore(after);

    private void Restore(Point[] points)
    {
        cable.BendPoints = points.Select(point => point.Copy()).ToList();
        cable.NotifyChanged();
    }
}
