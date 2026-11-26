using Gwire.Models.Base;
using System.Numerics;

namespace Gwire.Models;

/// <summary>
/// A physical internal connection shared by path tracing and signal calculation.
/// A null Pair represents a wire, contact, or a resistive segment of a potentiometer
/// (then Potentiometer is set).
/// </summary>
public record ElectricalConnection(ConnectionPoint From, ConnectionPoint To, ConnectionPair? Pair = null, double Resistance = 0,
    Potentiometer? Potentiometer = null)
{
    public Complex? GetImpedance(double frequency) => Pair is null
        ? new Complex(Resistance, 0) : Pair.GetImpedance(frequency);

    /// <summary>
    /// Share of the signal this element passes between its own two points (0 to 1), given the
    /// impedance of the circuit before it. Wires pass everything.
    /// </summary>
    public double GetTransfer(double frequency, Complex sourceImpedance) =>
        Potentiometer is not null ? Potentiometer.GetTransfer(this)
        : Pair is not null ? Pair.GetTransfer(frequency, sourceImpedance)
        : ConnectionPair.GetSeriesTransfer(new Complex(Resistance, 0), sourceImpedance);
}
