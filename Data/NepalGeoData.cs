namespace NepalMediHub.Data;

/// <summary>
/// Static Nepal province/district reference data for address dropdowns.
/// Sample subset for an academic project — not an exhaustive administrative list.
/// </summary>
public static class NepalGeoData
{
    public static readonly IReadOnlyDictionary<string, string[]> ProvinceDistricts =
        new Dictionary<string, string[]>
        {
            ["Koshi"] = new[] { "Morang", "Sunsari", "Jhapa", "Ilam", "Dhankuta", "Udayapur" },
            ["Madhesh"] = new[] { "Dhanusha", "Mahottari", "Sarlahi", "Bara", "Parsa", "Siraha" },
            ["Bagmati"] = new[] { "Kathmandu", "Lalitpur", "Bhaktapur", "Kavrepalanchok", "Chitwan", "Makwanpur", "Dhading" },
            ["Gandaki"] = new[] { "Kaski", "Tanahun", "Syangja", "Gorkha", "Lamjung", "Nawalpur" },
            ["Lumbini"] = new[] { "Rupandehi", "Kapilvastu", "Dang", "Banke", "Bardiya", "Palpa" },
            ["Karnali"] = new[] { "Surkhet", "Jumla", "Dailekh", "Kalikot", "Salyan" },
            ["Sudurpashchim"] = new[] { "Kailali", "Kanchanpur", "Doti", "Achham", "Dadeldhura", "Baitadi" }
        };

    public static IEnumerable<string> Provinces => ProvinceDistricts.Keys;

    public static IEnumerable<string> DistrictsFor(string? province)
        => province != null && ProvinceDistricts.TryGetValue(province, out var districts)
            ? districts
            : Array.Empty<string>();
}
