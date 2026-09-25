using NepalMediHub.Models;
using NepalMediHub.ViewModels;

namespace NepalMediHub.Services;

/// <summary>
/// Order lifecycle: create an order from the user's cart, read a user's orders,
/// and (for the Admin panel) read/update all orders. Totals are always computed
/// server-side from live product prices — client-supplied amounts are never trusted.
/// </summary>
public interface IOrderService
{
    /// <summary>Flat delivery fee (NPR) added to every order. Demo constant.</summary>
    decimal DeliveryCharge { get; }

    Task<OrderResult> CreateFromCartAsync(string userId, CheckoutViewModel delivery, string paymentMethod);

    // Customer-scoped reads (always filtered by userId so one user cannot see another's orders).
    Task<IReadOnlyList<Order>> GetForUserAsync(string userId);
    Task<Order?> GetForUserAsync(string userId, int orderId);

    // Payment transitions (used by the payment flow in Phase 10).
    Task<Order?> GetByTransactionUuidAsync(string transactionUuid);
    Task<bool> MarkPaidAsync(int orderId, string? gatewayRef);
    Task<bool> MarkPaymentFailedAsync(int orderId);

    // Admin-scoped operations.
    Task<IReadOnlyList<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int orderId);
    Task<bool> UpdateStatusAsync(int orderId, OrderStatus status);
    Task<(bool success, string? error)> CancelOrderAsync(string userId, int orderId);
}
