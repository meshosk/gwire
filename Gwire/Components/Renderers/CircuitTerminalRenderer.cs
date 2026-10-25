using Gwire.Models;
using Gwire.Models.Base;
using Gwire.UndoRedo;
using Gwire.UndoRedo.Actions;
using Microsoft.AspNetCore.Components;

namespace Gwire.Components.Renderers;

/// <summary>
/// Uses the part renderer's SVG and dragging behavior with terminal attachment handling.
/// </summary>
public sealed class CircuitTerminalRenderer : CircuitPartRenderer
{
    private readonly HashSet<BaseCircuitPart> observedParts = [];
    private ConnectionPoint? dragStartAttachment;
    private ConnectionPoint? connectionTarget;

    [Parameter, EditorRequired]
    public Circuit Circuit { get; set; } = default!;

    [Parameter, EditorRequired]
    public SourceGroundPair Pair { get; set; } = default!;

    protected override Point PartPosition => Pair.GetPosition(CircuitPart, SourceGroundPairs);

    [Parameter, EditorRequired]
    public IReadOnlyList<SourceGroundPair> SourceGroundPairs { get; set; } = default!;

    protected override bool HasDragChanges => base.HasDragChanges ||
        !ReferenceEquals(dragStartAttachment, Pair.GetAttachment(CircuitPart));

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);

        var parts = Circuit.Parts.Concat(SourceGroundPairs
            .SelectMany(pair => new[] { pair.PowerSource, pair.Ground }))
            .Where(part => !ReferenceEquals(part, CircuitPart)).ToHashSet();
        foreach (var part in observedParts.Except(parts).ToArray())
        {
            part.Changed -= HandleChanged;
            observedParts.Remove(part);
        }
        foreach (var part in parts.Except(observedParts))
        {
            part.Changed += HandleChanged;
            observedParts.Add(part);
        }
    }

    private void HandleChanged() => StateHasChanged();

    protected override void OnDragStarted()
    {
        dragStartAttachment = Pair.GetAttachment(CircuitPart);
        Pair.AttachTo(CircuitPart, null, SourceGroundPairs);
    }

    protected override void OnDragMoved() => UpdateConnectionTarget();

    protected override void OnDragEnding() => Pair.AttachTo(CircuitPart, connectionTarget, SourceGroundPairs);

    protected override IUndoableAction CreateMoveAction() =>
        new MoveTerminalAction(Pair, CircuitPart, SourceGroundPairs, dragStartPart, PartPosition.Copy(),
            dragStartAttachment, Pair.GetAttachment(CircuitPart));

    private void UpdateConnectionTarget()
    {
        ConnectionPoint? target = null;
        var position = SourceGroundPair.GetPointPosition(CircuitPart.Points[0], SourceGroundPairs);
        var closestDistanceSquared = 20f * 20f;
        foreach (var point in Circuit.Parts.Concat(SourceGroundPairs
            .SelectMany(pair => new[] { pair.PowerSource, pair.Ground })).SelectMany(part => part.Points))
        {
            if (!Pair.CanAttachTo(CircuitPart, point, SourceGroundPairs))
            {
                continue;
            }
            var candidate = SourceGroundPair.GetPointPosition(point, SourceGroundPairs);
            var dx = position.X - candidate.X;
            var dy = position.Y - candidate.Y;
            var distanceSquared = dx * dx + dy * dy;
            if (distanceSquared <= closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                target = point;
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
        CircuitPart.Points[0].IsHighlighted = connectionTarget is not null;
    }

    protected override void OnDragReset()
    {
        if (connectionTarget is not null)
        {
            connectionTarget.IsHighlighted = false;
        }
        CircuitPart.Points[0].IsHighlighted = false;
        connectionTarget = null;
        dragStartAttachment = null;
    }

    public override void Dispose()
    {
        foreach (var part in observedParts)
        {
            part.Changed -= HandleChanged;
        }
        base.Dispose();
    }
}
