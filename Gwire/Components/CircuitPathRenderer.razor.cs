using Gwire.Models;
using Gwire.Models.Base;
using Microsoft.AspNetCore.Components;

namespace Gwire.Components;

public partial class CircuitPathRenderer : IDisposable
{
    private readonly HashSet<BaseCircuitPart> observedParts = [];

    [Parameter, EditorRequired]
    public Circuit Circuit { get; set; } = default!;

    [Parameter, EditorRequired]
    public IReadOnlyList<SourceGroundPair> SourceGroundPairs { get; set; } = default!;

    private ConnectionPoint? GetCircuitPoint(SourceGroundPair pair, CircuitPart terminal)
    {
        var point = pair.GetAttachment(terminal);
        while (point is not null && !Circuit.Parts.Contains(point.Owner))
        {
            var ownerPair = SourceGroundPairs.FirstOrDefault(candidate => ReferenceEquals(candidate.PowerSource, point.Owner) ||
                ReferenceEquals(candidate.Ground, point.Owner));
            point = ownerPair?.GetAttachment((CircuitPart)point.Owner);
        }
        return point;
    }

    private IEnumerable<string> GetPathPoints(SourceGroundPair pair)
    {
        var source = GetCircuitPoint(pair, pair.PowerSource);
        if (source is null)
        {
            yield break;
        }

        var visited = new HashSet<ConnectionPoint> { source };
        var connections = new HashSet<(ConnectionPoint, ConnectionPoint)>();
        var pending = new Queue<ConnectionPoint>();
        pending.Enqueue(source);

        while (pending.TryDequeue(out var point))
        {
            foreach (var connected in point.GetConnectedPoints(point).Concat(point.Owner.GetConnectedPoints(point)))
            {
                if (visited.Add(connected))
                {
                    pending.Enqueue(connected);
                }

                // Render every reachable connection, including loops, once in either direction.
                if (connections.Contains((connected, point)) || !connections.Add((point, connected)))
                {
                    continue;
                }

                yield return GetPolylinePoints([point, connected]);
            }
        }
    }

    private IReadOnlyList<string> GetShortCircuitPathPoints(SourceGroundPair pair)
    {
        var source = GetCircuitPoint(pair, pair.PowerSource);
        var ground = GetCircuitPoint(pair, pair.Ground);
        if (source is null || ground is null)
        {
            return [];
        }
        if (ReferenceEquals(source, ground))
        {
            return [GetPolylinePoints([source])];
        }

        var discovery = new Dictionary<ConnectionPoint, int>();
        var lowest = new Dictionary<ConnectionPoint, int>();
        var reachesGround = new HashSet<ConnectionPoint>();
        var edges = new Stack<(ConnectionPoint From, ConnectionPoint To)>();
        var pending = new Stack<(ConnectionPoint Point, ConnectionPoint? Parent, ConnectionPoint[] Neighbors, int NextIndex)>();
        var paths = new List<string>();

        void AddPoint(ConnectionPoint point, ConnectionPoint? parent)
        {
            discovery[point] = discovery.Count;
            lowest[point] = discovery[point];
            if (ReferenceEquals(point, ground))
            {
                reachesGround.Add(point);
            }
            var neighbors = point.GetConnectedPoints(point)
                .Concat(point.Owner.GetConnectedPoints(point)).Distinct().ToArray();
            pending.Push((point, parent, neighbors, 0));
        }

        // Biconnected blocks on the source-to-ground branch contain exactly the edges on simple paths.
        // Collect their edges once instead of enumerating exponentially many overlapping paths.
        AddPoint(source, null);
        while (pending.TryPop(out var frame))
        {
            var (point, parent, neighbors, nextIndex) = frame;
            if (nextIndex < neighbors.Length)
            {
                pending.Push((point, parent, neighbors, nextIndex + 1));
                var connected = neighbors[nextIndex];
                if (ReferenceEquals(connected, parent))
                {
                    continue;
                }
                if (!discovery.TryGetValue(connected, out var connectedOrder))
                {
                    edges.Push((point, connected));
                    AddPoint(connected, point);
                }
                else if (connectedOrder < discovery[point])
                {
                    edges.Push((point, connected));
                    lowest[point] = Math.Min(lowest[point], connectedOrder);
                }
                continue;
            }

            if (parent is null)
            {
                continue;
            }
            var containsGround = reachesGround.Contains(point);
            if (containsGround)
            {
                reachesGround.Add(parent);
            }
            lowest[parent] = Math.Min(lowest[parent], lowest[point]);
            if (lowest[point] < discovery[parent])
            {
                continue;
            }

            // A block attached through only one point cannot lead to ground unless ground is in its subtree.
            (ConnectionPoint From, ConnectionPoint To) edge;
            do
            {
                edge = edges.Pop();
                if (containsGround)
                {
                    paths.Add(GetPolylinePoints([edge.From, edge.To]));
                }
            }
            while (!ReferenceEquals(edge.From, parent) || !ReferenceEquals(edge.To, point));
        }

        return paths;
    }

    private string GetPolylinePoints(IReadOnlyList<ConnectionPoint> path)
    {
        var positions = new List<Point> { SourceGroundPair.GetPointPosition(path[0], SourceGroundPairs) };
        for (var index = 1; index < path.Count; index++)
        {
            var previous = path[index - 1];
            var current = path[index];
            if (previous.Owner is CablePart cable && ReferenceEquals(current.Owner, cable))
            {
                positions.AddRange(ReferenceEquals(previous, cable.Points[0])
                    ? cable.BendPoints
                    : cable.BendPoints.AsEnumerable().Reverse());
            }
            positions.Add(SourceGroundPair.GetPointPosition(current, SourceGroundPairs));
        }

        return string.Join(" ", positions.Select(position =>
            FormattableString.Invariant($"{position.X},{position.Y}")));
    }

    private static string GetInvertedColor(string color) =>
        $"#{(Convert.ToInt32(color[1..], 16) ^ 0xFFFFFF):X6}";

    protected override void OnParametersSet()
    {
        var parts = Circuit.Parts.Concat(SourceGroundPairs
            .SelectMany(pair => new[] { pair.PowerSource, pair.Ground })).ToHashSet();
        foreach (var part in observedParts.Except(parts).ToArray())
        {
            part.Changed -= HandleChanged;
            observedParts.Remove(part);
        }
        foreach (var part in parts.Except(observedParts))
        {
            part.Changed += HandleChanged;
            observedParts.Add(part);
        }
    }

    private void HandleChanged() => StateHasChanged();

    public void Dispose()
    {
        foreach (var part in observedParts)
        {
            part.Changed -= HandleChanged;
        }
    }
}
