using Gwire.Models;

namespace Gwire.Services;

public sealed class ConfirmationService
{
    private readonly SemaphoreSlim dialogLock = new(1, 1);

    public event Action<ConfirmationRequest>? ConfirmationShown;

    public async Task<ConfirmationResult> ShowAsync(string title, string text)
    {
        await dialogLock.WaitAsync();
        try
        {
            var show = ConfirmationShown
                ?? throw new InvalidOperationException("No confirmation modal is available.");
            var request = new ConfirmationRequest(title, text);
            show(request);
            return await request.Result;
        }
        finally
        {
            dialogLock.Release();
        }
    }
}
