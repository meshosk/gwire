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
    internal List<SourceGroundPair> SourceGroundPairs { get; } = [new()];
    private readonly UndoRedoHistory history = new();
    private ElementReference editorElement;
    private DotNetObjectReference<SchemeEditor>? shortcutReference;
    private float zoom = 1;

    private int ZoomPercentage => (int)MathF.Round(zoom * 100);

    /// <summary>
    /// Index of the part selected in the "Add part" dropdown. -1 if no part is selected.
    /// </summary>
    private int selectedPartForAddIndex = -1;

    /// <summary>
    /// The currently selected part in the scheme editor, or null if no part is selected.
    /// </summary>
    internal BaseCircuitPart? SelectedPart { get; set; }

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    protected override void OnInitialized() => history.Changed += HandleHistoryChanged;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            shortcutReference = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("gwire.enableUndoRedoShortcuts", editorElement, shortcutReference);
            await JS.InvokeVoidAsync("gwire.enableSchemePan", editorElement, shortcutReference);
        }
    }

    private void HandleHistoryChanged() => StateHasChanged();

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

    [JSInvokable]
    public bool HandleZoomChanged(float zoom)
    {
        if (history.IsFrozen)
        {
            return false;
        }

        this.zoom = Math.Clamp(zoom, 0.2f, 2f);
        StateHasChanged();
        return true;
    }

    private async Task SetZoomAsync(float zoom)
    {
        await JS.InvokeVoidAsync("gwire.setSchemeZoom", editorElement, zoom);
    }

    private Task SetZoomPercentageAsync(int percentage) => SetZoomAsync(percentage / 100f);

    [JSInvokable]
    public void DeleteSelectedPart() => DeletePart(SelectedPart);

    private void DeletePart(BaseCircuitPart? part)
    {
        if (!history.IsFrozen && part is not null && Circuit.Parts.Contains(part))
        {
            history.Execute(new RemovePartAction(Circuit, this, part));
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

    private void ClearSelection() => SelectedPart = null;

    private void SetPartSelection(BaseCircuitPart part, bool isSelected)
    {
        if (isSelected)
        {
            SelectedPart = part;
        }
        else if (ReferenceEquals(SelectedPart, part))
        {
            SelectedPart = null;
        }
    }

    private void AddSourceGroundPair()
    {
        if (history.IsFrozen)
        {
            return;
        }
        var color = ConnectionGroup.GetLineColor(SourceGroundPairs.Count);
        while (SourceGroundPairs.Any(candidate => string.Equals(candidate.LineColor, color, StringComparison.OrdinalIgnoreCase)))
        {
            color = $"#{Random.Shared.Next(0x1000000):x6}";
        }
        var pair = new SourceGroundPair { LineColor = color };
        var offset = SourceGroundPairs.Count * 110;
        pair.PowerSource.MoveTo(new Point(pair.PowerSource.SchemePosition.X, pair.PowerSource.SchemePosition.Y + offset));
        pair.Ground.MoveTo(new Point(pair.Ground.SchemePosition.X, pair.Ground.SchemePosition.Y + offset));
        history.Execute(new AddSourceGroundPairAction(SourceGroundPairs, pair));
    }

    private void SetPairColor(SourceGroundPair pair, string? color)
    {

        color = string.IsNullOrWhiteSpace(color) ? "red" : color; // yeah default is red :D

        if (!history.IsFrozen && !string.Equals(pair.LineColor, color, StringComparison.OrdinalIgnoreCase))
        {
            history.Execute(new ChangePairColorAction(pair, pair.LineColor, color));
        }
    }

    private void AddCable()
    {
        if (history.IsFrozen)
        {
            return;
        }

        history.Execute(AddPartAction.CreateCable(Circuit, this));
    }

    private void AddSelectedPart()
    {
        if (history.IsFrozen || selectedPartForAddIndex < 0 || selectedPartForAddIndex >= GwireParts.Parts.Count)
        {
            return;
        }

        history.Execute(AddPartAction.CreatePart(Circuit, this, GwireParts.Parts[selectedPartForAddIndex]));
    }

}
