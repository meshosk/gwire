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
