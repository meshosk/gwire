using Gwire.Models;
using Gwire.Models.Pairs;
using System.Text.Json.Serialization;
using System.Numerics;

namespace Gwire.Models.Base;

/// <summary>
/// An electrical element between existing connection points of its part.
/// Each subclass owns its terminals; their number depends on the element type.
/// Unlike a connection group, a pair does not represent a direct connection.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(CoilPair), "coil")]
[JsonDerivedType(typeof(ResistorPair), "resistor")]
[JsonDerivedType(typeof(CapacitorPair), "capacitor")]
[JsonDerivedType(typeof(Potentiometer), "potentiometer")]
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

    /// <summary>
    /// The terminals of this element in their fixed order. The part adds and removes exactly these points.
    /// </summary>
    public abstract IReadOnlyList<ConnectionPoint> GetPoints();

    public abstract Complex? GetImpedance(double frequency);

    /// <summary>
    /// Internal connections this element creates between its own terminals.
    /// </summary>
    public abstract IEnumerable<ElectricalConnection> GetElectricalConnections();

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

    public bool Contains(ConnectionPoint point) => GetPoints().Any(terminal => ReferenceEquals(terminal, point));

    /// <summary>
    /// Clones the element and maps its terminals to the cloned points of the part.
    /// </summary>
    public abstract ConnectionPair Clone(IReadOnlyDictionary<ConnectionPoint, ConnectionPoint> clonedPoints);
}
