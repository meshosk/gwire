namespace Gwire.Models;

public sealed class ConfirmationRequest(string title, string text)
{
    private readonly TaskCompletionSource<ConfirmationResult> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public string Title { get; } = title;
    public string Text { get; } = text;

    internal Task<ConfirmationResult> Result => completion.Task;
    internal void Complete(ConfirmationResult result) => completion.TrySetResult(result);
}
