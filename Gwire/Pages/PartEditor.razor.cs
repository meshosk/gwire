using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Serialization;
using Gwire.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text;

namespace Gwire.Pages;

public partial class PartEditor : ComponentBase, IAsyncDisposable
{
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private GwirePartsService GwireParts { get; set; } = default!;

    [Inject]
    private MessageService MessageService { get; set; } = default!;

    [Inject]
    private ConfirmationService ConfirmationService { get; set; } = default!;

    private bool HasUnsavedChanges => history.HasEntries;

    private async Task<bool> ConfirmDiscardChangesAsync() => !HasUnsavedChanges ||
        await ConfirmationService.ShowAsync("Unsaved changes",
            "This part has unsaved changes. Do you want to discard them and continue?") == ConfirmationResult.YES;

    private async Task ConfirmNavigationAsync(LocationChangingContext context)
    {
        if (!await ConfirmDiscardChangesAsync())
        {
            context.PreventNavigation();
        }
    }

    /// <summary>
    /// Currently edited part
    /// </summary>
    public CircuitPart EditedPart { get; private set; } = new();

    /// <summary>
    /// Part selected in the editor
    /// </summary>
    private ConnectionPoint? SelectedPointInEditor { get; set; }
    /// <summary>
    /// Edited part's active state. This is the state that is currently being edited in the editor.
    /// </summary>
    private PartState? ActiveStateInEditor { get; set; }
    /// <summary>
    /// Parts that is currently being edited in the editor. 
    /// </summary>
    private ConnectionGroup? ActiveConnectionGroup { get; set; }
    
    
    private string selectedTag = string.Empty;
    private Guid? selectedPartId;

    private IEnumerable<CircuitPart> AvailableParts => GwireParts.Parts;

    private IReadOnlyList<string> AvailableTags => GwireParts.Tags
        .Where(tag => !EditedPart.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
        .ToList();

    #region Drags

    private ConnectionPoint? draggedPoint;
    private Point dragOffset;
    private bool pointWasDragged;

    private void StartPointDrag(ConnectionPoint point, PointerEventArgs eventArgs)
    {
        if (pendingEdit is not null)
        {
            return;
        }

        SelectedPointInEditor = point;
        BeginEdit();
        draggedPoint = point;
        var pointer = new Point(eventArgs);
        dragOffset = new Point(pointer.X - point.LocalPosition.X, pointer.Y - point.LocalPosition.Y);
        pointWasDragged = false;
    }

    private void EndPointDrag(PointerEventArgs eventArgs)
    {
        if (draggedPoint is not null && !pointWasDragged && ActiveConnectionGroup is not null)
        {
            if (!ActiveConnectionGroup.ConnectedPints.Remove(draggedPoint))
            {
                ActiveConnectionGroup.ConnectedPints.Add(draggedPoint);
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
        if (draggedPoint is not null)
        {
            CancelEdit();
        }
        ResetPointDrag();
    }

    private void ResetPointDrag()
    {
        draggedPoint = null;
        dragOffset = new Point();
        pointWasDragged = false;
    }

    private void MoveDraggedItem(PointerEventArgs eventArgs)
    {
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

    #endregion

    private void AddPoint()
    {
        var count = EditedPart.Points.Count;
        var point = new ConnectionPoint(EditedPart)
        {
            Label = $"Point {count + 1}",
            LocalPosition = new Point(120 + (count % 5) * 140, 120 + (count / 5) * 100)
        };

        EditedPart.Points.Add(point);
        SelectedPointInEditor = point;
    }

    private void AddConnectionGroup(PartState state)
    {
        var group = new ConnectionGroup();
        state.ConnectionGroups.Add(group);
    }

    private void AddState()
    {
        var state = new PartState { Label = $"State {EditedPart.States.Count + 1}" };
        EditedPart.States.Add(state);
        SelectActiveState(state);
        ActiveConnectionGroup = null;
    }

    private void RemoveState(PartState state)
    {
        if (!EditedPart.States.Remove(state))
        {
            return;
        }

        if (ReferenceEquals(ActiveStateInEditor, state) || ReferenceEquals(EditedPart.ActiveState, state))
        {
            ActiveStateInEditor = EditedPart.States.FirstOrDefault();
            EditedPart.ActiveState = ActiveStateInEditor;
        }

        if (ActiveConnectionGroup is not null && state.ConnectionGroups.Contains(ActiveConnectionGroup))
        {
            ActiveConnectionGroup = null;
        }
    }

    private void SelectActiveState(PartState state)
    {
        ActiveStateInEditor = state;
        EditedPart.ActiveState = state;
    }

    private void AddTag()
    {
        if (string.IsNullOrWhiteSpace(selectedTag) || !GwireParts.Tags.Contains(selectedTag, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        EditedPart.Tags.Add(selectedTag);
        selectedTag = string.Empty;
    }

    private void RemoveTag(string tag)
    {
        EditedPart.Tags.Remove(tag);
    }

    private void RemoveSelectedPoint()
    {
        if (SelectedPointInEditor is null)
        {
            return;
        }

        EditedPart.Points.Remove(SelectedPointInEditor);
        foreach (var state in EditedPart.States)
        {
            foreach (var group in state.ConnectionGroups)
            {
                group.ConnectedPints.Remove(SelectedPointInEditor);
            }
        }

        if (ReferenceEquals(draggedPoint, SelectedPointInEditor))
        {
            draggedPoint = null;
        }

        SelectedPointInEditor = null;
    }

    private void RemoveActiveConnectionGroup(ConnectionGroup group)
    {
        var state = EditedPart.States.FirstOrDefault(candidate => candidate.ConnectionGroups.Contains(group));
        if (state is null)
        {
            return;
        }

        state.ConnectionGroups.Remove(group);
        if (ReferenceEquals(ActiveConnectionGroup, group))
        {
            ActiveConnectionGroup = null;
        }
    }

    private string PointClass(ConnectionPoint point) =>
        ReferenceEquals(point, SelectedPointInEditor) ? "connection-point is-selected" : "connection-point";

    private void AddRemovePointToGroup(ConnectionGroup group, ConnectionPoint? selectedPoint)
    {
        if (selectedPoint is null)
        {
            return;
        }

        if (group.ConnectedPints.Contains(selectedPoint))
        {
            group.ConnectedPints.Remove(selectedPoint);
        }
        else
        {
            group.ConnectedPints.Add(selectedPoint);
        }
    }

    private void SavePartToService()
    {
        var savedAsCopy = EditedPart.IsFromRepo || AvailableParts.Any(existing => existing.IsFromRepo && existing.Id == EditedPart.Id);
        var partToSave = EditedPart;
        if (savedAsCopy)
        {
            partToSave = EditedPart.Clone();
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
            history = new();
        }
        selectedPartId = EditedPart.Id;
        MessageService.Show("Part saved", savedAsCopy ? "Part saved as your own copy." : "Part saved to the parts service.", MessageType.Success);
    }

    private async Task NewPart()
    {
        if (!await ConfirmDiscardChangesAsync())
        {
            return;
        }

        SetEditedPart(new CircuitPart());
        selectedPartId = null;
        selectedTag = string.Empty;
    }

    private async Task LoadSelectedPart()
    {
        var part = AvailableParts.FirstOrDefault(candidate => candidate.Id == selectedPartId);
        if (part is null || !await ConfirmDiscardChangesAsync())
        {
            return;
        }

        SetEditedPart(part.Clone());
        selectedPartId = part.Id;
        MessageService.Show("Part loaded", "Part loaded for editing.", MessageType.Success);
    }

    private void SetEditedPart(CircuitPart part)
    {
        pendingEdit = null;
        history = new();
        EditedPart = part;

        if (EditedPart.Width <= 0)
        {
            EditedPart.Width = 500;
        }

        if (EditedPart.Height <= 0)
        {
            EditedPart.Height = 500;
        }

        SelectedPointInEditor = null;
        ActiveStateInEditor = EditedPart.ActiveState is { } activeState && EditedPart.States.Contains(activeState)
            ? activeState
            : EditedPart.States.FirstOrDefault();
        EditedPart.ActiveState = ActiveStateInEditor;
        ActiveConnectionGroup = null;
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
            selectedPartId = null;
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
        await ObjectJsonSerializer.ExportAsync<CircuitPart>(EditedPart, stream);
        stream.Position = 0;

        using var streamReference = new DotNetStreamReference(stream);
        await JS.InvokeVoidAsync("gwire.downloadFileFromStream", CreateFileName(EditedPart.Name), streamReference);
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
                EditedPart.SvgMarkup = svgMarkup;
                EditedPart.SvgLocalPosition = new Point();
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
        EditedPart.SvgMarkup = string.Empty;
        EditedPart.SvgLocalPosition = new Point();
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
