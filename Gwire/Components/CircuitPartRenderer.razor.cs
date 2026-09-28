using Gwire.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Text;

namespace Gwire.Components;

public partial class CircuitPartRenderer
{
    private bool isDragging;
    private double dragStartClientX;
    private double dragStartClientY;
    private double dragStartPartX;
    private double dragStartPartY;

    [Parameter, EditorRequired]
    public CircuitPart CircuitPart { get; set; } = default!;

    private string PartTransform => $"translate({CircuitPart.SchemeX} {CircuitPart.SchemeY})";

    private string? SvgImageSource => string.IsNullOrWhiteSpace(CircuitPart.SvgMarkup)
        ? null
        : $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(CircuitPart.SvgMarkup))}";

    private void StartDrag(PointerEventArgs eventArgs)
    {
        isDragging = true;
        dragStartClientX = eventArgs.ClientX;
        dragStartClientY = eventArgs.ClientY;
        dragStartPartX = CircuitPart.SchemeX;
        dragStartPartY = CircuitPart.SchemeY;
    }

    private void MoveDraggedPart(PointerEventArgs eventArgs)
    {
        if (!isDragging)
        {
            return;
        }

        CircuitPart.SchemeX = dragStartPartX + eventArgs.ClientX - dragStartClientX;
        CircuitPart.SchemeY = dragStartPartY + eventArgs.ClientY - dragStartClientY;
    }

    private void EndDrag(PointerEventArgs eventArgs) => ResetDrag();

    private void CancelDrag(PointerEventArgs eventArgs) => ResetDrag();

    private void ResetDrag()
    {
        isDragging = false;
        dragStartClientX = 0;
        dragStartClientY = 0;
        dragStartPartX = 0;
        dragStartPartY = 0;
    }
}
