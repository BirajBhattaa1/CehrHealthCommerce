using System.ComponentModel.DataAnnotations;
using NepalMediHub.Models;

namespace NepalMediHub.ViewModels.Admin;

/// <summary>Create/edit form for a category in the admin panel.</summary>
public class CategoryFormViewModel
{
    public int CategoryId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Parent category")]
    public int? ParentCategoryId { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public Category ToCategory() => new()
    {
        CategoryId = CategoryId,
        Name = Name,
        Description = Description,
        ParentCategoryId = ParentCategoryId,
        IsActive = IsActive
    };

    public static CategoryFormViewModel FromCategory(Category c) => new()
    {
        CategoryId = c.CategoryId,
        Name = c.Name,
        Description = c.Description,
        ParentCategoryId = c.ParentCategoryId,
        IsActive = c.IsActive
    };
}
