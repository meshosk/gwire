using Gwire.Models.Base;

namespace Gwire.Models;

/// <summary>
/// Holds the edited part and editor selections across navigation within the application.
/// </summary>
public sealed class PartEditorState
{
    /// <summary>
    /// The part currently being edited, including its points, states, and SVG background.
    /// </summary>
    public CircuitPart EditedPart { get; set; } = new();

    /// <summary>
    /// The connection point selected for editing, or null when no point is selected.
    /// </summary>
    public ConnectionPoint? SelectedPointInEditor { get; set; }

    /// <summary>
    /// The state currently being edited within the active state group.
    /// </summary>
    public PartState? ActiveStateInEditor { get; set; }

    /// <summary>
    /// The zero-based index of the state group currently being edited.
    /// </summary>
    public int ActiveStateGroupIndex { get; set; }

    /// <summary>
    /// The state group selected by ActiveStateGroupIndex in the edited part.
    /// </summary>
    public PartStateGroup ActiveStateGroupInEditor => EditedPart.StateGroups[ActiveStateGroupIndex];

    /// <summary>
    /// The connection group whose point membership can be toggled by clicking canvas points, or null when none is active.
    /// </summary>
    public ConnectionGroup? ActiveConnectionGroup { get; set; }

    /// <summary>
    /// The zero-based index of the selected sidebar tab: Part, Points, or States.
    /// </summary>
    public int ActiveTabIndex { get; set; }

    /// <summary>
    /// The draft text entered in the tag input before a suggested tag is added to the part.
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
    /// Captures a clone of the edited part and the selection as indices into it.
    /// </summary>
    public PartSnapshot CaptureSnapshot() => new(
        EditedPart.Clone(),
        SelectedPointInEditor is null ? -1 : EditedPart.Points.IndexOf(SelectedPointInEditor),
        ActiveStateGroupIndex,
        ActiveStateInEditor is null ? -1 : ActiveStateGroupInEditor.States.IndexOf(ActiveStateInEditor),
        ActiveConnectionGroup is null ? -1 : ActiveStateInEditor?.ConnectionGroups.IndexOf(ActiveConnectionGroup) ?? -1);

    /// <summary>
    /// Restores the edited part and the selection captured by <see cref="CaptureSnapshot"/>.
    /// </summary>
    public void RestoreSnapshot(PartSnapshot snapshot)
    {
        // Each restoration gets a fresh graph; further edits must not mutate the stored snapshot.
        EditedPart = snapshot.Part.Clone();
        SelectedPointInEditor = EditedPart.Points.ElementAtOrDefault(snapshot.SelectedPointIndex);
        ActiveStateGroupIndex = snapshot.StateGroupIndex;
        ActiveStateInEditor = ActiveStateGroupInEditor.States.ElementAtOrDefault(snapshot.StateIndex);
        ActiveConnectionGroup = ActiveStateInEditor?.ConnectionGroups.ElementAtOrDefault(snapshot.ConnectionGroupIndex);
        SelectedTag = string.Empty;
    }
}
