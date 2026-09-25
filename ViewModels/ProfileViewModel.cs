using System.ComponentModel.DataAnnotations;

namespace NepalMediHub.ViewModels;

public class ProfileViewModel
{
    [Display(Name = "National ID (NID)")]
    public string NID { get; set; } = string.Empty;

    [Required, StringLength(120), Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone, Display(Name = "Phone number")]
    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; }

    public IList<string> Roles { get; set; } = new List<string>();
}
