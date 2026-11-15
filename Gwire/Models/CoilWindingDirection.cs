using System.Text.Json.Serialization;

namespace Gwire.Models;

[JsonConverter(typeof(JsonStringEnumConverter<CoilWindingDirection>))]
public enum CoilWindingDirection
{
    CCW,
    CW
}
