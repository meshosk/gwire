using Gwire.Models;
using Gwire.Models.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Text;

namespace Gwire.Pages;

public partial class SchemeEditor : ComponentBase
{
    private readonly Circuit Circuit = new();
    private CircuitPart? draggedPart;
    private ConnectionPoint? draggedCablePoint;
    private double dragOffsetX;
    private double dragOffsetY;

    private void AddCable()
    {
        var cableNumber = Circuit.Parts.OfType<CablePart>().Count() + 1;
        var cable = new CablePart
        {
            Name = $"Cable {cableNumber}"
        };

        cable.Points[0].LocalX = 300;
        cable.Points[0].LocalY = 300 + (cableNumber - 1) * 40;
        cable.Points[1].LocalX = 500;
        cable.Points[1].LocalY = 300 + (cableNumber - 1) * 40;
        Circuit.Parts.Add(cable);
    }

    private void StartPartDrag(CircuitPart part, PointerEventArgs eventArgs)
    {
        draggedCablePoint = null;
        draggedPart = part;
        dragOffsetX = eventArgs.OffsetX - part.SchemeX;
        dragOffsetY = eventArgs.OffsetY - part.SchemeY;
    }

    private void StartCablePointDrag(ConnectionPoint point, PointerEventArgs eventArgs)
    {
        draggedPart = null;
        draggedCablePoint = point;
        dragOffsetX = eventArgs.OffsetX - point.LocalX;
        dragOffsetY = eventArgs.OffsetY - point.LocalY;
    }

    private void MoveDraggedItem(PointerEventArgs eventArgs)
    {
        if (draggedPart is not null)
        {
            draggedPart.SchemeX = eventArgs.OffsetX - dragOffsetX;
            draggedPart.SchemeY = eventArgs.OffsetY - dragOffsetY;
            return;
        }

        if (draggedCablePoint is not null)
        {
            draggedCablePoint.LocalX = eventArgs.OffsetX - dragOffsetX;
            draggedCablePoint.LocalY = eventArgs.OffsetY - dragOffsetY;
        }
    }

    private void EndDrag(PointerEventArgs eventArgs) => CancelDrag(eventArgs);

    private void CancelDrag(PointerEventArgs eventArgs)
    {
        draggedPart = null;
        draggedCablePoint = null;
        dragOffsetX = 0;
        dragOffsetY = 0;
    }

    private static string PartTransform(CircuitPart part) => $"translate({part.SchemeX} {part.SchemeY})";

    private static string? SvgImageSource(CircuitPart part) => string.IsNullOrWhiteSpace(part.SvgMarkup)
        ? null
        : $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(part.SvgMarkup))}";
}
