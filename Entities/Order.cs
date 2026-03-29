namespace WebApplication1.Entities;

public sealed class Order
{
    public Guid Id { get; set; }
    public Guid CustomerUserId { get; set; }
    public Guid BusinessId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public string? CancelReason { get; set; }

    public User Customer { get; set; } = null!;
    public Business Business { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
