using NepalMediHub.Data;
using NepalMediHub.Models;
using NepalMediHub.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NepalMediHub.Areas.Admin.Controllers;

/// <summary>
/// Admin dashboard landing page. The whole Admin area is restricted to the Admin role;
/// customers who reach any admin URL are redirected to Access Denied.
/// </summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel
        {
            ProductCount = await _db.Products.CountAsync(),
            ActiveProductCount = await _db.Products.CountAsync(p => p.IsActive),
            CategoryCount = await _db.Categories.CountAsync(),
            OrderCount = await _db.Orders.CountAsync(),
            PendingOrderCount = await _db.Orders.CountAsync(o => o.OrderStatus == OrderStatus.Pending),
            TotalRevenue = await _db.Orders
                .Where(o => o.PaymentStatus == PaymentStatus.Successful)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m,
            CustomerCount = (await _userManager.GetUsersInRoleAsync("Customer")).Count,
            RecentOrders = await _db.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync(),
            LowStockProducts = await _db.Products
                .Where(p => p.IsActive && p.StockQuantity <= 5)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .ToListAsync()
        };

        return View(model);
    }
}
