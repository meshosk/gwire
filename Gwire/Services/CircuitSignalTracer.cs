using Gwire.Models;
using Gwire.Models.Base;

namespace Gwire.Services;

/// <summary>
/// Collects connections on paths between each coil's terminals, excluding dead-end branches.
/// </summary>
public static class CircuitSignalTracer
{
    public static IReadOnlyList<SignalConnection> Trace(Circuit circuit)
    {
        var parts = circuit.Parts.ToHashSet();
        var connections = new Dictionary<(ConnectionPoint From, ConnectionPoint To), List<CoilPair>>();
        foreach (var coil in circuit.Parts.OfType<CircuitPart>().SelectMany(part => part.ConnectionPairs).OfType<CoilPair>())
        {
            // A newly created, unwired coil still has a visible internal connection.
            AddConnection(connections, coil.Point2, coil.Point1, coil);
            foreach (var connection in SignalPathFinder.FindConnections(coil, parts))
            {
                AddConnection(connections, connection.From, connection.To, coil);
            }
        }

        return connections.Select(connection => new SignalConnection(
            connection.Key.From, connection.Key.To, connection.Value)).ToList();
    }

    private static void AddConnection(
        Dictionary<(ConnectionPoint From, ConnectionPoint To), List<CoilPair>> connections,
        ConnectionPoint from, ConnectionPoint to, CoilPair coil)
    {
        var key = connections.ContainsKey((to, from)) ? (to, from) : (from, to);
        if (!connections.TryGetValue(key, out var coils))
        {
            coils = [];
            connections.Add(key, coils);
        }
        if (!coils.Contains(coil))
        {
            coils.Add(coil);
        }
    }
}
