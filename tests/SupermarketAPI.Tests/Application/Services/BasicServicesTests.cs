using FluentAssertions;
using SupermarketAPI.Application.Services;
using Xunit;

namespace SupermarketAPI.Tests.Application.Services
{
    public class BasicServicesTests
    {
        [Fact]
        public void NormalizationService_ShouldNormalizeNames()
        {
            // Arrange
            var service = new NormalizationService();
            var input = "Leite UHT 1L";

            // Act
            var result = service.NormalizeName(input);

            // Assert
            result.Should().Be("leite uht 1l");
        }

        [Fact]
        public void NormalizationService_ShouldNormalizeUnits()
        {
            // Arrange
            var service = new NormalizationService();
            var unit = "kg";

            // Act
            var (normalizedUnit, factor) = service.NormalizeUnit(unit);

            // Assert
            normalizedUnit.Should().Be("g");
            factor.Should().Be(1000m);
        }

        [Fact]
        public void MatchingService_ShouldCalculateLevenshteinDistance()
        {
            // Arrange
            var service = new MatchingService();
            var str1 = "kitten";
            var str2 = "sitting";

            // Act
            var distance = service.LevenshteinDistance(str1, str2);

            // Assert
            distance.Should().Be(3);
        }

        [Fact]
        public void MatchingService_ShouldCalculateSimilarity()
        {
            // Arrange
            var service = new MatchingService();
            var str1 = "kitten";
            var str2 = "sitting";

            // Act
            var similarity = service.Similarity(str1, str2);

            // Assert
            similarity.Should().BeGreaterThan(0.5);
            similarity.Should().BeLessThan(1.0);
        }

        [Fact]
        public void MatchingService_ShouldReturnOneForIdenticalStrings()
        {
            // Arrange
            var service = new MatchingService();
            var str = "identical";

            // Act
            var similarity = service.Similarity(str, str);

            // Assert
            similarity.Should().Be(1.0);
        }
    }
}
