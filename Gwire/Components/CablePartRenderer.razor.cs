using Gwire.Models;
using Gwire.Models.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Gwire.Components;

public partial class CablePartRenderer : IDisposable
{
    private ElementReference element;
    private bool wasDragged;
    private CablePart? subscribedCable;
    private ConnectionPoint? draggedPoint;
    private ConnectionPoint? originalConnection;
    private ConnectionPoint? connectionTarget;
    private long? dragPointerId;
    private Point dragStartClient;
    private Point dragStartPoint;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Parameter]
    public bool IsSelected { get; set; }

    [Parameter]
    public EventCallback<bool> IsSelectedChanged { get; set; }

    [Parameter, EditorRequired]
    public CablePart CablePart { get; set; } = default!;

    [Parameter, EditorRequired]
    public Circuit Circuit { get; set; } = default!;

    protected override void OnParametersSet()
    {
        if (ReferenceEquals(subscribedCable, CablePart))
        {
            return;
        }

        if (subscribedCable is not null)
        {
            subscribedCable.Changed -= HandleChanged;
        }
        subscribedCable = CablePart;
        subscribedCable.Changed += HandleChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("gwire.enablePointerCapture", element);
        }
    }

    private void PrepareSelection(PointerEventArgs eventArgs) => wasDragged = false;

    private Task Select() => wasDragged || IsSelected ? Task.CompletedTask : IsSelectedChanged.InvokeAsync(true);

    private void HandleChanged() => _ = InvokeAsync(StateHasChanged);

    private void StartDrag(ConnectionPoint point, PointerEventArgs eventArgs)
    {
        if (eventArgs.Button != 0 || !eventArgs.IsPrimary || draggedPoint is not null)
        {
            return;
        }

        draggedPoint = point;
        dragPointerId = eventArgs.PointerId;
        wasDragged = false;
        dragStartClient = new Point(eventArgs);
        dragStartPoint = point.LocalPosition.Copy();
        originalConnection = point.ConnectionPoints.FirstOrDefault(connection => connection.Owner is CircuitPart);
        if (originalConnection is not null)
        {
            point.Disconnect(originalConnection);
        }
        UpdateConnectionTarget();
    }

    private void UpdateConnectionTarget()
    {
        if (draggedPoint is null)
        {
            return;
        }

        ConnectionPoint? target = null;
        // Both pin circles have a radius of 10 in scheme coordinates.
        var closestDistanceSquared = 20f * 20f;
        foreach (var part in Circuit.Parts.OfType<CircuitPart>())
        {
            foreach (var point in part.Points)
            {
                var delta = new Point(
                    draggedPoint.LocalPosition.X - (part.SchemePosition.X + point.LocalPosition.X),
                    draggedPoint.LocalPosition.Y - (part.SchemePosition.Y + point.LocalPosition.Y));
                var distanceSquared = delta.X * delta.X + delta.Y * delta.Y;
                if (distanceSquared <= closestDistanceSquared &&
                    (target is null || distanceSquared < closestDistanceSquared ||
                     ReferenceEquals(point, originalConnection)))
                {
                    closestDistanceSquared = distanceSquared;
                    target = point;
                }
            }
        }

        if (!ReferenceEquals(connectionTarget, target))
        {
            if (connectionTarget is not null)
            {
                connectionTarget.IsHighlighted = false;
            }
            connectionTarget = target;
            if (connectionTarget is not null)
            {
                connectionTarget.IsHighlighted = true;
            }
        }
        draggedPoint.IsHighlighted = connectionTarget is not null;
    }

    private void MoveDraggedPoint(PointerEventArgs eventArgs)
    {
        if (draggedPoint is null || dragPointerId != eventArgs.PointerId)
        {
            return;
        }

        var pointer = new Point(eventArgs);
        wasDragged |= MathF.Abs(pointer.X - dragStartClient.X) > 2 ||
            MathF.Abs(pointer.Y - dragStartClient.Y) > 2;
        draggedPoint.LocalPosition = new Point(dragStartPoint.X + pointer.X - dragStartClient.X,
            dragStartPoint.Y + pointer.Y - dragStartClient.Y);
        UpdateConnectionTarget();
    }

    private void EndDrag(PointerEventArgs eventArgs)
    {
        if (draggedPoint is null || dragPointerId != eventArgs.PointerId)
        {
            return;
        }

        MoveDraggedPoint(eventArgs);
        if (connectionTarget is not null)
        {
            draggedPoint.Connect(connectionTarget);
        }
        ResetDrag();
    }

    private void CancelDrag(PointerEventArgs eventArgs)
    {
        if (draggedPoint is null || dragPointerId != eventArgs.PointerId)
        {
            return;
        }

        if (originalConnection is not null)
        {
            draggedPoint.Connect(originalConnection);
        }
        ResetDrag();
    }

    private void ResetDrag()
    {
        if (connectionTarget is not null)
        {
            connectionTarget.IsHighlighted = false;
        }
        if (draggedPoint is not null)
        {
            draggedPoint.IsHighlighted = false;
        }
        draggedPoint = null;
        originalConnection = null;
        connectionTarget = null;
        dragPointerId = null;
    }

    public void Dispose()
    {
        if (subscribedCable is not null)
        {
            subscribedCable.Changed -= HandleChanged;
        }
        if (dragPointerId is { } pointerId)
        {
            CancelDrag(new PointerEventArgs { PointerId = pointerId });
        }
    }
}
