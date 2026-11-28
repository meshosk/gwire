# UI and Blazor conventions

These summarize patterns already used in the code. The binding rules are in
[AGENTS.md](../AGENTS.md); where this file and AGENTS.md overlap, AGENTS.md wins.

## Files per component

- `Name.razor`: markup. Keep only trivial code here (a property, a one-line method).
- `Name.razor.cs`: `public partial class Name` with the logic. Inject services with
  `[Inject] private T Service { get; set; } = default!;`.
- `Name.razor.css`: component-scoped styles. Global styles go to `wwwroot/css/app.css`.
- `Name.razor.js`: optional collocated JavaScript module, loaded with
  `JS.InvokeAsync<IJSObjectReference>("import", "./Components/.../Name.razor.js")`.
- Class member order: fields and properties, then constructors, then methods.

New `.razor` folders under `Components/` may need an `@using` in `_Imports.razor`
(existing ones: `Components`, `Dialogs`, `Displayers`, `Renderers`, `SvgMenu`, `Layout`).

## Styling and icons

- Bootstrap 5.3.3 utility and component classes (`btn btn-sm btn-outline-primary`,
  `d-flex gap-2`, `card shadow-sm`, `list-group`, `modal`). Prefer them over custom CSS.
- Font Awesome Free 7.3.1 icons: `<i class="fa-solid fa-…" aria-hidden="true"></i>`.
- Icon-only buttons carry both `title` and `aria-label`. Toggle buttons set `aria-pressed`.
- Both libraries are vendored in `wwwroot/lib`; do not add CDN links or npm packages.

## Component patterns

- **Disable instead of predicates.** UI availability is a boolean, for example
  `disabled="@history.IsFrozen"` on HTML or `Disabled="@History.IsFrozen"` on components.
- **Two-way binding** between parent and child: a `[Parameter] T Value` plus
  `[Parameter] EventCallback<T> ValueChanged`, used as `@bind-Value`. Examples:
  `IsSelected` on renderers, `SelectedPoint` on part sub-editors, `Visible` on
  `SearchDialog`, `Part` on `PartTextDialog`. Use `@ref` only for DOM/JS interop.
- **Subscribing to a model parameter's events.** Override `SetParametersAsync`, save the old
  value, `await base.SetParametersAsync(parameters)`, then move the subscription only if
  the reference changed. Unsubscribe in `Dispose`. See `CircuitPartRenderer.SetParametersAsync`.
- **Async.** Never `async void`. Return `Task`/`ValueTask` and await. Keep synchronous
  handlers synchronous; from Blazor events call `StateHasChanged()` directly.
- **Dispose.** Components holding JS objects, `DotNetObjectReference`s, or event
  subscriptions implement `IDisposable` or `IAsyncDisposable` and release them.
- **Short one-off callbacks** are inline lambdas where the object is created (for example
  the relevance function passed to `SearchOptions<T>`).

## Dialogs, messages, and search

- Toasts: `MessageService.Show(title, text, MessageType.Success | Danger | …)`.
- Confirmation: `await ConfirmationService.ShowAsync(title, text) == ConfirmationResult.YES`.
- Search/pick from a list: create a `SearchOptions<T>(relevanceFunc, textFunc)` once in
  `OnInitialized` with `Title`, `Items`, `AvailableTags`; render
  `<SearchDialog Options="…" @bind-Visible="…" />`; set the visible flag to true and
  `await options.Result` (null on cancel).
- SVG context menu: `<SvgContextMenu Target="#css-selector">` with
  `SvgContextMenuItem` (`OnClick` receives the scheme position) and `SvgContextSubmenu`.
  The menu renders through the `svg-context-menu` section inside the canvas SVG.
- Bootstrap modals are opened and closed through small JS modules
  (`confirmation-modal.js`, `part-text-modal.js`).

## JavaScript interop

Global scripts in `wwwroot/js/` are loaded by `<script>` tags in `wwwroot/index.html` and
attach functions to `window.gwire`:

| File | Functions | Used for |
| --- | --- | --- |
| `pointer-capture.js` | `enablePointerCapture`, `getSvgPoint`, `focusSchemeEditor` | Keeps pointer events on a dragged SVG element; client-to-SVG coordinates. |
| `scheme-pan.js` | `enableSchemePan`, `disableSchemePan`, `setSchemeZoom` | Viewport, wheel zoom, pan mode, Space key. |
| `undo-redo.js` | `enableUndoRedoShortcuts`, `disableUndoRedoShortcuts` | Ctrl/Cmd+Z/Y, Delete key. |
| `svg-context-menu.js` | `attachSvgContextMenu`, `showSvgContextMenu`, `showSvgContextSubmenu` | Context menu opening and placement. |
| `svg-background.js` | `fitSvgBackground` | Fitting part artwork. |
| `file-download.js` | `downloadFileFromStream` | Saving JSON exports (`DotNetStreamReference`). |
| `scheme-pdf-export.js` | `exportSchemePdf`, `showPdfExportProgress`, `hidePdfExportProgress` | PDF export with jsPDF (`lib/jspdf`), progress modal. |
| `confirmation-modal.js`, `part-text-modal.js` | ES module exports (`show`, `hide`, …) | Imported as modules, not global. |

Rules:

- Add new global functions under `window.gwire` (`window.gwire = window.gwire || {};`) and
  add the `<script>` tag to `index.html` before the Blazor script. Prefer an ES module
  (collocated `.razor.js` or `js/*.js` imported with `"import"`) for code used by one component.
- Callbacks from JS into .NET use `DotNetObjectReference.Create(this)` and
  `[JSInvokable]` public methods; dispose the reference in `DisposeAsync`.
- Use the browser only for what .NET cannot do (DOM measurements, pointer capture,
  downloads, PDF rendering). Keep model logic in C#.

## Language and text

US English for identifiers, comments, and UI strings. Keep CRLF line endings and the
existing BOM/no-BOM state of each file.
