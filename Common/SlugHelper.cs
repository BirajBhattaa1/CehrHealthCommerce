using System.Text.RegularExpressions;

namespace NepalMediHub.Common;

/// <summary>Converts free text into a lowercase, URL-friendly slug (e.g. "Pain Relief" → "pain-relief").</summary>
public static class SlugHelper
{
    public static string Slugify(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var s = input.Trim().ToLowerInvariant();
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");   // drop punctuation
        s = Regex.Replace(s, @"\s+", "-");            // spaces → hyphens
        s = Regex.Replace(s, @"-+", "-");             // collapse repeats
        return s.Trim('-');
    }
}
