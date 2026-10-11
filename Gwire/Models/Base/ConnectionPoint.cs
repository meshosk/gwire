using System.Text.Json.Serialization;

namespace Gwire.Models.Base;

/// <summary>
/// Represent connection point of a part. Multiple parts can be connected into a circuit.
/// </summary>
public class ConnectionPoint
{
    private bool isHighlighted;

    [JsonIgnore]
    public bool IsHighlighted
    {
        get => isHighlighted;
        set
        {
            if (isHighlighted == value)
            {
                return;
            }

            isHighlighted = value;
            Owner?.NotifyChanged();
        }
    }

    /// <summary>
    /// Connection needs a name.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Position of the point in the part's local coordinate system.
    /// </summary>
    public Point LocalPosition { get; set; } = new();

    #region networking

    /// <summary>
    /// Part that owns actual connection point.
    /// </summary>
    [JsonInclude]
    public BaseCircuitPart Owner { get; private set; } = default!;

    /// <summary>
    /// If is not null, this point is connected to point of other part.
    /// </summary>
    [JsonInclude]
    public List<ConnectionPoint> ConnectionPoints { get; private set; } = new();


    [JsonConstructor]
    private ConnectionPoint()
    {
    }

    public ConnectionPoint(BaseCircuitPart owner)
    {
        Owner = owner;
    }

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

        if (Owner is Gwire.Models.CircuitPart part && point.Owner is Gwire.Models.CablePart)
        {
            part.UpdateConnectedCablePoints();
        }
        else if (point.Owner is Gwire.Models.CircuitPart connectedPart && Owner is Gwire.Models.CablePart)
        {
            connectedPart.UpdateConnectedCablePoints();
        }
        else
        {
            Owner?.NotifyChanged();
            point.Owner?.NotifyChanged();
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

        Owner?.NotifyChanged();
        point.Owner?.NotifyChanged();
    }

    /// <summary>
    /// Creates a clone without external point connections.
    /// </summary>
    public ConnectionPoint Clone(BaseCircuitPart newPointOwner)
    {
        var clone = (ConnectionPoint)MemberwiseClone();
        clone.Owner = newPointOwner; // shady but works
        clone.ConnectionPoints = new List<ConnectionPoint>();
        clone.isHighlighted = false;
        clone.LocalPosition = LocalPosition.Copy();
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

    /// <summary>
    /// Finds a path to the destination through external connections and each owner's active state.
    /// Returns an empty list when the destination cannot be reached.
    /// </summary>
    public IReadOnlyList<ConnectionPoint> FindPathTo(ConnectionPoint destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        var predecessors = new Dictionary<ConnectionPoint, ConnectionPoint?> { [this] = null };
        var pending = new Queue<ConnectionPoint>();
        pending.Enqueue(this);

        while (pending.TryDequeue(out var point))
        {
            if (ReferenceEquals(point, destination))
            {
                var path = new List<ConnectionPoint>();
                for (ConnectionPoint? current = destination; current is not null; current = predecessors[current])
                {
                    path.Add(current);
                }
                path.Reverse();
                return path;
            }

            foreach (var connected in point.GetConnectedPoints(point).Concat(point.Owner.GetConnectedPoints(point)))
            {
                if (predecessors.TryAdd(connected, point))
                {
                    pending.Enqueue(connected);
                }
            }
        }

        return Array.Empty<ConnectionPoint>();
    }

    #endregion
}
