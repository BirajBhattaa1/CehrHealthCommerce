using NepalMediHub.Models;

namespace NepalMediHub.Services;

/// <summary>Filter/sort/paging options for a product search.</summary>
public class ProductQuery
{
    public string? Search { get; set; }
    public ProductType? Type { get; set; }
    public string? CategorySlug { get; set; }

    /// <summary>One of: "price_asc", "price_desc", "name". Anything else = newest first.</summary>
    public string? Sort { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
