# Architecture

Gwire is a single-page Blazor WebAssembly app. All logic runs in the browser. The only
network traffic is reading the parts catalog (and chosen schemes) from GitHub raw files.

## Startup

`Gwire/Program.cs` registers the root components and these **scoped** services (in
WebAssembly, scoped means one instance for the whole app session):

| Service | Responsibility |
| --- | --- |
| `HttpClient` | Base address = app base URL. Used by the catalog service. |
| `GwirePartsService` | Downloads and holds catalog parts, tags, and scheme list; keeps parts the user saved locally. See [parts-catalog.md](parts-catalog.md). |
| `MessageService` | Raises toast messages (`Show(title, text, MessageType)`), displayed by `MessageToasts`. |
| `ConfirmationService` | `await ShowAsync(title, text)` returns `ConfirmationResult`; displayed by `ConfirmationDialog`. One dialog at a time (semaphore). |
| `UndoRedoService` | Independent undo/redo histories keyed by `HistoryIds`. See [undo-redo.md](undo-redo.md). |
| `EditorStateService` | Holds `SchemeEditorState` and `PartEditorState` so both editors keep their document, selection, zoom, etc. when the user navigates between pages. |
| `SchemePdfExporter` | Calls `gwire.exportSchemePdf` in JavaScript. |

When adding a service, register it in `Program.cs` the same way and inject it with
`[Inject]` in the component's code-behind.

## Routing and layout

- `App.razor` uses `MainLayout` as the default layout and `Pages/NotFound` for unknown routes.
- `/` (`Pages/Home.razor`) immediately redirects to `scheme-editor`.
- `MainLayout` renders:
  - a header with navigation buttons for the two editors and a
    `<SectionOutlet SectionName="editor-toolbar" />`. Each editor page fills it with
    `<SectionContent SectionName="editor-toolbar">`, so page-specific toolbar buttons
    live in the page's own `.razor` file.
  - the page body, only after the repository loading dialog has finished
    (`canUseApp`). The body gets a cascading `EventCallback` named `ReloadRepository`
    that pages use for the "Reload repository data" button.
  - `MessageToasts` and `ConfirmationDialog`, once, globally.
- Other named sections: `svg-context-menu` (filled by `SvgContextMenu`, rendered inside
  the scheme SVG) and `point-signal-popup` (filled by `CircuitSignalRenderer`).

## State that survives navigation

Pages are recreated on navigation, so a page must not keep document state in its own
fields. Instead it reads and writes through `EditorStateService.SchemeEditor` or
`EditorStateService.PartEditor`. `SchemeEditor` exposes them through small wrapper
properties, for example `private float zoom { get => State.Zoom; set => State.Zoom = value; }`.
Only transient UI flags (dialog visible, loading spinner) stay in page fields.

## Change notification

Models are plain classes. `BaseCircuitPart` exposes `event Action? Changed` and
`NotifyChanged()`. Renderers subscribe to their part's `Changed` event (and unsubscribe
when the parameter changes or on dispose) and call `StateHasChanged()`. Model methods
that move or connect things (`CircuitPart.MoveTo`, `ConnectionPoint.Connect/Disconnect`,
`CircuitPart.UpdateConnectedCablePoints`) raise the events for you. If you mutate a
model property directly and the canvas does not refresh, call `NotifyChanged()` on the
owning part.

Services raise their own events (`GwirePartsService.Changed`,
`UndoRedoHistoryWrapper.Changed`); pages subscribe in `OnInitialized` and unsubscribe in
`DisposeAsync`.

## JavaScript interop

Browser-only features (pointer capture, pan/zoom, keyboard shortcuts, context menu
positioning, file download, PDF export, Bootstrap modals) are in `wwwroot/js`. See
[ui-conventions.md](ui-conventions.md#javascript-interop).

## Offline and hosting

`wwwroot/service-worker.published.js` caches published assets. `index.html` skips service
worker registration on `localhost` and on the desktop `gwire:` protocol. The base path
`/` is rewritten to `/gwire/` by the GitHub Pages workflow; do not hard-code absolute
paths, use relative URLs.
