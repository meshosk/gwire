using Gwire.Models;

namespace Gwire.Pages;

public partial class PartEditor
{
    /// <summary>
    /// Captures a snapshot of the current state of the part editor, including the edited part, selected point index, state group index, and active group index.
    /// </summary>
    /// <param name="Part"></param>
    /// <param name="SelectedPointIndex"></param>
    /// <param name="StateGroupIndex"></param>
    /// <param name="ActiveGroupIndex"></param>
    public sealed record PartSnapshot(CircuitPart Part, int SelectedPointIndex, int StateGroupIndex, int ActiveGroupIndex)
    {
        public static PartSnapshot CaptureSnapshot(PartEditorState editor)
        {
            var part = editor.EditedPart.Clone();
            for (var index = 0; index < part.StateGroups.Count; index++)
            {
                var source = editor.EditedPart.StateGroups[index];
                var target = part.StateGroups[index];
                target.ActiveState = target.States.ElementAtOrDefault(source.ActiveState is null ? -1 : source.States.IndexOf(source.ActiveState));
            }

            return new(
                part,
                editor.SelectedPointInEditor is null ? -1 : editor.EditedPart.Points.IndexOf(editor.SelectedPointInEditor),
                editor.ActiveStateGroupIndex,
                editor.ActiveConnectionGroup is null ? -1 : editor.ActiveStateInEditor?.ConnectionGroups.IndexOf(editor.ActiveConnectionGroup) ?? -1);
        }
    }
}
