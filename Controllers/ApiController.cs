using NepalMediHub.Data;
using NepalMediHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NepalMediHub.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApiController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    /// <summary>
    /// Returns the caller's OWN medical purchase history. The route takes no userId:
    /// the subject is always derived from the authenticated principal, so a caller cannot
    /// read another citizen's records by guessing or changing an identifier.
    /// </summary>
    [HttpGet("user/me/medical-history")]
    public async Task<IActionResult> GetMedicalPurchaseHistory()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { error = "Not authenticated." });

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return NotFound(new { error = "User not found" });

        var orders = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId && o.OrderStatus != OrderStatus.Cancelled)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        var medicalHistory = orders.SelectMany(order => order.Items.Select(item => new
        {
            orderId = order.OrderId,
            orderDate = order.OrderDate.ToString("yyyy-MM-dd"),
            medicineName = item.ProductName,
            quantity = item.Quantity,
            unitPrice = item.UnitPrice,
            prescriptionRequired = item.Product?.PrescriptionRequired ?? false,
            manufacturer = item.Product?.Manufacturer,
            patientHealthId = order.PatientHealthId
        })).ToList();

        return Ok(new
        {
            userId = user.Id,
            patientHealthId = user.PatientHealthId,
            fullName = user.FullName,
            nid = user.NID,
            totalOrders = orders.Count,
            purchaseHistory = medicalHistory
        });
    }
}
