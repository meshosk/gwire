using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Gwire.Components;

public partial class SvgContextSubmenu
{
    private ElementReference element;
    private bool isExpanded;
    private bool positionMenu;

    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Parameter, EditorRequired] public string Label { get; set; } = string.Empty;
    [Parameter, EditorRequired] public RenderFragment ChildContent { get; set; } = default!;
    [Parameter] public bool Disabled { get; set; }

    private void Toggle()
    {
        if (!Disabled)
        {
            isExpanded = !isExpanded;
            positionMenu = isExpanded;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (positionMenu)
        {
            positionMenu = false;
            await JS.InvokeVoidAsync("gwire.showSvgContextSubmenu", element);
        }
    }
}
