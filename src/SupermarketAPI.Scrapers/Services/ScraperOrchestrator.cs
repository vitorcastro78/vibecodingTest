using SupermarketAPI.Scrapers.Abstractions;
using SupermarketAPI.Domain.Entities;
using SupermarketAPI.Domain.Enums;
using SupermarketAPI.Infrastructure.Data;
using SupermarketAPI.Infrastructure.Repositories;
using SupermarketAPI.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SupermarketAPI.Scrapers.Services
{
    public class ScraperOrchestrator
    {
        private readonly IEnumerable<IScraper> _scrapers;
        private readonly SupermarketDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public ScraperOrchestrator(
            IEnumerable<IScraper> scrapers,
            SupermarketDbContext context,
            IUnitOfWork unitOfWork)
        {
            _scrapers = scrapers;
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteScrapingAsync()
        {
            var log = new ScrapingLog
            {
                StartedAt = DateTime.UtcNow,
                Status = "InProgress",
                SupermarketId = 1 // ID do supermercado "All"
            };

            try
            {
                _context.ScrapingLogs.Add(log);
                await _context.SaveChangesAsync();

                foreach (var scraper in _scrapers)
                {
                    await ExecuteScraperAsync(scraper);
                }

                log.Status = "Completed";
                log.CompletedAt = DateTime.UtcNow;
                log.Duration = log.CompletedAt - log.StartedAt;
                log.ProductsScraped = await _context.Products.CountAsync();
            }
            catch (Exception ex)
            {
                log.Status = "Failed";
                log.ErrorDetails = ex.Message;
                log.CompletedAt = DateTime.UtcNow;
                log.Duration = log.CompletedAt - log.StartedAt;
            }
            finally
            {
                await _context.SaveChangesAsync();
            }
        }

        private async Task ExecuteScraperAsync(IScraper scraper)
        {
            try
            {
                Console.WriteLine($"Iniciando scraping {scraper.Name}...");
                
                var scrapedProducts = await scraper.ScrapeAsync();
                
                Console.WriteLine($"{scraper.Name} coletou {scrapedProducts.Count} produtos");
                
                // TODO: Implementar lógica de salvamento dos produtos
                foreach (var product in scrapedProducts)
                {
                    Console.WriteLine($"  - {product.Name}: €{product.Price:F2}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no scraper {scraper.Name}: {ex.Message}");
            }
        }
    }
}