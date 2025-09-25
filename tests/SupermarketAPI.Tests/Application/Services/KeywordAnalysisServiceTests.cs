using FluentAssertions;
using SupermarketAPI.Application.Services;
using SupermarketAPI.Domain.Entities;
using Xunit;

namespace SupermarketAPI.Tests.Application.Services
{
    public class KeywordAnalysisServiceTests
    {
        private readonly IKeywordAnalysisService _service;
        private readonly INormalizationService _normalizationService;

        public KeywordAnalysisServiceTests()
        {
            _normalizationService = new NormalizationService();
            _service = new KeywordAnalysisService(_normalizationService);
        }

        [Fact]
        public async Task AnalyzeProductAsync_ShouldAnalyzeProductCorrectly()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Leite UHT 1L Integral Nestlé",
                Category = new Category { Name = "Laticínios" },
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var analysis = await _service.AnalyzeProductAsync(product);

            // Assert
            analysis.Product.Should().Be(product);
            analysis.Keywords.Should().NotBeEmpty();
            analysis.Brands.Should().Contain("Nestlé");
            analysis.Categories.Should().Contain("Laticínios");
            analysis.OverallScore.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task ExtractKeywordsAsync_ShouldExtractRelevantKeywords()
        {
            // Arrange
            var productName = "Leite UHT 1L Integral";

            // Act
            var keywords = await _service.ExtractKeywordsAsync(productName);

            // Assert
            keywords.Should().NotBeEmpty();
            keywords.Should().Contain("leite");
            keywords.Should().Contain("uht");
            keywords.Should().Contain("1l");
            keywords.Should().Contain("integral");
        }

        [Fact]
        public async Task ExtractBrandsAsync_ShouldExtractKnownBrands()
        {
            // Arrange
            var productName = "Leite UHT 1L Nestlé";

            // Act
            var brands = await _service.ExtractBrandsAsync(productName);

            // Assert
            brands.Should().NotBeEmpty();
            brands.Should().Contain("Nestlé");
        }

        [Fact]
        public async Task ExtractCategoriesAsync_ShouldExtractProductCategories()
        {
            // Arrange
            var productName = "Leite UHT 1L Integral";

            // Act
            var categories = await _service.ExtractCategoriesAsync(productName);

            // Assert
            categories.Should().NotBeEmpty();
            categories.Should().Contain("Laticínios");
        }

        [Fact]
        public async Task ExtractFeaturesAsync_ShouldExtractProductFeatures()
        {
            // Arrange
            var productName = "Leite UHT 1L Integral Orgânico";

            // Act
            var features = await _service.ExtractFeaturesAsync(productName);

            // Assert
            features.Should().NotBeEmpty();
            features.Should().Contain("Orgânico");
        }

        [Fact]
        public async Task CalculateRelevanceScoreAsync_ShouldReturnHighScoreForRelevantKeywords()
        {
            // Arrange
            var productName = "Leite UHT 1L Integral";
            var targetKeywords = new List<string> { "leite", "uht", "integral" };

            // Act
            var score = await _service.CalculateRelevanceScoreAsync(productName, targetKeywords);

            // Assert
            score.Should().BeGreaterThan(0.8m);
        }

        [Fact]
        public async Task CalculateRelevanceScoreAsync_ShouldReturnLowScoreForIrrelevantKeywords()
        {
            // Arrange
            var productName = "Leite UHT 1L Integral";
            var targetKeywords = new List<string> { "pão", "cereais", "massas" };

            // Act
            var score = await _service.CalculateRelevanceScoreAsync(productName, targetKeywords);

            // Assert
            score.Should().BeLessThan(0.3m);
        }

        [Fact]
        public async Task FindSimilarProductsByKeywordsAsync_ShouldFindSimilarProducts()
        {
            // Arrange
            var targetProduct = new Product
            {
                Id = 1,
                Name = "Leite UHT 1L Integral",
                CreatedAt = DateTime.UtcNow
            };

            var candidates = new List<Product>
            {
                new Product { Id = 2, Name = "Leite UHT 500ml", CreatedAt = DateTime.UtcNow },
                new Product { Id = 3, Name = "Leite Desnatado 1L", CreatedAt = DateTime.UtcNow },
                new Product { Id = 4, Name = "Pão de Forma", CreatedAt = DateTime.UtcNow },
                new Product { Id = 5, Name = "Leite Condensado", CreatedAt = DateTime.UtcNow }
            };

            // Act
            var similarProducts = await _service.FindSimilarProductsByKeywordsAsync(targetProduct, candidates);

            // Assert
            similarProducts.Should().NotBeEmpty();
            similarProducts.Should().HaveCount(3); // 3 produtos similares
            similarProducts.Should().NotContain(p => p.Name == "Pão de Forma");
        }
    }
}
