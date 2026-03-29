using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Dtos;

public sealed record BusinessProductDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed record CreateProductRequest(
    [Required, MaxLength(300)] string Name,
    [MaxLength(2000)] string? Description,
    [Range(0.01, double.MaxValue)] decimal Price,
    [Range(0, int.MaxValue)] int StockQuantity);

public sealed record UpdateProductRequest(
    [MaxLength(300)] string? Name,
    [MaxLength(2000)] string? Description,
    decimal? Price,
    int? StockQuantity,
    bool? IsActive);

public sealed record BusinessOrderListItemDto(
    Guid Id,
    Guid CustomerUserId,
    string CustomerEmail,
    string Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CancelledAt,
    int ItemCount);
