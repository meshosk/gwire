namespace Gwire.Components.SearchDialog;

/// <summary>
/// Common settings and search state used by the dialog independently of the item type.
/// Create a new dialog instance for each opening and pass its Options parameter once.
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
