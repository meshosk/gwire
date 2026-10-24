using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Gwire.Components.SearchDialog;

public partial class SearchDialog : ComponentBase, IAsyncDisposable
{
    private readonly string titleId = $"search-title-{Guid.NewGuid():N}";
    private readonly string inputId = $"search-input-{Guid.NewGuid():N}";
    private readonly string suggestionsId = $"search-tags-{Guid.NewGuid():N}";
    private ElementReference modalElement;
    private IJSObjectReference? module;
    private DotNetObjectReference<SearchDialog>? receiver;
    private IReadOnlyList<SearchItem> results = [];
    private SearchItem? selectedMatch;
    private string draftValue = string.Empty;
    private bool showTagSuggestions;
    private int activeTagSuggestionIndex;
    private bool responding;
    private bool showRequested;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Parameter, EditorRequired]
    public SearchOptions Options { get; set; } = default!;

    [Parameter]
    public bool Visible { get; set; }

    [Parameter]
    public EventCallback<bool> VisibleChanged { get; set; }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var previousVisible = Visible;
        await base.SetParametersAsync(parameters);
        if (previousVisible == Visible)
        {
            return;
        }

        showRequested = Visible;
        if (Visible)
        {
            responding = false;
            selectedMatch = null;
            UpdateResults();
        }
        else if (module is not null)
        {
            await module.InvokeVoidAsync("hide", modalElement, 0);
        }
    }

    private IReadOnlyList<string> TagSuggestions => draftValue.StartsWith('#')
        ? Options.AvailableTags
            .Where(tag => !Options.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            .Where(tag => tag.Contains(draftValue[1..].Trim(), StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToList()
        : [];

    protected override void OnInitialized() => UpdateResults();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!showRequested)
        {
            return;
        }

        showRequested = false;
        try
        {
            module ??= await JS.InvokeAsync<IJSObjectReference>("import", "./Components/SearchDialog/SearchDialog.razor.js");
            receiver ??= DotNetObjectReference.Create(this);
            await module.InvokeVoidAsync("show", modalElement, receiver);
        }
        catch
        {
            Options.Cancel();
            throw;
        }
    }

    private void UpdateResults()
    {
        var hasFilters = Options.Tags.Count > 0 || Options.TextTerms.Count > 0;
        results = Options.TextItems
            .OrderByDescending(item => hasFilters ? item.Relevance(Options.Tags, Options.TextTerms) : 0)
            .Take(5)
            .ToList();
            StateHasChanged();
    }

    private void ResetTagSuggestions()
    {
        activeTagSuggestionIndex = 0;
        showTagSuggestions = draftValue.StartsWith('#');
    }

    private void CloseTagSuggestions() => showTagSuggestions = false;

    private void HandleKeyDown(KeyboardEventArgs args)
    {
        if (responding)
        {
            return;
        }

        if (draftValue.StartsWith('#'))
        {
            var suggestions = TagSuggestions;
            if (args.Key is not ("ArrowDown" or "ArrowUp" or "Enter") || suggestions.Count == 0)
            {
                return;
            }

            if (!showTagSuggestions)
            {
                ResetTagSuggestions();
                return;
            }

            switch (args.Key)
            {
                case "ArrowDown":
                    activeTagSuggestionIndex = (activeTagSuggestionIndex + 1) % suggestions.Count;
                    break;
                case "ArrowUp":
                    activeTagSuggestionIndex = (activeTagSuggestionIndex + suggestions.Count - 1) % suggestions.Count;
                    break;
                case "Enter":
                    AddTag(suggestions[Math.Clamp(activeTagSuggestionIndex, 0, suggestions.Count - 1)]);
                    break;
            }
        }
        else if (args.Key == "Enter")
        {
            var term = draftValue.Trim();
            if (term.Length == 0)
            {
                return;
            }

            if (!Options.TextTerms.Contains(term, StringComparer.OrdinalIgnoreCase))
            {
                Options.TextTerms.Add(term);
            }
            ClearInput();
        }
    }

    private void AddTag(string tag)
    {
        if (responding || !TagSuggestions.Contains(tag, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        Options.Tags.Add(tag);
        ClearInput();
    }

    private void ClearInput()
    {
        draftValue = string.Empty;
        CloseTagSuggestions();
        activeTagSuggestionIndex = 0;
        UpdateResults();
    }

    private void ClearFilters()
    {
        Options.Tags.Clear();
        Options.TextTerms.Clear();
        ClearInput();
    }

    private void RemoveTag(string tag)
    {
        Options.Tags.Remove(tag);
        activeTagSuggestionIndex = 0;
        UpdateResults();
    }

    private void RemoveTextTerm(string term)
    {
        Options.TextTerms.Remove(term);
        UpdateResults();
    }

    private async Task SelectAsync(SearchItem match)
    {
        if (responding || module is null)
        {
            return;
        }

        selectedMatch = match;
        responding = true;
        await module.InvokeVoidAsync("hide", modalElement, 1);
    }

    private async Task CancelAsync()
    {
        if (!responding && module is not null)
        {
            responding = true;
            await module.InvokeVoidAsync("hide", modalElement, 0);
        }
    }

    [JSInvokable]
    public async Task Complete(int result)
    {
        await VisibleChanged.InvokeAsync(false);
        if (result == 1 && selectedMatch is { } match)
        {
            Options.Select(match);
        }
        else
        {
            Options.Cancel();
        }
        selectedMatch = null;
        responding = true;
    }

    public async ValueTask DisposeAsync()
    {
        Options.Cancel();
        if (module is not null)
        {
            await module.InvokeVoidAsync("dispose", modalElement);
            await module.DisposeAsync();
        }
        receiver?.Dispose();
    }
}
