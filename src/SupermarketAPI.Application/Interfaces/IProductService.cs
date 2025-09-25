using SupermarketAPI.Application.DTOs;
using SupermarketAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupermarketAPI.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProductsAsync(int skip = 0, int take = 50);
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm, int skip = 0, int take = 50);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId, int skip = 0, int take = 50);
        Task<IEnumerable<ProductDto>> GetProductsWithBestPricesAsync(int categoryId, int take = 10);
        Task<IEnumerable<ProductDto>> GetSimilarProductsAsync(int productId, int take = 5);
        Task<IEnumerable<ProductDto>> CompareProductAsync(int productId);
        Task<IEnumerable<ProductDto>> GetProductsOnSaleAsync(int take = 20);
        Task<IEnumerable<Product>> GetProductsAsync(List<int> productIds);
        Task<IEnumerable<Product>> GetUserFavoritesAsync(int userId);
        Task<IEnumerable<Product>> GetPriceDropsAsync();
        Task<IEnumerable<Product>> GetTopProductsAsync(int take = 10);
    }
}
