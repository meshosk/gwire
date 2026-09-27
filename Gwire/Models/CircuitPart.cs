using Gwire.Models.Base;

namespace Gwire.Models;

/// <summary>
/// All other parts than cable
/// </summary>
public sealed class CircuitPart : BaseCircuitPart
{
    /// <summary>
    /// Width of the part in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Height of the part in pixels.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// SVG markup used as the visual background of the part.
    /// </summary>
    public string SvgMarkup { get; set; } = string.Empty;
    /// <summary>
    /// Top-left position of the SVG background in the part's local coordinate system.
    /// </summary>
    public double SvgLocalX { get; set; }
    public double SvgLocalY { get; set; }

    /// <summary>
    /// X position of the part in a circuit scheme.
    /// </summary>
    public double SchemeX { get; set; }

    /// <summary>
    /// Y position of the part in a circuit scheme.
    /// </summary>
    public double SchemeY { get; set; }

    /// <summary>
    /// Tags used to search and filter parts in the catalog.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Clones the shared connection graph, dimensions, and the part's tags.
    /// </summary>
    public override CircuitPart Clone()
    {
        var clone = (CircuitPart)base.Clone();
        clone.Width = Width;
        clone.Height = Height;
        clone.Tags = [.. Tags];
        return clone;
    }
}
