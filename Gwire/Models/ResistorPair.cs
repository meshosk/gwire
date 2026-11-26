using Gwire.Models.Base;
using System.Text.Json.Serialization;
using System.Numerics;

namespace Gwire.Models;

public sealed class ResistorPair : ConnectionPair
{
    [JsonIgnore]
    public override string TypeName => "Resistor";

    /// <summary>
    /// Resistance in ohms.
    /// </summary>
    public double Resistance { get; set; }

    public override Complex? GetImpedance(double frequency) => double.IsFinite(Resistance) && Resistance >= 0
        ? new Complex(Resistance, 0) : null;
}
