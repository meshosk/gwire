using System.Text.Json.Serialization;

namespace Gwire.Models;

[JsonConverter(typeof(JsonStringEnumConverter<CoilPolarity>))]
public enum CoilPolarity
{
    North,
    South
}
