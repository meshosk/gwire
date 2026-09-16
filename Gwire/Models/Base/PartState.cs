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
}
