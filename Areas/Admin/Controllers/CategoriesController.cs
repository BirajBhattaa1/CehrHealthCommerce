using NepalMediHub.Services;
using NepalMediHub.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NepalMediHub.Areas.Admin.Controllers;

/// <summary>Admin category management: list, create, edit, activate/deactivate.</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categories;

    public CategoriesController(ICategoryService categories) => _categories = categories;

    public async Task<IActionResult> Index()
    {
        var categories = await _categories.GetAllForAdminAsync();
        return View(categories);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new CategoryFormViewModel { IsActive = true };
        await PopulateParentsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateParentsAsync(model);
            return View(model);
        }

        var (ok, error) = await _categories.CreateAsync(model.ToCategory());
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error ?? "Could not create the category.");
            await PopulateParentsAsync(model);
            return View(model);
        }

        TempData["Success"] = $"Category \"{model.Name}\" created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categories.GetByIdAsync(id);
        if (category is null) return NotFound();

        var model = CategoryFormViewModel.FromCategory(category);
        await PopulateParentsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateParentsAsync(model);
            return View(model);
        }

        var (ok, error) = await _categories.UpdateAsync(model.ToCategory());
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error ?? "Could not update the category.");
            await PopulateParentsAsync(model);
            return View(model);
        }

        TempData["Success"] = $"Category \"{model.Name}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var category = await _categories.GetByIdAsync(id);
        if (category is null) return NotFound();

        await _categories.SetActiveAsync(id, !category.IsActive);
        TempData["Success"] = category.IsActive
            ? $"\"{category.Name}\" deactivated."
            : $"\"{category.Name}\" activated.";
        return RedirectToAction(nameof(Index));
    }

    // ---------------------------------------------------------------------

    /// <summary>Populates the parent dropdown with top-level categories (keeps a single sub-level), excluding self.</summary>
    private async Task PopulateParentsAsync(CategoryFormViewModel model)
    {
        var all = await _categories.GetAllForAdminAsync();
        var parents = all
            .Where(c => c.ParentCategoryId == null && c.CategoryId != model.CategoryId)
            .OrderBy(c => c.Name)
            .ToList();

        ViewBag.Parents = new SelectList(parents, "CategoryId", "Name", model.ParentCategoryId);
    }
}
