# Path and signal tracing

The scheme editor has two mutually exclusive analysis overlays
(`SchemeEditorState.ShowSourceGroundPair` and `ShowSignalMode` turn each other off).
Both use the same connectivity: external point connections plus each part's internal
connections for its **current** active states and potentiometer positions
(see [domain-model.md](domain-model.md#the-circuit-graph-at-a-glance)). Changing a switch
state or pot position therefore changes the result immediately.

The results are illustrative, meant to show routing and rough tonal effect. They are not
an electrical simulation, and documentation or UI text should not present them as one.

## Measurement pair (source-ground path)

- `SourceGroundPair` (`Models/SourceGroundPair.cs`) holds two editor-only terminals,
  `PowerSource` and `Ground`, each a small `CircuitPart` with one point and inline SVG.
  They live in `SchemeEditorState.SourceGroundPairs`, not in the `Circuit`, and are not
  saved. A pair is reset whenever a new scheme is set.
- A terminal can be attached to any connection point (`AttachTo`, `CanAttachTo` prevents
  cycles); when attached it follows that point (`GetPosition`).
- `CircuitTerminalRenderer` draws and drags the terminals; `CircuitPathRenderer` does a
  BFS from the power source attachment and draws every reachable connection once.

## Signal mode (coils)

Pipeline, all in `Gwire/Services`, called once per render by `CircuitSignalRenderer`:

1. `CircuitSignalTracer.TraceWithPoints(circuit)` loops over every `CoilPair` in the circuit.
2. `SignalNetwork(circuit, coil)` collects all `ElectricalConnection`s that have an
   impedance, de-duplicates ideal wires, and builds an adjacency list that excludes the
   coil itself (so the coil cannot close its own path).
3. `SignalPathFinder.FindConnections(coil, network)` finds every connection that lies on
   some simple path between the coil's two terminals, using a biconnected-block DFS
   (dead-end branches and dangling loops are dropped). No path means the coil is not
   heard and is skipped.
4. `SignalPropagation.Calculate` collapses zero-impedance connections with union-find,
   builds a nodal admittance matrix for 120 Hz, 1 kHz, and 5 kHz, injects the coil as a
   current source (sign flipped by `Winding` × `Polarity`), and solves it with
   `SignalEquationSolver` (Gaussian elimination with partial pivoting).
5. The tracer returns `SignalTraceResult`: per-connection signals per coil (used for the
   colored ribbons) and per-point signals per coil (used by the point inspection popup,
   `PointSignalModal`).

`SignalSpectrum` keeps complex phasors internally so out-of-phase coils cancel when
combined with `Combine`; `Bass`/`Mids`/`Treble` are magnitudes.

Element models used by the calculation: coils have a fixed reference impedance
(10 kΩ), resistors are real, capacitors are `-j/(2πfC)`, potentiometer segments are
resistors proportional to the wiper position, and wires/closed contacts are 0 Ω.

## When changing this area

- Keep path highlighting and signal mode on the same connectivity source
  (`GetElectricalConnections` / `GetConnectedPoints`), otherwise they will disagree.
- `SignalNetwork`, `SignalPathFinder`, `SignalPropagation`, and `SignalEquationSolver`
  are `internal`; the public entry point is `CircuitSignalTracer`.
- There are no unit tests. Verify by building and checking known schemes in the app,
  and ask the user before running the build (see AGENTS.md).
