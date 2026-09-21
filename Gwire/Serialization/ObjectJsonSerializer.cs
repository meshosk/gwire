using Microsoft.AspNetCore.Components.Forms;
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
    /// Serializes the given object as JSON into the output stream.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="output">The stream that receives the JSON data.</param>
    public static async Task ExportAsync<T>(T value, Stream output)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(output);

        await JsonSerializer.SerializeAsync(output, value, Options);
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
            var value = await JsonSerializer.DeserializeAsync<T>(stream, Options)
                ?? throw new InvalidDataException("The JSON file does not contain a part.");

            return value;
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("The selected file is not valid part JSON.", exception);
        }
    }

}
