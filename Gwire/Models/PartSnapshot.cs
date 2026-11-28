namespace Gwire.Models;

/// <summary>
/// Undo snapshot of the part editor: a cloned part and the editor selection as indices into it.
/// Captured and restored by <see cref="PartEditorState"/>; -1 means nothing selected.
/// </summary>
public sealed record PartSnapshot(CircuitPart Part, int SelectedPointIndex, int StateGroupIndex, int StateIndex, int ConnectionGroupIndex);
