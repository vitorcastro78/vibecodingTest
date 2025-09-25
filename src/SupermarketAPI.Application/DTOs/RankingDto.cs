using System;
using System.Collections.Generic;

namespace SupermarketAPI.Application.DTOs
{
    public class RankingDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int SupermarketsCount { get; set; }
        public List<RankingProductDto> TopProducts { get; set; } = new List<RankingProductDto>();
    }

    public class RankingProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public decimal BestPrice { get; set; }
        public string BestPriceSupermarket { get; set; } = string.Empty;
        public decimal AveragePrice { get; set; }
        public decimal Savings { get; set; }
        public decimal SavingsPercentage { get; set; }
        public int SupermarketsCount { get; set; }
    }
}
