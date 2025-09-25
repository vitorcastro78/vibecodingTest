using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupermarketAPI.Infrastructure.Data;

namespace SupermarketAPI.API.Services
{
    public class AnalyticsUpdateService
    {
        private readonly SupermarketDbContext _db;
        private readonly ILogger<AnalyticsUpdateService> _logger;

        public AnalyticsUpdateService(SupermarketDbContext db, ILogger<AnalyticsUpdateService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task UpdateMetricsAsync()
        {
            try
            {
                _logger.LogInformation("[Analytics] Atualizando métricas...");

                // Exemplo: atualizar média/min/max dos produtos
                var products = await _db.Products.ToListAsync();
                foreach (var p in products)
                {
                    var prices = await _db.ProductPrices
                        .Where(pp => pp.ProductId == p.Id && pp.IsAvailable)
                        .Select(pp => pp.Price)
                        .ToListAsync();
                    if (prices.Count > 0)
                    {
                        p.AveragePrice = prices.Average();
                        p.MinPrice = prices.Min();
                        p.MaxPrice = prices.Max();
                        p.LastPriceUpdate = DateTime.UtcNow;
                    }
                }

                await _db.SaveChangesAsync();
                _logger.LogInformation("[Analytics] Métricas atualizadas.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Analytics] Erro ao atualizar métricas");
            }
        }
    }
}


