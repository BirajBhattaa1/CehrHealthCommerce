namespace NepalMediHub.Services;

/// <summary>
/// Result of an order-placement attempt. Carries either the created order
/// (on success) or a user-facing error message (on failure).
/// </summary>
public record OrderResult(bool Success, string? Error, int OrderId)
{
    public static OrderResult Ok(int orderId) => new(true, null, orderId);
    public static OrderResult Fail(string error) => new(false, error, 0);
}
