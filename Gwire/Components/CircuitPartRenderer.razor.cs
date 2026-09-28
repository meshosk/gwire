using Gwire.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Text;

namespace Gwire.Components;

public partial class CircuitPartRenderer
{
    [Parameter, EditorRequired]
    public CircuitPart CircuitPart { get; set; } = default!;

    [Parameter, EditorRequired]
    public EventCallback<PointerEventArgs> DragStarted { get; set; }

    private string PartTransform => $"translate({CircuitPart.SchemeX} {CircuitPart.SchemeY})";

    private string? SvgImageSource => string.IsNullOrWhiteSpace(CircuitPart.SvgMarkup)
        ? null
        : $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(CircuitPart.SvgMarkup))}";

}
