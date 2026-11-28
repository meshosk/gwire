using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Gwire.Components.Dialogs;

public partial class ShareLinkDialog : ComponentBase, IAsyncDisposable
{
    private ElementReference modalElement;
    private IJSObjectReference? module;
    private DotNetObjectReference<ShareLinkDialog>? receiver;
    private bool showRequested;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Parameter]
    public string? Link { get; set; }

    [Parameter]
    public EventCallback<string?> LinkChanged { get; set; }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var previousLink = Link;
        await base.SetParametersAsync(parameters);
        if (previousLink == Link)
        {
            return;
        }

        if (Link is not null)
        {
            showRequested |= previousLink is null;
        }
        else
        {
            showRequested = false;
            if (module is not null)
            {
                await module.InvokeVoidAsync("hide", modalElement, 0);
            }
        }
        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (showRequested)
        {
            showRequested = false;
            module ??= await JS.InvokeAsync<IJSObjectReference>("import", "./js/confirmation-modal.js");
            receiver ??= DotNetObjectReference.Create(this);
            if (Link is not null)
            {
                await module.InvokeVoidAsync("show", modalElement, receiver);
            }
        }
    }

    private async Task CopyAsync()
    {
        if (Link is not null)
        {
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", Link);
        }
    }

    [JSInvokable]
    public Task Complete(int result) => LinkChanged.InvokeAsync(null);

    public async ValueTask DisposeAsync()
    {
        if (module is not null)
        {
            await module.InvokeVoidAsync("dispose", modalElement);
            await module.DisposeAsync();
        }
        receiver?.Dispose();
    }
}
