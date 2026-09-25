namespace NepalMediHub.Services;

/// <summary>Outcome of a cart mutation, used to surface a user-friendly message.</summary>
public record CartActionResult(bool Success, string Message)
{
    public static CartActionResult Ok(string message) => new(true, message);
    public static CartActionResult Fail(string message) => new(false, message);
}
