using System;

namespace SupermarketAPI.Domain.Entities
{
    public class ScrapingLog : BaseEntity
    {
        public int SupermarketId { get; set; }
        public string Status { get; set; } = string.Empty; // Success, Failed, Partial
        public int ProductsScraped { get; set; }
        public int ProductsUpdated { get; set; }
        public int ProductsNew { get; set; }
        public int Errors { get; set; }
        public string? ErrorDetails { get; set; } // JSON com detalhes dos erros
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TimeSpan? Duration { get; set; }
        public string? UserAgent { get; set; }
        public string? ProxyUsed { get; set; }
        public int HttpRequests { get; set; }
        public int HttpErrors { get; set; }
        
        // Navigation properties
        public virtual Supermarket Supermarket { get; set; } = null!;
    }
}
