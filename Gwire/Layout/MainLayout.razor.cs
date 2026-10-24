using Gwire.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Gwire.Layout;

public partial class MainLayout
{
    private bool canUseApp;
    private string? repositoryError;

    [Inject]
    private GwirePartsService GwireParts { get; set; } = default!;

    [Inject]
    private ILogger<MainLayout> Logger { get; set; } = default!;

    private async Task LoadRepositoryAsync()
    {
        repositoryError = null;
        StateHasChanged();
        try
        {
            await GwireParts.InitializeAsync();
            canUseApp = true;
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidDataException or IOException or JsonException or TaskCanceledException)
        {
            Logger.LogError(exception, "Repository data could not be loaded.");
            repositoryError = "Repository data could not be loaded. Check your connection and try again, or continue without the repository and import a scheme from a JSON file.";
        }
    }

    private void ContinueWithoutRepository() => canUseApp = true;
}
