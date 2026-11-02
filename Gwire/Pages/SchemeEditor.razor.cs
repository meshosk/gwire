using Gwire.Components.Displayers.SearchDialog;
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
    [CascadingParameter(Name = "ReloadRepository")]
    public EventCallback ReloadRepository { get; set; }

    private CircuitPart? editedTextPart;

    [Inject]
    private EditorStateService EditorStateService { get; set; } = default!;

    private SchemeEditorState State => EditorStateService.SchemeEditor;

    private Circuit Circuit { get => State.Circuit; set => State.Circuit = value; }

    internal List<SourceGroundPair> SourceGroundPairs => State.SourceGroundPairs;

    [Inject]
    private UndoRedoService UndoRedoService { get; set; } = default!;

    private UndoRedoService.UndoRedoHistoryWrapper history => UndoRedoService.getHistory(HistoryIds.SchemeEditor);

    private ElementReference editorElement;

    private ElementReference pdfExportModal;

    private DotNetObjectReference<SchemeEditor>? shortcutReference;

    private float zoom { get => State.Zoom; set => State.Zoom = value; }

    private bool showDetails { get => State.ShowDetails; set => State.ShowDetails = value; }

    private bool isLoadingScheme;

    private bool showSourceGroundPair { get => State.ShowSourceGroundPair; set => State.ShowSourceGroundPair = value; }

    private string selectedTag { get => State.SelectedTag; set => State.SelectedTag = value; }

    private bool showTagSuggestions { get => State.ShowTagSuggestions; set => State.ShowTagSuggestions = value; }

    private int activeTagSuggestionIndex { get => State.ActiveTagSuggestionIndex; set => State.ActiveTagSuggestionIndex = value; }

    private IReadOnlyList<string> AvailableTags => GwireParts.Tags
        .Where(tag => !Circuit.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
        .ToList();

    private IReadOnlyList<string> TagSuggestions => AvailableTags
        .Where(tag => tag.Contains(selectedTag.Trim(), StringComparison.OrdinalIgnoreCase))
        .Take(8)
        .ToList();

    private int ZoomPercentage => (int)MathF.Round(zoom * 100);

    private SearchOptions<CircuitPart> partSearchOptions = default!;

    private bool PartSearchIsShown;

    private SearchOptions<GwireSchemeInfo> schemeSearchOptions = default!;

    private bool SchemeSearchIsShown;

    private (string Name, string Description, string[] Tags) unchangedSchemeDetails { get => State.UnchangedSchemeDetails; set => State.UnchangedSchemeDetails = value; }

    [Inject]
    private ConfirmationService ConfirmationService { get; set; } = default!;

    private bool HasUnsavedChanges => history.HasEntries
        || Circuit.Name != unchangedSchemeDetails.Name
        || Circuit.Description != unchangedSchemeDetails.Description
        || !Circuit.Tags.SequenceEqual(unchangedSchemeDetails.Tags);

    /// <summary>
    /// The currently selected part in the scheme editor, or null if no part is selected.
    /// </summary>
    internal BaseCircuitPart? SelectedPart { get => State.SelectedPart; set => State.SelectedPart = value; }

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private MessageService MessageService { get; set; } = default!;

    [Inject]
    private ILogger<SchemeEditor> Logger { get; set; } = default!;

    [Inject]
    private SchemePdfExporter PdfExporter { get; set; } = default!;

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

    private async Task<bool> ConfirmDiscardChangesAsync() => !HasUnsavedChanges ||
        await ConfirmationService.ShowAsync("Unsaved changes",
            "This scheme has unsaved changes. Do you want to discard them and continue?") == ConfirmationResult.YES;

    private void HandleRepositoryChanged() => StateHasChanged();

    protected override void OnInitialized()
    {
        history.Changed += HandleHistoryChanged;
        GwireParts.Changed += HandleRepositoryChanged;
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
            part => part.Name)
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
            await JS.InvokeVoidAsync("gwire.enableSchemePan", editorElement, shortcutReference,
                new { zoom, x = State.ViewportX, y = State.ViewportY, panEnabled = State.PanEnabled });
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

    [JSInvokable]
    public void HandleViewportChanged(float x, float y, bool panEnabled)
    {
        State.ViewportX = x;
        State.ViewportY = y;
        State.PanEnabled = panEnabled;
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
            var action = new RemovePartAction(Circuit, State, part);
            action.Redo();
            history.addRedoAction(action);
        }
    }

    public async ValueTask DisposeAsync()
    {
        history.Changed -= HandleHistoryChanged;
        GwireParts.Changed -= HandleRepositoryChanged;
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
        showSourceGroundPair = false;
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

        if (isLoadingScheme)
        {
            await JS.InvokeVoidAsync("gwire.hidePdfExportProgress", pdfExportModal);
        }

        if (history.IsFrozen || !await ConfirmDiscardChangesAsync() || history.IsFrozen)
        {
            return;
        }

        SetCircuit(circuit);
        MessageService.Show("Scheme imported", "Scheme imported successfully.", MessageType.Success);
    }

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

        var action = AddPartAction.CreateCable(Circuit, State);
        action.Redo();
        history.addRedoAction(action);
    }

    private async Task SearchSchemesAsync()
    {
        if (history.IsFrozen || SchemeSearchIsShown)
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

    private void DuplicatePart(CircuitPart part)
    {
        if (history.IsFrozen || !Circuit.Parts.Contains(part))
        {
            return;
        }

        var action = AddPartAction.CreatePart(Circuit, State, part);
        action.Redo();
        history.addRedoAction(action);
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

        var action = AddPartAction.CreatePart(Circuit, State, selectedPart);
        action.Redo();
        history.addRedoAction(action);
    }
}
