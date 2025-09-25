using FluentAssertions;
using SupermarketAPI.Scrapers.Services;
using Xunit;

namespace SupermarketAPI.Tests.Scrapers.Services
{
    public class AntiBotDetectionServiceTests
    {
        private readonly IAntiBotDetectionService _service;
        private readonly IUserAgentRotationService _userAgentService;
        private readonly IProxyRotationService _proxyService;

        public AntiBotDetectionServiceTests()
        {
            _userAgentService = new UserAgentRotationService();
            _proxyService = new ProxyRotationService();
            _service = new AntiBotDetectionService(_userAgentService, _proxyService);
        }

        [Fact]
        public async Task DetectAntiBotMeasuresAsync_ShouldDetectCaptcha()
        {
            // Arrange
            var html = "<html><body><div class='captcha'>Please solve this captcha</div></body></html>";
            var url = "https://example.com";

            // Act
            var isDetected = await _service.DetectAntiBotMeasuresAsync(html, url);

            // Assert
            isDetected.Should().BeTrue();
        }

        [Fact]
        public async Task DetectAntiBotMeasuresAsync_ShouldDetectRateLimiting()
        {
            // Arrange
            var html = "<html><body><div>Rate limit exceeded. Please try again later.</div></body></html>";
            var url = "https://example.com";

            // Act
            var isDetected = await _service.DetectAntiBotMeasuresAsync(html, url);

            // Assert
            isDetected.Should().BeTrue();
        }

        [Fact]
        public async Task DetectAntiBotMeasuresAsync_ShouldDetectBlocking()
        {
            // Arrange
            var html = "<html><body><div>Access denied. Your IP has been blocked.</div></body></html>";
            var url = "https://example.com";

            // Act
            var isDetected = await _service.DetectAntiBotMeasuresAsync(html, url);

            // Assert
            isDetected.Should().BeTrue();
        }

        [Fact]
        public async Task DetectAntiBotMeasuresAsync_ShouldNotDetectNormalPage()
        {
            // Arrange
            var html = "<html><body><div>Welcome to our website</div></body></html>";
            var url = "https://example.com";

            // Act
            var isDetected = await _service.DetectAntiBotMeasuresAsync(html, url);

            // Assert
            isDetected.Should().BeFalse();
        }

        [Fact]
        public async Task IsCaptchaPresentAsync_ShouldDetectVariousCaptchaTypes()
        {
            // Arrange
            var captchaHtmls = new[]
            {
                "<div class='captcha'>Solve this</div>",
                "<div class='recaptcha'>reCAPTCHA</div>",
                "<div class='hcaptcha'>hCaptcha</div>",
                "<div data-sitekey='abc123'>Challenge</div>"
            };

            foreach (var html in captchaHtmls)
            {
                // Act
                var isPresent = await _service.IsCaptchaPresentAsync(html);

                // Assert
                isPresent.Should().BeTrue($"Should detect captcha in: {html}");
            }
        }

        [Fact]
        public async Task IsRateLimitedAsync_ShouldDetectRateLimiting()
        {
            // Arrange
            var rateLimitHtmls = new[]
            {
                "<div>Rate limit exceeded</div>",
                "<div>Too many requests</div>",
                "<div>Slow down</div>",
                "<div>Try again later</div>",
                "<div>429</div>"
            };

            foreach (var html in rateLimitHtmls)
            {
                // Act
                var isRateLimited = await _service.IsRateLimitedAsync(html);

                // Assert
                isRateLimited.Should().BeTrue($"Should detect rate limiting in: {html}");
            }
        }

        [Fact]
        public async Task IsBlockedAsync_ShouldDetectBlocking()
        {
            // Arrange
            var blockedHtmls = new[]
            {
                "<div>Access denied</div>",
                "<div>Blocked</div>",
                "<div>Forbidden</div>",
                "<div>403</div>",
                "<div>IP blocked</div>"
            };

            foreach (var html in blockedHtmls)
            {
                // Act
                var isBlocked = await _service.IsBlockedAsync(html);

                // Assert
                isBlocked.Should().BeTrue($"Should detect blocking in: {html}");
            }
        }

        [Fact]
        public async Task ExtractChallengeTokenAsync_ShouldExtractTokens()
        {
            // Arrange
            var htmlWithToken = "<input name='_token' value='abc123' />";
            var htmlWithoutToken = "<div>No token here</div>";

            // Act
            var token1 = await _service.ExtractChallengeTokenAsync(htmlWithToken);
            var token2 = await _service.ExtractChallengeTokenAsync(htmlWithoutToken);

            // Assert
            token1.Should().Be("abc123");
            token2.Should().BeEmpty();
        }

        [Fact]
        public async Task CalculateDelayAsync_ShouldIncreaseWithRequestCount()
        {
            // Arrange
            var url = "https://example.com";

            // Act
            var delay1 = await _service.CalculateDelayAsync(url, 1);
            var delay10 = await _service.CalculateDelayAsync(url, 10);
            var delay50 = await _service.CalculateDelayAsync(url, 50);

            // Assert
            delay1.Should().BeLessThan(delay10);
            delay10.Should().BeLessThan(delay50);
        }

        [Fact]
        public async Task HandleAntiBotChallengeAsync_ShouldReturnAppropriateResponse()
        {
            // Arrange
            var captchaHtml = "<div class='captcha'>Please solve this captcha</div>";
            var url = "https://example.com";

            // Act
            var response = await _service.HandleAntiBotChallengeAsync(captchaHtml, url);

            // Assert
            response.IsAntiBotDetected.Should().BeTrue();
            response.SuggestedAction.Should().Be(AntiBotAction.WaitAndRetry);
            response.Delay.Should().BeGreaterThan(TimeSpan.Zero);
        }
    }
}
