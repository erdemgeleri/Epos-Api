using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dtos;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/catalog")]
public sealed class CatalogController : ControllerBase
{
    private readonly AppDbContext _db;

    public CatalogController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("businesses")]
    public async Task<ActionResult<IReadOnlyList<CatalogBusinessDto>>> Businesses(CancellationToken cancellationToken)
    {
        var list = await _db.Businesses.AsNoTracking()
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .Select(b => new CatalogBusinessDto(b.Id, b.Name, b.Description, b.City, b.IsActive))
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet("businesses/{businessId:guid}/products")]
    public async Task<ActionResult<IReadOnlyList<CatalogProductDto>>> Products(Guid businessId, CancellationToken cancellationToken)
    {
        var exists = await _db.Businesses.AnyAsync(b => b.Id == businessId && b.IsActive, cancellationToken);
        if (!exists)
        {
            return NotFound();
        }

        var list = await _db.Products.AsNoTracking()
            .Where(p => p.BusinessId == businessId && p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new CatalogProductDto(p.Id, p.BusinessId, p.Name, p.Description, p.Price, p.StockQuantity, p.IsActive))
            .ToListAsync(cancellationToken);
        return Ok(list);
    }
}
