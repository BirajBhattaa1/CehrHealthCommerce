namespace NepalMediHub.Services;

/// <summary>
/// Strongly-typed configuration for the eSewa ePay v2 <b>sandbox</b> gateway,
/// bound from the "Esewa" section of appsettings.json.
///
/// SECURITY NOTE: The values shipped in appsettings.json are eSewa's PUBLIC,
/// documented sandbox test credentials (merchant code EPAYTEST) — they are not
/// real secrets and cannot move real money. For a production deployment the real
/// merchant code and secret key MUST be supplied out of source control via
/// user-secrets or environment variables (e.g. Esewa__SecretKey), never committed.
/// </summary>
public class EsewaOptions
{
    /// <summary>Merchant/product code. Sandbox test value is "EPAYTEST".</summary>
    public string ProductCode { get; set; } = "EPAYTEST";

    /// <summary>HMAC-SHA256 signing key issued by eSewa (sandbox test key by default).</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>eSewa hosted payment form endpoint (browser is POSTed here).</summary>
    public string FormUrl { get; set; } = "https://rc-epay.esewa.com.np/api/epay/main/v2/form";

    /// <summary>Server-to-server transaction status lookup endpoint.</summary>
    public string StatusUrl { get; set; } = "https://rc.esewa.com.np/api/epay/transaction/status/";
}
