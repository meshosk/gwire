using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gwire.Serialization;

/// <summary>
/// Provides JSON serialization for a specific object type while preserving object references.
/// </summary>
public class ObjectJsonSerializer<T>
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.Preserve
    };

    public string Serialize(T value) => JsonSerializer.Serialize(value, Options);

    public T? Deserialize(string json) => JsonSerializer.Deserialize<T>(json, Options);
}
