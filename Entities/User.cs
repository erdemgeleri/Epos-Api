namespace WebApplication1.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? BusinessId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public Business? Business { get; set; }
    public ICollection<Order> OrdersAsCustomer { get; set; } = new List<Order>();
    public ICollection<ChatMessage> ChatConversationsAsCustomer { get; set; } = new List<ChatMessage>();
    public ICollection<ChatMessage> SentChatMessages { get; set; } = new List<ChatMessage>();
}
