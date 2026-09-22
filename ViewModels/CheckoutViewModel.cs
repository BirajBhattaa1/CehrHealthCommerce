using System.ComponentModel.DataAnnotations;

namespace CehrHealthCommerce.ViewModels;

/// <summary>
/// Backs the checkout form. Collects Nepal-focused delivery details and the chosen
/// payment method, and carries the (server-computed) cart summary for display.
/// </summary>
public class CheckoutViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(120)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [StringLength(20)]
    [RegularExpression(@"^(\+977[- ]?)?\d{7,10}$", ErrorMessage = "Enter a valid phone number.")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address line is required.")]
    [StringLength(250)]
    [Display(Name = "Address (tole / street / house no.)")]
    public string AddressLine { get; set; } = string.Empty;

    [Required(ErrorMessage = "Province is required.")]
    [StringLength(60)]
    public string Province { get; set; } = string.Empty;

    [Required(ErrorMessage = "District is required.")]
    [StringLength(60)]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Municipality / rural municipality is required.")]
    [StringLength(80)]
    [Display(Name = "Municipality / Rural municipality")]
    public string Municipality { get; set; } = string.Empty;

    [StringLength(10)]
    [Display(Name = "Ward no.")]
    public string? Ward { get; set; }

    [StringLength(10)]
    [Display(Name = "Postal code")]
    public string? PostalCode { get; set; }

    [Required]
    [Display(Name = "Payment method")]
    public string PaymentMethod { get; set; } = "esewa";

    /// <summary>Server-computed cart summary (not model-bound from the form).</summary>
    public CartViewModel Cart { get; set; } = new();

    /// <summary>Flat delivery fee shown in the summary; set by the controller.</summary>
    public decimal DeliveryCharge { get; set; }

    public decimal Total => Cart.Subtotal + DeliveryCharge;
}
