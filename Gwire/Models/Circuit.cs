using Gwire.Models.Base;

namespace Gwire.Models;

/// <summary>
/// A wiring scheme composed of parts and cables.
/// </summary>
public sealed class Circuit
{
    public List<BaseCircuitPart> Parts { get; set; } = new();
}
