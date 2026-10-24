namespace Gwire.Services;

/// <summary>
/// Search metadata for a scheme without loading its contents.
/// </summary>
public sealed class GwireSchemeInfo
{
    public string File { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = [];
}
