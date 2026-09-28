using Gwire.Models;
using Gwire.Models.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Gwire.Pages;

public partial class SchemeEditor : ComponentBase
{
    private readonly Circuit Circuit = new();
    private CircuitPart? draggedPart;
    private double dragStartClientX;
    private double dragStartClientY;
    private double dragStartPartX;
    private double dragStartPartY;
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
        dragStartClientX = eventArgs.ClientX;
        dragStartClientY = eventArgs.ClientY;
        dragStartPartX = part.SchemeX;
        dragStartPartY = part.SchemeY;
    }

    private void MoveDraggedPart(PointerEventArgs eventArgs)
    {
        if (draggedPart is null)
        {
            return;
        }

        draggedPart.SchemeX = dragStartPartX + eventArgs.ClientX - dragStartClientX;
        draggedPart.SchemeY = dragStartPartY + eventArgs.ClientY - dragStartClientY;
    }

    private void EndDrag(PointerEventArgs eventArgs) => ResetDrag();

    private void CancelDrag(PointerEventArgs eventArgs) => ResetDrag();

    private void ResetDrag()
    {
        draggedPart = null;
        dragStartClientX = 0;
        dragStartClientY = 0;
        dragStartPartX = 0;
        dragStartPartY = 0;
    }

}
