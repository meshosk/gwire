using Gwire.Models;

namespace Gwire.Services;

public sealed class MessageService
{
    public event Action<Message>? MessageShown;

    public void Show(string title, string text, MessageType type)
    {
        MessageShown?.Invoke(new Message
        {
            Title = title,
            Text = text,
            Type = type
        });
    }
}
