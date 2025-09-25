using FluentAssertions;
using SupermarketAPI.Scrapers.Abstractions;
using SupermarketAPI.Scrapers.Sites;
using Xunit;

namespace SupermarketAPI.Tests.Integration.Scrapers
{
    public class BasicScraperTests
    {
        [Fact]
        public void AuchanScraper_ShouldHaveCorrectName()
        {
            // Arrange
            var scraper = new AuchanScraper();

            // Act & Assert
            scraper.Name.Should().Be("Auchan");
        }

        [Fact]
        public void PingoDoceScraper_ShouldHaveCorrectName()
        {
            // Arrange
            var scraper = new PingoDoceScraper();

            // Act & Assert
            scraper.Name.Should().Be("Pingo Doce");
        }

        [Fact]
        public void ContinenteScraper_ShouldHaveCorrectName()
        {
            // Arrange
            var scraper = new ContinenteScraper();

            // Act & Assert
            scraper.Name.Should().Be("Continente");
        }

        [Fact]
        public void LidlScraper_ShouldHaveCorrectName()
        {
            // Arrange
            var scraper = new LidlScraper();

            // Act & Assert
            scraper.Name.Should().Be("Lidl");
        }

        [Fact]
        public async Task AuchanScraper_ShouldReturnProducts()
        {
            // Arrange
            var scraper = new AuchanScraper();
            var cancellationToken = CancellationToken.None;

            // Act
            var products = await scraper.ScrapeAsync(cancellationToken);

            // Assert
            products.Should().NotBeNull();
            products.Should().NotBeEmpty();
            products.Should().AllSatisfy(p => p.Name.Should().NotBeNullOrEmpty());
        }

        [Fact]
        public async Task PingoDoceScraper_ShouldReturnProducts()
        {
            // Arrange
            var scraper = new PingoDoceScraper();
            var cancellationToken = CancellationToken.None;

            // Act
            var products = await scraper.ScrapeAsync(cancellationToken);

            // Assert
            products.Should().NotBeNull();
            products.Should().NotBeEmpty();
            products.Should().AllSatisfy(p => p.Name.Should().NotBeNullOrEmpty());
        }

        [Fact]
        public async Task ContinenteScraper_ShouldReturnProducts()
        {
            // Arrange
            var scraper = new ContinenteScraper();
            var cancellationToken = CancellationToken.None;

            // Act
            var products = await scraper.ScrapeAsync(cancellationToken);

            // Assert
            products.Should().NotBeNull();
            products.Should().NotBeEmpty();
            products.Should().AllSatisfy(p => p.Name.Should().NotBeNullOrEmpty());
        }

        [Fact]
        public async Task LidlScraper_ShouldReturnProducts()
        {
            // Arrange
            var scraper = new LidlScraper();
            var cancellationToken = CancellationToken.None;

            // Act
            var products = await scraper.ScrapeAsync(cancellationToken);

            // Assert
            products.Should().NotBeNull();
            products.Should().NotBeEmpty();
            products.Should().AllSatisfy(p => p.Name.Should().NotBeNullOrEmpty());
        }
    }
}
