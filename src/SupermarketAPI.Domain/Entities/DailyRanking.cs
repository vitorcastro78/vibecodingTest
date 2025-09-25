using System;

namespace SupermarketAPI.Domain.Entities
{
    public class DailyRanking : BaseEntity
    {
        public DateTime Date { get; set; }
        public int CategoryId { get; set; }
        public string RankingData { get; set; } = string.Empty; // JSON com dados do ranking
        public int TotalProducts { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int SupermarketsCount { get; set; }
        
        // Navigation properties
        public virtual Category Category { get; set; } = null!;
    }
}
