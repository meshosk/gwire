using Gwire.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Text;

namespace Gwire.Components;

/// <summary>
///
/// </summary>
public partial class PartSvgBackground
{
    private readonly string clipId = $"part-svg-clip-{Guid.NewGuid():N}";
    private bool isDragging;
    private Point dragOffset;
    private Enums.PartSvgBackground.ResizeDirection resizeDirection;
    private Point resizeStartClient;
    private int resizeStartWidth;
    private int resizeStartHeight;

    [Parameter, EditorRequired]
    public CircuitPart Part { get; set; } = default!;

    [Parameter]
    public EventCallback<CircuitPart> PartChanged { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string ClipPathUrl => $"url(#{clipId})";

    private bool IsBackgroundInteractionActive =>
        isDragging || resizeDirection != Enums.PartSvgBackground.ResizeDirection.None;

    private string? ImageSource => string.IsNullOrWhiteSpace(Part.SvgMarkup)
        ? null
        : $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(Part.SvgMarkup))}";

    private void StartDrag(PointerEventArgs eventArgs)
    {
        resizeDirection = Enums.PartSvgBackground.ResizeDirection.None;
        isDragging = true;
        var pointer = new Point((float)eventArgs.OffsetX, (float)eventArgs.OffsetY);
        dragOffset = new Point(pointer.X - Part.SvgLocalPosition.X, pointer.Y - Part.SvgLocalPosition.Y);
    }

    private void StartResize(Enums.PartSvgBackground.ResizeDirection direction, PointerEventArgs eventArgs)
    {
        isDragging = false;
        resizeDirection = direction;
        resizeStartClient = new Point(eventArgs);
        resizeStartWidth = Part.Width;
        resizeStartHeight = Part.Height;
    }

    private async Task HandlePointerMove(PointerEventArgs eventArgs)
    {
        if (resizeDirection != Enums.PartSvgBackground.ResizeDirection.None)
        {
            var pointer = new Point(eventArgs);
            if (resizeDirection is Enums.PartSvgBackground.ResizeDirection.Right or Enums.PartSvgBackground.ResizeDirection.Corner)
            {
                Part.Width = Math.Max(1, resizeStartWidth + (int)MathF.Round(pointer.X - resizeStartClient.X));
            }

            if (resizeDirection is Enums.PartSvgBackground.ResizeDirection.Bottom or Enums.PartSvgBackground.ResizeDirection.Corner)
            {
                Part.Height = Math.Max(1, resizeStartHeight + (int)MathF.Round(pointer.Y - resizeStartClient.Y));
            }

            await PartChanged.InvokeAsync(Part);
            return;
        }

        if (isDragging)
        {
            var pointer = new Point((float)eventArgs.OffsetX, (float)eventArgs.OffsetY);
            Part.SvgLocalPosition = new Point(pointer.X - dragOffset.X, pointer.Y - dragOffset.Y);
            await PartChanged.InvokeAsync(Part);
            return;
        }
    }

    private void HandlePointerUp(PointerEventArgs eventArgs) => CancelDrag();

    private void HandlePointerCancel(PointerEventArgs eventArgs) => CancelDrag();

    private void CancelDrag()
    {
        isDragging = false;
        resizeDirection = Enums.PartSvgBackground.ResizeDirection.None;
    }
}
