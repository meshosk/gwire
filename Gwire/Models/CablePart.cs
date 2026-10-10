using Gwire.Models.Base;

namespace Gwire.Models;

/// <summary>
/// A cable joins exactly two independently movable connection points.
/// </summary>
public sealed class CablePart : BaseCircuitPart
{
    /// <summary>
    /// Ordered bend points in scheme coordinates, between the two electrical endpoints.
    /// </summary>
    public List<Point> BendPoints { get; set; } = [];

    /// <summary>
    /// Finds the insertion index and projected position on the nearest cable segment.
    /// Returns null when the position is outside the cable or overlaps a point handle.
    /// </summary>
    public (int Index, Point Position)? FindBendPointInsertion(Point position)
    {
        var route = BendPoints
            .Prepend(Points[0].LocalPosition)
            .Append(Points[1].LocalPosition)
            .ToArray();
        for (var index = 0; index < route.Length; index++)
        {
            var delta = new Point(position.X - route[index].X, position.Y - route[index].Y);
            var radius = index == 0 || index == route.Length - 1 ? 11.5f : 7f;
            if (delta.X * delta.X + delta.Y * delta.Y <= radius * radius)
            {
                return null;
            }
        }

        var nearestSegment = -1;
        var nearestPosition = new Point();
        var closestDistanceSquared = float.MaxValue;
        for (var index = 0; index < route.Length - 1; index++)
        {
            var start = route[index];
            var segment = new Point(route[index + 1].X - start.X, route[index + 1].Y - start.Y);
            var lengthSquared = segment.X * segment.X + segment.Y * segment.Y;
            if (lengthSquared == 0)
            {
                continue;
            }

            var fraction = Math.Clamp(((position.X - start.X) * segment.X +
                (position.Y - start.Y) * segment.Y) / lengthSquared, 0f, 1f);
            var projected = new Point(start.X + segment.X * fraction, start.Y + segment.Y * fraction);
            var delta = new Point(position.X - projected.X, position.Y - projected.Y);
            var distanceSquared = delta.X * delta.X + delta.Y * delta.Y;
            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                nearestSegment = index;
                nearestPosition = projected;
            }
        }

        return nearestSegment >= 0 && closestDistanceSquared <= 3f * 3f
            ? (nearestSegment, nearestPosition)
            : null;
    }

    public override CablePart Clone()
    {
        var clone = (CablePart)base.Clone();
        clone.BendPoints = BendPoints.Select(point => point.Copy()).ToList();
        return clone;
    }

    public CablePart()
    {
        // cable has only one permanently connected state
        Points.Add(new ConnectionPoint(this));
        Points.Add(new ConnectionPoint(this));

        this.States.Add(new PartState() {
                ConnectionGroups =
                [
                    new ConnectionGroup()
                    {
                        ConnectedPints = [Points[0], Points[1]]
                    }
                ]
            }
        );
        this.ActiveState = this.States[0];
    }
}
