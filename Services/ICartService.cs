using NepalMediHub.Models;
using NepalMediHub.ViewModels;

namespace NepalMediHub.Services;

public interface ICartService
{
    Task<Cart> GetOrCreateCartAsync(string userId);
    Task<CartViewModel> GetCartAsync(string userId);
    Task<CartActionResult> AddAsync(string userId, int productId, int quantity);
    Task UpdateQuantityAsync(string userId, int productId, int quantity);
    Task RemoveAsync(string userId, int productId);
    Task ClearAsync(string userId);
    Task<int> GetItemCountAsync(string userId);
}
