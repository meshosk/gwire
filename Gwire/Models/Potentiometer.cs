using Gwire.Models.Base;
using System.Text.Json.Serialization;

namespace Gwire.Models;

public class Potentiometer
{
    private int position = 100;

    public static IReadOnlyList<int> Positions { get; } = Array.AsReadOnly(new[] { 0, 25, 50, 75, 100 });

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Total track resistance in ohms.
    /// </summary>
    public double Resistance { get; set; }

    public int Position
    {
        get => position;
        set
        {
            if (!Positions.Contains(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Position must be 0, 25, 50, 75, or 100.");
            }
            position = value;
        }
    }

    [JsonRequired]
    public ConnectionPoint A { get; set; } = default!;

    [JsonRequired]
    public ConnectionPoint W { get; set; } = default!;

    [JsonRequired]
    public ConnectionPoint B { get; set; } = default!;

    [JsonIgnore]
    public bool IsValid => !string.IsNullOrWhiteSpace(Name) && double.IsFinite(Resistance) && Resistance > 0;

    public bool Contains(ConnectionPoint point) => ReferenceEquals(point, A) || ReferenceEquals(point, W) || ReferenceEquals(point, B);

    public IEnumerable<ElectricalConnection> GetElectricalConnections()
    {
        if (IsValid)
        {
            yield return new ElectricalConnection(A, W, Resistance: Resistance * (100 - Position) / 100, Potentiometer: this);
            yield return new ElectricalConnection(W, B, Resistance: Resistance * Position / 100, Potentiometer: this);
        }
    }

    /// <summary>
    /// A linear pot divides the signal between its two segments: a segment passes the share
    /// of the other segment in the whole track (A-W at 50 % passes 50 %).
    /// </summary>
    public double GetTransfer(ElectricalConnection segment)
    {
        var otherShare = ReferenceEquals(segment.From, A) || ReferenceEquals(segment.To, A)
            ? Position / 100d
            : (100 - Position) / 100d;
        return segment.Resistance <= 0 ? 1 : otherShare;
    }

    public Potentiometer Clone(IReadOnlyDictionary<ConnectionPoint, ConnectionPoint> clonedPoints)
    {
        var clone = (Potentiometer)MemberwiseClone();
        clone.A = clonedPoints[A];
        clone.W = clonedPoints[W];
        clone.B = clonedPoints[B];
        return clone;
    }
}
