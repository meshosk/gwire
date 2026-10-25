namespace Gwire.Models.Base;

/// <summary>
/// An independently switchable set of states within a part.
/// </summary>
public sealed class PartStateGroup
{
    public string Label { get; set; } = "State";

    public List<PartState> States { get; set; } = new();

    public PartState? ActiveState { get; set; }
}
