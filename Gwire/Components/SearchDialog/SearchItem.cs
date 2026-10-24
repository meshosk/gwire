namespace Gwire.Components.SearchDialog;

public abstract class SearchItem
{
    /// <summary>
    /// Calculates the item's relevance based on the selected filters.
    /// </summary>
    public abstract int Relevance(IEnumerable<string> tags, IEnumerable<string> textTerms);

    /// <summary>
    /// The item's text representation for display in the search results list.
    /// </summary>
    public abstract string Text { get; }
}
