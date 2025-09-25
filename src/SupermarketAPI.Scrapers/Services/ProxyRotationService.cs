using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace SupermarketAPI.Scrapers.Services
{
    public interface IProxyRotationService
    {
        HttpClientHandler GetProxyHandler();
        void MarkProxyAsFailed(string proxy);
        bool HasWorkingProxies();
    }

    public class ProxyRotationService : IProxyRotationService
    {
        private readonly List<string> _proxies;
        private readonly HashSet<string> _failedProxies;
        private int _currentIndex = 0;
        private readonly Random _random;

        public ProxyRotationService()
        {
            _random = new Random();
            _failedProxies = new HashSet<string>();
            
            // Lista de proxies gratuitos (em produção, usar proxies pagos e mais confiáveis)
            _proxies = new List<string>
            {
                // Proxies gratuitos - em produção usar proxies pagos
                "http://proxy1.example.com:8080",
                "http://proxy2.example.com:8080",
                "http://proxy3.example.com:8080"
            };
        }

        public HttpClientHandler GetProxyHandler()
        {
            var workingProxies = _proxies.Where(p => !_failedProxies.Contains(p)).ToList();
            
            if (!workingProxies.Any())
            {
                // Se não há proxies funcionais, retorna handler sem proxy
                return new HttpClientHandler();
            }

            var proxy = workingProxies[_currentIndex % workingProxies.Count];
            _currentIndex++;

            return new HttpClientHandler
            {
                Proxy = new System.Net.WebProxy(proxy),
                UseProxy = true
            };
        }

        public void MarkProxyAsFailed(string proxy)
        {
            _failedProxies.Add(proxy);
        }

        public bool HasWorkingProxies()
        {
            return _proxies.Any(p => !_failedProxies.Contains(p));
        }
    }
}
