# Gwire

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
- [ ] Circuit parts editor
- [ ] Circuit parts import/export
- [ ] Circuit parts library
- [ ] Circuit parts undo/redo
- [ ] App icon
- [ ] App design
- [ ] UX improvements
- [ ] Wiring diagram editor
- [ ] Wiring diagram editor import/export
- [ ] Wiring diagram editor undo/redo
- [ ] Wiring diagram snapshots for experimentation
- [ ] Custom wire colours
- [ ] Wiring diagram library
- [ ] Wiring diagram PDF export
- [ ] Wiring diagram circuit routing from coils
- [ ] Make it a PWA
- [ ] Create an app wrapper and release it on GitHub for Electron or a similar platform
- [ ] Add parts and wiring diagrams to a library, possibly in a separate repository
- [ ] Add a manual, wiki, or how-to guide

## VS Code development

Open the repository root in VS Code. Install the .NET 10 SDK, Google Chrome, and the Microsoft C# Dev Kit extension (which also installs the C# extension).

1. Select **Gwire: Hot Reload + WASM Debug** in **Run and Debug** and press **F5**.
2. VS Code starts `dotnet watch` for `Gwire/Gwire.csproj` using the existing `http` launch profile, waits for `http://localhost:5188`, and opens Chrome with the Blazor WebAssembly C# debugger attached.
3. Set breakpoints in the client C# code and save changes to apply supported Hot Reload edits. The watch terminal prompts for a restart when a change cannot be applied. After a restart, reload the browser or restart the debug session if needed.
4. Stop debugging with **Shift+F5**. The watch task keeps running so another debug session can reuse it. To stop the app as well, focus the watch terminal and press **Ctrl+C**, or run **Tasks: Terminate Task** and select **Gwire: watch**.

To use Hot Reload without debugging, run **Tasks: Run Task** > **Gwire: watch** and open `http://localhost:5188` manually. No additional scripts are required; the standalone client development server provides the debug proxy, so the `Server` project is not needed for this workflow.

Chrome uses a dedicated debug profile at `.vscode/chrome-local-debug-profile`, which is already ignored by Git, and VS Code closes the whole debug browser when the session ends. This avoids reusing the JavaScript debugger's shared profile when it has a stale lock from an earlier session. The ordinary Chrome profile is unaffected.

The launch configuration uses the C# extension's `blazorwasm` debugger for .NET 10 managed C# breakpoints. Chrome uses `killBehavior: polite` so shutdown can release its profile lock. A diagnostic trace is written to `.vscode/wasm-debug.log` while verifying repeated starts. Service worker registration is skipped on `localhost`, `127.0.0.1`, and `[::1]` to avoid a service worker debug session ending and stopping the entire browser through the installed C# extension (see [vscode-csharp issue #9750](https://github.com/dotnet/vscode-csharp/issues/9750) for a related bug). Service worker registration on deployed hosts is unchanged. The dedicated debug profile starts without any previously registered service worker. Repeated starts and breakpoint behavior still require live verification.

## GitHub Pages

The live app is available at [https://meshosk.github.io/gwire/](https://meshosk.github.io/gwire/).

__It may be non-functional or only partially functional.__
