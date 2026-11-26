using Microsoft.AspNetCore.Components;

namespace Gwire.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    protected override void OnInitialized() => Navigation.NavigateTo("scheme-editor", replace: true);
}
