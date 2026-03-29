using System.ComponentModel.DataAnnotations;
using WebApplication1.Entities;

namespace WebApplication1.Dtos;

public sealed record AdminCreateUserRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required, MaxLength(200)] string DisplayName,
    [Required] UserRole Role,
    Guid? BusinessId);

public sealed record AdminUpdateUserRequest(
    [MaxLength(200)] string? DisplayName,
    bool? IsActive,
    Guid? BusinessId);

public sealed record AdminUserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string Role,
    Guid? BusinessId,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed record AdminBusinessDto(
    Guid Id,
    string Name,
    string? Description,
    string? AddressLine,
    string? City,
    string? Phone,
    string? TaxId,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed record AdminCreateBusinessRequest(
    [Required, MaxLength(300)] string Name,
    [MaxLength(2000)] string? Description,
    [MaxLength(500)] string? AddressLine,
    [MaxLength(200)] string? City,
    [MaxLength(50)] string? Phone,
    [MaxLength(50)] string? TaxId);

public sealed record AdminUpdateBusinessRequest(
    [MaxLength(300)] string? Name,
    [MaxLength(2000)] string? Description,
    [MaxLength(500)] string? AddressLine,
    [MaxLength(200)] string? City,
    [MaxLength(50)] string? Phone,
    [MaxLength(50)] string? TaxId,
    bool? IsActive);

public sealed record AdminOverviewDto(
    int UserCount,
    int BusinessCount,
    int ProductCount,
    int OrderCount,
    int OpenOrderCount);

public sealed record AdminOrderListItemDto(
    Guid Id,
    Guid CustomerUserId,
    string CustomerEmail,
    Guid BusinessId,
    string BusinessName,
    string Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CancelledAt);
