# UI and Blazor conventions

Patterns already used in the code. Binding rules (member order, async, line endings, language)
are in [AGENTS.md](../AGENTS.md) and win where they overlap. Related:
[js-interop.md](js-interop.md), [vendored-libraries.md](vendored-libraries.md).

## Files per component

- `Name.razor` markup (only trivial code), `Name.razor.cs` logic (`public partial class`,
  `[Inject] private T Service { get; set; } = default!;`), `Name.razor.css` scoped styles
  (global ones in `wwwroot/css/app.css`), optional `Name.razor.js` module.
- New `.razor` folders under `Components/` may need an `@using` in `_Imports.razor`.

## Styling and icons

- Prefer Bootstrap classes (`btn btn-sm btn-outline-primary`, `d-flex gap-2`, `card`) over custom CSS.
- Icons: Font Awesome `<i class="fa-solid fa-…" aria-hidden="true"></i>`; Material Design Icons
  `<i class="mdi mdi-…">` only for electronic symbols Font Awesome lacks.
- Icon-only buttons have `title` and `aria-label`; toggle buttons set `aria-pressed`.

## Component patterns

- **Availability** is a boolean: `disabled="@history.IsFrozen"` / `Disabled="@History.IsFrozen"`.
- **Two-way binding**: `[Parameter] T Value` + `EventCallback<T> ValueChanged`, used as
  `@bind-Value` (`IsSelected` on renderers, `SelectedPoint` on part sub-editors). `@ref` only for DOM/JS.
- **Model events from a parameter**: in `SetParametersAsync` save the old value, await the base,
  resubscribe only if the reference changed, unsubscribe in `Dispose`
  (see `CircuitPartRenderer.SetParametersAsync`).
- **Dispose** JS objects, `DotNetObjectReference`s, and event subscriptions.

## Dialogs, messages, and search

- Toast: `MessageService.Show(title, text, MessageType.…)`.
- Confirm: `await ConfirmationService.ShowAsync(title, text) == ConfirmationResult.YES`.
- Pick from a list: `SearchOptions<T>(relevance, text)` created in `OnInitialized`,
  `<SearchDialog Options="…" @bind-Visible="…" />`, then `await options.Result` (null on cancel).
- SVG context menu: `<SvgContextMenu Target="#selector">` with `SvgContextMenuItem` and
  `SvgContextSubmenu`, rendered through the `svg-context-menu` section in the canvas.
