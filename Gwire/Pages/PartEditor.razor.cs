using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Serialization;
using Gwire.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text;
using PartSvgBackground = Gwire.Components.Enums.PartSvgBackground;

namespace Gwire.Pages;

public partial class PartEditor : ComponentBase
{
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private GwirePartsService GwireParts { get; set; } = default!;

    /// <summary>
    /// Currently edited part
    /// </summary>
    public CircuitPart Part { get; private set; } = new();


    private ConnectionPoint? SelectedPoint { get; set; }
    private PartState? ActiveState { get; set; }
    private ConnectionGroup? ActiveConnectionGroup { get; set; }
    private string? ImportMessage { get; set; }
    private string ImportMessageClass { get; set; } = "alert-success";
    private string selectedTag = string.Empty;
    private Guid? selectedPartId;

    private IEnumerable<CircuitPart> AvailableParts => GwireParts.Parts.OfType<CircuitPart>();

    private IReadOnlyList<string> AvailableTags => GwireParts.Tags
        .Where(tag => !Part.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
        .ToList();

    #region Drags

    private ConnectionPoint? draggedPoint;
    private Point dragOffset;
    private bool pointWasDragged;

    private void StartPointDrag(ConnectionPoint point, PointerEventArgs eventArgs)
    {
        SelectedPoint = point;
        draggedPoint = point;
        var pointer = new Point((float)eventArgs.OffsetX, (float)eventArgs.OffsetY);
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

        CancelPointDrag(eventArgs);
    }

    private void CancelPointDrag(PointerEventArgs eventArgs)
    {
        draggedPoint = null;
        dragOffset = new Point();
        pointWasDragged = false;
    }

    private void MoveDraggedItem(PointerEventArgs eventArgs)
    {
        if (draggedPoint is not null)
        {
            var pointer = new Point((float)eventArgs.OffsetX, (float)eventArgs.OffsetY);
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
        var count = Part.Points.Count;
        var point = new ConnectionPoint
        {
            Owner = Part,
            Label = $"Point {count + 1}",
            LocalPosition = new Point(120 + (count % 5) * 140, 120 + (count / 5) * 100)
        };

        Part.Points.Add(point);
        SelectedPoint = point;
    }

    private void AddConnectionGroup(PartState state)
    {
        var group = new ConnectionGroup();
        state.ConnectionGroups.Add(group);
    }

    private void AddState()
    {
        var state = new PartState { Label = $"State {Part.States.Count + 1}" };
        Part.States.Add(state);
        SelectActiveState(state);
        ActiveConnectionGroup = null;
    }

    private void SelectActiveState(PartState state)
    {
        ActiveState = state;
        Part.ActiveState = state;
    }

    private void AddTag()
    {
        if (string.IsNullOrWhiteSpace(selectedTag) || !GwireParts.Tags.Contains(selectedTag, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        Part.Tags.Add(selectedTag);
        selectedTag = string.Empty;
    }

    private void RemoveTag(string tag)
    {
        Part.Tags.Remove(tag);
    }

    private void RemoveSelectedPoint()
    {
        if (SelectedPoint is null)
        {
            return;
        }

        Part.Points.Remove(SelectedPoint);
        foreach (var state in Part.States)
        {
            foreach (var group in state.ConnectionGroups)
            {
                group.ConnectedPints.Remove(SelectedPoint);
            }
        }

        if (ReferenceEquals(draggedPoint, SelectedPoint))
        {
            draggedPoint = null;
        }

        SelectedPoint = null;
    }

    private void RemoveActiveConnectionGroup(ConnectionGroup group)
    {
        if (ActiveState is null || ActiveConnectionGroup is null)
        {
            return;
        }

        ActiveState.ConnectionGroups.Remove(group);
        ActiveConnectionGroup = null;
    }

    private string PointClass(ConnectionPoint point) =>
        ReferenceEquals(point, SelectedPoint) ? "connection-point is-selected" : "connection-point";

    private void AddRemovePointToGroup(ConnectionGroup group, ConnectionPoint selectedPoint)
    {
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
        var savedAsCopy = Part.IsFromRepo || AvailableParts.Any(existing => existing.IsFromRepo && existing.Id == Part.Id);
        if (savedAsCopy)
        {
            var copy = Part.Clone();
            copy.Id = Guid.NewGuid();
            copy.IsFromRepo = false;
            SetEditedPart(copy);
        }

        GwireParts.SavePart(Part);
        selectedPartId = Part.Id;
        ImportMessage = savedAsCopy ? "Part saved as your own copy." : "Part saved to the parts service.";
        ImportMessageClass = "alert-success";
    }

    private void LoadSelectedPart()
    {
        var part = AvailableParts.FirstOrDefault(candidate => candidate.Id == selectedPartId);
        if (part is null)
        {
            return;
        }

        SetEditedPart(part.Clone());
        selectedPartId = part.Id;
        ImportMessage = "Part loaded for editing.";
        ImportMessageClass = "alert-success";
    }

    private void SetEditedPart(CircuitPart part)
    {
        Part = part;
        foreach (var point in Part.Points)
        {
            point.Owner = Part;
        }

        if (Part.Width <= 0)
        {
            Part.Width = 500;
        }

        if (Part.Height <= 0)
        {
            Part.Height = 500;
        }

        SelectedPoint = null;
        ActiveState = Part.ActiveState is { } activeState && Part.States.Contains(activeState)
            ? activeState
            : Part.States.FirstOrDefault();
        Part.ActiveState = ActiveState;
        ActiveConnectionGroup = null;
        CancelPointDrag(new PointerEventArgs());
    }

    private async Task ImportPartAsync(InputFileChangeEventArgs eventArgs)
    {
        try
        {
            SetEditedPart(await ObjectJsonSerializer.ImportAsync<CircuitPart>(eventArgs.File));
            selectedPartId = null;
            ImportMessage = "Part imported successfully.";
            ImportMessageClass = "alert-success";
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException)
        {
            ImportMessage = exception.Message;
            ImportMessageClass = "alert-danger";
        }
    }

    private async Task ExportPartAsync()
    {
        await using var stream = new MemoryStream();
        await ObjectJsonSerializer.ExportAsync<CircuitPart>(Part, stream);
        stream.Position = 0;

        using var streamReference = new DotNetStreamReference(stream);
        await JS.InvokeVoidAsync("gwire.downloadFileFromStream", CreateFileName(Part.Name), streamReference);
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

            Part.SvgMarkup = svgMarkup;
            Part.SvgLocalPosition = new Point();
            ImportMessage = "SVG background imported successfully.";
            ImportMessageClass = "alert-success";
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException)
        {
            ImportMessage = exception.Message;
            ImportMessageClass = "alert-danger";
        }
    }

    private void RemoveSvg()
    {
        Part.SvgMarkup = string.Empty;
        Part.SvgLocalPosition = new Point();
    }

    private static string CreateFileName(string name)
    {
        var safeName = string.Concat(name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character)).Trim();
        return string.IsNullOrWhiteSpace(safeName) ? "part.json" : $"{safeName}.part.json";
    }

}
