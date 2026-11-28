# JavaScript interop

Global scripts in `wwwroot/js/` are loaded by `<script>` tags in `wwwroot/index.html` and expose
functions on `window.gwire`; inside, the logic is organized in classes (for example
`UndoRedoShortcuts`, `SchemePan`, `SvgContextMenu`).

| File | Functions | Used for |
| --- | --- | --- |
| `pointer-capture.js` | `enablePointerCapture`, `getSvgPoint`, `focusSchemeEditor` | Pointer capture while dragging; client-to-SVG coordinates. |
| `scheme-pan.js` | `enableSchemePan`, `disableSchemePan`, `setSchemeZoom` | Viewport, wheel zoom, pan mode, Space key. |
| `undo-redo.js` | `enableUndoRedoShortcuts`, `disableUndoRedoShortcuts` | Ctrl/Cmd+Z/Y; Delete calls `DeleteSelectedPart` (scheme) or `DeleteSelectedPoint` (part editor). |
| `svg-context-menu.js` | `attachSvgContextMenu`, `showSvgContextMenu`, `showSvgContextSubmenu` | Context menu opening and placement. |
| `svg-background.js` | `fitSvgBackground` | Fitting part artwork. |
| `file-download.js` | `downloadFileFromStream` | Saving exports (`DotNetStreamReference`). |
| `scheme-pdf-export.js` | `exportSchemePdf`, `showPdfExportProgress`, `hidePdfExportProgress` | PDF export with jsPDF, progress modal. |
| `confirmation-modal.js`, `part-text-modal.js` | ES module exports | Imported as modules, not global. |

Collocated modules: `SearchDialog.razor.js`, `RepositoryLoadingDialog.razor.js`.

## Rules

- New global functions go under `window.gwire` (`window.gwire = window.gwire || {};`) with a
  `<script>` tag in `index.html` before the Blazor script. Code used by one component is an ES
  module imported with `JS.InvokeAsync<IJSObjectReference>("import", "./…/Name.razor.js")`.
- JS calls .NET through `DotNetObjectReference.Create(this)` and public `[JSInvokable]` methods;
  dispose the reference in `DisposeAsync`.
- Use JS only for what .NET cannot do (DOM measurement, pointer capture, downloads, PDF). Model logic stays in C#.
