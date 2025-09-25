using System;

namespace SupermarketAPI.Application.DTOs
{
    public class SupermarketDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public DateTime? LastScrapedAt { get; set; }
        public bool IsScrapingEnabled { get; set; }
        public int ScrapingIntervalMinutes { get; set; }
        public int ProductCount { get; set; }
        public int AvailableProductsCount { get; set; }
    }
}
