# Gwire

- .NET 10 Blazor WebAssembly app for guitar-wiring diagrams.
- Keep changes scoped; do not edit `bin/` or `obj/`.
- Keep changes within the requested scope. Include obvious follow-through needed for
  the change to work, such as carrying new properties through an existing clone.
  Ask before implementing optional related behavior or broader changes.
- Use component-scoped `.razor.css` for page-specific styles and
  `wwwroot/css/app.css` for global styles.
- Put Blazor component logic in a code-behind `.razor.cs` file. Keeping a few
  properties, a short method, or similarly small code directly in a `.razor`
  file is acceptable when a separate file would add unnecessary overhead.
- Preserve CRLF line endings.
- Verify changes with `dotnet build gwire.slnx --no-restore`.
- Keep verification proportionate to the change. Do not run unrelated, redundant,
  or low-value checks (for example, JSON linting when the requested configuration
  is already straightforward) or repeat a command that has not produced a result
  unless it is necessary to diagnose a concrete failure.
- using css Boostrap v5.3.3 and Font Awesome Free 7.3.1
- Use en.us for code, comments and UI
- Do not restore intentionally removed code or behavior unless the user explicitly requests it.
- Do not make assumptions that expand the requested change; do not alter related API contracts, generic constraints, or behavior unless explicitly requested.
- Prefer existing platform, framework, and project solutions before creating custom implementations. Create custom code only when a concrete requirement cannot be met by an existing solution.
- Preserve user-chosen names and renames of fields, properties, parameters, methods, and components. Do not rename them or revert user edits unless explicitly requested. Before editing, read the current files and make only the changes required for the task.
- Never use async void. Asynchronous methods must return Task or ValueTask and be awaited by their callers. Do not discard asynchronous work. Keep handlers synchronous when the work is synchronous; for Blazor notifications already running on the renderer context, call StateHasChanged directly.
- Avoid redundant fields, duplicated state, and extra abstractions when existing framework APIs can handle the requirement. For Blazor parameter changes, save the current parameter value in a local variable, await base.SetParametersAsync, then compare it with the updated parameter and adjust event subscriptions only if it changed. Do not store a second model reference in a field or read ParameterView manually when this lifecycle approach suffices.
- Do not build app by yourselft. If you want, please ask for confirmation before building the app.
