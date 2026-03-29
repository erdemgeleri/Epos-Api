using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dtos;
using WebApplication1.Entities;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/business/products")]
[Authorize(Roles = nameof(UserRole.Business))]
public sealed class BusinessProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILiveNotifier _live;

    public BusinessProductsController(AppDbContext db, ILiveNotifier live)
    {
        _db = db;
        _live = live;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BusinessProductDto>>> List(CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }
        var list = await _db.Products.AsNoTracking()
            .Where(p => p.BusinessId == businessId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new BusinessProductDto(p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.IsActive, p.CreatedAt))
            .ToListAsync(cancellationToken);
        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<BusinessProductDto>> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }
        var product = new Product
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new BusinessProductDto(product.Id, product.Name, product.Description, product.Price, product.StockQuantity, product.IsActive, product.CreatedAt);
        await _live.ProductChangedAsync(businessId, new { action = "created", businessId, product = dto }, cancellationToken);
        return CreatedAtAction(nameof(List), dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BusinessProductDto>> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.BusinessId == businessId, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        if (request.Name is not null)
        {
            product.Name = request.Name.Trim();
        }

        if (request.Description is not null)
        {
            product.Description = request.Description.Trim();
        }

        if (request.Price.HasValue)
        {
            product.Price = request.Price.Value;
        }

        if (request.StockQuantity.HasValue)
        {
            product.StockQuantity = request.StockQuantity.Value;
        }

        if (request.IsActive.HasValue)
        {
            product.IsActive = request.IsActive.Value;
        }

        product.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new BusinessProductDto(product.Id, product.Name, product.Description, product.Price, product.StockQuantity, product.IsActive, product.CreatedAt);
        await _live.ProductChangedAsync(businessId, new { action = "updated", businessId, product = dto }, cancellationToken);
        return Ok(dto);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> SoftDelete(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetBusinessId(out var businessId))
        {
            return Forbid();
        }
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.BusinessId == businessId, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        product.IsActive = false;
        product.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        await _live.ProductChangedAsync(businessId, new { action = "deactivated", businessId, productId = product.Id }, cancellationToken);
        return NoContent();
    }

    private bool TryGetBusinessId(out Guid businessId)
    {
        var raw = User.FindFirst("businessId")?.Value;
        return Guid.TryParse(raw, out businessId);
    }
}
