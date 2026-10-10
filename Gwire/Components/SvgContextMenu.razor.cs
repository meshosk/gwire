using Gwire.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Gwire.Components;

public partial class SvgContextMenu : IAsyncDisposable
{
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
        try
        {
            await action.InvokeAsync(menuOpenPosition);
        }
        finally
        {
            await DismissAsync();
        }
    }

    private async Task DismissAsync()
    {
        IsOpen = false;
        focusMenu = false;
        StateHasChanged();
        if (subscription is not null)
        {
            await subscription.InvokeVoidAsync("restoreFocus");
        }
    }

    private Task HandleKeyDown(KeyboardEventArgs eventArgs) =>
        eventArgs.Key == "Escape" ? DismissAsync() : Task.CompletedTask;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (attachedTarget != Target)
        {
            if (subscription is not null)
            {
                await subscription.InvokeVoidAsync("detach");
                await subscription.DisposeAsync();
            }

            receiver ??= DotNetObjectReference.Create(this);
            subscription = await JS.InvokeAsync<IJSObjectReference>("gwire.attachSvgContextMenu", Target, receiver);
            attachedTarget = Target;
        }
        if (focusMenu && IsOpen)
        {
            focusMenu = false;
            await JS.InvokeVoidAsync("gwire.showSvgContextMenu", overlay, pointerPosition.X, pointerPosition.Y);
        }
    }

    public async ValueTask DisposeAsync()
    {
        disposed = true;
        if (subscription is not null)
        {
            await subscription.InvokeVoidAsync("detach");
            await subscription.DisposeAsync();
        }
        receiver?.Dispose();
    }
}
