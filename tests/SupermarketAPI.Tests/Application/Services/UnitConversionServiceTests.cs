using FluentAssertions;
using SupermarketAPI.Application.Services;
using SupermarketAPI.Domain.Entities;
using Xunit;

namespace SupermarketAPI.Tests.Application.Services
{
    public class UnitConversionServiceTests
    {
        private readonly IUnitConversionService _service;

        public UnitConversionServiceTests()
        {
            _service = new UnitConversionService();
        }

        [Fact]
        public async Task ConvertToStandardUnitAsync_ShouldConvertKgToG()
        {
            // Arrange
            var productName = "Arroz 1kg";
            var price = 2.50m;
            var unit = "kg";

            // Act
            var result = await _service.ConvertToStandardUnitAsync(productName, price, unit);

            // Assert
            result.ConvertedPrice.Should().Be(0.0025m); // 2.50 / 1000
            result.StandardUnit.Should().Be("g");
            result.IsConverted.Should().BeTrue();
        }

        [Fact]
        public async Task ConvertToStandardUnitAsync_ShouldConvertLToMl()
        {
            // Arrange
            var productName = "Leite 1L";
            var price = 1.20m;
            var unit = "l";

            // Act
            var result = await _service.ConvertToStandardUnitAsync(productName, price, unit);

            // Assert
            result.ConvertedPrice.Should().Be(0.0012m); // 1.20 / 1000
            result.StandardUnit.Should().Be("ml");
            result.IsConverted.Should().BeTrue();
        }

        [Fact]
        public async Task ExtractUnitsFromNameAsync_ShouldExtractWeightUnits()
        {
            // Arrange
            var productName = "Arroz 1kg 500g";

            // Act
            var units = await _service.ExtractUnitsFromNameAsync(productName);

            // Assert
            units.Should().HaveCount(2);
            units.Should().Contain(u => u.Quantity == 1 && u.Unit == "kg");
            units.Should().Contain(u => u.Quantity == 500 && u.Unit == "g");
        }

        [Fact]
        public async Task ExtractUnitsFromNameAsync_ShouldExtractVolumeUnits()
        {
            // Arrange
            var productName = "Leite 1L 500ml";

            // Act
            var units = await _service.ExtractUnitsFromNameAsync(productName);

            // Assert
            units.Should().HaveCount(2);
            units.Should().Contain(u => u.Quantity == 1 && u.Unit == "l");
            units.Should().Contain(u => u.Quantity == 500 && u.Unit == "ml");
        }

        [Fact]
        public async Task ConvertPriceAsync_ShouldConvertBetweenSameCategory()
        {
            // Arrange
            var price = 2.50m;
            var fromUnit = "kg";
            var toUnit = "g";

            // Act
            var convertedPrice = await _service.ConvertPriceAsync(price, fromUnit, toUnit);

            // Assert
            convertedPrice.Should().Be(0.0025m); // 2.50 / 1000
        }

        [Fact]
        public async Task ConvertPriceAsync_ShouldNotConvertBetweenDifferentCategories()
        {
            // Arrange
            var price = 2.50m;
            var fromUnit = "kg";
            var toUnit = "l";

            // Act
            var convertedPrice = await _service.ConvertPriceAsync(price, fromUnit, toUnit);

            // Assert
            convertedPrice.Should().Be(2.50m); // Não converte entre categorias diferentes
        }

        [Fact]
        public async Task AreUnitsComparableAsync_ShouldReturnTrueForSameCategory()
        {
            // Arrange
            var unit1 = "kg";
            var unit2 = "g";

            // Act
            var areComparable = await _service.AreUnitsComparableAsync(unit1, unit2);

            // Assert
            areComparable.Should().BeTrue();
        }

        [Fact]
        public async Task AreUnitsComparableAsync_ShouldReturnFalseForDifferentCategories()
        {
            // Arrange
            var unit1 = "kg";
            var unit2 = "l";

            // Act
            var areComparable = await _service.AreUnitsComparableAsync(unit1, unit2);

            // Assert
            areComparable.Should().BeFalse();
        }

        [Fact]
        public async Task StandardizeProductAsync_ShouldStandardizeProduct()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Arroz 1kg",
                AveragePrice = 2.50m,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var standardized = await _service.StandardizeProductAsync(product);

            // Assert
            standardized.Product.Should().Be(product);
            standardized.StandardPrice.Should().Be(2.50m);
            standardized.StandardUnit.Should().Be("un");
            standardized.IsStandardized.Should().BeFalse(); // Não foi convertido
        }
    }
}
