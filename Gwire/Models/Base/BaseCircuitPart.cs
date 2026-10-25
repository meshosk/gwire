using System.Text.Json.Serialization;

namespace Gwire.Models.Base;

/// <summary>
/// Shared definition for every element that can occur in a wiring scheme.
/// Provides common metadata, terminals, and access to internal connections.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Gwire.Models.CablePart), "cable")]
[JsonDerivedType(typeof(Gwire.Models.CircuitPart), "circuitPart")]
public abstract class BaseCircuitPart
{
    public event Action? Changed;

    public void NotifyChanged() => Changed?.Invoke();

    protected BaseCircuitPart()
    {
        ClassType = GetType().Name;
    }

    /// <summary>
    /// Name of the concrete part class. // > maybe not needed at all
    /// </summary>
    public string ClassType { get; }

    /// <summary>
    /// Part name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Description - text only.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// List of connection points that can connect part into circuit.
    /// </summary>
    public List<ConnectionPoint> Points { get; set; } = new();

    /// <summary>
    /// Creates a deep clone of the part and its points without external connections.
    /// </summary>
    public virtual BaseCircuitPart Clone()
    {
        var clone = (BaseCircuitPart)MemberwiseClone();
        clone.Changed = null;
        clone.Points = Points.Select(point => point.Clone(clone)).ToList();
        return clone;
    }

    #region networking

    /// <summary>
    /// Gets the points internally connected to the starting point.
    /// </summary>
    /// <param name="startingPoint"></param>
    /// <returns></returns>
    public abstract IEnumerable<ConnectionPoint> GetConnectedPoints(ConnectionPoint startingPoint);

    #endregion

}
