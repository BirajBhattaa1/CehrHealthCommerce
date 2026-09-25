using System.ComponentModel.DataAnnotations;

namespace NepalMediHub.Models;

/// <summary>
/// Product category. Supports a single level of sub-categories via <see cref="ParentCategoryId"/>.
/// </summary>
public class Category
{
    public int CategoryId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>SEO-friendly unique slug, e.g. "pain-relief".</summary>
    [Required, StringLength(120)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
