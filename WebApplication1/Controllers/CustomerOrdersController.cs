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
[Route("api/customer/orders")]
[Authorize(Roles = nameof(UserRole.Customer))]
public sealed class CustomerOrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILiveNotifier _live;

    public CustomerOrdersController(AppDbContext db, ILiveNotifier live)
    {
        _db = db;
        _live = live;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderResponseDto>>> MyOrders(CancellationToken cancellationToken)
    {
        var customerId = GetUserId();
        var orders = await _db.Orders.AsNoTracking()
            .Where(o => o.CustomerUserId == customerId)
            .Include(o => o.Business)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
        return Ok(orders.Select(MapOrder).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> Create([FromBody] CreateCustomerOrderRequest request, CancellationToken cancellationToken)
    {
        var customerId = GetUserId();
        if (request.Items.Count == 0)
        {
            return BadRequest(new { message = "Sepet boş olamaz." });
        }

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            return BadRequest(new { message = "Geçersiz ürün." });
        }

        if (products.Any(p => p.BusinessId != request.BusinessId || !p.IsActive))
        {
            return BadRequest(new { message = "Ürünler seçilen işletmeye ait değil veya pasif." });
        }

        decimal total = 0;
        var lineSnapshots = new List<(Product Product, int Qty)>();
        foreach (var line in request.Items)
        {
            var product = products.First(p => p.Id == line.ProductId);
            if (product.StockQuantity < line.Quantity)
            {
                return BadRequest(new { message = $"Yetersiz stok: {product.Name}" });
            }

            total += product.Price * line.Quantity;
            lineSnapshots.Add((product, line.Quantity));
        }

        foreach (var (product, qty) in lineSnapshots)
        {
            product.StockQuantity -= qty;
            product.UpdatedAt = DateTimeOffset.UtcNow;
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerUserId = customerId,
            BusinessId = request.BusinessId,
            Status = OrderStatus.Completed,
            TotalAmount = total,
            CreatedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
        };

        foreach (var (product, qty) in lineSnapshots)
        {
            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                ProductNameSnapshot = product.Name,
                Quantity = qty,
                UnitPrice = product.Price,
            });
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var fresh = await _db.Orders.AsNoTracking()
            .Include(o => o.Business)
            .Include(o => o.Items)
            .FirstAsync(o => o.Id == order.Id, cancellationToken);

        var dto = MapOrder(fresh);
        await _live.OrderCreatedAsync(order.BusinessId, order.CustomerUserId, dto, cancellationToken);

        foreach (var (product, _) in lineSnapshots)
        {
            await _live.ProductChangedAsync(request.BusinessId, new
            {
                action = "stockUpdated",
                businessId = request.BusinessId,
                productId = product.Id,
                stockQuantity = product.StockQuantity,
            }, cancellationToken);
        }

        return Ok(dto);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<OrderResponseDto>> Cancel(Guid id, [FromBody] CancelOrderRequest? request, CancellationToken cancellationToken)
    {
        var customerId = GetUserId();
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var order = await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.CustomerUserId == customerId, cancellationToken);

        if (order is null)
        {
            return NotFound();
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return BadRequest(new { message = "Sipariş zaten iptal edilmiş." });
        }

        if (order.Status == OrderStatus.Completed)
        {
            foreach (var item in order.Items)
            {
                item.Product.StockQuantity += item.Quantity;
                item.Product.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTimeOffset.UtcNow;
        order.CancelReason = request?.Reason?.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var fresh = await _db.Orders.AsNoTracking()
            .Include(o => o.Business)
            .Include(o => o.Items)
            .FirstAsync(o => o.Id == order.Id, cancellationToken);

        var dto = MapOrder(fresh);
        await _live.OrderCancelledAsync(order.BusinessId, order.CustomerUserId, dto, cancellationToken);

        foreach (var item in order.Items)
        {
            await _live.ProductChangedAsync(order.BusinessId, new
            {
                action = "stockUpdated",
                businessId = order.BusinessId,
                productId = item.ProductId,
                stockQuantity = item.Product.StockQuantity,
            }, cancellationToken);
        }

        return Ok(dto);
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

    private static OrderResponseDto MapOrder(Order o) =>
        new(
            o.Id,
            o.BusinessId,
            o.Business.Name,
            o.Status.ToString(),
            o.TotalAmount,
            o.CreatedAt,
            o.CancelledAt,
            o.CancelReason,
            o.Items.Select(i => new OrderItemResponseDto(i.ProductId, i.ProductNameSnapshot, i.Quantity, i.UnitPrice)).ToList());
}
