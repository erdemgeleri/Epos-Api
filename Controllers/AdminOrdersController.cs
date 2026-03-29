using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dtos;
using WebApplication1.Entities;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = nameof(UserRole.Admin))]
public sealed class AdminOrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminOrdersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminOrderListItemDto>>> List(CancellationToken cancellationToken)
    {
        var list = await _db.Orders.AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Business)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new AdminOrderListItemDto(
                o.Id,
                o.CustomerUserId,
                o.Customer.Email,
                o.BusinessId,
                o.Business.Name,
                o.Status.ToString(),
                o.TotalAmount,
                o.CreatedAt,
                o.CancelledAt))
            .ToListAsync(cancellationToken);
        return Ok(list);
    }
}
