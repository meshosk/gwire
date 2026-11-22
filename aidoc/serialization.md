# JSON import/export

Files: `Gwire/Serialization/ObjectJsonSerializer.cs` (public entry point) and
`Gwire/Serialization/CircuitJsonSerializer.cs` (scheme format). Uses
`System.Text.Json` only.

## Entry point

`ObjectJsonSerializer.ExportAsync<T>(value, stream)` and `ImportAsync<T>(stream | IBrowserFile)`:

- `T == Circuit` is delegated to `CircuitJsonSerializer`.
- Anything else (in practice `CircuitPart`) is serialized directly with camelCase names,
  indented output, `ReferenceHandler.Preserve` (`$id` / `$ref`), and `MaxDepth = 1024`
  for old deeply nested files.
- Import failures are wrapped in `InvalidDataException`; callers show a toast through
  `MessageService`.

File names created by the editors: `<name>.scheme.json` and `<name>.part.json`.

## Polymorphism

Type discriminators use the `$type` property:

| Base type | Values |
| --- | --- |
| `BaseCircuitPart` | `cable` → `CablePart`, `circuitPart` → `CircuitPart` |
| `ConnectionPair` | `coil` → `CoilPair`, `resistor` → `ResistorPair`, `capacitor` → `CapacitorPair` |

A new subclass needs a `[JsonDerivedType]` attribute on its base class, or it cannot be
saved or loaded.

## Scheme format (version 1)

```json
{
  "schemeFormatVersion": 1,
  "circuit": { "name": "...", "description": "...", "tags": [], "parts": [ ... ] },
  "connections": [ { "from": { "$ref": "12" }, "to": { "$ref": "40" } } ]
}
```

- `ConnectionPoint.connectionPoints` is **not** written inside parts. External links are
  written once, after all parts, in `connections`, so wiring does not make the JSON deeply
  nested. `JsonPropertyOrder` guarantees parts come first, which is required because
  `$ref` can only point to objects that were already read.
- Export validates that every connection points to a point inside the scheme.
- Import validates the version (only 1 is accepted), that each point appears once and
  belongs to its part, and that every connection references known points. It then
  restores adjacency lists directly, without calling `Connect` (no cable moves, no events).
- A file without `schemeFormatVersion` is read as a legacy file: a bare `Circuit` with
  connections nested inside points.

If you change the scheme structure in an incompatible way, bump `FormatVersion` and keep
reading older versions; users and the catalog repository have existing files.

## What is not serialized

`[JsonIgnore]` properties: `CircuitPart.IsFromRepo`, `DisplayName`,
`ConnectionPoint.IsHighlighted`, `ConnectionPair.TypeName`, `Potentiometer.IsValid`,
`CableBendPoint.Position`. Editor state (`SchemeEditorState`, `PartEditorState`,
`SourceGroundPair`) is never saved.

## Catalog parts

`GwirePartsService` reads catalog part files with `JsonSerializerDefaults.Web` and
`ReferenceHandler.Preserve`, which matches the part export format.
