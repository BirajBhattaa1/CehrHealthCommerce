using System.ComponentModel.DataAnnotations;
using NepalMediHub.Models;
using Microsoft.AspNetCore.Http;

namespace NepalMediHub.ViewModels.Admin;

/// <summary>Create/edit form for a product in the admin panel.</summary>
public class ProductFormViewModel
{
    public int ProductId { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(60)]
    [Display(Name = "SKU")]
    public string SKU { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(0, 9999999, ErrorMessage = "Price must be zero or more.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock must be zero or more.")]
    [Display(Name = "Stock quantity")]
    public int StockQuantity { get; set; }

    [Display(Name = "Product type")]
    public ProductType ProductType { get; set; }

    [Required(ErrorMessage = "Please choose a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [StringLength(150)]
    public string? Manufacturer { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Expiry date")]
    public DateTime? ExpiryDate { get; set; }

    [Display(Name = "Prescription required")]
    public bool PrescriptionRequired { get; set; }

    [Display(Name = "Active (visible in store)")]
    public bool IsActive { get; set; } = true;

    /// <summary>Existing image path (kept if no new file is uploaded).</summary>
    [StringLength(300)]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }

    /// <summary>Optional uploaded image file. Saved under wwwroot/images/products.</summary>
    [Display(Name = "Upload image")]
    public IFormFile? ImageFile { get; set; }

    public Product ToProduct() => new()
    {
        ProductId = ProductId,
        Name = Name,
        SKU = SKU,
        Description = Description,
        Price = Price,
        StockQuantity = StockQuantity,
        ProductType = ProductType,
        CategoryId = CategoryId,
        Manufacturer = Manufacturer,
        ExpiryDate = ExpiryDate,
        PrescriptionRequired = PrescriptionRequired,
        IsActive = IsActive,
        ImageUrl = ImageUrl
    };

    public static ProductFormViewModel FromProduct(Product p) => new()
    {
        ProductId = p.ProductId,
        Name = p.Name,
        SKU = p.SKU,
        Description = p.Description,
        Price = p.Price,
        StockQuantity = p.StockQuantity,
        ProductType = p.ProductType,
        CategoryId = p.CategoryId,
        Manufacturer = p.Manufacturer,
        ExpiryDate = p.ExpiryDate,
        PrescriptionRequired = p.PrescriptionRequired,
        IsActive = p.IsActive,
        ImageUrl = p.ImageUrl
    };
}
