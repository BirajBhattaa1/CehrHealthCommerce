using NepalMediHub.Models;
using NepalMediHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NepalMediHub.Areas.Admin.Controllers;

/// <summary>Admin order management: view all orders, drill into details, update status.</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders) => _orders = orders;

    public async Task<IActionResult> Index()
    {
        var orders = await _orders.GetAllAsync();
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _orders.GetByIdAsync(id);
        if (order is null) return NotFound();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        var ok = await _orders.UpdateStatusAsync(id, status);
        TempData[ok ? "Success" : "Error"] = ok
            ? $"Order #{id} status updated to {status}."
            : "Could not update the order.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
