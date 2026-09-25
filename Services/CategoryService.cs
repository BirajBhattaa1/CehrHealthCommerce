using NepalMediHub.Common;
using NepalMediHub.Data;
using NepalMediHub.Models;
using Microsoft.EntityFrameworkCore;

namespace NepalMediHub.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _db;

    public CategoryService(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Category>> GetActiveAsync()
        => await _db.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task<IReadOnlyList<Category>> GetActiveCategoriesWithProductsAsync()
        => await _db.Categories
            .Where(c => c.IsActive && c.Products.Any(p => p.IsActive))
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task<IReadOnlyList<Category>> GetTopLevelWithChildrenAsync()
        => await _db.Categories
            .Where(c => c.IsActive && c.ParentCategoryId == null)
            .Include(c => c.Children.Where(ch => ch.IsActive))
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task<Category?> GetBySlugAsync(string slug)
        => await _db.Categories
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);

// Admin operations

    public async Task<IReadOnlyList<Category>> GetAllForAdminAsync()
        => await _db.Categories
            .Include(c => c.ParentCategory)
            .OrderBy(c => c.ParentCategoryId == null ? 0 : 1)
            .ThenBy(c => c.Name)
            .ToListAsync();

    public async Task<Category?> GetByIdAsync(int id)
        => await _db.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);

    public async Task<(bool ok, string? error)> CreateAsync(Category category)
    {
        category.Name = category.Name.Trim();
        category.Slug = await GenerateUniqueSlugAsync(category.Name, null);

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool ok, string? error)> UpdateAsync(Category category)
    {
        var existing = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId);
        if (existing is null) return (false, "Category not found.");

        if (category.ParentCategoryId == category.CategoryId)
            return (false, "A category cannot be its own parent.");

        existing.Name = category.Name.Trim();
        existing.Description = category.Description;
        existing.ParentCategoryId = category.ParentCategoryId;
        existing.IsActive = category.IsActive;
        existing.Slug = await GenerateUniqueSlugAsync(existing.Name, existing.CategoryId);

        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> SetActiveAsync(int id, bool active)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category is null) return false;

        category.IsActive = active;
        await _db.SaveChangesAsync();
        return true;
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, int? excludeId)
    {
        var baseSlug = SlugHelper.Slugify(name);
        if (string.IsNullOrEmpty(baseSlug)) baseSlug = "category";

        var slug = baseSlug;
        var i = 2;
        while (await _db.Categories.AnyAsync(c => c.Slug == slug && (excludeId == null || c.CategoryId != excludeId)))
            slug = $"{baseSlug}-{i++}";

        return slug;
    }
}
