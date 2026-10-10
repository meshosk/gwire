using Gwire.Models;
using Microsoft.AspNetCore.Components;

namespace Gwire.Components;

public partial class SvgContextMenuItem
{
    [CascadingParameter]
    private SvgContextMenu Menu { get; set; } = default!;

    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; } = default!;

    [Parameter]
    public EventCallback<Point> OnClick { get; set; }

    [Parameter]
    public Func<Point, bool>? CanExecute { get; set; }

    private bool CanRun => Menu.IsOpen && (CanExecute?.Invoke(Menu.menuOpenPosition) ?? true);

    private Task ExecuteAsync() => CanRun ? Menu.ExecuteAsync(OnClick) : Task.CompletedTask;
}
