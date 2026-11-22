# Parts catalog

Parts, tags, and example schemes come from the separate repository
[meshosk/gwire-parts-catalog](https://github.com/meshosk/gwire-parts-catalog). The client
reads it over raw GitHub URLs; nothing is written back.

Code: `Gwire/Services/RepoService/` (`GwirePartsService`, `GwireRepoIndex`,
`GwireSchemeInfo`; namespace `Gwire.Services`).

## Loading

1. `MainLayout` shows `RepositoryLoadingDialog` at startup and calls
   `GwirePartsService.InitializeAsync()`. The editors are not rendered until loading
   succeeds or the user chooses to continue without the repository.
2. `InitializeAsync` fetches
   `https://raw.githubusercontent.com/meshosk/gwire-parts-catalog/refs/heads/main/index.json`
   (`GwireRepoIndex`), then in parallel:
   - `tags`: a JSON array of tag strings,
   - `partsIndex` + `partsDir`: a list of part file names, each loaded as a `CircuitPart`
     and marked `IsFromRepo = true`,
   - `schemesIndex` (+ `schemesDir` for later downloads): a list of `GwireSchemeInfo`
     (file, name, description, tags).
   Paths are resolved relative to `index.json`. Requests use browser `no-cache`.
   A missing (404) tags, parts index, part file, or schemes index is logged and skipped.
3. Parts the user saved locally (`!IsFromRepo`) are kept across reloads. `Changed` is raised
   at the end; pages subscribe to refresh.
4. The "Reload repository data" toolbar button calls the cascading `ReloadRepository`
   callback, which reopens the loading dialog.

`LoadSchemeAsync(info)` downloads one scheme file as a stream; the scheme editor imports it
through the normal JSON import path.

## Using catalog data

- `Parts` feeds the "Add part" search dialog in the scheme editor and "Get part from repo"
  in the part editor. Always `Clone()` a catalog part before putting it in a scheme or
  editing it.
- `Tags` is the only list of allowed tags: both editors let users pick existing tags only.
- `SavePart(part)` stores a clone in memory, replacing a part with the same `Id`. Nothing
  persists after a page reload; to keep a part, the user exports it as JSON.

The desktop app also reads the catalog from GitHub, so it needs network access for it.
