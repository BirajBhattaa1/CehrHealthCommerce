namespace NepalMediHub.ViewModels.Admin;

/// <summary>Row in the admin user list.</summary>
public class AdminUserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NID { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = new List<string>();
    public bool IsLockedOut { get; set; }
    public DateTime CreatedAt { get; set; }
}
