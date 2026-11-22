# Build, run, and deploy

Full human instructions are in [README.md](../README.md). This is the short version for
agents.

## Before you build

AGENTS.md asks agents not to build the app on their own: ask the user first. When the
user agrees, the verification command is:

```sh
dotnet build gwire.slnx --no-restore
```

(VS Code task **Gwire: build** runs the same command.) It needs the .NET 10 SDK and a
prior restore. There is no test project, so the build is the only automated check.

## Run locally

- `dotnet watch --project Gwire/Gwire.csproj run --launch-profile http`
  (VS Code task **Gwire: watch**), then open http://localhost:5188.
- VS Code launch configuration **Gwire: Hot Reload + WASM Debug** starts the watch task
  and attaches the Blazor WebAssembly debugger in a dedicated Chrome profile.
- The `Server` project is only needed for Visual Studio debugging; the dev server in the
  `Gwire` project is enough otherwise.
- The service worker is not registered on localhost, so caching does not interfere with
  development.

## GitHub Pages

`.github/workflows/deploy-pages.yml` runs on every push to `main` (and manually). It runs
`dotnet publish Gwire/Gwire.csproj -c Release`, rewrites `<base href="/">` and the
service worker base to `/<repo-name>/`, and deploys `publish/wwwroot`. Keep `index.html`'s
`<base href="/" />` line and the `const base = "/";` line in
`service-worker.published.js` byte-identical, because the workflow edits them with `sed`
(the published file is renamed to `service-worker.js`).

## Desktop (Electron)

- `Desktop/main.cjs` serves the published client from the app resources over a custom
  `gwire://app/` protocol; no HTTP server or .NET runtime on the user's machine.
- `Desktop/Dockerfile` does everything in Docker: publishes the client, renders icons
  from `Gwire/wwwroot/gwire-webicon.svg`, installs Electron, and packages per platform
  (`--target windows` or `macos`, `APP_ARCH` = `x64` or `arm64`).
- `.github/workflows/build-desktop.yml` runs on tags `vMAJOR.MINOR.PATCH`, builds Windows
  portable executables and macOS apps (ad-hoc signed on a macOS runner), and creates a
  GitHub Release. The version comes from the tag (local builds use `Desktop/package.json`).
- Output goes to `Desktop/dist/` (ignored by Git). Do not edit or read files there as source.
