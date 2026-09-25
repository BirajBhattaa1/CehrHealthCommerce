using NepalMediHub.Models;

namespace NepalMediHub.Services;

/// <summary>
/// Abstraction over the payment gateway so controllers never depend on eSewa
/// directly. The demo ships a single sandbox implementation (<see cref="EsewaPaymentService"/>).
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Builds the signed set of form fields to auto-POST to the eSewa hosted page
    /// for the given order. The signature is computed server-side with the secret key.
    /// </summary>
    EsewaFormModel BuildForm(Order order, string successUrl, string failureUrl);

    /// <summary>
    /// Verifies the Base64 "data" payload eSewa returns to the success callback:
    /// first cryptographically (HMAC-SHA256 signature over the signed fields), then
    /// — best effort — by calling the server-to-server status API for confirmation.
    /// </summary>
    Task<EsewaVerificationResult> VerifyAsync(string encodedData);
}

/// <summary>The action URL plus hidden fields for the browser-side auto-submitting form.</summary>
public sealed class EsewaFormModel
{
    public string ActionUrl { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Fields { get; init; } = new Dictionary<string, string>();
}

/// <summary>Outcome of verifying an eSewa success callback.</summary>
public sealed class EsewaVerificationResult
{
    /// <summary>True only when the response signature matched our secret key.</summary>
    public bool SignatureValid { get; init; }

    /// <summary>True when the (verified) transaction status is COMPLETE.</summary>
    public bool IsComplete { get; init; }

    public string? TransactionUuid { get; init; }
    public string? TransactionCode { get; init; }
    public string? Status { get; init; }
    public string? Error { get; init; }

    public static EsewaVerificationResult Invalid(string error) =>
        new() { SignatureValid = false, IsComplete = false, Error = error };
}
