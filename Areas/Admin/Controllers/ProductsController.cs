using NepalMediHub.Models;
using NepalMediHub.Services;
using NepalMediHub.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NepalMediHub.Areas.Admin.Controllers;

/// <summary>Admin product management: list, create, edit, image upload, stock update, activate/deactivate.</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxImageBytes = 2 * 1024 * 1024; // 2 MB

    private readonly IProductService _products;
    private readonly ICategoryService _categories;
    private readonly IWebHostEnvironment _env;

    public ProductsController(IProductService products, ICategoryService categories, IWebHostEnvironment env)
    {
        _products = products;
        _categories = categories;
        _env = env;
    }

    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Search = q;
        var products = await _products.GetAllForAdminAsync(q);
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ProductFormViewModel { IsActive = true };
        await PopulateDropdownsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        await ValidateAndApplyImageAsync(model);

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        var (ok, error) = await _products.CreateAsync(model.ToProduct());
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error ?? "Could not create the product.");
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        TempData["Success"] = $"Product \"{model.Name}\" created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product is null) return NotFound();

        var model = ProductFormViewModel.FromProduct(product);
        await PopulateDropdownsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductFormViewModel model)
    {
        await ValidateAndApplyImageAsync(model);

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        var (ok, error) = await _products.UpdateAsync(model.ToProduct());
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error ?? "Could not update the product.");
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        TempData["Success"] = $"Product \"{model.Name}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product is null) return NotFound();

        await _products.SetActiveAsync(id, !product.IsActive);
        TempData["Success"] = product.IsActive
            ? $"\"{product.Name}\" deactivated (hidden from store)."
            : $"\"{product.Name}\" activated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStock(int id, int stockQuantity)
    {
        var ok = await _products.UpdateStockAsync(id, stockQuantity);
        TempData[ok ? "Success" : "Error"] = ok ? "Stock updated." : "Could not update stock.";
        return RedirectToAction(nameof(Index));
    }

    // ---------------------------------------------------------------------

    private async Task PopulateDropdownsAsync(ProductFormViewModel model)
    {
        var categories = await _categories.GetAllForAdminAsync();
        ViewBag.Categories = new SelectList(categories, "CategoryId", "Name", model.CategoryId);
        ViewBag.ProductTypes = new SelectList(Enum.GetValues(typeof(ProductType)), model.ProductType);
    }

    /// <summary>If an image file was uploaded, validate it and save it to wwwroot/images/products.</summary>
    private async Task ValidateAndApplyImageAsync(ProductFormViewModel model)
    {
        var file = model.ImageFile;
        if (file is null || file.Length == 0) return;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext))
        {
            ModelState.AddModelError(nameof(model.ImageFile), "Only JPG, PNG, WEBP or GIF images are allowed.");
            return;
        }
        if (file.Length > MaxImageBytes)
        {
            ModelState.AddModelError(nameof(model.ImageFile), "Image must be 2 MB or smaller.");
            return;
        }

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var folder = Path.Combine(webRoot, "images", "products");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, fileName);
        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Store a web-relative path; overrides any typed ImageUrl.
        model.ImageUrl = $"/images/products/{fileName}";
    }
}
