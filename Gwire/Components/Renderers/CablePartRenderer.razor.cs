using Gwire.Models;
using Gwire.Services;
using Gwire.UndoRedo;
using Gwire.UndoRedo.Actions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Gwire.Components.Renderers;

public partial class CablePartRenderer : IDisposable
{
    private ElementReference elementReference;
    private readonly string elementReferenceId = $"cable-{Guid.NewGuid():N}";
    [Parameter]
    public float Zoom { get; set; } = 1;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private UndoRedoService UndoRedoService { get; set; } = default!;

    private UndoRedoService.UndoRedoHistoryWrapper History => UndoRedoService.getHistory(HistoryIds.SchemeEditor);


    [Parameter]
    public EventCallback OnDelete { get; set; }

    [Parameter]
    public bool IsSelected { get; set; }

    [Parameter]
    public EventCallback<bool> IsSelectedChanged { get; set; }

    [Parameter, EditorRequired]
    public CablePart CablePart { get; set; } = default!;

    [Parameter, EditorRequired]
    public Circuit Circuit { get; set; } = default!;

    private string CablePath => CablePart.GetPathData();

    private Task SelectContextTarget() => History.IsFrozen || IsSelected
        ? Task.CompletedTask
        : IsSelectedChanged.InvokeAsync(true);

    private void ChangeColor(string? color)
    {
        if (string.IsNullOrEmpty(color))
        {
            color = "#000000";
        }

        if (History.IsFrozen || color == CablePart.Color)
        {
            return;
        }
 
        var action = new ChangeCableColorAction(CablePart, CablePart.Color, color);
        action.Redo();
        History.addRedoAction(action);
    }

    private bool CanAddBendPoint(Point position) =>
        !History.IsFrozen && CablePart.FindBendPointInsertion(position) is not null;

    private void AddBendPoint(Point position)
    {
        if (History.IsFrozen || CablePart.FindBendPointInsertion(position) is not { } bend)
        {
            return;
        }

        var bends = CablePart.BendPoints.ToList();
        bends.Insert(bend.Index, new CableBendPoint(bend.Position.X, bend.Position.Y));
        var action = new ChangeCableRouteAction(CablePart, CablePart.BendPoints, bends);
        action.Redo();
        History.addRedoAction(action);
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

    private Task Select() => IsSelected ? Task.CompletedTask : IsSelectedChanged.InvokeAsync(true);

    private void HandleChanged() => StateHasChanged();

    public void Dispose()
    {
        if (CablePart is not null)
        {
            CablePart.Changed -= HandleChanged;
        }
    }
}
