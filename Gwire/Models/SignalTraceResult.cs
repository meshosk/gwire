using Gwire.Models.Base;

namespace Gwire.Models;

/// <summary>
/// A single calculation shared by signal ribbons and point inspection.
/// Point levels retain the identity of each contributing coil.
/// </summary>
public sealed record SignalTraceResult(
    IReadOnlyList<SignalConnection> Connections,
    IReadOnlyDictionary<ConnectionPoint, IReadOnlyDictionary<CoilPair, SignalSpectrum>> PointSignals);
