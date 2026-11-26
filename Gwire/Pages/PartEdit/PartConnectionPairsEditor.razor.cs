using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Models.Pairs;
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

    /// <summary>
    /// Two-terminal pairs edited here; potentiometers have their own editor.
    /// </summary>
    private IEnumerable<ConnectionPair> Pairs => Part.ConnectionPairs.Where(pair => pair is not Potentiometer);

    private ConnectionPair? SelectedPair => SelectedPoint is { } point
        ? Pairs.FirstOrDefault(pair => pair.Contains(point))
        : null;

    private Task Edit(Action edit) => Disabled ? Task.CompletedTask : ExecuteEdit.InvokeAsync(edit);

    private Task AddCoil() => AddPair(position => new CoilPair
    {
        Point1 = CreatePoint("Signal", position),
        Point2 = CreatePoint("Ground", new Point(position.X + 100, position.Y)),
        Color = ConnectionGroup.GetLineColor(Part.ConnectionPairs.OfType<CoilPair>().Count())
    });

    private Task AddResistor() => AddPair(position => new ResistorPair
    {
        Point1 = CreatePoint("Terminal 1", position),
        Point2 = CreatePoint("Terminal 2", new Point(position.X + 100, position.Y))
    });

    private Task AddCapacitor() => AddPair(position => new CapacitorPair
    {
        Point1 = CreatePoint("Terminal 1", position),
        Point2 = CreatePoint("Terminal 2", new Point(position.X + 100, position.Y))
    });

    private ConnectionPoint CreatePoint(string label, Point position) =>
        new(Part) { Label = label, LocalPosition = position };

    /// <summary>
    /// Creates the pair with its terminals at the next free position, names it, and adds it in one undoable edit.
    /// </summary>
    private async Task AddPair(Func<Point, ConnectionPair> createPair)
    {
        if (Disabled)
        {
            return;
        }

        var count = Part.Points.Count;
        var pair = createPair(new Point(120 + (count % 5) * 140, 120 + (count / 5) * 100));
        pair.Name = $"{pair.TypeName} {Part.ConnectionPairs.Count(existing => existing.GetType() == pair.GetType()) + 1}";
        await ExecuteEdit.InvokeAsync(() => Part.AddPair(pair));
        await SelectedPointChanged.InvokeAsync(pair.GetPoints()[0]);
    }

    private async Task RemovePair(ConnectionPair pair)
    {
        if (Disabled || !Part.ConnectionPairs.Contains(pair))
        {
            return;
        }

        await ExecuteEdit.InvokeAsync(() => Part.RemovePair(pair));
        await SelectedPointChanged.InvokeAsync(null);
    }
}
