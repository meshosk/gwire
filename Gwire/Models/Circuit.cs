using Gwire.Models.Base;

namespace Gwire.Models;

/// <summary>
/// A wiring scheme composed of parts and cables.
/// </summary>
public sealed class Circuit
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();

    public List<BaseCircuitPart> Parts { get; set; } = new();
}
