using Gwire.Models.Base;
using static Gwire.Pages.PartEdit.PartEditor;

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
    /// The zero-based index of the selected connection-group tab within the active state.
    /// </summary>
    public int ConnectionGroupTabIndex { get; set; }

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

    internal void RestoreSnapshot(PartSnapshot snapshot)
    {
        // Each restoration gets a fresh graph; further edits must not mutate the stored snapshot.
        EditedPart = snapshot.Part.Clone();
        for (var index = 0; index < EditedPart.StateGroups.Count; index++)
        {
            var source = snapshot.Part.StateGroups[index];
            var target = EditedPart.StateGroups[index];
            target.ActiveState = target.States.ElementAtOrDefault(source.ActiveState is null ? -1 : source.States.IndexOf(source.ActiveState));
        }
        SelectedPointInEditor = EditedPart.Points.ElementAtOrDefault(snapshot.SelectedPointIndex);
        ActiveStateGroupIndex = snapshot.StateGroupIndex;
        ActiveStateInEditor = ActiveStateGroupInEditor.ActiveState;
        ActiveConnectionGroup = ActiveStateInEditor?.ConnectionGroups.ElementAtOrDefault(snapshot.ActiveGroupIndex);
        SelectedTag = string.Empty;
    }
}
