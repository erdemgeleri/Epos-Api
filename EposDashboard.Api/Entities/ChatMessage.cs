namespace WebApplication1.Entities;

public sealed class ChatMessage
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Guid CustomerUserId { get; set; }
    public Guid SenderUserId { get; set; }
    public Guid? ReplyToMessageId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset SentAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }

    public Business Business { get; set; } = null!;
    public User Customer { get; set; } = null!;
    public User Sender { get; set; } = null!;
    public ChatMessage? ReplyTo { get; set; }
}
