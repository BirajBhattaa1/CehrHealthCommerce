using System.ComponentModel.DataAnnotations;

namespace NepalMediHub.Models;

/// <summary>A line in a shopping cart. Subtotals are computed server-side from the live product price.</summary>
public class CartItem
{
    public int CartItemId { get; set; }

    public int CartId { get; set; }
    public Cart? Cart { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
