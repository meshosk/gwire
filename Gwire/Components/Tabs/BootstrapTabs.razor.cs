using Microsoft.AspNetCore.Components;

namespace Gwire.Components.Tabs;

public partial class BootstrapTabs
{
    private readonly List<BootstrapTab> tabs = [];
    private BootstrapTab? activeTab;

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    internal void Register(BootstrapTab tab)
    {
        if (tabs.Contains(tab))
        {
            return;
        }

        tabs.Add(tab);
        activeTab ??= tabs.FirstOrDefault(item => !item.Disabled);
        StateHasChanged();
    }

    internal void Unregister(BootstrapTab tab)
    {
        if (!tabs.Remove(tab))
        {
            return;
        }

        if (ReferenceEquals(activeTab, tab))
        {
            activeTab = tabs.FirstOrDefault(item => !item.Disabled);
        }

        StateHasChanged();
    }

    internal void NotifyTabStateChanged()
    {
        StateHasChanged();
    }

    private bool IsActive(BootstrapTab tab) => ReferenceEquals(activeTab, tab);

    private string TabButtonClass(BootstrapTab tab) =>
        tab.Disabled ? "nav-link disabled" : IsActive(tab) ? "nav-link active" : "nav-link";

    private string TabPanelClass(BootstrapTab tab) =>
        IsActive(tab) ? "tab-pane fade show active" : "tab-pane fade";

    private async Task SelectTab(BootstrapTab tab)
    {
        if (tab.Disabled)
        {
            return;
        }

        activeTab = tab;
        await tab.OnClick.InvokeAsync();
    }
}
