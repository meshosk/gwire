using Gwire.Models;
using Gwire.Models.Base;
using Microsoft.AspNetCore.Components;

namespace Gwire.Components;

public partial class CircuitPathRenderer : IDisposable
{
    private readonly HashSet<BaseCircuitPart> observedParts = [];

    [Parameter, EditorRequired]
    public Circuit Circuit { get; set; } = default!;

    [Parameter, EditorRequired]
    public IReadOnlyList<SourceGroundPair> SourceGroundPairs { get; set; } = default!;

    private ConnectionPoint? GetCircuitPoint(SourceGroundPair pair, CircuitPart terminal)
    {
        var point = pair.GetAttachment(terminal);
        while (point is not null && !Circuit.Parts.Contains(point.Owner))
        {
            var ownerPair = SourceGroundPairs.FirstOrDefault(candidate => ReferenceEquals(candidate.PowerSource, point.Owner) ||
                ReferenceEquals(candidate.Ground, point.Owner));
            point = ownerPair?.GetAttachment((CircuitPart)point.Owner);
        }
        return point;
    }

    private string? GetPathPoints(SourceGroundPair pair)
    {
        var source = GetCircuitPoint(pair, pair.PowerSource);
        var ground = GetCircuitPoint(pair, pair.Ground);
        if (source is null || ground is null)
        {
            return null;
        }
        var path = source.FindPathTo(ground);
        if (path.Count == 0)
        {
            return null;
        }

        var positions = new List<Point> { SourceGroundPair.GetPointPosition(path[0], SourceGroundPairs) };
        for (var index = 1; index < path.Count; index++)
        {
            var previous = path[index - 1];
            var current = path[index];
            if (previous.Owner is CablePart cable && ReferenceEquals(current.Owner, cable))
            {
                positions.AddRange(ReferenceEquals(previous, cable.Points[0])
                    ? cable.BendPoints
                    : cable.BendPoints.AsEnumerable().Reverse());
            }
            positions.Add(SourceGroundPair.GetPointPosition(current, SourceGroundPairs));
        }

        return string.Join(" ", positions.Select(position =>
            FormattableString.Invariant($"{position.X},{position.Y}")));
    }

    protected override void OnParametersSet()
    {
        var parts = Circuit.Parts.Concat(SourceGroundPairs
            .SelectMany(pair => new[] { pair.PowerSource, pair.Ground })).ToHashSet();
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

    public void Dispose()
    {
        foreach (var part in observedParts)
        {
            part.Changed -= HandleChanged;
        }
    }
}
