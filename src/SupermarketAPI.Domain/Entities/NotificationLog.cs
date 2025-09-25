using System;

namespace SupermarketAPI.Domain.Entities
{
    public class NotificationLog : BaseEntity
    {
        public int UserId { get; set; }
        public string Type { get; set; } = string.Empty; // DailyFavorites, PriceDrop, WeeklySummary
        public string Content { get; set; } = string.Empty; // Conteúdo da mensagem enviada
        public DateTime SentAt { get; set; }
        public string Status { get; set; } = string.Empty; // Sent, Failed, Delivered, Read
        public string? WhatsAppMessageId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Channel { get; set; } = string.Empty; // WhatsApp, Email, SMS
        public int ProductCount { get; set; } // Número de produtos na notificação
        public decimal? TotalSavings { get; set; } // Economia total mencionada
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
