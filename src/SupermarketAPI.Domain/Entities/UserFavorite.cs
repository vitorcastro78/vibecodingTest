using System;

namespace SupermarketAPI.Domain.Entities
{
    public class UserFavorite : BaseEntity
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public DateTime AddedAt { get; set; }
        public decimal? PriceThreshold { get; set; } // Alerta quando preço baixar abaixo deste valor
        public bool IsPriceAlertEnabled { get; set; } = true;
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
