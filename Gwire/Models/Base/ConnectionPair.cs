using System.Text.Json.Serialization;

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
