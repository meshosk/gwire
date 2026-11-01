<h1><img src="logo/gwire-logo-bg.svg" alt="Gwire logo" width="64" /> Gwire</h1>

As a guitar player who can modify and set up my instruments reasonably well, I have often been frustrated by guitar wiring diagrams. I simply want to find suitable diagrams and use them. However, there are several common problems:

- The diagram requires parts that I do not have.
- I cannot find a diagram for the configuration I want.
- The diagram is hidden behind a paywall or in a forum.
- I have to learn other software, which is more complex from an electrical perspective.
- I need to understand traditional guitar wiring diagrams.

For this reason, I created this app. A few things to keep in mind first:

- I develop this app in my free time, so development is slower, even with AI assistance.
- This is a basic app focused on guitar parts and wiring diagrams.
- Circuit routing tests are basic and intended only to preview circuit states as switches change the routing.

__IN DEVELOPMENT__

This web app will make it possible to create custom guitar wiring diagrams, show active parts in a circuit and coil polarity, and export diagrams to PDF.

__Migrated to a pure .NET 10 Blazor client app__

___TODO___:

- [x] Move from JavaScript to .NET
- [x] Circuit parts editor
- [x] Circuit parts import/export
- [x] Circuit parts library
- [x] Circuit parts undo/redo
- [x] App icon
- [ ] App design
- [ ] UX improvements
- [x] Wiring diagram editor
- [x] Wiring diagram editor import/export
- [x] Wiring diagram editor undo/redo
- [ ] Wiring diagram snapshots for experimentation
- [x] Custom wire colours
- [x] Wiring diagram library
- [x] Wiring diagram PDF export
- [x] Wiring diagram circuit routing from coils
- [ ] Make it a PWA
- [ ] Create an app wrapper and release it on GitHub for Electron or a similar platform
- [x] Add parts and wiring diagrams to a library, possibly in a separate repository
- [ ] Add a manual, wiki, or how-to guide

## Data source

Gwire loads its parts, tags, and wiring diagram catalog from the [gwire-parts-catalog repository](https://github.com/meshosk/gwire-parts-catalog).

## VS Code development

Open the repository root in VS Code. Install the .NET 10 SDK, Google Chrome, and the Microsoft C# Dev Kit extension (which also installs the C# extension).

1. Select **Gwire: Hot Reload + WASM Debug** in **Run and Debug** and press **F5**.
2. VS Code starts `dotnet watch` for `Gwire/Gwire.csproj` using the existing `http` launch profile, waits for `http://localhost:5188`, and opens Chrome with the Blazor WebAssembly C# debugger attached.
3. Set breakpoints in the client C# code and save changes to apply supported Hot Reload edits. The watch terminal prompts for a restart when a change cannot be applied. After a restart, reload the browser or restart the debug session if needed.
4. Stop debugging with **Shift+F5**. The watch task keeps running so another debug session can reuse it. To stop the app as well, focus the watch terminal and press **Ctrl+C**, or run **Tasks: Terminate Task** and select **Gwire: watch**.

If a debug session leaves processes or a locked browser profile behind on macOS, stop debugging with **Shift+F5**, then run **Tasks: Run Task** > **Gwire: stop debug sessions (macOS)**. The task stops Chrome using the Gwire debug profile and Blazor `BrowserDebugHost` processes owned by the current VS Code instance, then removes the debug profile lock files after confirming that the browser has exited. This can stop other Blazor debug hosts in the same VS Code instance. The regular Chrome profile and `dotnet watch` are unaffected. No separate cleanup script is required.

To use Hot Reload without debugging, run **Tasks: Run Task** > **Gwire: watch** and open `http://localhost:5188` manually. No additional scripts are required; the standalone client development server provides the debug proxy, so the `Server` project is not needed for this workflow.

Chrome uses a dedicated debug profile at `.vscode/chrome-local-debug-profile`, which is already ignored by Git, and VS Code closes the whole debug browser when the session ends. This avoids reusing the JavaScript debugger's shared profile when it has a stale lock from an earlier session. The ordinary Chrome profile is unaffected.

The launch configuration uses the C# extension's `blazorwasm` debugger for .NET 10 managed C# breakpoints. Chrome uses `killBehavior: polite` so shutdown can release its profile lock. A diagnostic trace is written to `.vscode/wasm-debug.log` while verifying repeated starts. Service worker registration is skipped on `localhost`, `127.0.0.1`, and `[::1]` to avoid a service worker debug session ending and stopping the entire browser through the installed C# extension (see [vscode-csharp issue #9750](https://github.com/dotnet/vscode-csharp/issues/9750) for a related bug). Service worker registration on deployed hosts is unchanged. The dedicated debug profile starts without any previously registered service worker. Repeated starts and breakpoint behavior still require live verification.

## GitHub Pages

The live app is available at [https://meshosk.github.io/gwire/](https://meshosk.github.io/gwire/).

__It may be non-functional or only partially functional.__

## Desktop application

The Electron wrapper in `Desktop/` bundles the published Blazor WebAssembly client.
It loads local assets through `gwire://app/`, without an HTTP server or a .NET runtime
installation on the user's computer. The parts and wiring diagram catalog still
requires access to GitHub. The desktop client changes only when the desktop package
is rebuilt; it does not load the hosted web application.

Install Docker Desktop to build desktop packages. The .NET SDK, Node.js,
npm dependencies, client publishing, and Electron packaging run inside Docker.
On macOS, run **Tasks: Run Task** > **Gwire: build desktop (macOS)** in VS Code.
The task starts Docker Desktop if needed, waits for it to become ready, and runs
the build inside Docker. It exports only a `Gwire.app` for the current Mac's
architecture. It then applies and verifies a local ad-hoc signature using
macOS `codesign`, which cannot run in the Linux container. No distribution
certificate is needed for this local signature. The task does not launch the app.

Open `Desktop/dist/mac/Gwire.app` on Intel or
`Desktop/dist/mac-arm64/Gwire.app` on Apple Silicon. No DMG or ZIP is created locally.

To build a portable Windows executable, run from the repository root. Set
`APP_ARCH` to `x64` or `arm64` (the default is `x64`):

```sh
docker build --platform linux/amd64 --file Desktop/Dockerfile --target windows --build-arg APP_ARCH=x64 --output type=local,dest=Desktop/dist/windows-x64 .
```

Build dependencies and intermediate files stay in Docker. Desktop output is stored
in `Desktop/dist/`, which is ignored by Git. Local builds use the desktop package version from `Desktop/package.json`.
Release builds use the version from the pushed Git tag.

The **Build desktop** GitHub Actions workflow runs when a version tag such as
`v1.0.0` or `v1.0.1` is pushed to GitHub. The tag must use `vMAJOR.MINOR.PATCH` with
numeric components and no prerelease suffix. The tag version is embedded in both
desktop packages. After every build and macOS signing step succeeds, the workflow
creates a GitHub Release for that tag and attaches:

- `Gwire-1.0.0-win-x64.exe`: portable Windows app that runs without installation.
- `Gwire-1.0.0-win-x64.zip`: the complete unpacked Windows app, including `Gwire.exe`,
  Electron, and the bundled Blazor client. Extract the whole folder before running it.
- `Gwire-1.0.0-win-arm64.exe`: portable Windows ARM64 app without installation.
- `Gwire-1.0.0-win-arm64.zip`: the complete unpacked Windows ARM64 app.
- `Gwire-1.0.0-mac-arm64.zip`: complete `Gwire.app` for Apple Silicon.
- `Gwire-1.0.0-mac-x64.zip`: complete `Gwire.app` for Intel Macs.

Both native app icons are generated from `Gwire/wwwroot/gwire-webicon.svg` during
the Docker build. macOS ZIPs preserve the app's permissions and symbolic links.
No DMG is created. The local macOS task still produces only the `.app`.

For example, after committing the desired changes:

```sh
git tag v1.0.0
git push origin v1.0.0
```

Manual workflow runs produce downloadable artifacts without creating a release.
Builds run in Docker on Linux runners; a macOS runner applies and verifies the
ad-hoc signature before archiving the app. Distribution certificates and macOS
notarization are not configured. Automatic updates and native file associations
are not configured.
