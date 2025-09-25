using SupermarketAPI.Application.Services;
using Xunit;

namespace SupermarketAPI.Tests.Services
{
    public class MatchingServiceTests
    {
        private readonly IMatchingService _service;

        public MatchingServiceTests()
        {
            _service = new MatchingService();
        }

        [Fact]
        public void LevenshteinDistance_ShouldReturnZeroForEqualStrings()
        {
            var result = _service.LevenshteinDistance("test", "test");
            Assert.Equal(0, result);
        }

        [Fact]
        public void LevenshteinDistance_ShouldReturnCorrectDistance()
        {
            var result = _service.LevenshteinDistance("kitten", "sitting");
            Assert.Equal(3, result);
        }

        [Fact]
        public void Similarity_ShouldReturnOneForEqualStrings()
        {
            var result = _service.Similarity("test", "test");
            Assert.Equal(1.0, result);
        }

        [Fact]
        public void Similarity_ShouldReturnCorrectSimilarity()
        {
            var result = _service.Similarity("kitten", "sitting");
            Assert.True(result > 0.5);
        }
    }
}
