using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Models.Pairs;
using Microsoft.AspNetCore.Components;

namespace Gwire.Pages.PartEdit;

public partial class PartPotentiometersEditor
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

    private Potentiometer? SelectedPotentiometer => SelectedPoint is { } point
        ? Part.ConnectionPairs.OfType<Potentiometer>().FirstOrDefault(potentiometer => potentiometer.Contains(point)) : null;

    private Task Edit(Action edit) => Disabled ? Task.CompletedTask : ExecuteEdit.InvokeAsync(edit);

    private async Task AddPotentiometer()
    {
        if (Disabled)
        {
            return;
        }
        var potentiometer = new Potentiometer();
        await ExecuteEdit.InvokeAsync(() =>
        {
            var count = Part.Points.Count;
            var x = 120 + (count % 5) * 140;
            var y = 120 + (count / 5) * 100;
            potentiometer.Name = $"Potentiometer {Part.ConnectionPairs.OfType<Potentiometer>().Count() + 1}";
            potentiometer.A = new ConnectionPoint(Part) { Label = "A", LocalPosition = new Point(x, y) };
            potentiometer.W = new ConnectionPoint(Part) { Label = "W", LocalPosition = new Point(x + 100, y) };
            potentiometer.B = new ConnectionPoint(Part) { Label = "B", LocalPosition = new Point(x + 200, y) };
            Part.AddPair(potentiometer);
        });
        await SelectedPointChanged.InvokeAsync(potentiometer.A);
    }

    private async Task RemovePotentiometer(Potentiometer potentiometer)
    {
        if (Disabled || !Part.ConnectionPairs.Contains(potentiometer))
        {
            return;
        }
        await ExecuteEdit.InvokeAsync(() => Part.RemovePair(potentiometer));
        await SelectedPointChanged.InvokeAsync(null);
    }
}
