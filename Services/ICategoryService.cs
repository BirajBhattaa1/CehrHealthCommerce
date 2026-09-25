using NepalMediHub.Models;

namespace NepalMediHub.Services;

public interface ICategoryService
{
    // --- Customer-facing reads ---
    Task<IReadOnlyList<Category>> GetActiveAsync();
    Task<IReadOnlyList<Category>> GetActiveCategoriesWithProductsAsync();
    Task<IReadOnlyList<Category>> GetTopLevelWithChildrenAsync();
    Task<Category?> GetBySlugAsync(string slug);

    // --- Admin operations ---
    Task<IReadOnlyList<Category>> GetAllForAdminAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<(bool ok, string? error)> CreateAsync(Category category);
    Task<(bool ok, string? error)> UpdateAsync(Category category);
    Task<bool> SetActiveAsync(int id, bool active);
}
