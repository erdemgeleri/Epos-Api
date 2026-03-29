using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dtos;
using WebApplication1.Entities;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = nameof(UserRole.Admin))]
public sealed class AdminUsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AppPasswordHasher _passwordHasher;
    private readonly ILiveNotifier _live;

    public AdminUsersController(AppDbContext db, AppPasswordHasher passwordHasher, ILiveNotifier live)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _live = live;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> List([FromQuery] bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = _db.Users.AsNoTracking().AsQueryable();
        if (activeOnly)
        {
            query = query.Where(u => u.IsActive);
        }

        var list = await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserDto(u.Id, u.Email, u.DisplayName, u.Role.ToString(), u.BusinessId, u.IsActive, u.CreatedAt))
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminUserDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (u is null)
        {
            return NotFound();
        }

        return Ok(new AdminUserDto(u.Id, u.Email, u.DisplayName, u.Role.ToString(), u.BusinessId, u.IsActive, u.CreatedAt));
    }

    [HttpPost]
    public async Task<ActionResult<AdminUserDto>> Create([FromBody] AdminCreateUserRequest request, CancellationToken cancellationToken)
    {
        if (request.Role == UserRole.Business && !request.BusinessId.HasValue)
        {
            return BadRequest(new { message = "İşletme kullanıcısı için BusinessId gerekli." });
        }

        if (request.Role != UserRole.Business && request.BusinessId.HasValue)
        {
            return BadRequest(new { message = "Bu rol için BusinessId kullanılamaz." });
        }

        if (request.BusinessId.HasValue)
        {
            var businessExists = await _db.Businesses.AnyAsync(b => b.Id == request.BusinessId.Value, cancellationToken);
            if (!businessExists)
            {
                return BadRequest(new { message = "İşletme bulunamadı." });
            }
        }

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
            Role = request.Role,
            BusinessId = request.BusinessId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = string.Empty,
        };
        user.PasswordHash = _passwordHasher.Hash(user, request.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        await _live.UserChangedAsync(new { action = "created", userId = user.Id, email = user.Email, role = user.Role.ToString() }, cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = user.Id },
            new AdminUserDto(user.Id, user.Email, user.DisplayName, user.Role.ToString(), user.BusinessId, user.IsActive, user.CreatedAt));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminUserDto>> Update(Guid id, [FromBody] AdminUpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        if (request.BusinessId.HasValue && user.Role != UserRole.Business)
        {
            return BadRequest(new { message = "BusinessId yalnızca işletme kullanıcıları için." });
        }

        if (request.BusinessId.HasValue)
        {
            var ok = await _db.Businesses.AnyAsync(b => b.Id == request.BusinessId.Value, cancellationToken);
            if (!ok)
            {
                return BadRequest(new { message = "İşletme bulunamadı." });
            }

            user.BusinessId = request.BusinessId;
        }

        if (request.DisplayName is not null)
        {
            user.DisplayName = request.DisplayName.Trim();
        }

        if (request.IsActive.HasValue)
        {
            if (user.Role == UserRole.Admin && request.IsActive == false)
            {
                var otherActiveAdmins = await _db.Users.CountAsync(
                    u => u.Role == UserRole.Admin && u.IsActive && u.Id != user.Id,
                    cancellationToken);
                if (otherActiveAdmins == 0)
                {
                    return BadRequest(new { message = "Son aktif admin pasifleştirilemez." });
                }
            }

            user.IsActive = request.IsActive.Value;
        }

        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        await _live.UserChangedAsync(new { action = "updated", userId = user.Id }, cancellationToken);

        return Ok(new AdminUserDto(user.Id, user.Email, user.DisplayName, user.Role.ToString(), user.BusinessId, user.IsActive, user.CreatedAt));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> SoftDelete(Guid id, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        if (user.Role == UserRole.Admin)
        {
            var otherActiveAdmins = await _db.Users.CountAsync(
                u => u.Role == UserRole.Admin && u.IsActive && u.Id != user.Id,
                cancellationToken);
            if (otherActiveAdmins == 0)
            {
                return BadRequest(new { message = "Son aktif admin silinemez." });
            }
        }

        user.IsActive = false;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        await _live.UserChangedAsync(new { action = "deleted", userId = user.Id }, cancellationToken);
        return NoContent();
    }
}
