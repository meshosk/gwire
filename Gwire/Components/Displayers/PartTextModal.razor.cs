using Gwire.Models;
using Gwire.Services;
using Gwire.UndoRedo;
using Gwire.UndoRedo.Actions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Gwire.Components.Displayers;

public partial class PartTextModal : ComponentBase, IAsyncDisposable
{
    private string draftValue = string.Empty;
    private ElementReference modalElement;
    private IJSObjectReference? module;
    private DotNetObjectReference<PartTextModal>? receiver;
    private bool showRequested;
    private bool responding;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private UndoRedoService UndoRedoService { get; set; } = default!;

    private UndoRedoService.UndoRedoHistoryWrapper History => UndoRedoService.getHistory(HistoryIds.SchemeEditor);


    [Parameter]
    public CircuitPart? Part { get; set; }

    [Parameter]
    public EventCallback<CircuitPart?> PartChanged { get; set; }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var previousPart = Part;
        await base.SetParametersAsync(parameters);
        if (ReferenceEquals(previousPart, Part))
        {
            return;
        }

        if (Part is { } part)
        {
            draftValue = part.CustomTextValue ?? string.Empty;
            responding = false;
            showRequested |= previousPart is null;
            if (previousPart is null)
            {
                History.Freeze();
            }
        }
        else
        {
            showRequested = false;
            if (module is not null)
            {
                await module.InvokeVoidAsync("hide", modalElement, 0);
            }
            History.Unfreeze();
        }
        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (showRequested)
        {
            showRequested = false;
            module ??= await JS.InvokeAsync<IJSObjectReference>("import", "./js/part-text-modal.js");
            receiver ??= DotNetObjectReference.Create(this);
            if (Part is not null)
            {
                await module.InvokeVoidAsync("show", modalElement, receiver);
            }
        }
    }

    private Task SaveAsync() => RespondAsync(1);
    private Task CancelAsync() => RespondAsync(0);

    private async Task RespondAsync(int result)
    {
        if (!responding && Part is not null && module is not null)
        {
            responding = true;
            await module.InvokeVoidAsync("hide", modalElement, result);
        }
    }

    [JSInvokable]
    public async Task Complete(int result)
    {
        if (Part is not { } part)
        {
            return;
        }

        if (result == 1 && part.CustomTextValue is not null && part.CustomTextValue != draftValue)
        {
            var action = new ChangePartValueAction(part, draftValue);
            action.Redo();
            History.addRedoAction(action);
        }
        await PartChanged.InvokeAsync(null);
    }

    public async ValueTask DisposeAsync()
    {
        if (module is not null)
        {
            await module.InvokeVoidAsync("dispose", modalElement);
            await module.DisposeAsync();
        }
        receiver?.Dispose();
        if (Part is not null)
        {
            History.Unfreeze();
        }
    }
}
