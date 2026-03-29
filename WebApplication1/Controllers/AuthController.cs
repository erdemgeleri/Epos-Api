using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dtos;
using WebApplication1.Entities;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwt;
    private readonly AppPasswordHasher _passwordHasher;

    public AuthController(AppDbContext db, JwtTokenService jwt, AppPasswordHasher passwordHasher)
    {
        _db = db;
        _jwt = jwt;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("register-customer")]
    public async Task<ActionResult<AuthResponse>> RegisterCustomer([FromBody] RegisterCustomerRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            return Conflict(new { message = "Bu e-posta zaten kayıtlı." });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            Role = UserRole.Customer,
            BusinessId = null,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = string.Empty,
        };
        user.PasswordHash = _passwordHasher.Hash(user, request.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        var token = _jwt.CreateToken(user);
        return Ok(new AuthResponse(token, MapSummary(user)));
    }

    [HttpPost("register-business")]
    public async Task<ActionResult<AuthResponse>> RegisterBusiness([FromBody] RegisterBusinessRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            return Conflict(new { message = "Bu e-posta zaten kayıtlı." });
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = request.BusinessName.Trim(),
            Description = request.BusinessDescription?.Trim(),
            AddressLine = request.AddressLine?.Trim(),
            City = request.City?.Trim(),
            Phone = request.Phone?.Trim(),
            TaxId = request.TaxId?.Trim(),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _db.Businesses.Add(business);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            Role = UserRole.Business,
            BusinessId = business.Id,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = string.Empty,
        };
        user.PasswordHash = _passwordHasher.Hash(user, request.Password);
        _db.Users.Add(user);

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var token = _jwt.CreateToken(user);
        return Ok(new AuthResponse(token, MapSummary(user)));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user is null || !user.IsActive || !_passwordHasher.Verify(user, request.Password))
        {
            return Unauthorized(new { message = "E-posta veya şifre hatalı." });
        }

        var token = _jwt.CreateToken(user);
        return Ok(new AuthResponse(token, MapSummary(user)));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserMeDto>> Me(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(new UserMeDto(user.Id, user.Email, user.DisplayName, user.Role.ToString(), user.BusinessId, user.IsActive));
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var id))
        {
            throw new InvalidOperationException();
        }

        return id;
    }

    private static UserSummaryDto MapSummary(User user) =>
        new(user.Id, user.Email, user.DisplayName, user.Role.ToString(), user.BusinessId);
}
