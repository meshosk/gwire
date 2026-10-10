using Gwire.Models;
using Gwire.UndoRedo;
using Gwire.UndoRedo.Actions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Gwire.Components;

public partial class CableBendPoints : IDisposable
{
    private ElementReference element;
    private int? draggedIndex;
    private long? dragPointerId;
    private Point dragStartClient;
    private Point dragStartPoint;
    private Point[] dragStartPoints = [];

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Parameter, EditorRequired]
    public CablePart CablePart { get; set; } = default!;

    [Parameter, EditorRequired]
    public UndoRedoHistory History { get; set; } = default!;

    [Parameter]
    public bool IsSelected { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("gwire.enablePointerCapture", element);
        }
    }

    private void StartDrag(int index, PointerEventArgs eventArgs)
    {
        if (!IsSelected || eventArgs.Button != 0 || !eventArgs.IsPrimary || dragPointerId is not null)
        {
            return;
        }

        draggedIndex = index;
        dragPointerId = eventArgs.PointerId;
        dragStartClient = new Point(eventArgs);
        dragStartPoint = CablePart.BendPoints[index].Copy();
        dragStartPoints = CablePart.BendPoints.ToArray();
        History.Freeze();
    }

    private void MoveDraggedPoint(PointerEventArgs eventArgs)
    {
        if (dragPointerId != eventArgs.PointerId || draggedIndex is not { } index)
        {
            return;
        }

        var pointer = new Point(eventArgs);
        CablePart.BendPoints[index] = new Point(dragStartPoint.X + pointer.X - dragStartClient.X,
            dragStartPoint.Y + pointer.Y - dragStartClient.Y);
        CablePart.NotifyChanged();
    }

    private void EndDrag(PointerEventArgs eventArgs)
    {
        if (dragPointerId != eventArgs.PointerId)
        {
            return;
        }

        MoveDraggedPoint(eventArgs);
        var action = new ChangeCableRouteAction(CablePart, dragStartPoints, CablePart.BendPoints);
        if (action.HasChanges)
        {
            History.Record(action);
        }
        ResetDrag();
    }

    private void CancelDrag(PointerEventArgs eventArgs)
    {
        if (dragPointerId != eventArgs.PointerId)
        {
            return;
        }

        new ChangeCableRouteAction(CablePart, dragStartPoints, CablePart.BendPoints).Undo();
        ResetDrag();
    }

    private void ResetDrag()
    {
        draggedIndex = null;
        dragPointerId = null;
        dragStartPoints = [];
        History.Unfreeze();
    }

    public void Dispose()
    {
        if (dragPointerId is { } pointerId)
        {
            CancelDrag(new PointerEventArgs { PointerId = pointerId });
        }
    }
}
