using System;
using System.Collections.Generic;

namespace SupermarketAPI.Scrapers.Services
{
    public interface IScrapingConfigurationService
    {
        ScrapingConfig GetConfiguration(string supermarket);
        void UpdateConfiguration(string supermarket, ScrapingConfig config);
    }

    public class ScrapingConfigurationService : IScrapingConfigurationService
    {
        private readonly Dictionary<string, ScrapingConfig> _configurations;

        public ScrapingConfigurationService()
        {
            _configurations = new Dictionary<string, ScrapingConfig>
            {
                ["Auchan"] = new ScrapingConfig
                {
                    BaseUrl = "https://www.auchan.pt",
                    SearchUrl = "https://www.auchan.pt/pt/pesquisa?q={0}",
                    ProductSelector = ".product-item",
                    NameSelector = ".product-name",
                    PriceSelector = ".price",
                    ImageSelector = ".product-image img",
                    LinkSelector = ".product-link",
                    DelayBetweenRequests = TimeSpan.FromSeconds(2),
                    MaxRetries = 3,
                    Timeout = TimeSpan.FromSeconds(30)
                },
                ["PingoDoce"] = new ScrapingConfig
                {
                    BaseUrl = "https://www.pingodoce.pt",
                    SearchUrl = "https://www.pingodoce.pt/produtos?q={0}",
                    ProductSelector = ".product-card",
                    NameSelector = ".product-title",
                    PriceSelector = ".price-current",
                    ImageSelector = ".product-image img",
                    LinkSelector = ".product-link",
                    DelayBetweenRequests = TimeSpan.FromSeconds(3),
                    MaxRetries = 3,
                    Timeout = TimeSpan.FromSeconds(30)
                },
                ["Continente"] = new ScrapingConfig
                {
                    BaseUrl = "https://www.continente.pt",
                    SearchUrl = "https://www.continente.pt/pt/public/Pages/searchresults.aspx?k={0}",
                    ProductSelector = ".product-item",
                    NameSelector = ".product-name",
                    PriceSelector = ".price",
                    ImageSelector = ".product-image img",
                    LinkSelector = ".product-link",
                    DelayBetweenRequests = TimeSpan.FromSeconds(2),
                    MaxRetries = 3,
                    Timeout = TimeSpan.FromSeconds(30)
                },
                ["Lidl"] = new ScrapingConfig
                {
                    BaseUrl = "https://www.lidl.pt",
                    SearchUrl = "https://www.lidl.pt/pt/search?query={0}",
                    ProductSelector = ".product-tile",
                    NameSelector = ".product-title",
                    PriceSelector = ".price",
                    ImageSelector = ".product-image img",
                    LinkSelector = ".product-link",
                    DelayBetweenRequests = TimeSpan.FromSeconds(2),
                    MaxRetries = 3,
                    Timeout = TimeSpan.FromSeconds(30)
                }
            };
        }

        public ScrapingConfig GetConfiguration(string supermarket)
        {
            return _configurations.TryGetValue(supermarket, out var config) 
                ? config 
                : new ScrapingConfig();
        }

        public void UpdateConfiguration(string supermarket, ScrapingConfig config)
        {
            _configurations[supermarket] = config;
        }
    }

    public class ScrapingConfig
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string SearchUrl { get; set; } = string.Empty;
        public string ProductSelector { get; set; } = string.Empty;
        public string NameSelector { get; set; } = string.Empty;
        public string PriceSelector { get; set; } = string.Empty;
        public string ImageSelector { get; set; } = string.Empty;
        public string LinkSelector { get; set; } = string.Empty;
        public TimeSpan DelayBetweenRequests { get; set; } = TimeSpan.FromSeconds(1);
        public int MaxRetries { get; set; } = 3;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    }
}
