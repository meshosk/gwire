using Gwire.Models;
using Gwire.Models.Base;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Gwire.Serialization;

/// <summary>
/// Stores external point connections after the parts so wiring topology does not increase JSON depth.
/// </summary>
internal static class CircuitJsonSerializer
{
    private const int FormatVersion = 1;
    private static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(typeInfo =>
        {
            if (typeInfo.Type == typeof(ConnectionPoint))
            {
                var connections = typeInfo.Properties.Single(property => property.Name == "connectionPoints");
                connections.ShouldSerialize = (_, _) => false;
            }
        });

        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            TypeInfoResolver = resolver
        };
    }

    public static async Task ExportAsync(Circuit circuit, Stream output)
    {
        var points = GetPoints(circuit);
        var connections = new List<PointConnection>();
        foreach (var part in circuit.Parts)
        {
            foreach (var point in part.Points)
            {
                foreach (var connectedPoint in point.ConnectionPoints)
                {
                    if (connectedPoint is null || !points.Contains(connectedPoint))
                    {
                        throw new JsonException("A connection references a point outside the scheme.");
                    }

                    connections.Add(new PointConnection { From = point, To = connectedPoint });
                }
            }
        }

        // One serialization call shares the framework's reference resolver between the circuit and its links.
        await JsonSerializer.SerializeAsync(output, new SchemeDocument
        {
            Circuit = circuit,
            Connections = connections
        }, Options);
    }

    public static async Task<Circuit> ImportAsync(Stream input, JsonSerializerOptions legacyOptions)
    {
        using var document = await JsonDocument.ParseAsync(input, new JsonDocumentOptions
        {
            MaxDepth = legacyOptions.MaxDepth
        });
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("schemeFormatVersion", out var version))
        {
            return root.Deserialize<Circuit>(legacyOptions)
                ?? throw new JsonException("The JSON file does not contain a scheme.");
        }

        if (version.ValueKind != JsonValueKind.Number || !version.TryGetInt32(out var number) || number != FormatVersion)
        {
            throw new JsonException("The scheme file format version is not supported.");
        }

        var scheme = root.Deserialize<SchemeDocument>(Options)
            ?? throw new JsonException("The JSON file does not contain a scheme.");
        if (scheme.Circuit is null || scheme.Connections is null)
        {
            throw new JsonException("The scheme is missing its circuit or connections.");
        }

        var points = GetPoints(scheme.Circuit);
        foreach (var connection in scheme.Connections)
        {
            if (connection is null || connection.From is null || connection.To is null ||
                !points.Contains(connection.From) || !points.Contains(connection.To))
            {
                throw new JsonException("A connection references a point outside the scheme.");
            }
        }

        // Restore the saved adjacency lists directly, without moving cable endpoints or raising UI events.
        foreach (var point in points)
        {
            point.ConnectionPoints.Clear();
        }
        foreach (var connection in scheme.Connections)
        {
            connection.From.ConnectionPoints.Add(connection.To);
        }

        return scheme.Circuit;
    }

    private static HashSet<ConnectionPoint> GetPoints(Circuit circuit)
    {
        if (circuit.Parts is null)
        {
            throw new JsonException("The scheme is missing its parts.");
        }

        var points = new HashSet<ConnectionPoint>();
        foreach (var part in circuit.Parts)
        {
            if (part is null || part.Points is null)
            {
                throw new JsonException("The scheme contains an invalid part.");
            }

            foreach (var point in part.Points)
            {
                if (point is null || point.ConnectionPoints is null || !ReferenceEquals(point.Owner, part) || !points.Add(point))
                {
                    throw new JsonException("A scheme point has an invalid owner or occurs more than once.");
                }
            }
        }
        return points;
    }

    private sealed class SchemeDocument
    {
        // Parts must be read before links: System.Text.Json resolves references to preceding objects.
        [JsonPropertyOrder(-1)]
        public int SchemeFormatVersion { get; set; } = FormatVersion;

        [JsonRequired, JsonPropertyOrder(0)]
        public Circuit Circuit { get; set; } = default!;

        [JsonRequired, JsonPropertyOrder(1)]
        public List<PointConnection> Connections { get; set; } = [];
    }

    private sealed class PointConnection
    {
        [JsonRequired]
        public ConnectionPoint From { get; set; } = default!;

        [JsonRequired]
        public ConnectionPoint To { get; set; } = default!;
    }
}
