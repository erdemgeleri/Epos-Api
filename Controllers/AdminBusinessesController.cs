using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dtos;
using WebApplication1.Entities;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/admin/businesses")]
[Authorize(Roles = nameof(UserRole.Admin))]
public sealed class AdminBusinessesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILiveNotifier _live;

    public AdminBusinessesController(AppDbContext db, ILiveNotifier live)
    {
        _db = db;
        _live = live;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminBusinessDto>>> List(CancellationToken cancellationToken)
    {
        var list = await _db.Businesses.AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new AdminBusinessDto(b.Id, b.Name, b.Description, b.AddressLine, b.City, b.Phone, b.TaxId, b.IsActive, b.CreatedAt))
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminBusinessDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var b = await _db.Businesses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (b is null)
        {
            return NotFound();
        }

        return Ok(new AdminBusinessDto(b.Id, b.Name, b.Description, b.AddressLine, b.City, b.Phone, b.TaxId, b.IsActive, b.CreatedAt));
    }

    [HttpPost]
    public async Task<ActionResult<AdminBusinessDto>> Create([FromBody] AdminCreateBusinessRequest request, CancellationToken cancellationToken)
    {
        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            AddressLine = request.AddressLine?.Trim(),
            City = request.City?.Trim(),
            Phone = request.Phone?.Trim(),
            TaxId = request.TaxId?.Trim(),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _db.Businesses.Add(business);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new AdminBusinessDto(business.Id, business.Name, business.Description, business.AddressLine, business.City, business.Phone, business.TaxId, business.IsActive, business.CreatedAt);
        await _live.BusinessChangedAsync(business.Id, new { action = "created", business = dto }, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = business.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminBusinessDto>> Update(Guid id, [FromBody] AdminUpdateBusinessRequest request, CancellationToken cancellationToken)
    {
        var business = await _db.Businesses.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (business is null)
        {
            return NotFound();
        }

        if (request.Name is not null)
        {
            business.Name = request.Name.Trim();
        }

        business.Description = request.Description?.Trim() ?? business.Description;
        business.AddressLine = request.AddressLine?.Trim() ?? business.AddressLine;
        business.City = request.City?.Trim() ?? business.City;
        business.Phone = request.Phone?.Trim() ?? business.Phone;
        business.TaxId = request.TaxId?.Trim() ?? business.TaxId;
        if (request.IsActive.HasValue)
        {
            business.IsActive = request.IsActive.Value;
        }

        business.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new AdminBusinessDto(business.Id, business.Name, business.Description, business.AddressLine, business.City, business.Phone, business.TaxId, business.IsActive, business.CreatedAt);
        await _live.BusinessChangedAsync(business.Id, new { action = "updated", business = dto }, cancellationToken);
        return Ok(dto);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> SoftDelete(Guid id, CancellationToken cancellationToken)
    {
        var business = await _db.Businesses.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (business is null)
        {
            return NotFound();
        }

        business.IsActive = false;
        business.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        await _live.BusinessChangedAsync(business.Id, new { action = "deactivated", businessId = business.Id }, cancellationToken);
        return NoContent();
    }
}
