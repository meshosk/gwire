namespace Gwire.Models.Base;

/// <summary>
/// Degines what groups are interconnected in a state.
/// </summary>
public class PartState
{
    /// <summary>
    /// State name
    /// </summary>
    public string Label { get; set; }
    /// <summary>
    /// State can set interconnect of a points groups, so part line simple on//off state can be created. 
    /// </summary>
    public List<ConnectionGroup> ConnectionGroups { get; set; } = new();

    /// <summary>
    /// Search all groups and return all points that are connected to the starting point.
    /// Starting point is excluded from the result list. Without duplicates.
    /// </summary>
    /// <param name="startingPoint"></param>
    /// <returns></returns>
    public IEnumerable<ConnectionPoint> GetConnectedPoints(ConnectionPoint startingPoint)
    {
        return ConnectionGroups
                .Where(group => group.ConnectedPints.Contains(startingPoint))
                .SelectMany(group => group.ConnectedPints)
                .Distinct()
                .Where(x => x != startingPoint);
    }
}

