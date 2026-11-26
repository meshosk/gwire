using System.Text.Json.Serialization;
using System.Numerics;

namespace Gwire.Models.Base;

/// <summary>
/// An electrical element between two of its part's existing connection points.
/// Unlike a connection group, a pair does not represent a direct connection.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(CoilPair), "coil")]
[JsonDerivedType(typeof(ResistorPair), "resistor")]
[JsonDerivedType(typeof(CapacitorPair), "capacitor")]
public abstract class ConnectionPair
{
    // Shared visual calibration; this is not an additional element in the circuit.
    protected const double ReferenceResistance = 10_000;

    [JsonIgnore]
    /// <summary>
    /// Used in the UI to display a human-readable name for the type of connection pair.
    /// </summary>
    public abstract string TypeName { get; }
    /// <summary>
    /// Pair internal name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    [JsonRequired]
    public ConnectionPoint Point1 { get; set; } = default!;

    [JsonRequired]
    public ConnectionPoint Point2 { get; set; } = default!;

    public abstract Complex? GetImpedance(double frequency);

    /// <summary>
    /// Share of the signal this pair passes between its two points (0 to 1) for the given
    /// impedance of the circuit before it: |Z_before| / |Z_before + Z_pair|.
    /// </summary>
    public virtual double GetTransfer(double frequency, Complex sourceImpedance) =>
        GetImpedance(frequency) is { } impedance ? GetSeriesTransfer(impedance, sourceImpedance) : 0;

    public static double GetSeriesTransfer(Complex impedance, Complex sourceImpedance)
    {
        if (impedance.Magnitude < 1e-9)
        {
            return 1;
        }
        var total = (sourceImpedance + impedance).Magnitude;
        return total > 0 ? Math.Min(1, sourceImpedance.Magnitude / total) : 0;
    }

    public bool Contains(ConnectionPoint point) =>
        ReferenceEquals(Point1, point) || ReferenceEquals(Point2, point);

    public ConnectionPair Clone(IReadOnlyDictionary<ConnectionPoint, ConnectionPoint> clonedPoints)
    {
        var clone = (ConnectionPair)MemberwiseClone();
        clone.Point1 = clonedPoints[Point1];
        clone.Point2 = clonedPoints[Point2];
        return clone;
    }
}
