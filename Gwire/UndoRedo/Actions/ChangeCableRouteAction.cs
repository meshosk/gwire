using Gwire.Models;

namespace Gwire.UndoRedo.Actions;

/// <summary>
/// Restores a cable's bend points, including their corner styles.
/// </summary>
public sealed class ChangeCableRouteAction : IUndoableAction
{
    private readonly CablePart cable;
    private readonly CableBendPoint[] before;
    private readonly CableBendPoint[] after;

    public ChangeCableRouteAction(CablePart cable, IEnumerable<CableBendPoint> before, IEnumerable<CableBendPoint> after)
    {
        this.cable = cable;
        this.before = before.ToArray();
        this.after = after.ToArray();
    }

    public bool HasChanges => !before.SequenceEqual(after);

    public void Undo() => Restore(before);
    public void Redo() => Restore(after);

    private void Restore(CableBendPoint[] points)
    {
        cable.BendPoints = points.Select(point => point.Copy()).ToList();
        cable.NotifyChanged();
    }
}
