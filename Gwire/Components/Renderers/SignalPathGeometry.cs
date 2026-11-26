using Gwire.Models;
using Gwire.Models.Base;
using System.Text;

namespace Gwire.Components.Renderers;

/// <summary>
/// Places neighboring signal bands along the cable's straight and rounded segments.
/// </summary>
internal static class SignalPathGeometry
{
    /// <summary>
    /// Start and end select a part of a straight connection (0 to 1); cable routes are always whole.
    /// </summary>
    public static string GetPath(ConnectionPoint from, ConnectionPoint to, float offset, float start = 0, float end = 1)
    {
        List<(Point Position, bool IsRounded)> route;
        if (from.Owner is CablePart cable && ReferenceEquals(to.Owner, cable))
        {
            route = [(cable.Points[0].LocalPosition, false)];
            route.AddRange(cable.BendPoints.Select(bend => (bend.Position, bend.IsRounded)));
            route.Add((cable.Points[1].LocalPosition, false));
            if (!ReferenceEquals(from, cable.Points[0]))
            {
                route.Reverse();
            }
        }
        else
        {
            route = [(GetPosition(from), false), (GetPosition(to), false)];
        }

        // Coincident attachment points need no SVG segment or lane offset.
        for (var index = route.Count - 1; index > 0; index--)
        {
            if (route[index].Position.Equals(route[index - 1].Position))
            {
                route.RemoveAt(index);
            }
        }
        if (route.Count < 2)
        {
            return string.Empty;
        }

        var points = route.Select((vertex, index) => Offset(route, index, offset)).ToArray();
        if (points.Length == 2)
        {
            var (first, last) = (points[0], points[1]);
            points = [Interpolate(first, last, start), Interpolate(first, last, end)];
        }
        var path = new StringBuilder(FormattableString.Invariant($"M {points[0].X},{points[0].Y}"));
        for (var index = 1; index < points.Length - 1; index++)
        {
            var corner = points[index];
            if (route[index].IsRounded)
            {
                var entry = new Point((points[index - 1].X + corner.X) / 2, (points[index - 1].Y + corner.Y) / 2);
                var exit = new Point((corner.X + points[index + 1].X) / 2, (corner.Y + points[index + 1].Y) / 2);
                path.Append(FormattableString.Invariant($" L {entry.X},{entry.Y} Q {corner.X},{corner.Y} {exit.X},{exit.Y}"));
            }
            else
            {
                path.Append(FormattableString.Invariant($" L {corner.X},{corner.Y}"));
            }
        }
        path.Append(FormattableString.Invariant($" L {points[^1].X},{points[^1].Y}"));
        return path.ToString();
    }

    private static Point GetPosition(ConnectionPoint point) => point.Owner is CircuitPart part
        ? new Point(part.SchemePosition.X + point.LocalPosition.X, part.SchemePosition.Y + point.LocalPosition.Y)
        : point.LocalPosition;

    private static Point Offset(IReadOnlyList<(Point Position, bool IsRounded)> route, int index, float offset)
    {
        var position = route[index].Position;
        var previous = GetNormal(route[Math.Max(0, index - 1)].Position, position);
        var next = GetNormal(position, route[Math.Min(route.Count - 1, index + 1)].Position);
        if (index == 0)
        {
            previous = next;
        }
        if (index == route.Count - 1)
        {
            next = previous;
        }

        // Limit the miter at sharp turns so neighboring bands do not spike away from the cable.
        var divisor = Math.Max(0.5f, 1 + previous.X * next.X + previous.Y * next.Y);
        return new Point(position.X + (previous.X + next.X) * offset / divisor,
            position.Y + (previous.Y + next.Y) * offset / divisor);
    }

    private static Point Interpolate(Point from, Point to, float position) =>
        new(from.X + (to.X - from.X) * position, from.Y + (to.Y - from.Y) * position);

    private static Point GetNormal(Point from, Point to)
    {
        var x = to.X - from.X;
        var y = to.Y - from.Y;
        var length = MathF.Sqrt(x * x + y * y);
        return length == 0 ? new Point() : new Point(-y / length, x / length);
    }
}
