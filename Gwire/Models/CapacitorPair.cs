using Gwire.Models.Base;
using System.Text.Json.Serialization;

namespace Gwire.Models;

public sealed class CapacitorPair : ConnectionPair
{
    [JsonIgnore]
    public override string TypeName => "Capacitor";

    /// <summary>
    /// Capacitance in farads.
    /// </summary>
    public double Capacitance { get; set; }
}
