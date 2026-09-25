using NepalMediHub.Models;

namespace NepalMediHub.ViewModels.Admin;

/// <summary>Aggregated figures and recent activity for the admin dashboard.</summary>
public class AdminDashboardViewModel
{
    public int ProductCount { get; set; }
    public int ActiveProductCount { get; set; }
    public int CategoryCount { get; set; }
    public int OrderCount { get; set; }
    public int PendingOrderCount { get; set; }
    public int CustomerCount { get; set; }
    public decimal TotalRevenue { get; set; }

    public IReadOnlyList<Order> RecentOrders { get; set; } = new List<Order>();
    public IReadOnlyList<Product> LowStockProducts { get; set; } = new List<Product>();
}
