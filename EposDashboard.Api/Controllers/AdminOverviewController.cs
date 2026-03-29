using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dtos;
using WebApplication1.Entities;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/admin/overview")]
[Authorize(Roles = nameof(UserRole.Admin))]
public sealed class AdminOverviewController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminOverviewController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<AdminOverviewDto>> Get(CancellationToken cancellationToken)
    {
        var users = await _db.Users.CountAsync(cancellationToken);
        var businesses = await _db.Businesses.CountAsync(cancellationToken);
        var products = await _db.Products.CountAsync(cancellationToken);
        var orders = await _db.Orders.CountAsync(cancellationToken);
        var openOrders = await _db.Orders.CountAsync(o => o.Status != OrderStatus.Cancelled, cancellationToken);

        return Ok(new AdminOverviewDto(users, businesses, products, orders, openOrders));
    }
}
