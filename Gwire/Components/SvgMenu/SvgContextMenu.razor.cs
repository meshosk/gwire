using Gwire.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Gwire.Components.SvgMenu;

public partial class SvgContextMenu : IAsyncDisposable
{
    // Keep the .NET receiver alive until any pending attachment has completed and its listener is detached.
    private readonly SemaphoreSlim subscriptionLock = new(1, 1);
    private ElementReference overlay;
    private DotNetObjectReference<SvgContextMenu>? receiver;
    private IJSObjectReference? subscription;
    private string? attachedTarget;
    private Point pointerPosition;
    private bool focusMenu;
    private bool disposed;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; } = default!;

    /// <summary>
    /// CSS selector of the SVG element whose right-click opens this menu.
    /// Render the menu outside pointer capture groups so its items remain clickable.
    /// </summary>
    [Parameter, EditorRequired]
    public string Target { get; set; } = string.Empty;

    internal bool IsOpen { get; private set; }
    internal Point menuOpenPosition;

    [JSInvokable]
    public Task Open(float clientX, float clientY, float svgX, float svgY) => InvokeAsync(() =>
    {
        if (disposed)
        {
            return;
        }

        pointerPosition = new Point(clientX, clientY);
        menuOpenPosition = new Point(svgX, svgY);
        IsOpen = true;
        focusMenu = true;
        StateHasChanged();
    });

    internal async Task ExecuteAsync(EventCallback<Point> action)
    {
        var position = menuOpenPosition;
        await DismissAsync();
        await action.InvokeAsync(position);
    }

    private async Task DismissAsync()
    {
        if (disposed)
        {
            return;
        }

        IsOpen = false;
        focusMenu = false;
        StateHasChanged();
        await subscriptionLock.WaitAsync();
        try
        {
            if (!disposed && subscription is not null)
            {
                await subscription.InvokeVoidAsync("restoreFocus");
            }
        }
        finally
        {
            subscriptionLock.Release();
        }
    }

    private Task HandleKeyDown(KeyboardEventArgs eventArgs) =>
        eventArgs.Key == "Escape" ? DismissAsync() : Task.CompletedTask;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await subscriptionLock.WaitAsync();
        try
        {
            if (disposed)
            {
                return;
            }

            if (attachedTarget != Target)
            {
                if (subscription is not null)
                {
                    await subscription.InvokeVoidAsync("detach");
                    await subscription.DisposeAsync();
                    subscription = null;
                    attachedTarget = null;
                }

                if (disposed)
                {
                    return;
                }

                var target = Target;
                receiver ??= DotNetObjectReference.Create(this);
                subscription = await JS.InvokeAsync<IJSObjectReference>("gwire.attachSvgContextMenu", target, receiver);
                attachedTarget = target;
            }
            if (!disposed && focusMenu && IsOpen)
            {
                focusMenu = false;
                await JS.InvokeVoidAsync("gwire.showSvgContextMenu", overlay, pointerPosition.X, pointerPosition.Y);
            }
        }
        finally
        {
            subscriptionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        await subscriptionLock.WaitAsync();
        try
        {
            if (subscription is not null)
            {
                await subscription.InvokeVoidAsync("detach");
                await subscription.DisposeAsync();
                subscription = null;
                attachedTarget = null;
            }
            receiver?.Dispose();
            receiver = null;
        }
        finally
        {
            subscriptionLock.Release();
        }
    }
}
