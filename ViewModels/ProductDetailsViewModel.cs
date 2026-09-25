using NepalMediHub.Models;

namespace NepalMediHub.ViewModels;

public class ProductDetailsViewModel
{
    public Product Product { get; set; } = default!;
    public IReadOnlyList<Product> Related { get; set; } = new List<Product>();
}
