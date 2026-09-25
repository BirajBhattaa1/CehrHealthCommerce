namespace NepalMediHub.Services;

/// <summary>
/// Stub implementation of the CEHR integration seam. Returns "not enabled" for every call.
/// A real implementation would live here and call an authorised government/CEHR endpoint.
/// </summary>
public class CehrIntegrationService : ICehrIntegrationService
{
    public bool IsIntegrationEnabled => false;

    public Task<CehrLinkResult> TryLinkCitizenAsync(string nid)
        => Task.FromResult(new CehrLinkResult(
            Success: false,
            Message: "CEHR integration is a documented future capability and is not enabled in this academic build."));
}
