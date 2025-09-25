using SupermarketAPI.Application.Services;
using Xunit;

namespace SupermarketAPI.Tests.Services
{
    public class NormalizationServiceTests
    {
        private readonly INormalizationService _service;

        public NormalizationServiceTests()
        {
            _service = new NormalizationService();
        }

        [Fact]
        public void NormalizeName_ShouldRemoveAccents()
        {
            var input = "Leite UHT 1L";
            var result = _service.NormalizeName(input);
            Assert.Equal("leite uht 1l", result);
        }

        [Fact]
        public void NormalizeName_ShouldHandleEmptyString()
        {
            var result = _service.NormalizeName("");
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void NormalizeUnit_ShouldConvertKgToG()
        {
            var (unit, factor) = _service.NormalizeUnit("kg");
            Assert.Equal("g", unit);
            Assert.Equal(1000m, factor);
        }

        [Fact]
        public void NormalizeUnit_ShouldConvertLToMl()
        {
            var (unit, factor) = _service.NormalizeUnit("l");
            Assert.Equal("ml", unit);
            Assert.Equal(1000m, factor);
        }
    }
}
