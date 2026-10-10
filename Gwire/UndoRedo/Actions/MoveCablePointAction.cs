using Gwire.Models;
using Gwire.Models.Base;

namespace Gwire.UndoRedo.Actions;

public sealed class MoveCablePointAction : IUndoableAction
{
    private readonly ConnectionPoint point;
    private readonly Point before;
    private readonly Point after;
    private readonly ConnectionPoint[] beforeConnections;
    private readonly ConnectionPoint[] afterConnections;

    public MoveCablePointAction(ConnectionPoint point, Point before, IEnumerable<ConnectionPoint> beforeConnections)
    {
        this.point = point;
        this.before = before.Copy();
        after = point.LocalPosition.Copy();
        this.beforeConnections = beforeConnections.ToArray();
        afterConnections = point.ConnectionPoints.ToArray();
    }

    public bool HasChanges => !before.Equals(after) ||
        beforeConnections.Length != afterConnections.Length ||
        beforeConnections.Any(connection => !afterConnections.Contains(connection));

    public void Undo() => Restore(before, beforeConnections);
    public void Redo() => Restore(after, afterConnections);

    private void Restore(Point position, ConnectionPoint[] connections)
    {
        foreach (var connection in point.ConnectionPoints.ToArray())
        {
            point.Disconnect(connection);
        }

        point.LocalPosition = position.Copy();
        foreach (var connection in connections)
        {
            point.Connect(connection);
        }
        point.Owner.NotifyChanged();
    }
}
