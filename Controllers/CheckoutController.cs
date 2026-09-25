using NepalMediHub.Data;
using NepalMediHub.Models;
using NepalMediHub.Services;
using NepalMediHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace NepalMediHub.Controllers;

/// <summary>
/// Checkout: collects delivery details, then places the order from the cart.
/// Requires an authenticated customer (linked to their simulated NID identity).
/// </summary>
[Authorize]
public class CheckoutController : Controller
{
    private readonly ICartService _cart;
    private readonly IOrderService _orders;
    private readonly UserManager<ApplicationUser> _userManager;

    public CheckoutController(
        ICartService cart,
        IOrderService orders,
        UserManager<ApplicationUser> userManager)
    {
        _cart = cart;
        _orders = orders;
        _userManager = userManager;
    }

    private string UserId => _userManager.GetUserId(User)!;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cart = await _cart.GetCartAsync(UserId);
        if (cart.IsEmpty)
        {
            TempData["Error"] = "Your cart is empty. Add products before checking out.";
            return RedirectToAction("Index", "Cart");
        }

        var user = await _userManager.GetUserAsync(User);
        var model = new CheckoutViewModel
        {
            // Pre-fill contact details from the signed-in profile for convenience.
            FullName = user?.FullName ?? string.Empty,
            Email = user?.Email ?? string.Empty,
            Phone = user?.PhoneNumber ?? string.Empty,
            Cart = cart,
            DeliveryCharge = _orders.DeliveryCharge
        };

        PopulateGeo();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        // Always rebuild the cart summary server-side (never trust posted totals).
        var cart = await _cart.GetCartAsync(UserId);
        if (cart.IsEmpty)
        {
            TempData["Error"] = "Your cart is empty. Add products before checking out.";
            return RedirectToAction("Index", "Cart");
        }

        model.Cart = cart;
        model.DeliveryCharge = _orders.DeliveryCharge;

        if (!ModelState.IsValid)
        {
            PopulateGeo();
            return View(model);
        }

        var result = await _orders.CreateFromCartAsync(UserId, model, model.PaymentMethod);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not place your order. Please try again.");
            PopulateGeo();
            return View(model);
        }

        // eSewa → go to the sandbox payment flow; COD → straight to confirmation.
        if (string.Equals(model.PaymentMethod, "esewa", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Pay", "Payment", new { orderId = result.OrderId });

        return RedirectToAction("Confirmation", "Orders", new { id = result.OrderId });
    }

    private void PopulateGeo()
    {
        ViewBag.Provinces = NepalGeoData.Provinces;
        ViewBag.GeoJson = System.Text.Json.JsonSerializer.Serialize(NepalGeoData.ProvinceDistricts);
    }
}
