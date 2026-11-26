using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Models.Signal;
using System.Text.Json.Serialization;
using System.Numerics;

namespace Gwire.Models.Pairs;

public sealed class CapacitorPair : ConnectionPair
{
    [JsonIgnore]
    public override string TypeName => "Capacitor";

    [JsonRequired]
    public ConnectionPoint Point1 { get; set; } = default!;

    [JsonRequired]
    public ConnectionPoint Point2 { get; set; } = default!;

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

    public override IReadOnlyList<ConnectionPoint> GetPoints() => [Point1, Point2];

    public override IEnumerable<ElectricalConnection> GetElectricalConnections()
    {
        if (GetImpedance(SignalSpectrum.Frequencies[0]) is not null)
        {
            yield return new ElectricalConnection(Point1, Point2, this);
        }
    }

    public override ConnectionPair Clone(IReadOnlyDictionary<ConnectionPoint, ConnectionPoint> clonedPoints)
    {
        var clone = (CapacitorPair)MemberwiseClone();
        clone.Point1 = clonedPoints[Point1];
        clone.Point2 = clonedPoints[Point2];
        return clone;
    }
}
