using System.Text.Json.Serialization;

namespace Gwire.Models.Pairs;

[JsonConverter(typeof(JsonStringEnumConverter<CoilPolarity>))]
public enum CoilPolarity
{
    North,
    South
}
