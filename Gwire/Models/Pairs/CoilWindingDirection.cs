using System.Text.Json.Serialization;

namespace Gwire.Models.Pairs;

[JsonConverter(typeof(JsonStringEnumConverter<CoilWindingDirection>))]
public enum CoilWindingDirection
{
    CCW,
    CW
}
