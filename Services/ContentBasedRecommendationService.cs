using NepalMediHub.Data;
using NepalMediHub.Models;
using Microsoft.EntityFrameworkCore;

namespace NepalMediHub.Services;

/// <summary>
/// Simple content-based recommender: suggests products that share the same
/// category as the current product, and falls back to the same product type
/// when there are not enough category matches. No external ML/service required.
/// </summary>
public class ContentBasedRecommendationService : IRecommendationService
{
    private readonly ApplicationDbContext _db;

    public ContentBasedRecommendationService(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Product>> GetRelatedAsync(Product product, int count = 4)
    {
        // 1. Same category (strongest content signal).
        var sameCategory = await _db.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive
                        && p.ProductId != product.ProductId
                        && p.CategoryId == product.CategoryId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();

        if (sameCategory.Count >= count)
        {
            return sameCategory;
        }

        // 2. Fall back to the same product type to fill remaining slots.
        var excludeIds = sameCategory.Select(p => p.ProductId).Append(product.ProductId).ToList();
        var remaining = count - sameCategory.Count;

        var sameType = await _db.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive
                        && p.ProductType == product.ProductType
                        && !excludeIds.Contains(p.ProductId))
            .OrderByDescending(p => p.CreatedAt)
            .Take(remaining)
            .ToListAsync();

        return sameCategory.Concat(sameType).ToList();
    }
}
