using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Models.Signal;
using System.Text.Json.Serialization;
using System.Numerics;

namespace Gwire.Models.Pairs;

public sealed class ResistorPair : ConnectionPair
{
    [JsonIgnore]
    public override string TypeName => "Resistor";

    [JsonRequired]
    public ConnectionPoint Point1 { get; set; } = default!;

    [JsonRequired]
    public ConnectionPoint Point2 { get; set; } = default!;

    /// <summary>
    /// Resistance in ohms.
    /// </summary>
    public double Resistance { get; set; }

    public override Complex? GetImpedance(double frequency) => double.IsFinite(Resistance) && Resistance >= 0
        ? new Complex(Resistance, 0) : null;

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
        var clone = (ResistorPair)MemberwiseClone();
        clone.Point1 = clonedPoints[Point1];
        clone.Point2 = clonedPoints[Point2];
        return clone;
    }
}
