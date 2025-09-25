using System;
using System.Collections.Generic;

namespace SupermarketAPI.Domain.Entities
{
    public class Supermarket : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public DateTime? LastScrapedAt { get; set; }
        public bool IsScrapingEnabled { get; set; } = true;
        public int ScrapingIntervalMinutes { get; set; } = 1440; // 24 horas por padrão
        public string? ScrapingConfiguration { get; set; } // JSON com configurações específicas
        
        // Navigation properties
        public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();
        public virtual ICollection<ScrapingLog> ScrapingLogs { get; set; } = new List<ScrapingLog>();
    }
}
