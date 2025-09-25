using Microsoft.EntityFrameworkCore;
using SupermarketAPI.Domain.Entities;
using SupermarketAPI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupermarketAPI.Infrastructure.Repositories
{
    public class ProductPriceRepository : Repository<ProductPrice>
    {
        public ProductPriceRepository(SupermarketDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ProductPrice>> GetProductPricesAsync(int productId)
        {
            return await _dbSet
                .Where(pp => pp.ProductId == productId && pp.IsAvailable)
                .Include(pp => pp.Supermarket)
                .Include(pp => pp.Product)
                .OrderBy(pp => pp.Price)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductPrice>> GetLatestPricesByProductAsync(int productId)
        {
            var latestScrapedAt = await _dbSet
                .Where(pp => pp.ProductId == productId)
                .MaxAsync(pp => pp.ScrapedAt);

            return await _dbSet
                .Where(pp => pp.ProductId == productId && 
                           pp.ScrapedAt == latestScrapedAt && 
                           pp.IsAvailable)
                .Include(pp => pp.Supermarket)
                .Include(pp => pp.Product)
                .OrderBy(pp => pp.Price)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductPrice>> GetPriceHistoryAsync(int productId, int supermarketId, DateTime fromDate)
        {
            return await _dbSet
                .Where(pp => pp.ProductId == productId && 
                           pp.SupermarketId == supermarketId && 
                           pp.ScrapedAt >= fromDate)
                .Include(pp => pp.Supermarket)
                .OrderBy(pp => pp.ScrapedAt)
                .ToListAsync();
        }

        public async Task<ProductPrice?> GetBestPriceAsync(int productId)
        {
            return await _dbSet
                .Where(pp => pp.ProductId == productId && pp.IsAvailable)
                .Include(pp => pp.Supermarket)
                .Include(pp => pp.Product)
                .OrderBy(pp => pp.Price)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ProductPrice>> GetProductsOnSaleAsync(int take = 20)
        {
            return await _dbSet
                .Where(pp => pp.IsOnSale && pp.IsAvailable)
                .Include(pp => pp.Supermarket)
                .Include(pp => pp.Product)
                .ThenInclude(p => p.Category)
                .OrderByDescending(pp => pp.ScrapedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<decimal> GetAveragePriceAsync(int productId)
        {
            return await _dbSet
                .Where(pp => pp.ProductId == productId && pp.IsAvailable)
                .AverageAsync(pp => pp.Price);
        }

        public async Task<IEnumerable<ProductPrice>> GetPriceDropsAsync(decimal thresholdPercentage, DateTime fromDate)
        {
            // Esta é uma implementação simplificada
            // Em produção, seria necessário comparar preços históricos
            return await _dbSet
                .Where(pp => pp.IsAvailable && pp.ScrapedAt >= fromDate)
                .Include(pp => pp.Supermarket)
                .Include(pp => pp.Product)
                .ToListAsync();
        }
    }
}
