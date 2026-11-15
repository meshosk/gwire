using Gwire.Models;
using Gwire.Models.Base;
using Microsoft.AspNetCore.Components;

namespace Gwire.Pages.PartEdit;

/// <summary>
/// Edits electrical pairs while delegating each model change to the part editor's history.
/// </summary>
public partial class PartConnectionPairsEditor
{
    [Parameter, EditorRequired]
    public CircuitPart Part { get; set; } = default!;

    [Parameter]
    public ConnectionPoint? SelectedPoint { get; set; }

    [Parameter]
    public EventCallback<ConnectionPoint?> SelectedPointChanged { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<Action> ExecuteEdit { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    private ConnectionPair? SelectedPair => SelectedPoint is { } point
        ? Part.ConnectionPairs.FirstOrDefault(pair => pair.Contains(point))
        : null;

    private Task Edit(Action edit) => Disabled ? Task.CompletedTask : ExecuteEdit.InvokeAsync(edit);

    private async Task AddPair(ConnectionPair pair)
    {
        if (Disabled)
        {
            return;
        }

        await ExecuteEdit.InvokeAsync(() =>
        {
            var count = Part.Points.Count;
            var pairType = pair.TypeName;
            pair.Name = $"{pairType} {Part.ConnectionPairs.Count(existing => existing.GetType() == pair.GetType()) + 1}";
            pair.Point1 = new ConnectionPoint(Part)
            {
                Label = pair is CoilPair ? "Signal" : "Terminal 1",
                LocalPosition = new Point(120 + (count % 5) * 140, 120 + (count / 5) * 100)
            };
            pair.Point2 = new ConnectionPoint(Part)
            {
                Label = pair is CoilPair ? "Ground" : "Terminal 2",
                LocalPosition = new Point(pair.Point1.LocalPosition.X + 100, pair.Point1.LocalPosition.Y)
            };
            if (pair is CoilPair coil)
            {
                coil.Color = ConnectionGroup.GetLineColor(Part.ConnectionPairs.OfType<CoilPair>().Count());
            }

            Part.Points.Add(pair.Point1);
            Part.Points.Add(pair.Point2);
            Part.ConnectionPairs.Add(pair);
        });
        await SelectedPointChanged.InvokeAsync(pair.Point1);
    }

    private async Task RemovePair(ConnectionPair pair)
    {
        if (Disabled || !Part.ConnectionPairs.Contains(pair))
        {
            return;
        }

        await ExecuteEdit.InvokeAsync(() =>
        {
            foreach (var point in new[] { pair.Point1, pair.Point2 })
            {
                foreach (var connected in point.ConnectionPoints.ToArray())
                {
                    point.Disconnect(connected);
                }
                Part.Points.Remove(point);
                foreach (var group in Part.StateGroups.SelectMany(stateGroup => stateGroup.States)
                    .SelectMany(state => state.ConnectionGroups))
                {
                    group.ConnectedPints.Remove(point);
                }
            }
            Part.ConnectionPairs.Remove(pair);
        });
        await SelectedPointChanged.InvokeAsync(null);
    }
}
