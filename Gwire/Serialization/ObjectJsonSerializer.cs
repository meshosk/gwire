using Microsoft.AspNetCore.Components.Forms;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gwire.Serialization;

/// <summary>
/// Imports and exports circuit parts as JSON while preserving object references.
/// </summary>
public static class ObjectJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.Preserve
    };

/// <summary>
/// Creates a URI with data for direct download of the given object as JSON.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="value"></param>
/// <returns></returns>
    public static string Export<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var json = JsonSerializer.Serialize(value, Options);
        return $"data:application/json;charset=utf-8,{Uri.EscapeDataString(json)}";
    }
/// <summary>
/// Imports an object from a JSON file uploaded by user using file upload input.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="file"></param>
/// <returns></returns>
/// <exception cref="InvalidDataException"></exception>
    public static async Task<T> ImportAsync<T>(IBrowserFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        try
        {
            await using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var json = await reader.ReadToEndAsync();
            var value = JsonSerializer.Deserialize<T>(json, Options)
                ?? throw new InvalidDataException("The JSON file does not contain a part.");

            return value;
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("The selected file is not valid part JSON.", exception);
        }
    }

}
