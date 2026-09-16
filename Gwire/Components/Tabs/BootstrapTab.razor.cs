using Microsoft.AspNetCore.Components;

namespace Gwire.Components.Tabs;

public partial class BootstrapTab : IDisposable
{
    private readonly string id = $"bootstrap-tab-{Guid.NewGuid():N}";
    private BootstrapTabs? owner;
    private bool? previousDisabled;

    [CascadingParameter]
    private BootstrapTabs? Owner
    {
        get => owner;
        set
        {
            if (ReferenceEquals(owner, value))
            {
                return;
            }

            owner?.Unregister(this);
            owner = value;
            owner?.Register(this);
        }
    }

    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public EventCallback OnClick { get; set; }

    internal string TabId => $"{id}-tab";
    internal string PanelId => $"{id}-panel";

    protected override void OnParametersSet()
    {
        if (owner is null)
        {
            throw new InvalidOperationException("BootstrapTab must be used inside a BootstrapTabs component.");
        }

        if (previousDisabled != Disabled)
        {
            previousDisabled = Disabled;
            owner.NotifyTabStateChanged();
        }
    }

    public void Dispose() => owner?.Unregister(this);
}
