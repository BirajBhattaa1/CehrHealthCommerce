using NepalMediHub.Models;
using NepalMediHub.Services;
using NepalMediHub.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace NepalMediHub.Controllers;

[Route("products")]
public class ProductsController : Controller
{
    private readonly IProductService _products;
    private readonly ICategoryService _categories;
    private readonly IRecommendationService _recommendations;

    public ProductsController(
        IProductService products,
        ICategoryService categories,
        IRecommendationService recommendations)
    {
        _products = products;
        _categories = categories;
        _recommendations = recommendations;
    }

    [HttpGet("")]
    public Task<IActionResult> Index(string? q, string? sort, int page = 1)
        => BuildList("All products", null, null, "Index", q, sort, page,
            "Browse our full catalogue of health products.");

    [HttpGet("medicine")]
    public Task<IActionResult> Medicines(string? q, string? sort, int page = 1)
        => BuildList("Medicines", ProductType.Medicine, null, "Medicines", q, sort, page,
            "Over-the-counter and prescription medicines.");

    [HttpGet("equipment")]
    public Task<IActionResult> Equipment(string? q, string? sort, int page = 1)
        => BuildList("Medical Equipment", ProductType.MedicalEquipment, null, "Equipment", q, sort, page,
            "Diagnostic and home-care medical devices.");

    [HttpGet("health")]
    public Task<IActionResult> HealthProducts(string? q, string? sort, int page = 1)
        => BuildList("Health Products", ProductType.HealthProduct, null, "HealthProducts", q, sort, page,
            "Wellness, hygiene and everyday health essentials.");

    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
    {
        var categories = await _categories.GetTopLevelWithChildrenAsync();
        ViewData["MetaDescription"] = "Browse product categories at Nepal MediHub — medicines, medical equipment and health products.";
        return View(categories);
    }

    [HttpGet("category/{slug}")]
    public async Task<IActionResult> Category(string slug, string? q, string? sort, int page = 1)
    {
        var category = await _categories.GetBySlugAsync(slug);
        if (category is null)
        {
            return NotFound();
        }

        var vm = await BuildListModel(category.Name, null, slug, "Category", q, sort, page, category.Description);
        vm.CategorySlug = slug;
        ViewData["MetaDescription"] = category.Description ?? $"Shop {category.Name} at Nepal MediHub.";
        return View("List", vm);
    }

    [HttpGet("details/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var product = await _products.GetBySlugAsync(slug);
        if (product is null)
        {
            return NotFound();
        }

        ViewData["MetaDescription"] = BuildMetaDescription(product);
        var related = await _recommendations.GetRelatedAsync(product, 4);

        return View(new ProductDetailsViewModel { Product = product, Related = related });
    }

    // ---- helpers -----------------------------------------------------------

    private async Task<IActionResult> BuildList(
        string title, ProductType? type, string? categorySlug, string listAction,
        string? q, string? sort, int page, string? subtitle)
    {
        var vm = await BuildListModel(title, type, categorySlug, listAction, q, sort, page, subtitle);
        return View("List", vm);
    }

    private async Task<ProductListViewModel> BuildListModel(
        string title, ProductType? type, string? categorySlug, string listAction,
        string? q, string? sort, int page, string? subtitle)
    {
        var results = await _products.SearchAsync(new ProductQuery
        {
            Search = q,
            Type = type,
            CategorySlug = categorySlug,
            Sort = sort,
            Page = page
        });

        var categories = await _categories.GetActiveCategoriesWithProductsAsync();

        return new ProductListViewModel
        {
            Title = title,
            Subtitle = subtitle,
            Query = q,
            Sort = sort,
            ActiveType = type,
            CategorySlug = categorySlug,
            ListAction = listAction,
            Results = results,
            Categories = categories
        };
    }

    private static string BuildMetaDescription(Product p)
    {
        if (!string.IsNullOrWhiteSpace(p.Description))
        {
            var d = p.Description!.Trim();
            return d.Length > 160 ? d[..157] + "..." : d;
        }
        return $"Buy {p.Name} online at Nepal MediHub.";
    }
}
