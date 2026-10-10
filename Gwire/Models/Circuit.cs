using Gwire.Models.Base;
using System.Text.Json.Serialization;

namespace Gwire.Models;

/// <summary>
/// A wiring scheme composed of parts and cables.
/// </summary>
public sealed class Circuit
{
    [JsonIgnore]
    public BaseCircuitPart? SelectedPart { get; set; }

    public List<BaseCircuitPart> Parts { get; set; } = new();
}
