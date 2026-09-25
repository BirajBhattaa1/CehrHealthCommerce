using NepalMediHub.Common;
using NepalMediHub.Data;
using NepalMediHub.Models;
using Microsoft.EntityFrameworkCore;

namespace NepalMediHub.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _db;

    public ProductService(ApplicationDbContext db) => _db = db;

    public async Task<PagedResult<Product>> SearchAsync(ProductQuery query)
    {
        // LINQ-to-Entities queries are fully parameterised by EF Core (no string
        // concatenation), so user-supplied search terms cannot cause SQL injection.
        var q = _db.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive);

        if (query.Type is not null)
        {
            q = q.Where(p => p.ProductType == query.Type);
        }

        if (!string.IsNullOrWhiteSpace(query.CategorySlug))
        {
            q = q.Where(p => p.Category != null && p.Category.Slug == query.CategorySlug);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(p =>
                p.Name.Contains(term) ||
                (p.Description != null && p.Description.Contains(term)) ||
                (p.Manufacturer != null && p.Manufacturer.Contains(term)));
        }

        q = query.Sort switch
        {
            "price_asc" => q.OrderBy(p => p.Price),
            "price_desc" => q.OrderByDescending(p => p.Price),
            "name" => q.OrderBy(p => p.Name),
            _ => q.OrderByDescending(p => p.CreatedAt)
        };

        var page = query.Page < 1 ? 1 : query.Page;
        var size = query.PageSize < 1 ? 12 : query.PageSize;

        var total = await q.CountAsync();
        var items = await q
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PagedResult<Product>
        {
            Items = items,
            Page = page,
            PageSize = size,
            TotalCount = total
        };
    }

    public async Task<Product?> GetBySlugAsync(string slug)
        => await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

    public async Task<Product?> GetByIdAsync(int id)
        => await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProductId == id);

    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(int count = 8)
        => await _db.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();

// Admin operations

    public async Task<IReadOnlyList<Product>> GetAllForAdminAsync(string? search = null)
    {
        // Includes inactive products (admins manage everything).
        var q = _db.Products.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(p => p.Name.Contains(term) || p.SKU.Contains(term));
        }

        return await q.OrderByDescending(p => p.UpdatedAt).ToListAsync();
    }

    public async Task<(bool ok, string? error)> CreateAsync(Product product)
    {
        product.Name = product.Name.Trim();
        product.SKU = product.SKU.Trim();

        if (await _db.Products.AnyAsync(p => p.SKU == product.SKU))
            return (false, "A product with this SKU already exists.");

        product.Slug = await GenerateUniqueSlugAsync(product.Name, null);
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool ok, string? error)> UpdateAsync(Product product)
    {
        var existing = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
        if (existing is null) return (false, "Product not found.");

        var sku = product.SKU.Trim();
        if (await _db.Products.AnyAsync(p => p.SKU == sku && p.ProductId != product.ProductId))
            return (false, "A product with this SKU already exists.");

        existing.Name = product.Name.Trim();
        existing.SKU = sku;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.StockQuantity = product.StockQuantity;
        existing.ProductType = product.ProductType;
        existing.CategoryId = product.CategoryId;
        existing.Manufacturer = product.Manufacturer;
        existing.ExpiryDate = product.ExpiryDate;
        existing.PrescriptionRequired = product.PrescriptionRequired;
        existing.IsActive = product.IsActive;
        // Only overwrite the image when a new one was supplied.
        if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            existing.ImageUrl = product.ImageUrl;
        // Keep the slug in sync with the name (stable if the name is unchanged).
        existing.Slug = await GenerateUniqueSlugAsync(existing.Name, existing.ProductId);
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> SetActiveAsync(int id, bool active)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == id);
        if (product is null) return false;

        product.IsActive = active;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStockAsync(int id, int stockQuantity)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == id);
        if (product is null) return false;

        product.StockQuantity = stockQuantity < 0 ? 0 : stockQuantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, int? excludeId)
    {
        var baseSlug = SlugHelper.Slugify(name);
        if (string.IsNullOrEmpty(baseSlug)) baseSlug = "product";

        var slug = baseSlug;
        var i = 2;
        while (await _db.Products.AnyAsync(p => p.Slug == slug && (excludeId == null || p.ProductId != excludeId)))
            slug = $"{baseSlug}-{i++}";

        return slug;
    }
}
