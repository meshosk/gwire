using Gwire.Models.Base;
using System.Text.Json.Serialization;

namespace Gwire.Models;

public sealed class ResistorPair : ConnectionPair
{
    [JsonIgnore]
    public override string TypeName => "Resistor";

    /// <summary>
    /// Resistance in ohms.
    /// </summary>
    public double Resistance { get; set; }
}
