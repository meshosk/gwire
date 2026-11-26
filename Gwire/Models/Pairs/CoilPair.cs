using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Models.Signal;
using System.Text.Json.Serialization;
using System.Numerics;

namespace Gwire.Models.Pairs;

/// <summary>
/// The two terminals of one pickup coil. Point1 is Signal and Point2 is Ground
/// by convention; neither terminal is forced to the scheme's ground.
/// </summary>
public sealed class CoilPair : ConnectionPair
{
    [JsonIgnore]
    public override string TypeName => "Coil";

    [JsonRequired]
    public ConnectionPoint Point1 { get; set; } = default!;

    [JsonRequired]
    public ConnectionPoint Point2 { get; set; } = default!;

    public string Color { get; set; } = "#e63946";

    public CoilWindingDirection Winding { get; set; } = CoilWindingDirection.CCW;

    public CoilPolarity Polarity { get; set; } = CoilPolarity.North;

    // All coils use the same reference source resistance in this illustrative model.
    public override Complex? GetImpedance(double frequency) => new Complex(ReferenceResistance, 0);

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
        var clone = (CoilPair)MemberwiseClone();
        clone.Point1 = clonedPoints[Point1];
        clone.Point2 = clonedPoints[Point2];
        return clone;
    }
}
