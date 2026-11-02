using Gwire.Components.Displayers.SearchDialog;
using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Serialization;
using Gwire.Services;
using Gwire.UndoRedo;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text;

namespace Gwire.Pages.PartEdit;

public partial class PartEditor : ComponentBase, IAsyncDisposable
{
    [CascadingParameter(Name = "ReloadRepository")]
    public EventCallback ReloadRepository { get; set; }

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private GwirePartsService GwireParts { get; set; } = default!;

    [Inject]
    private MessageService MessageService { get; set; } = default!;

    [Inject]
    private ConfirmationService ConfirmationService { get; set; } = default!;

    [Inject]
    private EditorStateService EditorStateService { get; set; } = default!;

    private bool HasUnsavedChanges => history.HasEntries;

    private object partTabsKey = new();

    private SearchOptions<CircuitPart> partSearchOptions = default!;

    private bool PartSearchIsShown;

    private IEnumerable<CircuitPart> AvailableParts => GwireParts.Parts;

    private IReadOnlyList<string> AvailableTags => GwireParts.Tags
        .Where(tag => !EditorStateService.PartEditor.EditedPart.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
        .ToList();

    private IReadOnlyList<string> TagSuggestions => AvailableTags
        .Where(tag => tag.Contains(EditorStateService.PartEditor.SelectedTag.Trim(), StringComparison.OrdinalIgnoreCase))
        .Take(8)
        .ToList();

    private ConnectionPoint? draggedPoint;

    private long? valueDragPointerId;

    private Point dragOffset;

    private bool pointWasDragged;

    private async Task<bool> ConfirmDiscardChangesAsync() => !HasUnsavedChanges ||
        await ConfirmationService.ShowAsync("Unsaved changes",
            "This part has unsaved changes. Do you want to discard them and continue?") == ConfirmationResult.YES;

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
            Title = "Open part",
            Items = GwireParts.Parts,
            AvailableTags = GwireParts.Tags
        };
    }

    private void StartPointDrag(ConnectionPoint point, PointerEventArgs eventArgs)
    {
        if (pendingEdit is not null)
        {
            return;
        }

        EditorStateService.PartEditor.SelectedPointInEditor = point;
        BeginEdit();
        draggedPoint = point;
        var pointer = new Point(eventArgs);
        dragOffset = new Point(pointer.X - point.LocalPosition.X, pointer.Y - point.LocalPosition.Y);
        pointWasDragged = false;
    }

    private void StartValueDrag(PointerEventArgs eventArgs)
    {
        if (pendingEdit is not null || eventArgs.Button != 0 || !eventArgs.IsPrimary)
        {
            return;
        }

        BeginEdit();
        valueDragPointerId = eventArgs.PointerId;
        var pointer = new Point(eventArgs);
        dragOffset = new Point(pointer.X - EditorStateService.PartEditor.EditedPart.ValuePosition.X, pointer.Y - EditorStateService.PartEditor.EditedPart.ValuePosition.Y);
    }

    private void EndPointDrag(PointerEventArgs eventArgs)
    {
        if (valueDragPointerId is not null)
        {
            if (valueDragPointerId == eventArgs.PointerId)
            {
                MoveDraggedItem(eventArgs);
                CompleteEdit();
                ResetPointDrag();
            }
            return;
        }

        if (draggedPoint is not null && !pointWasDragged && EditorStateService.PartEditor.ActiveConnectionGroup is not null)
        {
            if (!EditorStateService.PartEditor.ActiveConnectionGroup.ConnectedPints.Remove(draggedPoint))
            {
                EditorStateService.PartEditor.ActiveConnectionGroup.ConnectedPints.Add(draggedPoint);
            }
        }

        if (draggedPoint is not null)
        {
            CompleteEdit();
        }
        ResetPointDrag();
    }

    private void CancelPointDrag(PointerEventArgs eventArgs)
    {
        if (valueDragPointerId is not null)
        {
            if (valueDragPointerId == eventArgs.PointerId)
            {
                CancelEdit();
                ResetPointDrag();
            }
            return;
        }

        if (draggedPoint is not null)
        {
            CancelEdit();
        }
        ResetPointDrag();
    }

    private void ResetPointDrag()
    {
        draggedPoint = null;
        valueDragPointerId = null;
        dragOffset = new Point();
        pointWasDragged = false;
    }

    private void MoveDraggedItem(PointerEventArgs eventArgs)
    {
        if (valueDragPointerId is not null)
        {
            if (valueDragPointerId == eventArgs.PointerId)
            {
                var pointer = new Point(eventArgs);
                EditorStateService.PartEditor.EditedPart.ValuePosition = new Point(pointer.X - dragOffset.X, pointer.Y - dragOffset.Y);
            }
            return;
        }

        if (draggedPoint is not null)
        {
            var pointer = new Point(eventArgs);
            var nextPosition = new Point(pointer.X - dragOffset.X, pointer.Y - dragOffset.Y);
            pointWasDragged |= MathF.Abs(nextPosition.X - draggedPoint.LocalPosition.X) > 2 ||
                MathF.Abs(nextPosition.Y - draggedPoint.LocalPosition.Y) > 2;
            draggedPoint.LocalPosition = nextPosition;
            return;
        }
    }

    private void SetCustomTextEnabled(bool enabled) =>
        ExecuteEdit(() => EditorStateService.PartEditor.EditedPart.CustomTextValue = enabled ? "value" : null);

    private void AddPoint()
    {
        var count = EditorStateService.PartEditor.EditedPart.Points.Count;
        var point = new ConnectionPoint(EditorStateService.PartEditor.EditedPart)
        {
            Label = $"Point {count + 1}",
            LocalPosition = new Point(120 + (count % 5) * 140, 120 + (count / 5) * 100)
        };

        EditorStateService.PartEditor.EditedPart.Points.Add(point);
        EditorStateService.PartEditor.SelectedPointInEditor = point;
    }

    private void ResetTagSuggestions()
    {
        EditorStateService.PartEditor.ActiveTagSuggestionIndex = 0;
        EditorStateService.PartEditor.ShowTagSuggestions = true;
    }

    private void CloseTagSuggestions() => EditorStateService.PartEditor.ShowTagSuggestions = false;

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

        if (!EditorStateService.PartEditor.ShowTagSuggestions)
        {
            ResetTagSuggestions();
            return;
        }

        switch (eventArgs.Key)
        {
            case "ArrowDown":
                EditorStateService.PartEditor.ActiveTagSuggestionIndex = (EditorStateService.PartEditor.ActiveTagSuggestionIndex + 1) % suggestions.Count;
                break;
            case "ArrowUp":
                EditorStateService.PartEditor.ActiveTagSuggestionIndex = (EditorStateService.PartEditor.ActiveTagSuggestionIndex + suggestions.Count - 1) % suggestions.Count;
                break;
            case "Enter":
                AddTag(suggestions[Math.Clamp(EditorStateService.PartEditor.ActiveTagSuggestionIndex, 0, suggestions.Count - 1)]);
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

        ExecuteEdit(() => EditorStateService.PartEditor.EditedPart.Tags.Add(tag));
        EditorStateService.PartEditor.SelectedTag = string.Empty;
        ResetTagSuggestions();
    }

    private void RemoveTag(string tag)
    {
        EditorStateService.PartEditor.EditedPart.Tags.Remove(tag);
    }

    private void RemoveSelectedPoint()
    {
        if (EditorStateService.PartEditor.SelectedPointInEditor is null)
        {
            return;
        }

        EditorStateService.PartEditor.EditedPart.Points.Remove(EditorStateService.PartEditor.SelectedPointInEditor);
        foreach (var state in EditorStateService.PartEditor.EditedPart.StateGroups.SelectMany(group => group.States))
        {
            foreach (var group in state.ConnectionGroups)
            {
                group.ConnectedPints.Remove(EditorStateService.PartEditor.SelectedPointInEditor);
            }
        }

        if (ReferenceEquals(draggedPoint, EditorStateService.PartEditor.SelectedPointInEditor))
        {
            draggedPoint = null;
        }

        EditorStateService.PartEditor.SelectedPointInEditor = null;
    }

    private string PointClass(ConnectionPoint point) =>
        ReferenceEquals(point, EditorStateService.PartEditor.SelectedPointInEditor) ? "connection-point is-selected" : "connection-point";

    private void SavePartToService()
    {
        var savedAsCopy = EditorStateService.PartEditor.EditedPart.IsFromRepo || AvailableParts.Any(existing => existing.IsFromRepo && existing.Id == EditorStateService.PartEditor.EditedPart.Id);
        var partToSave = EditorStateService.PartEditor.EditedPart;
        if (savedAsCopy)
        {
            partToSave = EditorStateService.PartEditor.EditedPart.Clone();
            partToSave.Id = Guid.NewGuid();
            partToSave.IsFromRepo = false;
        }

        GwireParts.SavePart(partToSave);
        if (savedAsCopy)
        {
            SetEditedPart(partToSave);
        }
        else
        {
            history.Clear();
        }
        MessageService.Show("Part saved", savedAsCopy ? "Part saved as your own copy." : "Part saved to the parts service.", MessageType.Success);
    }

    private async Task NewPart()
    {
        if (!await ConfirmDiscardChangesAsync())
        {
            return;
        }

        SetEditedPart(new CircuitPart());
        partTabsKey = new();
        EditorStateService.PartEditor.ActiveTabIndex = 0;
        EditorStateService.PartEditor.SelectedTag = string.Empty;
    }

    private async Task LoadSelectedPart()
    {
        if (PartSearchIsShown)
        {
            return;
        }

        PartSearchIsShown = true;
        StateHasChanged();
        var part = await partSearchOptions.Result;
        if (part is null || !await ConfirmDiscardChangesAsync())
        {
            return;
        }

        SetEditedPart(part.Clone());
        MessageService.Show("Part loaded", "Part loaded for editing.", MessageType.Success);
    }

    private void SetEditedPart(CircuitPart part)
    {
        pendingEdit = null;
        history.Clear();
        EditorStateService.PartEditor.EditedPart = part;
        EditorStateService.PartEditor.SelectedTag = string.Empty;
        EditorStateService.PartEditor.ActiveTagSuggestionIndex = 0;
        CloseTagSuggestions();

        if (EditorStateService.PartEditor.EditedPart.Width <= 0)
        {
            EditorStateService.PartEditor.EditedPart.Width = 500;
        }

        if (EditorStateService.PartEditor.EditedPart.Height <= 0)
        {
            EditorStateService.PartEditor.EditedPart.Height = 500;
        }

        EditorStateService.PartEditor.SelectedPointInEditor = null;
        EditorStateService.PartEditor.ActiveStateGroupIndex = 0;
        EditorStateService.PartEditor.ActiveStateInEditor = EditorStateService.PartEditor.ActiveStateGroupInEditor.ActiveState is { } activeState && EditorStateService.PartEditor.ActiveStateGroupInEditor.States.Contains(activeState)
            ? activeState
            : EditorStateService.PartEditor.ActiveStateGroupInEditor.States.FirstOrDefault();
        EditorStateService.PartEditor.ActiveStateGroupInEditor.ActiveState = EditorStateService.PartEditor.ActiveStateInEditor;
        EditorStateService.PartEditor.ActiveConnectionGroup = null;
        EditorStateService.PartEditor.ConnectionGroupTabIndex = 0;
        ResetPointDrag();
    }

    private async Task ImportPartAsync(InputFileChangeEventArgs eventArgs)
    {
        try
        {
            var part = await ObjectJsonSerializer.ImportAsync<CircuitPart>(eventArgs.File);
            if (!await ConfirmDiscardChangesAsync())
            {
                return;
            }

            SetEditedPart(part);
            MessageService.Show("Part imported", "Part imported successfully.", MessageType.Success);
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException)
        {
            MessageService.Show("Part import failed", exception.Message, MessageType.Danger);
        }
    }

    private async Task ExportPartAsync()
    {
        await using var stream = new MemoryStream();
        await ObjectJsonSerializer.ExportAsync<CircuitPart>(EditorStateService.PartEditor.EditedPart, stream);
        stream.Position = 0;

        using var streamReference = new DotNetStreamReference(stream);
        await JS.InvokeVoidAsync("gwire.downloadFileFromStream", CreateFileName(EditorStateService.PartEditor.EditedPart.Name), streamReference);
    }

    private async Task ImportSvgAsync(InputFileChangeEventArgs eventArgs)
    {
        try
        {
            await using var stream = eventArgs.File.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var svgMarkup = await reader.ReadToEndAsync();
            if (!svgMarkup.Contains("<svg", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("The selected file does not contain an SVG image.");
            }

            ExecuteEdit(() =>
            {
                EditorStateService.PartEditor.EditedPart.SvgMarkup = svgMarkup;
                EditorStateService.PartEditor.EditedPart.SvgLocalPosition = new Point();
                EditorStateService.PartEditor.EditedPart.SvgRotation = 0;
            });
            MessageService.Show("SVG background imported", "SVG background imported successfully.", MessageType.Success);
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException)
        {
            MessageService.Show("SVG import failed", exception.Message, MessageType.Danger);
        }
    }

    /// <summary>
    /// Removes the SVG background from the edited part.
    /// </summary>
    private void RemoveSvg()
    {
        EditorStateService.PartEditor.EditedPart.SvgMarkup = string.Empty;
        EditorStateService.PartEditor.EditedPart.SvgLocalPosition = new Point();
        EditorStateService.PartEditor.EditedPart.SvgRotation = 0;
    }

    /// <summary>
    /// Returns a safe file name for the part, replacing invalid characters with underscores and appending ".part.json" extension. If the name is empty or whitespace, it defaults to "part.json".
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static string CreateFileName(string name)
    {
        var safeName = string.Concat(name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character)).Trim();
        return string.IsNullOrWhiteSpace(safeName) ? "part.json" : $"{safeName}.part.json";
    }
}
