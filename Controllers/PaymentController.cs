using NepalMediHub.Models;
using NepalMediHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace NepalMediHub.Controllers;

/// <summary>
/// Drives the eSewa sandbox payment flow:
///   Pay      → builds a signed form and auto-POSTs the browser to eSewa
///   Success  → eSewa redirects here; we verify the signed response and mark the order paid
///   Failure  → eSewa redirects here when the user cancels or payment fails
///
/// Only sandbox test payments are handled — no real funds move.
/// </summary>
[Authorize]
public class PaymentController : Controller
{
    private readonly IOrderService _orders;
    private readonly IPaymentService _payment;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IOrderService orders,
        IPaymentService payment,
        UserManager<ApplicationUser> userManager,
        ILogger<PaymentController> logger)
    {
        _orders = orders;
        _payment = payment;
        _userManager = userManager;
        _logger = logger;
    }

    private string UserId => _userManager.GetUserId(User)!;

    /// <summary>Renders an auto-submitting form that POSTs the signed order to eSewa.</summary>
    [HttpGet]
    [EnableRateLimiting("payment")]
    public async Task<IActionResult> Pay(int orderId)
    {
        // Ownership check — a user can only pay for their own order.
        var order = await _orders.GetForUserAsync(UserId, orderId);
        if (order is null) return NotFound();

        if (order.PaymentStatus == PaymentStatus.Successful)
            return RedirectToAction("Confirmation", "Orders", new { id = order.OrderId });

        if (order.Payment is null || !string.Equals(order.Payment.Method, "eSewa", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "This order is not set up for online payment.";
            return RedirectToAction("Details", "Orders", new { id = order.OrderId });
        }

        var successUrl = Url.Action(nameof(Success), "Payment", null, Request.Scheme)!;
        var failureUrl = Url.Action(nameof(Failure), "Payment", null, Request.Scheme)!;

        var form = _payment.BuildForm(order, successUrl, failureUrl);
        return View(form);
    }

    /// <summary>eSewa success callback. Verifies the signed payload before crediting the order.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Success(string? data)
    {
        var result = await _payment.VerifyAsync(data ?? string.Empty);

        if (!result.SignatureValid || string.IsNullOrEmpty(result.TransactionUuid))
        {
            TempData["Error"] = result.Error ?? "We could not verify your payment.";
            return RedirectToAction("Index", "Orders");
        }

        var order = await _orders.GetByTransactionUuidAsync(result.TransactionUuid);
        if (order is null)
        {
            _logger.LogWarning("Verified eSewa payment for unknown transaction {Uuid}.", result.TransactionUuid);
            TempData["Error"] = "We verified the payment but could not match it to an order.";
            return RedirectToAction("Index", "Orders");
        }

        if (result.IsComplete)
        {
            await _orders.MarkPaidAsync(order.OrderId, result.TransactionCode);
            TempData["Success"] = "Payment successful. Thank you!";
            return RedirectToAction("Confirmation", "Orders", new { id = order.OrderId });
        }

        await _orders.MarkPaymentFailedAsync(order.OrderId);
        TempData["Error"] = $"Payment was not completed (status: {result.Status}).";
        return RedirectToAction("Details", "Orders", new { id = order.OrderId });
    }

    /// <summary>eSewa failure/cancel callback. We do not mutate state here (payment stays pending).</summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Failure(string? data)
    {
        // We intentionally avoid changing order state on an unauthenticated, unverified
        // failure callback. The order remains pending so the customer can retry.
        return View();
    }
}
