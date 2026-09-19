using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Text;
using System.Text.Json;

namespace Gwire.Pages;

public partial class PartEditor : ComponentBase
{
    private readonly CustomPartJsonSerializer partSerializer = new();

    /// <summary>
    /// Currently edited part
    /// </summary>
    public CustomPart Part { get; private set; } = new();


    private ConnectionPoint? SelectedPoint { get; set; }
    private PartState? ActiveState { get; set; }
    private ConnectionGroup? ActiveConnectionGroup { get; set; }
    private string? ImportMessage { get; set; }
    private string ImportMessageClass { get; set; } = "alert-success";

    private string ExportUri => $"data:application/json;charset=utf-8,{Uri.EscapeDataString(partSerializer.Serialize(Part))}";
    private string ExportFileName => CreateFileName(Part.Name);


    #region Drags

    private ConnectionPoint? draggedPoint;
    private bool isSvgDragged;
    private double dragOffsetX;
    private double dragOffsetY;
    private bool pointWasDragged;

    private void StartPointDrag(ConnectionPoint point, PointerEventArgs eventArgs)
    {
        isSvgDragged = false;
        SelectedPoint = point;
        draggedPoint = point;
        dragOffsetX = eventArgs.OffsetX - point.LocalX;
        dragOffsetY = eventArgs.OffsetY - point.LocalY;
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
        isSvgDragged = false;
        dragOffsetX = 0;
        dragOffsetY = 0;
        pointWasDragged = false;
    }

    private void MoveDraggedItem(PointerEventArgs eventArgs)
    {
        if (draggedPoint is not null)
        {
            var nextX = Math.Clamp(eventArgs.OffsetX - dragOffsetX, 15, 785);
            var nextY = Math.Clamp(eventArgs.OffsetY - dragOffsetY, 15, 435);
            pointWasDragged |= Math.Abs(nextX - draggedPoint.LocalX) > 2 || Math.Abs(nextY - draggedPoint.LocalY) > 2;
            draggedPoint.LocalX = nextX;
            draggedPoint.LocalY = nextY;
            return;
        }

        if (isSvgDragged)
        {
            Part.SvgLocalX = Math.Clamp(eventArgs.OffsetX - dragOffsetX, -800, 800);
            Part.SvgLocalY = Math.Clamp(eventArgs.OffsetY - dragOffsetY, -450, 450);
        }
    }

    #endregion

    private void AddPoint()
    {
        var count = Part.Points.Count;
        var point = new ConnectionPoint
        {
            Label = $"Point {count + 1}",
            LocalX = 120 + (count % 5) * 140,
            LocalY = 120 + (count / 5) * 100
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
        ActiveState = state;
        ActiveConnectionGroup = null;
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

    private void StartSvgDrag(PointerEventArgs eventArgs)
    {
        SelectedPoint = null;
        draggedPoint = null;
        isSvgDragged = true;
        dragOffsetX = eventArgs.OffsetX - Part.SvgLocalX;
        dragOffsetY = eventArgs.OffsetY - Part.SvgLocalY;
        pointWasDragged = false;
    }

    private async Task ImportPartAsync(InputFileChangeEventArgs eventArgs)
    {
        try
        {
            var file = eventArgs.File;


            await using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var json = await reader.ReadToEndAsync();
            var importedPart = partSerializer.Deserialize(json)
                ?? throw new InvalidDataException("The JSON file does not contain a part.");

            ValidateImport(importedPart);
            Part = importedPart;
            SelectedPoint = null;
            ActiveState = Part.States.FirstOrDefault();
            ActiveConnectionGroup = null;
            CancelPointDrag(new PointerEventArgs());
            ImportMessage = "Part imported successfully.";
            ImportMessageClass = "alert-success";
        }
        catch (JsonException)
        {
            ImportMessage = "The selected file is not valid part JSON.";
            ImportMessageClass = "alert-danger";
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException)
        {
            ImportMessage = exception.Message;
            ImportMessageClass = "alert-danger";
        }
    }

    private async Task ImportSvgAsync(InputFileChangeEventArgs eventArgs)
    {
        try
        {
            var file = eventArgs.File;
 
            await using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var svgMarkup = await reader.ReadToEndAsync();
            if (!svgMarkup.Contains("<svg", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("The selected file does not contain an SVG image.");
            }

            Part.SvgMarkup = svgMarkup;
            Part.SvgLocalX = 0;
            Part.SvgLocalY = 0;
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
        Part.SvgLocalX = 0;
        Part.SvgLocalY = 0;
    }

    private static void ValidateImport(CustomPart importedPart)
    {
        foreach (var point in importedPart.Points)
        {
            if (double.IsNaN(point.LocalX) || double.IsInfinity(point.LocalX) || point.LocalX is < 15 or > 785 ||
                double.IsNaN(point.LocalY) || double.IsInfinity(point.LocalY) || point.LocalY is < 15 or > 435)
            {
                throw new InvalidDataException("A point position is outside the editor bounds.");
            }
        }

        var partPoints = importedPart.Points.ToHashSet();
        foreach (var group in importedPart.States.SelectMany(state => state.ConnectionGroups))
        {
            if (group.ConnectedPints.Any(point => !partPoints.Contains(point)))
            {
                throw new InvalidDataException("A connection group references a point outside the part.");
            }
        }
    }

    private static string CreateFileName(string name)
    {
        var safeName = string.Concat(name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character)).Trim();
        return string.IsNullOrWhiteSpace(safeName) ? "part.json" : $"{safeName}.part.json";
    }

    private string? SvgImageSource => string.IsNullOrWhiteSpace(Part.SvgMarkup)
        ? null
        : $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(Part.SvgMarkup))}";

}
