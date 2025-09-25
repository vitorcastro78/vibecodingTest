using Microsoft.EntityFrameworkCore;
using SupermarketAPI.Application.DTOs;
using SupermarketAPI.Application.Interfaces;
using SupermarketAPI.Infrastructure.Data;

namespace SupermarketAPI.Application.Services
{
    public class RankingService : IRankingService
    {
        private readonly SupermarketDbContext _db;

        public RankingService(SupermarketDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<RankingDto>> GetDailyRankingsAsync(DateTime? date = null)
        {
            var d = (date ?? DateTime.UtcNow).Date;
            return await _db.DailyRankings.AsNoTracking()
                .Where(r => r.Date.Date == d)
                .Include(r => r.Category)
                .Select(r => new RankingDto
                {
                    Id = r.Id,
                    Date = r.Date,
                    CategoryId = r.CategoryId,
                    CategoryName = r.Category!.Name,
                    TotalProducts = r.TotalProducts,
                    AveragePrice = r.AveragePrice,
                    MinPrice = r.MinPrice,
                    MaxPrice = r.MaxPrice,
                    SupermarketsCount = r.SupermarketsCount
                })
                .ToListAsync();
        }

        public async Task<RankingDto?> GetRankingByCategoryAsync(int categoryId, DateTime? date = null)
        {
            var d = (date ?? DateTime.UtcNow).Date;
            var r = await _db.DailyRankings.AsNoTracking()
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.CategoryId == categoryId && x.Date.Date == d);
            if (r == null) return null;
            return new RankingDto
            {
                Id = r.Id,
                Date = r.Date,
                CategoryId = r.CategoryId,
                CategoryName = r.Category?.Name ?? string.Empty,
                TotalProducts = r.TotalProducts,
                AveragePrice = r.AveragePrice,
                MinPrice = r.MinPrice,
                MaxPrice = r.MaxPrice,
                SupermarketsCount = r.SupermarketsCount
            };
        }

        public Task<IEnumerable<RankingProductDto>> GetTopProductsByCategoryAsync(int categoryId, int take = 10)
        {
            // Simplificado: buscar melhores preços atuais
            var d = DateTime.UtcNow.Date;
            var query = from p in _db.Products
                        where p.CategoryId == categoryId && p.IsActive
                        select new RankingProductDto
                        {
                            ProductId = p.Id,
                            ProductName = p.Name,
                            ProductImageUrl = p.ImageUrl,
                            BestPrice = _db.ProductPrices
                                .Where(pp => pp.ProductId == p.Id && pp.IsAvailable)
                                .Select(pp => pp.Price)
                                .DefaultIfEmpty(decimal.MaxValue)
                                .Min(),
                            BestPriceSupermarket = _db.ProductPrices
                                .Where(pp => pp.ProductId == p.Id && pp.IsAvailable &&
                                            pp.Price == _db.ProductPrices.Where(x => x.ProductId == p.Id && x.IsAvailable).Select(x => x.Price).DefaultIfEmpty(decimal.MaxValue).Min())
                                .Select(pp => pp.Supermarket.Name)
                                .FirstOrDefault() ?? string.Empty,
                            AveragePrice = p.AveragePrice ?? 0,
                            Savings = 0,
                            SavingsPercentage = 0,
                            SupermarketsCount = _db.ProductPrices.Where(pp => pp.ProductId == p.Id && pp.IsAvailable).Select(pp => pp.SupermarketId).Distinct().Count()
                        };
            return Task.FromResult<IEnumerable<RankingProductDto>>(query.Take(take).ToList());
        }

        public Task<IEnumerable<RankingProductDto>> GetBestDealsAsync(int take = 20)
        {
            var query = from pp in _db.ProductPrices
                        where pp.IsOnSale && pp.IsAvailable
                        orderby (pp.OriginalPrice ?? pp.Price) - pp.Price descending
                        select new RankingProductDto
                        {
                            ProductId = pp.ProductId,
                            ProductName = pp.Product.Name,
                            ProductImageUrl = pp.Product.ImageUrl,
                            BestPrice = pp.Price,
                            BestPriceSupermarket = pp.Supermarket.Name,
                            AveragePrice = pp.Product.AveragePrice ?? 0,
                            Savings = (pp.OriginalPrice ?? pp.Price) - pp.Price,
                            SavingsPercentage = pp.OriginalPrice.HasValue && pp.OriginalPrice.Value > 0 ? ((pp.OriginalPrice.Value - pp.Price) / pp.OriginalPrice.Value) * 100 : 0,
                            SupermarketsCount = _db.ProductPrices.Where(x => x.ProductId == pp.ProductId && x.IsAvailable).Select(x => x.SupermarketId).Distinct().Count()
                        };
            return Task.FromResult<IEnumerable<RankingProductDto>>(query.Take(take).ToList());
        }

        public Task<IEnumerable<RankingProductDto>> GetPriceDropsAsync(decimal thresholdPercentage = 10, int take = 20)
        {
            var query = from pp in _db.ProductPrices
                        where pp.IsOnSale && pp.IsAvailable && pp.OriginalPrice.HasValue &&
                              ((pp.OriginalPrice.Value - pp.Price) / pp.OriginalPrice.Value) * 100 >= thresholdPercentage
                        orderby ((pp.OriginalPrice.Value - pp.Price) / pp.OriginalPrice.Value) * 100 descending
                        select new RankingProductDto
                        {
                            ProductId = pp.ProductId,
                            ProductName = pp.Product.Name,
                            ProductImageUrl = pp.Product.ImageUrl,
                            BestPrice = pp.Price,
                            BestPriceSupermarket = pp.Supermarket.Name,
                            AveragePrice = pp.Product.AveragePrice ?? 0,
                            Savings = (pp.OriginalPrice.Value - pp.Price),
                            SavingsPercentage = ((pp.OriginalPrice.Value - pp.Price) / pp.OriginalPrice.Value) * 100,
                            SupermarketsCount = _db.ProductPrices.Where(x => x.ProductId == pp.ProductId && x.IsAvailable).Select(x => x.SupermarketId).Distinct().Count()
                        };
            return Task.FromResult<IEnumerable<RankingProductDto>>(query.Take(take).ToList());
        }

        public async Task<RankingDto> GenerateDailyRankingAsync(int categoryId)
        {
            var d = DateTime.UtcNow.Date;
            var prices = await _db.ProductPrices
                .Where(pp => pp.Product.CategoryId == categoryId && pp.ScrapedAt.Date == d && pp.IsAvailable)
                .ToListAsync();

            var totalProducts = prices.Select(p => p.ProductId).Distinct().Count();
            var avg = prices.Any() ? prices.Average(p => p.Price) : 0;
            var min = prices.Any() ? prices.Min(p => p.Price) : 0;
            var max = prices.Any() ? prices.Max(p => p.Price) : 0;
            var supermarketsCount = prices.Select(p => p.SupermarketId).Distinct().Count();

            var existing = await _db.DailyRankings.FirstOrDefaultAsync(r => r.CategoryId == categoryId && r.Date.Date == d);
            if (existing == null)
            {
                existing = new Domain.Entities.DailyRanking
                {
                    Date = d,
                    CategoryId = categoryId,
                    RankingData = "{}",
                    TotalProducts = totalProducts,
                    AveragePrice = avg,
                    MinPrice = min,
                    MaxPrice = max,
                    SupermarketsCount = supermarketsCount,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _db.DailyRankings.Add(existing);
            }
            else
            {
                existing.TotalProducts = totalProducts;
                existing.AveragePrice = avg;
                existing.MinPrice = min;
                existing.MaxPrice = max;
                existing.SupermarketsCount = supermarketsCount;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            return await GetRankingByCategoryAsync(categoryId, d) ?? new RankingDto
            {
                Date = d,
                CategoryId = categoryId
            };
        }

        public async Task<bool> GenerateAllRankingsAsync()
        {
            var categories = await _db.Categories.Select(c => c.Id).ToListAsync();
            foreach (var c in categories)
            {
                await GenerateDailyRankingAsync(c);
            }
            return true;
        }
    }
}


