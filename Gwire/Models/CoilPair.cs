using Gwire.Models.Base;
using System.Text.Json.Serialization;
using System.Numerics;

namespace Gwire.Models;

/// <summary>
/// The two terminals of one pickup coil. Point1 is Signal and Point2 is Ground
/// by convention; neither terminal is forced to the scheme's ground.
/// </summary>
public sealed class CoilPair : ConnectionPair
{
    [JsonIgnore]
    public override string TypeName => "Coil";

    public string Color { get; set; } = "#e63946";

    public CoilWindingDirection Winding { get; set; } = CoilWindingDirection.CCW;

    public CoilPolarity Polarity { get; set; } = CoilPolarity.North;

    // All coils use the same reference source resistance in this illustrative model.
    public override Complex? GetImpedance(double frequency) => new Complex(ReferenceResistance, 0);
}
