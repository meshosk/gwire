# Part editor

Route `/part-editor`. Files in `Gwire/Pages/PartEdit/`:

| File | Role |
| --- | --- |
| `PartEditor.razor` / `.razor.cs` / `.razor.css` | Page layout, toolbar, canvas, file actions, point and value-text dragging. |
| `PartEditor.History.cs` | Partial class with the snapshot-based undo logic (`BeginEdit`, `CompleteEdit`, `CancelEdit`, `ExecuteEdit`). |
| `PartSnapshot.cs` | `PartEditor.PartSnapshot` record: cloned part plus selected indices. |
| `PartConnectionPairsEditor` | Add/edit coils, resistors, capacitors between two points. |
| `PartPotentiometersEditor` | Add/edit potentiometers (A/W/B points, resistance). |
| `PartStateGroupComponent`, `PartStateComponent`, `PartPointGroupComponent` | Edit state groups, their states, and the connection groups (shorted points) of a state. |

The edited part and all editor selections live in `EditorStateService.PartEditor`
(`PartEditorState`), so the work survives navigation to the scheme editor and back.

## Layout

Two columns:

- **Sidebar** (`BootstrapTabs`) with three tabs:
  - **Part**: name, description, tags (only tags that exist in the catalog), width/height,
    SVG background import with position and rotation, optional custom value text.
  - **Points**: add connection points, connection pairs and potentiometers, select a point
    and edit its label and X/Y.
  - **States** (disabled until the part has points): state groups, states, and connection
    groups. While a connection group is active, clicking a point on the canvas toggles
    its membership.
- **Canvas**: SVG preview of the part with its artwork (`PartSvgBackground`), draggable
  points, and the draggable value text. Connection groups are drawn with colors from
  `ConnectionGroup.GetLineColor(index)`.

## File and catalog actions

- New part, open part from the catalog (`SearchDialog`), import/export `*.part.json`
  (`ObjectJsonSerializer`), import an SVG file as artwork.
- **Save locally** calls `GwirePartsService.SavePart`, which stores a clone in memory
  for this session only (there is no persistent storage). A part that came from the
  catalog (`IsFromRepo`) is saved as a copy with a new `Id`.
- Save and export run `ValidatePotentiometers()` first.
- Replacing the edited part asks for confirmation when the history has entries.

## Making an edit undoable

The part editor does **not** use one action class per change. Wrap every model change in
`ExecuteEdit(() => …)`, or for a drag call `BeginEdit()` on pointer down and
`CompleteEdit()` on pointer up (`CancelEdit()` on cancel). `CompleteEdit` serializes the
before/after clones and adds a `PartSnapshotAction` only if something really changed.
Sub-editor components receive `ExecuteEdit` as a parameter and must route their changes
through it. Details in [undo-redo.md](undo-redo.md#part-editor-snapshots).
