using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Dtos;

public sealed record OrderLineRequest(
    [Required] Guid ProductId,
    [Range(1, 1000)] int Quantity);

public sealed record CreateCustomerOrderRequest(
    [Required] Guid BusinessId,
    [MinLength(1)] IReadOnlyList<OrderLineRequest> Items);

public sealed record OrderItemResponseDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);

public sealed record OrderResponseDto(
    Guid Id,
    Guid BusinessId,
    string BusinessName,
    string Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CancelledAt,
    string? CancelReason,
    IReadOnlyList<OrderItemResponseDto> Items);

public sealed record CancelOrderRequest([MaxLength(1000)] string? Reason);
