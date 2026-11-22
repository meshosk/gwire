# Scheme editor

Route `/scheme-editor`. Files: `Gwire/Pages/SchemeEditor.razor`, `.razor.cs`, `.razor.css`,
plus the renderers in `Gwire/Components/Renderers/`.

## What the page does

- Toolbar (rendered into the layout's `editor-toolbar` section): new scheme, open scheme
  from the catalog, import/export JSON, export PDF, reload catalog, pan mode toggle,
  undo/redo, zoom controls, details panel (name, description, tags), add cable, add part,
  measurement pair toggle, signal mode toggle, help and sponsor links.
- Canvas: one `<svg class="scheme-editor-canvas">` with a grid background. Parts and cables
  are drawn inside `<g data-pdf-content>`; only that group is exported to PDF.
- Right-click on the background opens `SvgContextMenu` (add part, add cable, measurement pair).
- The selected item is drawn last (`OrderBy(ReferenceEquals(part, SelectedPart))`) so it
  is on top.
- Before replacing the current scheme (new, import, open from catalog) the page calls
  `ConfirmDiscardChangesAsync()`, which asks only when there are history entries or
  changed details.

The document and view settings live in `EditorStateService.SchemeEditor`
(`SchemeEditorState`), not in page fields. See [architecture.md](architecture.md#state-that-survives-navigation).

## Renderers

| Component | Draws / handles |
| --- | --- |
| `CircuitPartRenderer` | One `CircuitPart`: SVG artwork (`SvgBackground`), custom value text, connection point circles, selection outline, drag to move, and its context menu (pot positions, coil signal color, state selection per state group, edit text, duplicate, delete). |
| `CablePartRenderer` | One `CablePart`: path, endpoint handles, dragging endpoints and snapping them to the nearest part point, adding bend points, color. Uses `CableBendPoints` for bend handles. |
| `CableBendPoints` | Drag, remove, and round/sharp toggle for cable bend points. |
| `CircuitTerminalRenderer` | Subclass of `CircuitPartRenderer` for the power source and ground terminals of a `SourceGroundPair`; dragging attaches them to points. |
| `CircuitPathRenderer` | Highlights every connection reachable from the power source terminal. |
| `CircuitSignalRenderer` | Signal mode: colored bass-to-treble ribbons per coil and the point signal popup (`PointSignalModal`). Geometry in `SignalPathGeometry`. |

Renderers receive the model object as an `[EditorRequired]` parameter, subscribe to its
`Changed` event in `SetParametersAsync` (see the pattern in
[ui-conventions.md](ui-conventions.md#component-patterns)), and report selection back
through a two-way bound `IsSelected` / `IsSelectedChanged` parameter.

## Pointer interaction pattern

Dragging follows the same sequence in every renderer:

1. `pointerdown` (primary button only): store the pointer id, start positions, and call
   `History.Freeze()`. `gwire.enablePointerCapture` (in `pointer-capture.js`) keeps
   pointer events on the element.
2. `pointermove`: convert the client delta to scheme units by dividing by `Zoom`, and
   move the model (`CircuitPart.MoveTo`, cable point `LocalPosition`, …).
3. `pointerup`: if something changed, push an undo action with `History.addRedoAction`
   (the change is already applied). Always `History.Unfreeze()`.
4. `pointercancel` or dispose during a drag: undo the in-progress change and unfreeze.

A movement under 2 px counts as a click (selection) rather than a drag.

## Zoom and pan

`wwwroot/js/scheme-pan.js` owns the viewport transform, wheel zoom, Space-to-pan and the
pan toggle button (`#scheme-pan-toggle`). It calls back into the page through
`[JSInvokable]` methods `HandleZoomChanged`, `HandleViewportChanged`; the page stores the
values in `SchemeEditorState` so they survive navigation. Zoom is clamped to 0.2–2.
Set zoom from C# with `gwire.setSchemeZoom`, not by assigning the field.

## Keyboard

`wwwroot/js/undo-redo.js`: Ctrl/Cmd+Z undo, Ctrl/Cmd+Shift+Z or Ctrl/Cmd+Y redo
(`HandleHistoryShortcut`), Delete/Backspace deletes the selected item
(`DeleteSelectedPart`). Shortcuts are ignored while typing in inputs.

## Adding a new scheme action

1. Add a toolbar button or context menu item with `disabled="@history.IsFrozen"`
   (or `Disabled="@History.IsFrozen"` on menu items).
2. In the handler, return early if `history.IsFrozen`.
3. Create an `IUndoableAction` in `Gwire/UndoRedo/Actions/`, call `action.Redo()` to apply
   it, then `history.addRedoAction(action)`. See [undo-redo.md](undo-redo.md).
