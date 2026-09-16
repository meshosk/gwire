namespace Gwire.Models.Base;

/// <summary>
/// Represent connection point of a part. Multiple parts can be connected into a circuit.
/// </summary>
public class ConnectionPoint
{
    /// <summary>
    /// Connection needs a name.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// X coordinate of the point in the part's local coordinate system.
    /// </summary>
    public double LocalX { get; set; }
    
    /// <summary>
    /// Y coordinate of the point in the part's local coordinate system.
    /// </summary>
    public double LocalY { get; set; }
}