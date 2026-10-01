using System.Text.Json.Serialization;

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

    #region networking

    /// <summary>
    /// Part that owns actual connection point.
    /// </summary>
    public BaseCircuitPart Owner { get; set; }

    /// <summary>
    /// If is not null, this point is connected to point of other part.
    /// </summary>
    [JsonInclude]
    public List<ConnectionPoint> ConnectionPoints { get; private set; } = new();

    /// <summary>
    /// Bi-directional point connections
    /// </summary>  
    /// <param name="point"></param>
    public void Connect(ConnectionPoint point)
    {
        if (!this.ConnectionPoints.Contains(point))
        {
            this.ConnectionPoints.Add(point);
        }
        
        if (!point.ConnectionPoints.Contains(this))
        {
            point.ConnectionPoints.Add(this);
        }
    }

    public void Disconnect(ConnectionPoint point)
    {
        if (this.ConnectionPoints.Contains(point))
        {
            this.ConnectionPoints.Remove(point);
        }

        if (point.ConnectionPoints.Contains(this))
        {
            point.ConnectionPoints.Remove(this);
        }
    }

    /// <summary>
    /// Creates a clone without external point connections.
    /// </summary>
    public ConnectionPoint Clone()
    {
        var clone = (ConnectionPoint)MemberwiseClone();
        clone.ConnectionPoints = new List<ConnectionPoint>();
        return clone;
    }


    /// <summary>
    /// Get all points connected to this point, except the starting point.
    /// </summary>
    /// <param name="startingPoint"></param>
    /// <returns></returns>
    public IEnumerable<ConnectionPoint> GetConnectedPoints(ConnectionPoint startingPoint)
    {
        return ConnectionPoints.Where(p => p != startingPoint);
    }

    #endregion
}
