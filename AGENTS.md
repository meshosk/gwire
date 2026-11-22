# Gwire

.NET 10 Blazor WebAssembly app for guitar wiring diagrams. Before changing code, read
[aidoc/index.md](aidoc/index.md) for architecture, domain model, and conventions.

## Scope and approval

- Do only what the user explicitly asked for. If anything else seems needed (even if it looks obvious, related, or beneficial), explain why and wait for approval. If the scope changes, describe the new scope and wait again.
- Questions and requests for explanations do not authorize file edits.
- A direct request for a specific change is approval; implement it without asking again. Otherwise, describe the exact changes and files first and wait. Do not show a diff unless asked.
- Removing or renaming a symbol does not authorize changing its callers or their behavior. Do not change related API contracts, generic constraints, or behavior. If the replacement is unclear, ask.
- Do not restore intentionally removed code, and do not rename or revert the user's names and edits.
- Before editing, read the current files and note their state; when reverting, undo only your own changes. Before finishing, check that every changed behavior was authorized.

## Building

- Do not build on your own; ask first. When approved, verify with `dotnet build gwire.slnx --no-restore`.
- Keep checks proportionate; do not run redundant checks or repeat a command without a concrete reason.

## Code

- Do not edit `bin/` or `obj/`. Preserve CRLF line endings. Use en-US for code, comments, and UI.
- Prefer existing framework and project solutions over custom code; avoid redundant fields, duplicated state, and extra abstractions.
- Class member order: fields and properties, constructors, methods.
- Inline short single-use callbacks as lambdas where the consuming object is created.
- Never use `async void`; return and await `Task`/`ValueTask`. Keep synchronous work synchronous; call `StateHasChanged` directly when already on the renderer context.

## Blazor and UI

- Logic goes in `.razor.cs` code-behind (tiny members may stay in `.razor`). Page styles in `.razor.css`, global styles in `wwwroot/css/app.css`.
- Bootstrap 5.3.3 and Font Awesome Free 7.3.1.
- Express availability with a boolean `Disabled` parameter (`Disabled="@History.IsFrozen"`), not predicates that ignore their argument.
- Prefer parameters and two-way binding (`@bind-X` + `XChanged`) over `@ref`; use `@ref` only for DOM/JS interop.
- When reacting to a changed parameter: save the old value, `await base.SetParametersAsync`, compare, and resubscribe only if it changed. Do not keep a second model reference or read `ParameterView` manually.
