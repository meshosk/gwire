using Gwire.Models;
using Gwire.Models.Base;
using Gwire.UndoRedo;
using Gwire.UndoRedo.Actions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Gwire.Pages;

public partial class SchemeEditor : ComponentBase, IAsyncDisposable
{
    private readonly Circuit Circuit = new();
    private readonly UndoRedoHistory history = new();
    private ElementReference editorElement;
    private DotNetObjectReference<SchemeEditor>? shortcutReference;
    private int selectedPartIndex = -1;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    protected override void OnInitialized() => history.Changed += HandleHistoryChanged;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            shortcutReference = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("gwire.enableUndoRedoShortcuts", editorElement, shortcutReference);
            await JS.InvokeVoidAsync("gwire.enableSchemePan", editorElement);
        }
    }

    private void HandleHistoryChanged() => _ = InvokeAsync(StateHasChanged);

    [JSInvokable]
    public void HandleHistoryShortcut(bool redo)
    {
        if (redo)
        {
            history.Redo();
        }
        else
        {
            history.Undo();
        }
    }

    private void DeleteSelectedPart()
    {
        if (!history.IsFrozen && Circuit.SelectedPart is { } part)
        {
            history.Execute(new RemovePartAction(Circuit, part));
        }
    }

    public async ValueTask DisposeAsync()
    {
        history.Changed -= HandleHistoryChanged;
        if (shortcutReference is not null)
        {
            await JS.InvokeVoidAsync("gwire.disableSchemePan", editorElement);
            await JS.InvokeVoidAsync("gwire.disableUndoRedoShortcuts", editorElement);
            shortcutReference.Dispose();
        }
    }

    private void ClearSelection() => Circuit.SelectedPart = null;

    private void SetPartSelection(BaseCircuitPart part, bool isSelected)
    {
        if (isSelected)
        {
            Circuit.SelectedPart = part;
        }
        else if (ReferenceEquals(Circuit.SelectedPart, part))
        {
            Circuit.SelectedPart = null;
        }
    }

    private void AddCable()
    {
        if (history.IsFrozen)
        {
            return;
        }

        var cableNumber = Circuit.Parts.OfType<CablePart>().Count() + 1;
        var cable = new CablePart
        {
            Name = $"Cable {cableNumber}"
        };

        cable.Points[0].LocalPosition = new Point(300, 300 + (cableNumber - 1) * 40);
        cable.Points[1].LocalPosition = new Point(500, 300 + (cableNumber - 1) * 40);
        history.Execute(new AddPartAction(Circuit, cable));
    }

    private void AddSelectedPart()
    {
        if (history.IsFrozen || selectedPartIndex < 0 || selectedPartIndex >= GwireParts.Parts.Count)
        {
            return;
        }

        if (GwireParts.Parts[selectedPartIndex] is not CircuitPart selectedPart)
        {
            return;
        }

        var part = selectedPart.Clone();
        var partNumber = Circuit.Parts.OfType<CircuitPart>().Count();
        part.SchemePosition = new Point(150 + partNumber * 25, 150 + partNumber * 25);
        history.Execute(new AddPartAction(Circuit, part));
    }

}
