using Gwire.Models;
using Gwire.Services;
using Microsoft.AspNetCore.Components;

namespace Gwire.Components.Displayers;

public partial class MessageToasts : ComponentBase, IDisposable
{
    private readonly List<Message> messages = [];

    [Inject]
    private MessageService MessageService { get; set; } = default!;

    protected override void OnInitialized()
    {
        MessageService.MessageShown += Show;
    }

    private void Show(Message message)
    {
        messages.Add(message);
        StateHasChanged();
    }

    private void Dismiss(Message message)
    {
        messages.Remove(message);
    }

    private static string GetMessageClass(MessageType type) => type switch
    {
        MessageType.Danger => "danger",
        MessageType.Warning => "warning",
        MessageType.Success => "success",
        _ => "info"
    };

    public void Dispose()
    {
        MessageService.MessageShown -= Show;
    }
}
