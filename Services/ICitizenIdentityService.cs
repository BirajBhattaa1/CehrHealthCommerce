namespace NepalMediHub.Services;

/// <summary>
/// Represents the citizen identity layer. In this project the NID is SIMULATED and stored
/// locally; this service is the seam where a real Government NID / CEHR identity service
/// would be plugged in later.
/// </summary>
public interface ICitizenIdentityService
{
    /// <summary>Removes formatting (spaces/hyphens) and returns a canonical NID string.</summary>
    string NormalizeNid(string nid);

    /// <summary>Validates the (simulated) NID format. Returns true if acceptable.</summary>
    bool IsValidNidFormat(string nid);

    /// <summary>Checks whether a NID is not already used by another citizen account.</summary>
    Task<bool> IsNidAvailableAsync(string nid, string? excludeUserId = null);
}
