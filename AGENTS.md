# Gwire

- .NET 10 Blazor WebAssembly app for guitar-wiring diagrams.
- Keep changes scoped; do not edit `bin/` or `obj/`.
- The user explicitly forbids expanding requests on your own initiative. Implement only what the user explicitly requested. If any additional change is needed, explain why and ask the user before making it, even if it seems necessary, obvious, related, or beneficial. Do not treat a request to remove or rename a member as authorization to remove or change the behavior that uses it.
- Questions and requests for explanations do not authorize file edits.
- Before modifying files, describe the exact changes and list the files involved, then wait for the user's explicit approval. After approval, execute exactly the approved changes. If the scope needs to change, explain the revised scope and wait for approval again.
- Removing or renaming a symbol does not authorize removing its callers or changing their behavior. If the intended replacement is unclear, ask and wait for the user's answer.
- If additional changes appear necessary, explain them and obtain explicit approval before making them.
- Before editing, record the current diff for the affected files. When reverting your work, reverse only your own changes against that recorded state. If the target state is ambiguous, ask.
- Before finishing, compare the resulting diff with the user's request. Every changed behavior must have explicit authorization.
- Do not show a proposed diff unless the user explicitly requests it. Use the description of planned edits for approval; keep diff recording and verification internal.
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
- Use a boolean `Disabled` parameter for UI availability based on state, such as `Disabled="@History.IsFrozen"`. Do not use `CanExecute="_ => !History.IsFrozen"` or equivalent lambdas that ignore their argument; reserve predicates for checks that depend on the argument.
- Never use async void. Asynchronous methods must return Task or ValueTask and be awaited by their callers. Do not discard asynchronous work. Keep handlers synchronous when the work is synchronous; for Blazor notifications already running on the renderer context, call StateHasChanged directly.
- Prefer declarative component parameters and two-way binding (`@bind-...` with a matching `EventCallback`) so Blazor handles state propagation and rendering. Use `@ref` only when necessary, such as DOM access for JavaScript interop; do not use component refs to call methods when binding can express the interaction.
- Avoid redundant fields, duplicated state, and extra abstractions when existing framework APIs can handle the requirement. For Blazor parameter changes, save the current parameter value in a local variable, await base.SetParametersAsync, then compare it with the updated parameter and adjust event subscriptions only if it changed. Do not store a second model reference in a field or read ParameterView manually when this lifecycle approach suffices.
- Do not build app by yourselft. If you want, please ask for confirmation before building the app.
- Define short callback logic used only once inline where the consuming object is created, using a lambda. Do not extract it into a separate static or helper method (for example, search relevance logic passed to SearchOptions).
- Order class members as follows: fields and properties first, constructors next, and methods last. Apply this order when creating a class and reorder members accordingly whenever editing an existing class.
