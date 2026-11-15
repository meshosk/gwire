using Gwire.Models;
using Gwire.Models.Base;
using Gwire.Services;
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
    private readonly HashSet<BaseCircuitPart> observedParts = [];
    private IReadOnlyList<SignalConnection> connections = [];

    // Slight overlap hides antialiasing seams between adjacent gradient layers.
    private static string GradientStrokeWidth =>
        (SignalWidth / GradientSteps + 0.2f).ToString(CultureInfo.InvariantCulture);

    [Parameter, EditorRequired]
    public Circuit Circuit { get; set; } = default!;

    protected override void OnParametersSet()
    {
        var parts = Circuit.Parts.ToHashSet();
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
        connections = CircuitSignalTracer.Trace(Circuit);
    }

    private void HandleChanged()
    {
        connections = CircuitSignalTracer.Trace(Circuit);
        StateHasChanged();
    }

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
