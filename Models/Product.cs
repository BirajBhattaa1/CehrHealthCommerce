using System.ComponentModel.DataAnnotations;

namespace NepalMediHub.Models;

/// <summary>A sellable health product (medicine, medical equipment or general health product).</summary>
public class Product
{
    public int ProductId { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    /// <summary>SEO-friendly unique slug, e.g. "digital-blood-pressure-monitor".</summary>
    [Required, StringLength(180)]
    public string Slug { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string SKU { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(0, 9999999)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [StringLength(300)]
    public string? ImageUrl { get; set; }

    public ProductType ProductType { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [StringLength(150)]
    public string? Manufacturer { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool PrescriptionRequired { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
