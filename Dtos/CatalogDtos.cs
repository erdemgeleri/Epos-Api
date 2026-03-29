namespace WebApplication1.Dtos;

public sealed record CatalogBusinessDto(
    Guid Id,
    string Name,
    string? Description,
    string? City,
    bool IsActive);

public sealed record CatalogProductDto(
    Guid Id,
    Guid BusinessId,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    bool IsActive);
