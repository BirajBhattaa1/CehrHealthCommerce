using NepalMediHub.Models;
using NepalMediHub.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace NepalMediHub.ViewComponents;

/// <summary>Renders the navbar cart link with a live item-count badge.</summary>
public class CartSummaryViewComponent : ViewComponent
{
    private readonly ICartService _cart;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartSummaryViewComponent(ICartService cart, UserManager<ApplicationUser> userManager)
    {
        _cart = cart;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var count = 0;
        if (UserClaimsPrincipal.Identity?.IsAuthenticated ?? false)
        {
            var userId = _userManager.GetUserId(UserClaimsPrincipal);
            if (!string.IsNullOrEmpty(userId))
            {
                count = await _cart.GetItemCountAsync(userId);
            }
        }
        return View(count);
    }
}
