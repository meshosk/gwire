using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Services;
using Gwire.Models.Pairs;
using Gwire.Models.Signal;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace Gwire.Components.Renderers;

/// <summary>
/// Draws a continuous bass-to-treble color ribbon for each reachable coil contribution.
/// </summary>
public partial class CircuitSignalRenderer : IDisposable
{
    private const int GradientSteps = 32;
    private const float SignalWidth = 14;
    private const float SignalSpacing = 16;
    // Elements whose two ends carry different levels are drawn narrowing in this many pieces.
    private const int TaperPieces = 8;
    private readonly HashSet<BaseCircuitPart> observedParts = [];
    private SignalTraceResult? trace;

    private IReadOnlyList<SignalConnection> connections => trace?.Connections ?? [];

    [Parameter, EditorRequired]
    public Circuit Circuit { get; set; } = default!;

    [Parameter]
    public bool Enabled { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void OnParametersSet()
    {
        var parts = Enabled ? Circuit.Parts.ToHashSet() : new HashSet<BaseCircuitPart>();
        foreach (var part in observedParts.Except(parts).ToArray())
        {
            part.Changed -= HandleChanged;
            observedParts.Remove(part);
        }
        foreach (var part in parts.Except(observedParts))
        {
            part.Changed += HandleChanged;
            observedParts.Add(part);
        }
        trace = Enabled ? CircuitSignalTracer.TraceWithPoints(Circuit) : null;
    }

    private void HandleChanged()
    {
        trace = Enabled ? CircuitSignalTracer.TraceWithPoints(Circuit) : null;
        StateHasChanged();
    }

    private static float[] GetGradientWidths(SignalSpectrum spectrum)
    {
        var widths = new float[GradientSteps];
        for (var step = 0; step < GradientSteps; step++)
        {
            var level = spectrum.GetLevel((step + 0.5) / GradientSteps);
            // A nonlinear display scale makes weak currents visible while preserving zero.
            widths[step] = (float)(SignalWidth / GradientSteps * Math.Sqrt(Math.Min(1, level)));
        }
        return widths;
    }

    private static IEnumerable<(string Path, float Width, int Step)> GetBands(
        SignalConnection connection, CoilPair coil, float coilOffset)
    {
        var start = GetGradientWidths(connection.Signals[coil]);
        var end = GetGradientWidths(connection.EndSignals[coil]);
        var pieces = start.Zip(end).Any(widths => MathF.Abs(widths.First - widths.Second) > 0.01f) ? TaperPieces : 1;
        for (var piece = 0; piece < pieces; piece++)
        {
            var position = (piece + 0.5f) / pieces;
            var widths = start.Zip(end, (first, second) => first + (second - first) * position).ToArray();
            var nextOffset = coilOffset - widths.Sum() / 2;
            for (var step = 0; step < GradientSteps; step++)
            {
                var offset = nextOffset + widths[step] / 2;
                nextOffset += widths[step];
                if (widths[step] <= 0)
                {
                    continue;
                }
                var path = SignalPathGeometry.GetPath(connection.From, connection.To, offset,
                    (float)piece / pieces, (piece + 1f) / pieces);
                if (path.Length > 0)
                {
                    yield return (path, widths[step], step);
                }
            }
        }
    }

    private static float GetFlowWidth(SignalConnection connection, CoilPair coil) =>
        (GetGradientWidths(connection.Signals[coil]).Sum() + GetGradientWidths(connection.EndSignals[coil]).Sum()) / 2;

    // Keep adjacent gradient strokes joined as their widths change.
    private static string GradientStrokeWidth(float width) =>
        (width + Math.Min(0.2f, width / 2)).ToString(CultureInfo.InvariantCulture);

    private static string GetGradientColor(string color, int step)
    {
        var position = step / (GradientSteps - 1f);
        var darkSide = position <= 0.5f;
        var percentage = darkSide ? 55 + position * 90 : 100 - (position - 0.5f) * 110;
        var shade = darkSide ? "black" : "white";
        return FormattableString.Invariant($"color-mix(in srgb, {color} {percentage}%, {shade})");
    }

    public void Dispose()
    {
        foreach (var part in observedParts)
        {
            part.Changed -= HandleChanged;
        }
    }
}
