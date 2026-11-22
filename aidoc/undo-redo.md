# Undo/redo

Every user edit in both editors must be undoable. The implementation is in
`Gwire/Services/UndoRedoService.cs`, `UndoRedoService.HistoryWrapper.cs`, and
`Gwire/UndoRedo/`.

## Service and histories

- `UndoRedoService` keeps independent histories keyed by string. Use the constants in
  `HistoryIds` (`SchemeEditor`, `PartEditor`); do not invent ids inline.
- Get a history with `UndoRedoService.getHistory(HistoryIds.X)`. It returns an
  `UndoRedoHistoryWrapper` with `CanUndo`, `CanRedo`, `HasEntries`, `IsFrozen`,
  `Undo()`, `Redo()`, `Clear()`, `Freeze()`, `Unfreeze()`, `addRedoAction(action)`, and
  a `Changed` event. (The lowercase `getHistory` and `addRedoAction` names are the
  project's chosen names; keep them.)
- `addRedoAction` **records an action that has already been applied**. It does not run it.
  The usual pattern is:

  ```csharp
  if (history.IsFrozen) return;
  var action = new SomeAction(...);
  action.Redo();                 // apply
  history.addRedoAction(action); // record
  ```

  For drags the change is applied continuously during pointer moves and the action is
  recorded on pointer up.
- **Freeze** blocks undo/redo (and, by convention, other editing commands) while an
  interaction or async operation is in progress: dragging, PDF export, scheme import.
  Every `Freeze()` must be balanced by `Unfreeze()`, normally in a `finally` block.
  UI elements disable themselves with `disabled="@history.IsFrozen"` and handlers return
  early when frozen.
- `Clear()` is called when a new document is loaded. `HasEntries` drives the
  "unsaved changes" confirmation.

## Scheme editor actions

One class per kind of change in `Gwire/UndoRedo/Actions/`, each implementing
`IUndoableAction { void Undo(); void Redo(); }`:

`AddPartAction` (factory methods `CreateCable`, `CreatePart`; implemented as an inverted
`RemovePartAction`), `RemovePartAction` (restores index, connections, cable positions, and
source/ground attachments), `MovePartAction`, `MoveCablePointAction`,
`ChangeCableRouteAction`, `ChangeCableColorAction`, `ChangeCoilColorAction`,
`ChangePairColorAction`, `ChangePartStateAction`, `ChangePartValueAction`,
`ChangePotentiometerPositionAction`, `MoveTerminalAction`, `AddSourceGroundPairAction`.

When you add an action:

- Store everything needed to go both ways in the constructor (copy `Point` values with
  `Copy()`, copy connection lists with `ToArray()`).
- Mutate models through their methods (`MoveTo`, `Connect`, `Disconnect`) so change events
  fire, or call `NotifyChanged()` yourself.
- Do not keep references to UI components.

## Part editor snapshots

The part editor records whole-part snapshots instead of fine-grained actions:

- `PartSnapshot.CaptureSnapshot(state)` clones the edited part (preserving which state is
  active in each group) and the selected point/group indices.
- `BeginEdit()` captures the "before" snapshot and freezes history.
- `CompleteEdit()` captures "after", compares both clones serialized as JSON, and adds a
  `PartSnapshotAction` only when they differ. It always unfreezes.
- `CancelEdit()` restores the "before" snapshot.
- `ExecuteEdit(action)` wraps a single synchronous change in Begin/Complete.
- `PartSnapshotAction` swaps the stored snapshot with the current state on both undo and redo.

So in the part editor, never mutate `EditedPart` outside `ExecuteEdit` or a
`BeginEdit`/`CompleteEdit` pair.
