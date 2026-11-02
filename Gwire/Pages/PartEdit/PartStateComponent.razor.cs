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
    public int ConnectionGroupTabIndex { get; set; }

    [Parameter]
    public EventCallback<int> ConnectionGroupTabIndexChanged { get; set; }

    [Parameter]
    public EventCallback<PartState> OnCopy { get; set; }

    [Parameter]
    public EventCallback<PartState> OnRemove { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

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
            await ActiveConnectionGroupChanged.InvokeAsync(null);
            await ConnectionGroupTabIndexChanged.InvokeAsync(0);
        }
    }
}
