using NepalMediHub.Data;
using NepalMediHub.Models;
using NepalMediHub.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace NepalMediHub.Services;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _db;

    public CartService(ApplicationDbContext db) => _db = db;

    public async Task<Cart> GetOrCreateCartAsync(string userId)
    {
        var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart is null)
        {
            cart = new Cart { UserId = userId };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
        }
        return cart;
    }

    public async Task<CartViewModel> GetCartAsync(string userId)
    {
        var cart = await _db.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        var vm = new CartViewModel();
        if (cart is null)
        {
            return vm;
        }

        foreach (var item in cart.Items.Where(i => i.Product != null && i.Product!.IsActive))
        {
            var p = item.Product!;
            vm.Lines.Add(new CartLineViewModel
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Slug = p.Slug,
                ImageUrl = p.ImageUrl,
                UnitPrice = p.Price,               // live price — never trusted from the client
                Quantity = item.Quantity,
                StockQuantity = p.StockQuantity,
                PrescriptionRequired = p.PrescriptionRequired
            });
        }

        vm.Lines = vm.Lines.OrderBy(l => l.Name).ToList();
        return vm;
    }

    public async Task<CartActionResult> AddAsync(string userId, int productId, int quantity)
    {
        if (quantity < 1)
        {
            quantity = 1;
        }

        var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == productId && p.IsActive);
        if (product is null)
        {
            return CartActionResult.Fail("That product is not available.");
        }
        if (product.StockQuantity <= 0)
        {
            return CartActionResult.Fail($"{product.Name} is out of stock.");
        }

        var cart = await GetOrCreateCartAsync(userId);
        var line = await _db.CartItems.FirstOrDefaultAsync(i => i.CartId == cart.CartId && i.ProductId == productId);

        var desired = (line?.Quantity ?? 0) + quantity;
        var capped = false;
        if (desired > product.StockQuantity)
        {
            desired = product.StockQuantity;
            capped = true;
        }

        if (line is null)
        {
            _db.CartItems.Add(new CartItem { CartId = cart.CartId, ProductId = productId, Quantity = desired });
        }
        else
        {
            line.Quantity = desired;
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return capped
            ? CartActionResult.Ok($"Only {product.StockQuantity} of {product.Name} are available; your cart was updated to the maximum.")
            : CartActionResult.Ok($"{product.Name} was added to your cart.");
    }

    public async Task UpdateQuantityAsync(string userId, int productId, int quantity)
    {
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
        var line = cart?.Items.FirstOrDefault(i => i.ProductId == productId);
        if (cart is null || line is null)
        {
            return;
        }

        if (quantity < 1)
        {
            _db.CartItems.Remove(line);
        }
        else
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            var max = product?.StockQuantity ?? quantity;
            line.Quantity = max < 1 ? 1 : Math.Min(quantity, max);
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task RemoveAsync(string userId, int productId)
    {
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
        var line = cart?.Items.FirstOrDefault(i => i.ProductId == productId);
        if (line is not null)
        {
            _db.CartItems.Remove(line);
            await _db.SaveChangesAsync();
        }
    }

    public async Task ClearAsync(string userId)
    {
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart is not null && cart.Items.Any())
        {
            _db.CartItems.RemoveRange(cart.Items);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<int> GetItemCountAsync(string userId)
        => await _db.CartItems
            .Where(i => i.Cart!.UserId == userId)
            .SumAsync(i => (int?)i.Quantity) ?? 0;
}
