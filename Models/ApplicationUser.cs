using Microsoft.AspNetCore.Identity;

namespace NepalMediHub.Models;

/// <summary>
/// Application user extending ASP.NET Core Identity.
/// NID is the proposed unique citizen identifier (SIMULATED — not connected to any
/// Government of Nepal system). It is unique but is NOT used as the primary key.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    /// <summary>Simulated National ID. Unique per citizen. Stored locally only.</summary>
    public string NID { get; set; } = string.Empty;

    public string? PatientHealthId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public Cart? Cart { get; set; }
}
