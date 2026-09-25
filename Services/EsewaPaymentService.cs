using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NepalMediHub.Models;
using Microsoft.Extensions.Options;

namespace NepalMediHub.Services;

/// <summary>
/// eSewa ePay v2 <b>sandbox</b> integration.
///
/// Request:  we build the hidden form fields and sign
///           "total_amount=..,transaction_uuid=..,product_code=.." with HMAC-SHA256
///           (Base64) using the secret key, then the browser auto-POSTs to eSewa.
/// Response: eSewa redirects back with a Base64 "data" blob; we recompute the HMAC
///           over the fields named in "signed_field_names" and compare it (constant
///           time) to the returned signature, then optionally confirm via the status API.
///
/// No real money moves — this targets eSewa's public test merchant (EPAYTEST).
/// </summary>
public class EsewaPaymentService : IPaymentService
{
    private const string RequestSignedFields = "total_amount,transaction_uuid,product_code";

    private readonly EsewaOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EsewaPaymentService> _logger;

    public EsewaPaymentService(
        IOptions<EsewaOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<EsewaPaymentService> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public EsewaFormModel BuildForm(Order order, string successUrl, string failureUrl)
    {
        if (order.Payment is null)
            throw new InvalidOperationException("Order has no payment record to pay for.");

        // The exact string used for total_amount MUST be identical in the field and
        // in the signed message, or eSewa will reject the signature.
        var totalAmount = FormatAmount(order.TotalAmount);
        var transactionUuid = order.Payment.TransactionUuid;
        var productCode = _options.ProductCode;

        var message = $"total_amount={totalAmount},transaction_uuid={transactionUuid},product_code={productCode}";
        var signature = Sign(message);

        var fields = new Dictionary<string, string>
        {
            ["amount"] = totalAmount,
            ["tax_amount"] = "0",
            ["total_amount"] = totalAmount,
            ["transaction_uuid"] = transactionUuid,
            ["product_code"] = productCode,
            ["product_service_charge"] = "0",
            ["product_delivery_charge"] = "0",
            ["success_url"] = successUrl,
            ["failure_url"] = failureUrl,
            ["signed_field_names"] = RequestSignedFields,
            ["signature"] = signature
        };

        return new EsewaFormModel
        {
            ActionUrl = _options.FormUrl,
            Fields = fields
        };
    }

    public async Task<EsewaVerificationResult> VerifyAsync(string encodedData)
    {
        if (string.IsNullOrWhiteSpace(encodedData))
            return EsewaVerificationResult.Invalid("No payment data was returned by the gateway.");

        // 1. Base64-decode the callback blob into its JSON representation.
        Dictionary<string, string> data;
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(encodedData));
            data = ParseJsonToStringMap(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not decode eSewa callback payload.");
            return EsewaVerificationResult.Invalid("The payment response could not be read.");
        }

        if (!data.TryGetValue("signed_field_names", out var signedFieldNames) ||
            !data.TryGetValue("signature", out var providedSignature))
        {
            return EsewaVerificationResult.Invalid("The payment response was missing its signature.");
        }

        // 2. Rebuild the signed message from the named fields, in the given order,
        //    using the values exactly as eSewa returned them.
        var parts = new List<string>();
        foreach (var name in signedFieldNames.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var key = name.Trim();
            data.TryGetValue(key, out var value);
            parts.Add($"{key}={value}");
        }

        var expectedSignature = Sign(string.Join(",", parts));

        // 3. Constant-time comparison to avoid timing side-channels.
        var signatureValid = FixedTimeEquals(expectedSignature, providedSignature);
        if (!signatureValid)
        {
            _logger.LogWarning("eSewa signature mismatch for transaction {Uuid}.",
                data.GetValueOrDefault("transaction_uuid"));
            return EsewaVerificationResult.Invalid("Payment could not be verified (signature mismatch).");
        }

        var status = data.GetValueOrDefault("status");
        var transactionUuid = data.GetValueOrDefault("transaction_uuid");
        var transactionCode = data.GetValueOrDefault("transaction_code");
        var totalAmount = data.GetValueOrDefault("total_amount");

        // 4. Best-effort server-to-server confirmation. If the network call fails
        //    (e.g. offline demo), we fall back to the cryptographically verified status.
        var confirmedStatus = await TryLookupStatusAsync(transactionUuid, totalAmount) ?? status;

        return new EsewaVerificationResult
        {
            SignatureValid = true,
            IsComplete = string.Equals(confirmedStatus, "COMPLETE", StringComparison.OrdinalIgnoreCase),
            TransactionUuid = transactionUuid,
            TransactionCode = transactionCode,
            Status = confirmedStatus
        };
    }

    // ---------------------------------------------------------------------

    /// <summary>Queries the eSewa status API. Returns null if it cannot be reached.</summary>
    private async Task<string?> TryLookupStatusAsync(string? transactionUuid, string? totalAmount)
    {
        if (string.IsNullOrWhiteSpace(transactionUuid)) return null;

        try
        {
            var amount = (totalAmount ?? string.Empty).Replace(",", string.Empty);
            var url = $"{_options.StatusUrl}?product_code={Uri.EscapeDataString(_options.ProductCode)}" +
                      $"&total_amount={Uri.EscapeDataString(amount)}" +
                      $"&transaction_uuid={Uri.EscapeDataString(transactionUuid)}";

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(8);

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("status", out var statusProp))
                return statusProp.GetString();
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "eSewa status lookup unavailable; using signed status instead.");
        }

        return null;
    }

    /// <summary>Computes Base64(HMAC-SHA256(message, secretKey)).</summary>
    private string Sign(string message)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SecretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToBase64String(hash);
    }

    private static string FormatAmount(decimal amount) =>
        amount.ToString("0.##", CultureInfo.InvariantCulture);

    private static bool FixedTimeEquals(string a, string b)
    {
        var ba = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        return ba.Length == bb.Length && CryptographicOperations.FixedTimeEquals(ba, bb);
    }

    /// <summary>Flattens a flat JSON object into a string→string map (values read as raw text).</summary>
    private static Dictionary<string, string> ParseJsonToStringMap(string json)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        using var doc = JsonDocument.Parse(json);
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            map[prop.Name] = prop.Value.ValueKind == JsonValueKind.String
                ? prop.Value.GetString() ?? string.Empty
                : prop.Value.GetRawText();
        }
        return map;
    }
}
