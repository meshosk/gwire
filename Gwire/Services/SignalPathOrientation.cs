using Gwire.Models.Base;

namespace Gwire.Services;

internal static class SignalPathOrientation
{
    public static IReadOnlyDictionary<ConnectionPoint, int> Rank(
        ConnectionPoint source, ConnectionPoint ground,
        IReadOnlyList<(ConnectionPoint From, ConnectionPoint To)> paths)
    {
        var neighbors = new Dictionary<ConnectionPoint, List<ConnectionPoint>>();
        foreach (var (from, to) in paths)
        {
            if (!neighbors.TryGetValue(from, out var fromNeighbors))
            {
                neighbors[from] = fromNeighbors = [];
            }
            if (!neighbors.TryGetValue(to, out var toNeighbors))
            {
                neighbors[to] = toNeighbors = [];
            }
            fromNeighbors.Add(to);
            toNeighbors.Add(from);
        }

        // Adding a temporary source-to-ground edge makes the retained graph
        // biconnected. Visit ground first so an st-numbering starts at source and
        // ends at ground. The temporary edge is never rendered.
        neighbors[source].Insert(0, ground);

        var points = new List<ConnectionPoint>();
        var discovery = new Dictionary<ConnectionPoint, int>();
        var lowest = new Dictionary<ConnectionPoint, int>();
        var parents = new Dictionary<ConnectionPoint, ConnectionPoint>();
        var pending = new Stack<(ConnectionPoint Point, int NextNeighbor)>();

        void Discover(ConnectionPoint point)
        {
            discovery[point] = points.Count;
            lowest[point] = points.Count;
            points.Add(point);
            pending.Push((point, 0));
        }

        Discover(source);
        while (pending.TryPop(out var frame))
        {
            var (point, next) = frame;
            if (next == neighbors[point].Count)
            {
                if (parents.TryGetValue(point, out var completedParent))
                {
                    lowest[completedParent] = Math.Min(lowest[completedParent], lowest[point]);
                }
                continue;
            }

            pending.Push((point, next + 1));
            var neighbor = neighbors[point][next];
            if (!discovery.TryGetValue(neighbor, out var index))
            {
                parents[neighbor] = point;
                Discover(neighbor);
            }
            else if (!parents.TryGetValue(point, out var parent) || !ReferenceEquals(parent, neighbor))
            {
                lowest[point] = Math.Min(lowest[point], index);
            }
        }

        // In an st-numbering every intermediate point has a neighbor on either
        // side, so each oriented edge lies on a directed Signal-to-Ground path.
        var order = new LinkedList<ConnectionPoint>();
        var positions = new Dictionary<ConnectionPoint, LinkedListNode<ConnectionPoint>>
        {
            [source] = order.AddFirst(source),
            [ground] = order.AddLast(ground)
        };
        var insertBefore = new HashSet<ConnectionPoint> { source };
        foreach (var point in points.Skip(2))
        {
            var parent = parents[point];
            if (insertBefore.Contains(points[lowest[point]]))
            {
                positions[point] = order.AddBefore(positions[parent], point);
                insertBefore.Remove(parent);
            }
            else
            {
                positions[point] = order.AddAfter(positions[parent], point);
                insertBefore.Add(parent);
            }
        }

        return order.Select((point, index) => (point, index))
            .ToDictionary(entry => entry.point, entry => entry.index);
    }
}
