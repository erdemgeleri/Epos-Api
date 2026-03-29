using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dtos;
using WebApplication1.Entities;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/business/orders")]
[Authorize(Roles = nameof(UserRole.Business))]
public sealed class BusinessOrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public BusinessOrdersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BusinessOrderListItemDto>>> List(CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }
        var list = await _db.Orders.AsNoTracking()
            .Where(o => o.BusinessId == businessId)
            .Include(o => o.Customer)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new BusinessOrderListItemDto(
                o.Id,
                o.CustomerUserId,
                o.Customer.Email,
                o.Status.ToString(),
                o.TotalAmount,
                o.CreatedAt,
                o.CancelledAt,
                o.Items.Count))
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    private bool TryGetBusinessId(out Guid businessId)
    {
        var raw = User.FindFirst("businessId")?.Value;
        return Guid.TryParse(raw, out businessId);
    }
}
