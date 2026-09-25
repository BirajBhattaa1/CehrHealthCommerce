using System.Text;
using NepalMediHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace NepalMediHub.Controllers;

/// <summary>
/// Serves SEO infrastructure endpoints — an XML sitemap and robots.txt.
/// This is part of Area 3 (SEO &amp; Analytics) of the CSC381 project.
///
/// Attribute routing ([HttpGet("/sitemap.xml")], [HttpGet("/robots.txt")]) is used
/// so the endpoints live at the exact root paths crawlers expect. These attribute
/// routes coexist with the application's conventional {controller}/{action} routing.
///
/// The sitemap is generated dynamically from the live catalogue, so newly added
/// (active) categories and products appear automatically — no hand-maintained file.
/// </summary>
public class SeoController : Controller
{
    private readonly IProductService _products;
    private readonly ICategoryService _categories;

    public SeoController(IProductService products, ICategoryService categories)
    {
        _products = products;
        _categories = categories;
    }

    /// <summary>
    /// Dynamic XML sitemap listing every publicly crawlable page: the landing/section
    /// pages, each active category page, and each active product detail page.
    /// </summary>
    [HttpGet("/sitemap.xml")]
    public async Task<IActionResult> Sitemap()
    {
        var scheme = Request.Scheme;
        var entries = new List<SitemapEntry>();

        void Add(string? loc, string changeFreq, string priority, DateTime? lastMod = null)
        {
            if (!string.IsNullOrEmpty(loc))
                entries.Add(new SitemapEntry(loc!, changeFreq, priority, lastMod));
        }

        // Static / section pages
        Add(Url.Action("Index", "Home", null, scheme), "daily", "1.0");
        Add(Url.Action("About", "Home", null, scheme), "monthly", "0.3");
        Add(Url.Action("Index", "Products", null, scheme), "daily", "0.9");
        Add(Url.Action("Medicines", "Products", null, scheme), "daily", "0.8");
        Add(Url.Action("Equipment", "Products", null, scheme), "daily", "0.8");
        Add(Url.Action("HealthProducts", "Products", null, scheme), "daily", "0.8");
        Add(Url.Action("Categories", "Products", null, scheme), "weekly", "0.6");

        // Active category pages
        var categories = await _categories.GetActiveAsync();
        foreach (var c in categories)
            Add(Url.Action("Category", "Products", new { slug = c.Slug }, scheme), "weekly", "0.6");

        // Active product detail pages
        var products = await _products.GetAllForAdminAsync();
        foreach (var p in products.Where(p => p.IsActive))
            Add(Url.Action("Details", "Products", new { slug = p.Slug }, scheme), "weekly", "0.7", p.UpdatedAt);

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        foreach (var e in entries)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{System.Security.SecurityElement.Escape(e.Loc)}</loc>");
            if (e.LastMod.HasValue)
                sb.AppendLine($"    <lastmod>{e.LastMod.Value.ToUniversalTime():yyyy-MM-dd}</lastmod>");
            sb.AppendLine($"    <changefreq>{e.ChangeFreq}</changefreq>");
            sb.AppendLine($"    <priority>{e.Priority}</priority>");
            sb.AppendLine("  </url>");
        }
        sb.AppendLine("</urlset>");

        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }

    /// <summary>
    /// robots.txt — allows crawling of the public storefront while disallowing
    /// private/authenticated areas (admin, account, cart, checkout, orders, payment),
    /// and points crawlers at the sitemap.
    /// </summary>
    [HttpGet("/robots.txt")]
    public IActionResult Robots()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var sb = new StringBuilder();
        sb.AppendLine("User-agent: *");
        sb.AppendLine("Allow: /");
        sb.AppendLine("Disallow: /Admin");
        sb.AppendLine("Disallow: /Account");
        sb.AppendLine("Disallow: /Cart");
        sb.AppendLine("Disallow: /Checkout");
        sb.AppendLine("Disallow: /Orders");
        sb.AppendLine("Disallow: /Payment");
        sb.AppendLine();
        sb.AppendLine($"Sitemap: {baseUrl}/sitemap.xml");
        return Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }

    private readonly record struct SitemapEntry(string Loc, string ChangeFreq, string Priority, DateTime? LastMod);
}
