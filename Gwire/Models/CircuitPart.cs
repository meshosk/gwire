using Gwire.Models.Base;
using System.Text.Json.Serialization;

namespace Gwire.Models;

/// <summary>
/// All other parts than cable
/// </summary>
public sealed class CircuitPart : BaseCircuitPart
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// if true part was loaded from repo
    /// </summary>
    [JsonIgnore]
    public bool IsFromRepo { get; set; }

    /// <summary>
    /// Use this for gerring part name
    /// </summary>
    [JsonIgnore]
    public string DisplayName => IsFromRepo ? $"*{Name}" : Name;

    /// <summary>
    /// Width of the part in pixels.
    /// </summary>
    public int Width { get; set; } = 500;

    /// <summary>
    /// Height of the part in pixels.
    /// </summary>
    public int Height { get; set; } = 500;

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
