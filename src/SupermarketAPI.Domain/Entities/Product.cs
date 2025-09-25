using System;
using System.Collections.Generic;

namespace SupermarketAPI.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? Keywords { get; set; } // Palavras-chave para matching
        public decimal? AveragePrice { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTime? LastPriceUpdate { get; set; }
        
        // Navigation properties
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();
        public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();
    }
}
