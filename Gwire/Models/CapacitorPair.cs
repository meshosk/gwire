using Gwire.Models.Base;
using System.Text.Json.Serialization;
using System.Numerics;

namespace Gwire.Models;

public sealed class CapacitorPair : ConnectionPair
{
    [JsonIgnore]
    public override string TypeName => "Capacitor";

    /// <summary>
    /// Capacitance in farads.
    /// </summary>
    public double Capacitance { get; set; }

    public override Complex? GetImpedance(double frequency)
    {
        if (!double.IsFinite(Capacitance) || Capacitance <= 0 || !double.IsFinite(frequency) || frequency <= 0)
        {
            return null;
        }
        var reactance = (1 / (2 * Math.PI * frequency)) / Capacitance;
        return double.IsFinite(reactance) ? new Complex(0, -reactance) : null;
    }
}
