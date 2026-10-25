namespace Gwire.Components.Displayers.SearchDialog;

/// <summary>
/// Common settings and search state used by the dialog independently of the item type.
/// Reuse the same options and dialog instance across openings.
/// </summary>
public abstract class SearchOptions
{
    public string Title { get; init; } = "Search";
    public IReadOnlyList<string> AvailableTags { get; init; } = [];
    public List<string> Tags { get; init; } = [];
    public List<string> TextTerms { get; init; } = [];

    public abstract IReadOnlyList<SearchItem> TextItems { get; }

    internal abstract void Select(SearchItem item);
    internal abstract void Cancel();
}
