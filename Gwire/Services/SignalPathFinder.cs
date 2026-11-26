using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Models.Pairs;

namespace Gwire.Services;

/// <summary>
/// Finds every connection on a simple path between a coil's terminals.
/// Uses the same biconnected-block approach as source-ground path highlighting,
/// without enumerating exponentially many paths through parallel branches.
/// </summary>
internal static class SignalPathFinder
{
    public static IReadOnlyList<(ConnectionPoint From, ConnectionPoint To)> FindConnections(
        CoilPair coil, SignalNetwork network)
    {
        var source = coil.Point1;
        var destination = coil.Point2;
        var discovery = new Dictionary<ConnectionPoint, int>();
        var lowest = new Dictionary<ConnectionPoint, int>();
        var reachesDestination = new HashSet<ConnectionPoint>();
        var edges = new Stack<(ConnectionPoint From, ConnectionPoint To)>();
        var pending = new Stack<(ConnectionPoint Point, ConnectionPoint? Parent, ConnectionPoint[] Neighbors, int NextIndex)>();
        var connections = new List<(ConnectionPoint From, ConnectionPoint To)>();

        void AddPoint(ConnectionPoint point, ConnectionPoint? parent)
        {
            discovery[point] = discovery.Count;
            lowest[point] = discovery[point];
            if (ReferenceEquals(point, destination))
            {
                reachesDestination.Add(point);
            }
            var neighbors = network.GetNeighbors(point).ToArray();
            pending.Push((point, parent, neighbors, 0));
        }

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
            var containsDestination = reachesDestination.Contains(point);
            if (containsDestination)
            {
                reachesDestination.Add(parent);
            }
            lowest[parent] = Math.Min(lowest[parent], lowest[point]);
            if (lowest[point] < discovery[parent])
            {
                continue;
            }

            // Keep only blocks on the source-to-destination branch. A dangling loop
            // has no simple path to the destination and is excluded just like a leaf.
            (ConnectionPoint From, ConnectionPoint To) edge;
            do
            {
                edge = edges.Pop();
                if (containsDestination)
                {
                    connections.Add(edge);
                }
            }
            while (!ReferenceEquals(edge.From, parent) || !ReferenceEquals(edge.To, point));
        }

        return connections;
    }
}
