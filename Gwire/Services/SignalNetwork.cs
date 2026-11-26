using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Models.Pairs;
using Gwire.Models.Signal;
using System.Numerics;

namespace Gwire.Services;

/// <summary>
/// Shares physical connections with path tracing. The selected source's internal
/// connection remains available to the calculation, but cannot close its own path.
/// </summary>
internal sealed class SignalNetwork
{
    private readonly Dictionary<ConnectionPoint, HashSet<ConnectionPoint>> adjacency = [];

    public IReadOnlyList<ElectricalConnection> Connections { get; }

    public SignalNetwork(Circuit circuit, CoilPair source)
    {
        var points = circuit.Parts.SelectMany(part => part.Points).ToHashSet();
        var connections = new List<ElectricalConnection>();
        var idealConnections = new HashSet<(ConnectionPoint, ConnectionPoint)>();
        var coils = circuit.Parts.OfType<CircuitPart>().SelectMany(part => part.ConnectionPairs).OfType<CoilPair>().ToArray();
        var candidates = circuit.Parts.SelectMany(part => part.GetElectricalConnections())
            .Concat(points.SelectMany(point => point.ConnectionPoints.Select(connected => new ElectricalConnection(point, connected))));
        foreach (var connection in candidates)
        {
            var (from, to) = (connection.From, connection.To);
            if (!points.Contains(from) || !points.Contains(to) || ReferenceEquals(from, to))
            {
                continue;
            }
            if (connection.GetImpedance(SignalSpectrum.Frequencies[0]) is null)
            {
                continue;
            }
            // Older parts draw a coil as a wire between its terminals. Next to a CoilPair that wire
            // would short the coil, so it is ignored like in the topological tracing.
            if (connection.GetImpedance(SignalSpectrum.Frequencies[0]) == Complex.Zero
                && coils.Any(coil => coil.Contains(from) && coil.Contains(to)))
            {
                continue;
            }
            if (connection.Pair is null && connection.Resistance == 0
                && (idealConnections.Contains((to, from)) || !idealConnections.Add((from, to))))
            {
                continue;
            }
            connections.Add(connection);
            if (!ReferenceEquals(connection.Pair, source))
            {
                if (!adjacency.ContainsKey(from))
                {
                    adjacency[from] = [];
                }
                if (!adjacency.ContainsKey(to))
                {
                    adjacency[to] = [];
                }
                adjacency[from].Add(to);
                adjacency[to].Add(from);
            }
        }
        Connections = connections;
    }

    public IEnumerable<ConnectionPoint> GetNeighbors(ConnectionPoint point) =>
        adjacency.TryGetValue(point, out var neighbors) ? neighbors : [];
}
