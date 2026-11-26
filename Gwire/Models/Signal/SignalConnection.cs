using Gwire.Models.Base;
using Gwire.Models.Pairs;

namespace Gwire.Models.Signal;

/// <summary>
/// A connection reached by coil signals and its illustrative levels: Signals at From, EndSignals at To.
/// </summary>
public sealed record SignalConnection(
    ConnectionPoint From, ConnectionPoint To, IReadOnlyDictionary<CoilPair, SignalSpectrum> Signals,
    IReadOnlyDictionary<CoilPair, SignalSpectrum> EndSignals)
{
    public IReadOnlySet<CoilPair> ReverseFlow { get; init; } = new HashSet<CoilPair>();

    public IReadOnlyList<CoilPair> Coils => Signals.Keys.ToArray();
}
