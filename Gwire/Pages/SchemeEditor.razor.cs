using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Serialization;
using Gwire.Services;
using Gwire.UndoRedo;
using Gwire.UndoRedo.Actions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace Gwire.Pages;

public partial class SchemeEditor : ComponentBase, IAsyncDisposable
{
    private CircuitPart? editedTextPart;
    private Circuit Circuit = new();
    internal List<SourceGroundPair> SourceGroundPairs { get; } = [new()];
    [Inject]
    private UndoRedoService UndoRedoService { get; set; } = default!;

    private UndoRedoService.UndoRedoHistoryWrapper history => UndoRedoService.getHistory(HistoryIds.SchemeEditor);

    private ElementReference editorElement;
    private DotNetObjectReference<SchemeEditor>? shortcutReference;
    private float zoom = 1;
    private bool showDetails;
    private string selectedTag = string.Empty;

    private IReadOnlyList<string> AvailableTags => GwireParts.Tags
        .Where(tag => !Circuit.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
        .ToList();

    private void ToggleDetails() => showDetails = !showDetails;

    private void AddTag()
    {
        if (string.IsNullOrWhiteSpace(selectedTag) || !AvailableTags.Contains(selectedTag, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        Circuit.Tags.Add(selectedTag);
        selectedTag = string.Empty;
    }

    private void RemoveTag(string tag) => Circuit.Tags.Remove(tag);

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

    [Inject]
    private MessageService MessageService { get; set; } = default!;

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
            var action = new RemovePartAction(Circuit, this, part);
            action.Redo();
            history.addRedoAction(action);
        }
    }

    public async ValueTask DisposeAsync()
    {
        history.Changed -= HandleHistoryChanged;
        history.Clear();
        if (shortcutReference is not null)
        {
            await JS.InvokeVoidAsync("gwire.disableSchemePan", editorElement);
            await JS.InvokeVoidAsync("gwire.disableUndoRedoShortcuts", editorElement);
            shortcutReference.Dispose();
        }
    }

    private void NewScheme()
    {
        if (history.IsFrozen)
        {
            return;
        }

        SetCircuit(new Circuit());
    }

    private void SetCircuit(Circuit circuit)
    {
        history.Clear();
        Circuit = circuit;
        SourceGroundPairs.Clear();
        SelectedPart = null;
        selectedPartForAddIndex = -1;
        selectedTag = string.Empty;
    }

    private async Task ImportSchemeAsync(InputFileChangeEventArgs eventArgs)
    {
        if (history.IsFrozen)
        {
            return;
        }

        try
        {
            var circuit = await ObjectJsonSerializer.ImportAsync<Circuit>(eventArgs.File);
            if (history.IsFrozen)
            {
                return;
            }

            SetCircuit(circuit);
            MessageService.Show("Scheme imported", "Scheme imported successfully.", MessageType.Success);
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException)
        {
            MessageService.Show("Scheme import failed", "The selected file does not contain valid scheme JSON or could not be read.", MessageType.Danger);
        }
    }

    private async Task ExportSchemeAsync()
    {
        if (history.IsFrozen)
        {
            return;
        }

        await using var stream = new MemoryStream();
        await ObjectJsonSerializer.ExportAsync<Circuit>(Circuit, stream);
        stream.Position = 0;

        using var streamReference = new DotNetStreamReference(stream);
        await JS.InvokeVoidAsync("gwire.downloadFileFromStream", CreateFileName(Circuit.Name), streamReference);
    }

    private static string CreateFileName(string name)
    {
        var safeName = string.Concat(name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character)).Trim();
        return string.IsNullOrWhiteSpace(safeName) ? "scheme.json" : $"{safeName}.scheme.json";
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
        var action = new AddSourceGroundPairAction(SourceGroundPairs, pair);
        action.Redo();
        history.addRedoAction(action);
    }

    private void SetPairColor(SourceGroundPair pair, string? color)
    {

        color = string.IsNullOrWhiteSpace(color) ? "red" : color; // yeah default is red :D

        if (!history.IsFrozen && !string.Equals(pair.LineColor, color, StringComparison.OrdinalIgnoreCase))
        {
            var action = new ChangePairColorAction(pair, pair.LineColor, color);
            action.Redo();
            history.addRedoAction(action);
        }
    }

    private void AddCable()
    {
        if (history.IsFrozen)
        {
            return;
        }

        var action = AddPartAction.CreateCable(Circuit, this);
        action.Redo();
        history.addRedoAction(action);
    }

    private void AddSelectedPart()
    {
        if (history.IsFrozen || selectedPartForAddIndex < 0 || selectedPartForAddIndex >= GwireParts.Parts.Count)
        {
            return;
        }

        var action = AddPartAction.CreatePart(Circuit, this, GwireParts.Parts[selectedPartForAddIndex]);
        action.Redo();
        history.addRedoAction(action);
    }

}
