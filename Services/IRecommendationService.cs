using NepalMediHub.Models;

namespace NepalMediHub.Services;

/// <summary>Product recommendation seam. See <see cref="ContentBasedRecommendationService"/>.</summary>
public interface IRecommendationService
{
    /// <summary>Returns products related to <paramref name="product"/> (same category, then same type).</summary>
    Task<IReadOnlyList<Product>> GetRelatedAsync(Product product, int count = 4);
}
