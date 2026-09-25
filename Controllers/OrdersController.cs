using NepalMediHub.Models;
using NepalMediHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace NepalMediHub.Controllers;

/// <summary>
/// Customer-facing order history. Every query is scoped to the signed-in user's id,
/// so a user can only ever see their own orders (authorization + data isolation).
/// </summary>
[Authorize]
public class OrdersController : Controller
{
    private readonly IOrderService _orders;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(IOrderService orders, UserManager<ApplicationUser> userManager)
    {
        _orders = orders;
        _userManager = userManager;
    }

    private string UserId => _userManager.GetUserId(User)!;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var orders = await _orders.GetForUserAsync(UserId);
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _orders.GetForUserAsync(UserId, id);
        if (order is null) return NotFound();
        return View(order);
    }

    /// <summary>Order-placed confirmation screen (shown right after checkout).</summary>
    [HttpGet]
    public async Task<IActionResult> Confirmation(int id)
    {
        var order = await _orders.GetForUserAsync(UserId, id);
        if (order is null) return NotFound();
        return View(order);
    }

    [HttpGet]
    public async Task<IActionResult> Invoice(int id)
    {
        var order = await _orders.GetForUserAsync(UserId, id);
        if (order is null) return NotFound();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _orders.CancelOrderAsync(UserId, id);

        if (result.success)
        {
            TempData["Success"] = "Order cancelled successfully. Stock has been restored.";
        }
        else
        {
            TempData["Error"] = result.error ?? "Unable to cancel order.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
