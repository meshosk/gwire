namespace Gwire.Models;

public sealed class Message
{
    public required string Title { get; init; }
    public required string Text { get; init; }
    public required MessageType Type { get; init; }
}
