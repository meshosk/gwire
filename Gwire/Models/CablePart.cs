using Gwire.Models.Base;

namespace Gwire.Models;

/// <summary>
/// A cable joins exactly two independently movable connection points.
/// </summary>
public sealed class CablePart : BaseCircuitPart
{
    public string Color { get; set; } = "#000000";

    /// <summary>
    /// Ordered bend points in scheme coordinates, between the two electrical endpoints.
    /// </summary>
    public List<CableBendPoint> BendPoints { get; set; } = [];

    /// <summary>
    /// Finds the insertion index and projected position on the nearest cable segment.
    /// Returns null when the position is outside the cable or overlaps a point handle.
    /// </summary>
    public (int Index, Point Position)? FindBendPointInsertion(Point position)
    {
        var route = BendPoints.Select(bend => bend.Position)
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
        foreach (var segment in GetRouteSegments())
        {
            var deltaX = segment.End.X - segment.Start.X;
            var deltaY = segment.End.Y - segment.Start.Y;
            var lengthSquared = deltaX * deltaX + deltaY * deltaY;
            if (lengthSquared == 0)
            {
                continue;
            }

            var fraction = Math.Clamp(((position.X - segment.Start.X) * deltaX +
                (position.Y - segment.Start.Y) * deltaY) / lengthSquared, 0f, 1f);
            var projected = new Point(segment.Start.X + deltaX * fraction, segment.Start.Y + deltaY * fraction);
            var delta = new Point(position.X - projected.X, position.Y - projected.Y);
            var distanceSquared = delta.X * delta.X + delta.Y * delta.Y;
            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                nearestSegment = segment.InsertionIndex;
                nearestPosition = projected;
            }
        }

        return nearestSegment >= 0 && closestDistanceSquared <= 3f * 3f
            ? (nearestSegment, nearestPosition)
            : null;
    }

    /// <summary>
    /// Builds the SVG route shared by the cable and electrical path overlays.
    /// </summary>
    public string GetPathData(bool reverse = false)
    {
        var path = new System.Text.StringBuilder();
        var start = Points[reverse ? 1 : 0].LocalPosition;
        path.Append(FormattableString.Invariant($"M {start.X},{start.Y}"));
        var corners = GetCorners();
        if (reverse)
        {
            corners = corners.Reverse();
        }
        foreach (var corner in corners)
        {
            var entry = reverse ? corner.Exit : corner.Entry;
            var exit = reverse ? corner.Entry : corner.Exit;
            path.Append(FormattableString.Invariant($" L {entry.X},{entry.Y}"));
            if (corner.IsRounded)
            {
                path.Append(FormattableString.Invariant($" Q {corner.Position.X},{corner.Position.Y} {exit.X},{exit.Y}"));
            }
        }
        var end = Points[reverse ? 0 : 1].LocalPosition;
        path.Append(FormattableString.Invariant($" L {end.X},{end.Y}"));
        return path.ToString();
    }

    private IEnumerable<(Point Position, Point Entry, Point Exit, bool IsRounded)> GetCorners()
    {
        for (var index = 0; index < BendPoints.Count; index++)
        {
            var bend = BendPoints[index];
            var position = bend.Position;
            var previous = index == 0 ? Points[0].LocalPosition : BendPoints[index - 1].Position;
            var next = index == BendPoints.Count - 1 ? Points[1].LocalPosition : BendPoints[index + 1].Position;
            if (!bend.IsRounded)
            {
                yield return (position, position, position, false);
                continue;
            }

            var entry = new Point((previous.X + position.X) / 2f, (previous.Y + position.Y) / 2f);
            var exit = new Point((position.X + next.X) / 2f, (position.Y + next.Y) / 2f);
            yield return (position, entry, exit, true);
        }
    }

    private IEnumerable<(Point Start, Point End, int InsertionIndex)> GetRouteSegments()
    {
        var previous = Points[0].LocalPosition;
        var index = 0;
        foreach (var corner in GetCorners())
        {
            yield return (previous, corner.Entry, index);
            previous = corner.Entry;
            if (corner.IsRounded)
            {
                // Approximate the SVG quadratic only for pointer hit testing.
                for (var step = 1; step <= 12; step++)
                {
                    var t = step / 12f;
                    var u = 1f - t;
                    var point = new Point(
                        u * u * corner.Entry.X + 2 * u * t * corner.Position.X + t * t * corner.Exit.X,
                        u * u * corner.Entry.Y + 2 * u * t * corner.Position.Y + t * t * corner.Exit.Y);
                    yield return (previous, point, step <= 6 ? index : index + 1);
                    previous = point;
                }
            }
            index++;
        }
        yield return (previous, Points[1].LocalPosition, index);
    }

    public override CablePart Clone()
    {
        var clone = (CablePart)base.Clone();
        clone.BendPoints = BendPoints.Select(point => point.Copy()).ToList();
        return clone;
    }

    public override IEnumerable<ConnectionPoint> GetConnectedPoints(ConnectionPoint startingPoint) =>
        Points.Contains(startingPoint)
            ? Points.Where(point => !ReferenceEquals(point, startingPoint))
            : Array.Empty<ConnectionPoint>();

    public CablePart()
    {
        Points.Add(new ConnectionPoint(this));
        Points.Add(new ConnectionPoint(this));
    }
}
