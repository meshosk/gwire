using System.Text.Json.Serialization;

namespace Gwire.Services;


/// <summary>
/// Represents the structure of the index.json file in the Gwire parts catalog repository.
/// </summary>
public sealed class GwireRepoIndex
{
    public string Tags { get; set; } = string.Empty;

    [JsonPropertyName("partsDir")]
    public string PartsDirectory { get; set; } = string.Empty;

    public string PartsIndex { get; set; } = string.Empty;

    public string SchemesIndex { get; set; } = string.Empty;
}
