using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Models.Pairs;
using Gwire.Models.Signal;
using System.Numerics;

namespace Gwire.Services;

/// <summary>
/// One connection reached by a coil's signal, oriented in the direction the signal spreads.
/// Start is the level entering at From, End the level leaving at To.
/// </summary>
internal readonly record struct SignalFlow(
    ElectricalConnection Connection, ConnectionPoint From, ConnectionPoint To, SignalSpectrum Start, SignalSpectrum End);

/// <summary>
/// Spreads one coil's signal point by point from its Signal point (Point1) to its own
/// Ground point (Point2) in three bands, carrying the impedance of the circuit before each
/// point along with the level.
/// </summary>
internal static class SignalPropagation
{
    /// <summary>
    /// The Signal point starts at 100 % with the coil's impedance behind it. A wire changes
    /// nothing. A part changes the signal only between its own points, by its
    /// <see cref="ElectricalConnection.GetTransfer"/>; Z_before grows by each part. A fork splits
    /// the level by the admittance of its branches (part + impedance to Ground), a join adds
    /// them up. Dead ends are ignored.
    /// </summary>
    public static (IReadOnlyDictionary<ConnectionPoint, SignalSpectrum> PointLevels, IReadOnlyList<SignalFlow> Flows) Calculate(
        CoilPair coil, SignalNetwork network, IReadOnlyList<(ConnectionPoint From, ConnectionPoint To)> paths)
    {
        // Orient the coil's loop from Signal to Ground; every point is visited after all
        // points that feed it.
        var rank = SignalPathOrientation.Rank(coil.Point1, coil.Point2, paths);
        var loopPairs = paths.ToHashSet();
        var outgoing = rank.Keys.ToDictionary(point => point, _ => new List<(ElectricalConnection Connection, ConnectionPoint To)>());
        foreach (var connection in network.Connections.Where(connection => !ReferenceEquals(connection.Pair, coil)
            && (loopPairs.Contains((connection.From, connection.To)) || loopPairs.Contains((connection.To, connection.From)))))
        {
            var (from, to) = rank[connection.From] < rank[connection.To]
                ? (connection.From, connection.To) : (connection.To, connection.From);
            outgoing[from].Add((connection, to));
        }
        var order = rank.OrderBy(entry => entry.Value).Select(entry => entry.Key).ToArray();

        var bands = SignalSpectrum.Frequencies.Count;
        var levels = order.ToDictionary(point => point, _ => new Complex[bands]);
        var flows = new Dictionary<ElectricalConnection, (ConnectionPoint From, ConnectionPoint To, Complex[] Start, Complex[] End)>(
            ReferenceEqualityComparer.Instance);
        foreach (var (from, branches) in outgoing)
        {
            foreach (var (connection, to) in branches)
            {
                flows[connection] = (from, to, new Complex[bands], new Complex[bands]);
            }
        }

        var reversed = (coil.Winding == CoilWindingDirection.CW) ^ (coil.Polarity == CoilPolarity.South);
        var sourceSign = reversed ? -1 : 1;
        for (var band = 0; band < bands; band++)
        {
            var frequency = SignalSpectrum.Frequencies[band];
            Complex ImpedanceOf(ElectricalConnection connection) => connection.GetImpedance(frequency) ?? Complex.Zero;

            // Impedance from each point to Ground, from Ground backwards.
            var after = new Dictionary<ConnectionPoint, Complex>();
            foreach (var point in order.Reverse())
            {
                var branchImpedances = outgoing[point].Select(branch => ImpedanceOf(branch.Connection) + after[branch.To]).ToArray();
                after[point] = branchImpedances.Length == 0 || branchImpedances.Any(IsZero)
                    ? Complex.Zero
                    : Complex.One / branchImpedances.Aggregate(Complex.Zero, (total, impedance) => total + Complex.One / impedance);
            }

            // Level and impedance before each point, from the Signal point forwards.
            var incomingAdmittance = order.ToDictionary(point => point, _ => Complex.Zero);
            var shortedBefore = new HashSet<ConnectionPoint>();
            levels[coil.Point1][band] = sourceSign;
            foreach (var point in order)
            {
                var before = ReferenceEquals(point, coil.Point1) ? coil.GetImpedance(frequency) ?? Complex.Zero
                    : shortedBefore.Contains(point) || IsZero(incomingAdmittance[point]) ? Complex.Zero
                    : Complex.One / incomingAdmittance[point];
                var branches = outgoing[point];
                if (branches.Count == 0)
                {
                    continue;
                }

                // A fork splits by admittance; a branch with no impedance takes everything.
                var branchImpedances = branches.Select(branch => ImpedanceOf(branch.Connection) + after[branch.To]).ToArray();
                var shorted = branchImpedances.Count(IsZero);
                var admittances = branchImpedances.Select(impedance => shorted > 0
                    ? IsZero(impedance) ? 1d : 0d
                    : 1 / impedance.Magnitude).ToArray();
                var totalAdmittance = admittances.Sum();
                for (var index = 0; index < branches.Count; index++)
                {
                    var (connection, to) = branches[index];
                    var impedance = ImpedanceOf(connection);
                    var flow = flows[connection];
                    flow.Start[band] = levels[point][band] * (totalAdmittance > 0 ? admittances[index] / totalAdmittance : 1d / branches.Count);
                    flow.End[band] = flow.Start[band] * connection.GetTransfer(frequency, before);
                    levels[to][band] += flow.End[band];

                    var output = before + impedance;
                    if (IsZero(output))
                    {
                        shortedBefore.Add(to);
                    }
                    else
                    {
                        incomingAdmittance[to] += Complex.One / output;
                    }
                }
            }
        }

        static SignalSpectrum ToSpectrum(Complex[] phasors) => SignalSpectrum.FromPhasors(phasors[0], phasors[1], phasors[2]);
        return (levels.ToDictionary(entry => entry.Key, entry => ToSpectrum(entry.Value)),
            flows.Select(entry => new SignalFlow(entry.Key, entry.Value.From, entry.Value.To,
                ToSpectrum(entry.Value.Start), ToSpectrum(entry.Value.End))).ToList());
    }

    private static bool IsZero(Complex value) => value.Magnitude < 1e-9;
}
