using Gwire.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Gwire.Layout;

public partial class MainLayout
{
    private bool canUseApp;
    private bool showRepositoryLoading = true;
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
            showRepositoryLoading = false;
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidDataException or IOException or JsonException or TaskCanceledException)
        {
            Logger.LogError(exception, "Repository data could not be loaded.");
            repositoryError = canUseApp
                ? "Repository data could not be refreshed. Check your connection and try again, or continue using the previously loaded data."
                : "Repository data could not be loaded. Check your connection and try again, or continue without the repository and import a scheme from a JSON file.";
        }
    }

    private void ReloadRepository()
    {
        if (showRepositoryLoading)
        {
            return;
        }

        repositoryError = null;
        showRepositoryLoading = true;
    }

    private void ContinueWithoutRepository()
    {
        canUseApp = true;
        showRepositoryLoading = false;
    }
}
