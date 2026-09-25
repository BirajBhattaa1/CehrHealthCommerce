using NepalMediHub.Models;
using NepalMediHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace NepalMediHub.ViewComponents;

/// <summary>
/// Reusable recommendation widget. Given a product id, it uses the content-based
/// <see cref="IRecommendationService"/> to render a "You may also like" strip.
/// Used on the product details page and as a cross-sell on the cart page.
/// </summary>
public class RelatedProductsViewComponent : ViewComponent
{
    private readonly IProductService _products;
    private readonly IRecommendationService _recommendations;

    public RelatedProductsViewComponent(IProductService products, IRecommendationService recommendations)
    {
        _products = products;
        _recommendations = recommendations;
    }

    public async Task<IViewComponentResult> InvokeAsync(int productId, int count = 4, string? heading = null)
    {
        var product = await _products.GetByIdAsync(productId);
        if (product is null)
            return View(new RelatedProductsViewModel());

        var related = await _recommendations.GetRelatedAsync(product, count);
        return View(new RelatedProductsViewModel
        {
            Heading = heading ?? "You may also like",
            Products = related
        });
    }
}

/// <summary>View model for the <see cref="RelatedProductsViewComponent"/>.</summary>
public class RelatedProductsViewModel
{
    public string Heading { get; set; } = "You may also like";
    public IReadOnlyList<Product> Products { get; set; } = new List<Product>();
}
