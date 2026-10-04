using Gwire.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text;

namespace Gwire.Components;

public partial class CircuitPartRenderer : IDisposable
{
    private ElementReference element;
    private CircuitPart? subscribedPart;
    private long? dragPointerId;
    private Point dragStartClient;
    private Point dragStartPart;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Parameter, EditorRequired]
    public CircuitPart CircuitPart { get; set; } = default!;

    private string PartTransform => $"translate({CircuitPart.SchemePosition.X} {CircuitPart.SchemePosition.Y})";

    private string? SvgImageSource => string.IsNullOrWhiteSpace(CircuitPart.SvgMarkup)
        ? null
        : $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(CircuitPart.SvgMarkup))}";

    protected override void OnParametersSet()
    {
        if (ReferenceEquals(subscribedPart, CircuitPart))
        {
            return;
        }

        if (subscribedPart is not null)
        {
            subscribedPart.Changed -= HandleChanged;
        }
        subscribedPart = CircuitPart;
        subscribedPart.Changed += HandleChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("gwire.enablePointerCapture", element);
        }
    }

    private void HandleChanged() => _ = InvokeAsync(StateHasChanged);

    private void StartDrag(PointerEventArgs eventArgs)
    {
        if (eventArgs.Button != 0 || !eventArgs.IsPrimary || dragPointerId is not null)
        {
            return;
        }

        dragPointerId = eventArgs.PointerId;
        dragStartClient = new Point(eventArgs);
        dragStartPart = CircuitPart.SchemePosition.Copy();
    }

    private void MoveDraggedPart(PointerEventArgs eventArgs)
    {
        if (dragPointerId != eventArgs.PointerId)
        {
            return;
        }

        var pointer = new Point(eventArgs);
        CircuitPart.MoveTo(new Point(dragStartPart.X + pointer.X - dragStartClient.X,
            dragStartPart.Y + pointer.Y - dragStartClient.Y));
    }

    private void EndDrag(PointerEventArgs eventArgs)
    {
        MoveDraggedPart(eventArgs);
        CancelDrag(eventArgs);
    }

    private void CancelDrag(PointerEventArgs eventArgs)
    {
        if (dragPointerId == eventArgs.PointerId)
        {
            dragPointerId = null;
        }
    }

    public void Dispose()
    {
        if (subscribedPart is not null)
        {
            subscribedPart.Changed -= HandleChanged;
        }
    }
}
