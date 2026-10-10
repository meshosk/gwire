using Gwire.Models;
using Gwire.Models.Base;
using Gwire.UndoRedo;
using Gwire.UndoRedo.Actions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Gwire.Components;

public partial class CablePartRenderer : IDisposable
{
    private ElementReference elementReference;
    private readonly string elementReferenceId = $"cable-{Guid.NewGuid():N}";
    private bool wasDragged;
    private ConnectionPoint? draggedPoint;
    private ConnectionPoint? originalConnection;
    private ConnectionPoint[] dragStartConnections = [];
    private ConnectionPoint? connectionTarget;
    private long? dragPointerId;
    private Point dragStartClient;
    private Point dragStartPoint;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Parameter, EditorRequired]
    public UndoRedoHistory History { get; set; } = default!;

    [Parameter]
    public bool IsSelected { get; set; }

    [Parameter]
    public EventCallback<bool> IsSelectedChanged { get; set; }

    [Parameter, EditorRequired]
    public CablePart CablePart { get; set; } = default!;

    [Parameter, EditorRequired]
    public Circuit Circuit { get; set; } = default!;

    private IEnumerable<Point> RoutePoints => CablePart.BendPoints
        .Prepend(CablePart.Points[0].LocalPosition)
        .Append(CablePart.Points[1].LocalPosition);

    private string CablePath => string.Join(" ", RoutePoints.Select(point =>
        FormattableString.Invariant($"{point.X},{point.Y}")));

    private Task SelectContextTarget() => History.IsFrozen || IsSelected
        ? Task.CompletedTask
        : IsSelectedChanged.InvokeAsync(true);

    private bool CanAddBendPoint(Point position) =>
        !History.IsFrozen && CablePart.FindBendPointInsertion(position) is not null;

    private void AddBendPoint(Point position)
    {
        if (History.IsFrozen || CablePart.FindBendPointInsertion(position) is not { } bend)
        {
            return;
        }

        var bends = CablePart.BendPoints.ToList();
        bends.Insert(bend.Index, bend.Position);
        History.Execute(new ChangeCableRouteAction(CablePart, CablePart.BendPoints, bends));
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var previousCablePart = CablePart;
        await base.SetParametersAsync(parameters);

        if (ReferenceEquals(previousCablePart, CablePart))
        {
            return;
        }

        if (previousCablePart is not null)
        {
            previousCablePart.Changed -= HandleChanged;
        }
        CablePart.Changed += HandleChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("gwire.enablePointerCapture", elementReference);
        }
    }

    private void PrepareSelection(PointerEventArgs eventArgs) => wasDragged = false;

    private Task Select() => wasDragged || IsSelected ? Task.CompletedTask : IsSelectedChanged.InvokeAsync(true);

    private void HandleChanged() => StateHasChanged();

    private void StartDrag(ConnectionPoint point, PointerEventArgs eventArgs)
    {
        if (eventArgs.Button != 0 || !eventArgs.IsPrimary || dragPointerId is not null)
        {
            return;
        }

        draggedPoint = point;
        dragPointerId = eventArgs.PointerId;
        wasDragged = false;
        dragStartClient = new Point(eventArgs);
        dragStartPoint = point.LocalPosition.Copy();
        dragStartConnections = point.ConnectionPoints.ToArray();
        History.Freeze();
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
        if (dragPointerId != eventArgs.PointerId)
        {
            return;
        }

        var pointer = new Point(eventArgs);
        wasDragged |= MathF.Abs(pointer.X - dragStartClient.X) > 2 ||
            MathF.Abs(pointer.Y - dragStartClient.Y) > 2;
        var position = new Point(dragStartPoint.X + pointer.X - dragStartClient.X,
            dragStartPoint.Y + pointer.Y - dragStartClient.Y);
        if (draggedPoint is not null)
        {
            draggedPoint.LocalPosition = position;
            UpdateConnectionTarget();
        }
    }

    private void EndDrag(PointerEventArgs eventArgs)
    {
        if (dragPointerId != eventArgs.PointerId)
        {
            return;
        }

        MoveDraggedPoint(eventArgs);
        if (draggedPoint is not null)
        {
            if (connectionTarget is not null)
            {
                draggedPoint.Connect(connectionTarget);
            }
            var action = new MoveCablePointAction(draggedPoint, dragStartPoint, dragStartConnections);
            if (action.HasChanges)
            {
                History.Record(action);
            }
        }
        ResetDrag();
    }

    private void CancelDrag(PointerEventArgs eventArgs)
    {
        if (dragPointerId != eventArgs.PointerId)
        {
            return;
        }

        if (draggedPoint is not null)
        {
            new MoveCablePointAction(draggedPoint, dragStartPoint, dragStartConnections).Undo();
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
        dragStartConnections = [];
        History.Unfreeze();
    }

    public void Dispose()
    {
        if (CablePart is not null)
        {
            CablePart.Changed -= HandleChanged;
        }
        if (dragPointerId is { } pointerId)
        {
            CancelDrag(new PointerEventArgs { PointerId = pointerId });
        }
    }
}
