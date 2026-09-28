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
    private double dragOffsetX;
    private double dragOffsetY;
    private int selectedPartIndex = -1;

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

    private void AddSelectedPart()
    {
        if (selectedPartIndex < 0 || selectedPartIndex >= GwireParts.Parts.Count)
        {
            return;
        }

        if (GwireParts.Parts[selectedPartIndex] is not CircuitPart selectedPart)
        {
            return;
        }

        var part = selectedPart.Clone();
        var partNumber = Circuit.Parts.OfType<CircuitPart>().Count();
        part.SchemeX = 150 + partNumber * 25;
        part.SchemeY = 150 + partNumber * 25;
        Circuit.Parts.Add(part);
    }

    private void StartPartDrag(CircuitPart part, PointerEventArgs eventArgs)
    {
        draggedPart = part;
        dragOffsetX = eventArgs.OffsetX - part.SchemeX;
        dragOffsetY = eventArgs.OffsetY - part.SchemeY;
    }

    private void MoveDraggedItem(PointerEventArgs eventArgs)
    {
        if (draggedPart is not null)
        {
            draggedPart.SchemeX = eventArgs.OffsetX - dragOffsetX;
            draggedPart.SchemeY = eventArgs.OffsetY - dragOffsetY;
            return;
        }

    }

    private void EndDrag(PointerEventArgs eventArgs) => CancelDrag(eventArgs);

    private void CancelDrag(PointerEventArgs eventArgs)
    {
        draggedPart = null;
        dragOffsetX = 0;
        dragOffsetY = 0;
    }

    private static string PartTransform(CircuitPart part) => $"translate({part.SchemeX} {part.SchemeY})";

    private static string? SvgImageSource(CircuitPart part) => string.IsNullOrWhiteSpace(part.SvgMarkup)
        ? null
        : $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(part.SvgMarkup))}";
}
