using Gwire.Models.Base;

namespace Gwire.Models;

/// <summary>
/// A reachable connection and the pickup coils contributing to its display.
/// This describes connectivity, not frequency response or signal cancellation.
/// </summary>
public sealed record SignalConnection(ConnectionPoint From, ConnectionPoint To, IReadOnlyList<CoilPair> Coils);
