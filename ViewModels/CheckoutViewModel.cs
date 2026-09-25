using System.ComponentModel.DataAnnotations;

namespace NepalMediHub.ViewModels;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(120, MinimumLength = 2)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [StringLength(20)]
    [RegularExpression(@"^(?:\+977[- ]?)?9[78]\d{8}$", ErrorMessage = "Enter a valid 10-digit Nepali mobile number starting with 97 or 98")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    [StringLength(150)]
    [RegularExpression(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", ErrorMessage = "Email must contain @ and a valid domain")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [StringLength(250, MinimumLength = 5)]
    [Display(Name = "Address (tole / street / house no.)")]
    public string AddressLine { get; set; } = string.Empty;

    [Required(ErrorMessage = "Province is required")]
    [StringLength(60)]
    public string Province { get; set; } = string.Empty;

    [Required(ErrorMessage = "District is required")]
    [StringLength(60)]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Municipality is required")]
    [StringLength(80)]
    [Display(Name = "Municipality / Rural municipality")]
    public string Municipality { get; set; } = string.Empty;

    [StringLength(10)]
    [RegularExpression(@"^\d{1,2}$", ErrorMessage = "Ward number must be 1 or 2 digits")]
    [Display(Name = "Ward no.")]
    public string? Ward { get; set; }

    [StringLength(10)]
    [RegularExpression(@"^[0-9]{5}$", ErrorMessage = "Postal code must be 5 digits")]
    [Display(Name = "Postal code")]
    public string? PostalCode { get; set; }

    [Required]
    [Display(Name = "Payment method")]
    public string PaymentMethod { get; set; } = "esewa";

    public CartViewModel Cart { get; set; } = new();

    public decimal DeliveryCharge { get; set; }

    public decimal Total => Cart.Subtotal + DeliveryCharge;
}
