using Microsoft.EntityFrameworkCore;
using SupermarketAPI.Domain.Entities;
using SupermarketAPI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupermarketAPI.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>
    {
        public ProductRepository(SupermarketDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, int skip = 0, int take = 50)
        {
            return await _dbSet
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.ProductPrices.Where(pp => pp.IsAvailable))
                .ThenInclude(pp => pp.Supermarket)
                .OrderBy(p => p.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, int skip = 0, int take = 50)
        {
            var normalizedSearchTerm = searchTerm.ToLower();
            
            return await _dbSet
                .Where(p => p.IsActive && 
                    (p.Name.ToLower().Contains(normalizedSearchTerm) ||
                     p.NormalizedName.ToLower().Contains(normalizedSearchTerm) ||
                     p.Brand.ToLower().Contains(normalizedSearchTerm) ||
                     p.Keywords.ToLower().Contains(normalizedSearchTerm)))
                .Include(p => p.Category)
                .Include(p => p.ProductPrices.Where(pp => pp.IsAvailable))
                .ThenInclude(pp => pp.Supermarket)
                .OrderBy(p => p.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsWithBestPricesAsync(int categoryId, int take = 10)
        {
            return await _dbSet
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .Include(p => p.ProductPrices.Where(pp => pp.IsAvailable))
                .ThenInclude(pp => pp.Supermarket)
                .OrderBy(p => p.ProductPrices.Where(pp => pp.IsAvailable).Min(pp => pp.Price))
                .Take(take)
                .ToListAsync();
        }

        public async Task<Product?> GetProductWithPricesAsync(int productId)
        {
            return await _dbSet
                .Where(p => p.Id == productId && p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.ProductPrices.Where(pp => pp.IsAvailable))
                .ThenInclude(pp => pp.Supermarket)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Product>> GetSimilarProductsAsync(int productId, int take = 5)
        {
            var product = await GetByIdAsync(productId);
            if (product == null) return new List<Product>();

            return await _dbSet
                .Where(p => p.Id != productId && 
                           p.CategoryId == product.CategoryId && 
                           p.IsActive)
                .Include(p => p.ProductPrices.Where(pp => pp.IsAvailable))
                .ThenInclude(pp => pp.Supermarket)
                .OrderBy(p => p.Name)
                .Take(take)
                .ToListAsync();
        }
    }
}
