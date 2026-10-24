using Gwire.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gwire.Services;

/// <summary>
/// Service provides parts for scheme editor or for editing them in part editor
/// </summary>
/// <param name="httpClient"></param>
public sealed partial class GwirePartsService(HttpClient httpClient)
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

    /// <summary>
    /// Search metadata and file names from the repository's schemes index.
    /// </summary>
    public List<GwireSchemeInfo> Schemes { get; } = [];

    public event Action? Changed;

    private Uri? schemesDirectoryUrl;

    public async Task<Stream> LoadSchemeAsync(GwireSchemeInfo scheme)
    {
        ArgumentNullException.ThrowIfNull(scheme);
        if (schemesDirectoryUrl is null)
        {
            throw new InvalidDataException("The catalog does not specify a schemes directory.");
        }

        var schemeUrl = new Uri(schemesDirectoryUrl, scheme.File);
        return await httpClient.GetStreamAsync(schemeUrl);
    }

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
        var index = await GetRepositoryJsonAsync<GwireRepoIndex>(IndexUrl)
            ?? throw new InvalidDataException("The Gwire parts catalog index is empty.");

        var tagsTask = LoadTagsAsync(index.Tags);
        var schemesTask = LoadSchemesAsync(index.SchemesIndex);
        var partsTask = LoadPartsAsync(index.PartsDirectory, index.PartsIndex);
        await Task.WhenAll(tagsTask, schemesTask, partsTask);
        var parts = await partsTask;
        var tags = await tagsTask;
        var schemes = await schemesTask;

        schemesDirectoryUrl = string.IsNullOrWhiteSpace(index.SchemesDirectory)
            ? null
            : new Uri(new Uri(IndexUrl), index.SchemesDirectory);

        Tags.Clear();
    
        Tags.AddRange(tags.Where(t => t is not null));
        var ownParts = Parts.Where(part => !part.IsFromRepo).ToArray();
        Parts.Clear();
        Parts.AddRange(parts.Where(p => p is not null));
        Parts.AddRange(ownParts);
        Schemes.Clear();
        Schemes.AddRange(schemes.Where(s => s is not null));
        Changed?.Invoke();
    }

    private async Task<T?> GetRepositoryJsonAsync<T>(string url, JsonSerializerOptions? options = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.SetBrowserRequestCache(BrowserRequestCache.NoCache);
        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(options);
    }

    private async Task<List<GwireSchemeInfo>> LoadSchemesAsync(string schemesIndexFile)
    {
        if (string.IsNullOrWhiteSpace(schemesIndexFile))
        {
            return [];
        }

        var schemesIndexUrl = new Uri(new Uri(IndexUrl), schemesIndexFile).AbsoluteUri;
        try
        {
            return await GetRepositoryJsonAsync<List<GwireSchemeInfo>>(schemesIndexUrl) ?? [];
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Trace.WriteLine($"The Gwire parts catalog schemes index file '{schemesIndexFile}' was not found. No schemes will be available.");
            return [];
        }
    }

    private async Task<List<string>> LoadTagsAsync(string tagsFile)
    {
        if (string.IsNullOrWhiteSpace(tagsFile))
        {
            return [];
        }

        var tagsUrl = new Uri(new Uri(IndexUrl), tagsFile).AbsoluteUri;
        try {
            return await GetRepositoryJsonAsync<List<string>>(tagsUrl) ?? [];
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Trace.WriteLine($"The Gwire parts catalog tags file '{tagsFile}' was not found. No tags will be available.");
            return [];
        }

    }

    private async Task<List<CircuitPart>> LoadPartsAsync(string partsDirectory, string partsIndexFile)
    {
        if (string.IsNullOrWhiteSpace(partsDirectory) || string.IsNullOrWhiteSpace(partsIndexFile))
        {
            return [];
        }

        var partsIndexUrl = new Uri(new Uri(IndexUrl), partsIndexFile).AbsoluteUri;
        var partsDirectoryUrl = new Uri(new Uri(IndexUrl), partsDirectory);
        var partFiles = new List<string>();
        try
        {
            var loadedPartFiles = await GetRepositoryJsonAsync<List<string>>(partsIndexUrl);
            partFiles = loadedPartFiles ?? [];
            if (loadedPartFiles is null)
            {
                Trace.WriteLine($"The Gwire parts catalog parts index file '{partsIndexFile}' is empty. No parts will be available.");
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Trace.WriteLine($"The Gwire parts catalog parts index file '{partsIndexFile}' was not found. No parts will be available.");
        }

        var parts = await Task.WhenAll(partFiles.Select(async partFile =>
        {
            var partUrl = new Uri(partsDirectoryUrl, partFile).AbsoluteUri;
            try {
                var part = await GetRepositoryJsonAsync<CircuitPart>(partUrl, PartJsonOptions);
                if (part is null)
                {
                    Trace.WriteLine($"The Gwire parts catalog part '{partFile}' is empty. It will be skipped.");
                    return null;
                }

                part.IsFromRepo = true;
                return part;
            } catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Trace.WriteLine($"The Gwire parts catalog part '{partFile}' was not found. It will be skipped.");
            }
            return null;
        }));

        return [.. parts.OfType<CircuitPart>()];
    }
}
