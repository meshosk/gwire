using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Gwire.Components.Displayers;

public partial class RepositoryLoadingDialog : ComponentBase, IAsyncDisposable
{
    private ElementReference modalElement;
    private IJSObjectReference? module;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Parameter]
    public string? Error { get; set; }

    [Parameter]
    public EventCallback LoadRequested { get; set; }

    [Parameter]
    public EventCallback ContinueRequested { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            module = await JS.InvokeAsync<IJSObjectReference>("import", "./Components/Displayers/RepositoryLoadingDialog.razor.js");
            await module.InvokeVoidAsync("show", modalElement);
            await LoadRequested.InvokeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (module is not null)
        {
            await module.InvokeVoidAsync("dispose", modalElement);
            await module.DisposeAsync();
        }
    }
}
