using Gwire.Models;
using Gwire.Models.Base;
using Microsoft.AspNetCore.Components;

namespace Gwire.Pages.PartEdit;

public partial class PartStateGroupComponent
{
    [CascadingParameter(Name = "ExecutePartEdit")]
    private EventCallback<Action> ExecuteEdit { get; set; }

    [Parameter, EditorRequired]
    public List<PartStateGroup> StateGroups { get; set; } = default!;

    [Parameter]
    public EventCallback<List<PartStateGroup>> StateGroupsChanged { get; set; }

    [Parameter]
    public int ActiveStateGroupIndex { get; set; }

    [Parameter]
    public EventCallback<int> ActiveStateGroupIndexChanged { get; set; }

    [Parameter]
    public PartState? ActiveStateInEditor { get; set; }

    [Parameter]
    public EventCallback<PartState?> ActiveStateInEditorChanged { get; set; }

    [Parameter]
    public ConnectionGroup? ActiveConnectionGroup { get; set; }

    [Parameter]
    public EventCallback<ConnectionGroup?> ActiveConnectionGroupChanged { get; set; }

    [Parameter]
    public int ConnectionGroupTabIndex { get; set; }

    [Parameter]
    public EventCallback<int> ConnectionGroupTabIndexChanged { get; set; }

    [Parameter]
    public ConnectionPoint? SelectedPointInEditor { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    private PartStateGroup? ActiveStateGroupInEditor => StateGroups.ElementAtOrDefault(ActiveStateGroupIndex);

    private async Task SelectStateGroup(int index)
    {
        var state = StateGroups[index].ActiveState;
        await ActiveStateGroupIndexChanged.InvokeAsync(index);
        await ActiveStateInEditorChanged.InvokeAsync(state);
        await ActiveConnectionGroupChanged.InvokeAsync(null);
        await ConnectionGroupTabIndexChanged.InvokeAsync(0);
    }

    private async Task AddStateGroup()
    {
        var group = new PartStateGroup();
        var number = 1;
        while (StateGroups.Any(existing => string.Equals(existing.Label, group.Label, StringComparison.OrdinalIgnoreCase)))
        {
            group.Label = $"State {number++}";
        }

        await ExecuteEdit.InvokeAsync(() => StateGroups.Add(group));
        await StateGroupsChanged.InvokeAsync(StateGroups);
        await SelectStateGroup(StateGroups.IndexOf(group));
    }

    private async Task RemoveStateGroup()
    {
        if (StateGroups.Count <= 1)
        {
            return;
        }

        var index = ActiveStateGroupIndex;
        await ExecuteEdit.InvokeAsync(() => StateGroups.RemoveAt(index));
        await StateGroupsChanged.InvokeAsync(StateGroups);
        await SelectStateGroup(Math.Min(index, StateGroups.Count - 1));
    }

    private async Task SetStateGroupLabel(string value)
    {
        if (ActiveStateGroupInEditor is not { } group)
        {
            return;
        }

        await ExecuteEdit.InvokeAsync(() => group.Label = value ?? string.Empty);
        await StateGroupsChanged.InvokeAsync(StateGroups);
    }

    private async Task AddState()
    {
        if (ActiveStateGroupInEditor is not { } group)
        {
            return;
        }

        var state = new PartState { Label = $"State {group.States.Count + 1}" };
        await ExecuteEdit.InvokeAsync(() =>
        {
            group.States.Add(state);
            group.ActiveState = state;
        });
        await StateGroupsChanged.InvokeAsync(StateGroups);
        await ActiveStateInEditorChanged.InvokeAsync(state);
        await ActiveConnectionGroupChanged.InvokeAsync(null);
        await ConnectionGroupTabIndexChanged.InvokeAsync(0);
    }

    private async Task CopyState(PartState state)
    {
        if (ActiveStateGroupInEditor is not { } group)
        {
            return;
        }

        var copy = new PartState
        {
            Label = $"{state.Label} copy",
            ConnectionGroups = state.ConnectionGroups.Select(connectionGroup => new ConnectionGroup
            {
                ConnectedPints = connectionGroup.ConnectedPints.ToList()
            }).ToList()
        };
        await ExecuteEdit.InvokeAsync(() => group.States.Add(copy));
        await StateGroupsChanged.InvokeAsync(StateGroups);
    }

    private async Task RemoveState(PartState state)
    {
        if (ActiveStateGroupInEditor is not { } group || !group.States.Contains(state))
        {
            return;
        }

        var removeActiveState = ReferenceEquals(ActiveStateInEditor, state) || ReferenceEquals(group.ActiveState, state);
        var removeActiveGroup = ActiveConnectionGroup is not null && state.ConnectionGroups.Contains(ActiveConnectionGroup);
        await ExecuteEdit.InvokeAsync(() =>
        {
            group.States.Remove(state);
            if (removeActiveState)
            {
                group.ActiveState = group.States.FirstOrDefault();
            }
        });
        await StateGroupsChanged.InvokeAsync(StateGroups);
        if (removeActiveState)
        {
            await ActiveStateInEditorChanged.InvokeAsync(group.ActiveState);
        }
        if (removeActiveGroup)
        {
            await ActiveConnectionGroupChanged.InvokeAsync(null);
            await ConnectionGroupTabIndexChanged.InvokeAsync(0);
        }
    }

    private async Task SelectActiveState(PartState state)
    {
        if (ActiveStateGroupInEditor is not { } group)
        {
            return;
        }

        group.ActiveState = state;
        await StateGroupsChanged.InvokeAsync(StateGroups);
        await ActiveStateInEditorChanged.InvokeAsync(state);
        await ConnectionGroupTabIndexChanged.InvokeAsync(0);
    }
}
