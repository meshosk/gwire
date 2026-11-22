# Overview and repository map

Gwire is a browser app for drawing guitar wiring diagrams ("schemes"). A user places
parts (pickups, switches, pots, jacks, capacitors…) on an SVG canvas, connects them with
cables, flips switch states, and sees which coils are active, the current path between
a source and ground, and an illustrative bass/mids/treble signal level. Schemes and parts
can be imported/exported as JSON and schemes can be exported to PDF.

Live app: https://meshosk.github.io/gwire/ (deployed from `main`).

## Solution

`gwire.slnx` contains two projects:

| Project | What it is |
| --- | --- |
| `Gwire/Gwire.csproj` | The app. Blazor WebAssembly (`Microsoft.NET.Sdk.BlazorWebAssembly`), `net10.0`, nullable and implicit usings enabled, service worker for offline use. |
| `Server/Server.csproj` | A tiny ASP.NET Core host that serves the client files for Visual Studio debugging. It has no API. Do not add server logic here; the deployed app is static. |

`Desktop/` is an Electron wrapper (not part of the solution) that bundles the published
client. See [build-and-deploy.md](build-and-deploy.md).

## Folder map of `Gwire/`

| Folder | Contents |
| --- | --- |
| `Program.cs`, `App.razor`, `_Imports.razor` | Startup, DI registration, router. |
| `Layout/` | `MainLayout`: top toolbar, repository loading dialog, global toasts and confirmation modal. |
| `Pages/` | Routable pages: `SchemeEditor` (`/scheme-editor`), `PartEdit/PartEditor` (`/part-editor`) and its sub-editors, `Home` (redirects to the scheme editor), `Info`, `NotFound`. |
| `Components/Renderers/` | SVG renderers used on the scheme canvas: parts, cables, bend points, source/ground terminals, path and signal overlays. |
| `Components/Displayers/` | Dialogs and overlays: confirmation modal, toasts, search dialog, part text modal, point signal popup, repository loading dialog. |
| `Components/SvgMenu/` | Right-click context menu rendered inside the SVG. |
| `Components/Tabs/` | `BootstrapTabs` / `BootstrapTab` wrapper used by the part editor. |
| `Components/` (root) | `PartSvgBackground`, `SvgBackground` (part artwork). |
| `Models/` | Domain model and editor state. `Models/Base/` holds the abstract part, points, states, and connection pairs. See [domain-model.md](domain-model.md). |
| `Services/` | Injectable services (catalog, messages, confirmation, undo/redo, editor state, PDF export) and the static signal-calculation classes. |
| `UndoRedo/` | `IUndoableAction`, history ids, and one class per undoable scheme action. |
| `Serialization/` | JSON import/export for parts and schemes. |
| `Extensions/` | Small LINQ helpers. |
| `wwwroot/` | `index.html`, global `css/app.css`, JavaScript interop in `js/`, vendored libraries in `lib/` (Bootstrap, Font Awesome, jsPDF), icons, manifest, service worker. |

Never edit `bin/`, `obj/`, `Desktop/dist/`, or vendored files in `wwwroot/lib/`.

## Other folders at the repository root

| Folder | Contents |
| --- | --- |
| `.github/workflows/` | `deploy-pages.yml` (GitHub Pages from `main`), `build-desktop.yml` (desktop packages on `v*.*.*` tags). |
| `.vscode/` | Launch and task definitions for Hot Reload and WASM debugging. |
| `aidoc/` | This documentation. |
| `logo/` | Logo source files. |

## Branches

`main` is the deployed branch. Day-to-day work happens on `dev`.
