using System.Text.Json.Serialization;

namespace Gwire.Models;

public record struct CableBendPoint(float X, float Y, bool IsRounded = false)
{
    [JsonIgnore]
    public readonly Point Position => new(X, Y);

    public readonly CableBendPoint Copy() => this;
}
