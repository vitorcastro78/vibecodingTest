using System;
using System.Collections.Generic;

namespace SupermarketAPI.Application.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal? AveragePrice { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTime? LastPriceUpdate { get; set; }
        public List<ProductPriceDto> Prices { get; set; } = new List<ProductPriceDto>();
    }

    public class ProductPriceDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int SupermarketId { get; set; }
        public string SupermarketName { get; set; } = string.Empty;
        public string SupermarketLogoUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string OriginalUnit { get; set; } = string.Empty;
        public decimal? PricePerUnit { get; set; }
        public string Url { get; set; } = string.Empty;
        public DateTime ScrapedAt { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsOnSale { get; set; }
        public decimal? OriginalPrice { get; set; }
        public DateTime? SaleEndDate { get; set; }
        public string? SaleDescription { get; set; }
        public decimal? DiscountPercentage { get; set; }
    }
}
