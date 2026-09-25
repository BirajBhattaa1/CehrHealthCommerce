using NepalMediHub.Common;
using NepalMediHub.Models;

namespace NepalMediHub.Services;

public interface IProductService
{
    // --- Customer-facing reads ---
    Task<PagedResult<Product>> SearchAsync(ProductQuery query);
    Task<Product?> GetBySlugAsync(string slug);
    Task<Product?> GetByIdAsync(int id);
    Task<IReadOnlyList<Product>> GetFeaturedAsync(int count = 8);

    // --- Admin operations ---
    Task<IReadOnlyList<Product>> GetAllForAdminAsync(string? search = null);
    Task<(bool ok, string? error)> CreateAsync(Product product);
    Task<(bool ok, string? error)> UpdateAsync(Product product);
    Task<bool> SetActiveAsync(int id, bool active);
    Task<bool> UpdateStockAsync(int id, int stockQuantity);
}
