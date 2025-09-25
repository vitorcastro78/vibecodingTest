using FluentAssertions;
using SupermarketAPI.Scrapers.Abstractions;
using SupermarketAPI.Scrapers.Sites;
using SupermarketAPI.Scrapers.Services;
using Xunit;

namespace SupermarketAPI.Tests.Integration.Scrapers
{
    public class AuchanScraperIntegrationTests
    {
        private readonly AuchanScraper _scraper;

        public AuchanScraperIntegrationTests()
        {
            _scraper = new AuchanScraper();
        }

        [Fact]
        public async Task ScrapeAsync_ShouldReturnProducts()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Act
            var products = await _scraper.ScrapeAsync(cancellationToken);

            // Assert
            products.Should().NotBeNull();
            products.Should().NotBeEmpty();
            products.Should().AllSatisfy(p => p.Name.Should().NotBeNullOrEmpty());
            products.Should().AllSatisfy(p => p.Price.Should().BeGreaterThan(0));
        }

        [Fact]
        public async Task ScrapeAsync_ShouldReturnProductsWithValidData()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Act
            var products = await _scraper.ScrapeAsync(cancellationToken);

            // Assert
            products.Should().AllSatisfy(p => 
            {
                p.Name.Should().NotBeNullOrEmpty();
                p.NormalizedName.Should().NotBeNullOrEmpty();
                p.Price.Should().BeGreaterThan(0);
                p.Unit.Should().NotBeNullOrEmpty();
                p.IsAvailable.Should().BeTrue();
            });
        }

        [Fact]
        public async Task ScrapeAsync_ShouldHandleCancellation()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act & Assert
            var products = await _scraper.ScrapeAsync(cancellationTokenSource.Token);
            products.Should().NotBeNull();
        }

        [Fact]
        public void Name_ShouldReturnCorrectName()
        {
            // Act & Assert
            _scraper.Name.Should().Be("Auchan");
        }

    }
}
