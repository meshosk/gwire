# Domain model

All model classes are in `Gwire/Models` (namespace `Gwire.Models`),
`Gwire/Models/Base` (namespace `Gwire.Models.Base`), `Gwire/Models/Signal` (namespace
`Gwire.Models.Signal`: `SignalSpectrum`, `SignalConnection`, `SignalTraceResult`), and `Gwire/Models/Pairs`
(namespace `Gwire.Models.Pairs`: coil, resistor, and capacitor pairs, `Potentiometer`, coil enums, `SourceGroundPair`; their base `ConnectionPair` stays in `Models/Base`). They are plain mutable classes that
are serialized directly to JSON, so every public property is part of the file format
(see [serialization.md](serialization.md)).

## The circuit graph at a glance

```
Circuit (a scheme)
 └─ Parts: List<BaseCircuitPart>
      ├─ CircuitPart   (any physical part: pickup, switch, pot, jack, cap…)
      │    ├─ Points: List<ConnectionPoint>        terminals, in part-local coordinates
      │    ├─ StateGroups: List<PartStateGroup>    independently switchable groups
      │    │    └─ States: List<PartState>, ActiveState
      │    │         └─ ConnectionGroups: List<ConnectionGroup>  points shorted together in that state
      │    └─ ConnectionPairs: List<ConnectionPair> CoilPair | ResistorPair | CapacitorPair (Point1/Point2)
      │                                             | Potentiometer (A / W / B), terminals referencing Points
      └─ CablePart     (a wire with exactly two Points and optional bend points)
```

Electrical connectivity has two layers:

1. **External connections**: `ConnectionPoint.ConnectionPoints` links a point to points of
   other parts. In practice a cable endpoint connects to a part terminal. Always use
   `Connect()` / `Disconnect()`; they keep the link bidirectional and move cable
   endpoints onto the part terminal.
2. **Internal connections**: `BaseCircuitPart.GetConnectedPoints(point)` and
   `GetElectricalConnections()` describe what the part connects inside itself:
   - `CablePart`: its two endpoints are connected to each other.
   - `CircuitPart`: every pair of points inside each `ConnectionGroup` of each group's
     **active** state, plus each `ConnectionPair` that has an impedance, plus each valid
     potentiometer's two track segments (A–W and W–B, resistance by wiper position).

`ElectricalConnection(From, To, Pair, Resistance)` is the shared representation used by
path highlighting and the signal calculation. `Pair == null` means a wire, contact, or
potentiometer segment.

## Key types

| Type | Notes |
| --- | --- |
| `Circuit` | Name, Description, Tags, Parts. The scheme document. |
| `BaseCircuitPart` | Abstract. Name, Description, Points, `Changed` event, `Clone()`. JSON polymorphic with `$type` = `cable` or `circuitPart`. |
| `CircuitPart` | `Id` (Guid), size (`Width`/`Height`), `SvgMarkup` artwork with `SvgLocalPosition` and `SvgRotation`, `SchemePosition` on the canvas, `Tags`, optional `CustomTextValue` with `ValuePosition` (for values like "500k"), `IsFromRepo` (not serialized; `DisplayName` prefixes `*`). Move it with `MoveTo()` so attached cable ends follow. |
| `CablePart` | `Color`, `BendPoints` (scheme coordinates). Endpoint `LocalPosition` is in **scheme** coordinates, unlike part points. `GetPathData()` builds the SVG path; `FindBendPointInsertion()` does hit testing. |
| `ConnectionPoint` | `Label`, `LocalPosition` (relative to the owning part), `Owner`, `ConnectionPoints`, `IsHighlighted` (UI only). `FindPathTo()` does BFS through external and active internal connections. |
| `PartStateGroup` / `PartState` / `ConnectionGroup` | A part has one or more state groups that switch independently (on the scheme canvas each group with more than one state becomes a submenu in the part's context menu). Each group has states (for example switch positions) and one `ActiveState`. A state lists groups of points that are connected while it is active. Note the property spelling `ConnectionGroup.ConnectedPints`: it is part of the JSON format, so do not rename it. |
| `ConnectionPair` | Abstract element between existing points of the same part. Each subclass declares its own terminals and returns them from `GetPoints()` (two for coil, resistor, capacitor; three for a potentiometer); `CircuitPart.AddPair`/`RemovePair` add and remove exactly those points. `GetElectricalConnections()` gives its internal connections, `GetImpedance(frequency)` and `GetTransfer(frequency, sourceImpedance)` the signal share passed between its points. JSON `$type`: `coil`, `resistor`, `capacitor`, `potentiometer`. |
| `CoilPair` | A pickup coil. `Point1` = signal, `Point2` = ground by convention. `Color` (used in signal mode), `Winding` (CW/CCW), `Polarity` (North/South). |
| `ResistorPair` / `CapacitorPair` | `Resistance` in ohms / `Capacitance` in farads. Invalid values mean "not conducting". |
| `Potentiometer` | A `ConnectionPair`. `GetImpedance` returns null; its two segments carry their resistance in `ElectricalConnection.Resistance`. `Resistance`, `Position` restricted to 0, 25, 50, 75, 100 (setter throws otherwise), terminals `A`, `W`, `B`. |
| `SourceGroundPair` | Editor-only measurement terminals (not part of `Circuit`, not saved). See [signal-tracing.md](signal-tracing.md). |
| `SignalSpectrum` | Illustrative bass/mids/treble levels at 120 Hz / 1 kHz / 5 kHz, with phasors kept internally so contributions can cancel. Not real measurements. |
| `Point` | Mutable `struct` with float `X`/`Y`. Use `Copy()` when storing a position for later (undo data). |
| `SchemeEditorState` / `PartEditorState` | Per-editor UI state kept by `EditorStateService`. |

## Rules that are easy to break

- **Reference identity matters.** Points, pairs, potentiometers, and connection groups refer
  to the same `ConnectionPoint` instances in `Points`. Compare with `ReferenceEquals`, and
  never replace a point object while other structures still reference it.
- **Clone, don't share.** `CircuitPart.Clone()` deep-copies points, pairs, potentiometers,
  and states, remaps references to the cloned points, drops external connections, and
  keeps each group's active state. Use it when adding a catalog part to a scheme,
  duplicating, saving to the catalog, or snapshotting for undo. Adding to a scheme and
  duplicating then call `ResetStates()`, so a placed part starts in the first state of each group.
- The part editor never sets `ActiveState`; it edits the state selected in `PartEditorState`.
- `CircuitPart` always has at least one `PartStateGroup` (enforced on deserialization).
- Coordinates: part points are part-local; `SchemePosition` places the part; cable points
  and bend points are already in scheme coordinates.
- Keep public property names stable: they are the JSON contract for files in users' hands
  and in the catalog repository.
