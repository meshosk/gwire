namespace Gwire.Components.SearchDialog;

/// <summary>
/// Supplies typed data and search behavior. Pass this instance to the dialog's Options
/// parameter and await Result. Cancellation completes Result with the default value (null for reference types).
/// </summary>
public sealed class SearchOptions<T> : SearchOptions
{
    private TaskCompletionSource<T?> completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public SearchOptions(Func<T, IEnumerable<string>, IEnumerable<string>, int> relevanceFunc, Func<T, string> textFunc)
    {
        ArgumentNullException.ThrowIfNull(relevanceFunc);
        ArgumentNullException.ThrowIfNull(textFunc);
        RelevanceFunc = relevanceFunc;
        TextFunc = textFunc;
    }

    public IReadOnlyList<T> Items { get; init; } = [];
    internal Func<T, IEnumerable<string>, IEnumerable<string>, int> RelevanceFunc { get; }
    internal Func<T, string> TextFunc { get; }

    public override IReadOnlyList<SearchItem> TextItems => Items
        .Select(item => (SearchItem)new SearchItem<T>(item, this))
        .ToList();

    public Task<T?> Result => completion.Task;

    internal override void Select(SearchItem item) =>
        Complete(((SearchItem<T>)item).Item);

    internal override void Cancel() => Complete(default);

    private void Complete(T? result)
    {
        var pending = completion;
        completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        pending.TrySetResult(result);
    }
}
