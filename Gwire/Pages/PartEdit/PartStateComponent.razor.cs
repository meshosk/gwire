using Gwire.Models;
using Gwire.Models.Base;
using Microsoft.AspNetCore.Components;

namespace Gwire.Pages.PartEdit;

public partial class PartStateComponent
{
    [CascadingParameter(Name = "ExecutePartEdit")]
    private EventCallback<Action> ExecuteEdit { get; set; }

    [Parameter, EditorRequired]
    public PartState State { get; set; } = default!;

    [Parameter]
    public EventCallback<PartState> StateChanged { get; set; }

    [Parameter]
    public ConnectionPoint? SelectedPointInEditor { get; set; }

    [Parameter]
    public ConnectionGroup? ActiveConnectionGroup { get; set; }

    [Parameter]
    public EventCallback<ConnectionGroup?> ActiveConnectionGroupChanged { get; set; }

    [Parameter]
    public EventCallback<PartState> OnCopy { get; set; }

    [Parameter]
    public EventCallback<PartState> OnRemove { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// The connection group tab follows the active connection group, whose points canvas clicks toggle.
    /// </summary>
    private int ActiveConnectionGroupIndex => ActiveConnectionGroup is null
        ? 0 : Math.Max(0, State.ConnectionGroups.IndexOf(ActiveConnectionGroup));

    private Task SelectConnectionGroup(int index) =>
        ActiveConnectionGroupChanged.InvokeAsync(State.ConnectionGroups.ElementAtOrDefault(index));

    private async Task SetStateLabel(string value)
    {
        await ExecuteEdit.InvokeAsync(() => State.Label = value ?? string.Empty);
        await StateChanged.InvokeAsync(State);
    }

    private async Task AddConnectionGroup()
    {
        var group = new ConnectionGroup();
        await ExecuteEdit.InvokeAsync(() => State.ConnectionGroups.Add(group));
        await StateChanged.InvokeAsync(State);
        await ActiveConnectionGroupChanged.InvokeAsync(group);
    }

    private async Task RemoveActiveConnectionGroup(ConnectionGroup group)
    {
        if (!State.ConnectionGroups.Contains(group))
        {
            return;
        }

        var removeActiveGroup = ReferenceEquals(ActiveConnectionGroup, group);
        await ExecuteEdit.InvokeAsync(() => State.ConnectionGroups.Remove(group));
        await StateChanged.InvokeAsync(State);
        if (removeActiveGroup)
        {
            await ActiveConnectionGroupChanged.InvokeAsync(State.ConnectionGroups.FirstOrDefault());
        }
    }
}
