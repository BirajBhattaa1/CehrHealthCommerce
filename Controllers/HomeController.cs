using System.Diagnostics;
using CehrHealthCommerce.Models;
using CehrHealthCommerce.Services;
using Microsoft.AspNetCore.Mvc;

namespace CehrHealthCommerce.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProductService _products;

    public HomeController(ILogger<HomeController> logger, IProductService products)
    {
        _logger = logger;
        _products = products;
    }

    public async Task<IActionResult> Index()
    {
        var featured = await _products.GetFeaturedAsync(8);
        return View(featured);
    }

    public IActionResult About() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
