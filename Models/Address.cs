using System.ComponentModel.DataAnnotations;

namespace NepalMediHub.Models;

/// <summary>A saved delivery address for a user (Nepal-focused fields).</summary>
public class Address
{
    public int AddressId { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [StringLength(60)]
    public string? Label { get; set; }

    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string AddressLine { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Province { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string District { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Municipality { get; set; } = string.Empty;

    [StringLength(10)]
    public string? Ward { get; set; }

    [StringLength(10)]
    public string? PostalCode { get; set; }

    public bool IsDefault { get; set; }
}
