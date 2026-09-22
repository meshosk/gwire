namespace Gwire.Models.Base;

/// <summary>
/// Shared definition for every element that can occur in a wiring scheme.
/// A wire and a user-defined part differ only in their specialised data, not
/// in how their terminals and internal connections are represented.
/// </summary>
public abstract class CircuitPart
{
    /// <summary>
    /// ID - not sure if it is needed
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();
    /// <summary>
    /// Part name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Description - text only.
    /// </summary>
    public string Description { get; set; } = string.Empty;

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
    /// List of connection points that can connect part into circuit.
    /// </summary>
    public List<ConnectionPoint> Points { get; set; } = new();
    /// <summary>
    /// Defines states. Each state defines how points are interconnected. Active is only one.
    /// </summary>
    public List<PartState> States { get; set; } = new();

    /// <summary>
    /// Defines which state is active.
    /// </summary>
    public PartState? ActiveState { get; internal set; } = null;
}
