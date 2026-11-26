using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Models.Pairs;
using Gwire.Models.Signal;

namespace Gwire.Services;

/// <summary>
/// Shows how each coil's signal spreads from its Signal point to its own Ground point and
/// how the parts on the way shape it. See <see cref="SignalPropagation"/> for the rules.
/// </summary>
public static class CircuitSignalTracer
{
    // Relative to the coil's own signal; weaker ribbons would only be visual noise.
    private const double MinimumVisibleLevel = 0.02;

    public static IReadOnlyList<SignalConnection> Trace(Circuit circuit) => TraceWithPoints(circuit).Connections;

    public static SignalTraceResult TraceWithPoints(Circuit circuit)
    {
        var connections = new Dictionary<(ConnectionPoint From, ConnectionPoint To),
            (Dictionary<CoilPair, SignalSpectrum> Start, Dictionary<CoilPair, SignalSpectrum> End)>();
        var reverseFlow = new Dictionary<(ConnectionPoint From, ConnectionPoint To), HashSet<CoilPair>>();
        var pointSignals = new Dictionary<ConnectionPoint, Dictionary<CoilPair, SignalSpectrum>>();
        foreach (var coil in circuit.Parts.OfType<CircuitPart>().SelectMany(part => part.ConnectionPairs).OfType<CoilPair>())
        {
            var network = new SignalNetwork(circuit, coil);
            var paths = SignalPathFinder.FindConnections(coil, network);
            // A coil without a closed loop is not heard.
            if (paths.Count == 0)
            {
                continue;
            }

            var (levels, flows) = SignalPropagation.Calculate(coil, network, paths);
            foreach (var (point, spectrum) in levels)
            {
                if (!pointSignals.TryGetValue(point, out var signals))
                {
                    signals = [];
                    pointSignals.Add(point, signals);
                }
                signals[coil] = spectrum;
            }
            foreach (var flow in flows)
            {
                // A connection with a signal on only one end would be drawn as a narrowing
                // triangle; show only connections that carry a signal on both ends.
                if (IsVisible(flow.Start) && IsVisible(flow.End))
                {
                    AddConnection(connections, reverseFlow, flow.From, flow.To, coil, flow.Start, flow.End);
                }
            }
        }

        return new SignalTraceResult(
            connections.Where(connection => IsVisible(Combine(connection.Value.Start.Values))
                    && IsVisible(Combine(connection.Value.End.Values)))
                .Select(connection => new SignalConnection(
                    connection.Key.From, connection.Key.To, connection.Value.Start, connection.Value.End)
                {
                    ReverseFlow = reverseFlow[connection.Key]
                }).ToList(),
            pointSignals.ToDictionary(entry => entry.Key,
                entry => (IReadOnlyDictionary<CoilPair, SignalSpectrum>)entry.Value));
    }

    private static void AddConnection(
        Dictionary<(ConnectionPoint From, ConnectionPoint To),
            (Dictionary<CoilPair, SignalSpectrum> Start, Dictionary<CoilPair, SignalSpectrum> End)> connections,
        Dictionary<(ConnectionPoint From, ConnectionPoint To), HashSet<CoilPair>> reverseFlow,
        ConnectionPoint from, ConnectionPoint to, CoilPair coil, SignalSpectrum start, SignalSpectrum end)
    {
        var reversed = connections.ContainsKey((to, from));
        var key = reversed ? (to, from) : (from, to);
        if (!connections.TryGetValue(key, out var signals))
        {
            signals = (new Dictionary<CoilPair, SignalSpectrum>(), new Dictionary<CoilPair, SignalSpectrum>());
            connections.Add(key, signals);
            reverseFlow.Add(key, []);
        }
        if (reversed)
        {
            reverseFlow[key].Add(coil);
            (start, end) = (end, start);
        }
        // Parallel parts between the same two points share one ribbon and add up.
        signals.Start[coil] = signals.Start.TryGetValue(coil, out var previousStart) ? previousStart.Combine(start) : start;
        signals.End[coil] = signals.End.TryGetValue(coil, out var previousEnd) ? previousEnd.Combine(end) : end;
    }

    private static SignalSpectrum Combine(IEnumerable<SignalSpectrum> signals) =>
        signals.Aggregate(default(SignalSpectrum), (total, signal) => total.Combine(signal));

    private static bool IsVisible(SignalSpectrum signal) =>
        Math.Max(signal.Bass, Math.Max(signal.Mids, signal.Treble)) >= MinimumVisibleLevel;
}
