using System.Diagnostics;
using System.Text;
using NepalMediHub.Data;
using NepalMediHub.Models;
using NepalMediHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NepalMediHub.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProductService _products;
    private readonly ApplicationDbContext _db;

    public HomeController(ILogger<HomeController> logger, IProductService products, ApplicationDbContext db)
    {
        _logger = logger;
        _products = products;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var featured = await _products.GetFeaturedAsync(8);
        return View(featured);
    }

    public IActionResult About() => View();

    [HttpGet("sitemap.xml")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Sitemap()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var xml = new StringBuilder();
        xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        xml.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // Static pages
        xml.AppendLine($"  <url><loc>{baseUrl}/</loc><changefreq>daily</changefreq><priority>1.0</priority></url>");
        xml.AppendLine($"  <url><loc>{baseUrl}/Home/About</loc><changefreq>monthly</changefreq><priority>0.7</priority></url>");
        xml.AppendLine($"  <url><loc>{baseUrl}/Products</loc><changefreq>daily</changefreq><priority>0.9</priority></url>");
        xml.AppendLine($"  <url><loc>{baseUrl}/Products/Categories</loc><changefreq>weekly</changefreq><priority>0.8</priority></url>");
        xml.AppendLine($"  <url><loc>{baseUrl}/Products/Medicines</loc><changefreq>daily</changefreq><priority>0.9</priority></url>");
        xml.AppendLine($"  <url><loc>{baseUrl}/Products/Equipment</loc><changefreq>weekly</changefreq><priority>0.8</priority></url>");
        xml.AppendLine($"  <url><loc>{baseUrl}/Products/HealthProducts</loc><changefreq>weekly</changefreq><priority>0.8</priority></url>");

        // Categories
        var categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
        foreach (var category in categories)
        {
            xml.AppendLine($"  <url><loc>{baseUrl}/Products/Category?slug={category.Slug}</loc><changefreq>weekly</changefreq><priority>0.8</priority></url>");
        }

        // Products
        var products = await _db.Products.Where(p => p.IsActive).ToListAsync();
        foreach (var product in products)
        {
            xml.AppendLine($"  <url><loc>{baseUrl}/Products/Details/{product.Slug}</loc><changefreq>weekly</changefreq><priority>0.7</priority></url>");
        }

        xml.AppendLine("</urlset>");
        return Content(xml.ToString(), "application/xml", Encoding.UTF8);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
