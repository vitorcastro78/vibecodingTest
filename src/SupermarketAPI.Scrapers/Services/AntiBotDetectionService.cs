using SupermarketAPI.Scrapers.Services;
using System.Text.RegularExpressions;

namespace SupermarketAPI.Scrapers.Services
{
    public interface IAntiBotDetectionService
    {
        Task<bool> DetectAntiBotMeasuresAsync(string html, string url);
        Task<AntiBotResponse> HandleAntiBotChallengeAsync(string html, string url);
        Task<bool> IsCaptchaPresentAsync(string html);
        Task<bool> IsRateLimitedAsync(string html);
        Task<bool> IsBlockedAsync(string html);
        Task<string> ExtractChallengeTokenAsync(string html);
        Task<TimeSpan> CalculateDelayAsync(string url, int requestCount);
    }

    public class AntiBotDetectionService : IAntiBotDetectionService
    {
        private readonly IUserAgentRotationService _userAgentService;
        private readonly IProxyRotationService _proxyService;
        private readonly Dictionary<string, int> _requestCounts;
        private readonly Dictionary<string, DateTime> _lastRequestTimes;

        public AntiBotDetectionService(
            IUserAgentRotationService userAgentService,
            IProxyRotationService proxyService)
        {
            _userAgentService = userAgentService;
            _proxyService = proxyService;
            _requestCounts = new Dictionary<string, int>();
            _lastRequestTimes = new Dictionary<string, DateTime>();
        }

        public async Task<bool> DetectAntiBotMeasuresAsync(string html, string url)
        {
            var domain = ExtractDomain(url);
            
            // Verificar múltiplos indicadores
            var indicators = new List<bool>
            {
                await IsCaptchaPresentAsync(html),
                await IsRateLimitedAsync(html),
                await IsBlockedAsync(html),
                IsCloudflareChallenge(html),
                IsBotDetectionPresent(html),
                IsJavaScriptChallenge(html)
            };

            return indicators.Any(i => i);
        }

        public async Task<AntiBotResponse> HandleAntiBotChallengeAsync(string html, string url)
        {
            var response = new AntiBotResponse
            {
                IsAntiBotDetected = await DetectAntiBotMeasuresAsync(html, url),
                SuggestedAction = AntiBotAction.Continue,
                Delay = TimeSpan.Zero,
                NewUserAgent = null,
                NewProxy = null
            };

            if (!response.IsAntiBotDetected)
            {
                return response;
            }

            // Determinar tipo de proteção e ação apropriada
            if (await IsCaptchaPresentAsync(html))
            {
                response.SuggestedAction = AntiBotAction.WaitAndRetry;
                response.Delay = TimeSpan.FromMinutes(5);
                response.NewUserAgent = _userAgentService.GetRandomUserAgent();
            }
            else if (await IsRateLimitedAsync(html))
            {
                response.SuggestedAction = AntiBotAction.WaitAndRetry;
                response.Delay = await CalculateDelayAsync(url, GetRequestCount(url));
                response.NewUserAgent = _userAgentService.GetNextUserAgent();
            }
            else if (IsCloudflareChallenge(html))
            {
                response.SuggestedAction = AntiBotAction.ChangeProxy;
                response.Delay = TimeSpan.FromSeconds(30);
                response.NewProxy = _proxyService.GetProxyHandler();
            }
            else if (IsBotDetectionPresent(html))
            {
                response.SuggestedAction = AntiBotAction.ChangeUserAgent;
                response.Delay = TimeSpan.FromSeconds(10);
                response.NewUserAgent = _userAgentService.GetRandomUserAgent();
            }

            return response;
        }

        public async Task<bool> IsCaptchaPresentAsync(string html)
        {
            var captchaPatterns = new[]
            {
                @"captcha",
                @"recaptcha",
                @"hcaptcha",
                @"challenge",
                @"verify.*human",
                @"prove.*not.*robot",
                @"data-sitekey",
                @"g-recaptcha"
            };

            return await Task.Run(() => captchaPatterns.Any(pattern => 
                Regex.IsMatch(html, pattern, RegexOptions.IgnoreCase)));
        }

        public async Task<bool> IsRateLimitedAsync(string html)
        {
            var rateLimitPatterns = new[]
            {
                @"rate.*limit",
                @"too.*many.*requests",
                @"slow.*down",
                @"try.*again.*later",
                @"429",
                @"quota.*exceeded",
                @"throttled"
            };

            return await Task.Run(() => rateLimitPatterns.Any(pattern => 
                Regex.IsMatch(html, pattern, RegexOptions.IgnoreCase)));
        }

        public async Task<bool> IsBlockedAsync(string html)
        {
            var blockPatterns = new[]
            {
                @"access.*denied",
                @"blocked",
                @"forbidden",
                @"403",
                @"ip.*blocked",
                @"suspicious.*activity",
                @"security.*check"
            };

            return await Task.Run(() => blockPatterns.Any(pattern => 
                Regex.IsMatch(html, pattern, RegexOptions.IgnoreCase)));
        }

        public async Task<string> ExtractChallengeTokenAsync(string html)
        {
            var tokenPatterns = new[]
            {
                // input hidden com aspas duplas
                "name=\"_token\"\\s+value=\"([^\\\"]+)\"",
                // input hidden com aspas simples
                "name=\\'_token\\'\\s+value=\\'([^\\']+)\\'",
                "name=\"csrf_token\"\\s+value=\"([^\\\"]+)\"",
                "name=\"authenticity_token\"\\s+value=\"([^\\\"]+)\"",
                "data-token=\"([^\\\"]+)\""
            };

            foreach (var pattern in tokenPatterns)
            {
                var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            // Fallback JSON: token: "abc123" ou "token":"abc123"
            var jsonMatch = Regex.Match(html, "token\\s*[:=]\\s*\\\"([^\\\"]+)\\\"", RegexOptions.IgnoreCase);
            if (jsonMatch.Success) return jsonMatch.Groups[1].Value;

            // Fallback meta
            var metaMatch = Regex.Match(html, "<meta\\s+name=\\\"csrf-token\\\"\\s+content=\\\"([^\\\"]+)\\\"", RegexOptions.IgnoreCase);
            if (metaMatch.Success) return metaMatch.Groups[1].Value;

            return string.Empty;
        }

        public async Task<TimeSpan> CalculateDelayAsync(string url, int requestCount)
        {
            var domain = ExtractDomain(url);
            var baseDelay = TimeSpan.FromSeconds(2);
            
            // Aumentar delay baseado no número de requests
            var multiplier = Math.Min(requestCount / 10, 5); // Máximo 5x o delay base
            var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * (1 + multiplier));
            
            // Delay aleatório para parecer mais humano
            var randomFactor = Random.Shared.NextDouble() * 0.5 + 0.75; // 75% a 125% do delay
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * randomFactor);
            
            return delay;
        }

        private bool IsCloudflareChallenge(string html)
        {
            var cloudflarePatterns = new[]
            {
                @"cloudflare",
                @"cf-ray",
                @"checking.*browser",
                @"ddos.*protection",
                @"security.*check"
            };

            return cloudflarePatterns.Any(pattern => 
                Regex.IsMatch(html, pattern, RegexOptions.IgnoreCase));
        }

        private bool IsBotDetectionPresent(string html)
        {
            var botDetectionPatterns = new[]
            {
                @"bot.*detection",
                @"anti.*bot",
                @"honeypot",
                @"trap.*field",
                @"invisible.*field",
                @"behavior.*analysis"
            };

            return botDetectionPatterns.Any(pattern => 
                Regex.IsMatch(html, pattern, RegexOptions.IgnoreCase));
        }

        private bool IsJavaScriptChallenge(string html)
        {
            // Verificar se há JavaScript que precisa ser executado
            var jsPatterns = new[]
            {
                @"eval\(",
                @"setTimeout\(",
                @"document\.write",
                @"window\.location",
                @"challenge.*javascript",
                @"js.*challenge"
            };

            return jsPatterns.Any(pattern => 
                Regex.IsMatch(html, pattern, RegexOptions.IgnoreCase));
        }

        private string ExtractDomain(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host;
            }
            catch
            {
                return "unknown";
            }
        }

        private int GetRequestCount(string url)
        {
            var domain = ExtractDomain(url);
            return _requestCounts.GetValueOrDefault(domain, 0);
        }

        private void IncrementRequestCount(string url)
        {
            var domain = ExtractDomain(url);
            _requestCounts[domain] = GetRequestCount(url) + 1;
            _lastRequestTimes[domain] = DateTime.UtcNow;
        }
    }

    public class AntiBotResponse
    {
        public bool IsAntiBotDetected { get; set; }
        public AntiBotAction SuggestedAction { get; set; }
        public TimeSpan Delay { get; set; }
        public string? NewUserAgent { get; set; }
        public HttpClientHandler? NewProxy { get; set; }
        public string? ChallengeToken { get; set; }
    }

    public enum AntiBotAction
    {
        Continue,
        WaitAndRetry,
        ChangeUserAgent,
        ChangeProxy,
        SolveCaptcha,
        Abort
    }
}
