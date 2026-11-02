using Gwire.Models;
using Gwire.Models.Base;
using Microsoft.AspNetCore.Components;

namespace Gwire.Pages.PartEdit;

public partial class PartPointGroupComponent
{
    [CascadingParameter(Name = "ExecutePartEdit")]
    private EventCallback<Action> ExecuteEdit { get; set; }

    [Parameter, EditorRequired]
    public ConnectionGroup Group { get; set; } = default!;

    [Parameter]
    public EventCallback<ConnectionGroup> GroupChanged { get; set; }

    [Parameter]
    public ConnectionPoint? SelectedPointInEditor { get; set; }

    [Parameter]
    public EventCallback<ConnectionGroup> OnRemove { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    private async Task AddRemovePointToGroup()
    {
        if (SelectedPointInEditor is not { } selectedPoint)
        {
            return;
        }

        await ExecuteEdit.InvokeAsync(() =>
        {
            if (!Group.ConnectedPints.Remove(selectedPoint))
            {
                Group.ConnectedPints.Add(selectedPoint);
            }
        });
        await GroupChanged.InvokeAsync(Group);
    }

    private async Task RemovePoint(ConnectionPoint point)
    {
        await ExecuteEdit.InvokeAsync(() => Group.ConnectedPints.Remove(point));
        await GroupChanged.InvokeAsync(Group);
    }
}
