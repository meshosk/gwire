namespace Gwire.Components.SearchDialog;

public sealed class SearchItem<T> : SearchItem
{
    public SearchItem(T item, SearchOptions<T> options)
    {
        Item = item;
        Options = options;
    }

    public T Item { get; init; }
    private SearchOptions<T> Options { get; }

    public override int Relevance(IEnumerable<string> tags, IEnumerable<string> textTerms) =>
        Options.RelevanceFunc(Item, tags, textTerms);

    public override string Text => Options.TextFunc(Item);
}
