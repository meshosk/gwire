namespace Gwire.Components.Displayers.SearchDialog;

public abstract class SearchItem
{
    /// <summary>
    /// The item's text representation for display in the search results list.
    /// </summary>
    public abstract string Text { get; }

    /// <summary>
    /// Calculates the item's relevance based on the selected filters.
    /// </summary>
    public abstract int Relevance(IEnumerable<string> tags, IEnumerable<string> textTerms);

}
