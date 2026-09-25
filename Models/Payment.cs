using System.ComponentModel.DataAnnotations;

namespace NepalMediHub.Models;

/// <summary>Payment record for an order (one-to-one). Uses sandbox gateway only.</summary>
public class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public decimal Amount { get; set; }

    [StringLength(40)]
    public string Method { get; set; } = "eSewa";

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    /// <summary>Unique id we generate and send to the gateway (transaction_uuid).</summary>
    [Required, StringLength(60)]
    public string TransactionUuid { get; set; } = string.Empty;

    /// <summary>Reference/transaction code returned by the gateway on success.</summary>
    [StringLength(120)]
    public string? GatewayRef { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }
}
