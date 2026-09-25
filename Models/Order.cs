using System.ComponentModel.DataAnnotations;

namespace NepalMediHub.Models;

/// <summary>A customer order. The delivery address is snapshotted at order time.</summary>
public class Order
{
    public int OrderId { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    // --- Delivery address snapshot (Nepal-focused) ---
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Email { get; set; } = string.Empty;

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

    [StringLength(100)]
    public string? PatientHealthId { get; set; }

    [StringLength(500)]
    public string? PrescriptionFileUrl { get; set; }

    // Navigation
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public Payment? Payment { get; set; }
}
