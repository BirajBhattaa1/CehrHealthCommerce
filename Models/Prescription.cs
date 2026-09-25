using System.ComponentModel.DataAnnotations;

namespace NepalMediHub.Models;

/// <summary>
/// Minimal prescription record. Designed so a full pharmacist-verification workflow can be
/// added later. This project does NOT perform real/legal prescription validation.
/// </summary>
public class Prescription
{
    public int PrescriptionId { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    [Required, StringLength(300)]
    public string FilePath { get; set; } = string.Empty;

    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Pending;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
