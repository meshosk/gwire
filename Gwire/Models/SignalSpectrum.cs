using System.Numerics;

namespace Gwire.Models;

/// <summary>
/// Normalized, illustrative signal levels rather than electrical measurements.
/// </summary>
public readonly record struct SignalSpectrum(double Bass, double Mids, double Treble)
{
    private Complex? BassPhasor { get; init; }
    private Complex? MidsPhasor { get; init; }
    private Complex? TreblePhasor { get; init; }

    public static IReadOnlyList<double> Frequencies { get; } = Array.AsReadOnly(new[] { 120d, 1_000d, 5_000d });

    public static SignalSpectrum Full { get; } = new(1, 1, 1);

    public double Level => (Bass + Mids + Treble) / 3;

    public double this[int band] => band switch
    {
        0 => Bass,
        1 => Mids,
        2 => Treble,
        _ => throw new ArgumentOutOfRangeException(nameof(band))
    };

    public SignalSpectrum WithBand(int band, double level) => band switch
    {
        0 => this with { Bass = level, BassPhasor = null },
        1 => this with { Mids = level, MidsPhasor = null },
        2 => this with { Treble = level, TreblePhasor = null },
        _ => throw new ArgumentOutOfRangeException(nameof(band))
    };

    public SignalSpectrum Strongest(SignalSpectrum other) => new(
        Math.Max(Bass, other.Bass), Math.Max(Mids, other.Mids), Math.Max(Treble, other.Treble));

    /// <summary>
    /// Adds contributions after the network has accounted for their impedances.
    /// Keep phase until after addition so opposite contributions can cancel.
    /// </summary>
    public SignalSpectrum Combine(SignalSpectrum other) => FromPhasors(
        GetPhasor(0) + other.GetPhasor(0), GetPhasor(1) + other.GetPhasor(1), GetPhasor(2) + other.GetPhasor(2));

    public static SignalSpectrum FromPhasors(Complex bass, Complex mids, Complex treble) =>
        new(bass.Magnitude, mids.Magnitude, treble.Magnitude)
        {
            BassPhasor = bass,
            MidsPhasor = mids,
            TreblePhasor = treble
        };

    public Complex GetPhasor(int band) => band switch
    {
        0 => BassPhasor ?? new Complex(Bass, 0),
        1 => MidsPhasor ?? new Complex(Mids, 0),
        2 => TreblePhasor ?? new Complex(Treble, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(band))
    };

    public double GetLevel(double position)
    {
        position = Math.Clamp(position, 0, 1);
        return position <= 0.5
            ? Bass + (Mids - Bass) * position * 2
            : Mids + (Treble - Mids) * (position - 0.5) * 2;
    }
}
