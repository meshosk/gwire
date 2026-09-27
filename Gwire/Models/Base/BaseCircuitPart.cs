namespace Gwire.Models.Base;

/// <summary>
/// Shared definition for every element that can occur in a wiring scheme.
/// A wire and a user-defined part differ only in their specialised data, not
/// in how their terminals and internal connections are represented.
/// </summary>
public abstract class BaseCircuitPart
{
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
    /// Defines states. Each state defines how points are interconnected. Active is only one.
    /// </summary>
    public List<PartState> States { get; set; } = new();

    /// <summary>
    /// Defines which state is active.
    /// </summary>
    public PartState? ActiveState { get; set; } = null;

    /// <summary>
    /// Creates a deep clone while preserving the connections between cloned points and states.
    /// </summary>
    public virtual BaseCircuitPart Clone()
    {
        var clone = (BaseCircuitPart)MemberwiseClone();
        var clonedPoints = new Dictionary<ConnectionPoint, ConnectionPoint>();

        clone.Points = Points.Select(point =>
        {
            var clonedPoint = new ConnectionPoint
            {
                Label = point.Label,
                LocalX = point.LocalX,
                LocalY = point.LocalY
            };
            clonedPoints.Add(point, clonedPoint);
            return clonedPoint;
        }).ToList();

        clone.States = States.Select(state => new PartState
        {
            Label = state.Label,
            ConnectionGroups = state.ConnectionGroups.Select(group => new ConnectionGroup
            {
                ConnectedPints = group.ConnectedPints.Select(point => clonedPoints[point]).ToList()
            }).ToList()
        }).ToList();

        var activeStateIndex = States.FindIndex(state => ReferenceEquals(state, ActiveState));
        clone.ActiveState = activeStateIndex >= 0 ? clone.States[activeStateIndex] : null;
        return clone;
    }
}
