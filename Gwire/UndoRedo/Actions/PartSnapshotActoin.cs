using Gwire.Models;

namespace Gwire.UndoRedo.Actions;

/// <summary>
/// Undoes and redoes a part edit by swapping the stored snapshot with the current editor state.
/// </summary>
public sealed class PartSnapshotAction(PartEditorState editor, PartSnapshot initialSnapshot) : IUndoableAction
{
    private PartSnapshot snapshot = initialSnapshot;

    public void Undo() => SwapSnapshot();
    public void Redo() => SwapSnapshot();

    private void SwapSnapshot()
    {
        var current = editor.CaptureSnapshot();
        editor.RestoreSnapshot(snapshot);
        snapshot = current;
    }
}
