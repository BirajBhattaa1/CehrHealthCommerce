using NepalMediHub.Common;
using NepalMediHub.Models;

namespace NepalMediHub.ViewModels;

public class ProductListViewModel
{
    public string Title { get; set; } = "Products";
    public string? Subtitle { get; set; }
    public string? Query { get; set; }
    public string? Sort { get; set; }
    public ProductType? ActiveType { get; set; }
    public string? CategorySlug { get; set; }

    /// <summary>The controller action used to build sort/pagination links (Index, Medicines, ...).</summary>
    public string ListAction { get; set; } = "Index";

    public PagedResult<Product> Results { get; set; } = new();
    public IReadOnlyList<Category> Categories { get; set; } = new List<Category>();

    /// <summary>Route values to preserve across pagination links.</summary>
    public Dictionary<string, string> RouteValues()
    {
        var values = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(Query)) values["q"] = Query;
        if (!string.IsNullOrWhiteSpace(Sort)) values["sort"] = Sort;
        if (!string.IsNullOrWhiteSpace(CategorySlug)) values["slug"] = CategorySlug;
        return values;
    }
}
