using System;

namespace SupermarketAPI.Domain.Entities
{
    public class ProductPrice : BaseEntity
    {
        public int ProductId { get; set; }
        public int SupermarketId { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; } = string.Empty; // kg, g, l, ml, unidade
        public string OriginalUnit { get; set; } = string.Empty; // Unidade original do site
        public decimal? PricePerUnit { get; set; } // Preço por unidade padronizada
        public string Url { get; set; } = string.Empty;
        public DateTime ScrapedAt { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsOnSale { get; set; } = false;
        public decimal? OriginalPrice { get; set; } // Preço original antes da promoção
        public DateTime? SaleEndDate { get; set; }
        public string? SaleDescription { get; set; }
        
        // Navigation properties
        public virtual Product Product { get; set; } = null!;
        public virtual Supermarket Supermarket { get; set; } = null!;
    }
}
