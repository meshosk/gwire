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
