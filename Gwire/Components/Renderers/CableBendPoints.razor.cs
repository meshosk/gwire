using Gwire.Models;
using Gwire.Services;
using Gwire.UndoRedo;
using Gwire.UndoRedo.Actions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Gwire.Components.Renderers;

public partial class CableBendPoints : IDisposable
{
    private ElementReference element;
    private readonly string elementId = $"cable-bends-{Guid.NewGuid():N}";
    private int? contextIndex;
    private int? draggedIndex;
    private long? dragPointerId;
    private Point dragStartClient;
    private CableBendPoint dragStartPoint;
    private CableBendPoint[] dragStartPoints = [];

    [Parameter]
    public float Zoom { get; set; } = 1;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Parameter, EditorRequired]
    public CablePart CablePart { get; set; } = default!;

    [Inject]
    private UndoRedoService UndoRedoService { get; set; } = default!;

    private UndoRedoService.UndoRedoHistoryWrapper History => UndoRedoService.getHistory(HistoryIds.SchemeEditor);


    [Parameter]
    public bool IsSelected { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("gwire.enablePointerCapture", element);
        }
    }

    private void ChangeBendStyle(bool isRounded)
    {
        if (History.IsFrozen || contextIndex is not { } index || index >= CablePart.BendPoints.Count)
        {
            return;
        }

        var bends = CablePart.BendPoints.ToList();
        bends[index] = bends[index] with { IsRounded = isRounded };
        var action = new ChangeCableRouteAction(CablePart, CablePart.BendPoints, bends);
        if (action.HasChanges)
        {
            action.Redo();
            History.addRedoAction(action);
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
        CablePart.BendPoints[index] = new CableBendPoint(dragStartPoint.X + (pointer.X - dragStartClient.X) / Zoom,
            dragStartPoint.Y + (pointer.Y - dragStartClient.Y) / Zoom, dragStartPoint.IsRounded);
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
            History.addRedoAction(action);
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
