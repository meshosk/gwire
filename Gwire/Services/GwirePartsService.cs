using Gwire.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gwire.Services;

/// <summary>
/// Service provides parts for scheme editor or for editing them in part editor
/// </summary>
/// <param name="httpClient"></param>
public sealed class GwirePartsService(HttpClient httpClient)
{
    /// <summary>
    /// Where to search for repo index. 
    /// </summary>
    private const string IndexUrl = "https://raw.githubusercontent.com/meshosk/gwire-parts-catalog/refs/heads/main/index.json";
    private static readonly JsonSerializerOptions PartJsonOptions = new(JsonSerializerDefaults.Web)
    {
        ReferenceHandler = ReferenceHandler.Preserve
    };

    /// <summary>
    /// List of tags that can be used to filter parts in the scheme editor.
    /// </summary>
    public List<string> Tags { get; } = [];
    /// <summary>
    /// List of parts that can be used in the scheme editor. It contains user created parts and parts from the Gwire parts catalog repository. 
    /// </summary>
    public List<CircuitPart> Parts { get; } = [];

    public void SavePart(CircuitPart part)
    {
        // no nulls 
        ArgumentNullException.ThrowIfNull(part);

        var savedPart = part.Clone(); // saving as clone, because clone has disconnected points
        var index = Parts.FindIndex(existing => existing.Id == part.Id);

        if (index >= 0)
        {
            Parts[index] = savedPart;
        }
        else
        {
            Parts.Add(savedPart);
        }
    }

    public async Task InitializeAsync()
    {
        var index = await httpClient.GetFromJsonAsync<GwireRepoIndex>(IndexUrl)
            ?? throw new InvalidDataException("The Gwire parts catalog index is empty.");

        var tagsTask = LoadTagsAsync(index.Tags);
        var parts = await LoadPartsAsync(index.PartsDirectory, index.PartsIndex);
        var tags = await tagsTask;

        Tags.Clear();
        Tags.AddRange(tags);
        Parts.Clear();
        Parts.AddRange(parts);
    }

    private async Task<List<string>> LoadTagsAsync(string tagsFile)
    {
        if (string.IsNullOrWhiteSpace(tagsFile))
        {
            return [];
        }

        var tagsUrl = new Uri(new Uri(IndexUrl), tagsFile).AbsoluteUri;
        return await httpClient.GetFromJsonAsync<List<string>>(tagsUrl)
            ?? throw new InvalidDataException("The Gwire parts catalog tags file is empty.");
    }

    private async Task<List<CircuitPart>> LoadPartsAsync(string partsDirectory, string partsIndexFile)
    {
        if (string.IsNullOrWhiteSpace(partsDirectory) || string.IsNullOrWhiteSpace(partsIndexFile))
        {
            return [];
        }

        var partsIndexUrl = new Uri(new Uri(IndexUrl), partsIndexFile).AbsoluteUri;
        var partsDirectoryUrl = new Uri(new Uri(IndexUrl), partsDirectory);
        var partFiles = await httpClient.GetFromJsonAsync<List<string>>(partsIndexUrl)
            ?? throw new InvalidDataException("The Gwire parts catalog parts index is empty.");

        var parts = await Task.WhenAll(partFiles.Select(async partFile =>
        {
            var partUrl = new Uri(partsDirectoryUrl, partFile).AbsoluteUri;
            var part = await httpClient.GetFromJsonAsync<CircuitPart>(partUrl, PartJsonOptions)
                ?? throw new InvalidDataException($"The Gwire parts catalog part '{partFile}' is empty.");
            part.IsFromRepo = true;
            return part;
        }));

        return [.. parts];
    }

    /// <summary>
    /// Reprezents the structure of the index.json file in the Gwire parts catalog repository.
    /// </summary>
    private sealed class GwireRepoIndex
    {
        public string Tags { get; set; } = string.Empty;

        [JsonPropertyName("partsDir")]
        public string PartsDirectory { get; set; } = string.Empty;

        public string PartsIndex { get; set; } = string.Empty;
    }
}
