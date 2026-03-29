using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Dtos;

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password);

public sealed record RegisterCustomerRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required, MaxLength(200)] string DisplayName);

public sealed record RegisterBusinessRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required, MaxLength(200)] string DisplayName,
    [Required, MaxLength(300)] string BusinessName,
    [MaxLength(2000)] string? BusinessDescription,
    [MaxLength(500)] string? AddressLine,
    [MaxLength(200)] string? City,
    [MaxLength(50)] string? Phone,
    [MaxLength(50)] string? TaxId);

public sealed record AuthResponse(string Token, UserSummaryDto User);

public sealed record UserSummaryDto(Guid Id, string Email, string DisplayName, string Role, Guid? BusinessId);

public sealed record UserMeDto(Guid Id, string Email, string DisplayName, string Role, Guid? BusinessId, bool IsActive);
