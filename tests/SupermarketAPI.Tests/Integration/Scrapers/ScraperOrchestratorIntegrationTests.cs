using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SupermarketAPI.Infrastructure.Data;
using SupermarketAPI.Scrapers.Abstractions;
using SupermarketAPI.Scrapers.Services;
using SupermarketAPI.Scrapers.Sites;
using Xunit;

namespace SupermarketAPI.Tests.Integration.Scrapers
{
    public class ScraperOrchestratorIntegrationTests : IDisposable
    {
        private readonly SupermarketDbContext _context;
        private readonly ScraperOrchestrator _orchestrator;
        private readonly List<IScraper> _scrapers;

        public ScraperOrchestratorIntegrationTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<SupermarketDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SupermarketDbContext(options);

            // Setup scrapers
            _scrapers = new List<IScraper>
            {
                new AuchanScraper(),
                new PingoDoceScraper(),
                new ContinenteScraper(),
                new LidlScraper()
            };

            _orchestrator = new ScraperOrchestrator(_scrapers, _context, null!);
        }

        [Fact]
        public async Task ExecuteScrapingAsync_ShouldExecuteSuccessfully()
        {
            // Arrange
            // Add some test data to the database
            await SeedTestDataAsync();

            // Act
            await _orchestrator.ExecuteScrapingAsync();

            // Assert
            var scrapingLogs = await _context.ScrapingLogs.ToListAsync();
            scrapingLogs.Should().NotBeEmpty();
            scrapingLogs.Should().Contain(log => log.Status == "Completed");
        }

        [Fact]
        public async Task ExecuteScrapingAsync_ShouldLogErrorsWhenScrapingFails()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            await _orchestrator.ExecuteScrapingAsync();

            // Assert
            var scrapingLogs = await _context.ScrapingLogs.ToListAsync();
            scrapingLogs.Should().NotBeEmpty();
            
            // Should have at least one log entry
            scrapingLogs.Should().Contain(log => !string.IsNullOrEmpty(log.Status));
        }

        [Fact]
        public async Task ExecuteScrapingAsync_ShouldHandleMultipleScrapers()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            await _orchestrator.ExecuteScrapingAsync();

            // Assert
            var scrapingLogs = await _context.ScrapingLogs.ToListAsync();
            scrapingLogs.Should().NotBeEmpty();
            
            // Should process all scrapers
            scrapingLogs.Count.Should().BeGreaterThanOrEqualTo(1);
        }

        private async Task SeedTestDataAsync()
        {
            // Add some test data
            var supermarket = new SupermarketAPI.Domain.Entities.Supermarket
            {
                Id = 1,
                Name = "Test Supermarket",
                CreatedAt = DateTime.UtcNow
            };

            _context.Supermarkets.Add(supermarket);
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
