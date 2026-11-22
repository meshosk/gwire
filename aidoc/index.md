# Gwire AI documentation index

Start here before changing code. Each topic below has a short summary and a link to a
focused document. Read the documents for the area you are about to change; you do not
need to read all of them.

Working rules for agents (scope, approval, building, style) are in
[AGENTS.md](../AGENTS.md). They take precedence over anything in these documents.

| Topic | Read it when you… | Document |
| --- | --- | --- |
| Overview and repository map | need to know what Gwire is, which projects exist and where code lives | [overview.md](overview.md) |
| Architecture | touch startup, DI services, the layout, routing, or state that survives navigation | [architecture.md](architecture.md) |
| Domain model | change parts, points, cables, states, coils/resistors/capacitors, potentiometers, or the circuit graph | [domain-model.md](domain-model.md) |
| Scheme editor | work on the `/scheme-editor` page, the SVG canvas, renderers, dragging, zoom/pan, or the context menu | [scheme-editor.md](scheme-editor.md) |
| Part editor | work on the `/part-editor` page, point editing, states, connection pairs, or potentiometers | [part-editor.md](part-editor.md) |
| Undo/redo | add or change any user edit (every edit must be undoable) | [undo-redo.md](undo-redo.md) |
| Path and signal tracing | change the source-ground measurement path or the coil signal mode | [signal-tracing.md](signal-tracing.md) |
| JSON import/export | change serialized models or the scheme/part file formats | [serialization.md](serialization.md) |
| Parts catalog | change loading of parts, tags, and schemes from the remote catalog repository | [parts-catalog.md](parts-catalog.md) |
| UI and Blazor conventions | write components, styles, dialogs, messages, or JavaScript interop | [ui-conventions.md](ui-conventions.md) |
| Build, run, and deploy | need to build, debug, publish to GitHub Pages, or package the desktop app | [build-and-deploy.md](build-and-deploy.md) |

## Quick facts

- .NET 10 Blazor WebAssembly client only (`Gwire/`). There is no backend API; the
  `Server/` project is just a local static host for debugging.
- UI: Bootstrap 5.3.3 and Font Awesome Free 7.3.1, both vendored in `Gwire/wwwroot/lib`.
- Diagrams are rendered as SVG by Razor components; a few browser features use small
  JavaScript files in `Gwire/wwwroot/js` under the `window.gwire` namespace.
- Parts, tags, and example schemes are downloaded at startup from the
  [gwire-parts-catalog](https://github.com/meshosk/gwire-parts-catalog) repository.
- There is no automated test project.
- Source files use CRLF line endings; many `.razor` and `.md` files start with a UTF-8 BOM.
  Keep the existing encoding of each file you edit.
- Code, comments, and UI text are in US English.

## Keeping these documents accurate

When a change makes a statement here wrong (a renamed class, a new service, a changed
file format), update the matching document in the same change, if the user approves the
documentation edit. Describe concepts and point to files; do not copy large code blocks,
because they go stale.
