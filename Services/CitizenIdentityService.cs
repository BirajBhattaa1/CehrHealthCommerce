using NepalMediHub.Data;
using Microsoft.EntityFrameworkCore;

namespace NepalMediHub.Services;

/// <summary>
/// Local, simulated citizen identity service. It validates NID format and uniqueness against
/// our own database only. It does NOT contact any Government of Nepal system.
/// </summary>
public class CitizenIdentityService : ICitizenIdentityService
{
    private readonly ApplicationDbContext _db;

    public CitizenIdentityService(ApplicationDbContext db)
    {
        _db = db;
    }

    public string NormalizeNid(string nid)
        => new string((nid ?? string.Empty).Where(char.IsDigit).ToArray());

    public bool IsValidNidFormat(string nid)
    {
        var normalized = NormalizeNid(nid);
        // Simulated rule: 10 to 16 digits. (Real NID validation would be defined by the issuer.)
        return normalized.Length is >= 10 and <= 16;
    }

    public async Task<bool> IsNidAvailableAsync(string nid, string? excludeUserId = null)
    {
        var normalized = NormalizeNid(nid);
        return !await _db.Users.AnyAsync(u =>
            u.NID == normalized && (excludeUserId == null || u.Id != excludeUserId));
    }
}
