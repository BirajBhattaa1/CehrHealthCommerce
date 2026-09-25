namespace NepalMediHub.Services;

/// <summary>Result of a (future) attempt to link a citizen to the national CEHR.</summary>
public record CehrLinkResult(bool Success, string Message);

/// <summary>
/// FUTURE INTEGRATION POINT.
/// This interface documents where the e-commerce system would talk to Nepal's proposed
/// Centralized Electronic Health Record (CEHR). It is intentionally a stub — the project does
/// NOT connect to any real government or CEHR API.
/// </summary>
public interface ICehrIntegrationService
{
    /// <summary>Always false in this academic build.</summary>
    bool IsIntegrationEnabled { get; }

    /// <summary>Placeholder for a future call that would link a citizen (by NID) to the CEHR.</summary>
    Task<CehrLinkResult> TryLinkCitizenAsync(string nid);
}
