using Gwire.Models;
using Gwire.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Gwire.Components;

public partial class ConfirmationModal : ComponentBase, IAsyncDisposable
{
    private ConfirmationRequest? request;
    private ElementReference modalElement;
    private IJSObjectReference? module;
    private DotNetObjectReference<ConfirmationModal>? receiver;
    private bool showRequested;
    private bool responding;

    [Inject]
    private ConfirmationService ConfirmationService { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    protected override void OnInitialized() => ConfirmationService.ConfirmationShown += Show;

    private void Show(ConfirmationRequest request)
    {
        this.request = request;
        responding = false;
        showRequested = true;
        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/confirmation-modal.js");
            receiver = DotNetObjectReference.Create(this);
        }

        if (showRequested && module is not null)
        {
            showRequested = false;
            await module.InvokeVoidAsync("show", modalElement, receiver);
        }
    }

    private async Task RespondAsync(ConfirmationResult result)
    {
        if (!responding && request is not null && module is not null)
        {
            responding = true;
            await module.InvokeVoidAsync("hide", modalElement, result);
        }
    }

    [JSInvokable]
    public void Complete(ConfirmationResult result)
    {
        var completed = request;
        request = null;
        completed?.Complete(result);
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        ConfirmationService.ConfirmationShown -= Show;
        request?.Complete(ConfirmationResult.NO);
        if (module is not null)
        {
            await module.InvokeVoidAsync("dispose", modalElement);
            await module.DisposeAsync();
        }
        receiver?.Dispose();
    }
}
