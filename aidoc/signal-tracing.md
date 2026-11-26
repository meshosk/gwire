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
   impedance and de-duplicates ideal wires. Zero-impedance connections between the two
   terminals of *any* coil are dropped: older parts draw a coil as a wire between its
   terminals, which would otherwise short it. The adjacency list excludes the coil itself
   (so the coil cannot close its own path).
3. `SignalPathFinder.FindConnections(coil, network)` finds every connection on a simple
   path between the coil's Signal point and its Ground point (the coil's loop). No path
   means the coil is not heard and is skipped. `SignalPathOrientation.Rank` orders the
   loop points from Signal to Ground (st-numbering).
4. `SignalPropagation.Calculate` spreads the signal along the ranked loop (rules below)
   using each part's `GetImpedance(frequency)` for 120 Hz, 1 kHz, and 5 kHz. No system of
   equations is solved. It returns the level at every loop point and a `SignalFlow`
   (start and end level, in spreading direction) for every loop connection.
5. `CircuitSignalTracer` turns this into `SignalTraceResult`: per-connection signals per
   coil at both ends (`SignalConnection.Signals` at `From`, `EndSignals` at `To`, used for
   the colored ribbons) and per-point signals per coil (used by the point inspection
   popup, `PointSignalModal`).

### How a coil's signal spreads (required behavior)

The ribbons visualize where a coil's signal goes and how each part shapes its tone. This
is illustrative, not exact electronics. The signal spreads point by point, in the order it
travels around the coil's loop, and carries the impedance of the circuit before it:

1. **A coil's signal starts at its Signal point (`Point1`) at 100 % and ends at its own
   Ground point (`Point2`).** The impedance before the Signal point is the coil's own
   (`CoilPair.GetImpedance`, 10 kOhm). Each coil uses its own Ground, also in series wiring.
2. **A wire or cable never changes the signal** or the impedance before it. A short to
   ground therefore happens only at the coil's Ground point, never along a cable.
3. **Z_after** of a point is the impedance from it to Ground along the loop, computed
   backwards from Ground (0): parts in series add up, parallel branches combine as
   parallel impedances. It is only used to split forks.
4. **A part changes the signal only between its own two points**, by its own
   `ElectricalConnection.GetTransfer(frequency, Z_before)`; nothing after it affects it:
   - capacitor, resistor, other coil (`ConnectionPair.GetTransfer`):
     |Z_before| / |Z_before + Z_part|, so a capacitor passes little bass and almost all
     treble;
   - linear pot segment (`Potentiometer.GetTransfer`): the share of the other segment in
     the whole track, so A-W at 50 % passes 50 %.
   The impedance before its output is Z_before + Z_part. The ribbon narrows along the part.
5. **At a fork** the level splits by the admittance of the branches (part + Z_after);
   a branch with no impedance takes everything. **At a join** incoming branches add up and
   their impedances before combine in parallel.
6. **Dead-end branches** (e.g. the cable to the output jack) and dangling loops are
   ignored: only the coil's loop from Signal to Ground is drawn.
7. Each ribbon is a bass-to-treble gradient whose width per band is the level in that
   band. Anything below `MinimumVisibleLevel` (2 % of the coil's signal) is not drawn,
   and a connection is drawn only when both of its ends are above it.
8. The flow animation runs in the spreading direction, from Signal toward Ground.

`SignalSpectrum` keeps complex phasors internally so out-of-phase coils cancel when
combined with `Combine`; `Bass`/`Mids`/`Treble` are magnitudes.

Element models used by the calculation: coils have a fixed reference impedance
(10 kΩ), resistors are real, capacitors are `-j/(2πfC)`, potentiometer segments are
resistors proportional to the wiper position, and wires/closed contacts are 0 Ω.

## When changing this area

- Keep path highlighting and signal mode on the same connectivity source
  (`GetElectricalConnections` / `GetConnectedPoints`), otherwise they will disagree.
- `SignalNetwork`, `SignalPathFinder`, `SignalPathOrientation`, and `SignalPropagation`
  are `internal`; the public entry point is `CircuitSignalTracer`.
- There are no unit tests. Verify by building and checking known schemes in the app,
  and ask the user before running the build (see AGENTS.md).
