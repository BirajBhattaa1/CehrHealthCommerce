using NepalMediHub.Data;
using NepalMediHub.Models;
using NepalMediHub.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace NepalMediHub.Services;

/// <summary>
/// EF Core implementation of the order lifecycle. All queries are LINQ-to-Entities
/// (parameterized by EF Core — safe from SQL injection). Order totals are recomputed
/// from the live product price at placement time so the client cannot tamper with them.
/// </summary>
public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _db;

    public OrderService(ApplicationDbContext db) => _db = db;

    /// <summary>Flat NPR 100 delivery fee for the demo. Kept simple deliberately.</summary>
    public decimal DeliveryCharge => 100m;

    public async Task<OrderResult> CreateFromCartAsync(string userId, CheckoutViewModel delivery, string paymentMethod)
    {
        // Load the cart with items and their products.
        var cart = await _db.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        var items = cart?.Items
            .Where(i => i.Product != null && i.Product.IsActive)
            .ToList() ?? new List<CartItem>();

        if (items.Count == 0)
            return OrderResult.Fail("Your cart is empty.");

        // Re-validate stock server-side before committing the order.
        foreach (var item in items)
        {
            if (item.Quantity > item.Product!.StockQuantity)
                return OrderResult.Fail(
                    $"Not enough stock for \"{item.Product.Name}\" (only {item.Product.StockQuantity} left). Please update your cart.");
        }

        var method = NormalizeMethod(paymentMethod);

        var transactionUuid = GenerateTransactionUuid();

        // Compute totals from live prices — never from anything the client submitted.
        var subtotal = items.Sum(i => i.Product!.Price * i.Quantity);
        var total = subtotal + DeliveryCharge;

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = total,
            OrderStatus = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            // Delivery snapshot (Nepal-focused address fields).
            FullName = delivery.FullName,
            Phone = delivery.Phone,
            Email = delivery.Email,
            AddressLine = delivery.AddressLine,
            Province = delivery.Province,
            District = delivery.District,
            Municipality = delivery.Municipality,
            Ward = delivery.Ward,
            PostalCode = delivery.PostalCode
        };

        foreach (var item in items)
        {
            // Snapshot name + unit price so the order is stable even if the product changes later.
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.Product!.Name,
                UnitPrice = item.Product.Price,
                Quantity = item.Quantity
            });

            // Reserve stock at placement so we can never oversell.
            item.Product.StockQuantity -= item.Quantity;
        }

        order.Payment = new Payment
        {
            Amount = total,
            Method = method,
            PaymentStatus = PaymentStatus.Pending,
            TransactionUuid = transactionUuid,
            CreatedAt = DateTime.UtcNow
        };

        // Cash on Delivery is confirmed immediately (paid in person on delivery).
        // eSewa stays Pending until the sandbox payment is verified.
        if (method == "Cash on Delivery")
            order.OrderStatus = OrderStatus.Confirmed;

        _db.Orders.Add(order);

        // Empty the cart now that its contents have become an order.
        _db.CartItems.RemoveRange(cart!.Items);

        await _db.SaveChangesAsync();
        return OrderResult.Ok(order.OrderId);
    }

    public async Task<IReadOnlyList<Order>> GetForUserAsync(string userId)
        => await _db.Orders
            .Include(o => o.Payment)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

    public async Task<Order?> GetForUserAsync(string userId, int orderId)
        => await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

    public async Task<Order?> GetByTransactionUuidAsync(string transactionUuid)
        => await _db.Orders
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Payment != null && o.Payment.TransactionUuid == transactionUuid);

    public async Task<bool> MarkPaidAsync(int orderId, string? gatewayRef)
    {
        var order = await _db.Orders.Include(o => o.Payment).FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order?.Payment is null) return false;

        order.Payment.PaymentStatus = PaymentStatus.Successful;
        order.Payment.GatewayRef = gatewayRef;
        order.Payment.VerifiedAt = DateTime.UtcNow;

        order.PaymentStatus = PaymentStatus.Successful;
        if (order.OrderStatus == OrderStatus.Pending)
            order.OrderStatus = OrderStatus.Confirmed;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkPaymentFailedAsync(int orderId)
    {
        var order = await _db.Orders.Include(o => o.Payment).FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order?.Payment is null) return false;

        order.Payment.PaymentStatus = PaymentStatus.Failed;
        order.PaymentStatus = PaymentStatus.Failed;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync()
        => await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Payment)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

    public async Task<Order?> GetByIdAsync(int orderId)
        => await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

    public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order is null) return false;

        order.OrderStatus = status;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<(bool success, string? error)> CancelOrderAsync(string userId, int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

        if (order is null)
            return (false, "Order not found.");

        if (order.OrderStatus != OrderStatus.Pending && order.OrderStatus != OrderStatus.Processing)
            return (false, "Only pending or processing orders can be cancelled.");

        order.OrderStatus = OrderStatus.Cancelled;

        foreach (var item in order.Items.Where(i => i.Product != null))
        {
            item.Product!.StockQuantity += item.Quantity;
        }

        await _db.SaveChangesAsync();
        return (true, null);
    }

    private static string NormalizeMethod(string? method)
        => string.Equals(method, "cod", StringComparison.OrdinalIgnoreCase)
            ? "Cash on Delivery"
            : "eSewa";

    private static string GenerateTransactionUuid()
        => $"CEHR-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8]}";
}
