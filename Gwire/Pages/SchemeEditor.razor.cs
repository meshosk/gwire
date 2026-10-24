using Gwire.Components.SearchDialog;
using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Serialization;
using Gwire.Services;
using Gwire.UndoRedo;
using Gwire.UndoRedo.Actions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Text.Json;

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
    private ElementReference pdfExportModal;
    private DotNetObjectReference<SchemeEditor>? shortcutReference;
    private float zoom = 1;
    private bool showDetails;
    private bool isLoadingScheme;
    private bool showSourceGroundPair = true;
    private string selectedTag = string.Empty;
    private bool showTagSuggestions;
    private int activeTagSuggestionIndex;

    private IReadOnlyList<string> AvailableTags => GwireParts.Tags
        .Where(tag => !Circuit.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
        .ToList();

    private IReadOnlyList<string> TagSuggestions => AvailableTags
        .Where(tag => tag.Contains(selectedTag.Trim(), StringComparison.OrdinalIgnoreCase))
        .Take(8)
        .ToList();

    private void ToggleDetails() => showDetails = !showDetails;

    private void ResetTagSuggestions()
    {
        activeTagSuggestionIndex = 0;
        showTagSuggestions = true;
    }

    private void CloseTagSuggestions() => showTagSuggestions = false;

    private void HandleTagKeyDown(KeyboardEventArgs eventArgs)
    {
        if (eventArgs.Key == "Escape")
        {
            CloseTagSuggestions();
            return;
        }

        if (eventArgs.Key is not ("ArrowDown" or "ArrowUp" or "Enter"))
        {
            return;
        }

        var suggestions = TagSuggestions;
        if (suggestions.Count == 0)
        {
            return;
        }

        if (!showTagSuggestions)
        {
            ResetTagSuggestions();
            return;
        }

        switch (eventArgs.Key)
        {
            case "ArrowDown":
                activeTagSuggestionIndex = (activeTagSuggestionIndex + 1) % suggestions.Count;
                break;
            case "ArrowUp":
                activeTagSuggestionIndex = (activeTagSuggestionIndex + suggestions.Count - 1) % suggestions.Count;
                break;
            case "Enter":
                AddTag(suggestions[Math.Clamp(activeTagSuggestionIndex, 0, suggestions.Count - 1)]);
                break;
        }
    }

    private void AddTag(string selected)
    {
        var tag = AvailableTags.FirstOrDefault(tag =>
            string.Equals(tag, selected, StringComparison.OrdinalIgnoreCase));
        if (tag is null)
        {
            return;
        }

        Circuit.Tags.Add(tag);
        selectedTag = string.Empty;
        ResetTagSuggestions();
    }

    private void RemoveTag(string tag) => Circuit.Tags.Remove(tag);

    private int ZoomPercentage => (int)MathF.Round(zoom * 100);

    private SearchOptions<CircuitPart> partSearchOptions = default!;
    private bool PartSearchIsShown;
    private SearchOptions<GwireSchemeInfo> schemeSearchOptions = default!;
    private bool SchemeSearchIsShown;
    private (string Name, string Description, string[] Tags) unchangedSchemeDetails = (string.Empty, string.Empty, []);

    [Inject]
    private ConfirmationService ConfirmationService { get; set; } = default!;

    private bool HasUnsavedChanges => history.HasEntries
        || Circuit.Name != unchangedSchemeDetails.Name
        || Circuit.Description != unchangedSchemeDetails.Description
        || !Circuit.Tags.SequenceEqual(unchangedSchemeDetails.Tags);

    private async Task<bool> ConfirmDiscardChangesAsync() => !HasUnsavedChanges ||
        await ConfirmationService.ShowAsync("Unsaved changes",
            "This scheme has unsaved changes. Do you want to discard them and continue?") == ConfirmationResult.YES;

    /// <summary>
    /// The currently selected part in the scheme editor, or null if no part is selected.
    /// </summary>
    internal BaseCircuitPart? SelectedPart { get; set; }

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private MessageService MessageService { get; set; } = default!;

    [Inject]
    private ILogger<SchemeEditor> Logger { get; set; } = default!;

    protected override void OnInitialized()
    {
        history.Changed += HandleHistoryChanged;
        partSearchOptions = new SearchOptions<CircuitPart>(
            (part, tags, textTerms) =>
            {
                var name = part.Name.ToLowerInvariant();
                var description = part.Description.ToLowerInvariant();
                var partTags = part.Tags.Select(tag => tag.ToLowerInvariant()).ToHashSet();

                return tags.Count(tag => partTags.Contains(tag.ToLowerInvariant()))
                    + textTerms.Select(term => term.ToLowerInvariant())
                        .Count(term => name.Contains(term) || description.Contains(term));
            },
            part => part.DisplayName)
        {
            Title = "Add part",
            Items = GwireParts.Parts,
            AvailableTags = GwireParts.Tags
        };
        schemeSearchOptions = new SearchOptions<GwireSchemeInfo>(
            (scheme, tags, textTerms) =>
            {
                var name = scheme.Name.ToLowerInvariant();
                var description = scheme.Description.ToLowerInvariant();
                var schemeTags = scheme.Tags.Select(tag => tag.ToLowerInvariant()).ToHashSet();

                return tags.Count(tag => schemeTags.Contains(tag.ToLowerInvariant()))
                    + textTerms.Select(term => term.ToLowerInvariant())
                        .Count(term => name.Contains(term) || description.Contains(term));
            },
            scheme => string.IsNullOrWhiteSpace(scheme.Name) ? scheme.File : scheme.Name)
        {
            Title = "Open scheme",
            Items = GwireParts.Schemes,
            AvailableTags = GwireParts.Tags
        };
    }

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
        await JS.InvokeVoidAsync("gwire.hidePdfExportProgress", pdfExportModal);
        if (shortcutReference is not null)
        {
            await JS.InvokeVoidAsync("gwire.disableSchemePan", editorElement);
            await JS.InvokeVoidAsync("gwire.disableUndoRedoShortcuts", editorElement);
            shortcutReference.Dispose();
        }
    }

    private async Task NewScheme()
    {
        if (history.IsFrozen || !await ConfirmDiscardChangesAsync() || history.IsFrozen)
        {
            return;
        }

        SetCircuit(new Circuit());
    }

    private void SetCircuit(Circuit circuit)
    {
        history.Clear();
        Circuit = circuit;
        unchangedSchemeDetails = (circuit.Name, circuit.Description, circuit.Tags.ToArray());
        SourceGroundPairs.Clear();
        SourceGroundPairs.Add(new SourceGroundPair());
        showSourceGroundPair = true;
        SelectedPart = null;
        selectedTag = string.Empty;
        activeTagSuggestionIndex = 0;
        CloseTagSuggestions();
    }

    private Task ImportSchemeAsync(InputFileChangeEventArgs eventArgs) =>
        ImportSchemeAsync(() => Task.FromResult(eventArgs.File.OpenReadStream()));

    private async Task ImportSchemeAsync(Func<Task<Stream>> openJsonStream)
    {
        if (history.IsFrozen)
        {
            return;
        }

        Circuit circuit;
        history.Freeze();
        try
        {
            await using var stream = await openJsonStream();
            circuit = await ObjectJsonSerializer.ImportAsync<Circuit>(stream);
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidDataException or IOException or TaskCanceledException)
        {
            Logger.LogError(exception, "Scheme import failed.");
            MessageService.Show("Scheme import failed", "The scheme JSON could not be read or does not contain a valid scheme.", MessageType.Danger);
            return;
        }
        finally
        {
            history.Unfreeze();
        }

        SetCircuit(circuit);
        MessageService.Show("Scheme imported", "Scheme imported successfully.", MessageType.Success);
    }

    [Inject]
    private SchemePdfExporter PdfExporter { get; set; } = default!;

    private async Task ExportPdfAsync()
    {
        if (history.IsFrozen || Circuit.Parts.Count == 0)
        {
            return;
        }

        history.Freeze();
        try
        {
            await JS.InvokeVoidAsync("gwire.showPdfExportProgress", pdfExportModal);
            await PdfExporter.ExportAsync(editorElement, Circuit.Name);
        }
        catch (JSException exception)
        {
            Logger.LogError(exception, "Scheme PDF export failed.");
            MessageService.Show("PDF export failed", "The PDF could not be created. Please try again; see the browser console for details.", MessageType.Danger);
        }
        finally
        {
            try
            {
                await JS.InvokeVoidAsync("gwire.hidePdfExportProgress", pdfExportModal);
            }
            finally
            {
                history.Unfreeze();
            }
        }
    }

    private async Task ExportSchemeAsync()
    {
        if (history.IsFrozen)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Circuit.Name))
        {
            await JS.InvokeVoidAsync("alert", "Before exporting, add the scheme information and enter a scheme name.");
            showDetails = true;
            return;
        }

        try
        {
            await using var stream = new MemoryStream();
            await ObjectJsonSerializer.ExportAsync<Circuit>(Circuit, stream);
            stream.Position = 0;

            using var streamReference = new DotNetStreamReference(stream);
            await JS.InvokeVoidAsync("gwire.downloadFileFromStream", CreateFileName(Circuit.Name), streamReference);
        }
        catch (Exception exception) when (exception is JsonException or IOException or JSException)
        {
            Logger.LogError(exception, "Scheme export failed.");
            MessageService.Show("Scheme export failed", "The scheme could not be exported. Your scheme is still open; see the browser console for details.", MessageType.Danger);
        }
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

    private void ToggleSourceGroundPair()
    {
        if (!history.IsFrozen)
        {
            showSourceGroundPair = !showSourceGroundPair;
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

    private async Task SearchSchemesAsync()
    {
        if (history.IsFrozen || SchemeSearchIsShown || !await ConfirmDiscardChangesAsync() || history.IsFrozen)
        {
            return;
        }

        SchemeSearchIsShown = true;
        StateHasChanged();
        var scheme = await schemeSearchOptions.Result;
        if (scheme is null || history.IsFrozen)
        {
            return;
        }

        isLoadingScheme = true;
        try
        {
            await ImportSchemeAsync(async () =>
            {
                await JS.InvokeVoidAsync("gwire.showPdfExportProgress", pdfExportModal);
                return await GwireParts.LoadSchemeAsync(scheme);
            });
        }
        finally
        {
            try
            {
                await JS.InvokeVoidAsync("gwire.hidePdfExportProgress", pdfExportModal);
            }
            finally
            {
                isLoadingScheme = false;
            }
        }
    }

    private async Task SearchPartsAsync()
    {
        if (history.IsFrozen || PartSearchIsShown)
        {
            return;
        }

        PartSearchIsShown = true;
        StateHasChanged();
        var selectedPart = await partSearchOptions.Result;

        if (selectedPart is null || history.IsFrozen)
        {
            return;
        }

        var action = AddPartAction.CreatePart(Circuit, this, selectedPart);
        action.Redo();
        history.addRedoAction(action);
    }

}
