using System;
using System.Collections.Generic;
using System.Linq;

namespace SupermarketAPI.Scrapers.Services
{
    public interface IUserAgentRotationService
    {
        string GetRandomUserAgent();
        string GetNextUserAgent();
    }

    public class UserAgentRotationService : IUserAgentRotationService
    {
        private readonly List<string> _userAgents;
        private int _currentIndex = 0;
        private readonly Random _random;

        public UserAgentRotationService()
        {
            _random = new Random();
            _userAgents = new List<string>
            {
                // Chrome
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                
                // Firefox
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:120.0) Gecko/20100101 Firefox/120.0",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:120.0) Gecko/20100101 Firefox/120.0",
                
                // Safari
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Safari/605.1.15",
                
                // Edge
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0",
                
                // Mobile
                "Mozilla/5.0 (iPhone; CPU iPhone OS 17_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Linux; Android 14; SM-G998B) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36"
            };
        }

        public string GetRandomUserAgent()
        {
            return _userAgents[_random.Next(_userAgents.Count)];
        }

        public string GetNextUserAgent()
        {
            var userAgent = _userAgents[_currentIndex];
            _currentIndex = (_currentIndex + 1) % _userAgents.Count;
            return userAgent;
        }
    }
}
