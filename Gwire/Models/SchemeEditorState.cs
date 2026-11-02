using Gwire.Models.Base;

namespace Gwire.Models;

/// <summary>
/// Holds the scheme and editor settings across navigation within the application.
/// </summary>
public sealed class SchemeEditorState
{
    /// <summary>
    /// The scheme currently being edited, including its parts, connections, and metadata.
    /// </summary>
    public Circuit Circuit { get; set; } = new();

    /// <summary>
    /// The part or cable selected on the canvas, or null when nothing is selected.
    /// </summary>
    public BaseCircuitPart? SelectedPart { get; set; }

    /// <summary>
    /// The source and ground terminal pairs used by the scheme editor, including their attachments.
    /// </summary>
    public List<SourceGroundPair> SourceGroundPairs { get; } = [new()];

    /// <summary>
    /// The canvas scale factor, where 1 represents 100 percent zoom.
    /// </summary>
    public float Zoom { get; set; } = 1;

    /// <summary>
    /// The horizontal coordinate of the visible canvas origin in scheme units.
    /// </summary>
    public float ViewportX { get; set; }

    /// <summary>
    /// The vertical coordinate of the visible canvas origin in scheme units.
    /// </summary>
    public float ViewportY { get; set; }

    /// <summary>
    /// Whether pan mode is selected with the toolbar button, excluding the temporary Space-key override.
    /// </summary>
    public bool PanEnabled { get; set; }

    /// <summary>
    /// Whether the panel for editing the scheme name, description, and tags is expanded.
    /// </summary>
    public bool ShowDetails { get; set; }

    /// <summary>
    /// Whether source and ground terminals are displayed on the canvas.
    /// </summary>
    public bool ShowSourceGroundPair { get; set; }

    /// <summary>
    /// The draft text entered in the tag input before a suggested tag is added to the scheme.
    /// </summary>
    public string SelectedTag { get; set; } = string.Empty;

    /// <summary>
    /// Whether tag suggestions are enabled for display below the tag input.
    /// </summary>
    public bool ShowTagSuggestions { get; set; }

    /// <summary>
    /// The zero-based index of the tag suggestion highlighted for keyboard selection.
    /// </summary>
    public int ActiveTagSuggestionIndex { get; set; }

    /// <summary>
    /// The name, description, and tags recorded when the scheme was created or loaded, used to detect unsaved metadata changes.
    /// </summary>
    public (string Name, string Description, string[] Tags) UnchangedSchemeDetails { get; set; } = (string.Empty, string.Empty, []);
}
