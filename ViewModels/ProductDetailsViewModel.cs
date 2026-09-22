using CehrHealthCommerce.Models;

namespace CehrHealthCommerce.ViewModels;

public class ProductDetailsViewModel
{
    public Product Product { get; set; } = default!;
    public IReadOnlyList<Product> Related { get; set; } = new List<Product>();
}
