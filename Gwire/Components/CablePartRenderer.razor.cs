using Gwire.Models;
using Gwire.Models.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Gwire.Components;

public partial class CablePartRenderer
{
    private ConnectionPoint? draggedPoint;
    private double dragStartClientX;
    private double dragStartClientY;
    private double dragStartPointX;
    private double dragStartPointY;

    [Parameter, EditorRequired]
    public CablePart CablePart { get; set; } = default!;

    private void StartDrag(ConnectionPoint point, PointerEventArgs eventArgs)
    {
        draggedPoint = point;
        dragStartClientX = eventArgs.ClientX;
        dragStartClientY = eventArgs.ClientY;
        dragStartPointX = point.LocalX;
        dragStartPointY = point.LocalY;
    }

    private void MoveDraggedPoint(PointerEventArgs eventArgs)
    {
        if (draggedPoint is null)
        {
            return;
        }

        draggedPoint.LocalX = dragStartPointX + eventArgs.ClientX - dragStartClientX;
        draggedPoint.LocalY = dragStartPointY + eventArgs.ClientY - dragStartClientY;
    }

    private void EndDrag(PointerEventArgs eventArgs) => ResetDrag();

    private void CancelDrag(PointerEventArgs eventArgs) => ResetDrag();

    private void ResetDrag()
    {
        draggedPoint = null;
        dragStartClientX = 0;
        dragStartClientY = 0;
        dragStartPointX = 0;
        dragStartPointY = 0;
    }
}
