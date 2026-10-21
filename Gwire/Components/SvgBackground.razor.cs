using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Gwire.Components;

public partial class SvgBackground
{
    private ElementReference background;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Parameter, EditorRequired]
    public string Source { get; set; } = string.Empty;

    [Parameter]
    public int Width { get; set; }

    [Parameter]
    public int Height { get; set; }

    [Parameter]
    public int Rotation { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender) =>
        await JS.InvokeVoidAsync("gwire.fitSvgBackground", background, Source, Width, Height, Rotation);
}
