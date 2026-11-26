using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Services;
using Gwire.UndoRedo;
using Gwire.UndoRedo.Actions;
using Gwire.Models.Signal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Gwire.Components.Renderers;

public partial class ConnectionPointRenderer : IDisposable
{
    private ElementReference element;
    private long? pointerId;
    private Point startClient;
    private Point startPosition;
    private bool wasDragged;
    private ConnectionPoint[] startConnections = [];
    private ConnectionPoint? originalConnection;
    private ConnectionPoint? connectionTarget;
    private bool showSignalBubble;

    [Parameter, EditorRequired]
    public ConnectionPoint Point { get; set; } = default!;

    [Parameter]
    public Circuit? Circuit { get; set; }

    [Parameter]
    public float Zoom { get; set; } = 1;

    [Parameter]
    public bool IsSelected { get; set; }

    [CascadingParameter]
    private SignalTraceResult? SignalTrace { get; set; }

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private UndoRedoService UndoRedoService { get; set; } = default!;

    private UndoRedoService.UndoRedoHistoryWrapper History => UndoRedoService.getHistory(HistoryIds.SchemeEditor);

    private string PointClass => $"connection-point{(Point.IsHighlighted ? " connection-highlight" : "")}{(IsSelected ? " cable-selected" : "")}";

    private string SignalBubbleTransform
    {
        get
        {
            var position = Point.LocalPosition;
            if (Point.Owner is CircuitPart part)
            {
                position = new Point(position.X + part.SchemePosition.X, position.Y + part.SchemePosition.Y);
            }

            return FormattableString.Invariant($"translate({position.X} {position.Y - 10}) scale({1 / Zoom})");
        }
    }

    protected override void OnParametersSet()
    {
        if (SignalTrace is null)
        {
            showSignalBubble = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("gwire.enablePointerCapture", element);
        }
    }

    private void StartGesture(PointerEventArgs args)
    {
        if (args.Button != 0 || !args.IsPrimary || pointerId is not null)
        {
            return;
        }

        wasDragged = false;
        if (History.IsFrozen)
        {
            return;
        }

        pointerId = args.PointerId;
        startClient = new Point(args);
    }

    private void MoveGesture(PointerEventArgs args)
    {
        if (pointerId != args.PointerId)
        {
            return;
        }

        var deltaX = (float)args.ClientX - startClient.X;
        var deltaY = (float)args.ClientY - startClient.Y;
        if (!wasDragged)
        {
            if (History.IsFrozen || (MathF.Abs(deltaX) <= 2 && MathF.Abs(deltaY) <= 2))
            {
                return;
            }

            startPosition = Point.Owner is CircuitPart part ? part.SchemePosition.Copy() : Point.LocalPosition.Copy();
            wasDragged = true;
            History.Freeze();
            if (Point.Owner is CablePart)
            {
                startConnections = Point.ConnectionPoints.ToArray();
                originalConnection = startConnections.FirstOrDefault(connection => connection.Owner is CircuitPart);
                if (originalConnection is not null)
                {
                    Point.Disconnect(originalConnection);
                }
            }
        }

        var position = new Point(startPosition.X + deltaX / Zoom, startPosition.Y + deltaY / Zoom);
        if (Point.Owner is CircuitPart circuitPart)
        {
            circuitPart.MoveTo(position);
        }
        else
        {
            Point.LocalPosition = position;
            UpdateConnectionTarget();
            Point.Owner.NotifyChanged();
        }
    }

    private void EndGesture(PointerEventArgs args)
    {
        if (pointerId != args.PointerId)
        {
            return;
        }

        try
        {
            MoveGesture(args);
            if (!wasDragged)
            {
                if (SignalTrace is not null && !History.IsFrozen)
                {
                    showSignalBubble = !showSignalBubble;
                }
                return;
            }

            if (Point.Owner is CircuitPart part)
            {
                if (!startPosition.Equals(part.SchemePosition))
                {
                    History.addRedoAction(new MovePartAction(part, startPosition, part.SchemePosition.Copy()));
                }
            }
            else
            {
                if (connectionTarget is not null)
                {
                    Point.Connect(connectionTarget);
                }
                var action = new MoveCablePointAction(Point, startPosition, startConnections);
                if (action.HasChanges)
                {
                    History.addRedoAction(action);
                }
            }
        }
        finally
        {
            ResetGesture();
        }
    }

    private void UpdateConnectionTarget()
    {
        ConnectionPoint? target = null;
        var closestDistanceSquared = 20f * 20f;
        if (Circuit is not null)
        {
            foreach (var part in Circuit.Parts.OfType<CircuitPart>())
            {
                foreach (var candidate in part.Points)
                {
                    var deltaX = Point.LocalPosition.X - (part.SchemePosition.X + candidate.LocalPosition.X);
                    var deltaY = Point.LocalPosition.Y - (part.SchemePosition.Y + candidate.LocalPosition.Y);
                    var distanceSquared = deltaX * deltaX + deltaY * deltaY;
                    if (distanceSquared <= closestDistanceSquared &&
                        (target is null || distanceSquared < closestDistanceSquared || ReferenceEquals(candidate, originalConnection)))
                    {
                        closestDistanceSquared = distanceSquared;
                        target = candidate;
                    }
                }
            }
        }

        if (!ReferenceEquals(connectionTarget, target))
        {
            connectionTarget?.IsHighlighted = false;
            connectionTarget = target;
            connectionTarget?.IsHighlighted = true;
        }
        Point.IsHighlighted = connectionTarget is not null;
    }

    private void CancelGesture(PointerEventArgs args)
    {
        if (pointerId == args.PointerId)
        {
            RollBackGesture();
        }
    }

    private void ResetGesture()
    {
        pointerId = null;
        if (!wasDragged)
        {
            return;
        }

        try
        {
            if (Point.Owner is CablePart)
            {
                connectionTarget?.IsHighlighted = false;
                Point.IsHighlighted = false;
                connectionTarget = null;
                originalConnection = null;
                startConnections = [];
            }
        }
        finally
        {
            History.Unfreeze();
        }
    }

    private void RollBackGesture()
    {
        if (pointerId is null)
        {
            return;
        }

        try
        {
            if (wasDragged && Point.Owner is CircuitPart part)
            {
                part.MoveTo(startPosition);
            }
            else if (wasDragged)
            {
                new MoveCablePointAction(Point, startPosition, startConnections).Undo();
            }
        }
        finally
        {
            ResetGesture();
        }
    }

    public void Dispose() => RollBackGesture();
}
